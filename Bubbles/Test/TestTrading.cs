using Bubbles.Analytics;
using Bubbles.Manager;
using Bubbles.Market;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.TechnicalAnalysis;

namespace Bubbles.Test
{
    /// <summary>
    /// Test che implementa l'algoritmo di trading iterando lungo i paramentri i,j,k
    /// </summary>
    public  class TestTrading : CTestBase
    {

        /// <summary>
        /// serve a mettere liste di parametri
        /// dopo un cliclo for alla scoperta delle combinazioni piu' convenienti
        /// è utile raccogliere i parametri migliori 
        /// in un elenco per successive elaborazioni
        /// </summary>
        public class TestParam
        {
            public int I { get; private set; }
            public int J { get; private set; }
            public int W { get; private set; }
            public int K { get; private set; }
            public TestParam(){}
            public TestParam(int i, int j, int k, int w)
            {
                this.I = i;
                this.J = j;
                this.K = k;
                this.W = w;
            }
        }



        public override void Run()
        {

            var tests = new List<TestUnit>();
            var testResultFileName = @"data\output\results" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";

            //TODO: caricare da configurazioni
            var testLoad = GetTestData();
            //List<TestParam> testLoad = null;

            if (testLoad == null || testLoad.Count == 0)
            {
                testLoad = new List<TestParam>() {new TestParam(0, 0, 0, 0)};
            }

            var e = Evaluations.EvaluationManager.Instance;
            foreach (var testParam in testLoad)
            {
                e.Clear();
                /********************************************************************
                 * CONFIGURAZIONE
                 *******************************************************************/
                //MainConfiguration.Configuration = new Configuration(); //configuration;
                // MainConfiguration.Configuration = Configuration.BuildFromFile("global.json", "personal.json");
                MainConfiguration.Configuration = Configuration.BuildFromDirectory(@"data/configuration");
                MainConfiguration.Configuration.Init();
                Configuration.SaveToFile(MainConfiguration.Configuration, @"data/configuration/serialized.result.json");

                // chiama la configurazione
                Customize(testParam);

                /********************************************************************
                * MANAGER
                *******************************************************************/
                var manager = Manager.Builder.Create(MainConfiguration.Configuration.ManagerClass);

                if (manager == null)
                {
                    throw new Exception("manager not configured");
                }

                
                manager.Start();

                if (MainConfiguration.Configuration.RealTimeHeartBeat && testLoad.Count == 1)
                {
                    break;
                }

                System.Console.WriteLine(manager.Market.UniqueName);

                // salvo il risultato
                var test = new TestUnit()
                {
                    Coins = manager.Market.TotCoins,
                    Money = manager.Market.TotMoney,
                    Coinvalue = manager.Market.Buy,
                    Value = manager.Market.TotValue,
                    Average = manager.Market.Average,
                    MaxCoins = manager.Market.MaxCoins,
                    MaxMoney = manager.Market.MaxMoney,
                    MaxValue = manager.Market.MaxValue,
                    Bids = manager.Market.TotBids,
                    Ask = manager.Market.TotAsks,
                    Gain = manager.Statistics.Gain,
                    i = testParam.I,
                    j = testParam.J,
                    w = testParam.W,
                    k = testParam.K
                };


                /********************************************************************
                * VISUALIZZAZIONE
                *******************************************************************/
                // stampa il risultato
                test.Print();
                if (test.Average > MainConfiguration.Configuration.StartWalletMoney)
                    // deve esserci stato almeno un trade
                {
                    if (test.Value > MainConfiguration.Configuration.StartWalletMoney)
                        // devo aver guadagnato piu' di quanto ho investito
                    {
                        // salva il risultato se ci sono stati risultati incoraggianti
                        CUtility.Log("*** GOOD");
                        tests.Add(test);
                        /*
                        // scrive il risultato su csv
                        using (var log = new StreamWriter(testResultFileName, true))
                        {
                            test.Print(log);
                            log.Flush();
                            log.Close();
                        }
                         * */
                        // scrive il risultato con codice
                        using (var log = new StreamWriter(testResultFileName + ".txt", true))
                        {
                            test.PrintCode(log);
                            log.Flush();
                            log.Close();
                        }

                        // salvo la migliore configurazione
                        var best = tests.OrderByDescending(o => o.Coins*o.Coinvalue + o.Money).FirstOrDefault();
                        if (best != null)
                        {
                            Configuration.SaveToFile(MainConfiguration.Configuration,
                                @"data/configuration/best.result.json");
                        }
                        var bestAvg = tests.OrderByDescending(o => o.Average).FirstOrDefault();
                        if (bestAvg != null)
                        {
                            Configuration.SaveToFile(MainConfiguration.Configuration,
                                @"data/configuration/bestavg.result.json");
                        }
                    }
                }

                AnalyticsTools.Print();
                e.Finalize();
            }

            /********************************************************************
            * RISULTATI
            *******************************************************************/
            if (MainConfiguration.Configuration.RealTimeHeartBeat && testLoad.Count == 1)
            {
                
            }else{

                // mostro la performance migliore
                CUtility.Log("*** BEST VALUE");
                var firstOrDefault = tests.OrderByDescending(o => o.Coins*o.Coinvalue + o.Money).FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.Print();

                CUtility.Log("*** BEST AVERAGE");
                var orDefault = tests.OrderByDescending(o => o.Average).FirstOrDefault();
                if (orDefault != null)
                    orDefault.Print();
            }

        }

        public override void Customize(TestParam testParam)
        {
            throw new NotImplementedException();
        }

        public override List<TestParam> GetTestData()
        {
            throw new NotImplementedException();
        }
    }
}
