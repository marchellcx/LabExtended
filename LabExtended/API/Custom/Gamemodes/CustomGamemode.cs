using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabExtended.Core;

using LabExtended.Events;
using LabExtended.Events.Round;

using LabExtended.Extensions;
using LabExtended.Utilities.Update;

using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using YamlDotNet.Serialization;

namespace LabExtended.API.Custom.Gamemodes
{
    /// <summary>
    /// Base class for custom gamemodes.
    /// </summary>
    public abstract class CustomGamemode : CustomObject<CustomGamemode>
    {
        private static PlayerUpdateComponent updateComponent = PlayerUpdateComponent.Create();

        /// <summary>
        /// Disables all currently active registered game modes.
        /// </summary>
        /// <returns>true if at least one game mode was disabled; otherwise, false.</returns>
        public static bool DisableAll()
        {
            bool anyDisabled = false;

            foreach (var pair in RegisteredObjects)
            {
                var gamemode = pair.Value;

                if (gamemode.IsActive)
                {
                    gamemode.Disable();

                    anyDisabled = true;
                }
            }

            return anyDisabled;
        }

        /// <summary>
        /// Retrieves a list of all currently active custom gamemodes.
        /// </summary>
        /// <returns>A list of <see cref="CustomGamemode"/> objects that are currently active. The list is empty if no custom
        /// gamemodes are active.</returns>
        public static List<CustomGamemode> GetActive()
        {
            var list = new List<CustomGamemode>();

            foreach (var pair in RegisteredObjects)
            {
                if (pair.Value.IsActive)
                {
                    list.Add(pair.Value);
                }
            }

            return list;
        }

        private Stopwatch runTimeWatch = new();

        /// <summary>
        /// Whether or not the gamemode is currently active.
        /// </summary>
        [YamlIgnore]
        public bool IsActive => runTimeWatch.IsRunning;

        /// <summary>
        /// Gets or sets a value indicating whether the game mode can be activated after a round has already started.
        /// </summary>
        [Description("Whether or not the gamemode can be started in the middle of the round.")]
        public virtual bool CanActivateMidRound { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the gamemode is automatically disabled when the round ends.
        /// </summary>
        [Description("Whether or not the gamemode should be automatically disabled when the round ends.")]
        public virtual bool ShouldDisableOnRoundEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the gamemode prevents the default wave spawns from occurring.
        /// </summary>
        [Description("Whether or not the gamemode prevents the default wave spawns from occurring.")]
        public virtual bool PreventWaveSpawns { get; set; }

        /// <summary>
        /// Gets or sets the list of gamemode identifiers that are incompatible with this gamemode.
        /// </summary>
        [Description("A list of gamemode IDs that are incompatible with this gamemode.")]
        public virtual string[] IncompatibleGamemodes { get; set; } = [];

        /// <summary>
        /// Gets the amount of time that has elapsed since the gamemode has started.
        /// </summary>
        [YamlIgnore]
        public TimeSpan RunTime => runTimeWatch.Elapsed;

        /// <summary>
        /// Gets the date and time at which the current run started.
        /// </summary>
        [YamlIgnore]
        public DateTime StartTime => DateTime.Now - runTimeWatch.Elapsed;

        /// <summary>
        /// Enables the current instance if it is not already active and can be enabled.
        /// </summary>
        /// <returns>true if the instance was successfully enabled; otherwise, false.</returns>
        public bool Enable()
        {
            if (IsActive)
                return false;

            if (!CanBeEnabled(GetActive()))
                return false;

            runTimeWatch.Restart();

            ApiLog.Info("Custom Gamemodes", $"Enabled custom gamemode &3{Id}&r!");

            OnEnabled();

            updateComponent.OnLateUpdate += OnUpdate;

            ExRoundEvents.AssignedRoles += OnAssignedRoles;
            ExRoundEvents.AssigningRoles += OnAssigningRoles;

            ExRoundEvents.LateJoinSetRole += OnLateJoinSetRole;
            ExRoundEvents.LateJoinSettingRole += OnLateJoinSettingRole;

            ExRoundEvents.Started += OnRoundStarted;
            ExRoundEvents.Restarting += OnRoundRestarting;
            ExRoundEvents.WaitingForPlayers += OnWaitingForPlayers;

            ExPlayerEvents.Left += OnPlayerLeft;
            ExPlayerEvents.Verified += OnPlayerJoined;

            ServerEvents.WaveRespawned += OnWaveSpawned;
            ServerEvents.WaveRespawning += OnWaveSpawning;

            ServerEvents.WaveTeamSelected += OnWaveSelected;
            ServerEvents.WaveTeamSelecting += OnWaveSelecting;

            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.RoundEnding += OnRoundEnding;

            ServerEvents.RoundEndingConditionsCheck += OnRoundCheckingEndConditions;
            return true;
        }

        /// <summary>
        /// Disables the current instance if it is active.
        /// </summary>
        /// <returns>true if the instance was active and is now disabled; otherwise, false.</returns>
        public bool Disable()
        {
            if (!IsActive)
                return false;

            runTimeWatch.Stop();
            runTimeWatch.Reset();

            ExRoundEvents.AssignedRoles -= OnAssignedRoles;
            ExRoundEvents.AssigningRoles -= OnAssigningRoles;

            ExRoundEvents.LateJoinSetRole -= OnLateJoinSetRole;
            ExRoundEvents.LateJoinSettingRole -= OnLateJoinSettingRole;

            ExRoundEvents.Started -= OnRoundStarted;
            ExRoundEvents.Restarting -= OnRoundRestarting;
            ExRoundEvents.WaitingForPlayers -= OnWaitingForPlayers;

            ExPlayerEvents.Left -= OnPlayerLeft;
            ExPlayerEvents.Verified -= OnPlayerJoined;

            ServerEvents.WaveRespawned -= OnWaveSpawned;
            ServerEvents.WaveRespawning -= OnWaveSpawning;

            ServerEvents.WaveTeamSelected -= OnWaveSelected;
            ServerEvents.WaveTeamSelecting -= OnWaveSelecting;

            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.RoundEnding -= OnRoundEnding;

            ServerEvents.RoundEndingConditionsCheck -= OnRoundCheckingEndConditions;

            ApiLog.Info("Custom Gamemodes", $"Disabled custom gamemode &3{Id}&r!");

            updateComponent.OnLateUpdate -= OnUpdate;

            OnDisabled();
            return true;
        }

        /// <summary>
        /// Determines whether or not the gamemode can be enabled given the other currently active gamemodes.
        /// </summary>
        /// <param name="otherModes">List of other active modes.</param>
        /// <returns><c>true</c> if the gamemode can be enabled; otherwise, <c>false</c>.</returns>
        public virtual bool CanBeEnabled(List<CustomGamemode> otherModes)
        {
            if (IsActive)
                return false;

            if (!CanActivateMidRound && !ExRound.IsWaitingForPlayers)
                return false;

            if (IncompatibleGamemodes?.Length > 0 && otherModes.Any(x => IncompatibleGamemodes.Contains(x.Id)))
                return false;

            return true;
        }

        /// <summary>
        /// Gets called when the gamemode is enabled.
        /// </summary>
        public virtual void OnEnabled()
        {

        }

        /// <summary>
        /// Gets called when the gamemode is disabled.
        /// </summary>
        public virtual void OnDisabled()
        {

        }

        /// <summary>
        /// Appends a textual representation of the current gamemode's state to the specified <see cref="StringBuilder"/>
        /// instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> instance to which the gamemode's state will be appended. Cannot be null.</param>
        public virtual void PrintState(StringBuilder builder)
        {

        }

        /// <inheritdoc/>
        public override void OnRegistered()
        {
            base.OnRegistered();

            ApiLog.Info("Custom Gamemodes", $"&2Registered&r custom gamemode &3{Id}&r");
        }

        /// <inheritdoc/>
        public override void OnUnregistered()
        {
            base.OnUnregistered();

            if (IsActive)
                Disable();

            ApiLog.Info("Custom Gamemodes", $"&1Unregistered&r custom gamemode &3{Id}&r");
        }

        /// <summary>
        /// Gets called once per frame.
        /// </summary>
        public virtual void OnUpdate()
        {

        }

        /// <summary>
        /// Gets called before a round end is triggered.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundCheckingEndConditions(RoundEndingConditionsCheckEventArgs args)
        {

        }


        /// <summary>
        /// Gets called when the round is about to end.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundEnding(RoundEndingEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when the round ends.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnRoundEnded(RoundEndedEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when the round starts restarting.
        /// </summary>
        public virtual void OnRoundRestarting()
        {
            if (ShouldDisableOnRoundEnd && IsActive)
            {
                Disable();
            }
        }

        /// <summary>
        /// Gets called when the round restart finishes and the server is ready for player connections.
        /// </summary>
        public virtual void OnWaitingForPlayers()
        {

        }

        /// <summary>
        /// Gets called when the round starts.
        /// </summary>
        public virtual void OnRoundStarted()
        {

        }

        /// <summary>
        /// Gets called before player's round-start roles are assigned. 
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnAssigningRoles(AssigningRolesEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after player's round-start roles are assigned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnAssignedRoles(AssignedRolesEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when a player is setting their role while late joining.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnLateJoinSettingRole(LateJoinSettingRoleEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after a player's role is set by late-join.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnLateJoinSetRole(LateJoinSetRoleEventArgs args)
        {

        }

        /// <summary>
        /// Gets called when a new player joins the server.
        /// </summary>
        /// <param name="player">The player who joined.</param>
        public virtual void OnPlayerJoined(ExPlayer player)
        {

        }

        /// <summary>
        /// Gets called when a player leaves the server.
        /// </summary>
        /// <param name="player">The player who left.</param>
        public virtual void OnPlayerLeft(ExPlayer player)
        {

        }

        /// <summary>
        /// Gets called before a new reinforcement wave is selected.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSelecting(WaveTeamSelectingEventArgs args)
        {

        }

        /// <summary>
        /// Gets called after a new reinforcement wave is selected.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSelected(WaveTeamSelectedEventArgs args)
        {

        }

        /// <summary>
        /// Gets called before a new reinforcement wave is spawned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSpawning(WaveRespawningEventArgs args)
        {
            if (PreventWaveSpawns)
            {
                args.IsAllowed = false;
            }
        }

        /// <summary>
        /// Gets called after a new reinforcement wave has been spawned.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        public virtual void OnWaveSpawned(WaveRespawnedEventArgs args)
        {

        }
    }
}