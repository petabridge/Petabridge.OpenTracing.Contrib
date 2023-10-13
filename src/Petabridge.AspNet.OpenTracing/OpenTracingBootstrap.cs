using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using OpenTracing.Util;

namespace Petabridge.AspNet.OpenTracing
{
    public class OpenTracingBootstrap: IHttpModule
    {
        private static readonly Func<HttpContext, string> DefaultOperationNameFormatter = 
            context => $"HTTP {context.Request.HttpMethod} {context.Request.Path}";
        
        private ITracer _tracer;
        public ITracer Tracer
        {
            get
            {
                if (_tracer == null)
                    _tracer = GlobalTracer.Instance;
                return _tracer;
            }
            set => _tracer = value;
        }

        private Func<HttpContext, string> _operationNameFormatter;
        public Func<HttpContext, string> OperationNameFormatter
        {
            get
            {
                if (_operationNameFormatter == null)
                    _operationNameFormatter = DefaultOperationNameFormatter;
                return _operationNameFormatter;
            }
            set => _operationNameFormatter = value;
        }

        private void BeginRequestHandler(object source, EventArgs args)
        {
            var application = (HttpApplication)source;
            var context = application.Context;

            var spanBuilder = Tracer.BuildSpan(OperationNameFormatter(context));

            try
            {
                var parentContext = Tracer.Extract(BuiltinFormats.HttpHeaders,
                    new RequestHeadersAdapter(context.Request.Headers));
                spanBuilder = spanBuilder.AsChildOf(parentContext);
            }
            catch (Exception)
            {
                // ignore extractor errors
            }

            var scope = spanBuilder.WithTag(Tags.Component, "HttpIn")
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag(Tags.HttpMethod, context.Request.HttpMethod)
                .WithTag(Tags.HttpUrl, context.Request.Url.ToString())
                .StartActive();
                
            context.Items.Add(Defaults.OpenTracingContextItem, scope);
            Debug.WriteLine("Created span: {0}", scope.Span.Context.SpanId);
        }

        private static void EndRequestHandler(object source, EventArgs args)
        {
            var application = (HttpApplication)source;
            var context = application.Context;

            // don't attempt to process trace where there is none
            if (!context.Items.Contains(Defaults.OpenTracingContextItem)) 
                return;

            var scope = (IScope)context.Items[Defaults.OpenTracingContextItem];
            Debug.WriteLine("Finishing span {0}", scope.Span.Context.SpanId);
            
            scope.Span.SetTag(Tags.HttpStatus, context.Response.StatusCode);
            context.Items.Remove(Defaults.OpenTracingContextItem);
            
            // Propagate the span
            var headersDictionary = new RequestHeadersAdapter();
            GlobalTracer.Instance.Inject(scope.Span.Context, BuiltinFormats.HttpHeaders, headersDictionary);

            var contextNvc = headersDictionary.Aggregate(new NameValueCollection(), (v, item) =>
            {
                v.Add(item.Key, item.Value);
                return v;
            });
            
            context.Response.Headers.Add(contextNvc);
            
            scope.Dispose();
        }

        private static void ErrorHandler(object source, EventArgs args)
        {
            var application = (HttpApplication)source;
            var context = application.Context;

            // don't attempt to process trace where there is none
            if (!context.Items.Contains(Defaults.OpenTracingContextItem)) 
                return;

            var ex = application.Server.GetLastError();

            var scope = (IScope)context.Items[Defaults.OpenTracingContextItem];
            scope.Span.SetTag(Tags.Error, true);
            scope.Span.Log(new Dictionary<string, object>(3)
            {
                { LogFields.Event, Tags.Error.Key },
                { LogFields.ErrorKind, ex.GetType().Name },
                { LogFields.ErrorObject, ex }
            });
        }
        
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequestHandler;
            context.EndRequest += EndRequestHandler;
            context.Error += ErrorHandler;
        }

        public void Dispose()
        {
            // nothing to dispose
        }
    }
}