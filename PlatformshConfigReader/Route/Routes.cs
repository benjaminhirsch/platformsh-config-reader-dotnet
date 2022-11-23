using PlatformshConfigReader.Route.Nodes;

namespace PlatformshConfigReader.Route;

public class Routes
{
    private readonly IList<IRouteNode> _nodes;

    public Routes(IList<IRouteNode>? nodes = null)
    {
        _nodes = nodes ?? new List<IRouteNode>();
    }

    public void Add(IRouteNode node)
    {
        _nodes.Add(node);
    }

    public IList<IRouteNode> GetNodes()
    {
        return _nodes;
    }

    public Upstream GetPrimaryRoute()
    {
        return (Upstream)GetNodes().First(n => n.IsPrimary());
    }
}

public enum RouteType : uint
{
    Upstream = 0,
    Redirect = 1
}