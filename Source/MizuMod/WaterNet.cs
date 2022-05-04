using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MizuMod;

public class WaterNet
{
    private static int nextID = 1;

    private readonly HashSet<IBuilding_WaterNet> allTanks = new HashSet<IBuilding_WaterNet>();

    private readonly HashSet<IBuilding_WaterNet> drainers = new HashSet<IBuilding_WaterNet>();

    public readonly int ID;

    private readonly Dictionary<CompProperties_WaterNetInput.InputType, HashSet<IBuilding_WaterNet>> inputterTypeDic
        = new Dictionary<CompProperties_WaterNetInput.InputType, HashSet<IBuilding_WaterNet>>();

    private readonly HashSet<IBuilding_WaterNet> outputters = new HashSet<IBuilding_WaterNet>();

    private readonly Dictionary<CompProperties_WaterNetTank.DrawType, HashSet<IBuilding_WaterNet>> tankTypeDic =
        new Dictionary<CompProperties_WaterNetTank.DrawType, HashSet<IBuilding_WaterNet>>();

    public WaterNet()
    {
        ID = nextID;
        nextID++;

        WaterType = WaterType.NoWater;
        StoredWaterType = WaterType.NoWater;
        StoredWaterType = WaterType.NoWater;

        inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet] = new HashSet<IBuilding_WaterNet>();
        inputterTypeDic[CompProperties_WaterNetInput.InputType.Rain] = new HashSet<IBuilding_WaterNet>();
        inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterPool] = new HashSet<IBuilding_WaterNet>();
        inputterTypeDic[CompProperties_WaterNetInput.InputType.Terrain] = new HashSet<IBuilding_WaterNet>();

        tankTypeDic[CompProperties_WaterNetTank.DrawType.Self] = new HashSet<IBuilding_WaterNet>();
        tankTypeDic[CompProperties_WaterNetTank.DrawType.Faucet] = new HashSet<IBuilding_WaterNet>();
    }

    public WaterNet(IBuilding_WaterNet thing)
        : this()
    {
        AddThing(thing);
    }

    public List<IBuilding_WaterNet> AllThings { get; } = new List<IBuilding_WaterNet>();

    public IEnumerable<HashSet<IBuilding_WaterNet>> FlatTankList { get; private set; }

    public MapComponent_WaterNetManager Manager { get; set; }

    public WaterType StoredWaterType { get; private set; }

    public WaterType StoredWaterTypeForFaucet
    {
        get
        {
            var totalWaterType = WaterType.NoWater;
            foreach (var tank in tankTypeDic[CompProperties_WaterNetTank.DrawType.Faucet])
            {
                totalWaterType = totalWaterType.GetMinType(tank.TankComp.StoredWaterType);
            }

            return totalWaterType;
        }
    }

    // 仮
    public float StoredWaterVolume
    {
        get
        {
            var sumStoredWaterVolume = 0.0f;
            foreach (var tank in allTanks)
            {
                sumStoredWaterVolume += tank.GetComp<CompWaterNetTank>().StoredWaterVolume;
            }

            return sumStoredWaterVolume;
        }
    }

    public float StoredWaterVolumeForFaucet
    {
        get
        {
            var sumStoredWaterVolume = 0.0f;
            foreach (var tank in tankTypeDic[CompProperties_WaterNetTank.DrawType.Faucet])
            {
                sumStoredWaterVolume += tank.TankComp.StoredWaterVolume;
            }

            return sumStoredWaterVolume;
        }
    }

    public WaterType WaterType { get; private set; }

    public static void ClearNextID()
    {
        nextID = 1;
    }

    public void AddInputThing(IBuilding_WaterNet thing)
    {
        thing.InputWaterNet = this;

        AddThingToList(thing);
    }

    public void AddOutputThing(IBuilding_WaterNet thing)
    {
        thing.OutputWaterNet = this;

        AddThingToList(thing);
    }

    public void AddThing(IBuilding_WaterNet thing)
    {
        thing.InputWaterNet = this;
        thing.OutputWaterNet = this;

        AddThingToList(thing);
    }

    public void AddWaterVolume(float amount)
    {
        var totalAmount = amount;

        while (totalAmount > 0.0f)
        {
            var tanks = AllThings.FindAll(
                t => t.TankComp != null && t.TankComp.AmountCanAccept > 0.0f && t.InputComp != null
                     && t.InputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet));

            if (tanks.Count == 0)
            {
                break;
            }

            var averageWaterFlow = totalAmount / tanks.Count;
            foreach (var tank in tanks)
            {
                totalAmount -= tank.GetComp<CompWaterNetTank>().AddWaterVolume(averageWaterFlow);
            }
        }
    }

    public void ClearThings()
    {
        foreach (var thing in AllThings)
        {
            thing.InputWaterNet = null;
            thing.OutputWaterNet = null;
        }

        AllThings.Clear();
        foreach (var item in inputterTypeDic)
        {
            item.Value.Clear();
        }

        outputters.Clear();
        allTanks.Clear();
        foreach (var item in tankTypeDic)
        {
            item.Value.Clear();
        }

        FlatTankList = null;
    }

    // 仮
    public void DrawWaterVolume(float amount)
    {
        var totalAmount = amount;

        while (totalAmount > 0.1f)
        {
            var tanks = allTanks.Where(t => t.TankComp.StoredWaterVolume > 0.0f);

            var buildingWaterNets = tanks as IBuilding_WaterNet[] ?? tanks.ToArray();
            if (buildingWaterNets.Length == 0)
            {
                break;
            }

            var averageAmount = totalAmount / buildingWaterNets.Length;
            foreach (var tank in buildingWaterNets)
            {
                totalAmount -= tank.TankComp.DrawWaterVolume(averageAmount);
            }
        }
    }

    // 仮
    public void DrawWaterVolumeForFaucet(float amount)
    {
        var totalAmount = amount;
        var count = 0;
        while (totalAmount > 0.0f && (totalAmount > 0.1f || count < 5))
        {
            count++;
            var tanks = tankTypeDic[CompProperties_WaterNetTank.DrawType.Faucet]
                .Where(t => t.TankComp.StoredWaterVolume > 0.0f);

            var buildingWaterNets = tanks as IBuilding_WaterNet[] ?? tanks.ToArray();
            if (buildingWaterNets.Length == 0)
            {
                break;
            }

            var averageAmount = totalAmount / buildingWaterNets.Length;
            foreach (var tank in buildingWaterNets)
            {
                totalAmount -= tank.TankComp.DrawWaterVolume(averageAmount);
            }
        }
    }

    public void RemoveThing(IBuilding_WaterNet thing)
    {
        RemoveThingFromList(thing);
    }

    public void UpdateInputWaterFlow()
    {
        // 入力値をクリア
        foreach (var item in inputterTypeDic)
        {
            foreach (var t in item.Value)
            {
                t.InputComp.InputWaterFlow = 0.0f;
                t.InputComp.InputWaterType = WaterType.NoWater;
            }
        }

        // 雨入力の入力量を決定
        foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.Rain])
        {
            // 建造物にどれだけ屋根がかぶっているかチェック
            if (!t.InputComp.IsActivated)
            {
                continue;
            }

            // 屋根ボーナスの計算(屋根の枚数/設備の面積*屋根効率)
            var building = t as Building;
            if (building == null)
            {
                continue;
            }

            var roofBonus = (float)t.GetRoofNumNearby(t.InputComp.RoofDistance) / building.def.size.Area
                            * t.InputComp.RoofEfficiency;

            var addRainWaterVolume = t.InputComp.BaseRainFlow * Manager.map.weatherManager.RainRate
                                                              * t.GetUnroofedPercent() * (1 + roofBonus);
            t.InputComp.InputWaterFlow = Mathf.Min(
                t.InputComp.InputWaterFlow + addRainWaterVolume,
                t.InputComp.MaxInputWaterFlow);
            if (addRainWaterVolume > 0.0f)
            {
                t.InputComp.InputWaterType = t.InputComp.InputWaterType.GetMinType(WaterType.RawWater);
            }
        }

        // 地下水入力タイプの入力量を決定
        foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterPool])
        {
            if (!t.InputComp.IsActivated)
            {
                continue;
            }

            // 自身に貯水機能がない＆出力機能があるが有効な出力先がない場合は入力しない
            if (t.TankComp == null && t.OutputComp != null && !t.OutputComp.FoundEffectiveInputter)
            {
                continue;
            }

            // 自身に貯水機能がある＆貯水量が満タンの場合は入力しない
            if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0f)
            {
                continue;
            }

            if (t.WaterPool == null || !(t.WaterPool.CurrentWaterVolume > 0f))
            {
                continue;
            }

            // 地下水があれば入力あり
            t.InputComp.InputWaterFlow = t.InputComp.MaxInputWaterFlow;
            t.InputComp.InputWaterType = t.InputComp.InputWaterType.GetMinType(t.WaterPool.WaterType);

            // 地下水の出力量(地下水を減らす)を設定
            t.WaterPool.OutputWaterFlow += t.InputComp.InputWaterFlow;
        }

        // 地形入力タイプの入力量を決定
        foreach (var t in inputterTypeDic[CompProperties_WaterNetInput.InputType.Terrain])
        {
            if (!t.InputComp.IsActivated)
            {
                continue;
            }

            var building = t as Building;
            if (building == null)
            {
                continue;
            }

            var terrainWaterType = Manager.map.terrainGrid.TerrainAt(building.Position).ToWaterType();
            if (!t.InputComp.AcceptWaterTypes.Contains(terrainWaterType))
            {
                continue;
            }

            // 入力可能水質と地形が合っていれば入力あり
            t.InputComp.InputWaterFlow = t.InputComp.MaxInputWaterFlow;
            t.InputComp.InputWaterType = t.InputComp.InputWaterType.GetMinType(terrainWaterType);
        }

        // 水道網入力タイプの入力量を決定
        // 出力を各入力設備に割り振っていく
        var outputWaterType = WaterType.NoWater;
        foreach (var outputter in outputters)
        {
            // 出力量ゼロの場合スキップ
            if (outputter.OutputComp.OutputWaterFlow <= 0f)
            {
                continue;
            }

            // 水道網内の水質を更新
            // 全ての出力水質のうち最低ランクの物にする
            // ここでは水道網全体の水質を決めるだけ
            outputWaterType =
                (WaterType)Mathf.Min((int)outputWaterType, (int)outputter.OutputComp.OutputWaterType);

            // 出力設備が現在出力可能な相手のリスト
            var effectiveInputters = inputterTypeDic[CompProperties_WaterNetInput.InputType.WaterNet].Where(
                t =>
                {
                    // 自分自身は除外
                    if (t == outputter)
                    {
                        return false;
                    }

                    // 機能していないものは除外
                    if (!t.InputComp.IsActivated)
                    {
                        return false;
                    }

                    // タンクが満タンであれば除外
                    if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0.0f)
                    {
                        return false;
                    }

                    // 出力水質が入力可能水質リストになければ除外
                    if (!t.InputComp.AcceptWaterTypes.Contains(outputter.OutputComp.OutputWaterType))
                    {
                        return false;
                    }

                    return true;
                });

            // 残り出力量
            var remainOutputWaterFlow = outputter.OutputComp.OutputWaterFlow;

            // 出力可能な相手のうち、定量入力を必要とするタイプを先に割り振る
            var constantInputters = effectiveInputters.Where(
                t => t.InputComp.InputWaterFlowType == CompProperties_WaterNetInput.InputWaterFlowType.Constant);
            foreach (var inputter in constantInputters)
            {
                var actualInputWaterFlow = inputter.InputComp.MaxInputWaterFlow - inputter.InputComp.InputWaterFlow;
                if (!(remainOutputWaterFlow >= actualInputWaterFlow))
                {
                    continue;
                }

                // 残量がまだ足りているなら入力する
                inputter.InputComp.InputWaterFlow += actualInputWaterFlow;
                inputter.InputComp.InputWaterType =
                    inputter.InputComp.InputWaterType.GetMinType(outputter.OutputComp.OutputWaterType);
                remainOutputWaterFlow -= actualInputWaterFlow;
            }

            // 処理高速化のため、残り出力を可能な限り無駄にせず割り振るループは削除

            // 出力可能な相手のうち、任意入力で良い物
            var anyInputters = effectiveInputters.Where(
                t =>
                {
                    // 入力が任意で良いもの以外は除外
                    if (t.InputComp.InputWaterFlowType != CompProperties_WaterNetInput.InputWaterFlowType.Any)
                    {
                        return false;
                    }

                    // タンクがあり満タンになったものは除外
                    if (t.TankComp != null && t.TankComp.AmountCanAccept <= 0.0f)
                    {
                        return false;
                    }

                    // 入力量が最大まで達している物は除外
                    if (t.InputComp.InputWaterFlow >= t.InputComp.MaxInputWaterFlow)
                    {
                        return false;
                    }

                    return true;
                });
            var buildingWaterNets = anyInputters as IBuilding_WaterNet[] ?? anyInputters.ToArray();
            if (buildingWaterNets.Length <= 0)
            {
                continue;
            }

            {
                // 均等割り
                var aveOutputWaterFlow = remainOutputWaterFlow / buildingWaterNets.Length;
                foreach (var inputter in buildingWaterNets)
                {
                    // 最大を超えない量を計算して入力量を増加
                    var actualInputWaterFlow = Mathf.Min(
                        aveOutputWaterFlow,
                        inputter.InputComp.MaxInputWaterFlow - inputter.InputComp.InputWaterFlow);
                    inputter.InputComp.InputWaterFlow += actualInputWaterFlow;
                    inputter.InputComp.InputWaterType =
                        inputter.InputComp.InputWaterType.GetMinType(outputter.OutputComp.OutputWaterType);
                    remainOutputWaterFlow -= actualInputWaterFlow;
                }
            }
        }

        // 水道網を流れる水の水質更新(水道網のタンクの水質とは別)
        WaterType = outputWaterType;
    }

    public void UpdateWaterTank()
    {
        // 水抜き量の決定
        var sumDrainWaterFlow = 0f;
        foreach (var drainer in drainers)
        {
            if (drainer.IsDraining)
            {
                sumDrainWaterFlow += drainer.SourceComp.DrainWaterFlow;
            }
        }

        var averageDrainWaterFlow = 0f;
        if (allTanks.Count > 0)
        {
            averageDrainWaterFlow = sumDrainWaterFlow / allTanks.Count;
        }

        // タンクの貯水量変更
        foreach (var tank in allTanks)
        {
            // 入力と出力の差分だけ更新する
            var inputWaterFlow = 0.0f;
            if (tank.InputComp != null)
            {
                inputWaterFlow = tank.InputComp.InputWaterFlow;
            }

            var outputWaterFlow = 0.0f;
            if (tank.OutputComp != null)
            {
                outputWaterFlow = tank.OutputComp.OutputWaterFlow;
            }

            // 水抜き量も考慮
            var deltaWaterFlow = inputWaterFlow - outputWaterFlow - averageDrainWaterFlow;
            if (deltaWaterFlow > 0.0f)
            {
                tank.TankComp.AddWaterVolume(deltaWaterFlow / 60000.0f);
            }
            else if (deltaWaterFlow < 0.0f)
            {
                tank.TankComp.DrawWaterVolume(-deltaWaterFlow / 60000.0f);
            }
        }

        // タンク内の水質更新
        foreach (var tank in allTanks)
        {
            if (tank.TankComp.StoredWaterVolume <= 0f)
            {
                // 空っぽになったらクリア
                tank.TankComp.StoredWaterType = WaterType.NoWater;
            }
            else
            {
                // 空っぽでなければ現在のタンク水質と入力水質の低い方を選ぶ
                tank.TankComp.StoredWaterType = (WaterType)Mathf.Min(
                    (int)tank.TankComp.StoredWaterType,
                    (int)tank.InputComp.InputWaterType);
            }
        }

        // 水道網全体のタンク水質(蛇口をひねったときに出てくる水の種類)を決める
        StoredWaterType = WaterType.NoWater;
        foreach (var tank in allTanks)
        {
            StoredWaterType = (WaterType)Mathf.Min((int)StoredWaterType, (int)tank.TankComp.StoredWaterType);
        }

        // 貯水量平坦化
        if (FlatTankList == null)
        {
            return;
        }

        foreach (var list in FlatTankList)
        {
            // 平坦化対象になっている全タンクの貯水量と総容量、水質を求める
            var allMax = 0.0f;
            var allCur = 0.0f;
            var allWaterType = WaterType.NoWater;
            foreach (var t in list)
            {
                allMax += t.TankComp.MaxWaterVolume;
                allCur += t.TankComp.StoredWaterVolume;
                allWaterType = (WaterType)Mathf.Min((int)allWaterType, (int)t.TankComp.StoredWaterType);
            }

            // 割合を求める
            var flatPercent = allCur / allMax;

            // 平坦化する
            foreach (var t in list)
            {
                t.TankComp.StoredWaterVolume = flatPercent * t.TankComp.MaxWaterVolume;
                t.TankComp.StoredWaterType = allWaterType;
            }
        }
    }

    private void AddThingToList(IBuilding_WaterNet thing)
    {
        // 全ての物を追加
        AllThings.Add(thing);

        // 水抜き機能を持つ設備を追加
        if (thing.HasDrainCapability)
        {
            drainers.Add(thing);
        }

        // 入力系を仕分けして追加
        if (thing.InputComp != null)
        {
            foreach (var inputType in thing.InputComp.InputTypes)
            {
                if (inputType == CompProperties_WaterNetInput.InputType.WaterNet)
                {
                    // 水道網入力タイプは、この水道網からの入力を受ける場合のみ追加
                    if (thing.InputWaterNet == this)
                    {
                        inputterTypeDic[inputType].Add(thing);
                    }
                }
                else
                {
                    // 水道網入力タイプ以外は無条件で追加
                    inputterTypeDic[inputType].Add(thing);
                }
            }
        }

        // 出力系を追加
        if (thing.OutputComp != null && thing.OutputWaterNet == this)
        {
            outputters.Add(thing);
        }

        // タンク系を追加
        if (thing.TankComp != null)
        {
            allTanks.Add(thing);
            foreach (var drawType in thing.TankComp.DrawTypes)
            {
                tankTypeDic[drawType].Add(thing);
            }
        }

        // 平坦化リストを再作成
        RefreshFlatTankList();
    }

    private void RefreshFlatTankList()
    {
        var flatTankListTmp = new HashSet<HashSet<IBuilding_WaterNet>>();

        // IDが負でない⇒有効な平坦化IDを持っている
        var flatTanks = allTanks.Where(t => t.TankComp.FlatID >= 0);
        var count = 0;
        foreach (var unused in flatTanks)
        {
            count++;
        }

        if (count == 0)
        {
            return;
        }

        foreach (var t in flatTanks)
        {
            var foundLists = new List<HashSet<IBuilding_WaterNet>>();
            foreach (var list in flatTankListTmp)
            {
                foreach (var listItem in list)
                {
                    // IDが違うものは平坦化対象ではない
                    if (t.TankComp.FlatID != listItem.TankComp.FlatID)
                    {
                        continue;
                    }

                    // 隣接していないものは平坦化対象ではない
                    if (!t.IsAdjacentToCardinalOrInside(listItem))
                    {
                        continue;
                    }

                    // 見つかったリストを加える
                    foundLists.Add(list);
                    break;
                }
            }

            if (foundLists.Count == 0)
            {
                // 新しい平坦化リストを作成
                flatTankListTmp.Add(new HashSet<IBuilding_WaterNet> { t });
            }
            else if (foundLists.Count == 1)
            {
                // 見つかった平坦化リストに追加
                foundLists[0].Add(t);
            }
            else
            {
                // 複数のリストが見つかった→一つに統合
                var firstList = foundLists[0];
                for (var i = 1; i < foundLists.Count; i++)
                {
                    foreach (var t2 in foundLists[i])
                    {
                        firstList.Add(t2);
                    }

                    flatTankListTmp.Remove(foundLists[i]);
                }
            }
        }

        // 2個以上のタンクが含まれるリストのみ有効
        FlatTankList = flatTankListTmp.Where(list => list.Count >= 2);
    }

    private void RemoveThingFromList(IBuilding_WaterNet thing)
    {
        // 全リストから削除
        AllThings.Remove(thing);

        // 水抜きリストから削除
        drainers.Remove(thing);

        // 入力辞書から削除
        foreach (var item in inputterTypeDic)
        {
            item.Value.Remove(thing);
        }

        // 出力リストから削除
        outputters.Remove(thing);

        // タンクリストから削除
        allTanks.Remove(thing);
        foreach (var item in tankTypeDic)
        {
            item.Value.Remove(thing);
        }

        // 平坦化リストを再作成
        RefreshFlatTankList();
    }
}