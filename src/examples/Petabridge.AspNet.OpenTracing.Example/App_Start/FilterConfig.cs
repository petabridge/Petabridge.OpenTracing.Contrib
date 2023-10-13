using System.Web.Mvc;

namespace Petabridge.AspNet.OpenTracing.Example
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new OpenTracingPropagateAttribute());
            filters.Add(new HandleErrorAttribute());
        }
    }
}