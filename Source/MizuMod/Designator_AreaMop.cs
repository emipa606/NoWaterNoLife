using RimWorld;
using Verse;

namespace MizuMod
{
    public abstract class Designator_AreaMop : Designator
    {
        private readonly DesignateMode mode;

        public Designator_AreaMop(DesignateMode mode)
        {
            this.mode = mode;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            useMouseIcon = true;

            // this.hotKey = KeyBindingDefOf.Misc7;
            // this.tutorTag = "AreaSnowClear";
        }

        public override bool DragDrawMeasurements => true;

        public override int DraggableDimensions => 2;

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
            {
                return false;
            }

            var flag = Map.areaManager.Mop()[c];
            if (mode == DesignateMode.Add)
            {
                return !flag;
            }

            return flag;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (mode == DesignateMode.Add)
            {
                Map.areaManager.Mop()[c] = true;
            }
            else
            {
                Map.areaManager.Mop()[c] = false;
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            Map.areaManager.Mop().MarkForDraw();
        }
    }
}