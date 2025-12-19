using LabExtended.API;
using LabExtended.Extensions;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

namespace LabExtended.Commands.Tokens.Methods
{
    /// <summary>
    /// Methods for parsing raycast arguments in commands.
    /// </summary>
    public static class RaycastMethods
    {
        private static bool _RayPoint(CommandContext ctx, MethodToken token, out object result)
        {
            result = null!;
            
            if (token.ParsedArguments.Count < 1)
                return false;

            if (token.ParsedArguments[0] is not MethodToken methodToken)
                return false;
            
            if (!methodToken.TryExecuteMethod<RaycastHit>(ctx, out var hit))
                return false;

            result = hit.point;
            return true;
        }
        
        private static bool _RayToPlayer(CommandContext ctx, MethodToken token, out object result)
        {
            result = null!;
            
            if (token.ParsedArguments.Count < 1)
                return false;

            if (token.ParsedArguments[0] is not MethodToken methodToken)
                return false;

            if (!methodToken.TryExecuteMethod<RaycastHit>(ctx, out var hit))
                return false;

            if (hit.collider == null)
                return false;

            if (!hit.collider.gameObject.TryFindComponent<ReferenceHub>(out var hub))
                return false;

            result = ExPlayer.Get(hub)!;
            return result != null;
        }
        
        private static bool _TryCastRay(CommandContext ctx, MethodToken token, out object result)
        {
            float GetDistanceOrDefault()
            {
                var source = token.ParsedArguments.AtOrDefault(0, null);

                if (source is not StringToken stringToken || string.IsNullOrWhiteSpace(stringToken.Value))
                    return 100f;

                if (!float.TryParse(stringToken.Value, out var distance))
                    return 100f;

                return distance;
            }

            int GetMaskOrDefault()
            {
                var source = token.ParsedArguments.AtOrDefault(0, null);

                if (source is not StringToken stringToken || string.IsNullOrWhiteSpace(stringToken.Value))
                    return FpcStateProcessor.Mask.value;

                if (stringToken.Value.TrySplit(',', true, null, out var masks))
                    return LayerMask.GetMask(masks);

                if (int.TryParse(stringToken.Value, out var mask))
                    return mask;

                return FpcStateProcessor.Mask.value;
            }

            var cast = Physics.Raycast(
                ctx.Sender.Rotation.CameraPosition,
                ctx.Sender.Rotation.CameraForward, 
                
                out var hit, 
                
                GetDistanceOrDefault(), 
                GetMaskOrDefault());

            result = hit;
            return cast;
        }
        
        internal static void RegisterMethods()
        {
            MethodToken.Methods["ray"] = _TryCastRay;
            MethodToken.Methods["rayPoint"] = _RayPoint;
            MethodToken.Methods["rayToPlayer"] = _RayToPlayer;
            
            // rayToPlayer(ray(10;Player))
        }
    }
}