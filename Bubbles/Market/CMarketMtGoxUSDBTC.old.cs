using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Cryptography;
using Bubbles.TechnicalAnalysis;
using RestSharp;
using RestSharp.Contrib;

namespace Bubbles.Market
{
    public class CMarketMtGoxUsdbtc : CMarket
    {

        private const string Apikey = "";
        private const string Secret = "";
        //private static UInt32 _nonce = CUtility.UnixTime.Now;
        private static Int64 _nonce = DateTime.Now.Ticks;


        public CMarketMtGoxUsdbtc()
        {
            this.Currency = "USD";
            //this.Ema = 0;
            this.Fee = (decimal)0.06; // aggiornato da sistema
            this.CryptoCurrency = "BTC";
            this.Name = "MtGox";
            this.UniqueName = "MtGox:USD/BTC";
            this.Decimals = 5;
        }

        internal class JsonGetTickerFastLastLocal
        {
            public CMarketMtGoxUsdbtc.JsonGetBalanceMtGoxValues buy { get; set; }
            public CMarketMtGoxUsdbtc.JsonGetBalanceMtGoxValues sell { get; set; }
            public long now { get; set; }
        }

        internal class JsonGetTickerFastMtGox
        {
            public string result { get; set; }
            public SimulateddataSourceMtgox.JsonGetTickerFastLastLocal @return { get; set; }

        }

        public override void GetWallet()
        {
            //throw new NotImplementedException();



        }

        public override bool GetTicker()
        {
            try
            {
                var client = new RestClient("http://data.mtgox.com/api/1/BTCUSD/ticker_fast");
                var request = new RestRequest
                {
                    Method = Method.GET
                };
                var response = client.Execute<JsonGetTickerFastMtGox>(request);
                //if (response == null) return false;
                var ask = response.Data.@return.sell.value;
                var bid = response.Data.@return.buy.value;
                var now = response.Data.@return.now;
                this.Ask = decimal.Parse(ask, CultureInfo.InvariantCulture);
                this.Bid = decimal.Parse(bid, CultureInfo.InvariantCulture);
                this.Date = CUtility.UnixTimeStampToDateTime((double)now / 1000000).AddHours(1);
                return true;
            }
            catch (Exception)
            {

                throw;
                return false;
            }
        }

        public override void Init()
        {
            //throw new NotImplementedException();
        }

        protected string ToQueryString(NameValueCollection parameters)
        {
            if (parameters != null)
            {
                return "&" + string.Join("&", Array.ConvertAll(parameters.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(parameters[key]))));
            }

            return "";
        }
        private Int64 getNonce()
        {
            if (_nonce < DateTime.Now.Ticks)
            {
                _nonce = DateTime.Now.Ticks;
                return _nonce;
            }
            _nonce++;
            return _nonce;
        }
        private string getHash(string secret, String message)
        {
            var hmacsha512 = new HMACSHA512(Convert.FromBase64String(secret));
            var messageBytes = Encoding.UTF8.GetBytes(message);
            return Convert.ToBase64String(hmacsha512.ComputeHash(messageBytes));
        }

        internal class JsonGetBalanceMtGoxValues
        {
            public string value { get; set; }
            //public string value_int { get; set; }
            //public string display { get; set; }
            //public string display_short { get; set; }
            public string currency { get; set; }
        }

        internal class JsonGetBalanceMtGoxWallet
        {
            public JsonGetBalanceMtGoxValues Balance { get; set; }
            public string Operations { get; set; }
            //public JsonGetBalanceMtGoxValues Daily_Withdraw_Limit { get; set; }
            //public JsonGetBalanceMtGoxValues Monthly_Withdraw_Limit { get; set; }
            //public JsonGetBalanceMtGoxValues Max_Withdraw { get; set; }
            public JsonGetBalanceMtGoxValues Open_Orders { get; set; }

        }

        internal class JsonGetBalanceMtGoxWallets
        {
            public JsonGetBalanceMtGoxWallet BTC { get; set; }
            public JsonGetBalanceMtGoxWallet USD { get; set; }
        }

        internal class JsonGetBalanceMtGoxData
        {
            //public string login { get; set; }
            //public string index { get; set; }
            //public string id { get; set; }
            //public List<string> rights { get; set; }
            //public string language { get; set; }
            //public string created { get; set; }
            //public string last_login { get; set; }
            public JsonGetBalanceMtGoxWallets Wallets { get; set; }
            //public JsonGetBalanceMtGoxValues Monthly_Volume { get; set; }
            public string Trade_Fee { get; set; }
        }

        internal class JsonGetBalanceMtGox
        {
            public string result { get; set; }
            public JsonGetBalanceMtGoxData data { get; set; }
        }

        protected void GetBalance()
        {
            try
            {
                //throw new NotImplementedException();
                CUtility.Log("");
                CUtility.Log(string.Format("[GetBalance] site:{0} ", this.Name));

                const string apiurl = "https://data.mtgox.com/api/2/";
                const string path = "BTCUSD/money/info";

                var client = new RestClient(apiurl + path);
                var request = new RestRequest();
                _nonce = getNonce(); //DateTime.Now.Ticks;
                //_nonce++;

                var data = "nonce=" + _nonce;

                var sign = getHash(Secret, path + Convert.ToChar(0) + data);
                request.AddHeader("Rest-Key", Apikey);
                request.AddHeader("Rest-Sign", sign);

                request.Method = Method.POST;
                request.AddParameter("nonce", _nonce);

                var response = client.Execute<JsonGetBalanceMtGox>(request);
                CUtility.Log(response.Content);

                var openorderBtc = decimal.Parse(response.Data.data.Wallets.BTC.Open_Orders.value,
                    CultureInfo.InvariantCulture.NumberFormat);
                var openorderMoney = decimal.Parse(response.Data.data.Wallets.USD.Open_Orders.value,
                    CultureInfo.InvariantCulture.NumberFormat);

                this.TotCoins =
                    decimal.Parse(response.Data.data.Wallets.BTC.Balance.value, CultureInfo.InvariantCulture.NumberFormat) -
                    openorderBtc;
                this.TotMoney =
                    decimal.Parse(response.Data.data.Wallets.USD.Balance.value, CultureInfo.InvariantCulture.NumberFormat) -
                    openorderMoney;

                this.HasOpenOrders = openorderBtc > 0 || openorderMoney > 0;

                this.Fee = decimal.Parse(response.Data.data.Trade_Fee, CultureInfo.InvariantCulture.NumberFormat)/100;

                //OnGetBalance();
            }
            catch (Exception ex)
            {

                CUtility.Log(ex.Message);
                //this.SessionDisabled = true;
            }
        }

        internal class JsonTrade
        {
            public string Stamp { get; set; }
            public string Price { get; set; }
            public string Amount { get; set; }
            public string PriceInt { get; set; }
            public string AmountInt { get; set; }
            public string Tid { get; set; }
            public string PriceCurrency { get; set; }
            public string Item { get; set; }
            public string TradeType { get; set; }
            public string Primary { get; set; }
            public string Properties { get; set; }
        }

        internal class JsonOrderBookMtGox
        {

            public List<JsonTrade> asks { get; set; }
            public List<JsonTrade> bids { get; set; }
        }

        internal class JsonOrderBook
        {
            public string Result { get; set; }
            public JsonOrderBookMtGox Return { get; set; }
        }

        internal class JsonOrderResponse
        {
            public string Result { get; set; }
            public string Data { get; set; }
        }

        internal class JsonCancelOrderResponse
        {
            public string Result { get; set; }
            //public string Data { get; set; }
        }

        internal class JsonSingleOrderMtgox
        {
            public string status { get; set; }
            public string oid { get; set; }
            /*
            public string Amount { get; set; }
            public string PriceInt { get; set; }
            public string AmountInt { get; set; }
            public string Tid { get; set; }
            public string PriceCurrency { get; set; }
            public string Item { get; set; }
            public string TradeType { get; set; }
            public string Primary { get; set; }
            public string Properties { get; set; }
             * */
        }

        internal class JsonOrdersMtgox
        {
            public string Result { get; set; }
            public List<JsonSingleOrderMtgox> Data { get; set; }
        }
        /*
        public void GetOrders()
        {
            //throw new NotImplementedException();
            CUtility.Log(string.Format("[GetOrders] site:{0} ", this.UniqueName));


            //http://data.mtgox.com/api/1/BTCUSD/depth/fetch (corretta)

            const string url = "http://data.mtgox.com/api/1/BTCUSD/depth/fetch";
            CUtility.Log(url);

            var client = new RestClient(url);
            var request = new RestRequest { Method = Method.GET };

            //IRestResponse<JsonOrderBookMtGox> 
            var response = client.Execute<JsonOrderBook>(request);
            //CApplication.Log(response.Content);

            Asks.Clear();
            Bids.Clear();

            // riempo gli ask e i bid
            if (response.Data != null && response.Data.Return != null)
            {
                foreach (var ll in response.Data.Return.asks)
                {

                    var ml = new CMarketListing
                        {
                            Price = double.Parse(ll.Price, CultureInfo.InvariantCulture.NumberFormat),
                            BtcSize = double.Parse(ll.Amount, CultureInfo.InvariantCulture.NumberFormat),
                            Parent = this //,
                            //Date = CApplication.UnixTimeStampToDateTime(double.Parse(ll.Stamp.ToString(CultureInfo.InvariantCulture.NumberFormat), CultureInfo.InvariantCulture.NumberFormat))
                        };

                    Asks.Add(ml);


                }
                foreach (var ll in response.Data.Return.bids)
                {

                    var ml = new CMarketListing
                        {
                            Price = double.Parse(ll.Price, CultureInfo.InvariantCulture.NumberFormat),
                            BtcSize = double.Parse(ll.Amount, CultureInfo.InvariantCulture.NumberFormat),
                            Parent = this //,
                            //Date = CApplication.UnixTimeStampToDateTime(double.Parse(ll.Stamp.ToString(CultureInfo.InvariantCulture.NumberFormat), CultureInfo.InvariantCulture.NumberFormat))
                        };

                    Bids.Add(ml);


                }
            }
            else
            {
                CUtility.Log(@"error");
            }

            DoSaveTicker();
        }
        */
        protected void GetOpenOrders()
        {
            try
            {
                //throw new NotImplementedException();
                CUtility.Log("");
                CUtility.Log(string.Format("[OpenOrders] site:{0} ", this.Name));

                const string apiurl = "https://data.mtgox.com/api/2/";
                const string path = "BTCUSD/money/orders";

                var client = new RestClient(apiurl + path);
                var request = new RestRequest();
                _nonce = getNonce(); //DateTime.Now.Ticks;
                //_nonce++;

                var data = "nonce=" + _nonce;

                var sign = getHash(Secret, path + Convert.ToChar(0) + data);
                request.AddHeader("Rest-Key", Apikey);
                request.AddHeader("Rest-Sign", sign);

                request.Method = Method.POST;
                request.AddParameter("nonce", _nonce);

                var response = client.Execute<JsonOrdersMtgox>(request);
                //CUtility.Log(response.Content);

                OpenOrders.RemoveAll(x => ! response.Content.Contains(x.OrderId));

                var ordercompleted = true;
                foreach (var o in response.Data.Data)
                {
                    //if (o.oid == this.OrderIdR2)
                    {
                        //ordercompleted = false;
                       
                    }
                }
                if (ordercompleted)
                {
                    //this.OrderIdR2 = string.Empty;
                }

                /*
                var openorderBtc = double.Parse(response.Data.data.Wallets.BTC.Open_Orders.value, CultureInfo.InvariantCulture.NumberFormat);
                var openorderMoney = double.Parse(response.Data.data.Wallets.USD.Open_Orders.value, CultureInfo.InvariantCulture.NumberFormat);

                this.AvailableBtc = double.Parse(response.Data.data.Wallets.BTC.Balance.value, CultureInfo.InvariantCulture.NumberFormat) - openorderBtc;
                this.AvailableMoney = double.Parse(response.Data.data.Wallets.USD.Balance.value, CultureInfo.InvariantCulture.NumberFormat) - openorderMoney;

                this.HasOpenOrder = openorderBtc > 0 || openorderMoney > 0;
                */


                //this.Fee = double.Parse(response.Data.data.Trade_Fee, CultureInfo.InvariantCulture.NumberFormat) / 100;

                //OnGetBalance();
            }
            catch (Exception ex)
            {

                CUtility.Log(ex.Message);
                //this.SessionDisabled = true;
            }


        }

        public override string DoOrder(CUtility.OrderType type, decimal? price, decimal amount, ref string orderId, bool isMarket)
        {
            
            if (price == null)
            {
                return null;
            }
              

            amount = Math.Round(amount, 8);
            price = Math.Round(price.Value, this.Decimals);

            

             


            const string apiurl = "https://data.mtgox.com/api/2/";
            const string path = "BTCUSD/money/order/add";

            var client = new RestClient(apiurl + path);
            var request = new RestRequest();
            _nonce = getNonce(); // DateTime.Now.Ticks;


            var data = "nonce=" + _nonce;

            var amountInt = (Math.Round(amount, 8)*100000000).ToString(CultureInfo.InvariantCulture);

            // Bid/ask
            string orderType = "bid";
            if (type == CUtility.OrderType.Sell)
            {
                orderType = "ask";
            }
            var parameters = new NameValueCollection
                {
                    {"amount_int", amountInt},
                    {"type", orderType}
                };

            //if (price != null)
            {
                if (!isMarket)
                {
                    var priceInt = (Math.Round((double) price, 5)*100000).ToString(CultureInfo.InvariantCulture);
                    parameters.Add("price_int", priceInt);
                }
            }

            var sign = getHash(Secret, path + Convert.ToChar(0) + data + ToQueryString(parameters));
            request.AddHeader("Rest-Key", Apikey);
            request.AddHeader("Rest-Sign", sign);

            request.Method = Method.POST;
            request.AddParameter("nonce", _nonce);
            foreach (string key in parameters.Keys)
            {
                request.AddParameter(key, parameters[key]);
            }

            var response = client.Execute<JsonOrderResponse>(request);
            CUtility.Log(response.Content);

            try
            {

                var result = response.Data.Result == "success" ? response.Data.Data : null;
                if (result != null)
                {
                    orderId = response.Data.Data;
                    //this.OnDoOrder(type, price, amount, orderId,r2);
                    //OrdersInThisIteration++;
                    
                }
                return result;

            }
            catch (Exception e)
            {
                CUtility.Log(e.Message);
                return null;
            }
        }

        public override bool DoCancelOrder(string orderId)
        {
            //if (PreDoCancelOrder(orderId))
            {

                const string apiurl = "https://data.mtgox.com/api/2/";
                const string path = "BTCUSD/money/order/cancel";

                var client = new RestClient(apiurl + path);
                var request = new RestRequest();
                _nonce = getNonce(); //DateTime.Now.Ticks;


                var data = "nonce=" + _nonce;

                //var amountInt = (Math.Round(amount, 8) * 100000000).ToString(CultureInfo.InvariantCulture);

                var parameters = new NameValueCollection
                {
                    {"oid", orderId}
                   
                };
                /*
                if (price != null)
                {
                    var priceInt = (Math.Round((double)price, 5) * 100000).ToString(CultureInfo.InvariantCulture);
                    parameters.Add("price_int", priceInt);
                }
                */
                var sign = getHash(Secret, path + Convert.ToChar(0) + data + ToQueryString(parameters));
                request.AddHeader("Rest-Key", Apikey);
                request.AddHeader("Rest-Sign", sign);

                request.Method = Method.POST;
                request.AddParameter("nonce", _nonce);
                foreach (string key in parameters.Keys)
                {
                    request.AddParameter(key, parameters[key]);
                }

                var response = client.Execute<JsonCancelOrderResponse>(request);
                CUtility.Log(response.Content);

                try
                {

                    var result = response.Data.Result == "success" ;
                    /*
                    if (result != null)
                    {
                        this.OnDoOrder(type, price, amount);
                    }
                     * */
                    //if (result) OrdersInThisIteration--;
                    return result;

                }
                catch (Exception e)
                {
                    CUtility.Log(e.Message);
                    return false;
                }

            }
            return false;
            //throw new NotImplementedException();
        }

        public static CMarket Create()
        {
            var market = new CMarketMtGoxUsdbtc()
            {
                CandleMaker = new CandleMaker()
                {
                    GenerateFile = MainConfiguration.Configuration.GenerateCandleFile,
                    CandleWidth = MainConfiguration.Configuration.CandleWidthInSeconds,
                    Analyze = TechnicalAnalysis.Analyzer.Builder.Create(MainConfiguration.Configuration.AnalyzerClass)
                },
                CandleMakerHourly = new CandleMaker()
                {
                    GenerateFile = false,
                    CandleWidth = 60 * (15 * 4),
                    Analyze = null
                }
            };
            return market;
        }


        }

    
}
