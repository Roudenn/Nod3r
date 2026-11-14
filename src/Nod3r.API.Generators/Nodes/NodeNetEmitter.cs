using Nod3r.API.Generators.Common;

namespace Nod3r.API.Generators.Nodes;

internal static class NodeNetEmitter
{
    private const string NodeNetCreatorInterface = "global::Nod3r.Solver.Factories.INodeNetCreator";
    
    internal static string Emit(NodeNetModel type)
    {
        var writer = new CodeWriter();

        writer.Header();
        writer.OpenNamespace(type.Partial);
        writer.Line();
        writer.Line(Types.GetPartialTypeDefinitionLine(type.Type) + $" : {NodeNetCreatorInterface}<{type.Type.Name}>;");
        writer.Open();
        writer.Line($"public static {type.Type.Name} CreateNet()");
        writer.Open();
        writer.Line($"return new {type.Type.Name}();");
        writer.Close();
        writer.Close();
        writer.CloseNamespace(type.Partial);
        return writer.ToString();
    }
}
