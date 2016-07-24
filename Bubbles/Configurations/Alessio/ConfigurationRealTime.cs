using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Analytics;
using Bubbles.Evaluations;
using Bubbles.Manager;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis;
using Bubbles.Test;
using DataProvider.Models.TradingData;

namespace Bubbles.Configurations
{
    public static class Builders
    {
        public static TestTrading BuildTrading()
        {
            return new CTradingAlessioClassic();
        }

        public static Analyze BuildAnalyze()
        {
            return new AnalyzeWithCommands()
            {
                //MacdList = MainConfiguration.Configuration.MacdList,
                EmaDiff = MainConfiguration.Configuration.EmaDiff, // ema buoni: 10,20 10,21,  8,20
                StopLoss = new CStopLoss()
                {
                    Percent = MainConfiguration.Configuration.StopLossPercent
                }
            };
        }
    }
}

namespace Bubbles.Test
{


    /// <summary>
    /// in questo test uso i valori standard
    /// </summary>
    public class CTradingAlessioClassic : TestTrading
    {

        public override void Customize(TestParam testParam)
        {

            /***************************************
            * MODIFICHE ALLA CONFIGURAZIONE STANDARD
            **************************************/
            // filenames
            //const string filename = @"data\mtgox-hourly-candles-USD.csv"; // 3,4,2 macd
            //const string filename = @"data\mtgoxcandlesMonth-15m";  //macd 2,4,-5
            //const string filename = @"data\mtgoxcandles10days-5mins";
            //const string filename = @"data\mtgoxcandles2013-6hrs"; //3,4,-3 macd
            //const string filename = @"data\mtgoxcandles2013-12hrs";
            //const string filename = @"data\mtgoxcandles2days-1m";
            //const string filename = @"data\mtgoxcandlesAug2013-15m";
            //const string filename = @"data\mtgox-augdec2013-15m.csv"; //0,3,-5
            //const string filename = @"data\mtgox-complete2013.csv";
            //const string filename = @"data\candles-2013-5m.csv";
            //const string filename = @"data\candles-2013-jan-apr-5m.csv";

            //const string filename = @"data\candles-btce-complete-15m.csv";
            const string filename = @"data\candles-2013-jun-dec-5m.csv";
            //const string filename = @"data\candles-2013-1m.csv";
            //const string filename = @"data\candles-septdec2013-15m.csv";
            //const string filename = @"data\candles-mtgox-jan2014-15m";
            //const string filename = @"data\candles-jan2014-15m";

            // indicatore Ema con rapporto
            MainConfiguration.Configuration.StartWalletCoins = 14M;// 0.33M;//0.33M * 1;
            MainConfiguration.Configuration.StartWalletMoney = 0;
            MainConfiguration.Configuration.QueueLength = 200;

            MainConfiguration.Configuration.EmaDiff = EmaDiff.Create(10, 21);
            /*
            MainConfiguration.Configuration.MacdList = new List<MacdItem>()
                {
                    new MacdItem() {Macd = Macd.Create(12, 26, 9)} 
                };
            */

            MainConfiguration.Configuration.GenerateOutputTradeHistory = true;

            // effettuo un test senza greedyness
            var gain = 15M/10M;// 1.2M; //2;
            if (false)
            {
                
                MainConfiguration.Configuration.GreedyWithLongTermTrend = false;
                MainConfiguration.Configuration.GreedyWithCoinsGain = 1 + 0.1M*gain;
                MainConfiguration.Configuration.GreedyWithMoneyGain = 1 + 0.1M*gain;
            }
            if (true)
            {
                // greedy baby ;-)
                MainConfiguration.Configuration.GreedyWithCoinsGain = 1 + 0.1M*gain;
                MainConfiguration.Configuration.GreedyWithMoneyGain = 1 + 0.1M*gain;
            }

            MainConfiguration.Configuration.MarketSimulatorCandleFileName = filename;
            MainConfiguration.Configuration.CandleWidthInSeconds = 60 * 15;

            MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit = 4;
            MainConfiguration.Configuration.AlarmStrongSellMacdLimit = 8;

            // aggiunti per fare un test con tanti valori ma in realtà sono parametri da non utlizare normalmente
            MainConfiguration.Configuration.EnableStopLoss = true;
            MainConfiguration.Configuration.StopLossPercent = 0.1M * 3;
            MainConfiguration.Configuration.ZeroFilterSequenceLen = 10;

           // MainConfiguration.Configuration.TestNumber = testParam.I;
           // MainConfiguration.Configuration.TestNumber = testParam.I;

            MainConfiguration.Configuration.Tolerance = 0.001M * 2; //{1..4} meglio 2

            MainConfiguration.Configuration.SecurityLimit = 1.0M + 0.1M*1; //*2;
            
            //MainConfiguration.Configuration.UndervalueAllowed = true;
            //MainConfiguration.Configuration.UndervalueTolerance = 0M;
            //MainConfiguration.Configuration.AlarmStrongSellBuyLimit = 1.0M + 0.1M*7; // 8 è meglio testParam.I;

            // disabilito il noise per vedere se cambiano i numeri
            MainConfiguration.Configuration.MarketSimulatorEnableNoise = false;

            MainConfiguration.Configuration.Commands = new string[] { "stoplossvalue", "stoploss", "alarm", "poll", "derivative", "expensivecheap" };


            //MainConfiguration.Configuration.ManagerIsConservative = true;
            //MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest = 10000;

            if (false)
            {
                // gioco con i valori reali
                MainConfiguration.Configuration.QueueLength = 200;
                MainConfiguration.Configuration.StartWalletCoins = 0.33M;
                MainConfiguration.Configuration.StartWalletMoney = 0;
                MainConfiguration.Configuration.GenerateCandleFile = true;
                MainConfiguration.Configuration.GenerateOutputTradeHistory = true;
                MainConfiguration.Configuration.MarketSimulatorUseMtgox = true;
                MainConfiguration.Configuration.MarketSimulatorUseCandleFile = false;
                MainConfiguration.Configuration.RealTimeHeartBeat = true;
                MainConfiguration.Configuration.RealTimeHeartBeatEveryMinutes = 1;
            }

            MainConfiguration.Configuration.Market = CMarketSimulator.Create();
            MainConfiguration.Configuration.Manager = new CManagerTrading()
            {
                
                Conservative = MainConfiguration.Configuration.ManagerIsConservative, // cerca sempre di investire la quantità di capitale
                PercCapital = MainConfiguration.Configuration.ManagerInvestCapitalPercent, // quantità di capitale da investire
                Market = MainConfiguration.Configuration.Market //market
            };

        }

        public override List<TestParam> GetTestData()
        {
            
            var testLoad = new List<TestParam>();
            
            // verifico come al variare dell'undervalue possa variare il risultato
            for (var i = 10; i <= 20; i++)
                for (var j = 0; j <= 4; j++)
                    //for (var k = 5; k <= 50; k += 5)
                        testLoad.Add(new TestParam(i, j, 0, 0));
            return testLoad;
            
            return null;
        }
    }

}









