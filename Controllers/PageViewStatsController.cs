namespace Neolution.OrchardCoreModules.PageViewStats.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using YesSql;

[Admin]
public class PageViewStatsController : Controller
{
    private readonly ISession session;
    private readonly IDbConnectionAccessor dbConnectionAccessor;
    private readonly IAuthorizationService authorizationService;
    private readonly IContentManager contentManager;

    private ISqlDialect dialect;
    private string table;

    public PageViewStatsController(ISession session, IDbConnectionAccessor dbConnectionAccessor, IAuthorizationService authorizationService, IContentManager contentManager)
    {
        this.session = session;
        this.dbConnectionAccessor = dbConnectionAccessor;
        this.authorizationService = authorizationService;
        this.contentManager = contentManager;

        SetupDatabaseFields();
    }

    [HttpGet]
    public async Task<ActionResult> Index(int history)
    {
        if (!await this.authorizationService.AuthorizeAsync(User, PageViewStatsPermissions.ViewPageViewStats))
        {
            return Forbid();
        }

        if (history > 30 || history <= 0)
        {
            // Limit history to maximum number of 30 days
            history = 30;
        }

        await using var connection = this.dbConnectionAccessor.CreateConnection();

        /*var insertCmd =
            $"INSERT INTO {dialect.QuoteForTableName(table)} ({dialect.QuoteForColumnName(nameof(Abstractions.PageView.CreatedUtc))}, {dialect.QuoteForColumnName(nameof(Abstractions.PageView.ContentItemId))}) " +
            $"VALUES (@{nameof(Abstractions.PageView.CreatedUtc)}, @{nameof(Abstractions.PageView.ContentItemId)});";*/

        var viewModel = new PageViewStatsViewModel { History = history };

        /*var selectCmd = $"SELECT COUNT(*) AS {dialect.QuoteForColumnName(nameof(GroupedPageView.Amount))}, {dialect.QuoteForColumnName(nameof(GroupedPageView.ContentItemId))} " +
                        $"FROM {dialect.QuoteForTableName(table)} " +
                        $"GROUP BY {dialect.QuoteForColumnName(nameof(GroupedPageView.ContentItemId))} " +
                        $"ORDER BY {dialect.QuoteForColumnName(nameof(GroupedPageView.Amount))} DESC";*/

        var selectCmd = $"SELECT " +
                    $"[t2].[value2] AS {dialect.QuoteForColumnName(nameof(DateGroupedPageView.Date))}, " +
                    $"[t2].[value] AS {dialect.QuoteForColumnName(nameof(DateGroupedPageView.Views))}, " +
                    $"(SELECT COUNT(*) FROM {PageView.TableName} AS [t3] WHERE ([t3].{nameof(PageView.RequestUserAgentIsRobot)} = 1) AND ((([t2].[value2] IS NULL) AND (CONVERT(DATE, [t3].{nameof(PageView.CreatedUtc)}) IS NULL)) OR (([t2].[value2] IS NOT NULL) AND (CONVERT(DATE, [t3].{nameof(PageView.CreatedUtc)}) IS NOT NULL) AND ((([t2].[value2] IS NULL) AND (CONVERT(DATE, [t3].{nameof(PageView.CreatedUtc)}) IS NULL)) OR (([t2].[value2] IS NOT NULL) AND (CONVERT(DATE, [t3].{nameof(PageView.CreatedUtc)}) IS NOT NULL) AND ([t2].[value2] = CONVERT(DATE, [t3].{nameof(PageView.CreatedUtc)}))))))) AS {dialect.QuoteForColumnName(nameof(DateGroupedPageView.BotViews))}" +
                    $"FROM (" +
                    $"SELECT COUNT(*) AS [value], [t1].[value] AS [value2] " +
                    $"FROM (SELECT CONVERT(DATE, [t0].{nameof(PageView.CreatedUtc)}) AS [value] FROM {PageView.TableName} AS [t0]) AS [t1] " +
                    $"GROUP BY [t1].[value] " +
                    $") AS [t2]";

        var model = (await connection.QueryAsync<DateGroupedPageView>(selectCmd)).ToList();

        var selectUniqueVisitors = $"SELECT DISTINCT " +
                                   $"[t1].{nameof(PageView.RequestIpAddress)}, [t1].{nameof(PageView.RequestUserAgentString)}, [t1].[value] AS {nameof(UniqueVisitorPageView.Date)} " +
                                   $"FROM (" +
                                   $"SELECT [t0].{nameof(PageView.RequestIpAddress)}, [t0].{nameof(PageView.RequestUserAgentString)}, CONVERT(DATE, [t0].{nameof(PageView.CreatedUtc)}) AS[value], [t0].{nameof(PageView.RequestUserAgentIsRobot)} " +
                                   $"FROM {PageView.TableName} AS[t0]) AS[t1] " +
                                   $"WHERE NOT([t1].{nameof(PageView.RequestUserAgentIsRobot)} = 1)";

        var uniqueVisitors = (await connection.QueryAsync<UniqueVisitorPageView>(selectUniqueVisitors)).ToList();

        viewModel.DateGroupedPageViews = new List<PageViewsPerDate>();
        foreach (var groupedPageViews in model)
        {
            var date = DateOnly.FromDateTime(groupedPageViews.Date.UtcDateTime);
            var uniqueVisitorsCount = uniqueVisitors.Count(x => DateOnly.FromDateTime(x.Date.UtcDateTime) == date);

            var item = new PageViewsPerDate
            {
                Date = date,
                Views = groupedPageViews.Views,
                BotViews = groupedPageViews.BotViews,
                UniqueVisitors = uniqueVisitorsCount
            };

            viewModel.DateGroupedPageViews.Add(item);
        }

        // Filter entries
        var latestDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(history * -1);
        viewModel.DateGroupedPageViews = viewModel.DateGroupedPageViews.Where(x => x.Date >= latestDate).OrderByDescending(x => x.Date).ToList();
        
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