namespace Nod3r.Types;

// TODO: consider making all node rules static

/// <summary>
/// Interface for node rules that allows to statically create new instances of the same type.
/// </summary>
/// <typeparam name="T">Type of node network, must be the same as the implemented type.</typeparam>
public interface INodeRuleCreator<out T> where T : INodeRule
{
    abstract static T CreateRule();
}
