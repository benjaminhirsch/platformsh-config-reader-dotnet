namespace PlatformshConfigReader.Exception;

public class UnknownRouteType : System.Exception
{
    public UnknownRouteType()
    {
    }

    public UnknownRouteType(string? message) : base(message)
    {
    }
}