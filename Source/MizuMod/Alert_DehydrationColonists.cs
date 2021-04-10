using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Alert_DehydrationColonists : Alert
    {
        public Alert_DehydrationColonists()
        {
            defaultLabel = MizuStrings.AlertDehydration.Translate();
            defaultPriority = AlertPriority.High;
        }

        private IEnumerable<Pawn> DehydratingColonists =>
            from p in PawnsFinder.AllMaps_FreeColonistsSpawned
            where p.needs.Water() != null && p.needs.Water().Dehydrating
            select p;

        public override TaggedString GetExplanation()
        {
            var stringBuilder = new StringBuilder();
            foreach (var current in DehydratingColonists)
            {
                stringBuilder.AppendLine("    " + current.Name.ToStringShort);
            }

            return string.Format(MizuStrings.AlertDehydrationDesc.Translate(), stringBuilder);
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(DehydratingColonists.FirstOrDefault());
        }
    }
}