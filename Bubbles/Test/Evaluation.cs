using System.Collections.Generic;
using Bubbles.Manager;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis; //metto l'alias perchè prima o poi sporterò su DataProvider
using System;
using System.Globalization;
using Bubbles.TechnicalAnalysis.Analyzer;
using DataSource = Bubbles.Market;

namespace Bubbles.Test
{
    class Evaluation : CTestBase
    {
        public override void Run()
        {

            /********************************************************************
                 * CONFIGURAZIONE
                 *******************************************************************/
            MainConfiguration.Configuration = new Configuration(); //configuration;
            MainConfiguration.Configuration.Init();

            //inizializzo il valutatore
            var eval = Evaluations.EvaluationManager.Instance;
            eval.Clear();

            var market = Market.CMarketEvaluator.Create();
            market.Init();
           
            
            var _lastCandleId = (uint)0;

            while(market.GetTicker() && !market.Stop ){
                if (!market.CandleMaker.Analyze.IsValid())
                {
                    // il candlemaker non puo' fare previsioni
                   // Log("** Analyzer not yet ready");
                    continue;
                }

                if (_lastCandleId != market.CandleMaker.CurrentCandleId)
                { 
                    //mi basta generare il suggerimento
                    var s15m = market.CandleMaker.Analyze.SuggestedAction(market);
                    var s1h = market.CandleMakerHourly.Analyze.SuggestedAction(market);
                    eval.Evaluate(market.CandleMaker.CurrentCandleDate, market.Buy);
                    _lastCandleId = market.CandleMaker.CurrentCandleId;
                }


            }

            //chiudo la sessione
            eval.Finalize();


        }

        public override void Customize(TestTrading.TestParam testParam)
        {
            return;
        }

        public override List<TestTrading.TestParam> GetTestData()
        {
            return null;
        }


        protected virtual DataSource.ISimulatedDataSource GetDataSource()
        {
            DataSource.ISimulatedDataSource simulatedDataSource;


            var dataSource = new DataSource.SimulateddataSourceTextFile()
                    {
                        FileName = @"data\candles-2013-15m.csv"
                    };
            simulatedDataSource = dataSource;


            return simulatedDataSource;
        }

    }
}




namespace Bubbles.Market
{



    internal class CMarketEvaluator : CMarketSimulator
    {

        

        public CMarketEvaluator() : base() { }


        public override string DoOrder(CUtility.OrderType type, decimal? cost, decimal amount, ref string orderId,
            bool marketOrder)
        {
            //we dont do orders
            throw new NotImplementedException();


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
        }

       



        public static CMarket Create()
        {
            
            var analyzer15m =  new EvaluationAnalyze()
                        {
                            Name = "15m",
                            timeUnit = 15,
                            //MacdList = MainConfiguration.Configuration.MacdList,
                            // EmaDiff = MainConfiguration.Configuration.EmaDiff, // ema buoni: 10,20 10,21,  8,20
                            EmaDiff = EmaDiff.Create(MainConfiguration.Configuration.EmaDiff[0], MainConfiguration.Configuration.EmaDiff[1]), // ema buoni: 10,20 10,21,  8,20
                            StopLoss = new CStopLoss()
                            {
                                Percent = MainConfiguration.Configuration.StopLossPercent
                            }
                        };
            var analyzer1h = new EvaluationAnalyze()
            {
                Name = "1h",
                timeUnit = 60,
                //MacdList = MainConfiguration.Configuration.MacdList,
                // EmaDiff = MainConfiguration.Configuration.EmaDiff, // ema buoni: 10,20 10,21,  8,20
                EmaDiff = EmaDiff.Create(MainConfiguration.Configuration.EmaDiff[0], MainConfiguration.Configuration.EmaDiff[1]), // ema buoni: 10,20 10,21,  8,20
              
                StopLoss = new CStopLoss()
                {
                    Percent = MainConfiguration.Configuration.StopLossPercent
                }
            };
            var candleMaker = new CandleMaker()
                {
                    GenerateFile = false, //MainConfiguration.Configuration.GenerateCandleFile,
                    CandleWidth = MainConfiguration.Configuration.CandleWidthInSeconds,
                    Analyze = analyzer15m
                };

            var market = new CMarketEvaluator()
            {
                UseMtGox = MainConfiguration.Configuration.MarketSimulatorUseMtgox,
                UseFile = MainConfiguration.Configuration.MarketSimulatorUseCandleFile,
                FileName = MainConfiguration.Configuration.MarketSimulatorCandleFileName,
                DeltaTime = MainConfiguration.Configuration.MarketSimulatorSqLiteDeltaTime,
                StartTime = MainConfiguration.Configuration.MarketSimulatorSqLiteStartDate,
                EndTime = MainConfiguration.Configuration.MarketSimulatorSqLiteEndDate,
                CandleMaker = candleMaker,
                CandleMakerHourly = new CandleMaker()
                {
                    GenerateFile = false,
                    CandleWidth = 60 * 15 * 4,
                    Analyze = analyzer1h
                }
            };
            return market;
        }

    }
}

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Analizzatore per la valutazione
    /// </summary>
    public class EvaluationAnalyze : Analyze
    {
        //il nome dell'analizzatore, utile per loggare più analizzatori
        public string Name;

        //time unit (larghezza candele)
        public int timeUnit;

        public override TradeAction SuggestedAction(CMarket market)
        {
            /********************************************************************************
             * COSTANTI (evaluator)
             ********************************************************************************/
            
            //var timeUnit = 15;

            //data corrente (servirebbe la data del ticker, ma vabbè...)
            var now = market.CandleMaker.GetLastCandles()[0].Date;

            var lastCandle = new
            {
                m15 = market.CandleMaker.GetLastCandles()[0],
                h1 = market.CandleMakerHourly.GetLastCandles()[0],
            };

            //valutatore
            var E = Evaluations.EvaluationManager.Instance;

            //formatta i nomi degli indicatori
            Func<string, string> N = (o =>
            {
                return string.Format("{0}[{1}]", o, this.Name);
            });



            /********************************************************************************
             * COSTANTI
             ********************************************************************************/

            var currentTrend = market.CandleMaker.Analyze.CurrentTrend; //Trend();
            var roclimit = MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit;
            var macdlimit = MainConfiguration.Configuration.AlarmStrongSellMacdLimit;

            /********************************************************************************
             * STOP LOSS
             ********************************************************************************/
            if (MainConfiguration.Configuration.EnableStopLoss)
            {
                if (StopLoss.IsSell(this.CoinValue))
                {

                    if (currentTrend == TechnicalAnalysis.Trend.Fall)
                    {
                        StopLoss.Reset();
                        StopLoss.SellCounter++;

                        //EVAL: se uso lo stoploss, sto prevedendo che il prezzo scenda, anche di poco
                        E.Add(N("stoploss"), CreatePrediction(now, TradeAction.StrongSell), timeUnit);


                    }
                }
            }

            if (!IsValid())
            {
                //AnalyticsTools.Call("notyetready");
                return TradeAction.Unknown;
            }




            /********************************************************************************
            * ROC (non ancora usato)
            ********************************************************************************/
            if (true)
            {
                var rocSpeed = Roc.Derivative.CurrentSpeed;
                if (Rocdiff > roclimit && rocSpeed <= 0 && currentTrend == TechnicalAnalysis.Trend.Fall)
                {
                    //LogTrade.Note = "inversione: vendere";
                    //EVAL: se dico di vendere, vuol dire che il prezzo scenderà, anche di poco
                    E.Add(N("-(rocdiff;rocspeed)"), CreatePrediction(now, TradeAction.Sell), timeUnit);

                    if (CurrentMacdValue > macdlimit)
                    {
                        //EVAL: questo è un rafforzativo, quindi mi aspetto un grande ribasso
                        E.Add(N("++(rocdiff;rocspeed;Macd122609)"), CreatePrediction(now, TradeAction.StrongSell), timeUnit);

                        //return TradeAction.StrongSell;
                    }
                }
                if (Rocdiff < -roclimit && rocSpeed <= 0 && currentTrend == TechnicalAnalysis.Trend.Raise)
                {
                    //LogTrade.Note = "inversione: comprare";
                    //EVAL: se dico di comprare, vuol dire che il prezzo salirà, anche di poco
                    E.Add(N("+(rocdiff;rocspeed)"), CreatePrediction(now, TradeAction.Buy), timeUnit);

                    if (CurrentMacdValue < -macdlimit)
                    {
                        //EVAL: questo è un rafforzativo, quindi mi aspetto un grande rialzo
                        E.Add(N("++(rocdiff;rocspeed;Macd122609)"), CreatePrediction(now, TradeAction.StrongBuy), timeUnit);
                        // return TradeAction.StrongBuy;

                    }
                }
            }
            if (true)
            {
                // se si mette a true, questo garantisce 12k e 15btc (l'altro pezzo sul roc va messo in tuning
                if (Rocdiff > roclimit && currentTrend == TechnicalAnalysis.Trend.Fall)
                {
                    //LogTrade.Note = "inversione: vendere";
                    //EVAL: se dico di vendere, vuol dire che il prezzo scenderà, anche di poco
                    E.Add(N("-(rocdiff)"), CreatePrediction(now, TradeAction.Sell), timeUnit);

                    if (CurrentMacdValue > 2)
                    {
                        //EVAL: questo è un rafforzativo, quindi mi aspetto un grande ribasso
                        E.Add(N("--(rocdiff;Macd122609)"), CreatePrediction(now, TradeAction.StrongSell), timeUnit);
                        //return TradeAction.StrongSell;
                    }
                }
                if (Rocdiff < -roclimit && currentTrend == TechnicalAnalysis.Trend.Raise)
                {
                    //LogTrade.Note = "inversione: comprare";
                    //EVAL: se dico di comprare, vuol dire che il prezzo salirà, anche di poco
                    E.Add(N("+(rocdiff;rocspeed)"), CreatePrediction(now, TradeAction.Buy), timeUnit);

                    if (CurrentMacdValue < -2)
                    {
                        //EVAL: questo è un rafforzativo, quindi mi aspetto un grande rialzo
                        E.Add(N("++(rocdiff;rocspeed;Macd122609)"), CreatePrediction(now, TradeAction.StrongBuy), timeUnit);
                        // return TradeAction.StrongBuy;
                    }

                }

                /********************************************************************************
                * ALLARMI
                ********************************************************************************/

                var emadiffValue = EmaDiff.CurrentValue; //EmaDiff.Calculate(); // potrebbe essere inutile (lo abbiamo già)
                var macdValue = this.CurrentMacdValue; // potrebbe essere inutile (lo abbiamo già)
                //var roclimit = 5.0M;
                if (Math.Sign(emadiffValue) == Math.Sign(macdValue))
                {
                    var limit = MainConfiguration.Configuration.AlarmStrongSellBuyLimit; // occorre capire che valori sono i migliori


                    if ((emadiffValue < -limit && macdValue < -limit))
                    {
                        //LogTrade.Note = ("suggestion: (emadiffValue < -limit && macdValue < -limit)");
                        // AnalyticsTools.Call("ema;macd;limit;-");
                        //return TradeAction.StrongSell;

                        //EVAL: 
                        E.Add(N("--(ema;macd;limit;)"), CreatePrediction(now, TradeAction.StrongSell), timeUnit);

                    }

                    if (emadiffValue > limit && macdValue > limit)
                    {
                        //LogTrade.Note = ("suggestion:  (emadiffValue > limit && macdValue > limit)");
                        // AnalyticsTools.Call("ema;macd;limit;+");
                        // return TradeAction.StrongBuy;

                        //EVAL: 
                        E.Add(N("++(ema;macd;limit;)"), CreatePrediction(now, TradeAction.StrongBuy), timeUnit);

                    }

                    if (emadiffValue < -1 && macdValue < -1)
                    {
                        //LogTrade.Note = ("suggestion:  (emadiffValue < -1 && macdValue < -1)");
                        // AnalyticsTools.Call("ema;macd;-1");
                        // return TradeAction.Sell;

                        //EVAL: 
                        E.Add(N("-(ema;macd;-1;)"), CreatePrediction(now, TradeAction.Sell), timeUnit);

                    }

                    if (emadiffValue > 1 && macdValue > 1)
                    {
                        //  AnalyticsTools.Call("ema;macd;+1");
                        //LogTrade.Note = ("suggestion:  (emadiffValue > 1 && macdValue > 1)");
                        //return TradeAction.Buy;

                        //EVAL: 
                        E.Add(N("+(ema;macd;+1;)"), CreatePrediction(now, TradeAction.Buy), timeUnit);

                    }

                }



                var poll = new Poll();
                /********************************************************************************
                 * MACD
                 ********************************************************************************/
                //foreach (MacdItem item in this.MacdList)
                {
                    poll.Add(Macd122609.SuggestedAction(CurrentMacdValue, currentTrend));
                    CUtility.Log(string.Format("MACD   {0}", Math.Round(CurrentMacdValue, 4).ToString(CultureInfo.InvariantCulture)));
                }

                /********************************************************************************
                * EMADIFF
                ********************************************************************************/
                var emadiffSuggestion = EmaDiff.SuggestedAction(CurrentEmaDiff, currentTrend);
                poll.Add(emadiffSuggestion);
                CUtility.Log(string.Format("EMADIFF {0}", Math.Round(CurrentEmaDiff, 4).ToString(CultureInfo.InvariantCulture)));


                /********************************************************************************
                * DECISION
                ********************************************************************************/
                //LogTrade.Note = "poll";

                var result = poll.Result();

                if (true)
                {
                    // questo sistema di sicurezza non genera troppi problemi, ma non aumenta il reddito
                    // pero' aumenta la media dei redditi

                    // sistema di sicurezza da testare
                    const decimal securitylimit = 1.5M; // va bene cosi'


                    if (result == TradeAction.Buy && market.Buy >= securitylimit * LogTrade.BuyAt && LogTrade.BuyAt > 0 &&
                        currentTrend == TechnicalAnalysis.Trend.Raise)
                    {
                        //  AnalyticsTools.Call("expensive");
                        //  return TradeAction.Hold;

                        //EVAL: in realtà, "expensive" scommette sul fatto che può comprare a meno
                        E.Add(N("expensive"), new Evaluations.Prediction()
                        {
                            Date = now,
                            Type = Evaluations.PredictionType.LessThan,
                            Value = this.CoinValue * (1 - 0.01M)
                        }, timeUnit);

                    }

                    if (result == TradeAction.Sell && market.Sell <= securitylimit * LogTrade.SellAt && LogTrade.SellAt > 0 &&
                        currentTrend == TechnicalAnalysis.Trend.Fall)
                    {
                        // AnalyticsTools.Call("cheap");
                        //  return TradeAction.Hold;

                        //EVAL: in realtà, "cheap" scommette sul fatto che può vendere a più
                        E.Add(N("cheap"), new Evaluations.Prediction()
                        {
                            Date = now,
                            Type = Evaluations.PredictionType.GreatherThan,
                            Value = this.CoinValue * (1 + 0.01M)
                        }, timeUnit);
                    }
                }

                if (result != TradeAction.Hold)
                {
                    // AnalyticsTools.Call("poll " + result.ToString());
                }

                //return result;

                //EVAL: globale, ovvero cosa fa l'algoritmo
                // E.Add("GLOBAL", CreatePrediction(now, result), timeUnit);

                return TradeAction.Unknown;
            }
        }


        protected Evaluations.Prediction CreatePrediction(DateTime now, TradeAction suggested)
        {

            var p = new Evaluations.Prediction() { Date = now };
            var v = this.CoinValue;
            //percentuali di variazione del prezzo
            var variationPercents = new
            {
                xsmall = 0.005M,
                small = 0.08M,
                medium = 0.012M,
                large = 0.015M,
                xlarge = 0.02M
            };






            switch (suggested)
            {
                case TradeAction.Buy:
                    p.Type = Evaluations.PredictionType.GreatherThan;
                    p.Value = v * (1 + variationPercents.small);
                    break;
                case TradeAction.StrongBuy:
                    p.Type = Evaluations.PredictionType.GreatherThan;
                    p.Value = v * (1 + variationPercents.large);
                    break;
                case TradeAction.Hold:
                    p.Type = Evaluations.PredictionType.Unknown;
                    p.Value = 0;
                    break;
                case TradeAction.Sell:
                    p.Type = Evaluations.PredictionType.LessThan;
                    p.Value = v * (1 - variationPercents.small);
                    break;
                //case TradeAction.SellStopLoss:
                //    p.Type = Evaluations.PredictionType.LessThan;
                //    p.Value = v * (1 - variationPercents.medium); //lo stoploss prevede che il prezzo scenda, altrimenti non avrebbe senso
                //    break;
                case TradeAction.StrongSell:
                    p.Type = Evaluations.PredictionType.LessThan;
                    p.Value = v * (1 + variationPercents.large);
                    break;
                case TradeAction.Unknown: p.Type = Evaluations.PredictionType.Unknown;
                    p.Value = 0;
                    break;
            }

            return p;

        }

    }

}