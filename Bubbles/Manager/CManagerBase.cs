using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;
using Bubbles.Analytics;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis;

namespace Bubbles.Manager
{

    


    abstract public class CManagerBase
    {

        //internal  ConfigurationHoldAndBuy ConfigurationHoldAndBuy;
        internal  CMarket Market;
        internal readonly System.Timers.Timer HeartBeatTimer = new System.Timers.Timer();

        //private readonly bool _writeOnFile;
        //private StreamWriter _fileStreamOut;

        internal TechnicalAnalysis.Trend CurrentTrend;
        internal TechnicalAnalysis.Trend CurrentLongTermTrend;
        internal TechnicalAnalysis.TradeAction SuggestedAction;

        internal int TickCounter = 0;
        //internal int TicksPerHearthBeat = 0;
        readonly Stopwatch _stopwatch = new Stopwatch();

        //private readonly bool _verbose = true;
        //public bool Verbose { get; set; }

        public decimal BuyAt { get; set; }
        public decimal SellAt { get; set; }

         public decimal SavedMoney { get; set; }
         public decimal SavedCoins { get; set; }

        public abstract bool OnHeartBeat();
        public abstract bool OnTick();

        public Statistics Statistics;



        public void Log(string msg)
        {
            if (MainConfiguration.Configuration.Verbose)
            {
                CUtility.Log(msg);
            }
        }

        protected CManagerBase() //(ConfigurationHoldAndBuy configuration, CMarket market)
        {

            SavedCoins = 0;
            SavedMoney = 0;
        }

       

        public void Start()
        {
            this.Statistics = new Statistics(){StartValue = decimal.MinValue, EndValue = decimal.MinValue};
            //TicksPerHearthBeat = ConfigurationHoldAndBuy.Tick;
            Market.Init();

            _stopwatch.Start();
            if ( MainConfiguration.Configuration.RealTimeHeartBeat)
            {
                // create an heartbeat
                HeartBeatTimer.Elapsed += OnHeartBeat;
                HeartBeatTimer.Interval = (double)MainConfiguration.Configuration.RealTimeHeartBeatEveryMinutes * (60 * 1000); // 1000 = 1 secondo
                HeartBeatTimer.Enabled = true;
            }
            else
            {
                while (! Market.Stop)
                {
                    OnHeartBeat(null, null);
                    Thread.Sleep(10);
                }
            }
        }

        private uint _lastCandleId = uint.MinValue;
        private decimal _averageDailyPriceTmpSum =0;
        private decimal _averageDailyPriceTmpNum =0;
        // Specify what you want to happen when the Elapsed event is raised.
        private  void OnHeartBeat(object source, ElapsedEventArgs e)
        {
            try
            {

                // verifico lo stato del mercato
                // aziono il candlemaker
                if (! Market.GetTicker())
                {
                    // c'è stato un errore
                    Log("** ticker error");
                    return;
                }

                
                if (! Market.CandleMaker.Analyze.IsValid())
                {
                    // il candlemaker non puo' fare previsioni
                    Log("** Analyzer not yet ready");
                    return;
                }
   
                // gestendo la creazione delle candele occorre ripensare l'heartbeat
                //
                if (_lastCandleId != Market.CandleMaker.CurrentCandleId)
                {

                    LogTrade.Init();

                    // aggiorno la mia situazione
                    Market.GetWallet();

                    var candlemaker = Market.CandleMaker;
                    this.SuggestedAction = candlemaker.Analyze.SuggestedAction(this.Market);
                    this.CurrentTrend = candlemaker.Analyze.CurrentTrend;
                    this.CurrentLongTermTrend = candlemaker.LongTermTrend();

                    OnHeartBeat();

                    var eval = Evaluations.EvaluationManager.Instance;
                    eval.Evaluate(Market.CandleMaker.CurrentCandleDate, Market.Buy);

                    if (this.Statistics.StartValue == decimal.MinValue && this.Market.TotValue > 0)
                    {
                        this.Statistics.StartValue = this.Market.TotValue;
                    }
                    this.Statistics.EndValue = this.Market.TotValue;

                    if (MainConfiguration.Configuration.GenerateOutputTradeHistory)
                    {
                        using (var fileStreamOut = new StreamWriter(string.Format(@"data\output\output{0}.csv",MainConfiguration.Configuration.SessionId), true) {AutoFlush = true})
                        {

                            LogTrade.CandleDate = candlemaker.CurrentCandleDate;
                            LogTrade.Bid = this.Market.Buy;

                            LogTrade.EmaDiff = candlemaker.Analyze.CurrentEmaDiff;
                            LogTrade.Macd = candlemaker.Analyze.CurrentMacdValue;
                            LogTrade.MacdStandard = candlemaker.Analyze.Values.Derivative.CurrentSpeed;
                                //this.Market.CandleMaker.Analyze.EmaDiff.Derivative.CurrentSpeed;
                            LogTrade.Roc = candlemaker.Analyze.Rocdiff; //this.Statistics.Gain;
                                //this.Market.CandleMaker.Analyze.MacdList[0].Macd.Derivative.CurrentSpeed;

                            LogTrade.SuggestedAction = this.SuggestedAction;
                            LogTrade.CurrentTrend = this.CurrentTrend;
                            LogTrade.CurrentLongTermTrend = this.CurrentLongTermTrend;

                            LogTrade.TotMoney = this.Market.TotMoney;
                            LogTrade.TotCoins = this.Market.TotCoins;
                            LogTrade.TotValue = this.Market.TotValue;

                            fileStreamOut.WriteLine(LogTrade.CsvRow());

                            Cloud.CloudManager.Push(MainConfiguration.Configuration.SessionId.ToString(), "output", LogTrade.ToObjectInstance());
                        }
                    }
                    //}

                    // loggo i valori parziali
                    if (candlemaker.CurrentCandleDate.Day == 1 &&
                       candlemaker.CurrentCandleDate.Hour == 0 &&
                        candlemaker.CurrentCandleDate.Minute == 0)
                    {
                        AnalyticsTools.ProgressList.List.Add(new Progress()
                        {
                            Coins = Market.TotCoins, 
                            Money = Market.TotMoney, 
                            Value = Market.TotValue,
                            Month = candlemaker.CurrentCandleDate.Month,
                            Year = candlemaker.CurrentCandleDate.Year
                        });
                    }


                    _averageDailyPriceTmpSum += (candlemaker.GetLastCandles()[0].High + candlemaker.GetLastCandles()[0].Low) / 2;
                    _averageDailyPriceTmpNum ++;
                    // prezzo medio giornaliero
                    if (candlemaker.CurrentCandleDate.Hour == 0 &&
                        candlemaker.CurrentCandleDate.Minute == 0)
                    {
                        this.Statistics.AverageDailyPrice = _averageDailyPriceTmpNum != 0 ? _averageDailyPriceTmpSum / _averageDailyPriceTmpNum : 0;
                        _averageDailyPriceTmpSum=0;
                        _averageDailyPriceTmpNum=0;
                    }

                    Log(string.Format("{0} ", Market.UniqueName));
                    Log("Date  : " + candlemaker.CurrentCandleDate.ToString(CultureInfo.InvariantCulture));
                    Log("Money : " + Math.Round(this.Market.TotMoney, 4).ToString(CultureInfo.InvariantCulture));
                    Log("SMoney: " + Math.Round(this.SavedMoney, 4).ToString(CultureInfo.InvariantCulture));
                    Log("Coins : " + Math.Round(this.Market.TotCoins, 4).ToString(CultureInfo.InvariantCulture));                   
                    Log("SMCoin: " + Math.Round(this.SavedCoins, 4).ToString(CultureInfo.InvariantCulture));
                    Log("Value : " + Math.Round(this.Market.TotValue, 4).ToString(CultureInfo.InvariantCulture));
                    Log("Gain% : " + Math.Round(this.Statistics.Gain, 2).ToString(CultureInfo.InvariantCulture));
                    Log("Bid   : " + Math.Round(this.Market.Buy, 4).ToString(CultureInfo.InvariantCulture));
                    Log("Bid ad: " + Math.Round(this.Statistics.AverageDailyPrice, 4).ToString(CultureInfo.InvariantCulture));
                    Log("EMA   : " + Math.Round(candlemaker.Analyze.CurrentEmaDiff, 4).ToString(CultureInfo.InvariantCulture));
                    Log("MACD  : " + Math.Round(candlemaker.Analyze.CurrentMacdValue, 4).ToString(CultureInfo.InvariantCulture));
                    //Log("ROC   : " + Math.Round(this.Market.CandleMaker.Analyze.CurrentMacdValue, 4).ToString(CultureInfo.InvariantCulture));

                    Log("Trend : " + this.CurrentTrend.ToString());
                    Log("LTrend: " + CurrentLongTermTrend.ToString());
                    Log("Sugg  : " + this.SuggestedAction.ToString());
                    Log("Action: " + LogTrade.Action);
                    Log("Messag: " + LogTrade.Motivation);
                    Log("Note  : " + LogTrade.Note);
                    Log("");


                }

                /*
                // verifico se deve scattare un tick
                TickCounter++;
                if (TickCounter >= MainConfiguration.Configuration.RealTimeTickEveryHeartBeats)
                {
                    // non uso il mod perchè potrei dover usare un 
                    // cambio dinamico al tickcounter
                    TickCounter = 0;
                    //Log("*** heartbeat");
                    OnTick();

                }
                */

                _lastCandleId = Market.CandleMaker.CurrentCandleId;

                // al termine dei task decido se fermare il processo oppure no

                if (Market.Stop)
                {
                    _stopwatch.Stop();
                    HeartBeatTimer.Enabled = false;

                    AnalyticsTools.ProgressList.List.Add(new Progress()
                    {
                        Coins = Market.TotCoins,
                        Money = Market.TotMoney,
                        Value = Market.TotValue,
                        Month = Market.CandleMaker.CurrentCandleDate.Month,
                        Year = Market.CandleMaker.CurrentCandleDate.Year
                    });

                    using (var stream = new StreamWriter(@"data\output\timing.csv", true) {AutoFlush = true})
                    {
                        AnalyticsTools.ProgressList.Print(stream);
                        stream.WriteLine(",,,,");
                        AnalyticsTools.ProgressList.List.Clear();
                    }
                    /*
                    if (_fileStreamOut != null)
                    {
                        _fileStreamOut.Close();
                        _fileStreamOut.Dispose();
                        _fileStreamOut = null;
                    }
                    */
                    Log("Time Elapsed : " + _stopwatch.Elapsed);
                }

                // secondo me questo è il momento in cui vanno salvate le persistenze
            }
            catch (Exception ex)
            {
                Log("Error in hearthbeat : " + ex.Message);
                //throw;
            }
            finally
            {

            }
        }


    }
}
