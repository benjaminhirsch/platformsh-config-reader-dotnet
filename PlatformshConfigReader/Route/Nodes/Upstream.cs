namespace PlatformshConfigReader.Route.Nodes;

public class Upstream : IRouteNode
{
    public Upstream(bool primary, string? id, string? upstream, string? originalUrl)
    {
        PrimaryRoute = primary;
        Type = RouteType.Upstream;
        Id = id;
        RelatedUpstream = upstream ?? throw new ArgumentNullException(nameof(upstream));
        OriginalUrl = originalUrl ?? throw new ArgumentNullException(nameof(originalUrl));
    }

    private bool PrimaryRoute { get; }
    private RouteType Type { get; }
    public string? Id { get; }
    public string RelatedUpstream { get; }
    public string OriginalUrl { get; }

    public RouteType GetRouteType()
    {
        return Type;
    }

    public bool IsPrimary()
    {
        return PrimaryRoute;
    }
}