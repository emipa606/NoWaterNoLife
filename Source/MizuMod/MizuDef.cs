using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MizuMod;

[StaticConstructorOnStartup]
public static class MizuDef
{
    public static readonly Dictionary<WaterType, WaterTypeDef> Dic_WaterTypeDef;

    public static readonly GlobalSettingDef GlobalSettings =
        DefDatabase<GlobalSettingDef>.GetNamed("Mizu_GlobalSettings");

    public static readonly HediffDef Hediff_Dehydration = DefDatabase<HediffDef>.GetNamed("Mizu_Dehydration");

    public static readonly HediffDef Hediff_DrankSeaWater = DefDatabase<HediffDef>.GetNamed("Mizu_DrankSeaWater");

    public static readonly HediffDef Hediff_Nursed = DefDatabase<HediffDef>.GetNamed("Mizu_Nursed");

    public static readonly JobDef Job_DeliverWater = DefDatabase<JobDef>.GetNamed("Mizu_DeliverWater");

    public static readonly JobDef Job_DrawFromTerrain = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromTerrain");

    public static readonly JobDef Job_DrawFromWaterNet = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromWaterNet");

    public static readonly JobDef Job_DrawFromWaterPool = DefDatabase<JobDef>.GetNamed("Mizu_DrawFromWaterPool");

    public static readonly JobDef Job_DrawWaterByPrisoner = DefDatabase<JobDef>.GetNamed("Mizu_DrawWaterByPrisoner");

    public static readonly JobDef Job_DrinkWater = DefDatabase<JobDef>.GetNamed("Mizu_DrinkWater");

    public static readonly JobDef Job_DrinkWaterFromBuilding =
        DefDatabase<JobDef>.GetNamed("Mizu_DrinkWaterFromBuilding");

    public static readonly JobDef Job_FeedWaterPatient = DefDatabase<JobDef>.GetNamed("Mizu_FeedWaterPatient");

    public static readonly JobDef Job_GetSnow = DefDatabase<JobDef>.GetNamed("Mizu_GetSnow");

    public static readonly JobDef Job_Mop = DefDatabase<JobDef>.GetNamed("Mizu_Mop");

    public static readonly JobDef Job_Nurse = DefDatabase<JobDef>.GetNamed("Mizu_Nurse");

    public static readonly JobDef Job_PourWater = DefDatabase<JobDef>.GetNamed("Mizu_PourWater");

    public static readonly JobDef Job_SupplyWaterToTool = DefDatabase<JobDef>.GetNamed("Mizu_SupplyWaterToTool");

    public static readonly JobDef Job_WaterFarm = DefDatabase<JobDef>.GetNamed("Mizu_WaterFarm");

    public static readonly List<ThingDef> List_WaterItem;

    public static readonly ThingDef Mote_SprinklerWater = ThingDef.Named("Mizu_MoteSprinklerWater");

    public static readonly NeedDef Need_Water = DefDatabase<NeedDef>.GetNamed("Mizu_Water");

    public static readonly RecordDef Record_WaterDrank = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrank");

    public static readonly RecordDef Record_WaterDrew = DefDatabase<RecordDef>.GetNamed("Mizu_WaterDrew");

    public static readonly StatCategoryDef StatCategory_Water =
        DefDatabase<StatCategoryDef>.GetNamed("Mizu_WaterCategory");

    public static ThingDef Thing_ClearWater = DefDatabase<ThingDef>.GetNamed("Mizu_ClearWater");

    public static ThingDef Thing_Mop = DefDatabase<ThingDef>.GetNamed("Mizu_Mop");

    public static readonly ThingDef Thing_MoppedThing = DefDatabase<ThingDef>.GetNamed("Mizu_MoppedThing");

    public static readonly ThingDef Thing_MudWater = DefDatabase<ThingDef>.GetNamed("Mizu_MudWater");

    public static readonly ThingDef Thing_NormalWater = DefDatabase<ThingDef>.GetNamed("Mizu_NormalWater");

    public static readonly ThingDef Thing_RawWater = DefDatabase<ThingDef>.GetNamed("Mizu_RawWater");

    public static readonly ThingDef Thing_SeaWater = DefDatabase<ThingDef>.GetNamed("Mizu_SeaWater");

    public static readonly ThingDef Thing_Snowball = DefDatabase<ThingDef>.GetNamed("Mizu_Snowball");

    public static readonly ThingDef Thing_WaterPipe = DefDatabase<ThingDef>.GetNamed("Mizu_WaterPipe");

    public static readonly ThingDef Thing_WaterPipeInWater = DefDatabase<ThingDef>.GetNamed("Mizu_WaterPipeInWater");

    public static ThingCategoryDef ThingCategory_Waters = DefDatabase<ThingCategoryDef>.GetNamed("Mizu_Waters");

    public static readonly ThoughtDef Thought_AteIcyFoodInHotSeason =
        DefDatabase<ThoughtDef>.GetNamed("Mizu_AteIcyFoodInHotSeason");

    public static ThoughtDef Thought_DrankClearWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankClearWater");

    public static readonly ThoughtDef Thought_DrankMudWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankMudWater");

    public static readonly ThoughtDef Thought_DrankScoopedWater =
        DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankScoopedWater");

    public static readonly ThoughtDef Thought_DrankSeaWater = DefDatabase<ThoughtDef>.GetNamed("Mizu_DrankSeaWater");

    public static readonly ThoughtDef Thought_SippedWaterLikeBeast =
        DefDatabase<ThoughtDef>.GetNamed("Mizu_SippedWaterLikeBeast");

    public static readonly WaterTypeDef WaterType_Clear = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeClear");

    public static readonly WaterTypeDef WaterType_Mud = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeMud");

    public static readonly WaterTypeDef WaterType_Normal = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeNormal");

    public static readonly WaterTypeDef WaterType_Raw = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeRaw");

    public static readonly WaterTypeDef WaterType_Sea = DefDatabase<WaterTypeDef>.GetNamed("Mizu_WaterTypeSea");

    static MizuDef()
    {
        List_WaterItem =
        [
            Thing_ClearWater,
            Thing_NormalWater,
            Thing_RawWater,
            Thing_MudWater,
            Thing_SeaWater
        ];

        Dic_WaterTypeDef = new Dictionary<WaterType, WaterTypeDef>
        {
            { WaterType.ClearWater, WaterType_Clear },
            { WaterType.NormalWater, WaterType_Normal },
            { WaterType.RawWater, WaterType_Raw },
            { WaterType.MudWater, WaterType_Mud },
            { WaterType.SeaWater, WaterType_Sea }
        };
    }
}