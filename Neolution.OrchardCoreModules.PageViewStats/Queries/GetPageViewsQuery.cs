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

public class GetPageViewsQueryResponse
{
    public string RequestIpAddress { get; set; }
    public string RequestUserAgentString { get; set; }
}

public class GetPageViewsQuery : IRequest<IList<GetPageViewsQueryResponse>>
{
    public DateOnly Date { get; init; }

    public GetPageViewsQuery(DateOnly date)
    {
        Date = date;
    }
}

public class GetPageViewsQueryHandler : IRequestHandler<GetPageViewsQuery, IList<GetPageViewsQueryResponse>>
{
    private readonly ISession session;
    private readonly ISiteService siteService;
    private readonly IDbConnectionAccessor dbConnectionAccessor;

    public GetPageViewsQueryHandler(ISession session, ISiteService siteService, IDbConnectionAccessor dbConnectionAccessor)
    {
        this.session = session;
        this.siteService = siteService;
        this.dbConnectionAccessor = dbConnectionAccessor;
    }

    public async Task<IList<GetPageViewsQueryResponse>> Handle(GetPageViewsQuery request, CancellationToken cancellationToken)
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
            .Select(x => new 
            {
	            x.RequestIpAddress,
	            x.RequestUserAgentString,
            });

        */

        var query = "SELECT" +
                    "  [t0].[RequestIpAddress]," +
                    "  [t0].[RequestUserAgentString] " +
                    "FROM [PageViews] AS [t0]" +
                    $"WHERE ([t0].[CreatedUtc] >= '{fromDate}')" +
                    $"AND ([t0].[CreatedUtc] <= '{untilDate}')";

        return (await connection.QueryAsync<GetPageViewsQueryResponse>(query)).ToList();
    }
}