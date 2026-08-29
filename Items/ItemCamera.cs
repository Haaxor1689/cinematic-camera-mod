using Allumeria;
using Allumeria.EntitySystem.Entities;
using Allumeria.Items;
using Allumeria.Rendering;
using Allumeria.UI.UINodes;
using CinematicCamera.EntitySystem.Components;
using CinematicCamera.Patches;

namespace CinematicCamera.Items;

internal struct CameraOptionItem
{
    internal string Name;
    internal int TextureId;
    internal Action<PlayerEntity> OnSelect;
}

public class ItemCamera : Item
{
    internal static CameraOptionItem[] Options = null!;

    internal static void InitializeOptions()
    {
        Options =
        [
            new CameraOptionItem
            {
                Name = "Back",
                TextureId = UIRendererPatch.RegisterTexture("back"),
                OnSelect = (player) => { },
            },
            new CameraOptionItem
            {
                Name = "Add",
                TextureId = UIRendererPatch.RegisterTexture("add"),
                OnSelect = (player) =>
                    player
                        .GetComponent<WaypointComponent>()
                        ?.AddWaypoint(
                            new CameraWaypoint(
                                Game.camera.position,
                                Game.camera.pitch,
                                Game.camera.yaw,
                                Game.camera.front
                            )
                        ),
            },
            new CameraOptionItem
            {
                Name = "Remove",
                TextureId = UIRendererPatch.RegisterTexture("remove"),
                OnSelect = (player) =>
                    player.GetComponent<WaypointComponent>()?.RemoveNearestWaypoint(),
            },
            new CameraOptionItem
            {
                Name = "Move",
                TextureId = UIRendererPatch.RegisterTexture("move"),
                OnSelect = (player) =>
                    player
                        .GetComponent<WaypointComponent>()
                        ?.MoveNearestWaypoint(
                            new CameraWaypoint(
                                Game.camera.position,
                                Game.camera.pitch,
                                Game.camera.yaw,
                                Game.camera.front
                            )
                        ),
            },
            new CameraOptionItem
            {
                Name = "Speed Up",
                TextureId = UIRendererPatch.RegisterTexture("forward"),
                OnSelect = (player) => player.GetComponent<WaypointComponent>()?.AdjustSpeed(0.2f),
            },
            new CameraOptionItem
            {
                Name = "Speed Down",
                TextureId = UIRendererPatch.RegisterTexture("backward"),
                OnSelect = (player) => player.GetComponent<WaypointComponent>()?.AdjustSpeed(-0.2f),
            },
            new CameraOptionItem
            {
                Name = "Play",
                TextureId = UIRendererPatch.RegisterTexture("play"),
                OnSelect = (player) => player.GetComponent<WaypointComponent>()?.StartFollowing(),
            },
            new CameraOptionItem
            {
                Name = "Clear all",
                TextureId = UIRendererPatch.RegisterTexture("clear_all"),
                OnSelect = (player) => player.GetComponent<WaypointComponent>()?.ClearWaypoints(),
            },
        ];
    }

    public ItemCamera(string strID)
        : base(strID)
    {
        this.usesRadialMenu = true;
    }

    public override void OnRadialClose(PlayerEntity player, int selection) =>
        Options[selection].OnSelect(player);

    public override void OnRadialOpen(PlayerEntity player, UIRadialMenu menu)
    {
        foreach (var option in Options)
        {
            menu.AddItem(
                new RadialItem(UIRendererPatch.TextureMarker, option.TextureId, option.Name)
            );
        }
    }
}
