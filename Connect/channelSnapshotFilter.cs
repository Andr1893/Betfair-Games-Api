using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi.Connect
{
    public class channelSnapshotFilter
    {
        public bool GameInformation { get; set; } = true;
        public bool MarketInformation { get; set; } = true;
        public bool BettingRoundInformation { get; set; } = true;
        public selectionsType SelectionsType { get; set; }
    }

    public enum selectionsType
    {
        MainBets,
        SideBets,
        CorrectPredictions
    }
}
