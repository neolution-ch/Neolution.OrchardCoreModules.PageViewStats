namespace Neolution.OrchardCoreModules.PageViewStats.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using Neolution.OrchardCoreModules.PageViewStats.Services;
using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
using OrchardCore.Admin;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using OrchardCore.Settings;
using YesSql;

[Admin]
public class DashboardController : Controller
{
    private readonly ISession session;
    private readonly IDbConnectionAccessor dbConnectionAccessor;
    private readonly IAuthorizationService authorizationService;
    private readonly IContentManager contentManager;
    private readonly ISiteService siteService;
    private readonly IPageViewsRepository repository;

    private ISqlDialect dialect;
    private string table;

    public DashboardController(ISession session, IDbConnectionAccessor dbConnectionAccessor, IAuthorizationService authorizationService, IContentManager contentManager, ISiteService siteService, IPageViewsRepository repository)
    {
        this.session = session;
        this.dbConnectionAccessor = dbConnectionAccessor;
        this.authorizationService = authorizationService;
        this.contentManager = contentManager;
        this.siteService = siteService;
        this.repository = repository;

        SetupDatabaseFields();
    }

    [HttpGet]
    public async Task<ActionResult> Index(int history)
    {
        if (!await this.authorizationService.AuthorizeAsync(User, PageViewStatsPermissions.ViewPageViewStats))
        {
            return Forbid();
        }

        var settings = await this.siteService.GetSiteSettingsAsync();
        var tz = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var today = DateOnly.FromDateTime(now);

        IList<DailyArchive> pageViews;
        if (history == 0)
        {
            pageViews = await this.repository.LoadAllPageViewsAsync();
        }
        else
        {
            if (history > 30 || history <= 1)
            {
                // Limit history to maximum number of 30 days
                history = 30;
            }

            pageViews = await this.repository.LoadPageViewsAsync(today.AddDays(history * -1), today);
        }

        var contentItems = await this.contentManager.GetAsync(pageViews.SelectMany(x => x.PageViews).Select(x => x.ContentItemId));
        var contentItemList = contentItems.ToList();
        var pageViewsPerSite = new List<PageViewsPerContentItem>();
        foreach (var groupedItems in pageViews.SelectMany(x => x.PageViews).GroupBy(x => x.ContentItemId))
        {
            var pageView = contentItemList.FirstOrDefault(x => x.ContentItemId == groupedItems.Key);
            if (pageView == null)
            {
                continue;
            }

            var item = new PageViewsPerContentItem
            {
                ContentItemId = groupedItems.Key,
                DisplayText = pageView.DisplayText,
                Route = pageView.Get<AutoroutePart>("AutoroutePart"),
                BotViews = groupedItems.Sum(g => g.BotViews),
                UniqueVisitors = groupedItems.Sum(g => g.UniqueVisitors),
                TotalViews = groupedItems.Sum(g => g.TotalViews),
            };

            pageViewsPerSite.Add(item);
        }


        var viewModel = new DashboardViewModel
        {
            History = history,
            PageViewsByDay = pageViews,
            PageViewsByContentItem = pageViewsPerSite
        };

        return View(viewModel);
    }

    private void SetupDatabaseFields()
    {
        dialect = session.Store.Configuration.SqlDialect;

        var tablePrefix = session.Store.Configuration.TablePrefix;
        if (!string.IsNullOrEmpty(tablePrefix))
        {
            tablePrefix += '_';
        }

        table = $"{tablePrefix}{PageView.TableName}";
    }
}