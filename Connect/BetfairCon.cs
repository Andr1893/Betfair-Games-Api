using BetfairGameApi.TO;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BetfairGameApi.Connect
{
    public class BetfairCon 
    {

        //Variaveis User 
        const string GAMEXAPIPASSWORD = "gamexAPIPassword";
        const string GAMEXAPIAGENT = "gamexAPIAgent";
        const string GAMEXAPIAGENTINSTANC = "gamexAPIAgentInstance";
        const string GAMEXAPIUSERNAME = "?username={0}";
       
      

        string username { get; set; }



        string BetfairUrl = "https://api.games.betfair.com/rest/v1/{0}{1}";


   
        //Betfair Api
        string ApiBetfair_Ping = "ping";
        string ApiBetfair_Account = "account";
        string ApiBetfair_Balance = "account/snapshot";
        string ApiBetfair_BetfairAvailableBalance = "account/betfair/snapshot";
        string ApiBetfair_AccountStatement = "account/statement";
        string ApiBetfair_BetLanding = "bet";
        string ApiBetfair_BetHistory = "bet/history";
        string ApiBetfair_ChannelInfo = "channels/{0}/info";
        string ApiBetfair_ChannelsnapshotbyId = "channels/{0}/snapshot";
        string ApiBetfair_PostTransferOrder = "account/transferOrder";
        string ApiBetfair_Order = "bet/order";
        string ApiBetfair_Channels = "channels";
        string ApiBetfair_Currentbets = "bet/snapshot";

        private NameValueCollection CustomHeaders { get; set; }


        readonly string GameInformation = "game";
        readonly string MarketInformation = "market";
        readonly string BettingRoundInformation = "timing";
        readonly string SelectionsType = "selectionsType";
        readonly string BetStatus = "betStatus";
        readonly string ChannelId = "channelId";


        public BetfairCon(string gamexAPIUsername, string gamexAPIPassword, string gamexAPIAgent)
        {
       
            CustomHeaders = new NameValueCollection();

            CustomHeaders[GAMEXAPIPASSWORD] = gamexAPIPassword ;//password
            CustomHeaders[GAMEXAPIAGENT] = gamexAPIAgent;//agent
            CustomHeaders[GAMEXAPIAGENTINSTANC] = CalculateMD5Hash(DateTime.Now.ToString() + gamexAPIAgent);//agente MD5
            username = gamexAPIUsername;//username

        }

        //MD5 Calculate
        private string CalculateMD5Hash(string input)
        {

            // step 1, calculate MD5 hash from input

            MD5 md5 = MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();

        }


        private Task<XmlResponse<T>> Invoke<T>(string url)
        {
            DateTime requestStart = DateTime.Now;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                var User = string.Format(GAMEXAPIUSERNAME, username);

                var URl = string.Format(BetfairUrl, url, User);
                var client = new RestClient(URl);
                // client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest(Method.GET);

                // easily add HTTP Headers
                request.AddHeader(GAMEXAPIAGENT, CustomHeaders[GAMEXAPIAGENT]);
                request.AddHeader(GAMEXAPIAGENTINSTANC, CustomHeaders[GAMEXAPIAGENTINSTANC]);
                request.AddHeader(GAMEXAPIPASSWORD, CustomHeaders[GAMEXAPIPASSWORD]);


                // execute the request
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader stringReader = new StringReader(content);
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse((T)serializer.Deserialize(stringReader), requestStart, lastByte, watch.ElapsedMilliseconds,new Exception(),true));

            }
            catch (Exception ex)
            {
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse(default(T), requestStart, lastByte, watch.ElapsedMilliseconds, ex, false));
            }

        }

        private Task<XmlResponse<T>> Invoke<T>(string url, Dictionary<string, object> args = null)
        {
            DateTime requestStart = DateTime.Now;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                var User = string.Format(GAMEXAPIUSERNAME, username);

                var URl = string.Format(BetfairUrl, url, User);


                var client = new RestClient(URl);
                // client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest(Method.GET);

                foreach (var item in args)
                {
                    request.AddParameter(item.Key,item.Value);
                }
            
                // easily add HTTP Headers
                request.AddHeader(GAMEXAPIAGENT, CustomHeaders[GAMEXAPIAGENT]);
                request.AddHeader(GAMEXAPIAGENTINSTANC, CustomHeaders[GAMEXAPIAGENTINSTANC]);
                request.AddHeader(GAMEXAPIPASSWORD, CustomHeaders[GAMEXAPIPASSWORD]);


                // execute the request
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string



                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader stringReader = new StringReader(content);
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse((T)serializer.Deserialize(stringReader), requestStart, lastByte, watch.ElapsedMilliseconds, new Exception(), true));

            }
            catch (Exception ex)
            {
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse(default(T), requestStart, lastByte, watch.ElapsedMilliseconds, ex, false));
            }

        }

        private Task<XmlResponse<T>> InvokePost<T,B>(string url,B Value)
        {
            DateTime requestStart = DateTime.Now;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            try
            {
                var User = string.Format(GAMEXAPIUSERNAME, username);
                var URl = string.Format(BetfairUrl, url, User);


                var client = new RestClient(URl);

                // client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest(Method.POST)
                {
                    RequestFormat = DataFormat.Xml,
                    XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer()

                };

                // easily add HTTP Headers
                request.AddHeader(GAMEXAPIAGENT, CustomHeaders[GAMEXAPIAGENT]);
                request.AddHeader(GAMEXAPIAGENTINSTANC, CustomHeaders[GAMEXAPIAGENTINSTANC]);
                request.AddHeader(GAMEXAPIPASSWORD, CustomHeaders[GAMEXAPIPASSWORD]);

                request.AddBody(Value);

                var cancellationTokenSource = new CancellationTokenSource();
                // execute the request
             
                var response = client.Execute(request);
                var content = response.Content; // raw content as string

              
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringReader stringReader = new StringReader(content);
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse((T)serializer.Deserialize(stringReader), requestStart, lastByte, watch.ElapsedMilliseconds, new Exception(), true));

            }
            catch (Exception ex)
            {
                var lastByte = DateTime.Now;
                watch.Stop();
                return Task.FromResult<XmlResponse<T>>(ToResponse(default(T), requestStart, lastByte, watch.ElapsedMilliseconds, ex, false));
            }
        }

        private XmlResponse<T> ToResponse<T>(T response, DateTime requestStart, DateTime lastByteStamp, long latency,Exception Error,bool HasError)
        {
            XmlResponse<T> r = new XmlResponse<T>();
            r.Error = Error;
            r.HasError = HasError;
            r.Result = response;
            r.LastByte = lastByteStamp;
            r.RequestStart = requestStart;
            return r;
        }

        #region  Account
        //Using string ApiBetfair_Balance = "https://api.games.betfair.com/rest/v1/account/snapshot";
        public Task<XmlResponse<accountSnapshot>> GetAccountBalance()
        {
            return Invoke<accountSnapshot>(ApiBetfair_Balance);
        }

        //Using  string ApiBetfair_Account = "https://api.games.betfair.com/rest/v1/account";
        public Task<XmlResponse<accountLanding>> GetAccountLanding()
        {
            return Invoke<accountLanding>(ApiBetfair_Account);
        }

        //Using  string ApiBetfair_BetfairAvailableBalance = "https://api.games.betfair.com/rest/v1/account/betfair/snapshot";
        public Task<XmlResponse<betfairAvailableBalance>> GetBetfairAvailableBalance()
        {
            return Invoke<betfairAvailableBalance>(ApiBetfair_BetfairAvailableBalance);
        }

        //Using  string ApiBetfair_AccountStatement = "https://api.games.betfair.com/rest/v1/account/statement";
        public Task<XmlResponse<accountStatement>> GetAccountStatement()
        {
            return Invoke<accountStatement>(ApiBetfair_AccountStatement);
        }
        #endregion


        //Using string ApiBetfair_Ping = "https://api.games.betfair.com/rest/v1/ping";
        public Task<XmlResponse<ping>> GetPing()
        {
            return Invoke<ping>(ApiBetfair_Ping);
        }
     


        // Get Info Channel Inf , Bets , History
        public Task<XmlResponse<channelLanding>> GetChannelLanding()
        {
            return Invoke<channelLanding>(ApiBetfair_Channels);
        }


        //Using  string ApiBetfair_ChannelInfo = "https://api.games.betfair.com/rest/v1/channels/{$ChannelID}/info";
        public Task<XmlResponse<channelInfo>> GetChannelInfo(int ChannelID)
        {
            return Invoke<channelInfo>(string.Format(ApiBetfair_ChannelInfo, ChannelID));
        }


        public Task<XmlResponse<channelSnapshot>> GetChannelSnapshot(int id)
        {
            var url = string.Format(ApiBetfair_ChannelsnapshotbyId,id);
            return Invoke<channelSnapshot>(url);
        }

        public Task<XmlResponse<channelSnapshot>> GetChannelSnapshot(int id, channelSnapshotFilter GameInformation)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            args[this.GameInformation] = GameInformation.GameInformation;
            args[this.BettingRoundInformation] = GameInformation.BettingRoundInformation;
            args[this.MarketInformation] = GameInformation.MarketInformation;
            args[this.SelectionsType] = GameInformation.SelectionsType;

            var url = string.Format(ApiBetfair_ChannelsnapshotbyId, id);
            return Invoke<channelSnapshot>(url, args);
        }



        //Using string ApiBetfair_BetLanding = "https://api.games.betfair.com/rest/v1/bet";
        public Task<XmlResponse<betLanding>> GetBetLanding()
        {
            return Invoke<betLanding>(ApiBetfair_BetLanding);
        }

        //Using string ApiBetfair_BetHistory = "https://api.games.betfair.com/rest/v1/bet/history";
        public Task<XmlResponse<betHistory>> GetBetHistory()
        {
            return Invoke<betHistory>(ApiBetfair_BetHistory);
        }

        //Using string ApiBetfair_BetHistory = "https://api.games.betfair.com/rest/v1/bet/snapshot";
        public Task<XmlResponse<betSnapshotType>> GetCurrentbets(CurrentbetsFilter HistoryFilter)
        {

            Dictionary<string, object> args = new Dictionary<string, object>();

            args[this.BetStatus] = HistoryFilter.betResult;
            args[this.ChannelId] = HistoryFilter.ChannelID;

            return Invoke<betSnapshotType>(ApiBetfair_Currentbets, args);
        }



        //Using  string ApiBetfair_PostTransferOrder = "https://api.games.betfair.com/rest/v1/channels/{0}/info";
        public Task<XmlResponse<responseTransferOrder>> GetTransferOrder(postTransferOrder TransferOrder)
        {
            return InvokePost<responseTransferOrder,postTransferOrder>(ApiBetfair_PostTransferOrder,TransferOrder);
        }

        //Using  string ApiBetfair_PostTransferOrder = "https://api.games.betfair.com/rest/v1/channels/{0}/info";
        public Task<XmlResponse<responseBetOrderType>> GeteBetOrderType(postBetOrderType MultipleBetOrder)
        {

            return InvokePost<responseBetOrderType, postBetOrderType>(ApiBetfair_Order, MultipleBetOrder);
        }

    }

   
}
