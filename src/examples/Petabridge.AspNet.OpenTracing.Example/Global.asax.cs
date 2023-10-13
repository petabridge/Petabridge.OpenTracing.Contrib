using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using OpenTracing.Contrib.LocalTracers;
using OpenTracing.Contrib.LocalTracers.Config.Builder;
using OpenTracing.Contrib.LocalTracers.Config.Console;
using OpenTracing.Contrib.LocalTracers.Console;
using OpenTracing.Mock;
using OpenTracing.Util;

namespace Petabridge.AspNet.OpenTracing.Example
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var builder = TracingConfigurationBuilder.Instance
                .WithConsoleTracing(
                    "[{date:h:mm:ss tt}] {spanId}{spanIdFloatPadding} | {logCategory}{logCategoryPadding} | {outputData}",
                    settings => settings
                        .WithColorMode(ColorMode.BasedOnCategory)
                        .WithColorsForTheBasedOnCategoryColorMode(
                            ConsoleColor.Green,
                            ConsoleColor.Red,
                            ConsoleColor.Magenta,
                            ConsoleColor.Blue)
                        .WithOutputSpanNameOnCategory(
                            activated: true,
                            finished: true)
                        .WithOutputDurationOnFinished(true)
                        .WithDataSerialization(
                            SetTagDataSerialization.Simple,
                            LogDataSerialization.Simple));
            var tracer = new MockTracer()
                .Decorate(ColoredConsoleTracerDecorationFactory.Create(builder.BuildConsoleConfiguration()));
            GlobalTracer.Register(tracer);
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}