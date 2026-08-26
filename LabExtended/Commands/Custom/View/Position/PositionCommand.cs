using LabExtended.Commands.Attributes;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using UnityEngine;

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    /// <summary>
    /// Displays detailed information about the sender's current position, rotation, camera state, and room context.
    /// </summary>
    /// <remarks>This command provides a comprehensive overview of the sender's spatial data, including
    /// transform and camera positions, rotation values, grounded state, and room information. It also includes relative
    /// position details if the sender's role supports first-person controls, and camera controller data if available.
    /// Use this command to diagnose or inspect the sender's current in-game location and orientation.</remarks>
    [CommandOverload("position", "Shows information about your current position.", "view.position")]
    public void Position()
    {
        Ok(x =>
        {
            x.AppendLine();
            x.AppendLine($"Transform Position: {Misc.ToPreciseString((Vector3)Sender.Transform.position)}");
            x.AppendLine($"Transform Rotation: {Sender.Transform.rotation}");

            x.AppendLine($"Camera Transform Position: {Misc.ToPreciseString((Vector3)Sender.CameraTransform.position)}");
            x.AppendLine($"Camera Transform Rotation: {Sender.CameraTransform.rotation}");

            x.AppendLine($"Is Grounded: {Sender.Position.IsGrounded}");

            var groundPos = Sender.Position.GroundPosition;

            if (groundPos.HasValue)
            {
                x.AppendLine($"Ground Position: {Misc.ToPreciseString((Vector3)groundPos.Value)}");
            }
            else
            {
                x.AppendLine($"Ground Position: (null)");
            }

            if (Sender.Position.Room != null)
            {
                x.AppendLine($"Cached Room: {Sender.Position.Room.Name} ({Sender.Position.Room.Shape}; {Sender.Position.Room.Zone}; {Sender.Position.Room.MainCoords})");
                x.AppendLine($"Cached Room Transform Local Position: {Sender.Position.Room.transform.InverseTransformPoint(Sender.Transform.position)}");
                x.AppendLine($"Cached Room Camera Transform Local Position: {Sender.Position.Room.transform.InverseTransformPoint(Sender.CameraTransform.position)}");
            }
            else
            {
                if (RoomUtils.TryGetRoom(Sender.Transform.position, out var curRoom))
                {
                    x.AppendLine($"Found Room: {curRoom.Name} ({curRoom.Shape}; {curRoom.Zone}; {curRoom.MainCoords})");
                    x.AppendLine($"Found Room Transform Local Position: {curRoom.transform.InverseTransformPoint(Sender.Transform.position)}");
                    x.AppendLine($"Found Room Camera Transform Local Position: {curRoom.transform.InverseTransformPoint(Sender.CameraTransform.position)}");
                }
                else
                {
                    x.AppendLine("Room: (null)");
                }
            }

            if (Sender.Role.Role is IFpcRole fpcRole)
            {
                var currentRelative = new RelativePosition(Sender.Transform.position);
                var receivedRelative = fpcRole.FpcModule.Motor.ReceivedPosition;

                x.AppendLine($"Current Relative: {currentRelative.PositionX}; {currentRelative.PositionY}; {currentRelative.PositionZ}; {currentRelative.Position.ToPreciseString()}; {currentRelative.WaypointId}");
                x.AppendLine($"Received Relative: {receivedRelative.PositionX}; {receivedRelative.PositionY}; {receivedRelative.PositionZ}; {receivedRelative.Position.ToPreciseString()}; {receivedRelative.WaypointId}");
            }

            if (Sender.Role.Role is ICameraController cameraController)
            {
                x.AppendLine($"Camera Position: {cameraController.CameraPosition.ToPreciseString()}");
                x.AppendLine($"Vertical Rotation: {cameraController.VerticalRotation}");
                x.AppendLine($"Horizontal Rotation: {cameraController.HorizontalRotation}");
            }
        });
    }
}