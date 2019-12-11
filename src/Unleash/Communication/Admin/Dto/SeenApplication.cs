using System;

namespace Unleash.Communication.Admin.Dto
{
    public class SeenApplication
    {
        public string AppName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Description { get; set; }
        public string[] Strategies { get; set; }
        public Uri Url { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
    }
}