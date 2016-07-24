using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Analytics;
using Bubbles.Evaluations;
using Bubbles.Manager;
using Bubbles.Market;


namespace Bubbles.TechnicalAnalysis.Analyzer
    {
        /// <summary>
        /// Questa classe aggiorna gli indicatori di analisi tecnica
        /// </summary>
        public  abstract partial class Analyze
        {
            //private readonly EmaDiff _emadiff = EmaDiff.Create(10, 21);
            //private readonly Macd _macd = Macd.Create(12, 26, 9);
            protected decimal CoinValue;

            public EmaDiff EmaDiff { get; set; }
            private decimal _emadiff = decimal.MinValue;
            public decimal CurrentEmaDiff { get { return _emadiff; } }

            // calcolo del trend
            public Ema Ema20 = new Ema() { NumberOfDays = 20, QueueLength = MainConfiguration.Configuration.QueueLength }; // 150
            public Ema Ema50 = new Ema() { NumberOfDays = 50, QueueLength = MainConfiguration.Configuration.QueueLength }; // 150
            public Sma Sma200 = new Sma() { NumberOfDays = 200 };

            public CStopLoss StopLoss { get; set; }
            public CStopLoss StopLossReverse { get; set; }

            //public CandlePredictor CandlePredictor { get; set; }

            public Roc Roc = new Roc() { QueueLength = MainConfiguration.Configuration.QueueLength };
            public decimal Rocdiff = decimal.MinValue;

            protected readonly Macd Macd122609 = Macd.Create(12, 26, 9);
            public decimal CurrentMacdValue = decimal.MinValue;

            //protected readonly Macd _macd031001 = Macd.Create(3, 10, 1);
            //protected decimal _macd031001Value = decimal.MinValue;

            //public List<MacdItem> MacdList;

            public decimal CurrentValueMax = decimal.MinValue;
            public decimal CurrentValueMin = decimal.MaxValue;
            public decimal LastValueMax = decimal.MinValue;
            public decimal LastValueMin = decimal.MaxValue;

            public abstract TradeAction SuggestedAction(CMarket market);

            public MyValue Values = new MyValue() { QueueLength = MainConfiguration.Configuration.QueueLength };

            public Trend CurrentTrend = TechnicalAnalysis.Trend.Unknown;

            protected Analyze()
            {
                InitDelegates();
            }

            public bool IsValid()
            {
                var result = EmaDiff.IsValid(); //&& (CandlePredictor == null || CandlePredictor.IsValid());
                result = result & Ema20.IsValid() && Ema50.IsValid() && Sma200.IsValid();
                result = result & Roc.IsValid();
                if (result)
                {
                    return this.Macd122609.IsValid(); //this.MacdList.All(item => item.Macd.IsValid());
                }

                return false;
            }

            /// <summary>
            /// l'algoritmo è una specie di sondaggio tra i vari indicatori
            /// vince la maggioranza, se ci sono parimerito si resta stabile
            /// </summary>
            /// <returns></returns>
            /// 
            private Trend Trend()
            {
                return Trend(CoinValue);
            }

            private Trend Trend(decimal currentPrice)
            {
                if (!IsValid())
                {
                    return TechnicalAnalysis.Trend.Unknown;
                }

                var fall = 0;
                var stable = 0;
                var raise = 0;

                // *************************************** 
                // calcolo del trend con le medie mobili
                var ema20 = Ema20.Calculate();
                var ema50 = Ema50.Calculate();
                var sma200 = Sma200.Calculate();


                // questo indicatore è più rigoroso
                if (currentPrice > ema20 && ema20 > ema50 && ema50 > sma200)
                {
                    raise++;
                }
                else if (currentPrice < ema20 && ema20 < ema50 && ema50 < sma200)
                {
                    fall++;
                }
                else
                {
                    stable++;
                }


                // *************************************** 
                // calcolo del trend con emadiff
                if (_emadiff == 0)
                {
                    stable++;
                }
                else if (_emadiff > 0)
                {
                    raise++;
                }
                else if (_emadiff < 0)
                {
                    fall++;
                }


                // *************************************** 
                // calcolo del trend con macd
                //foreach (var item in this.MacdList)
                {
                    // macd
                    if (CurrentMacdValue == 0)
                    {
                        stable++;
                    }
                    else if (CurrentMacdValue > 0)
                    {
                        raise++;
                    }
                    else if (CurrentMacdValue < 0)
                    {
                        fall++;
                    }

                }



                // *************************************** 
                // calcolo del punteggio del poll
                var ofall = new { trend = TechnicalAnalysis.Trend.Fall, score = fall };
                var oraise = new { trend = TechnicalAnalysis.Trend.Raise, score = raise };
                var ostable = new { trend = TechnicalAnalysis.Trend.Stable, score = stable };

                var array = new[] { ofall, oraise, ostable };
                var list = array.ToList().OrderByDescending(o => o.score);

                // quanti ne ho con lo score massimo?
                var maxscore = list.ElementAt(0).score;
                if (list.ElementAt(1).score == maxscore)
                {
                    // ho due elementi con lo stesso valore
                    // nel dubbio il trend è stabile (o sconosciuto?)
                    return TechnicalAnalysis.Trend.Stable;
                }
                else
                {
                    return list.ElementAt(0).trend;
                }
            }

            

            /// <summary>
            /// Riceve la candela e fa le sue analisi
            /// </summary>
            /// <param name="candle"></param>
            private decimal _lastCoinValue = decimal.MinValue;
            public void OnNewCandle(Candle candle)
            {
                _lastCoinValue = CoinValue;
                CoinValue = candle.Close;

                this.CurrentTrend = Trend();

                EmaDiff.Add(CoinValue);
                if (EmaDiff.IsValid())
                {
                   // _emadiff = EmaDiff.Calculate();
                    _emadiff = EmaDiff.CurrentValue;
                }

                Roc.Add(CoinValue);
                if (Roc.IsValid())
                {
                    Rocdiff = Roc.CurrentValue; //Roc.Calculate();
                }

                Values.Add(CoinValue);
                if (Values.IsValid())
                {
                    
                }
                /*
                foreach (MacdItem item in this.MacdList)
                {
                    item.Macd.Add(_coinValue);
                    if (item.Macd.IsValid())
                    {
                        item.CurrentValue = item.Macd.Calculate();
                    }
                }*/
                //this.Macd122609.Calculate();

                /*
                if (StopLoss != null) StopLoss.Add(_coinValue);
                if (StopLossReverse != null) StopLossReverse.Add(1/_coinValue);
                */
                if (StopLoss != null) StopLoss.Add(CoinValue);
                if (StopLossReverse != null) StopLossReverse.Add(1 / CoinValue); 

                /*
                _macd031001.Add(_coinValue);
                if (_macd031001.IsValid())
                {
                    _macd031001Value = _macd031001.Calculate();
                }
                */
                Macd122609.Add(CoinValue);
                if (Macd122609.IsValid())
                {
                    CurrentMacdValue = Macd122609.CurrentValue; //Macd122609.Calculate();
                }

                // calcolo trend
                Ema20.Add(candle.Close);
                Ema50.Add(candle.Close);
                Sma200.Add(candle.Close);

            }


        }

        public partial class AnalyzeWithCommands : Analyze
        {
            public override TradeAction SuggestedAction(CMarket market)
            {

                /********************************************************************************
                 * CONSERVATIVE DYNAMIC
                 * 
                 ********************************************************************************/
                if (MainConfiguration.Configuration.ManagerIsConservative &&
                    MainConfiguration.Configuration.ManagerConservativeTimesPerValue > 0)
                {
                    MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest = ((market.Buy + market.Sell) / 2) * MainConfiguration.Configuration.ManagerConservativeTimesPerValue;
                }

                /********************************************************************************
                 * EVALUATION
                 * vengono esaminati i comandi ed eseguiti uno ad uno
                 ********************************************************************************/
                var result = TradeAction.Unknown;
                var commandname = "";
                foreach (var commandName in MainConfiguration.Configuration.Commands)
                {
                    commandname = commandName;
                    var command = GetCommand(commandname);
                    if (command == null)
                    {
                        // in realtà dovrebbe incazzarsi di brutto
                        continue;
                    }
                    result = command(market, result);

                    // se il comando è Strong o hold allora termina subito
                    if (result == TradeAction.Hold) goto exitloop;
                    if (result == TradeAction.StrongBuy) goto exitloop;
                    if (result == TradeAction.StrongSell) goto exitloop;
                    //if (result == TradeAction.SellStopLoss) goto exitloop;

                }
            exitloop:

                /********************************************************************************
                * LOGGING
                ********************************************************************************/
                var e = Evaluations.EvaluationManager.Instance;
                var prediction = PredictionType.Unknown;
                switch (result)
                {
                    //case TradeAction.SellStopLoss:
                    //    commandname += "--";
                    //    prediction = PredictionType.LessThan;
                    //    break;
                    case TradeAction.StrongBuy:
                        commandname += "++ ";
                        prediction = PredictionType.GreatherThan;
                        break;
                    case TradeAction.StrongSell:
                        commandname += "-- ";
                        prediction = PredictionType.LessThan;
                        break;
                    case TradeAction.Hold:
                        commandname += "=  ";
                        prediction = PredictionType.Equal;
                        break;
                    case TradeAction.Buy:
                        commandname += "+  ";
                        prediction = PredictionType.GreatherThan;
                        break;
                    case TradeAction.Sell:
                        commandname += "-  ";
                        prediction = PredictionType.LessThan;
                        break;
                }

                AnalyticsTools.Call(commandname);
                
                var now = market.CandleMaker.CurrentCandleDate;
                var candleLen = MainConfiguration.Configuration.CandleWidthInSeconds / 60;
                e.Add(commandname, new Prediction() { Date = now, Type = prediction, Value = this.CoinValue }, candleLen);

                if (result != TradeAction.Hold)
                {
                    LogTrade.Note += " " + commandname;
                }

                return result;

            }
        }
    }

