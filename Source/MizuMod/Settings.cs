using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Settings : ModSettings
    {
        private const float DefaultFertilityFactorInNotWatering = 1.0f;

        private const float DefaultFertilityFactorInWatering = 1.2f;

        private const float MaxFertilityFactorInNotWatering = 1.0f;

        private const float MaxFertilityFactorInWatering = 5.0f;

        private const float MinFertilityFactorInNotWatering = 0.0f;

        private const float MinFertilityFactorInWatering = 0.1f;

        // 水やりしていない時の肥沃度係数
        private float fertilityFactorInNotWatering;

        private string fertilityFactorInNotWateringBuffer;

        // 水やりした時の肥沃度係数
        private float fertilityFactorInWatering;

        private string fertilityFactorInWateringBuffer;

        public Settings()
        {
            fertilityFactorInNotWatering = DefaultFertilityFactorInNotWatering;
            fertilityFactorInWatering = DefaultFertilityFactorInWatering;
        }

        public float FertilityFactorInNotWatering => fertilityFactorInNotWatering;

        public float FertilityFactorInWatering => fertilityFactorInWatering;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing_standard = new Listing_Standard();

            listing_standard.Begin(inRect);

            // デフォルトに戻す
            if (listing_standard.ButtonText(MizuStrings.OptionSetDefault.Translate()))
            {
                fertilityFactorInNotWatering = DefaultFertilityFactorInNotWatering;
                fertilityFactorInNotWateringBuffer = fertilityFactorInNotWatering.ToString("F2");
                fertilityFactorInWatering = DefaultFertilityFactorInWatering;
                fertilityFactorInWateringBuffer = fertilityFactorInWatering.ToString("F2");
            }

            // 水やりしていない時の肥沃度係数
            listing_standard.Label(string.Concat(MizuStrings.OptionGrowthRateFactorInNotWatering.Translate(), " (", MinFertilityFactorInNotWatering.ToString("F2"), " - ", MaxFertilityFactorInNotWatering.ToString("F2"), ")"));
            listing_standard.TextFieldNumeric(ref fertilityFactorInNotWatering, ref fertilityFactorInNotWateringBuffer, MinFertilityFactorInNotWatering, MaxFertilityFactorInNotWatering);

            // 水やりした時の肥沃度係数
            listing_standard.Label(string.Concat(MizuStrings.OptionGrowthRateFactorInWatering.Translate(), " (", MinFertilityFactorInWatering.ToString("F2"), " - ", MaxFertilityFactorInWatering.ToString("F2"), ")"));
            listing_standard.TextFieldNumeric(ref fertilityFactorInWatering, ref fertilityFactorInWateringBuffer, MinFertilityFactorInWatering, MaxFertilityFactorInWatering);

            listing_standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref fertilityFactorInNotWatering, "fertilityFactorInNotWatering", DefaultFertilityFactorInNotWatering);
            Scribe_Values.Look(ref fertilityFactorInWatering, "fertilityFactorInWatering", DefaultFertilityFactorInWatering);
        }
    }
}