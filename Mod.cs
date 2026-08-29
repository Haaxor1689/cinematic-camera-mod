using Allumeria;
using Allumeria.Input;
using Allumeria.Items.Crafting;
using CinematicCamera.EntitySystem.Components;
using CinematicCamera.Items;
using CinematicCamera.Patches;
using CinematicCamera.Utils;
using HarmonyLib;
using HarmonyLib.Tools;
using Ignitron.Aluminium.Assets;
using Ignitron.Aluminium.Assets.Description;
using Ignitron.Aluminium.Assets.Providers;
using Ignitron.Aluminium.Events;
using Ignitron.Aluminium.Registries;
using Ignitron.Aluminium.Translation;
using Ignitron.Loader;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CinematicCamera;

public sealed class Mod : IModEntrypoint
{
    public const string ModId = "cinematic_camera_mod";

    public void Main(ModBox box)
    {
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        // Apply harmony patches
        new Harmony($"{box.Metadata.Contributors.First().Name}.{ModId}").PatchAll();

        var assetManager = AssetManager.CreateDefault(box.RootPath, $"ignitron/{ModId}");

        Allumeria.DataManagement.AssetLoading.AssetManager.itemAtlas.ScanDirectory(
            assetManager,
            "textures/atlas/items",
            16
        );

        Allumeria.DataManagement.AssetLoading.AssetManager.itemAtlas.ScanDirectory(
            assetManager,
            "textures/atlas/ui",
            16
        );

        AluminiumRegistries.Translators.Register(
            ModId,
            new DefaultTranslator(
                assetManager.Load("translations/keys.txt", TranslationAssetProvider.Default)
            )
        );

        ContentRegistryEvents.Items += () =>
        {
            Allumeria.DataManagement.AssetLoading.AssetManager.models.TryAdd(
                ModelKey("item.camera"),
                assetManager.Load("models/item/camera.json", ModelAssetProvider.Default)
            );

            Allumeria.DataManagement.AssetLoading.AssetManager.textures.TryAdd(
                TextureKey("item.camera"),
                assetManager.Load(
                    "textures/item/camera.png",
                    new TextureAssetDescription { },
                    TextureAssetProvider.Default
                )
            );

            Allumeria.DataManagement.AssetLoading.AssetManager.textures.TryAdd(
                TextureKey("camera_track"),
                assetManager.Load(
                    "textures/camera_track.png",
                    new TextureAssetDescription { },
                    TextureAssetProvider.Default
                )
            );

            ItemRegistry.Initialize();
            ItemCamera.InitializeOptions();

            Catalogue.merchant.AddEntry(new ShopEntry(ItemRegistry.camera, 1));
        };

        PlayerEvents.Spawned += (player, world) =>
        {
            if (player.GetComponent<WaypointComponent>() != null)
                return;
            player.AddComponent(new WaypointComponent(player));
        };

        ClientLoopEvents.Updated += (game, deltaTime) =>
        {
            if (InputManager.keyboardState.IsKeyDown(Keys.KeyPad6))
                Game.gameState.worldManager.world?.timeManager.AddToTime(180);
        };

        ClientLoopEvents.Loaded += (game) =>
        {
            UIRendererPatch.InitializeTextures();

            var shader = assetManager.Load(
                "shaders/path_prism",
                new ShaderAssetDescription { },
                ShaderAssetProvider.Default
            );

            LineRenderer.Initialize(shader, TextureKey("camera_track"));
        };

        ClientLoopEvents.Unloaded += (game) => LineRenderer.Dispose();
    }

    internal static string ItemSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.items.{name}";

    internal static string BlockSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.blocks.{name}";

    internal static string UiSpriteKey(string name) => $"ignitron.{ModId}.textures.atlas.ui.{name}";

    internal static string ModelKey(string name) => $"ignitron.{ModId}.models.{name}";

    internal static string TextureKey(string name) => $"ignitron.{ModId}.textures.{name}";
}
