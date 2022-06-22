namespace Neolution.OrchardCoreModules.PageViewStats.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Neolution.OrchardCoreModules.PageViewStats.Models;
    using OrchardCore.BackgroundTasks;
    using OrchardCore.Documents;

    [BackgroundTask(Schedule = "0 3 * * *", Description = "Generate summary documents for page view statistics.")]
    public class ArchivePageViewsBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var manager = serviceProvider.GetRequiredService<IDocumentManager<MonthlyArchive>>();
            await manager.GetOrCreateImmutableAsync();
        }
    }
}
