﻿namespace Neolution.OrchardCoreModules.PageViewStats.Settings;

public class PageViewStatsSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether page views are collected.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this page views are collected; otherwise, <c>false</c>.
    /// </value>
    public bool IsEnabled { get; set; } = true;

    public bool CollectUserIp { get; set; }

    public bool CollectUserAgentString { get; set; }

    public bool CollectRequestReferer { get; set; }

    public bool RenderInContentItem { get; set; }
}