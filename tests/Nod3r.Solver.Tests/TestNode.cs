using Nod3r.API;
using Nod3r.Collections;

namespace Nod3r.Solver.Tests;

public struct DummyNodeNet<T>() : INodeNet<T, DummyNodeNet<T>>, INodeNetCreator<DummyNodeNet<T>> where T : INode
{
    public HashSet<LayerId> Nodes { get; } = new();
        
    public void Initialize()
    {
    }

    public void Shutdown()
    {
    }

    public void Merge(IReadOnlySet<DummyNodeNet<T>> nets)
    {
    }

    public void Split(DummyNodeNet<T> parent)
    {
    }

    public static DummyNodeNet<T> CreateNet()
    {
        return new DummyNodeNet<T>();
    }
}

public static class NodeRegistrationExtensions
{
    extension(INodeRegistration registration)
    {
        public void RegisterTest<TNode, TNet>(byte layerCapacity = 1)
            where TNode : INode, ITestNode
            where TNet : INodeNet<TNode, TNet>, INodeNetCreator<TNet>
        {
            registration.Register<TNode, TNet>(NodeRuleBuilder<TNode>.Empty, layerCapacity);
        }
    }
}

public interface ITestNode;

public struct TestNode1 : INode, ITestNode;
public struct TestNode2 : INode, ITestNode;
public struct TestNode3 : INode, ITestNode;
public struct TestNode4 : INode, ITestNode;
public struct TestNode5 : INode, ITestNode;
public struct TestNode6 : INode, ITestNode;
public struct TestNode7 : INode, ITestNode;
public struct TestNode8 : INode, ITestNode;
