using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Bubbles.SQLite;
using Bubbles.TechnicalAnalysis;
using DataProvider.Models.TradingData;
using RestSharp;

namespace Bubbles.Market
{

    internal interface ISimulatedDataSource
    {
        bool Init(CMarket market);
        bool Next();
        decimal Ask { get; set; }
        decimal Bid { get; set; }
        DateTime Date { get; set; }
        decimal Volume { get; set; }
    }

    internal class SimulateddataSourceSqLite : ISimulatedDataSource
    {
        private SQLiteMtGox _sql;
        public string Currency { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long SqLiteDeltaTime { get; set; }

        public bool Init(CMarket market)
        {
            _sql = new SQLiteMtGox()
            {
                Currency = this.Currency,
                StartTime = this.StartTime,
                EndTime = this.EndTime
            };

            return _sql.Start();
        }

        public bool Next()
        {
            //throw new NotImplementedException();
            if (! _sql.Next(this.SqLiteDeltaTime))
            {
                return false;
            }
            this.Ask = _sql.Ask;
            this.Bid = _sql.Bid;
            this.Date = _sql.MarketTime;
            return true;
        }

        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
    }

    internal class SimulateddataSourceTextFile : ISimulatedDataSource
    {

        //uso file o sql per la sequenza?
        public string FileName { get; set; }
        protected bool IsFileCsv;
        protected List<string> Candles = new List<string>();
        protected int CurrentCandle = 1; // il valore 0 contiene le intestazioni


        public virtual bool Init(CMarket market)
        {
            // i file sono in formato csv oppure in formato bitcoinchart, li distinguo dall'estensione 
            // si trovano in \data

            IsFileCsv = this.FileName.ToLower().EndsWith(".csv");

            // creo una lista di stringhe
            using (var fileStream = new StreamReader(FileName))
            {
                var file = fileStream.ReadToEnd();
                Candles = file.Split(new Char[] {'\n'}).ToList();
            }
            return true;
        }

        /// <summary>
        /// estrapola la i-esima candela dal set che ho in memeoria
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual string[] GetCandle(int index)
        {
            return Candles[index].Split(',');
        }

        public bool Next()
        {
            if (Candles.Count <= CurrentCandle)
            {
                return false;
            }

            var rnd = new Random();
            // numero casuale da 0 a 1/100
            var diffPerc = Math.Round(rnd.NextDouble()/100, 4);

            if (this.IsFileCsv)
            {
                // leggo da file la candela
                //var candle = _candles[_currentCandle++];
                //var candleValues = candle.Split(',');
                var candleValues = GetCandle(CurrentCandle++);

                decimal priceBid;
                decimal priceAsk;
                if (candleValues.Count() >= 3)
                {
                    /*
                    // nei files di candela non ci sono ask e bid ma open e close
                    priceBid = Convert.ToDecimal(candleValues[2], CultureInfo.InvariantCulture);
                    priceAsk = Convert.ToDecimal(candleValues[1], CultureInfo.InvariantCulture);
                     * */
                    priceBid = Convert.ToDecimal(candleValues[1], CultureInfo.InvariantCulture);
                    priceAsk = priceBid*(decimal) (1 + diffPerc);
                }
                else
                {
                    priceBid = Convert.ToDecimal(candleValues[1], CultureInfo.InvariantCulture);
                    priceAsk = priceBid*(decimal) (1 + diffPerc);
                }
                //this.Date = DateTime.Now;
                try
                {

                    //this.Date = DateTime.Now;
                    uint dateux = 0;
                    var result = uint.TryParse(candleValues[0], out dateux);
                    this.Date = result ? CUtility.UnixTime.ConvertToDateTime(dateux) : DateTime.Parse(candleValues[0]);

                }
                catch (Exception)
                {

                    this.Date = DateTime.Now;
                }


                this.Bid = Math.Round(priceBid, 4);
                this.Ask = Math.Round(priceAsk, 4);
            }
            else
            {
                var candle = Candles[CurrentCandle++];
                var candleValues = candle.Split(' ');
                decimal price = Convert.ToDecimal(candleValues[1].Split('\t')[7], CultureInfo.InvariantCulture);

                try
                {
                    this.Date = DateTime.Parse(candleValues[0] + " " + candleValues[1].Split('\t')[0]);
                }
                catch (Exception)
                {
                    this.Date = DateTime.Now;
                }

                this.Bid = Math.Round(price, 4);
                this.Ask = Math.Round(price*(decimal) (1 + diffPerc), 4);

            }

            return true;
        }

        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
    }

    internal class SimulateddataSourceTextFileReverse : SimulateddataSourceTextFile
    {

        protected override string[] GetCandle(int index)
        {
            //in questo caso inverto le candele
            var current = this.Candles[index].Split(',');
            var mid = (int) Math.Ceiling((decimal) this.Candles.Count/2);
            var dist = index - mid;
            var opp = mid - dist;
            var opposite = this.Candles[opp].Split(',');

            opposite[0] = current[0];
            return opposite;
        }
    }

    internal class SimulateddataSourceMtgox : ISimulatedDataSource
    {

        internal class JsonGetBalanceMtGoxValues
        {
            public string value { get; set; }
            //public string value_int { get; set; }
            //public string display { get; set; }
            //public string display_short { get; set; }
            public string currency { get; set; }
        }


        internal class JsonGetTickerFastLastLocal
        {
            public JsonGetBalanceMtGoxValues buy { get; set; }
            public JsonGetBalanceMtGoxValues sell { get; set; }
            public long now { get; set; }
        }

        internal class JsonGetTickerFastMtGox
        {
            public string result { get; set; }
            public JsonGetTickerFastLastLocal @return { get; set; }

        }


        // }


        public bool Init(CMarket market)
        {
            //http://api.bitcoincharts.com/v1/trades.csv?symbol=mtgoxUSD

            /*
             * occorre un ciclo while che esamina bitcoincharts e cerca di estrarre i dati
             */

            CUtility.Log("reading candles");
            var candleWidth = market.CandleMaker.CandleWidth;
            var candleLength = market.CandleMaker.QueueLength();

            var duration = candleWidth*candleLength;
            var from = DateTime.Now.ToUniversalTime().AddSeconds(-duration);
            var to = DateTime.Now.ToUniversalTime();
            var source = new DataProvider.Sources.CBitcoinChartsMtgoxBtcusd();
            var res = source.GetTrades(from,to);
            CUtility.Log("creating candles");
            foreach (Trade trade in res)
            {
                try
                {
                    var date = trade.Date;
                    var value = trade.Price;
                    var amount = trade.Amount;
                    market.CandleMaker.CandleBuilder(date, value, amount);
                    //CUtility.Log(string.Format("Candle {0} {1} ",date.ToString(CultureInfo.InvariantCulture),value.ToString(CultureInfo.InvariantCulture)));

                }
                catch (Exception ex)
                {
                    CUtility.Log(ex.Message);
                    //throw;
                }
            }
            CUtility.Log("start trading");
            //throw new NotImplementedException();
            return true;
        }

        public bool Next()
        {

            try
            {
                CUtility.Log("mtgox...");
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
                this.Date = CUtility.UnixTimeStampToDateTime((double) now/1000000).AddHours(1);
                 //CUtility.UnixTime.ConvertToDateTime((uint) now/1000000).AddHours(1); //DateTime.Now;
                CUtility.Log(string.Format("{0} {1}", this.Date.ToString(CultureInfo.InvariantCulture), this.Bid));
            }
            catch (Exception ex)
            {

                //throw;
                CUtility.Log(ex.Message + " " + ex.StackTrace);
                return true; // non puo' mai essere false se capita un errore su mtgox
            }

            return true;
        }

        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
    }


    internal class CMarketSimulator : CMarket
    {

        //uso file o sql per la sequenza?
        public bool UseFile { get; set; }
        private ISimulatedDataSource _simulatedDataSource;
        public string FileName { get; set; } // se uso i files posso passare il filename

        public bool UseMtGox { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long DeltaTime { get; set; }

        public CMarketSimulator()
        {
            this.Currency = "USD";
            //this.Ema = 0;
            this.Fee = (decimal) 0.006; // aggiornato da sistema
            this.CryptoCurrency = "BTC";
            this.Name = "Simulator";
            //this.UniqueName = "Simulator:USD/BTC";
            this.Decimals = 5;

            // situazione iniziale del mio portafoglio
            this.TotMoney = MainConfiguration.Configuration.StartWalletMoney;
            this.TotCoins = MainConfiguration.Configuration.StartWalletCoins;

            this.Stop = false;

        }

        public override string DoOrder(CUtility.OrderType type, decimal? cost, decimal amount, ref string orderId,
            bool marketOrder)
        {

            decimal costReal;
            //throw new NotImplementedException();
            if (type == CUtility.OrderType.Buy)
            {
                costReal  =cost.HasValue ? cost.Value : this.Buy;

                this.TotMoney -= amount * costReal;
                var coins = amount*(1M - Fee);
                this.TotCoins += coins;
                orderId = System.Guid.NewGuid().ToString();
                TotBids++;
                //this.TotValue = TotCoins * costReal + this.TotMoney;
            }
            else
            {
                costReal = cost.HasValue ? cost.Value : this.Sell;
                //throw new NotImplementedException();
                var money = amount * costReal;

                this.TotCoins -= amount;
                this.TotMoney += money*(1M - Fee);
                //this.TotValue = TotCoins * costReal + this.TotMoney;
                TotAsks++;
            }

            // fissa eventuali negativi per arrotondamento
            if (this.TotCoins < 0) this.TotCoins = 0;
            if (this.TotMoney < 0) this.TotMoney = 0;

            this.AvgTimes++;
            this.AvgSum += this.TotValue;

            return "";
        }

        public override bool DoCancelOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        public override void GetWallet()
        {
            //throw new NotImplementedException();
            // CUtility.Log("Money : " + Math.Round( this.TotMoney, this.Decimals ).ToString(CultureInfo.InvariantCulture));
            //CUtility.Log("Coins : " + Math.Round( this.TotCoins, this.Decimals ).ToString(CultureInfo.InvariantCulture));

            // il virtual total è il totale se tutte le bolle scoppiassero
            //var vtot = this.TotMoney + this.Bubbles.Sum(o => o.Coins*o.SellAt*(1-Fee));
            //CUtility.Log("VTot : " + Math.Round(vtot, this.Decimals).ToString(CultureInfo.InvariantCulture));

            // il real total è il totale di quello che ho
            //var rtot = this.TotMoney + this.TotCoins * this.Ask * (1-Fee);
            //CUtility.Log("RTot : " + Math.Round(rtot, this.Decimals).ToString(CultureInfo.InvariantCulture));
            if (TotCoins > MaxCoins) MaxCoins = TotCoins;
            if (TotMoney > MaxMoney) MaxMoney = TotMoney;
            if (TotValue > MaxValue) MaxValue = TotValue;         
        }

        public override bool GetTicker()
        {

            if (! _simulatedDataSource.Next())
            {

                {
                    CUtility.Log("END OF SIMULATION : ");

                    this.Stop = true;
                    return true;
                }
            }

            try
            {

                this.Sell = _simulatedDataSource.Ask;
                this.Buy = _simulatedDataSource.Bid;

                if (MainConfiguration.Configuration.MarketSimulatorEnableNoise)
                {
                    var rnd = new Random(1234);
                    var noise = (2*rnd.NextDouble() - 1) * (double) MainConfiguration.Configuration.MarketSimulatorNoisePercent;
                    this.Buy *= (1 + (decimal) noise);
                    this.Sell = this.Buy*(1.05M);
                }

                if (this.CandleMaker != null)
                {
                    this.CandleMaker.CandleBuilder(_simulatedDataSource.Date, _simulatedDataSource.Bid, _simulatedDataSource.Volume);
                }

                if (this.CandleMakerHourly != null)
                {
                    this.CandleMakerHourly.CandleBuilder(_simulatedDataSource.Date, _simulatedDataSource.Bid, _simulatedDataSource.Volume);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;

        }

        public override void Init()
        {

            _simulatedDataSource = this.GetDataSource();
            _simulatedDataSource.Init(this);

        }

        protected virtual ISimulatedDataSource GetDataSource()
        {
            ISimulatedDataSource simulatedDataSource;
            if (this.UseFile)
            {
                if (MainConfiguration.Configuration.MarketSimulatorUseCandleFileReversed)
                {
                    var dataSource = new SimulateddataSourceTextFileReverse()
                    {
                        FileName = this.FileName
                    };
                    simulatedDataSource = dataSource;
                }
                else
                {
                    var dataSource = new SimulateddataSourceTextFile()
                    {
                        FileName = this.FileName
                    };
                    simulatedDataSource = dataSource;
                }
            }
            else if (this.UseMtGox)
            {
                var datasource = new SimulateddataSourceMtgox() {};
                simulatedDataSource = datasource;
            }
            else
            {
                var dataSource = new SimulateddataSourceSqLite
                {
                    SqLiteDeltaTime = this.DeltaTime,
                    StartTime = this.StartTime,
                    EndTime = this.EndTime,
                    Currency = this.Currency
                };
                simulatedDataSource = dataSource;
            }
            return simulatedDataSource;
        }

        public static CMarket Create()
        {
            var market = new CMarketSimulator()
            {
                UseMtGox = MainConfiguration.Configuration.MarketSimulatorUseMtgox,
                UseFile = MainConfiguration.Configuration.MarketSimulatorUseCandleFile,
                FileName = MainConfiguration.Configuration.MarketSimulatorCandleFileName,
                DeltaTime = MainConfiguration.Configuration.MarketSimulatorSqLiteDeltaTime,
                StartTime = MainConfiguration.Configuration.MarketSimulatorSqLiteStartDate,
                EndTime = MainConfiguration.Configuration.MarketSimulatorSqLiteEndDate,
                CandleMaker = new CandleMaker()
                {
                    GenerateFile = MainConfiguration.Configuration.GenerateCandleFile,
                    CandleWidth = MainConfiguration.Configuration.CandleWidthInSeconds,
                    Analyze =  TechnicalAnalysis.Analyzer.Builder.Create(MainConfiguration.Configuration.AnalyzerClass)
                },
                CandleMakerHourly = new CandleMaker()
                {
                    GenerateFile = false,
                    CandleWidth =  60 * (15 * 4),
                    Analyze = null
                }
            };
            return market;
        }

    }
}
