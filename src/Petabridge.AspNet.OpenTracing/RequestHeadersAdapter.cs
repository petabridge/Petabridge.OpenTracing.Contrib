using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using OpenTracing;
using OpenTracing.Propagation;

namespace Petabridge.AspNet.OpenTracing
{
    /// <summary>
    /// Responsible for extracting and injecting <see cref="ISpanContext"/> from HTTP Headers.
    /// </summary>
    internal sealed class RequestHeadersAdapter: ITextMap
    {
        private readonly NameValueCollection _headers;

        public RequestHeadersAdapter(NameValueCollection headers = null)
        {
            _headers = headers ?? new NameValueCollection();
        }

        public void Set(string key, string value)
        {
            _headers.Set(key, value);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _headers.AllKeys
                .Distinct()
                .Select(key => new KeyValuePair<string, string>(key, _headers.Get(key)))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}