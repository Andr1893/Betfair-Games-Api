using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetfairGameApi.TO;
using BetfairGameApi.Connect;
using HoldemHand;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BetfairGameApi.Calc;
using BetfairGameApi.Cards;

namespace BetfairGameApi.Bots
{
    public class PokerBot
    {

        int GameId { get; set; }
        BetfairCon Bet = new BetfairCon("user", "PW", "maryBrown@AOL.com.myGames.4.0");
        MarketBook MarketBookPR;

        List<GameHands> MybetDutching = new List<GameHands>();


        int RoundPr { get; set; }
        int MarketCount { get; set; }
  
        MarketPeriodic marketListener { get; set; }


        private string[] Colors = new string[] { "c", "d", "h", "s" };
        private string[] Colorshand = new string[] { "a", "2", "3", "4", "5", "6", "7", "8", "9", "10", "j", "q", "k" };



        public PokerBot(MarketPeriodic marketListener,int GameId)
        {
            this.GameId = GameId;
            this.marketListener = marketListener;
        }

        public void Startbot()
        {


            AutoResetEvent waitHandle = new AutoResetEvent(false);
    
            var marketSubscription = marketListener.SubscribeMarketBook(GameId)
                .SubscribeOn(Scheduler.Default)
                .Subscribe(
                tick =>
                {
                    Bot(tick);
                },
                () =>
                {
                    Console.WriteLine("Run Bot: {0} | Market finished", GameId);     
                });

        }


        private void Bot(MarketBook Game)
        {
          

            if (Game.GetChannelStatus != channelStatusEnum.RUNNING)
                return;

            if (Game.GetMarketStatus != marketStatusEnum.ACTIVE || Game.GetMarketStatus == marketStatusEnum.SUSPENDED_GAME_ROUND_OVER)
                return;


            if (Game.CurrentBets.Count > 0)//se existir
            {
                var hands = Game.GetOdds;

                var HandNobet = hands.Where(x=>!x.Isbet).ToList();


                var t = (from w1 in MarketBookPR.GetOdds where HandNobet.Any(w2 => w1.BestOddback() < w2.BestOddback()) select w1.PlayerId).ToList();

                if (t.Count > 0)
                {
                    t.ForEach(item =>
                    {
                       // hands.Find(x => x.PlayerId == item).Isbet = true;
                    });

                    Game.Bet_CloseBackDutching(hands);
                }
            
                return;//se nao fechar começa de novo
            }



            if (Game.GetRound >= 4)//se round =4 e nao existe apostas começa de novo
            {
                return;
            }    


            var FindBet = Game.GetOdds.ToList();//procura pelas odds altas

            FindBet.ForEach(x=> 
            {
                if (x.DecimalOdd() < x.BestOddback())
                    x.Isbet = true;
            });

            var FindBet1 = Game.GetOdds.Where(x=>x.BestOddback()>x.DecimalOdd()).ToList();//procura pelas odds altas

            if (FindBet1.Count > 0)
            {

            }



            if (FindBet.Where(x=>x.Isbet == true).Count() > 0)//se existir 1 odd alta
            {
                Game.Bet_BackDutching(FindBet);
                MarketBookPR = Game;
            }
        }   

    }
}
