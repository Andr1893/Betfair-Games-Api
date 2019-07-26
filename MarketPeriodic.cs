using BetfairGameApi.Cards;
using BetfairGameApi.Connect;
using BetfairGameApi.TO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi
{
    public class MarketPeriodic : IDisposable
    {
        private BetfairCon _client;

        private DateTime _latestDataRequestStart = DateTime.Now;
        private DateTime _latestDataRequestFinish = DateTime.Now;

        List<cardsValue> Cards = new List<cardsValue>();

        private readonly object _lockObj = new object();

        private readonly ConcurrentDictionary<int, IObservable<MarketBook>> _markets =
            new ConcurrentDictionary<int, IObservable<MarketBook>>();

        private readonly ConcurrentDictionary<int, IObserver<MarketBook>> _observers =
            new ConcurrentDictionary<int, IObserver<MarketBook>>();

        private readonly IDisposable _polling;

        private MarketPeriodic(BetfairCon client,
            double periodInSec)
        {
            _client = client;

            _polling = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          NewThreadScheduler.Default).Subscribe(l => DoWork());
        }

        public static MarketPeriodic Create(BetfairCon client,
            double periodInSec)
        {
            return new MarketPeriodic(client, periodInSec);
        }


        public IObservable<MarketBook> SubscribeMarketBook(int marketId)
        {
            IObservable<MarketBook> market;
            if (_markets.TryGetValue(marketId, out market))
                return market;

            var observable = Observable.Create<MarketBook>(
               (IObserver<MarketBook> observer) =>
               {
                   _observers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                   {
                       IObserver<MarketBook> ob;
                       IObservable<MarketBook> o;
                       _markets.TryRemove(marketId, out o);
                       _observers.TryRemove(marketId, out ob);
                   });
               })
               .Publish()
               .RefCount();

            _markets.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);
            return observable;
        }


        private void DoWork()
        {
            string console = "";
            Console.SetCursorPosition(0, 0);
            foreach (var item in _markets.Keys.ToList())
            {
                var book = _client.GetChannelSnapshot(Convert.ToInt32(item)).Result;
                var CurrentBet = ExistBets(_client, Convert.ToInt32(item));


                if (!book.HasError)
                {
                    foreach (var observer in _observers)
                        observer.Value.OnError(book.Error);
                    return;
                }

                // we may have fresher data than the response to this request
                if (book.RequestStart < _latestDataRequestStart && book.LastByte > _latestDataRequestFinish)
                    return;

                lock (_lockObj)
                {
                    _latestDataRequestStart = book.RequestStart;
                    _latestDataRequestFinish = book.LastByte;
                }

                IObserver<MarketBook> o;
                if (!_observers.TryGetValue(book.Result.channel.id, out o)) continue;

                // check to see if the market is finished
                if (book.Result.channel.status != channelStatusEnum.RUNNING)
                    o.OnCompleted();
                else
                    o.OnNext(new MarketBook(book.Result.channel,CurrentBet));





                console += MarketSnapConsole(new MarketBook(book.Result.channel, CurrentBet));  //MarketSnapConsole(book.Result.channel, CurrentBet) ;
            }
            Console.Clear();
            Console.WriteLine(console);
          
        }

        public void Dispose()
        {
            if (_polling != null) _polling.Dispose();
        }


     

        public string MarketSnapConsole(MarketBook marketSnap)
        {
            var timeToJump = Convert.ToDateTime(DateTime.Now);
            var timeRemainingToJump = timeToJump.Subtract(DateTime.UtcNow);

            var sb = new StringBuilder()
                        .AppendFormat("{0} - {1}\n", marketSnap.GetGameName, marketSnap.GetGameId)
                        .AppendFormat("back: {0} | lay:{1}  ", marketSnap.GetMarketEfficiencyBack(), marketSnap.GetMarketEfficiencyLay())
                        .AppendFormat("Status: {0}", marketSnap.GetChannelStatus)
                        .AppendFormat(" | Round: {0}", marketSnap.GetRound)
                        .AppendFormat(" | Runners: {0}", marketSnap.GetSelection().Count)
                        .AppendFormat(" | TradedVolume: {0}", marketSnap.GetSelection().Sum(x => x.amountMatched).ToString().PadRight(50));
            sb.AppendLine();

            var Player = marketSnap.GetOdds;
            var comunity = marketSnap.GetCardsCommunityCards.PlayerPocket();

            sb.AppendFormat("Status: {2}\nCommunity Cards: {0} | Time Next Round: {1} \n",
                comunity,
                marketSnap.GetGameChannel.bettingWindowPercentageComplete.ToString().PadRight(50),
                marketSnap.GetMarket.status.ToString().PadRight(50));
            sb.AppendLine();

            if (marketSnap.GetSelection() != null && marketSnap.GetSelection().Count > 0)
            {
                foreach (var runner in marketSnap.GetOdds)
                {

                   
                    //var _Current = marketSnap.GetBet(runner.id).FirstOrDefault();
                    //var Stake = _Current != null ? _Current.size : 0;
                    //var profit = _Current != null ? ((_Current.price - 1) * Stake)- marketSnap.GetBet(runner.id).Where(x=>x.selectionReference.selectionId != runner.id).Sum(x=>x.size) : 0;

                    //sb.AppendLine(string.Format("{0} Status: {1} Cards: {2} \n-> [{3}] {4} : {5} : {6} [{7}] | Stake: {8} | odd:{9}",
                    //    runner.name,
                    //    runner.status.ToString().PadLeft(7),
                    //    Player.Where(x => x.PlayerName == runner.name).FirstOrDefault().Hands.Count > 0 ? Player.Where(x => x.PlayerName == runner.name).FirstOrDefault().PlayerPocket().PadLeft(8) : "",
                    //    runner.bestAvailableToBackPrices.Sum(a => a.amountUnmatched).ToString("0").PadLeft(7),
                    //    runner.bestAvailableToBackPrices.Count > 0 ? runner.bestAvailableToBackPrices[0].Value.ToString("0.00").PadLeft(6) : "  0.00",
                    //    Math.Round(profit, 2).ToString(),
                    //    runner.bestAvailableToLayPrices.Count > 0 ? runner.bestAvailableToLayPrices[0].Value.ToString("0.00").PadLeft(6) : "  0.00",
                    //    runner.bestAvailableToLayPrices.Sum(a => a.amountUnmatched).ToString("0").PadLeft(7),
                    //    Stake.ToString().PadRight(20),
                    //    Player.Where(x=>x.PlayerId == runner.id)?.FirstOrDefault().AddPip()));

                    sb.Append(runner._ToString());
                    sb.AppendLine();
                    sb.AppendLine();

                }
            }

            return sb.Append("\n\n\n").ToString();
        }

        public double GetMarketEfficiency(IEnumerable<double> odds)
        {
            double total = odds.Sum(c => 1.0 / c);
            return Math.Round(total*100,2);
        }

        private List<betSnapshotTypeBetSnapshotItem> ExistBets(BetfairCon Bet,int MarketID)
        {
            var Currentbets = Bet.GetCurrentbets(new CurrentbetsFilter() { betResult = betStatusEnum.ACTIVE, ChannelID = MarketID }).Result;

            if (Currentbets.HasError)
            {
                if (Currentbets.Result.total > 0)
                {
                    return Currentbets.Result.betSnapshotItem;
                }
            }
            else
            {
                return new List<betSnapshotTypeBetSnapshotItem>();
            }

            return new List<betSnapshotTypeBetSnapshotItem>();

        }

    }
}
