using System;
using System.Collections.Generic;
using System.Globalization;
using Bubbles.TechnicalAnalysis;
using DataProvider.Models.TradingData;
using DataProvider.Sources;

namespace Bubbles.Market
{
    public class OpenOrders
    {
        public double Amount { get; set; }
        public double Fee { get; set; }
        public double Price { get; set; }
        public string OrderId { get; set; }
        public double Tot { get { return Amount * Price * (1 + (Type == CUtility.OrderType.Buy ? -1 : 1) * Fee); } }
        public bool R2 { get; set; }
        public CUtility.OrderType Type { get; set; }
        public DateTime Date { get; set; }
    }

    /*
     * questa è una classe base per qualsiasi mercato
     * implementa le bolle, perchè le bolle sono legate al mercato anche se poi il manager decide la strategia di creazione e le gestisce
     * i comandi necessari sono:
     * leggi il ticker : situazione attuale del mercato
     * leggi il wallet : situazione 
     */
    public abstract class CMarket
    {
        
        // MTGOX
        public string Name { get; set; }
        // MTGOX:BTC:USD
        public string UniqueName {
            get { return string.Format("{0} - {1}/{2}", this.Name, this.Currency, this.CryptoCurrency); } 
        }
        // USD
        public string Currency { get; set; }
        // BTC
        public string CryptoCurrency { get; set; }

        // commissione applicata alle operazioni
        public decimal Fee { get; set; }
        // commissione applicata alle operazioni
        public int Decimals { get; set; }


        // prezzo di vendita attuale
        public decimal Sell { get; set; }
        // prezzo di acquisto attuale
        public decimal Buy { get; set; }
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }

        public string LastorderId { get; set; }

        // EMA Estimated Moving Average per il mercato attuale
        //public decimal Ema { get; set; }

        // soldi nel wallet
        public decimal TotMoney { get; set; }
        // cryptocoins nel wallet
        public decimal TotCoins { get; set; }
        // valore complessivo
        public decimal TotValue {
            get { return TotMoney + this.Buy*TotCoins; } 
        }

        public decimal MaxMoney { get; set; }
        public decimal MaxCoins { get; set; }
        public decimal MaxValue { get; set; }

        public int TotBids { get; set; }
        public int TotAsks { get; set; }

        // valore medio
        internal decimal AvgSum = 0;
        internal decimal AvgTimes = 1;
        public decimal Average { get { return AvgSum/AvgTimes ; } }

        // ha ordini aperti
        public bool HasOpenOrders { get; set; }
        public List<OpenOrders> OpenOrders = new List<OpenOrders>();

        // trend del mercato (util per calcolare l'EMA)
        //internal  Trend Trend;// = new Trend();
        public CandleMaker CandleMaker;
        public CandleMaker CandleMakerHourly;
        //public CandleMaker CandleMakerDaily;

        // bubbles è l'elenco delle bolle nel mercato
        //private readonly List<CBubble> _bubbles = new List<CBubble>();
        //public List<CBubble> Bubbles { get { return _bubbles; } }

        // effettua un ordine
        public abstract string DoOrder(CUtility.OrderType type, decimal? cost, decimal amount, ref string orderId, bool marketOrder);
        // cancella un ordine
        public abstract bool DoCancelOrder(string orderId);
        // prende la situazione del mio portafoglio (soldi e coins)
        public abstract void GetWallet();
        // prende la situazione del mercato attuale (ask,bid,etc...)
        public abstract bool GetTicker();
        // 
        public abstract void Init();

        // true se devo fermarmi (utile per il simulatore)
        public bool Stop { get; set; }

        protected void OnTicker()
        {
                CUtility.Log(string.Format("{0} {1} SELL:{2} BUY:{3}", this.UniqueName, this.Date.ToString(CultureInfo.InvariantCulture), this.Sell, this.Buy));

                // feed the candlemaker
                var value = this.Buy;
                if (this.CandleMaker != null)
                {
                    this.CandleMaker.CandleBuilder(this.Date, value, this.Volume);
                }

                if (this.CandleMakerHourly != null)
                {
                    this.CandleMakerHourly.CandleBuilder(this.Date, value, this.Volume);
                }
        }
        protected bool InitCandles(CMarket market, CBitcoinCharts _candleSource)
        {
            //http://api.bitcoincharts.com/v1/trades.csv?symbol=mtgoxUSD

            /*
             * occorre un ciclo while che esamina bitcoincharts e cerca di estrarre i dati
             */

            CUtility.Log("reading candles");
            var candleWidth = market.CandleMaker.CandleWidth;
            var candleLength = market.CandleMaker.QueueLength();

            var duration = candleWidth * candleLength;
            var from = DateTime.Now.ToUniversalTime().AddSeconds(-duration);
            var to = DateTime.Now.ToUniversalTime();
            var source = _candleSource;
            var res = source.GetTrades(from, to);
            CUtility.Log("creating candles");
            foreach (Trade trade in res)
            {
                try
                {

                    this.Buy = trade.Price;
                    this.Date = trade.Date;
                    this.Volume = trade.Amount;
                    OnTicker();
                    /*
                   var date = trade.Date;
                   var value = trade.Price;
                   var amount = trade.Amount;
                   
                   // feed the candlemaker
                   if (this.CandleMaker != null)
                   {
                       this.CandleMaker.CandleBuilder(date, value, amount);
                   }

                   if (this.CandleMakerHourly != null)
                   {
                       this.CandleMakerHourly.CandleBuilder(date, value, amount);
                   }
                   */
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
    }
}
