using Microsoft.Web.Administration;

namespace Extensions;

public static partial class ServerManagerSiteExtensions
{
    /// <summary>
    /// Gets a specific site by name or adds a new site.
    /// </summary>
    /// <param name="serverManager"></param>
    /// <param name="siteName"></param>
    /// <param name="protocol"></param>
    /// <param name="bindingInformation"></param>
    /// <param name="physicalPath"></param>
    /// <returns></returns>
    public static Site GetOrAddSite(this ServerManager serverManager, string siteName, string protocol, string bindingInformation, string physicalPath)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(siteName);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(protocol);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(bindingInformation);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(physicalPath);
        
        ThrowIfContainsInvalidCharacters(siteName, typeof(Site), SiteCollection.InvalidSiteNameCharacters());
        
        var site = serverManager.Sites[siteName];

        if (site != null)
        {
            // this is opinionated, for my use case.
            site.Bindings.Clear();
            site.Bindings.Add(bindingInformation, protocol);
            
            var physicalPathInfo = new DirectoryInfo(physicalPath);
            if (!physicalPathInfo.Exists)
            {
                // might need to consider 
                // adding some kind of ACL 
                // granting access to IUSR
                // for anonymous access.
                // AuthenticatedUser for WindowsAuth
                physicalPathInfo.Create();
            }
            
            var rootApplication = site.Applications.FirstOrDefault(a => a.Path == "/");
            var virtualDirectoryRoot = rootApplication?.VirtualDirectories.FirstOrDefault(vd => vd.Path == "/");

            // something has gone wrong with the virtual directories,
            // just scrap the site and start fresh.
            if (virtualDirectoryRoot == null)
            {
                site.Delete();
                serverManager.CommitChanges();
                return GetOrAddSite(serverManager, siteName, protocol, bindingInformation, physicalPath);
            }
            
            virtualDirectoryRoot.PhysicalPath = physicalPath;

        }

        return site ?? serverManager.Sites.Add(siteName, protocol, bindingInformation, physicalPath);
    }
    
    public static void SetPhysicalPath(this Site site, string path)
    {
        site.Applications.First().VirtualDirectories.First().PhysicalPath = path;
    }
    
    public static void BindToPort(this Site site, int port)
    {
        site.Bindings.First().BindingInformation = $"*:{port}:";
    }
}
