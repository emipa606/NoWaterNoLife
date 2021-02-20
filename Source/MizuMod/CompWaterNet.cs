using RimWorld;
using Verse;

namespace MizuMod
{
    public abstract class CompWaterNet : ThingComp
    {
        private bool lastIsActivatedForWaterNet;

        protected bool IsBrokenDown => parent.IsBrokenDown();

        protected bool SwitchIsOn => WaterNetBuilding.SwitchIsOn;

        protected bool PowerOn => WaterNetBuilding.PowerOn;

        public virtual bool IsActivated => WaterNetBuilding.IsActivated;

        protected bool IsActivatedForWaterNet => WaterNetBuilding.IsActivatedForWaterNet;

        public CompProperties_WaterNet Props => (CompProperties_WaterNet) props;

        protected IBuilding_WaterNet WaterNetBuilding => parent as IBuilding_WaterNet;

        private MapComponent_WaterNetManager WaterNetManager => parent.Map.GetComponent<MapComponent_WaterNetManager>();

        protected CompWaterNetInput InputComp => WaterNetBuilding.InputComp;
        protected CompWaterNetOutput OutputComp => WaterNetBuilding.OutputComp;
        protected CompWaterNetTank TankComp => WaterNetBuilding.TankComp;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastIsActivatedForWaterNet, "lastIsActivatedForWaterNet");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            lastIsActivatedForWaterNet = IsActivatedForWaterNet;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (lastIsActivatedForWaterNet == IsActivatedForWaterNet)
            {
                return;
            }

            lastIsActivatedForWaterNet = IsActivatedForWaterNet;
            foreach (var vec in WaterNetBuilding.OccupiedRect().ExpandedBy(1))
            {
                WaterNetManager.map.mapDrawer.MapMeshDirty(vec, MapMeshFlag.Things);
                WaterNetManager.map.mapDrawer.MapMeshDirty(vec, MapMeshFlag.Buildings);
            }

            WaterNetManager.RequestUpdateWaterNet();
        }
    }
}