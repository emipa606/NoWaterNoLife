using System.Text;
using Verse;

namespace MizuMod;

// バルブの場合、スイッチON/OFF⇒バルブの開閉(水を通すかどうか)
public class Building_Valve : Building_WaterNet
{
    private bool lastSwitchIsOn = true;

    public override Graphic Graphic => flickableComp == null ? base.Graphic : flickableComp.CurrentGraphic;

    public override bool HasInputConnector => base.HasInputConnector && SwitchIsOn;

    public override bool HasOutputConnector => base.HasOutputConnector && SwitchIsOn;

    public override bool IsActivatedForWaterNet => base.IsActivatedForWaterNet && SwitchIsOn;

    public override void CreateConnectors()
    {
        InputConnectors.Clear();
        OutputConnectors.Clear();

        InputConnectors.Add(Position + Rotation.FacingCell);
        InputConnectors.Add(Position + (Rotation.FacingCell * -1));

        OutputConnectors.Add(Position + Rotation.FacingCell);
        OutputConnectors.Add(Position + (Rotation.FacingCell * -1));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastSwitchIsOn, "lastSwitchIsOn");
    }

    protected override void Tick()
    {
        base.Tick();

        if (lastSwitchIsOn == SwitchIsOn)
        {
            return;
        }

        lastSwitchIsOn = SwitchIsOn;
        WaterNetManager.UpdateWaterNets();
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());

        if (SwitchIsOn)
        {
            return stringBuilder.ToString();
        }

        if (stringBuilder.Length > 0)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(MizuStrings.InspectValveClosed.Translate());

        return stringBuilder.ToString();
    }
}