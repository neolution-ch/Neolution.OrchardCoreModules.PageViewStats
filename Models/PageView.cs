namespace Neolution.OrchardCoreModules.PageViewStats.Models
{
    using System;

    public class PageView
    {
        /// <summary>
        /// The table name of the <see cref="PageView"/>.
        /// </summary>
        public static string TableName = "PageViews";

        /// <summary>
        /// The unique identifier of the <see cref="PageView"/>.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The id of the content item that is recorded by the <see cref="PageView"/> instance.
        /// </summary>
        public string ContentItemId { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        public string RequestIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the user agent string.
        /// </summary>
        public string RequestUserAgentString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user agent is likely a robot.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user agent is likely a robot; otherwise, <c>false</c>.
        /// </value>
        public bool? RequestUserAgentIsRobot { get; set; }

        /// <summary>
        /// The <see cref="DateTimeOffset"/> value the task was created.
        /// </summary>
        public DateTimeOffset CreatedUtc { get; set; }
    }
}
