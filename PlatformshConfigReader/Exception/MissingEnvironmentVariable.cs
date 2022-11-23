namespace PlatformshConfigReader.Exception;

public class MissingEnvironmentVariable : System.Exception
{
    public MissingEnvironmentVariable()
    {
    }

    public MissingEnvironmentVariable(string? message) : base(message)
    {
    }
}