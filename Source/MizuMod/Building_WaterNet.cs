using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_WaterNet : Building, IBuilding_WaterNet
    {
        // コネクタがあるか
        public virtual bool HasConnector => HasInputConnector || HasOutputConnector;

        // 入力コネクタがあるか
        public virtual bool HasInputConnector => InputConnectors.Count > 0;

        // 出力コネクタがあるか
        public virtual bool HasOutputConnector => OutputConnectors.Count > 0;

        // 入力コネクタと出力コネクタは同じか
        public virtual bool IsSameConnector => true;

        // 電力供給が賄えているか
        public bool PowerOn => powerTraderComp == null || powerTraderComp.PowerOn;

        // スイッチはONか
        public bool SwitchIsOn => FlickUtility.WantsToBeOn(this);

        // 機能しているか
        public virtual bool IsActivated =>
                // 壊れていない、電力供給ありor不要、(電力不要でも切り替えがある場合)ONになっている
                !this.IsBrokenDown() && PowerOn && SwitchIsOn;

        // 水道網として機能しているか(水を通すのか)
        // 基本的に電気が通ってなくても、壊れていても水は通す
        public virtual bool IsActivatedForWaterNet => true;

        // 水道網管理オブジェクト
        public MapComponent_WaterNetManager WaterNetManager => Map.GetComponent<MapComponent_WaterNetManager>();

        // 出力する水の種類
        public virtual WaterType OutputWaterType => WaterType.NoWater;

        public virtual UndergroundWaterPool WaterPool => null;

        public bool HasDrainCapability => flickableComp != null && sourceComp != null && sourceComp.SourceType == CompProperties_WaterSource.SourceType.Building;

        // 水抜き中か
        public bool IsDraining => flickableComp != null && !flickableComp.SwitchIsOn;

        public WaterNet InputWaterNet { get; set; }
        public WaterNet OutputWaterNet { get; set; }

        public virtual List<IntVec3> InputConnectors { get; private set; }
        public virtual List<IntVec3> OutputConnectors { get; private set; }

        private CompPowerTrader powerTraderComp;
        protected CompFlickable flickableComp;

        private CompWaterSource sourceComp;
        public CompWaterSource SourceComp
        {
            get
            {
                if (sourceComp == null)
                {
                    sourceComp = GetComp<CompWaterSource>();
                }

                return sourceComp;
            }
        }
        private CompWaterNetInput inputComp;
        public CompWaterNetInput InputComp
        {
            get
            {
                if (inputComp == null)
                {
                    inputComp = GetComp<CompWaterNetInput>();
                }

                return inputComp;
            }
        }
        private CompWaterNetOutput outputComp;
        public CompWaterNetOutput OutputComp
        {
            get
            {
                if (outputComp == null)
                {
                    outputComp = GetComp<CompWaterNetOutput>();
                }

                return outputComp;
            }
        }
        private CompWaterNetTank tankComp;
        public CompWaterNetTank TankComp
        {
            get
            {
                if (tankComp == null)
                {
                    tankComp = GetComp<CompWaterNetTank>();
                }

                return tankComp;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            powerTraderComp = GetComp<CompPowerTrader>();
            flickableComp = GetComp<CompFlickable>();

            InputConnectors = new List<IntVec3>();
            OutputConnectors = new List<IntVec3>();
            CreateConnectors();

            WaterNetManager.AddThing(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            WaterNetManager.RemoveThing(this);

            base.DeSpawn(mode);
        }

        public virtual void CreateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();
            CellRect rect = OccupiedRect().ExpandedBy(1);

            foreach (var cell in rect.EdgeCells)
            {
                if (cell.x == rect.minX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.minX && cell.z == rect.maxZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.minZ)
                {
                    continue;
                }
                if (cell.x == rect.maxX && cell.z == rect.maxZ)
                {
                    continue;
                }
                InputConnectors.Add(cell);
                OutputConnectors.Add(cell);
            }
        }

        public virtual void PrintForGrid(SectionLayer sectionLayer)
        {
            if (IsActivatedForWaterNet)
            {
                MizuGraphics.LinkedWaterNetOverlay.Print(sectionLayer, this);
            }
        }

        public virtual CellRect OccupiedRect()
        {
            return GenAdj.OccupiedRect(this);
        }

        public virtual bool IsAdjacentToCardinalOrInside(IBuilding_WaterNet other)
        {
            return GenAdj.IsAdjacentToCardinalOrInside(OccupiedRect(), other.OccupiedRect());
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (HasDrainCapability && IsDraining)
            {
                stringBuilder.Append(string.Concat(new string[]
                {
                    MizuStrings.InspectWaterTankDraining.Translate(),
                }));
            }

            if (DebugSettings.godMode)
            {
                if (stringBuilder.ToString() != string.Empty)
                {
                    stringBuilder.AppendLine();
                }
                if (InputWaterNet != null)
                {
                    stringBuilder.Append(string.Join(",", new string[] {
                        string.Format("InNetID({0})", InputWaterNet.ID),
                        string.Format("Stored({0},{1})", InputWaterNet.StoredWaterVolume.ToString("F2"), InputWaterNet.StoredWaterType.ToString()),
                        string.Format("Flow({0})", InputWaterNet.WaterType),
                    }));
                }
                else
                {
                    stringBuilder.Append("InNet(null)");
                }
                stringBuilder.AppendLine();
                if (OutputWaterNet != null)
                {
                    stringBuilder.Append(string.Join(",", new string[] {
                        string.Format("OutNetID({0})", OutputWaterNet.ID),
                        string.Format("Stored({0},{1})", OutputWaterNet.StoredWaterVolume.ToString("F2"), OutputWaterNet.StoredWaterType.ToString()),
                        string.Format("Flow({0})", OutputWaterNet.WaterType),
                    }));
                }
                else
                {
                    stringBuilder.Append("OutNet(null)");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
