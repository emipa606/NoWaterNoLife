using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MizuMod;

public class Alert_DehydrationAnimals : Alert
{
    public Alert_DehydrationAnimals()
    {
        defaultLabel = MizuStrings.AlertDehydrationAnimal.Translate();
    }

    private IEnumerable<Pawn> DehydratingAnimals =>
        from p in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer)
        where p.HostFaction == null && !p.RaceProps.Humanlike
        where p.needs.Water() != null && p.needs.Water().Dehydrating
        select p;

    public override TaggedString GetExplanation()
    {
        var stringBuilder = new StringBuilder();
        foreach (var current in from a in DehydratingAnimals orderby a.def.label select a)
        {
            stringBuilder.Append($"    {current.Name.ToStringShort}");
            if (current.Name.IsValid && !current.Name.Numerical)
            {
                stringBuilder.Append($" ({current.def.label})");
            }

            stringBuilder.AppendLine();
        }

        return string.Format(MizuStrings.AlertDehydrationAnimalDesc.Translate(), stringBuilder);
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritIs(DehydratingAnimals.FirstOrDefault());
    }
}