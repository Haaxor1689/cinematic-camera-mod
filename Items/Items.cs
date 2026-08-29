using Allumeria.Items;
using Ignitron.Aluminium.Extensions;

namespace CinematicCamera.Items;

public static class ItemRegistry
{
    internal static Item camera = null!;

    internal static void Initialize()
    {
        camera = ItemHelper
            .Create(() => new ItemCamera($"{Mod.ModId}.camera"))
            .SetItemSprite(Mod.ItemSpriteKey("camera"))
            .SetModel(Mod.ModelKey("item.camera"), Mod.TextureKey("item.camera"))
            .SellValue(1000)
            .SetRarity(3);
    }
}
