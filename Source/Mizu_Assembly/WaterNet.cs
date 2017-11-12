﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class WaterNet
    {
        private static int nextID = 1;

        public int ID = 0;

        private List<ThingWithComps> things = new List<ThingWithComps>();

        public List<ThingWithComps> Things
        {
            get
            {
                return things;
            }
        }

        public MapComponent_WaterNetManager Manager { get; set; }

        public WaterNet()
        {
            this.ID = nextID;
            nextID++;
        }

        public void AddThing(ThingWithComps thing)
        {
            CompWaterNet comp = thing.GetComp<CompWaterNet>();
            if (comp != null)
            {
                comp.WaterNet = this;
            }
            else
            {
                Log.Error("CompWaterNet is null");
            }
            things.Add(thing);
        }

        public void RemoveThing(ThingWithComps thing)
        {
            CompWaterNet comp = thing.GetComp<CompWaterNet>();
            if (comp != null)
            {
                comp.WaterNet = null;
            }
            things.Remove(thing);
        }

        // 仮
        public float WaterVolume
        {
            get
            {
                List<ThingWithComps> tanks = things.FindAll((t) => t.GetComp<CompWaterNetTank>() != null);

                float sumTankWaterVolume = 0.0f;
                foreach (var tank in tanks)
                {
                    sumTankWaterVolume += tank.GetComp<CompWaterNetTank>().StoredWaterVolume;
                }

                return sumTankWaterVolume;
            }
        }

        // 仮
        public void DrawWaterVolume(float amount)
        {
            float totalAmount = amount;

            while (totalAmount > 0.0f)
            {
                List<ThingWithComps> tanks = things.FindAll((t) =>
                {
                    CompWaterNetTank compTank = t.GetComp<CompWaterNetTank>();
                    return (compTank != null) && (compTank.StoredWaterVolume > 0.0f);
                });

                if (tanks.Count == 0)
                {
                    break;
                }

                float averageAmount = totalAmount / tanks.Count;
                foreach (var tank in tanks)
                {
                    CompWaterNetTank compTank = tank.GetComp<CompWaterNetTank>();
                    totalAmount -= compTank.DrawWaterVolume(averageAmount);
                }
            }
        }

        public void AddWaterVolume(float amount)
        {
            List<ThingWithComps> notFullTanks = things.FindAll((t) => {
                CompWaterNetTank comp = t.GetComp<CompWaterNetTank>();
                return (comp != null) && (comp.NeedSupply);
            });

            if (notFullTanks.Count > 0)
            {
                float averageWaterFlow = amount / notFullTanks.Count;

                foreach (var notFullTank in notFullTanks)
                {
                    notFullTank.GetComp<CompWaterNetTank>().AddWaterVolume(averageWaterFlow);
                }
            }
        }

        public void Tick()
        {
            List<ThingWithComps> pumps = things.FindAll((t) => t.GetComp<CompWaterNetPump>() != null);
            List<ThingWithComps> tanks = things.FindAll((t) => t.GetComp<CompWaterNetTank>() != null);

            float sumPumpWaterFlow = 0.0f;
            foreach (var pump in pumps)
            {
                sumPumpWaterFlow += pump.GetComp<CompWaterNetPump>().WaterFlow;
            }
            float sumPumpWaterFlowPerTick = sumPumpWaterFlow / 60000;

            if (sumPumpWaterFlowPerTick > 0.0f)
            {
                this.AddWaterVolume(sumPumpWaterFlowPerTick);
            }
            else if (sumPumpWaterFlowPerTick < 0.0f)
            {
                // 未実装
            }
        }
    }
}
