using System;

namespace Unleash.Communication.Admin.Dto
{
    public class Application
    {
        public string AppName { get; set; }
        public string[] Strategies { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Links Links { get; set; }
    }
}