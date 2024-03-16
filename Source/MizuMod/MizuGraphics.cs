using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MizuMod;

[StaticConstructorOnStartup]
public static class MizuGraphics
{
    // バケツ
    public static readonly List<Graphic> Buckets;

    public static readonly List<Graphic_Linked> LinkedWaterBoxes;

    public static readonly Graphic WaterNet = GraphicDatabase.Get<Graphic_Single>(
        "Things/Building/Production/Mizu_WaterNet",
        ShaderDatabase.MetaOverlay);

    public static readonly Graphic_LinkedWaterNetOverlay LinkedWaterNetOverlay =
        new Graphic_LinkedWaterNetOverlay(WaterNet);

    public static readonly Graphic WaterPipe =
        GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterPipe",
            ShaderDatabase.Transparent);

    public static readonly Graphic_LinkedWaterNet LinkedWaterPipe = new Graphic_LinkedWaterNet(WaterPipe);

    public static readonly Graphic WaterPipeClear =
        GraphicDatabase.Get<Graphic_Single>("Things/Mizu_Clear", ShaderDatabase.Transparent);

    public static readonly Graphic_LinkedWaterNet LinkedWaterPipeClear = new Graphic_LinkedWaterNet(WaterPipeClear);

    public static readonly Texture2D Texture_ButtonIngest = ContentFinder<Texture2D>.Get("UI/Buttons/Ingest");

    // 水箱
    public static readonly List<Graphic> WaterBoxes;

    // 水道管
    static MizuGraphics()
    {
        WaterBoxes =
        [
            GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox0",
                ShaderDatabase.Transparent),

            GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox1",
                ShaderDatabase.CutoutComplex),

            GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox2",
                ShaderDatabase.CutoutComplex),

            GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox3",
                ShaderDatabase.CutoutComplex),

            GraphicDatabase.Get<Graphic_Single>("Things/Building/Production/Mizu_WaterBox4",
                ShaderDatabase.CutoutComplex)
        ];

        LinkedWaterBoxes =
        [
            new Graphic_Linked(WaterBoxes[0]),
            new Graphic_Linked(WaterBoxes[1]),
            new Graphic_Linked(WaterBoxes[2]),
            new Graphic_Linked(WaterBoxes[3]),
            new Graphic_Linked(WaterBoxes[4])
        ];

        Buckets =
        [
            GraphicDatabase.Get<Graphic_Single>("Things/Item/Mizu_Bucket0", ShaderDatabase.Transparent),
            GraphicDatabase.Get<Graphic_Single>("Things/Item/Mizu_Bucket1", ShaderDatabase.CutoutComplex)
        ];
    }
}