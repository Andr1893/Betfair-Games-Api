
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi
{
    public class XmlResponse<T>
    {
        public T Result { get; set; }
        public DateTime LastByte { get; set; }
        public DateTime RequestStart { get; set; }
        public long LatencyMS { get; set; }
        public bool HasError { get; set; }
        public Exception Error { get; set; }
    }
}
