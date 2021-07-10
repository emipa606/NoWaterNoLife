using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MizuMod
{
    [HarmonyPatch(typeof(ITab_Pawn_Gear))]
    [HarmonyPatch("DrawThingRow")]
    internal class ITab_Pawn_Gear_DrawThingRow
    {
        private static void Postfix(ref float y, float width, Thing thing)
        {
            // 所持品に含まれる水アイテムに、所持品を直接摂取するボタンを増やす
            // yは次の行のtop上端座標、widthは右端座標

            // db=DrinkButton
            const float dbWidth = 24f;
            const float dbHeight = 24f;

            // 1アイコンにつき幅24f
            // 情報アイコン、ドロップアイコン、食べるアイコンを考慮すれば3個
            // 食べると飲むを分離できたはずなので2で良い
            var dbRight = width - (24f * 2);
            var dbTop = y - 28f;

            var selPawn = Find.Selector.SingleSelectedThing as Pawn;
            if (selPawn == null && Find.Selector.SingleSelectedThing is Corpse selCorpse)
            {
                selPawn = selCorpse.InnerPawn;
            }

            // ポーンデータがないなら終了
            if (selPawn == null)
            {
                return;
            }

            // プレイヤーが操作するポーンではない、またはそのポーンは倒れている→終了
            if (!selPawn.IsColonistPlayerControlled || selPawn.Downed)
            {
                return;
            }

            // 水として飲めないアイテムなら終了
            if (!thing.CanGetWater() || !thing.CanDrinkWaterNow())
            {
                return;
            }

            // 水アイテムでなかったり、食べられるものは能動的に飲むことはできない
            var comp = thing.TryGetComp<CompWaterSource>();
            if (comp == null || comp.SourceType != CompProperties_WaterSource.SourceType.Item ||
                thing.IsIngestibleFor(selPawn))
            {
                return;
            }

            // ツールチップとボタンを追加
            var dbRect = new Rect(dbRight - dbWidth, dbTop, dbWidth, dbHeight);
            TooltipHandler.TipRegion(dbRect,
                string.Format(MizuStrings.FloatMenuGetWater.Translate(), thing.LabelNoCount));
            if (!Widgets.ButtonImage(dbRect, MizuGraphics.Texture_ButtonIngest))
            {
                return;
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            var job = new Job(MizuDef.Job_DrinkWater, thing) {count = MizuUtility.WillGetStackCountOf(selPawn, thing)};
            selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
        }
    }
}