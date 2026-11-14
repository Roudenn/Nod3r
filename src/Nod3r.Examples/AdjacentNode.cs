using Nod3r.API.Registration;
using Nod3r.API.Rules;
using Nod3r.Collections;
using Nod3r.Solver;
using Nod3r.Solver.Factories;

namespace Nod3r.Examples;

public struct AdjacentNode : INode, INodeAutoRegistration<AdjacentNode>
{
    public static NodeRule<AdjacentNode> ConnectionRule =>
        NodeRuleBuilder.Create()
            .IncludeCartesian()
            .Compile<AdjacentNode>();
}
    
public struct AdjacentNodeNet() : INodeNet<AdjacentNode, AdjacentNodeNet>, INodeNetCreator<AdjacentNodeNet>
{
    public GenId Id { get; }
        
    public HashSet<LayerId> Nodes { get; } = new();

    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlyList<AdjacentNodeNet> nets)
    {
    }

    public void Split(AdjacentNodeNet parent)
    {
    }

    public static AdjacentNodeNet CreateNet()
    {
        return new AdjacentNodeNet();
    }
}
