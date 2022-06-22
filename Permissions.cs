namespace Neolution.OrchardCoreModules.PageViewStats;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

public class Permissions : IPermissionProvider
{
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(new[] {
            PageViewStatsPermissions.ViewPageViewStats,
            PageViewStatsPermissions.ManagePageViewStats
        }.AsEnumerable());
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[] {
            new PermissionStereotype {
                Name = "Administrator",
                Permissions = new[]
                {
                    PageViewStatsPermissions.ViewPageViewStats, 
                    PageViewStatsPermissions.ManagePageViewStats
                }
            }
        };
    }
}