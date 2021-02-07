using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    // バルブの場合、スイッチON/OFF⇒バルブの開閉(水を通すかどうか)
    public class Building_Valve : Building_WaterNet, IBuilding_WaterNet
    {
        private bool lastSwitchIsOn = true;

        public override bool HasInputConnector => base.HasInputConnector && SwitchIsOn;

        public override bool HasOutputConnector => base.HasOutputConnector && SwitchIsOn;

        public override bool IsActivatedForWaterNet => base.IsActivatedForWaterNet && SwitchIsOn;

        public override Graphic Graphic
        {
            get
            {
                if (flickableComp == null)
                {
                    return base.Graphic;
                }

                return flickableComp.CurrentGraphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref lastSwitchIsOn, "lastSwitchIsOn");
        }

        public override void Tick()
        {
            base.Tick();

            if (lastSwitchIsOn != SwitchIsOn)
            {
                lastSwitchIsOn = SwitchIsOn;
                WaterNetManager.UpdateWaterNets();
            }
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (!SwitchIsOn)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append(MizuStrings.InspectValveClosed.Translate());
            }
            return stringBuilder.ToString();
        }

        public override void CreateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();

            InputConnectors.Add(Position + Rotation.FacingCell);
            InputConnectors.Add(Position + (Rotation.FacingCell * (-1)));

            OutputConnectors.Add(Position + Rotation.FacingCell);
            OutputConnectors.Add(Position + (Rotation.FacingCell * (-1)));
        }
    }
}
