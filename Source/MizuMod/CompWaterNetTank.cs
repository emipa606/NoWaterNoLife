using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace MizuMod
{
    [StaticConstructorOnStartup]
    public class CompWaterNetTank : CompWaterNet
    {
        private static readonly float BarThick = 0.15f;
        private static readonly Material BarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.8f, 0.8f), false);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

        public new CompProperties_WaterNetTank Props => (CompProperties_WaterNetTank)props;

        public float AmountCanAccept
        {
            get
            {
                if (!IsActivated)
                {
                    return 0f;
                }
                return MaxWaterVolume - StoredWaterVolume;
            }
        }

        public float MaxWaterVolume => Props.maxWaterVolume;
        public bool ShowBar => Props.showBar;
        public int FlatID => Props.flatID;
        public List<CompProperties_WaterNetTank.DrawType> DrawTypes => Props.drawTypes;

        private float storedWaterVolume = 0;
        public float StoredWaterVolume
        {
            get => storedWaterVolume;
            set
            {
                if (value > MaxWaterVolume)
                {
                    storedWaterVolume = MaxWaterVolume;
                }
                else if (value < 0.0f)
                {
                    storedWaterVolume = 0.0f;
                }
                else
                {
                    storedWaterVolume = value;
                }
            }
        }

        public float StoredWaterVolumePercent
        {
            get
            {
                if (MaxWaterVolume <= 0f)
                {
                    return 0f;
                }

                return StoredWaterVolume / MaxWaterVolume;
            }
        }

        private WaterType storedWaterType = WaterType.NoWater;
        public WaterType StoredWaterType
        {
            get => storedWaterType;
            set => storedWaterType = value;
        }

        private CompFlickable compFlickable = null;

        public CompWaterNetTank() : base()
        {
            storedWaterVolume = 0.0f;
            storedWaterType = WaterType.NoWater;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref storedWaterVolume, "storedWaterVolume");
            Scribe_Values.Look<WaterType>(ref storedWaterType, "storedWaterType", WaterType.NoWater);

            if (storedWaterVolume > MaxWaterVolume)
            {
                storedWaterVolume = MaxWaterVolume;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compFlickable = parent.GetComp<CompFlickable>();
        }

        public float AddWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative water volume " + amount);
                return 0.0f;
            }

            var prevWaterVolume = StoredWaterVolume;
            StoredWaterVolume += amount;
            return StoredWaterVolume - prevWaterVolume;
        }

        public float DrawWaterVolume(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot draw negative water volume " + amount);
                return 0.0f;
            }
            var prevWaterVolume = StoredWaterVolume;
            StoredWaterVolume -= amount;
            return prevWaterVolume - StoredWaterVolume;
        }

        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append(string.Concat(new string[]
            {
                MizuStrings.InspectWaterTankStored.Translate(),
                ": ",
                StoredWaterVolume.ToString("F2"),
                " / ",
                MaxWaterVolume.ToString("F2"),
                " L"
            }));
            stringBuilder.Append(string.Concat(new string[]
            {
                "(",
                MizuStrings.GetInspectWaterTypeString(StoredWaterType),
                ")",
            }));

            return stringBuilder.ToString();
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (ShowBar)
            {
                var r = new GenDraw.FillableBarRequest
                {
                    center = parent.DrawPos + (Vector3.up * 0.1f) + (Vector3.back * parent.def.size.z / 4.0f),
                    size = new Vector2(parent.RotatedSize.x, BarThick),
                    //r.center = new Vector3(this.parent.DrawPos.x, 0.1f, 1.0f - r.size.y / 2.0f);
                    //Log.Message(this.parent.DrawPos.ToString());
                    fillPercent = StoredWaterVolume / MaxWaterVolume,
                    filledMat = BarFilledMat,
                    unfilledMat = BarUnfilledMat,
                    margin = 0.15f
                };
                GenDraw.DrawFillableBar(r);
            }
        }
    }
}
