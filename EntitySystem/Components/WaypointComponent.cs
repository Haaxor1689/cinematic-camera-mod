using Allumeria;
using Allumeria.DataManagement.AssetLoading;
using Allumeria.EntitySystem;
using Allumeria.EntitySystem.Entities;
using Allumeria.EntitySystem.Models;
using Allumeria.Rendering;
using Allumeria.UI;
using CinematicCamera.Items;
using CinematicCamera.Utils;
using OpenTK.Mathematics;

namespace CinematicCamera.EntitySystem.Components;

public class WaypointComponent : EntityComponent
{
    private const float MinimumWaypointDistanceSquared = 1.5f;
    private const float MinimumSpeed = 0.1f;
    private const float MaximumSpeed = 1.9f;

    private static Vector4 normalLight = new(10, 10, 10, 10);
    private static Vector4 highlightLight = new(2, 15, 10, 0);

    public EntityModel cameraModel;

    private readonly List<CameraWaypoint> waypoints = [];
    private float speed = 0.5f;

    private int nearestWaypointIndex = -1;

    public WaypointComponent(Entity parent)
        : base(parent)
    {
        var model = AssetManager.models.GetValueOrDefault(Mod.ModelKey("item.camera"));
        var texture = AssetManager.textures.GetValueOrDefault(Mod.TextureKey("item.camera"));

        if (model == null || texture == null)
            throw new InvalidOperationException(
                "Failed to load camera model or texture for WaypointComponent."
            );

        this.cameraModel = new EntityModel(model, texture);
    }

    public override void Render()
    {
        if (
            parent is not PlayerEntity player
            || player.heldItem != ItemRegistry.camera
            || waypoints.Count == 0
            || Game.clientState.cameraWaypointFollower.running
        )
            return;

        UpdateNearestWaypointIndex();

        for (var index = 0; index < waypoints.Count; index++)
        {
            var waypoint = waypoints[index];
            // Hide waypoints that are too close to the player to avoid clipping into the model (adjusted for player height)
            if (
                (waypoint.position - parent.position - Vector3.UnitY * 0.8f).LengthSquared
                < MinimumWaypointDistanceSquared
            )
                continue;

            var (Position, Rotation) = BuildRenderTransform(waypoint);
            cameraModel.Render(
                Position,
                Rotation,
                index == nearestWaypointIndex ? highlightLight : normalLight
            );
        }

        LineRenderer.SetColor(GetSpeedColor(speed));
        LineRenderer.Render();
    }

    internal void AddWaypoint(CameraWaypoint waypoint)
    {
        waypoints.Add(waypoint);
        SampleWaypoints();
    }

    internal void RemoveNearestWaypoint()
    {
        UpdateNearestWaypointIndex();
        if (nearestWaypointIndex == -1)
        {
            ChatLog.NewMessageSystemToPlayer("No waypoint to remove.", (PlayerEntity)parent);
            return;
        }

        waypoints.RemoveAt(nearestWaypointIndex);
        SampleWaypoints();
    }

    internal void MoveNearestWaypoint(CameraWaypoint cameraWaypoint)
    {
        UpdateNearestWaypointIndex();
        if (nearestWaypointIndex == -1)
        {
            ChatLog.NewMessageSystemToPlayer("No waypoint to move.", (PlayerEntity)parent);
            return;
        }

        waypoints[nearestWaypointIndex] = cameraWaypoint;
        SampleWaypoints();
    }

    internal void ClearWaypoints()
    {
        waypoints.Clear();
        SampleWaypoints();
        ChatLog.NewMessageSystemToPlayer("All camera waypoints cleared.", (PlayerEntity)parent);
    }

    internal void AdjustSpeed(float delta)
    {
        var adjustedSpeed = MathHelper.Clamp(speed + delta, MinimumSpeed, MaximumSpeed);
        if (speed != adjustedSpeed)
        {
            speed = adjustedSpeed;
            SampleWaypoints();
        }

        ChatLog.NewMessageSystemToPlayer(
            delta >= 0f ? $"Speed increased to {speed:F1}." : $"Speed decreased to {speed:F1}.",
            (PlayerEntity)parent
        );
    }

    internal void StartFollowing()
    {
        if (waypoints.Count <= 1)
        {
            ChatLog.NewMessageSystemToPlayer(
                "At least 2 waypoints are required to play the camera path.",
                (PlayerEntity)parent
            );
            return;
        }

        Game.clientState.cameraWaypointFollower.SetWaypoints(waypoints);
        Game.clientState.cameraWaypointFollower.Start(speed);
    }

    private void SampleWaypoints() =>
        LineRenderer.SetPath(WaypointPathSampler.SamplePath(waypoints, speed));

    private void UpdateNearestWaypointIndex()
    {
        nearestWaypointIndex = -1;
        var nearestDistanceSquared = float.MaxValue;
        var waypointCenter = parent.position + Vector3.UnitY * 0.8f;

        for (var index = 0; index < waypoints.Count; index++)
        {
            var distanceSquared = (waypoints[index].position - waypointCenter).LengthSquared;
            if (distanceSquared >= nearestDistanceSquared)
                continue;

            nearestDistanceSquared = distanceSquared;
            nearestWaypointIndex = index;
        }
    }

    private static (Vector3 Position, Matrix4 Rotation) BuildRenderTransform(
        CameraWaypoint waypoint
    )
    {
        var front = waypoint.front;
        if (front.LengthSquared < 1e-6f)
        {
            var yaw = NormalizeAngle(waypoint.yaw);
            var pitch = NormalizeAngle(waypoint.pitch);
            front = new Vector3(
                MathF.Cos(pitch) * MathF.Cos(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Sin(yaw)
            );
        }

        front = Vector3.Normalize(front);

        var right = Vector3.Cross(Vector3.UnitY, front);
        if (right.LengthSquared < 1e-6f)
            right = Vector3.UnitX;
        right = Vector3.Normalize(right);

        var up = Vector3.Normalize(Vector3.Cross(front, right));

        var rotation = new Matrix4(
            -right.X,
            -right.Y,
            -right.Z,
            0f,
            up.X,
            up.Y,
            up.Z,
            0f,
            -front.X,
            -front.Y,
            -front.Z,
            0f,
            0f,
            0f,
            0f,
            1f
        );

        var centerOffset = Vector3.TransformPosition(new Vector3(-0.25f, 0f, 0f), rotation);

        return (waypoint.position - centerOffset, rotation);
    }

    private static float NormalizeAngle(float angle)
    {
        // Waypoints can store either radians or degrees depending on capture source.
        return MathF.Abs(angle) > MathF.PI * 2f + 0.001f
            ? MathHelper.DegreesToRadians(angle)
            : angle;
    }

    private static Vector4 GetSpeedColor(float speed) =>
        Vector4.Lerp(
            new Vector4(0.3f, 1f, 0.3f, 1f),
            new Vector4(1f, 0.3f, 0.3f, 1f),
            (speed - MinimumSpeed) / (MaximumSpeed - MinimumSpeed)
        );
}
