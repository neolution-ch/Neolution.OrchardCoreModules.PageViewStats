namespace Neolution.OrchardCoreModules.PageViewStats.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using OrchardCore.Data;
using OrchardCore.Settings;
using YesSql;

public class GetPageViewsPerDayQueryResponse
{
    public string ContentItemId { get; set; }
    public int TotalViews { get; set; }
    public int BotViews { get; set; }
    public int UniqueVisitors { get; set; }
}

public class GetPageViewsPerDayQuery : IRequest<IList<GetPageViewsPerDayQueryResponse>>
{
    public DateOnly Date { get; init; }

    public GetPageViewsPerDayQuery(DateOnly date)
    {
        Date = date;
    }
}

public class PageViewsPerDayQueryHandler : IRequestHandler<GetPageViewsPerDayQuery, IList<GetPageViewsPerDayQueryResponse>>
{
    private readonly ISession session;
    private readonly ISiteService siteService;
    private readonly IDbConnectionAccessor dbConnectionAccessor;

    public PageViewsPerDayQueryHandler(ISession session, ISiteService siteService, IDbConnectionAccessor dbConnectionAccessor)
    {
        this.session = session;
        this.siteService = siteService;
        this.dbConnectionAccessor = dbConnectionAccessor;
    }

    public async Task<IList<GetPageViewsPerDayQueryResponse>> Handle(GetPageViewsPerDayQuery request, CancellationToken cancellationToken)
    {
        var dialect = session.Store.Configuration.SqlDialect;

        var tablePrefix = session.Store.Configuration.TablePrefix;
        if (!string.IsNullOrEmpty(tablePrefix))
        {
            tablePrefix += '_';
        }

        var table = $"{tablePrefix}{PageView.TableName}";

        var settings = await this.siteService.GetSiteSettingsAsync();

        var utcDay = UtcDay.Create(request.Date, settings.TimeZoneId);

        await using var connection = this.dbConnectionAccessor.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var fromDate = utcDay.From.ToString("O");
        var untilDate = utcDay.Until.ToString("O");

        /* LINQ2SQL:

         PageViews
            .Where(x => x.CreatedUtc >= utcStart && x.CreatedUtc <= utcEnd)
            .GroupBy(x => x.ContentItemId)
            .Select(x => new 
            { 
                ContentItemId = x.Key, 
                TotalViews = x.Count(),
                BotViews = x.Count(x => x.RequestUserAgentIsRobot == true),
                UniqueVisitors = x.Where(x => x.RequestUserAgentIsRobot != true).Select(x => new {x.RequestIpAddress, x.RequestUserAgentString}).Distinct().Count()
            });

        */

        var query = "SELECT" +
                    "  [t1].[ContentItemId]," +
                    "  [t1].[value] AS [TotalViews]," +
                    "  (SELECT" +
                    "    COUNT(*)" +
                    "  FROM [PageViews] AS [t2]" +
                    "  WHERE ([t2].[RequestUserAgentIsRobot] = 1)" +
                    "  AND ((([t1].[ContentItemId] IS NULL)" +
                    "  AND ([t2].[ContentItemId] IS NULL))" +
                    "  OR (([t1].[ContentItemId] IS NOT NULL)" +
                    "  AND ([t2].[ContentItemId] IS NOT NULL)" +
                    "  AND ([t1].[ContentItemId] = [t2].[ContentItemId])))" +
                    $"  AND ([t2].[CreatedUtc] >= '{fromDate}')" +
                    $"  AND ([t2].[CreatedUtc] <= '{untilDate}'))" +
                    "  AS [BotViews]," +
                    "  (SELECT" +
                    "    COUNT(*)" +
                    "  FROM (SELECT DISTINCT" +
                    "    [t3].[RequestIpAddress]," +
                    "    [t3].[RequestUserAgentString]" +
                    "  FROM [PageViews] AS [t3]" +
                    "  WHERE (NOT ([t3].[RequestUserAgentIsRobot] = 1))" +
                    "  AND ((([t1].[ContentItemId] IS NULL)" +
                    "  AND ([t3].[ContentItemId] IS NULL))" +
                    "  OR (([t1].[ContentItemId] IS NOT NULL)" +
                    "  AND ([t3].[ContentItemId] IS NOT NULL)" +
                    "  AND ([t1].[ContentItemId] = [t3].[ContentItemId])))" +
                    $"  AND ([t3].[CreatedUtc] >= '{fromDate}')" +
                    $"  AND ([t3].[CreatedUtc] <= '{untilDate}')) AS [t4])" +
                    "  AS [UniqueVisitors]" +
                    "FROM (SELECT" +
                    "  COUNT(*) AS [value]," +
                    "  [t0].[ContentItemId]" +
                    "FROM [PageViews] AS [t0]" +
                    $"WHERE ([t0].[CreatedUtc] >= '{fromDate}')" +
                    $"AND ([t0].[CreatedUtc] <= '{untilDate}')" +
                    "GROUP BY [t0].[ContentItemId]) AS [t1]";

        return (await connection.QueryAsync<GetPageViewsPerDayQueryResponse>(query)).ToList();
    }
}