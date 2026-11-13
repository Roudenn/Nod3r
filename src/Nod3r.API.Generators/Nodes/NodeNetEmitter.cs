using Nod3r.API.Generators.Common;

namespace Nod3r.API.Generators.Nodes;

internal static class NodeNetEmitter
{
    private const string NodeNetCreatorInterface = "";
    
    internal static string Emit(NodeNetModel type)
    {
        var writer = new CodeWriter();

        writer.Header();
        writer.OpenNamespace(type.Partial);
        writer.Line();
        writer.Line(Types.GetPartialTypeDefinitionLine(type.Type) + $" : {NodeNetCreatorInterface}<{type.Type.Name}>;");
        writer.Open();
        writer.Line("");
        writer.Close();
        writer.CloseNamespace(type.Partial);
        return writer.ToString();
    }
}
