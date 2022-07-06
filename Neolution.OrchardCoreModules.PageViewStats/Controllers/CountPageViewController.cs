namespace Neolution.OrchardCoreModules.PageViewStats.Controllers;

using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using Neolution.OrchardCoreModules.PageViewStats.Services;
using Neolution.OrchardCoreModules.PageViewStats.Settings;
using OrchardCore.Data;
using OrchardCore.Entities;
using OrchardCore.Settings;
using YesSql;

public class CountPageViewController : Controller
{
    /// <summary>
    /// The table for storing the page views
    /// </summary>
    private string table;

    /// <summary>
    /// The SQL dialect
    /// </summary>
    private ISqlDialect dialect;

    private readonly ISession session;
    private readonly IDbConnectionAccessor dbConnectionAccessor;
    private readonly ISiteService siteService;
    private readonly IBotDetector botDetector;

    public CountPageViewController(ISession session, IDbConnectionAccessor dbConnectionAccessor, ISiteService siteService, IBotDetector botDetector)
    {
        this.session = session;
        this.dbConnectionAccessor = dbConnectionAccessor;
        this.siteService = siteService;
        this.botDetector = botDetector;

        SetupDatabaseFields();
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    public async Task<ActionResult> IndexPost(string contentItemId)
    {
        var settings = (await siteService.GetSiteSettingsAsync()).As<PageViewStatsSettings>();

        if (!settings.IsEnabled)
        {
            // Ignore requests when page view statistics are disabled
            return Ok();
        }

        var pageView = new PageView
        {
            ContentItemId = contentItemId,
            CreatedUtc = DateTimeOffset.UtcNow,
        };
            
        if (settings.CollectUserIp)
        {
            pageView.RequestIpAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        if (settings.CollectUserAgentString)
        {
            pageView.RequestUserAgentString = this.Request.Headers.UserAgent.ToString();
            pageView.RequestUserAgentIsRobot = this.botDetector.CheckUserAgentString(pageView.RequestUserAgentString);
        }
            
        await using var connection = this.dbConnectionAccessor.CreateConnection();
            
        var insertCmd =
            $"INSERT INTO {dialect.QuoteForTableName(table)} (" +
            $"  {dialect.QuoteForColumnName(nameof(PageView.Id))}, " +
            $"  {dialect.QuoteForColumnName(nameof(PageView.CreatedUtc))}, " +
            $"  {dialect.QuoteForColumnName(nameof(PageView.ContentItemId))}, " +
            $"  {dialect.QuoteForColumnName(nameof(PageView.RequestIpAddress))}, " +
            $"  {dialect.QuoteForColumnName(nameof(PageView.RequestUserAgentString))}, " +
            $"  {dialect.QuoteForColumnName(nameof(PageView.RequestUserAgentIsRobot))}) " +
            $"VALUES (" +
            $"  @{nameof(PageView.Id)}, " +
            $"  @{nameof(PageView.CreatedUtc)}, " +
            $"  @{nameof(PageView.ContentItemId)}, " +
            $"  @{nameof(PageView.RequestIpAddress)}, " +
            $"  @{nameof(PageView.RequestUserAgentString)}, " +
            $"  @{nameof(PageView.RequestUserAgentIsRobot)});";

        await connection.ExecuteAsync(insertCmd, pageView);

        return Ok();
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