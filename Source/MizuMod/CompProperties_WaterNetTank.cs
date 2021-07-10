using System;
using System.Collections.Generic;

namespace MizuMod
{
    public class CompProperties_WaterNetTank : CompProperties_WaterNet
    {
        public enum DrawType : byte
        {
            Undefined = 0,

            Self,

            Faucet
        }

        public List<DrawType> drawTypes = new List<DrawType> {DrawType.Faucet};

        public int flatID = -1;

        public float maxWaterVolume = 0f;

        public bool showBar = true;

        public CompProperties_WaterNetTank()
            : base(typeof(CompWaterNetTank))
        {
        }

        public CompProperties_WaterNetTank(Type compClass)
            : base(compClass)
        {
        }
    }
}