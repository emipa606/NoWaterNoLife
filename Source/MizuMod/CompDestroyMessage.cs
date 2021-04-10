using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class CompDestroyMessage : ThingComp
    {
        public List<DestroyMode> DestroyModes => Props.destroyModes;

        public string MessageKey => Props.messageKey;

        public CompProperties_DestroyMessage Props => (CompProperties_DestroyMessage)props;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (string.IsNullOrEmpty(MessageKey))
            {
                return;
            }

            if (DestroyModes == null || !DestroyModes.Contains(mode))
            {
                return;
            }

            MoteMaker.ThrowText(
                parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f),
                previousMap,
                MessageKey.Translate(),
                Color.white);
        }
    }
}