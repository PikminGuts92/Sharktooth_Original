using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharktooth
{
    public class FileRequest
    {
        public FileRequest(string host, string uri, string query)
        {
            Host = host;
            Uri = uri;
            Query = query;
        }

        public string Host { get; } // cdn-ghtv.guitarhero.com
        public string Uri { get; }
        public virtual string Query { get; }
        public string[] Queries => Query.Split('&');

        public string FullRequest => $"http://{Host}{Uri}?{Query}";
        public override string ToString() => FullRequest;
    }
}
