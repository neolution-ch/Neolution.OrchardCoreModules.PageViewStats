namespace Neolution.OrchardCoreModules.PageViewStats.Drivers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Neolution.OrchardCoreModules.PageViewStats.Settings;
using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

public class PageViewStatsSettingsDisplayDriver : SectionDisplayDriver<ISite, PageViewStatsSettings>
{
    public const string GroupId = "pageViewStats";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public PageViewStatsSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(PageViewStatsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, PageViewStatsPermissions.ManagePageViewStats))
        {
            return null;
        }

        return Initialize<PageViewStatsSettingsViewModel>("PageViewStatsSettings_Edit", m =>
            {
                m.IsEnabled = settings.IsEnabled;
                m.CollectUserIp = settings.CollectUserIp;
                m.CollectUserAgentString = settings.CollectUserAgentString;
            })
            .Location("Content:1").OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(PageViewStatsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, PageViewStatsPermissions.ManagePageViewStats))
        {
            return null;
        }

        if (context.GroupId == GroupId)
        {
            var model = new PageViewStatsSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.IsEnabled = model.IsEnabled;
            settings.CollectUserIp = model.CollectUserIp;
            settings.CollectUserAgentString = model.CollectUserAgentString;
        }

        return await EditAsync(settings, context);
    }
}