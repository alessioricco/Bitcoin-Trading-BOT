using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Analytics;
using Bubbles.Evaluations;
using Bubbles.Manager;
using Bubbles.Market;
using DataProvider.Models.TradingData;

namespace Bubbles.TechnicalAnalysis.Analyzer
{
    
    /// <summary>
    /// Questo troncone implementa i Delegati che costruiscono il modello previsionale
    /// </summary>
    public abstract partial class Analyze
    {
        private decimal _tempStopLossMaxValue = decimal.MinValue;
        protected TradeAction EvalStopLossValue(CMarket market, TradeAction result)
        {
            var now = market.CandleMaker.CurrentCandleDate;
            var candleLen = MainConfiguration.Configuration.CandleWidthInSeconds / 60;
           // var E = Evaluations.EvaluationManager.Instance;

            /********************************************************************************
            * STOP LOSS SUL VALORE
            * lo stop loss viene fatto su fasce di valori per evitare l'effetto oscillazione
            ********************************************************************************/
            decimal stopLossPerc = 1.0M + 0.4M; //MainConfiguration.Configuration.TestNumber/100M;
            decimal[] stopLossLevels = { 0, 100, 250, 500, 1000, 2500, 5000, 10000, 12500, 15000, 20000, 25000, 50000 };

            if (market.TotCoins > 0)
            {

                var value = market.TotValue * stopLossPerc;
                var level = 100 * 1000M;

                var stopLossLevelIndex = 0;
                while (stopLossLevelIndex < stopLossLevels.Count() && stopLossLevels[stopLossLevelIndex] < value)
                {
                    level = stopLossLevels[stopLossLevelIndex++];
                }

                if (level < value && value < _tempStopLossMaxValue &&
                    _tempStopLossMaxValue == market.MaxValue)
                {
                    //stopLossLevelIndex++;
                    _tempStopLossMaxValue = decimal.MinValue;
                   // AnalyticsTools.Call("stoploss value");
                    //E.Add("stoploss value", new Prediction() { Date = now, Type = PredictionType.LessThan, Value = this.CoinValue }, candleLen);
                    return TradeAction.StrongSell;
                }
                else
                {
                    _tempStopLossMaxValue = (market.TotValue > _tempStopLossMaxValue)
                        ? _tempStopLossMaxValue = market.TotValue
                        : _tempStopLossMaxValue;
                }
            }
            else
            {
                _tempStopLossMaxValue = decimal.MinValue;
            }
            return result; //TradeAction.Unknown;
        }

        protected TradeAction EvalStopLoss(CMarket market, TradeAction result)
        {
            if (MainConfiguration.Configuration.EnableStopLoss)
            {
                //var now = market.CandleMaker.CurrentCandleDate;
                //var candleLen = MainConfiguration.Configuration.CandleWidthInSeconds / 60;
                //var E = Evaluations.EvaluationManager.Instance;

                if (Math.Abs(market.TotCoins) < 0.01M)
                {
                    // sono a zero
                    StopLoss.Reset();
                }
                else
                    if (StopLoss.IsSell(this.CoinValue))
                    {
                        var currentTrend = Trend();
                        if (currentTrend == TechnicalAnalysis.Trend.Fall)
                        {
                            StopLoss.Reset();
                            StopLoss.SellCounter++;
                            //AnalyticsTools.Call("stoploss");
                            //E.Add("stoploss", new Prediction() { Date = now, Type = PredictionType.LessThan, Value = this.CoinValue }, candleLen);
                            return TradeAction.StrongSell;
                        }
                    }
            }
            return result; //TradeAction.Unknown;
        }

        protected TradeAction EvalStopLossUpDown(CMarket market, TradeAction result)
        {
            var gain = 0.04M;

            if (Math.Abs(market.TotCoins) < 0.01M)
            {
                this.StopLoss.Reset();
            }
            else if (this.StopLoss.IsSell(market.TotValue))
            {
                //if (currentTrend == TechnicalAnalysis.Trend.Fall)
                {
                    this.StopLoss.Reset();
                    this.StopLoss.SellCounter++;
                   // AnalyticsTools.Call("stoploss");
                    if (this.CoinValue * (1 + gain) > LogTrade.SellAt)
                    {
                        return TradeAction.StrongSell;
                    }
                    else
                    {
                        return TradeAction.Sell;
                    }
                }
            }


            if (Math.Abs(market.TotMoney) < 0.01M)
            {
                this.StopLossReverse.Reset();
            }
            else if (this.StopLossReverse.IsSell(1 / this.CoinValue))
            {
                //if (currentTrend == TechnicalAnalysis.Trend.Raise)
                {
                    this.StopLossReverse.Reset();
                    this.StopLossReverse.SellCounter++;
                    //AnalyticsTools.Call("stoploss rev");
                    if (this.CoinValue * (1 - gain) < LogTrade.BuyAt)
                    {
                        return TradeAction.StrongBuy;
                    }
                    else
                    {
                        return TradeAction.Buy;
                    }
                }
            }

            //result = TradeAction.Hold;
            return result;
        }

        protected TradeAction EvalAlarmIndicator(CMarket market, TradeAction result)
        {
            //var now = market.CandleMaker.CurrentCandleDate;
            //var candleLen = MainConfiguration.Configuration.CandleWidthInSeconds / 60;
            //var E = Evaluations.EvaluationManager.Instance;

            var emadiffValue = this.CurrentEmaDiff; //EmaDiff.Calculate(); // potrebbe essere inutile (lo abbiamo già)
            var macdValue = this.CurrentMacdValue; // potrebbe essere inutile (lo abbiamo già)
            //var roclimit = 5.0M;
            if (Math.Sign(emadiffValue) == Math.Sign(macdValue))
            {
                var limit = MainConfiguration.Configuration.AlarmStrongSellBuyLimit; // occorre capire che valori sono i migliori


                if ((emadiffValue < -limit && macdValue < -limit))
                {
                    //LogTrade.Note = ("suggestion: (emadiffValue < -limit && macdValue < -limit)");
                    //AnalyticsTools.Call("ema;macd;limit;-");
                    //E.Add("ema;macd;limit;-", new Prediction() { Date = now, Type = PredictionType.LessThan, Value = this.CoinValue }, candleLen);
                    return TradeAction.StrongSell;
                }

                if (emadiffValue > limit && macdValue > limit)
                {
                    //LogTrade.Note = ("suggestion:  (emadiffValue > limit && macdValue > limit)");
                    //AnalyticsTools.Call("ema;macd;limit;+");
                    //E.Add("ema;macd;limit;+", new Prediction() { Date = now, Type = PredictionType.GreatherThan, Value = this.CoinValue }, candleLen);
                    return TradeAction.StrongBuy;
                }
                /*
                if (emadiffValue < -1 && macdValue < -1)
                {
                    //LogTrade.Note = ("suggestion:  (emadiffValue < -1 && macdValue < -1)");
                    AnalyticsTools.Call("ema;macd;-1");
                    return TradeAction.Sell;
                }

                if (emadiffValue > 1 && macdValue > 1)
                {
                    AnalyticsTools.Call("ema;macd;+1");
                    //LogTrade.Note = ("suggestion:  (emadiffValue > 1 && macdValue > 1)");
                    return TradeAction.Buy;
                }
                */
            }
            return result; //TradeAction.Unknown;
        }

        protected TradeAction EvalPoll(CMarket market, TradeAction result)
        {

            var poll = new Poll();
            this.Macd122609.Tolerance = MainConfiguration.Configuration.Tolerance;
            this.EmaDiff.Tolerance = MainConfiguration.Configuration.Tolerance;

            var macdSuggestion = this.Macd122609.SuggestedAction(this.CurrentMacdValue, this.CurrentTrend);
            poll.Add(macdSuggestion);

            var emadiffSuggestion = EmaDiff.SuggestedAction(CurrentEmaDiff, this.CurrentTrend);
            poll.Add(emadiffSuggestion);

            var moneySuggestion = this.Values.SuggestedAction(CoinValue, this.CurrentTrend);
            poll.Add(moneySuggestion);

            return poll.Result();
           
        }

        private TradeAction EvalDerivative(CMarket market)
        {
            var result2 = TradeAction.Unknown;
            var emaUp = this.EmaDiff.Derivative.IsCrossUp;
            var emaDown = this.EmaDiff.Derivative.IsCrossDown;
            var macdUp = this.Macd122609.Derivative.IsCrossUp;
            var macdDown = this.Macd122609.Derivative.IsCrossDown;

            LogTrade.MacdStandard = this.EmaDiff.Derivative.CurrentSpeed;
            LogTrade.Roc = this.Macd122609.Derivative.CurrentSpeed;
            LogTrade.RocSpeed = 0;

            if (emaUp && macdUp)
            {
                result2 = TradeAction.Buy;
            }
            else
                if (emaDown && macdDown)
                {
                    result2 = TradeAction.Sell;
                }
                else
                {
                    result2 = TradeAction.Hold;
                }
            return result2;
        }

        protected TradeAction EvalDerivative(CMarket market, TradeAction result)
        {
            var newresult = EvalDerivative(market);
            return EnforceDecision(result, newresult);
        }

        protected TradeAction EvalExpensiveCheap(CMarket market, TradeAction result)
        {
            if (result != TradeAction.Hold)
            {

                //var currentTrend = Trend();

                // sistema di sicurezza da testare
                decimal securitylimit = MainConfiguration.Configuration.SecurityLimit;

                // la velocità serve a modificare il limite per cui si valuta se un rezzo è expensive o cheap
                var speed = Math.Abs(this.CurrentMacdValue); //Math.Abs(Values.Derivative.CurrentSpeed);

                var gap = (LogTrade.BuyAt - market.Buy) / (1 + speed);
                if (result == TradeAction.Buy && market.Buy + gap >= securitylimit * LogTrade.BuyAt && LogTrade.BuyAt > 0 &&
                     this.CurrentTrend == TechnicalAnalysis.Trend.Raise)
                {

                    //LogTrade.Note += " expensive ";
                    //AnalyticsTools.Call("expensive");
                    return TradeAction.Hold;

                }

                gap = (LogTrade.SellAt - market.Sell) / (1 + speed);
                if (result == TradeAction.Sell && market.Sell + gap <= securitylimit * LogTrade.SellAt &&
                    LogTrade.SellAt > 0 &&
                     this.CurrentTrend == TechnicalAnalysis.Trend.Fall)
                {

                    //LogTrade.Note += " cheap ";
                    //AnalyticsTools.Call("cheap");
                    return TradeAction.Hold;

                }

            }
            return result;
        }

        private TradeAction EnforceDecision(TradeAction oldresult, TradeAction confirmResult)
        {
            if (oldresult == confirmResult)
            {
                if (oldresult == TradeAction.Buy)
                {
                    //AnalyticsTools.Call("poll buy; der buy");
                    //E.Add("poll buy; der buy", new Prediction() { Date = now, Type = PredictionType.GreatherThan, Value = this._coinValue }, candleLen);
                    return TradeAction.StrongBuy;
                }
                if (oldresult == TradeAction.Sell)
                {
                    //AnalyticsTools.Call("poll sell; der sell");
                    //E.Add("poll sell; der sell", new Prediction() { Date = now, Type = PredictionType.LessThan, Value = this._coinValue }, candleLen);
                    return TradeAction.StrongSell;
                }

            }
            return oldresult;
        }

        protected TradeAction EvalCandles(CMarket market)
        {
            var bullish = 0;
            var bearish = 0;
            var candles = market.CandleMaker.GetLastCandles();
            //var candles = market.CandleMakerHourly.GetLastCandles();
            for (var i = 0; i < 1; i++)
            {
                var c = new[] { candles[i], candles[i + 1], candles[i + 2] };
                var trend = CandleAnalyzer.Evaluate(c);
                if (trend == StackTrend.Bullish)
                {
                    // AnalyticsTools.Call("candle bull");
                    bullish++;
                }
                else
                    if (trend == StackTrend.Bearish)
                    {
                        // AnalyticsTools.Call("candle bear");
                        bearish++;
                    }
            }
            //var candleSuggestion = TradeAction.Hold;
            var bearbullishratio = 0.6M;
            if (bullish > bearish * bearbullishratio)
            {
                //LogTrade.Note += " BULL ";
                return TradeAction.Buy;
            }
            else if (bearish < bullish * bearbullishratio)
            {
                //LogTrade.Note += " BEAR ";
                return TradeAction.Sell;
            }
            return TradeAction.Hold;
        }

        protected TradeAction EvalCandles(CMarket market, TradeAction result)
        {
            var newresult = EvalCandles(market);
            return EnforceDecision(result, newresult);
        }

        protected  delegate TradeAction Evaluator(CMarket market, TradeAction result);

        readonly Hashtable _delegates = new Hashtable();

        void InitDelegates()
        {
            _delegates.Add("stoplossvalue", (Evaluator)EvalStopLossValue);
            _delegates.Add("stoploss", (Evaluator)EvalStopLoss);
            _delegates.Add("stoplossupdw", (Evaluator)EvalStopLossUpDown);
            _delegates.Add("alarm", (Evaluator)EvalAlarmIndicator);
            _delegates.Add("poll", (Evaluator)EvalPoll);
            _delegates.Add("derivative", (Evaluator)EvalDerivative);
            _delegates.Add("candles", (Evaluator)EvalCandles);
            _delegates.Add("expensivecheap", (Evaluator)EvalExpensiveCheap);
        }

       protected  Evaluator GetCommand(string commandName)
        {
            if (! _delegates.ContainsKey(commandName))
            {
                return null;
            }

            return (Evaluator)_delegates[commandName];
        }

      

    }
}
