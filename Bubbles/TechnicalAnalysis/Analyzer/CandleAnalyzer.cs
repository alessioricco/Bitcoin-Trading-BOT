using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Manager;

namespace Bubbles.TechnicalAnalysis.Analyzer
{
    public class CandleAnalyzer
    {
        static bool IsBearishTrend(Candle[] candles)
        {
            return candles[1].Color == Candle.CandleColor.Red && candles[2].Color == Candle.CandleColor.Red;
        }

        static bool IsBullishTrend(Candle[] candles)
        {
            return candles[1].Color == Candle.CandleColor.Green && candles[2].Color == Candle.CandleColor.Green;
        }

         static StackTrend IsMarubozu(Candle[] candles)
        {
            // la candela piu' recente è la 0

            // ribassista
                if (candles[0].IsMarubozuBearish())
                {
                    // verifico se c'è un trend ribassista
                    if (IsBearishTrend(candles))
                    {
                        //LogTrade.Note += " IsMarubozuBearish ";
                        return StackTrend.Bearish;
                    }
                }

                // rialzista
                if (candles[0].IsMarubozuBullish())
                {
                    // verifico se c'è un trend rialzista
                    if (IsBullishTrend(candles))
                    {
                        //LogTrade.Note += " IsMarubozuBullish ";
                        return StackTrend.Bullish;
                    }
                }
            return StackTrend.Stable;
        }

         static StackTrend IsEngulfing(Candle[] candles)
        {
            const int engulfingOffset = 1;
            const int engulfingBig = 0 + engulfingOffset;
            const int engulfingSmall = 1 + engulfingOffset;
            // bullish engulfing pattern ?
            // http://www.investopedia.com/terms/b/bullishengulfingpattern.asp

            var engulfingPatternValid = true;
            for (var i = 0; i < engulfingOffset; i++)
            {
                if (candles[i].Color != Candle.CandleColor.Green)
                {
                    engulfingPatternValid = false;
                }
            }
            if (engulfingPatternValid)
            {
                if (candles[engulfingBig].Color == Candle.CandleColor.Green)
                {
                    if (candles[engulfingSmall].Color == Candle.CandleColor.Red)
                    {
                        if (candles[engulfingBig].Close > candles[engulfingSmall].High)
                        {
                            if (candles[engulfingBig].Open < candles[engulfingSmall].Low)
                            {
                                // c'è un pattern
                                //LogTrade.Note += " engulfing bullish ";
                                //AnalyticsTools.Call("engulfing bullish");
                                //return TradeAction.Buy;
                                return StackTrend.Bullish;
                            }
                        }
                    }
                }
            }

            //http://www.investopedia.com/terms/b/bearishengulfingp.asp
            engulfingPatternValid = true;
            for (var i = 0; i < engulfingOffset; i++)
            {
                if (candles[i].Color != Candle.CandleColor.Red)
                {
                    engulfingPatternValid = false;
                }
            }
            if (engulfingPatternValid)
            {
                if (candles[engulfingBig].Color == Candle.CandleColor.Red)
                {
                    if (candles[engulfingSmall].Color == Candle.CandleColor.Green)
                    {
                        if (candles[engulfingBig].Close < candles[engulfingSmall].Low)
                        {
                            if (candles[engulfingBig].Open > candles[engulfingSmall].High)
                            {
                                // c'è un pattern
                                //LogTrade.Note += " engulfing bullish ";
                                //AnalyticsTools.Call("engulfing bearish");
                                return StackTrend.Bearish;
                            }
                        }
                    }
                }
            }
            return StackTrend.Stable;
        }

         static StackTrend IsDoji(Candle[] candles)
        {
            var dojiIndex = 0;
            if (candles[dojiIndex++].Color == Candle.CandleColor.Red)
            {
                //if (lastCandles[1].IsDoji() && lastCandles[2].IsDoji() && lastCandles[3].IsDoji())
                var dojiCount = 0;
                while (dojiIndex < candles.Count() && candles[dojiIndex++].IsDoji())
                {
                    dojiCount++;
                }
                ;
                // ci vogliono almeno 3 doji
                if (dojiCount >= 3 && dojiIndex < candles.Count() &&
                    candles[dojiIndex].Color == Candle.CandleColor.Green)
                {
                    // il prezzo scende
                    //LogTrade.Note += " doji bear ";
                    //AnalyticsTools.Call("doji bear");
                    //return TradeAction.Sell;
                    return StackTrend.Bearish;
                }
            }

            dojiIndex = 0;
            if (candles[dojiIndex++].Color == Candle.CandleColor.Green)
            {
                //if (lastCandles[1].IsDoji() && lastCandles[2].IsDoji() && lastCandles[3].IsDoji())
                var dojiCount = 0;
                while (dojiIndex < candles.Count() && candles[dojiIndex++].IsDoji())
                {
                    dojiCount++;
                }
                ;
                // ci vogliono almeno 3 doji
                if (dojiCount >= 3 && dojiIndex < candles.Count() &&
                    candles[dojiIndex].Color == Candle.CandleColor.Red)
                {
                    // il prezzo sale
                    //AnalyticsTools.Call("doji bull");
                    //LogTrade.Note += " doji bull ";
                    //return TradeAction.Buy;
                    return StackTrend.Bullish;
                }
            }
            return StackTrend.Stable;
        }

         static bool IsEveningStar(Candle[] candles)
        {
            if (candles[0].Color== Candle.CandleColor.Red)
                if (candles[1].IsDoji())
                    if (candles[2].Color == Candle.CandleColor.Green)
                        return true;
            return false;
        }

         static bool IsMorningStar(Candle[] candles)
        {
            if (candles[0].Color == Candle.CandleColor.Green)
                if (candles[1].IsDoji())
                    if (candles[2].Color == Candle.CandleColor.Red)
                        return true;
            return false;
        }

         static bool IsBullish(Candle[] candles)
        {
            return IsBearishTrend(candles) && (candles[0].IsInvertedHammer() || candles[0].IsHammer());
        }

         static bool IsBearish(Candle[] candles)
        {
            return IsBullishTrend(candles) && (candles[0].IsShootingStar() || candles[0].IsHangingMan());
        }


        public static StackTrend Evaluate(Candle[] candles)
        {
            var bearish = 0;
            var bullish = 0;

            var stockTrend = StackTrend.Stable;

            /*
            if (CandleAnalyzer.IsBullish(candles)) bullish++;
            if (CandleAnalyzer.IsMorningStar(candles)) bullish++;

            if (CandleAnalyzer.IsBearish(candles)) bearish++;
            if (CandleAnalyzer.IsEveningStar(candles)) bearish++;
            */
            /*
            
            stockTrend = CandleAnalyzer.IsEngulfing(candles);
            if (stockTrend == StackTrend.Bearish)
            {
                bearish++;
            }
            else if (stockTrend == StackTrend.Bullish)
            {
                bullish++;
            }
             */
            
            stockTrend = CandleAnalyzer.IsDoji(candles);
            if (stockTrend == StackTrend.Bearish)
            {
                bearish++;
            }
            else if (stockTrend == StackTrend.Bullish)
            {
                bullish++;
            }
            /*
            stockTrend = CandleAnalyzer.IsMarubozu(candles);
            if (stockTrend == StackTrend.Bearish)
            {
                bearish++;
            }
            else if (stockTrend == StackTrend.Bullish)
            {
                bullish++;
            }
            */

            if (((double)bearish / (double)(bearish + bullish)) > 0.75)
            {
                return StackTrend.Bearish;
            }
            if (((double)bullish / (double)(bearish + bullish)) > 0.75)
            {
                return StackTrend.Bullish;
            }
            return StackTrend.Stable;
        }

    }
}
