using BetfairGameApi.Bots;
using BetfairGameApi.Cards;
using BetfairGameApi.TO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BetfairGameApi.Bots.PokerBot;

namespace BetfairGameApi.Calc
{
    public class CalculateExchange
    {
        public int  PlayerId {get;set;}
        public double Odd { get; set; }
        public double Stake { get; set; }

        public static double CloseTrade(double OpeningOdd, double ClosingOdd, double StakeInit )
        {
            return (OpeningOdd / ClosingOdd) * StakeInit;
        }

        public static List<CalculateExchange> Dutching(List<GameHands> Hands, double StakeMin)
        {
            List<CalculateExchange> Calc = new List<CalculateExchange>();

            var OddMax = Hands.Max(x=>x.BestOddback());

            List<double> d = new List<double>();

            if (OddMax>9)
                return Calc;

            foreach (var item in Hands)
            {
                var Value = (StakeMin * (OddMax/item.BestOddback()));

                Calc.Add(new CalculateExchange()
                {
                    PlayerId = item.SelectionSnapshot.id,
                    Odd = item.BestOddback(),
                    Stake = Math.Round(Value,2),

                });

            }
            return Calc;
        }

        public static List<CalculateExchange> CloseBetDutching(List<GameHands> Hands)
        {
            List<CalculateExchange> Calc = new List<CalculateExchange>();

            var Bethands = Hands.Where(x=> x.PlayerBets.Count > 0).Max(x => x.PlayerBets.FirstOrDefault().price);

            var NoBethands = Hands.Where(x => x.PlayerBets.Count < 0).ToList();

            NoBethands.ForEach(item =>
            {
                if (item.Isbet)
                {
                    var Value = (Balance.Skate * (Bethands / item.BestOddback()));

                    Calc.Add(new CalculateExchange()
                    {
                        PlayerId = item.SelectionSnapshot.id,
                        Odd = item.BestOddback(),
                        Stake = Math.Round(Value, 2),
                    });
                }
            });

            return Calc;
        }


       
    }
}
