using System.Collections;
using System.Text.Json;
using PlatformshConfigReader.Exception;
using PlatformshConfigReader.Route;
using PlatformshConfigReader.Route.Nodes;

namespace PlatformshConfigReader;

public class ConfigReader
{
    private readonly IDictionary _environmentVariables;
    private readonly Dictionary<string, JsonElement>? _relationships;
    private readonly Routes _routes;
    private readonly Dictionary<string, string>? _variables;

    public ConfigReader()
    {
        _environmentVariables = Environment.GetEnvironmentVariables();

        var platformRelationShips = GetEnv("PLATFORM_RELATIONSHIPS")?.ToString();
        var platformVariables = GetEnv("PLATFORM_VARIABLES")?.ToString();
        var platformRoutes = GetEnv("PLATFORM_ROUTES")?.ToString();

        if (platformRelationShips == null)
            throw new MissingEnvironmentVariable("Missing $PLATFORM_RELATIONSHIPS, unable to proceed");

        if (platformVariables == null)
            throw new MissingEnvironmentVariable("Missing $PLATFORM_VARIABLES, unable to proceed");

        if (platformRoutes == null)
            throw new MissingEnvironmentVariable("Missing $PLATFORM_ROUTES, unable to proceed");

        var platformRelationShipsBased64Decoded = Convert.FromBase64String(platformRelationShips);
        var platformVariablesBased64Decoded = Convert.FromBase64String(platformVariables);

        _relationships = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            platformRelationShipsBased64Decoded,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Dictionary<string, JsonElement>();

        _variables = JsonSerializer.Deserialize<Dictionary<string, string>>(platformVariablesBased64Decoded,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Dictionary<string, string>();

        _routes = new Routes(ProcessRoutes(
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                Convert.FromBase64String(platformRoutes),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Dictionary<string, JsonElement>()));
    }

    private static IList<IRouteNode> ProcessRoutes(Dictionary<string, JsonElement> rawRoutes)
    {
        var nodeList = new List<IRouteNode>();
        foreach (var route in rawRoutes)
        {
            var type = route.Value.GetProperty("type").GetString();
            switch (type)
            {
                case "upstream":
                    nodeList.Add(new Upstream(
                        route.Value.GetProperty("primary").GetBoolean(),
                        route.Value.GetProperty("id").GetString(),
                        route.Value.GetProperty("upstream").GetString(),
                        route.Value.GetProperty("original_url").GetString()
                    ));
                    break;
                case "redirect":
                    nodeList.Add(new Redirect(
                        route.Value.GetProperty("original_url").GetString(),
                        route.Value.GetProperty("to").GetString()
                    ));
                    break;
                default:
                    throw new UnknownRouteType("Unknown route type found: " + type);
            }
        }

        return nodeList;
    }

    public bool IsValidPlatform()
    {
        return GetEnv("APPLICATION_NAME") != null;
    }

    public bool InBuild()
    {
        return IsValidPlatform() && GetEnv("ENVIRONMENT") == null;
    }

    public bool InRuntime()
    {
        return IsValidPlatform() && GetEnv("ENVIRONMENT") != null;
    }

    private object? GetEnv(string name)
    {
        return _environmentVariables[name];
    }

    public Routes GetRoutes()
    {
        return _routes;
    }

    public IDictionary GetEnvironmentVariables()
    {
        return _environmentVariables;
    }

    private Dictionary<string, JsonElement>? GetRelationships()
    {
        return _relationships;
    }

    public Dictionary<string, string>? GetVariables()
    {
        return _variables;
    }

    public string? GetVariable(string key)
    {
        return GetVariables()!.GetValueOrDefault(key, null);
    }

    public JsonElement GetCredentials(string relationship, int index = 0)
    {
        return GetRelationships()!.GetValueOrDefault(relationship)[index];
    }

    public bool HasRelationship(string name)
    {
        return GetRelationships()!.ContainsKey(name);
    }
}