namespace Neolution.OrchardCoreModules.PageViewStats.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
    using OrchardCore.Data;
    using OrchardCore.Environment.Shell;
    using OrchardCore.Indexing;
    using YesSql;

    public class PageViewsRepository
    {
        private readonly ShellSettings shellSettings;
        private readonly IStore store;
        private readonly IDbConnectionAccessor dbConnectionAccessor;
        private readonly ILogger<PageViewsRepository> logger;
        private readonly string tablePrefix;

        public PageViewsRepository(ShellSettings shellSettings, IStore store, IDbConnectionAccessor dbConnectionAccessor, ILogger<PageViewsRepository> logger)
        {
            this.shellSettings = shellSettings;
            this.store = store;
            this.dbConnectionAccessor = dbConnectionAccessor;
            this.logger = logger;

            tablePrefix = shellSettings["TablePrefix"];

            if (!string.IsNullOrEmpty(tablePrefix))
            {
                tablePrefix += '_';
            }
        }

        /*
        public async DateGroupedPageView LoadPageViewsAsync(DateOnly date)
        {
            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            try
            {
                var dialect = store.Configuration.SqlDialect;
                var sqlBuilder = dialect.CreateBuilder(tablePrefix);

                sqlBuilder.Select();
                sqlBuilder.Table(PageView.TableName);
                sqlBuilder.Selector("*");
                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Id")} > @Id");
                sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading page views");
                throw;
            }
        }

        public async DateGroupedPageView LoadPageViewsAsync(int year, int month)
        {
            
        }

        public async List<DateOnly> LoadAllDaysWithDataAsync()
        {
            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();

            try
            {
                var dialect = store.Configuration.SqlDialect;
                var sqlBuilder = dialect.CreateBuilder(tablePrefix);

                sqlBuilder.Select();
                sqlBuilder.Table(PageView.TableName);
                sqlBuilder.Selector("*");
                sqlBuilder.WhereAnd($"{dialect.QuoteForColumnName("Id")} > @Id");
                sqlBuilder.OrderBy($"{dialect.QuoteForColumnName("Id")}");

                return await connection.QueryAsync<IndexingTask>(sqlBuilder.ToSqlString(), new { Id = afterTaskId });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading page views");
                throw;
            }
        }
        */
    }
}
