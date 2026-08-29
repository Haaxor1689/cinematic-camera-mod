using Allumeria;
using Allumeria.Rendering;
using HarmonyLib;
using OpenTK.Mathematics;

namespace CinematicCamera.Patches;

// Hides the HUD when the camera waypoint follower is running, and restores it and the camera orientation when the follower stops.
[HarmonyPatch]
internal static class WaypointFollowerPatch
{
    private static bool wasRunningLastFrame;
    private static bool savedHUDState;
    internal static Vector3 savedPosition;
    internal static float savedPitch;
    internal static float savedYaw;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointFollower), nameof(WaypointFollower.Start))]
    private static void StartPostfix(WaypointFollower __instance)
    {
        savedPosition = Game.camera.position;
        savedPitch = Game.camera.pitch;
        savedYaw = Game.camera.yaw;

        savedHUDState = Game.hideHUD;
        Game.hideHUD = true;

        wasRunningLastFrame = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WaypointFollower), nameof(WaypointFollower.Update))]
    private static void UpdatePostfix(WaypointFollower __instance)
    {
        if (__instance.running || !wasRunningLastFrame)
            return;

        Game.camera.position = savedPosition;
        Game.camera.pitch = savedPitch;
        Game.camera.yaw = savedYaw;

        Game.hideHUD = savedHUDState;

        wasRunningLastFrame = false;
    }
}
