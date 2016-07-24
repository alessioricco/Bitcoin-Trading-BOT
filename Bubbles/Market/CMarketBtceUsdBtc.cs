using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BtcE;
using Bubbles.TechnicalAnalysis;
using DataProvider.Sources;
using Trade = DataProvider.Models.TradingData.Trade;

namespace Bubbles.Market
{
    //https://github.com/DmT021/BtceApi/blob/master/ApiTest/Program.cs
    public class CMarketBtceUsdBtc : CMarket
    {

        protected string Api = MainConfiguration.Configuration.BtceApi;
        protected string Secret = MainConfiguration.Configuration.BtceSecret;
        private readonly CBitcoinCharts _candleSource = new DataProvider.Sources.CBitcoinChartsBtceBtcusd();
        private readonly BtcePair currentPair = BtcePair.btc_usd;
        BtceApi _btceApi;

        public override void Init()
        {
            _btceApi = new BtceApi(Api, Secret);

            this.Currency = "USD";
            this.CryptoCurrency = "BTC";
            this.Name = "Btce";
            InitCandles(this, _candleSource);

            this.Fee = BtceApi.GetFee(currentPair) / 100M;
        }




        public override string DoOrder(CUtility.OrderType type, decimal? cost, decimal amount, ref string orderId, bool marketOrder)
        {
            //throw new NotImplementedException();

            /*
            * potremmo evitare di fare ordini se non vengono smazzati gli eventuali precedenti
            */
            return "";
            try
            {
                if (cost.HasValue)
                {
                    TradeType trade = type == CUtility.OrderType.Buy ? TradeType.Buy : TradeType.Sell;
                    var result = _btceApi.Trade(currentPair, trade, cost.Value, amount);
                    orderId = result.OrderId.ToString(CultureInfo.InvariantCulture);
                    this.LastorderId = orderId;
                    System.Console.WriteLine("Order success");
                    return orderId;
                }
                else
                {
                    System.Console.WriteLine("Order failed");
                    return "";
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Order error");
                return "";
                throw;
            }

        }

        public override bool DoCancelOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        public override void GetWallet()
        {
            //throw new NotImplementedException();
            var info = _btceApi.GetInfo( );
            this.TotCoins = info.Funds.Btc;
            this.TotMoney = info.Funds.Usd;
            
        }

        public override bool GetTicker()
        {

            try
            {
                var ticker = BtceApi.GetTicker(currentPair);
                // buy e sell vanno bene per gli ordini perchè buy > sell
                this.Sell = ticker.Sell; 
                this.Buy = ticker.Buy; 
                this.Date = CUtility.UnixTime.ConvertToDateTime(ticker.ServerTime);
                this.Volume = (decimal)ticker.VolumeCurrent; //decimal.Parse(ticker.Volume);

                OnTicker();

                return true;
            }
            catch (Exception)
            {
                
                throw;
            }

        }
        public static CMarket Create()
        {
            var market = new CMarketBtceUsdBtc()
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
