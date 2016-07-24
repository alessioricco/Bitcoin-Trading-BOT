using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bubbles.Extensions;
using Bubbles.TechnicalAnalysis;
using CCXSharp.Interfaces;
using CCXSharp.MtGox;
using CCXSharp.MtGox.Models;
using DataProvider.Sources;
using Trade = DataProvider.Models.TradingData.Trade;

namespace Bubbles.Market
{
    //https://bitbucket.org/pipe2grep/cryptocoinxchange
    public class CMarketMtGoxUsdbtc : CMarket
    {
        const CCXSharp.MtGox.Models.Currency currency = CCXSharp.MtGox.Models.Currency.USD;
        protected string Api = MainConfiguration.Configuration.MtGoxApi;
        protected string Secret = MainConfiguration.Configuration.MtGoxSecret;
        private readonly CBitcoinCharts _candleSource = new DataProvider.Sources.CBitcoinChartsMtgoxBtcusd();

        public override void Init()
        {
            this.Currency = "USD";
            this.CryptoCurrency = "BTC";
            this.Name = "MtGox";
            //this.UniqueName = "MtGox:USD/BTC";
            InitCandles(this, _candleSource);
        }


        public override string DoOrder(CUtility.OrderType type, decimal? cost, decimal amount, ref string orderId, bool marketOrder)
        {
            //throw new NotImplementedException();

            return "";

            /*
             * potremmo evitare di fare ordini se non vengono smazzati gli eventuali precedenti
             */

            IMtGoxExchange mtGoxExchange = new MtGoxExchange();
            mtGoxExchange.APIKey = Api;
            mtGoxExchange.APISecret = Secret;
            OrderType ordertype = type == CUtility.OrderType.Buy ? OrderType.Bid : OrderType.Ask;
            OrderCreateResponse response = mtGoxExchange.CreateOrder(currency, ordertype, (double) amount, null);

            if (response.Result == ResponseResult.Success)
            {   
                orderId = response.OID.ToString();
                System.Console.WriteLine("Order success");
                return response.OID.ToString();
            }
            else
            {
                orderId = "";
                System.Console.WriteLine("Order failed");
                return "";
            }
        }

        public override bool DoCancelOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        public override void GetWallet()
        {
            //throw new NotImplementedException();

            try
            {
                IMtGoxExchange mtGoxExchange = new MtGoxExchange();
                mtGoxExchange.APIKey = Api;
                mtGoxExchange.APISecret = Secret;
                var info = mtGoxExchange.GetAccountInfo();
                this.Fee = (decimal)info.TradeFee;
                var wallets = info.Wallets;
                this.TotCoins = (decimal)wallets.BTC.Balance.Value;
                this.TotMoney = (decimal)wallets.USD.Balance.Value;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public override bool GetTicker()
        {
            try
            {
                IMtGoxExchange mtGoxExchange = new MtGoxExchange();
                mtGoxExchange.APIKey = Api;
                mtGoxExchange.APISecret = Secret;

                var ticker = mtGoxExchange.GetTicker(currency);
                this.Buy = (decimal)ticker.AskValue; // sell 292 (i valori sono invertiti perchè gli ordini hanno buy > sell)
                this.Sell = (decimal) ticker.BidValue; // buy 291
                this.Date = ticker.TimeStamp;
                this.Volume = (decimal) ticker.VolumeValue; //decimal.Parse(ticker.Volume);

               OnTicker();

                return true;
            }
            catch (Exception)
            {
                throw;
                return true;
            }
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
