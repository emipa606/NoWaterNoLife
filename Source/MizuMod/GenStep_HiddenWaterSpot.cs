﻿using Verse;

namespace MizuMod;

public class GenStep_HiddenWaterSpot : GenStep
{
    public readonly int allSpotNum = 100;
    public readonly int blockSizeX = 30;
    public readonly int blockSizeZ = 30;

    public override int SeedPart => 38449900;

    public override void Generate(Map map, GenStepParams parms)
    {
        map.GetComponent<MapComponent_HiddenWaterSpot>()
            .CreateWaterSpot(blockSizeX, blockSizeZ, allSpotNum * map.Area / (250 * 250));
    }
}