using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_WaterNetWorkTable : Building_WorkTable, IBuilding_WaterNet
    {
        protected CompFlickable flickableComp;

        private CompWaterNetInput inputComp;

        private CompWaterNetOutput outputComp;

        private CompPowerTrader powerTraderComp;

        private CompWaterSource sourceComp;

        private CompWaterNetTank tankComp;

        public List<IntVec3> ConnectVecs
        {
            get
            {
                var vecs = new List<IntVec3>();
                var rect = OccupiedRect().ExpandedBy(1);

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

                    vecs.Add(cell);
                }

                return vecs;
            }
        }

        public WaterType StoredWaterType
        {
            get
            {
                if (inputComp != null && inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
                {
                    return InputWaterNet.StoredWaterType;
                }

                if (tankComp != null)
                {
                    return tankComp.StoredWaterType;
                }

                return WaterType.NoWater;
            }
        }

        public float StoredWaterVolume
        {
            get
            {
                if (inputComp != null && inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
                {
                    return InputWaterNet.StoredWaterVolume;
                }

                if (tankComp != null)
                {
                    return tankComp.StoredWaterVolume;
                }

                return 0.0f;
            }
        }

        // コネクタがあるか
        public virtual bool HasConnector => HasInputConnector || HasOutputConnector;

        // 水抜き機能があるか
        public bool HasDrainCapability =>
            flickableComp != null && SourceComp != null
                                  && SourceComp.SourceType == CompProperties_WaterSource.SourceType.Building;

        // 入力コネクタがあるか
        public virtual bool HasInputConnector => InputConnectors.Count > 0;

        // 出力コネクタがあるか
        public virtual bool HasOutputConnector => OutputConnectors.Count > 0;

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

        public List<IntVec3> InputConnectors { get; private set; }

        public WaterNet InputWaterNet { get; set; }

        // 機能しているか
        public virtual bool IsActivated =>

            // 壊れていない、電力供給ありor不要、(電力不要でも切り替えがある場合)ONになっている
            !this.IsBrokenDown() && PowerOn && SwitchIsOn;

        // 水道網として機能しているか(水を通すのか)
        // 基本的に電気が通ってなくても、壊れていても水は通す
        public virtual bool IsActivatedForWaterNet => true;

        // 水抜き中か
        public bool IsDraining => flickableComp != null && !flickableComp.SwitchIsOn;

        // 入力コネクタと出力コネクタは同じか
        public virtual bool IsSameConnector => true;

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

        public List<IntVec3> OutputConnectors { get; private set; }

        public WaterNet OutputWaterNet { get; set; }

        // 出力する水の種類
        public virtual WaterType OutputWaterType => WaterType.NoWater;

        // 電力供給が賄えているか
        public bool PowerOn => powerTraderComp == null || powerTraderComp.PowerOn;

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

        // スイッチはONか
        public bool SwitchIsOn => FlickUtility.WantsToBeOn(this);

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

        // 水道網管理オブジェクト
        public MapComponent_WaterNetManager WaterNetManager => Map.GetComponent<MapComponent_WaterNetManager>();

        public virtual UndergroundWaterPool WaterPool => null;

        public virtual void CreateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();
            var rect = OccupiedRect().ExpandedBy(1);

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

        public virtual bool IsAdjacentToCardinalOrInside(IBuilding_WaterNet other)
        {
            return GenAdj.IsAdjacentToCardinalOrInside(OccupiedRect(), other.OccupiedRect());
        }

        public virtual CellRect OccupiedRect()
        {
            return GenAdj.OccupiedRect(this);
        }

        public virtual void PrintForGrid(SectionLayer sectionLayer)
        {
            if (IsActivatedForWaterNet)
            {
                MizuGraphics.LinkedWaterNetOverlay.Print(sectionLayer, this, 0f);
            }
        }

        public void AddWaterVolume(float amount)
        {
            tankComp?.AddWaterVolume(amount);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            WaterNetManager.RemoveThing(this);

            base.DeSpawn(mode);
        }

        public void DrawWaterVolume(float amount)
        {
            if (inputComp != null && inputComp.InputTypes.Contains(CompProperties_WaterNetInput.InputType.WaterNet))
            {
                InputWaterNet.DrawWaterVolume(amount);
            }
            else
            {
                tankComp?.DrawWaterVolume(amount);
            }
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (HasDrainCapability && IsDraining)
            {
                stringBuilder.Append(string.Concat(MizuStrings.InspectWaterTankDraining.Translate()));
            }

            if (!DebugSettings.godMode)
            {
                return stringBuilder.ToString();
            }

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }

            if (InputWaterNet != null)
            {
                stringBuilder.Append(
                    string.Join(
                        ",",
                        $"InNetID({InputWaterNet.ID})",
                        $"Stored({InputWaterNet.StoredWaterVolume:F2},{InputWaterNet.StoredWaterType.ToString()})",
                        $"Flow({InputWaterNet.WaterType})"));
            }
            else
            {
                stringBuilder.Append("InNet(null)");
            }

            stringBuilder.AppendLine();
            if (OutputWaterNet != null)
            {
                stringBuilder.Append(
                    string.Join(
                        ",",
                        $"OutNetID({OutputWaterNet.ID})",
                        $"Stored({OutputWaterNet.StoredWaterVolume:F2},{OutputWaterNet.StoredWaterType.ToString()})",
                        $"Flow({OutputWaterNet.WaterType})"));
            }
            else
            {
                stringBuilder.Append("OutNet(null)");
            }

            return stringBuilder.ToString();
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
    }
}