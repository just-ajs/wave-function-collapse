using Grasshopper.Kernel;

namespace WaveFunctionCollapse
{
    internal class PatternParameter : IGH_Param
    {
    }

    public class GH_Toolpath : GH_Goo<IToolpath>
    {
        public GH_Toolpath() { Value = new SimpleToolpath(); }
        public GH_Toolpath(IToolpath native) { Value = native; }
        public override IGH_Goo Duplicate() => new GH_Toolpath(Value);
        public override bool IsValid => true;
        public override string TypeName => "Toolpath";
        public override string TypeDescription => "Toolpath";
        public override string ToString() => Value?.ToString();
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            if (source is GH_Target)
            {
                Value = (source as GH_Target).Value;
                return true;
            }

            if (source is IToolpath)
            {
                Value = source as IToolpath;
                return true;
            }
            return false;
        }
    }
}