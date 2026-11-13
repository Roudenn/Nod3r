using Microsoft.CodeAnalysis;
using Nod3r.API.Generators.Common;

namespace Nod3r.API.Generators.Nodes;

internal sealed record NodeNetModel(
    PartialTypeModel Partial,
    ITypeSymbol Type);
    