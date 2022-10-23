namespace Neolution.OrchardCoreModules.PageViewStats;

using System;
using Neolution.OrchardCoreModules.PageViewStats.Models;
using OrchardCore.Data.Migration;

public class Migrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateTable(PageView.TableName, table => table
            .Column<Guid>(nameof(PageView.Id), col => col.PrimaryKey())
            .Column<string>(nameof(PageView.ContentItemId), c => c.WithLength(26))
            .Column<string>(nameof(PageView.RequestIpAddress), c => c.WithLength(45))
            .Column<string>(nameof(PageView.RequestUserAgentString))
            .Column<bool?>(nameof(PageView.RequestUserAgentIsRobot))
            .Column<DateTimeOffset>(nameof(PageView.CreatedUtc), col => col.NotNull())
        );

        SchemaBuilder.AlterTable(PageView.TableName, table => table
            .CreateIndex($"IDX_{PageView.TableName}_{nameof(PageView.ContentItemId)}", nameof(PageView.ContentItemId))
        );
            
        return 1;
    }

    public int UpdateFrom1()
    {
        SchemaBuilder.AlterTable(PageView.TableName, table => table
            .AddColumn<string>(nameof(PageView.RequestReferer))
        );

        return 2;
    }
}