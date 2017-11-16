using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharktooth
{
    public class TokenPair
    {
        public TokenPair()
        {
            TimeStamp = DateTime.Now;
            Token = "";
        }

        public TokenPair(DateTime time, string token)
        {
            TimeStamp = time;
            Token = token;
        }

        /// <summary>
        /// Request time
        /// </summary>
        public DateTime TimeStamp { get; set; }

        public string TimeStampTicks => TimeStamp.Ticks.ToString();

        /// <summary>
        /// 128-bit hash (Hex)
        /// </summary>
        public string Token { get; set; }

        public override string ToString() => $"{this.TimeStamp} {this.Token}";
    }
}
