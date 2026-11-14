using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nod3r.API.Generators.Common;

namespace Nod3r.API.Generators.Nodes;

[Generator(LanguageNames.CSharp)]
public sealed class NodeNetGenerator : IIncrementalGenerator
{
    internal const string CollectStepName = "NodeNetCollect";
    internal const string TypesStepName = "ChunkDataTypes";
    
    private const string AttributeMetadataName = "Nod3r.API.Attributes.AutoNodeNetworkAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<NodeNetModel> types = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeMetadataName,
                static (node, _) => node is ClassDeclarationSyntax or StructDeclarationSyntax,
                static (attributeContext, cancellationToken) => CreateModel(attributeContext, cancellationToken))
            .WithTrackingName(CollectStepName);
        
        IncrementalValueProvider<GeneratorOutput<NodeNetModel>> output = types
            .Collect()
            .Select(static (input, _) => Analyze(input));
        
        GeneratorOutput.Register(
            context,
            output,
            TypesStepName,
            static type =>
                (type.Partial.HintName + ".NetCreator.g.cs", NodeNetEmitter.Emit(type)));
    }

    private static NodeNetModel CreateModel(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        return new NodeNetModel(PartialTypeModel.From(type), type);
    }

    private static GeneratorOutput<NodeNetModel> Analyze(ImmutableArray<NodeNetModel> nodes)
    {
        return GeneratorOutput<NodeNetModel>.Empty;
    }
}
