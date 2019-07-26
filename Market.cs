using BetfairGameApi.Connect;
using BetfairGameApi.TO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetfairGameApi
{
    public class Market
    {
        private static Market listener = null;
        private int connectionCount;
        private BetfairCon client;
        private static DateTime lastRequestStart;

        private static DateTime latestDataRequestStart = DateTime.Now;
        private static DateTime latestDataRequestFinish = DateTime.Now;

        private static object lockObj = new object();

        private ConcurrentDictionary<string, IObservable<channelSnapshotChannel>> markets =
            new ConcurrentDictionary<string, IObservable<channelSnapshotChannel>>();

        private ConcurrentDictionary<string, IObserver<channelSnapshotChannel>> observers =
            new ConcurrentDictionary<string, IObserver<channelSnapshotChannel>>();

        private Market(BetfairCon client,
            int connectionCount)
        {
            this.client = client;
            this.connectionCount = connectionCount;
            Task.Run(()=> PollMarketBooks());

        }

        public static Market Create(BetfairCon client,
            int connectionCount)
        {
            if (listener == null)
                listener = new Market(client,connectionCount);

            return listener;
        }

        public IObservable<channelSnapshotChannelGame> SubscribeRunner(string marketId)
        {
            var marketTicks = SubscribeMarketBook(marketId);

            var observable = Observable.Create<channelSnapshotChannelGame>(
              (IObserver<channelSnapshotChannelGame> observer) =>
              {
                  var subscription = marketTicks.Subscribe(tick =>
                  {

                      observer.OnNext(tick.game);
                  });

                  return Disposable.Create(() => subscription.Dispose());
              })
              .Publish()
              .RefCount();

            return observable;
        }

        public IObservable<channelSnapshotChannel> SubscribeMarketBook(string marketId)
        {
            IObservable<channelSnapshotChannel> market;
            if (markets.TryGetValue(marketId, out market))
                return market;

            var observable = Observable.Create<channelSnapshotChannel>(
               (IObserver<channelSnapshotChannel> observer) =>
               {
                   observers.AddOrUpdate(marketId, observer, (key, existingVal) => existingVal);
                   return Disposable.Create(() =>
                   {
                       IObserver<channelSnapshotChannel> ob;
                       IObservable<channelSnapshotChannel> o;
                       markets.TryRemove(marketId, out o);
                       observers.TryRemove(marketId, out ob);
                   });
               })
               .Publish()
               .RefCount();

            markets.AddOrUpdate(marketId, observable, (key, existingVal) => existingVal);
            return observable;
        }

        public IObservable<channelSnapshotChannelGameMarkets> SubscribeMarketSnapshot(string marketId)
        {
            var marketTicks = SubscribeMarketBook(marketId);

            var observable = Observable.Create<channelSnapshotChannelGameMarkets>(
              (IObserver<channelSnapshotChannelGameMarkets> observer) =>
              {
                  var subscription = marketTicks.Subscribe(tick =>
                  {

                      observer.OnNext(tick.game.markets);
                  });

                  return Disposable.Create(() => subscription.Dispose());
              })
              .Publish()
              .RefCount();

            return observable;
        }


        // TODO:// replace this with the Rx scheduler 
        private void PollMarketBooks()
        {
            for (int i = 0; i < connectionCount; i++)
            {
                Task.Run(() =>
                {
                while (true)
                {
                    if (markets.Count > 0)
                    {
                        // TODO:// look at spinwait or signalling instead of this
                        while (connectionCount > 1 && DateTime.Now.Subtract(lastRequestStart).TotalMilliseconds < (5000 / connectionCount))
                        {
                            int waitMs = (1000 / connectionCount) - (int)DateTime.Now.Subtract(lastRequestStart).TotalMilliseconds;
                            Thread.Sleep(waitMs > 0 ? waitMs : 0);
                        }

                        lock (lockObj)
                            lastRequestStart = DateTime.Now;




                        List<channelSnapshotChannel> book = new List<channelSnapshotChannel>();


                            foreach (var item in markets.Keys)
                            {
                                book.Add(client.GetChannelSnapshot(Convert.ToInt32(item)).Result.Result.channel);

                            }

                            if (book.Count > 0)
                            {

                                foreach (var market in book)
                                {
                                    IObserver<channelSnapshotChannel> o;
                                    if (observers.TryGetValue(market.id.ToString(), out o))
                                    {
                                        // check to see if the market is finished
                                        if (market.status != channelStatusEnum.RUNNING)
                                            o.OnCompleted();
                                        else
                                            o.OnNext(market);
                                    }

                                }


                            }
  
                        }
                        else
                            // TODO:// will die with rx scheduler
                            Thread.Sleep(500);
                    }
                });
                Thread.Sleep(1000 / connectionCount);
            }
        }
    }
}
