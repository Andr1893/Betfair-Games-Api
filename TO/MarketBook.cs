using BetfairGameApi.Bots;
using BetfairGameApi.Calc;
using BetfairGameApi.Cards;
using BetfairGameApi.Connect;
using HoldemHand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairGameApi.TO
{
    public class MarketBook
    {
        public channelSnapshotChannel Channel { get; set; }
        public List<betSnapshotTypeBetSnapshotItem> CurrentBets { get; set; }
        BetfairCon Bet = new BetfairCon(UserInfo.username, UserInfo.password, "maryBrown@AOL.com.myGames.4.0");
        private static string[] Colors = new string[] { "c", "d", "h", "s" };
        private static string[] Colorshand = new string[] { "a", "2", "3", "4", "5", "6", "7", "8", "9", "10", "j", "q", "k" };


        public MarketBook(channelSnapshotChannel Channel, List<betSnapshotTypeBetSnapshotItem> CurrentBets)
        {
            this.Channel = Channel;
            this.CurrentBets = CurrentBets;
        }

        public string GetGameName => Channel.name;

        public int GetGameId => Channel.id;

        public int GetRound => Channel.game.round;

        public int GetMarketId => Channel.game.markets.market.FirstOrDefault().id;

        public marketSnapshot GetMarket => Channel.game.markets.market.FirstOrDefault();

        public channelStatusEnum GetChannelStatus => Channel.status;

        public marketStatusEnum GetMarketStatus => GetMarket.status;

        public List<objectType> GetBetfairCardsPlayer => Channel.game.gameData.Where(x => x.name.Contains("Hand")).ToList();

        public objectType GetBetfairCardsCommunity => Channel.game.gameData.Where(x => x.name.Contains("Community Cards")).FirstOrDefault();

        public double GetMarketEfficiencyBack()
        {
            var nearestBacks = GetSelection()
               .Where(c => c.status == selectionStatusEnum.IN_PLAY)
               .Select(c => c.bestAvailableToBackPrices.Count > 0 ? c.bestAvailableToBackPrices.First().Value : 0.0);


            double total = nearestBacks.Sum(c => 1.0 / c);
            return Math.Round(total, 4);

        }  

        public double GetMarketEfficiencyLay()
        {
            var nearestLays = GetSelection()
                        .Where(c => c.status == selectionStatusEnum.IN_PLAY)
                        .Select(c => c.bestAvailableToLayPrices.Count > 0 ? c.bestAvailableToLayPrices.First().Value : 0.0);

            double total = nearestLays.Sum(c => 1.0 / c);
            return Math.Round(total, 4);
        } 

        public channelSnapshotChannelGame GetGameChannel => Channel.game;

        public GameHands GetCardsCommunityCards =>  GetHands(GetBetfairCardsCommunity);

        public selectionSnapshot GetSelection(string PlayerName) => Channel.game.markets.market.FirstOrDefault().selections.selection.Where(x => x.name == PlayerName).FirstOrDefault();

        public selectionSnapshot GetSelection(int PlayerId) => Channel.game.markets.market.FirstOrDefault().selections.selection.Where(x => x.id == PlayerId).FirstOrDefault();

        public List<selectionSnapshot> GetSelection() => Channel.game.markets.market.FirstOrDefault().selections.selection.ToList();

        public List<GameHands> GetOdds => CalculatorOdds();

        public GameHands GetOddsByName(int PlayerID) => GetOdds.Where(x => x?.PlayerId == PlayerID).FirstOrDefault();

        public List<GameHands> GetHandWhereNotBet()
        {
            var IdBet = CurrentBets.Select(x=>x.selectionReference.selectionId);
            var IdPlayer = GetOdds.Select(x=>x.PlayerId);
            List<GameHands> intt = new List<GameHands>();

            foreach (var item in IdPlayer)
            {
                if (!IdBet.Contains(item))
                    intt.Add(GetOddsByName(item));
            }

            return intt;
        }

        public List<GameHands> GetHandWheretBet() => CurrentBets.Select(x => GetOddsByName(x.selectionReference.selectionId)).ToList();

        public List<betSnapshotTypeBetSnapshotItem> GetBet(int BetID) => CurrentBets.Where(x => x.selectionReference.selectionId == BetID).ToList();

        public List<price> AvailableToBack(int PlayerId) => GetSelection(PlayerId).bestAvailableToBackPrices;

        public List<price> AvailableToLay(int PlayerId) => GetSelection(PlayerId).bestAvailableToLayPrices;

        private List<GameHands> CalculatorOdds()
        {
            List<GameHands> Values = new List<GameHands>();

            var PlayerText =  GetBetfairCardsPlayer.Select(x=> GetHands(x)).ToList();

            if (GetRound == 1)
            {
                for (int i = 0; i < PlayerText.Count; i++)
                {
                    var _S = GetSelection(PlayerText[i].PlayerName);
                    var _B = GetBet(_S.id);
                    Values.Add(new GameHands()
                    {
                        Hands = PlayerText[i].Hands,
                        PlayerName = PlayerText[i].PlayerName,
                        Isbet = _B?.Count > 0 ? true : false,
                        Probability = ((double)1 / (double)PlayerText.Count),
                        SelectionSnapshot = _S,
                        Status = PlayerText[i].Status,
                        PlayerBets = GetBet(_S.id),
                    });
                }
                return Values;
            }


            int count = 0, index = 0;
            int[] playerIndex = { -1, -1, -1, -1 };
            int[] pocketIndex = { -1, -1, -1, -1 };

            foreach (var pocket in PlayerText)
            {
                if (pocket.PlayerPocket() != " ")
                {
                    playerIndex[count] = count;
                    pocketIndex[count++] = index;
                }
                index++;
            }

            string[] pocketCards = new string[count];

            for (int i = 0; i < count; i++)
            {
                pocketCards[i] = PlayerText[pocketIndex[i]].PlayerPocket();
            }

            long[] wins = new long[count];
            long[] losses = new long[count];
            long[] ties = new long[count];
            long totalhands = 0;

            Hand.HandOdds(pocketCards, GetCardsCommunityCards.PlayerPocket(), "", wins, ties, losses, ref totalhands);

            System.Diagnostics.Debug.Assert(totalhands != 0);


            if (totalhands != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var _S = GetSelection(PlayerText[i].PlayerName);
                    var _B = GetBet(_S.id);
                    Values.Add(new GameHands()
                    {
                        Hands = PlayerText[i].Hands,
                        Isbet = _B?.Count > 0 ? true : false,
                        PlayerName = PlayerText[i].PlayerName,
                        Probability = (((double)wins[i]) + ((double)ties[i]) / 2.0) / ((double)totalhands),
                        SelectionSnapshot = GetSelection(PlayerText[i].PlayerName),
                        Status = PlayerText[i].Status,
                        PlayerBets = GetBet(_S.id),
                    });
                }
            }

            return Values;
        }

        private GameHands GetHands(objectType market)
        {
            var card = GetCards();


            List<cardsValue> Value = new List<cardsValue>();

            foreach (var items in market.property)
            {
                if (!items.value.Contains("NOT"))
                {
                    Value.Add(card.Where(x => x.BetfairValue.ToString() == items.value).First());
                }
            }

            return new GameHands()
            {
                PlayerName = market.name,
                Hands = Value,
                Status = market.status != "N/A" ? (selectionStatusEnum)Enum.Parse(typeof(selectionStatusEnum), market.status) : selectionStatusEnum.NOT_APPLICABLE,
            };

        }

        public List<cardsValue> GetCards()
        {
            List<cardsValue> C = new List<cardsValue>();
            int h = 0;

            for (int i = 0; i < Colors.Count(); i++)
            {
                for (int x = 0; x < Colorshand.Count(); x++)
                {
                    C.Add(new cardsValue()
                    {
                        Value = Colorshand[x],
                        BetfairValue = h,
                        Name = Colorshand[x] + Colors[i],
                        CardType = getcardtype(Colors[i]),

                    });
                    h++;
                }

            }

            return C;
        }

        private CardType getcardtype(string type)
        {
            switch (type)
            {
                case "c":
                    return CardType.Clubs;

                case "d":
                    return CardType.Diamonds;

                case "h":
                    return CardType.Hearts;

                default:
                    return CardType.Spades;

            }
        }

        public bool Bet_BackDutching(List<GameHands> BetHands)
        {
            var calc = CalculateExchange.Dutching(BetHands.Where(x=>x.Isbet).ToList(), Balance.Skate);

            if ((calc.Sum(x => x.Stake) > 10 || calc.Count == 0))
                return false;

            var PlaceOrder = calc.Select(bet => new postBetOrderTypeBetPlace()
            {
                bidType = betBidTypeEnum.BACK,
                price = bet.Odd,
                selectionId = bet.PlayerId,
                size = bet.Stake

            }).ToList();

            postBetOrderType Postbet = new postBetOrderType()
            {
                marketId = GetMarketId,
                betPlace = PlaceOrder,
                currency = "EUR",
                round = GetRound
            };

            var BetOrder = Bet.GeteBetOrderType(Postbet).Result;//aposta no player

            if (BetOrder.HasError)
            {
                return true;
            }

            return false;
        }

        public bool Bet_CloseBackDutching(List<GameHands> BetHands)
        {
            var calc = CalculateExchange.CloseBetDutching(BetHands);

            postBetOrderType Postbet = null;
            foreach (var item in calc)
            {
                Postbet = new postBetOrderType()
                {
                    marketId = GetGameId,
                    betPlace = new List<postBetOrderTypeBetPlace>() { new postBetOrderTypeBetPlace()
                            {
                                 bidType = betBidTypeEnum.BACK,
                                 price = item.Odd,
                                 selectionId = item.PlayerId,
                                 size = item.Stake,
                            }},
                    currency = "EUR",
                    round = GetRound
                };
            }

            if (Postbet == null)
                return false;


            var BetOrder = Bet.GeteBetOrderType(Postbet).Result;//aposta no player

            if (BetOrder.HasError)
            {
                return true;
            }

            return false;
        }
    }
}
