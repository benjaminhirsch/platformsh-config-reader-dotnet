namespace PlatformshConfigReader.Route.Nodes;

public class Redirect : IRouteNode
{
    public Redirect(string? originalUrl, string? to)
    {
        Type = RouteType.Redirect;
        OriginalUrl = originalUrl ?? throw new ArgumentNullException(nameof(originalUrl));
        To = to ?? throw new ArgumentNullException(nameof(to));
    }

    private RouteType Type { get; }
    public string OriginalUrl { get; }
    public string To { get; }

    public RouteType GetRouteType()
    {
        return Type;
    }

    public bool IsPrimary()
    {
        return false;
    }
}