using Microsoft.Web.Administration;

namespace Extensions;

public static partial class ServerManagerSiteExtensions
{
    public static ApplicationPool AddOrGetApplicationPool(this ServerManager serverManager, string applicationPoolName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(applicationPoolName);
        
        ThrowIfContainsInvalidCharacters(applicationPoolName, typeof(ApplicationPool), ApplicationPoolCollection.InvalidApplicationPoolNameCharacters());

        var applicationPool = serverManager.ApplicationPools[applicationPoolName];
        return applicationPool ?? serverManager.ApplicationPools.Add(applicationPoolName);
    }

    /// <summary>
    /// Adds or updates a collection of environment variables on a specific application pool.
    /// </summary>
    public static void AddOrUpdateEnvironmentVariables(this ServerManager serverManager, string appPoolName, Dictionary<string,string> environmentVariables)
    {
        var appConfig = serverManager.GetApplicationHostConfiguration();
        var appPoolsSection = appConfig.GetSection("system.applicationHost/applicationPools");
        var appPoolsCollection = appPoolsSection.GetCollection();

        var appPool = appPoolsCollection
            .FirstOrDefault(x => (string)x.Attributes["name"].Value == appPoolName);

        if (appPool == null)
        {
            return;
        }
        
        var envVarCollection = appPool.GetCollection("environmentVariables");
        foreach (var environmentVariable in environmentVariables)
        {
            var envVar = envVarCollection
                .FirstOrDefault(e => (string)e.Attributes["name"].Value == environmentVariable.Key);
            
            if (envVar == null)
            {
                envVar = envVarCollection.CreateElement("add");
                envVar.SetAttributeValue("name", environmentVariable.Key);
                envVar.SetAttributeValue("value", environmentVariable.Value);
                envVarCollection.Add(envVar);
            }
            else
            {
                envVar.SetAttributeValue("value", environmentVariable.Value);            
            }
        }
    }
}
