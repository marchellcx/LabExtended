using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

using Mirror;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

using PlayerRoles.Visibility;

using UnityEngine;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Provides patches and extension points for synchronizing player positions and roles during network updates.
    /// </summary>
    /// <remarks>This static class enables customization of player role synchronization by exposing the
    /// RoleSyncEvent event, allowing external handlers to influence how role data is sent during position updates. It is
    /// intended for use with the FpcServerPositionDistributor's update cycle and is typically used in multiplayer
    /// scenarios where custom visibility or role logic is required.</remarks>
    public static class PlayerPositionSyncPatch
    {
        /// <summary>
        /// Occurs when a player's role is being synchronized, allowing subscribers to modify or override the role
        /// assignment before it is finalized.
        /// </summary>
        /// <remarks>Subscribers can use this event to inspect or change the role that will be assigned to
        /// a player during synchronization. The event provides the source and target players, the original role, a
        /// network writer for additional data, and expects a RoleTypeId to be returned, which determines the final role
        /// assigned. If no handlers are attached, the original role is used.</remarks>
        public static event Func<ExPlayer, ExPlayer, RoleTypeId, NetworkWriter, RoleTypeId>? RoleSyncEvent;

        /// <summary>
        /// Invokes the RoleSyncEvent delegate to synchronize role information between players.
        /// </summary>
        /// <remarks>If RoleSyncEvent is not assigned, the method returns null. This method is typically
        /// used to propagate role changes or updates between players over the network.</remarks>
        /// <param name="source">The player initiating the role synchronization event.</param>
        /// <param name="target">The target player whose role information is being synchronized.</param>
        /// <param name="role">The role to be synchronized for the target player.</param>
        /// <param name="writer">The network writer used to serialize event data for transmission.</param>
        /// <returns>The RoleTypeId value returned by the RoleSyncEvent delegate if invoked; otherwise, null.</returns>
        public static RoleTypeId? InvokeEvent(ExPlayer source, ExPlayer target, RoleTypeId role, NetworkWriter writer)
        {
            return RoleSyncEvent?.Invoke(source, target, role, writer);
        }

        [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.LateUpdate))]
        private static bool Prefix()
        {
            if (!StaticUnityMethods.IsPlaying)
                return false;

            FpcServerPositionDistributor._sendCooldown += Time.deltaTime;

            if (FpcServerPositionDistributor._sendCooldown < FpcServerPositionDistributor.SendRate) 
                return false;

            FpcServerPositionDistributor._sendCooldown -= FpcServerPositionDistributor.SendRate;

            for (var x = 0; x < ExPlayer.Count; x++)
            {
                var player = ExPlayer.Players[x];

                if (player?.ReferenceHub == null 
                    || player.InstanceMode == CentralAuth.ClientInstanceMode.Unverified 
                    || player.ReferenceHub.isLocalPlayer) 
                    continue;

                if (!player.Toggles.ShouldReceivePositions)
                    continue;

                Sync(player);
            }

            return false;
        }

        private static void Sync(ExPlayer receiver)
        {
            using (var writer = NetworkWriterPool.Get())
            {
                writer.WriteMessageId<FpcPositionMessage>();

                ushort count = 0;

                var customVisibilityRole = receiver.ReferenceHub.roleManager.CurrentRole as ICustomVisibilityRole;

                for (var x = 0; x < ExPlayer.AllCount; x++)
                {
                    var player = ExPlayer.AllPlayers[x];

                    if (player?.ReferenceHub == null) 
                        continue;

                    if (!player.Toggles.ShouldSendPosition)
                        continue;

                    if (player.ReferenceHub.isLocalPlayer || (player.ReferenceHub.netId == receiver.ReferenceHub.netId && !player.Toggles.ShouldReceiveOwnPosition)) 
                        continue;

                    var fpcRole = player.ReferenceHub.roleManager.CurrentRole as IFpcRole;

                    if (fpcRole == null) 
                        continue;

                    var isInvisible = customVisibilityRole?.VisibilityController != null && !customVisibilityRole.VisibilityController.ValidateVisibility(player.ReferenceHub);

                    if (!isInvisible)
                    {
                        if ((ExPlayer.GhostedFlags & player.GhostBit) != 0)
                        {
                            isInvisible = true;
                        }
                        else if ((player.PersonalGhostFlags & receiver.GhostBit) != 0)
                        {
                            isInvisible = true;
                        }
                    }
                    
                    var validatedVisibilityEventArgs = new PlayerValidatedVisibilityEventArgs(receiver.ReferenceHub, player.ReferenceHub, !isInvisible);

                    PlayerEvents.OnValidatedVisibility(validatedVisibilityEventArgs);

                    isInvisible = !validatedVisibilityEventArgs.IsVisible;

                    var newSyncData = GetNewSyncData(receiver, player, fpcRole.FpcModule, isInvisible);

                    if (!isInvisible)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[count] = player.ReferenceHub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[count] = newSyncData;

                        count++;
                    }

                    var role = FpcServerPositionDistributor.GetVisibleRole(receiver.ReferenceHub, player.ReferenceHub);
                    var roleWriter = NetworkWriterPool.Get();

                    var baseRoleSyncResult = FpcServerPositionDistributor._roleSyncEvent?.Invoke(player.ReferenceHub, receiver.ReferenceHub, role, roleWriter);
                    var roleSyncResult = RoleSyncEvent?.Invoke(player, receiver, role, roleWriter);

                    if (baseRoleSyncResult != null) role = baseRoleSyncResult.Value;
                    if (roleSyncResult != null) role = roleSyncResult.Value;

                    if (player.Role.FakedList.HasGlobalValue) role = player.Role.FakedList.GlobalValue;
                    if (player.Role.FakedList.TryGetValue(receiver, out var recvValue)) role = recvValue;

                    if (player.Role.CustomRole != null) 
                        role = player.Role.CustomRole.GetAppearance(player, receiver, role, ref player.Role.customRoleData);

                    if (!player.ReferenceHub.roleManager.PreviouslySentRole.TryGetValue(receiver.ReferenceHub.netId, out var previousRole) || previousRole != role)
                        FpcServerPositionDistributor.SendRole(receiver.ReferenceHub, player.ReferenceHub, role, roleWriter);

                    NetworkWriterPool.Return(roleWriter);
                }

                writer.WriteUShort(count);

                for (var i = 0; i < count; i++)
                {
                    writer.WriteRecyclablePlayerId(new RecyclablePlayerId(FpcServerPositionDistributor._bufferPlayerIDs[i]));

                    FpcServerPositionDistributor._bufferSyncData[i].Write(writer);
                }

                receiver.Send(writer.ToArraySegment());
            }
        }

        private static FpcSyncData GetNewSyncData(ExPlayer receiver, ExPlayer target, FirstPersonMovementModule fpmm, bool isInvisible)
        {
            var position = fpmm.RelativePosition;

            if (target.Position.FakedList.HasGlobalValue)
                position = new(target.Position.FakedList.GlobalValue);
            else if (target.Position.FakedList.TryGetValue(receiver, out var recvValue))
                position = new(recvValue);

            var prevSyncData = GetPrevSyncData(receiver, target);

            var fpcSyncData = isInvisible 
                ? default 
                : new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, position, fpmm.MouseLook);

            FpcServerPositionDistributor.PreviouslySent[receiver.ReferenceHub.netId][target.ReferenceHub.netId] = fpcSyncData;
            return fpcSyncData;
        }

        private static FpcSyncData GetPrevSyncData(ExPlayer receiver, ExPlayer target)
        {
            if (!FpcServerPositionDistributor.PreviouslySent.TryGetValue(receiver.ReferenceHub.netId, out var dictionary))
            {
                FpcServerPositionDistributor.PreviouslySent.Add(receiver.ReferenceHub.netId, new Dictionary<uint, FpcSyncData>());
                return default;
            }

            if (!dictionary.TryGetValue(target.ReferenceHub.netId, out var fpcSyncData))
                return default;

            return fpcSyncData;
        }
    }
}