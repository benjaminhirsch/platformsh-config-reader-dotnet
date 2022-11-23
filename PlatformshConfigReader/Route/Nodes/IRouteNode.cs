namespace PlatformshConfigReader.Route.Nodes;

public interface IRouteNode
{
    public RouteType GetRouteType();

    public bool IsPrimary();
}