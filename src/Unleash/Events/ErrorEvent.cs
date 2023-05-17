using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Unleash.Events
{
    public class ErrorEvent
    {
        public ErrorType ErrorType { get; set; }
        public Exception Error { get; set; }
        public HttpStatusCode? StatusCode { get; internal set; }
        public string Resource { get; internal set; }
    }
}
