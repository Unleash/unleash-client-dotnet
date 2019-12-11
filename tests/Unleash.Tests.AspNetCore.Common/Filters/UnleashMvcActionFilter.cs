using Microsoft.AspNetCore.Mvc.Filters;

namespace Unleash.Tests.DotNetCore.AspNetCore.Mvc
{
    public class UnleashMvcActionFilter : IActionFilter
    {
        private IUnleashContextProvider ContextProvider { get; }

        public UnleashMvcActionFilter(IUnleashContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            ContextProvider.Context.Properties["ActionDisplayName"] = context.ActionDescriptor.DisplayName;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
