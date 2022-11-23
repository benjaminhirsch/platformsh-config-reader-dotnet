using System.Text;
using PlatformshConfigReader.Exception;
using PlatformshConfigReader.Route;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PlatformshConfigReaderTest;

[TestCaseOrderer("PlatformshConfigReaderTest.AlphabeticalOrderer", "PlatformshConfigReaderTest")]
public class ConfigReader
{
    [Fact]
    public void EnvMissingTest1()
    {
        var exception = Assert.Throws<MissingEnvironmentVariable>(() => new PlatformshConfigReader.ConfigReader());
        Assert.Equal("Missing $PLATFORM_RELATIONSHIPS, unable to proceed", exception.Message);
    }

    [Fact]
    public void EnvMissingTest2()
    {
        SetRelationshipsEnv();

        var exception = Assert.Throws<MissingEnvironmentVariable>(() => new PlatformshConfigReader.ConfigReader());
        Assert.Equal("Missing $PLATFORM_VARIABLES, unable to proceed", exception.Message);
    }

    [Fact]
    public void EnvMissingTest3()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();

        var exception = Assert.Throws<MissingEnvironmentVariable>(() => new PlatformshConfigReader.ConfigReader());
        Assert.Equal("Missing $PLATFORM_ROUTES, unable to proceed", exception.Message);
    }

    [Fact]
    public void TestSuccessfulInstantiation()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        Assert.IsType<PlatformshConfigReader.ConfigReader>(configReader);
        Assert.NotNull(configReader.GetVariables());
        Assert.Equal(2, configReader.GetVariables()?.Count);
        Assert.Equal(10, configReader.GetRoutes().GetNodes().Count);
    }

    [Fact]
    public void TestReceivingCredentials()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        var databaseCredentials = configReader.GetCredentials("database");
        Assert.Equal("246.0.97.91", databaseCredentials.GetProperty("ip").GetString());
        Assert.Equal("database.internal", databaseCredentials.GetProperty("host").GetString());
        Assert.True(databaseCredentials.GetProperty("query").GetProperty("is_master").GetBoolean());
        Assert.Equal("main", databaseCredentials.GetProperty("path").GetString());

        var redisCredentials = configReader.GetCredentials("redis");
        Assert.Equal("246.0.97.88", redisCredentials.GetProperty("ip").GetString());
        Assert.Equal("redis.internal", redisCredentials.GetProperty("host").GetString());

        var primaryRoute = configReader.GetRoutes().GetPrimaryRoute();
        Assert.Equal("https://{default}/conferences", primaryRoute.OriginalUrl);
        Assert.Equal("conference", primaryRoute.Id);
        Assert.Equal(RouteType.Upstream, primaryRoute.GetRouteType());

        Assert.Equal("6", configReader.GetVariable("dotnet.version"));
        Assert.Equal("value", configReader.GetVariable("variable"));
    }

    [Fact]
    public void TestIsValidPlatform()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        Assert.False(configReader.IsValidPlatform());

        var property =
            typeof(PlatformshConfigReader.ConfigReader).GetProperty("_environmentVariables");
        var newEnvironmentVariables = configReader.GetEnvironmentVariables();
        newEnvironmentVariables.Add("APPLICATION_NAME", "Foo");

        property?.SetValue(configReader, newEnvironmentVariables);

        Assert.True(configReader.IsValidPlatform());
    }

    [Fact]
    public void TestHasRelationshipCheck()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        Assert.False(configReader.HasRelationship("nonexistingrelationship"));
        Assert.True(configReader.HasRelationship("database"));
    }

    [Fact]
    public void TestInBuildTrue()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        Assert.False(configReader.InBuild());

        var property =
            typeof(PlatformshConfigReader.ConfigReader).GetProperty("_environmentVariables");
        var newEnvironmentVariables = configReader.GetEnvironmentVariables();
        newEnvironmentVariables.Add("APPLICATION_NAME", "Foo");
        property?.SetValue(configReader, newEnvironmentVariables);

        Assert.True(configReader.InBuild());
    }

    [Fact]
    public void TestInBuildFalseAndInRuntimeTrue()
    {
        SetRelationshipsEnv();
        SetVariablesEnv();
        SetRoutesEnv();

        var configReader = new PlatformshConfigReader.ConfigReader();
        Assert.False(configReader.InBuild());

        var property =
            typeof(PlatformshConfigReader.ConfigReader).GetProperty("_environmentVariables");
        var newEnvironmentVariables = configReader.GetEnvironmentVariables();
        newEnvironmentVariables.Add("APPLICATION_NAME", "Foo");
        newEnvironmentVariables.Add("ENVIRONMENT", "Foo");
        property?.SetValue(configReader, newEnvironmentVariables);

        Assert.False(configReader.InBuild());
        Assert.True(configReader.InRuntime());
    }

    private static void SetRelationshipsEnv()
    {
        const string jsonString = @"{
  ""database"": [
    {
      ""host"": ""database.internal"",
      ""ip"": ""246.0.97.91"",
      ""password"": """",
      ""path"": ""main"",
      ""port"": 3306,
      ""query"": {
        ""is_master"": true
      },
      ""scheme"": ""mysql"",
      ""username"": ""user""
    }
  ],
  ""redis"": [
    {
      ""host"": ""redis.internal"",
      ""ip"": ""246.0.97.88"",
      ""port"": 6379,
      ""scheme"": ""redis""
    }
  ]
}";
        Environment.SetEnvironmentVariable(
            "PLATFORM_RELATIONSHIPS",
            EncodeJsonStringAsBase64(jsonString));
    }

    private static void SetVariablesEnv()
    {
        const string jsonString = @"{
  ""dotnet.version"": ""6"",
  ""variable"": ""value""
}";
        Environment.SetEnvironmentVariable("PLATFORM_VARIABLES",
            EncodeJsonStringAsBase64(jsonString));
    }

    private static void SetRoutesEnv()
    {
        const string jsonString = @"{
  ""http://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/speakers"": {
    ""id"": null,
    ""primary"": false,
    ""to"": ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/speakers"",
    ""type"": ""redirect"",
    ""original_url"": ""http://{default}/speakers""
  },
  ""http://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/sessions"": {
    ""primary"": false,
    ""id"": null,
    ""original_url"": ""http://{default}/sessions"",
    ""to"": ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/sessions"",
    ""type"": ""redirect""
  },
  ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"": {
    ""upstream"": ""client"",
    ""type"": ""upstream"",
    ""original_url"": ""https://{default}/"",
    ""attributes"": {},
    ""id"": null,
    ""primary"": false
  },
  ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/conferences"": {
    ""original_url"": ""https://{default}/conferences"",
    ""attributes"": {},
    ""type"": ""upstream"",
    ""upstream"": ""conference"",
    ""id"": ""conference"",
    ""primary"": true
  },
  ""http://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"": {
    ""original_url"": ""http://{default}/"",
    ""to"": ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"",
    ""type"": ""redirect"",
    ""id"": null,
    ""primary"": false
  },
  ""http://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/conferences"": {
    ""primary"": false,
    ""id"": null,
    ""type"": ""redirect"",
    ""to"": ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/conferences"",
    ""original_url"": ""http://{default}/conferences""
  },
  ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/speakers"": {
    ""id"": ""speaker"",
    ""primary"": false,
    ""type"": ""upstream"",
    ""upstream"": ""speaker"",
    ""original_url"": ""https://{default}/speakers"",
    ""attributes"": {}
  },
  ""https://www.master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"": {
    ""id"": null,
    ""primary"": false,
    ""original_url"": ""https://www.{default}/"",
    ""attributes"": {},
    ""type"": ""redirect"",
    ""to"": ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/""
  },
  ""http://www.master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"": {
    ""primary"": false,
    ""id"": null,
    ""to"": ""https://www.master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/"",
    ""type"": ""redirect"",
    ""original_url"": ""http://www.{default}/""
  },
  ""https://master-7rqtwti-dvtam6qrvyjwi.eu-3.platformsh.site/sessions"": {
    ""primary"": false,
    ""id"": ""session"",
    ""original_url"": ""https://{default}/sessions"",
    ""attributes"": {},
    ""upstream"": ""session"",
    ""type"": ""upstream""
  }
}";
        Environment.SetEnvironmentVariable("PLATFORM_ROUTES",
            EncodeJsonStringAsBase64(jsonString));
    }

    private static string EncodeJsonStringAsBase64(string jsonString)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
    }
}

public class AlphabeticalOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        var result = testCases.ToList();
        result.Sort((x, y) =>
            StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));
        return result;
    }
}