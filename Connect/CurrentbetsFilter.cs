using BetfairGameApi.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi.Connect
{
    public class CurrentbetsFilter
    {
        public betStatusEnum betResult { get;set;}
        public int ChannelID { get; set; }
    }

    public enum betStatusEnum
    {
        MATCHED,
        UNMATCHED,
        LAPSED,
        CANCELLED,
        ACTIVE,
    }
}
