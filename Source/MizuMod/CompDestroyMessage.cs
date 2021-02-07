using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class CompDestroyMessage : ThingComp
    {
        public CompProperties_DestroyMessage Props => (CompProperties_DestroyMessage)props;

        public string MessageKey => Props.messageKey;

        public List<DestroyMode> DestroyModes => Props.destroyModes;

        public CompDestroyMessage() { }

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

            MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), previousMap, MessageKey.Translate(), Color.white);
        }
    }
}
