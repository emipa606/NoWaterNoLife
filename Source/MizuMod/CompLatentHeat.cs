using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class CompLatentHeat : ThingComp
    {
        // 隠し腐敗度
        private float hiddenRotProgress;

        // 潜熱値
        private float latentHeatAmount;

        private CompProperties_LatentHeat.AddCondition AddLatentHeatCondition => Props.addLatentHeatCondition;

        private ThingDef ChangedThingDef => Props.changedThingDef;

        private float HiddenRotProgress
        {
            get => hiddenRotProgress;
            set => hiddenRotProgress = value;
        }

        private float LatentHeatAmount
        {
            get => latentHeatAmount;
            set => latentHeatAmount = Mathf.Max(0f, value);
        }

        private float LatentHeatThreshold => Props.latentHeatThreshold;

        private CompProperties_LatentHeat Props => (CompProperties_LatentHeat)props;

        private float TemperatureThreshold => Props.temperatureThreshold;

        // 分離した時、潜熱値は特に変更しない
        // public override void PostSplitOff(Thing piece)
        // {
        // base.PostSplitOff(piece);
        // }
        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (!DebugSettings.godMode)
            {
                return stringBuilder.ToString();
            }

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("LatentHeatAmount:" + latentHeatAmount.ToString("F2"));
            stringBuilder.AppendLine();
            stringBuilder.Append("HiddenRotProgress:" + hiddenRotProgress);

            // if (stringBuilder.ToString() != string.Empty)
            // {
            // stringBuilder.AppendLine();
            // }
            // stringBuilder.Append(MizuStrings.InspectWaterFlowInput + ": " + this.InputWaterFlow.ToString("F2") + " L/day");
            // stringBuilder.Append(string.Concat(new string[]
            // {
            // "(",
            // MizuStrings.GetInspectWaterTypeString(this.InputWaterType),
            // ")",
            // }));
            return stringBuilder.ToString();
        }

        public override void CompTickRare()
        {
            CompTick();

            // 閾値より温度が高い場合はプラス
            var deltaTemperature = parent.AmbientTemperature - TemperatureThreshold;

            // 溶ける・凍る判定
            var direction = 0;
            switch (AddLatentHeatCondition)
            {
                case CompProperties_LatentHeat.AddCondition.Above:
                    // 温度が高いと潜熱プラス→溶ける
                    direction = 1;
                    break;
                case CompProperties_LatentHeat.AddCondition.Below:
                    // 温度が低いと潜熱プラス→凍る
                    direction = -1;
                    break;
                default:
                    Log.Error("AddLatentHeatCondition is invalid");
                    break;
            }

            // 潜熱値変更
            // (最後はデバッグ用の係数を掛けている)
            LatentHeatAmount += deltaTemperature * direction * MizuDef.GlobalSettings.forDebug.latentHeatRate;

            if (!(latentHeatAmount >= LatentHeatThreshold))
            {
                return;
            }

            // 潜熱値が閾値を超えた時の処理
            var map = parent.Map;
            var owner = parent.holdingOwner;

            if (ChangedThingDef == null)
            {
                // 変化後アイテムの設定が無い場合は消滅
                DestroyParent(map, owner);
                return;
            }

            // 変化後のアイテムを生成
            var changedThing = ThingMaker.MakeThing(ChangedThingDef);
            changedThing.stackCount = parent.stackCount;

            // 腐敗度の処理
            SetRotProgress(changedThing, GetRotProgress());

            // 消滅と生成
            DestroyParent(map, owner);
            CreateNewThing(changedThing, map, owner);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref latentHeatAmount, "latentHeatAmount");
            Scribe_Values.Look(ref hiddenRotProgress, "hiddenRotProgress");
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            base.PreAbsorbStack(otherStack, count);

            // 全体に対するother側の割合
            var otherRatio = count / (float)(parent.stackCount + count);

            var otherComp = otherStack.TryGetComp<CompLatentHeat>();
            if (otherComp == null)
            {
                return;
            }

            // 潜熱値の計算(加重平均)
            LatentHeatAmount = Mathf.Lerp(LatentHeatAmount, otherComp.LatentHeatAmount, otherRatio);

            // 隠し腐敗度の計算(加重平均)
            HiddenRotProgress = Mathf.Lerp(HiddenRotProgress, otherComp.HiddenRotProgress, otherRatio);
        }

        private void CreateNewThing(Thing thing, Map map, ThingOwner owner)
        {
            if (map != null)
            {
                // マップに落ちている場合
                GenSpawn.Spawn(thing, parent.Position, map);
                return;
            }

            if (owner == null)
            {
                return;
            }

            // 何らかの物の中に入っている場合
            if (owner.TryAdd(thing) == false)
            {
                Log.Error("failed TryAdd");
            }
        }

        private void DestroyParent(Map map, ThingOwner owner)
        {
            if (map != null)
            {
                // マップに落ちている場合
                parent.Destroy();
                return;
            }

            if (owner == null)
            {
                return;
            }

            // 何らかの物の中に入っている場合
            owner.Remove(parent);
            parent.Destroy();
        }

        private float GetRotProgress()
        {
            var compRotThis = parent.TryGetComp<CompRottable>();

            if (compRotThis == null)
            {
                // 腐敗度がないなら隠し腐敗度を返す
                return hiddenRotProgress;
            }

            // 腐敗度があるならその値を返す
            return compRotThis.RotProgress;
        }

        private void SetRotProgress(Thing thing, float rotProgress)
        {
            var compRotChanged = thing.TryGetComp<CompRottable>();
            if (compRotChanged != null)
            {
                // 腐敗度を持つ
                // →腐敗度をそこに設定(氷→水の変化時はここに入るはず)
                compRotChanged.RotProgress = rotProgress;
            }
            else
            {
                // 腐敗度を持たない
                // →潜熱を持つかチェック
                var compLatentHeatChanged = thing.TryGetComp<CompLatentHeat>();
                if (compLatentHeatChanged != null)
                {
                    // 潜熱を持っている
                    // →腐敗度を隠し腐敗度として設定(水→氷の変化時はここに入るはず)
                    compLatentHeatChanged.HiddenRotProgress = rotProgress;
                }
            }

            // 腐敗度も潜熱も持っていないなら、設定したかった腐敗進行値は捨てる
        }
    }
}