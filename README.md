# Betfair Exchange Games API v.1.0 

[betfair Documentation](https://developer.betfair.com/en/games-api/)

## INTEGRATE WITH THE EXCHANGE GAMES PLATFORM

* Exchange Texas Hold'em Poker
* Exchange Omaha Poker
* Exchange Blackjack
* Exchange Baccarat
* Exchange Hi Lo
* Exchange Card Racing



## First
  Add username and password 

        public static class UserInfo
        {
            public static string username => "User";

            public static string password => "PW";
        }

## Second

        static void Main(string[] args)
        {
            //min bet 1€
            Balance.Skate = 1;

            BetfairGameApi.Connect.BetfairCon Bet = new BetfairGameApi.Connect.BetfairCon(UserInfo.username, UserInfo.password, "maryBrown@AOL.com.myGames.4.0");


            var Ping = Bet.GetPing().Result.Result;
            var t = Bet.GetAccountStatement().Result;
            Console.WriteLine("Iniciar\n\n");
            Console.WriteLine(Ping.credentialCheck);
      


            var r = Bet.GetAccountBalance();

            var ChannelLanding = Bet.GetChannelLanding().Result.Result;

            var CahnnelPoker = ChannelLanding.channel.Where(x => x.gameType == "POKER").ToList();

            MarketPeriodic marketListener = MarketPeriodic.Create(Bet, CahnnelPoker.Count);
            PokerBot Pb;


            foreach (var item in CahnnelPoker)
            {
                Pb = new PokerBot(marketListener,item.id);
                Pb.Startbot();                 
            }

            Console.ReadKey();

        }
