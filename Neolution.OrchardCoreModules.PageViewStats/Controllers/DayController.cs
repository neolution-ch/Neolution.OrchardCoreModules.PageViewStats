namespace Neolution.OrchardCoreModules.PageViewStats.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neolution.OrchardCoreModules.PageViewStats.Queries;
using Neolution.OrchardCoreModules.PageViewStats.ViewModels;
using OrchardCore.Admin;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;

[Admin]
public class DayController : Controller
{
    private readonly IAuthorizationService authorizationService;
    private readonly IContentManager contentManager;
    private readonly ISender sender;
    
    public DayController(IAuthorizationService authorizationService, IContentManager contentManager, ISender sender)
    {
        this.authorizationService = authorizationService;
        this.contentManager = contentManager;
        this.sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult> Index(DateTime day)
    {
        if (!await this.authorizationService.AuthorizeAsync(User, PageViewStatsPermissions.ViewPageViewStats))
        {
            return Forbid();
        }

        var date = DateOnly.FromDateTime(day);
        
        var pageViews = await this.sender.Send(new GetPageViewsPerDayQuery(date));

        var contentItems = await this.contentManager.GetAsync(pageViews.Select(x => x.ContentItemId));

        var viewModel = new DayIndexViewModel
        {
            Date = date,
            Items = new List<PageViewsPerContentItem>()
        };

        foreach (var contentItem in contentItems)
        {
            var pageView = pageViews.FirstOrDefault(x => x.ContentItemId == contentItem.ContentItemId);
            if (pageView == null)
            {
                continue;
            }

            var item = new PageViewsPerContentItem
            {
                ContentItemId = contentItem.ContentItemId,
                DisplayText = contentItem.DisplayText,
                Route = contentItem.Get<AutoroutePart>("AutoroutePart"),
                TotalViews = pageView.TotalViews,
                BotViews = pageView.BotViews,
                UniqueVisitors = pageView.UniqueVisitors,
            };

            viewModel.Items.Add(item);
        }
        
        return View(viewModel);
    }
}