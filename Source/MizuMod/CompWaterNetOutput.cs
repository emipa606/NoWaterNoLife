﻿using System.Text;
using Verse;

namespace MizuMod;

public class CompWaterNetOutput : CompWaterNet
{
    public CompWaterNetOutput()
    {
        OutputWaterType = WaterType.NoWater;
    }

    public bool FoundEffectiveInputter { get; private set; }

    public override bool IsActivated => base.IsActivated && (!HasTank || !TankIsEmpty);

    public float OutputWaterFlow { get; set; }

    public WaterType OutputWaterType { get; set; }

    public new CompProperties_WaterNetOutput Props => (CompProperties_WaterNetOutput)props;

    protected virtual WaterType ForceOutputWaterType => Props.forceOutputWaterType;

    protected virtual float MaxOutputWaterFlow => Props.maxOutputWaterFlow;

    protected virtual CompProperties_WaterNetOutput.OutputWaterFlowType OutputWaterFlowType =>
        Props.outputWaterFlowType;

    private bool HasTank => TankComp != null;

    private bool TankIsEmpty => !HasTank || TankComp.StoredWaterVolume <= 0.0f;

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.CompInspectStringExtra());

        if (stringBuilder.ToString() != string.Empty)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(
            MizuStrings.InspectWaterFlowOutput.Translate() + ": " + OutputWaterFlow.ToString("F2") + " L/day");
        stringBuilder.Append(
            string.Concat(["(", MizuStrings.GetInspectWaterTypeString(OutputWaterType), ")"]));

        return stringBuilder.ToString();
    }

    public override void CompTick()
    {
        base.CompTick();

        UpdateOutputWaterStatus();
    }

    public void UpdateOutputWaterStatus()
    {
        if (!IsActivated)
        {
            // 機能していない
            OutputWaterType = WaterType.NoWater;
            OutputWaterFlow = 0f;
            return;
        }

        if (WaterNetBuilding.OutputWaterNet == null)
        {
            // 出力水道網なし
            OutputWaterType = WaterType.NoWater;
            OutputWaterFlow = 0f;
            return;
        }

        if (InputComp == null)
        {
            // 入力機能なしで出力だけあるものは存在しない
            OutputWaterType = WaterType.NoWater;
            OutputWaterFlow = 0f;
            return;
        }

        // 有効な出力先が1個でもあれば出力する
        FoundEffectiveInputter = false;
        foreach (var t in WaterNetBuilding.OutputWaterNet.AllThings)
        {
            // 自分自身は除外
            if (t == WaterNetBuilding)
            {
                continue;
            }

            // 相手の入力水道網と自分の出力水道網が一致しない場合は除外
            if (WaterNetBuilding.OutputWaterNet != t.InputWaterNet)
            {
                continue;
            }

            // 入力機能が無効、または水道網から入力しないタイプは無効
            if (t.InputComp is not { IsActivated: true }
                || !t.InputComp.InputTypes.Contains(
                    CompProperties_WaterNetInput.InputType.WaterNet))
            {
                continue;
            }

            // 貯水機能を持っているが満タンである場合は無効
            if (t.TankComp is { AmountCanAccept: <= 0f })
            {
                continue;
            }

            // 有効な出力先が見つかった
            FoundEffectiveInputter = true;
        }

        if (!FoundEffectiveInputter)
        {
            // 有効な出力先なし
            OutputWaterType = WaterType.NoWater;
            OutputWaterFlow = 0f;
            return;
        }

        // 水源を決める
        if (HasTank)
        {
            if (!TankIsEmpty)
            {
                // タンクがあり、タンクの中身がある
                // ⇒タンクが水源
                OutputWaterType = TankComp.StoredWaterType;
                OutputWaterFlow = MaxOutputWaterFlow;
                return;
            }
        }
        else
        {
            // タンクがない
            // ⇒水源は現在の入力

            // 基本は入力されている水質をそのまま出力とする
            // 出力の水質が強制されている場合はその水質にする
            var outWaterType = InputComp.InputWaterType;
            if (ForceOutputWaterType != WaterType.Undefined)
            {
                outWaterType = ForceOutputWaterType;
            }

            if (OutputWaterFlowType == CompProperties_WaterNetOutput.OutputWaterFlowType.Constant
                && InputComp.InputWaterFlow >= MaxOutputWaterFlow)
            {
                // 定量出力タイプで、入力が出力量を超えている場合、機能する
                OutputWaterType = outWaterType;
                OutputWaterFlow = MaxOutputWaterFlow;
                return;
            }

            if (OutputWaterFlowType == CompProperties_WaterNetOutput.OutputWaterFlowType.Any)
            {
                // 任意出力タイプの場合、入力と同じ量だけ出力する
                OutputWaterType = outWaterType;
                OutputWaterFlow = InputComp.InputWaterFlow;

                // 結果的に出力量が0の場合、水の種類をクリアする
                if (OutputWaterFlow == 0.0f)
                {
                    OutputWaterType = WaterType.NoWater;
                }

                return;
            }
        }

        // 有効な水源無し
        OutputWaterType = WaterType.NoWater;
        OutputWaterFlow = 0;
    }
}