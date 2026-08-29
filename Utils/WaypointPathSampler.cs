using Allumeria.Rendering;
using OpenTK.Mathematics;

namespace CinematicCamera.Utils;

internal static class WaypointPathSampler
{
    internal const float DeltaTime = 1f / 60f;
    private const float SmoothingFactor = 0.01f;
    private const int MaxSteps = 100_000;

    internal static List<Vector3> SamplePath(IReadOnlyList<CameraWaypoint>? waypoints, float speed)
    {
        var samples = new List<Vector3>();
        if (waypoints == null || waypoints.Count == 0)
            return samples;

        samples.Add(waypoints[0].position);
        if (waypoints.Count == 1)
            return samples;

        var position = waypoints[0].position;
        var progress = 0f;
        var waypointIndex = 0;
        var steps = 0;

        while (steps++ < MaxSteps)
        {
            if (progress >= 1f)
            {
                progress = 1f - progress;
                waypointIndex++;
            }

            if (waypoints.Count > waypointIndex + 1)
            {
                var rawPosition = Vector3.Lerp(
                    waypoints[waypointIndex].position,
                    waypoints[waypointIndex + 1].position,
                    progress
                );
                position = Vector3.Lerp(position, rawPosition, SmoothingFactor);
            }
            else
            {
                position = waypoints[waypointIndex].position;
                samples.Add(position);
                break;
            }

            samples.Add(position);
            progress += speed * DeltaTime;
        }

        return samples;
    }
}
