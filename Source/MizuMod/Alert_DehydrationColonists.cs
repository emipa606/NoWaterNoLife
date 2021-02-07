using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Alert_DehydrationColonists : Alert
    {
        private IEnumerable<Pawn> DehydratingColonists => from p in PawnsFinder.AllMaps_FreeColonistsSpawned
                                                          where p.needs.Water() != null && p.needs.Water().Dehydrating
                                                          select p;

        public Alert_DehydrationColonists()
        {
            defaultLabel = MizuStrings.AlertDehydration.Translate();
            defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            var stringBuilder = new StringBuilder();
            foreach (Pawn current in DehydratingColonists)
            {
                stringBuilder.AppendLine("    " + current.Name.ToStringShort);
            }
            return string.Format(MizuStrings.AlertDehydrationDesc.Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(DehydratingColonists.FirstOrDefault<Pawn>());
        }
    }
}
