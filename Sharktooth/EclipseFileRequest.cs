using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharktooth
{
    public class EclipseFileRequest : FileRequest
    {
        private List<TokenPair> _tokens;

        public EclipseFileRequest(string host, string uri, TokenPair pair) : base(host, uri, "")
        {
            _tokens = new List<TokenPair>();
            if (pair != null) _tokens.Add(pair);
        }

        public override string Query => (_tokens?.Count <= 0) ? "" : $"eclipseps3={_tokens.Last().TimeStampTicks}_{_tokens.Last().Token}";

        public List<TokenPair> Tokens => _tokens;
    }
}
