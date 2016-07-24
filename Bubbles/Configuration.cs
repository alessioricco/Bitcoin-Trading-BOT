using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Extensions;
using Bubbles.Manager;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Bubbles
{
    public  class Configuration
    {
        // WALLET
        public decimal StartWalletMoney { get; set; }
        public decimal StartWalletCoins { get; set; }

        // SIMULATOR
        public bool MarketSimulatorUseMtgox { get; set; }
        public bool MarketSimulatorUseCandleFile { get; set; }
        public string MarketSimulatorCandleFileName { get; set; }
        public bool MarketSimulatorUseCandleFileReversed { get; set; }

        public bool MarketSimulatorEnableNoise { get; set; }
        public decimal MarketSimulatorNoisePercent { get; set; }

        // SQLLITE
        public DateTime MarketSimulatorSqLiteStartDate { get; set; }
        public DateTime MarketSimulatorSqLiteEndDate { get; set; }
        public long MarketSimulatorSqLiteDeltaTime { get; set; }

        // GREEDY
        public bool GreedyWithMoney { get; set; }
        public decimal GreedyWithMoneyGain { get; set; }
        public bool GreedyWithCoins { get; set; }
        public decimal GreedyWithCoinsGain { get; set; }
        public bool GreedyWithLongTermTrend { get; set; }

        // LONG TERM TREND
        public decimal LongTermTrendTolerance { get; set; }

        // CANDLEMAKER
        public bool GenerateCandleFile { get; set; }
        public int CandleWidthInSeconds { get; set; }

        // ANALYZER
        //public List<MacdItem> MacdList { get; set; } 
        public int[] EmaDiff { get; set; }
        public bool EnableStopLoss { get; set; }
        public decimal StopLossPercent { get; set; }
        public decimal AlarmStrongSellBuyLimit { get; set; }

        public decimal AlarmStrongSellBuyRocLimit { get; set; }
        public decimal AlarmStrongSellMacdLimit { get; set; }

        // MANAGER
        public bool ManagerIsConservative { get; set; }
        public decimal ManagerConservativeMaxMoneyToInvest { get; set; }
        public decimal ManagerConservativeTimesPerValue { get; set; }
        public decimal ManagerInvestCapitalPercent { get; set; }

        // VERBOSITY
        public bool Verbose { get; set; }
        public bool GenerateOutputTradeHistory { get; set; }

        // HEARTBEAT
        public bool RealTimeHeartBeat { get; set; }
        public int RealTimeHeartBeatEveryMinutes { get; set; }
        public int RealTimeTickEveryHeartBeats { get; set; }

        // ZERO FILTER
        public int ZeroFilterSequenceLen { get; set; }
        public decimal ZeroFilterSequenceChangeSignPercent { get; set; }

        // ZERO FILTER DERIVATIVE
        public int ZeroFilterSequenceLenDerivative { get; set; }

        // QUEUE LENGTH
        public int QueueLength { get; set; }

        // SECURITY LIMIT
        public decimal SecurityLimit { get; set; }
        public decimal SecurityThreshold { get; set; }

        // MARKET
        public string MarketClass { get; set; }

        // MANAGER
        public string ManagerClass { get; set; }

        // ANALYZER
        public string AnalyzerClass { get; set; }

        public decimal Tolerance { get; set; }

        public int TestNumber { get; set; }

        //test data
        //public decimal[][] TestData { get; set; }

        // la sessionid serve per la produzione dei nomi di files
        public long SessionId { get; set; }

        // è la quantità massima ammissibile di perdita rispetto al valore iniziale durante un trade
        public bool UndervalueAllowed { get; set; }
        public decimal UndervalueTolerance { get; set; }

        public string[] Commands { get; set; }

        //CLOUD
        public string CloudHost { get; set; } //url del servizio cloud di reportistica
        
        //API MTGOX
        public string MtGoxApi { get; set; }
        public string MtGoxSecret { get; set; }
        //API BTCE
        public string BtceApi { get; set; }
        public string BtceSecret { get; set; }
        public void Init()
        {


           

        }

        public Configuration()
        {

            // soldi con cui inizio
            StartWalletMoney = 100;
            StartWalletCoins = 0;

            // se verbose, scriverà molto log
            Verbose = true;
            // genera il file outpus.csv con l'elenco dei trade fatti
            GenerateOutputTradeHistory = true;

            // Number of elements for the zero filtering
            ZeroFilterSequenceLen = 10;
            // a quale percentuale della sequenza inizio a cercare il cambio segno ?
            ZeroFilterSequenceChangeSignPercent = 0.25M;
            // zero filter per le derivate
            ZeroFilterSequenceLenDerivative = 5;

            // se true applica un timer (adatto a mtgox), se false un ciclo for (adatto ai files)
            RealTimeHeartBeat = false;
            // ogni quanti minuti emette un heartbeat
            RealTimeHeartBeatEveryMinutes = 5;
            // ogni quanti heartbeat viene generato un tick
            RealTimeTickEveryHeartBeats = 1;

            // applico l'algoritmo di avidità alle vendite
            GreedyWithMoney = true;
            // applico l'algoritmo di avidità con un certo guadagno % (se 1.0 significa qualsiasi guadagno)
            GreedyWithMoneyGain = 1.2M;
            // applico l'algoritmo di avidità agli acquisti
            GreedyWithCoins = false;
            // applico l'algoritmo di avidità con un certo guadagno % (se 1.0 significa qualsiasi guadagno)
            GreedyWithCoinsGain = 1.2M;
            // applico l'algoritmo di avidità su Money o Coin in base al trend a lungo termine
            GreedyWithLongTermTrend = true;

            LongTermTrendTolerance = 1.0M;

            // il simulatore funzionerà in tempo reale con i dati di mtgox
            MarketSimulatorUseMtgox = false;
            // il simulatore funzionerà leggendo un file di candele
            MarketSimulatorUseCandleFile = true;
            MarketSimulatorCandleFileName = @"data\candles-2013-15m.csv";
            MarketSimulatorUseCandleFileReversed = false;

            MarketSimulatorEnableNoise = false;
            MarketSimulatorNoisePercent = 0.05M;

            // il simulatore, su sqllite, inizia dalla seguente data
            MarketSimulatorSqLiteStartDate = new DateTime(2013, 1, 1, 0, 0, 0);
            // il simulatore, su sqllite, terminerà alla seguente data
            MarketSimulatorSqLiteEndDate = new DateTime(2014, 1, 1, 0, 0, 0);
            // va alla ricerca del successimo momento (next su sqlite)
            MarketSimulatorSqLiteDeltaTime = 60 * 5;

            // voglio che venga generato il file delle candele
            GenerateCandleFile = false;
            // ampiezza delle candele generate internamente
            CandleWidthInSeconds = 60 * 15;

            // Lunghezza delle code usate per i calcoli di ema e simili
            QueueLength = 150;

            // true se si vuole abilitare l'algoritmo di stoploss
            EnableStopLoss = false;
            // se l'algoritmo è abilitato valuta quando far scattare lo stop loss
            StopLossPercent = 0.3M;
            // Limite per attivare lo strong buy o lo strong sell
            AlarmStrongSellBuyLimit = 1.0M + 7M / 10.0M; //1.7M,

            AlarmStrongSellBuyRocLimit = 2.0M;
            AlarmStrongSellMacdLimit = 2.0M;

            // (se false viene investito tutto il capitale, altrimenti una percentuale)
            ManagerIsConservative = false;
            // se il manager è conservativo, quanti soldi investire ogni volta
            ManagerConservativeMaxMoneyToInvest = 1000;
            // percentuale del capitale investito dal manager (1.0 significa tutto) ogni volta
            ManagerInvestCapitalPercent = 1.0M;
            // utilizzo un limite relativo al valore medio attuale invece che un valore fisso
            ManagerConservativeTimesPerValue = 18M;

            // valore general pourpose
            TestNumber = 0;

            // tolleranza impiegata per valutare se un valore è zero oppure no (usata in macd)
            Tolerance = 0.002M;

            // valore di limite per l'acquisto o la vendita di coin oltre il quale si ha un blocco per prezzo eccessivo o troppo basso
            SecurityLimit = 1.2M;

            // la sessionId serve per la produzione di filename
            SessionId = DateTime.Now.Ticks;

            // Quantità massima di perdita di valore ammissible rispetto al valore iniziale
            UndervalueAllowed = true;
            UndervalueTolerance = 0M;

            Commands = new string[] { "stoplossvalue", "stoploss", "alarm", "poll", "derivative", "expensivecheap" };

            //classe di manager che verrà eseguita
            ManagerClass= "CManagerTrading";
            CloudHost = "";
            AnalyzerClass = "AnalyzeWithCommands";
            MarketClass = "CMarketSimulator";
            EmaDiff = new[] {10, 21};

            // API
            MtGoxApi = "mtgox api";
            MtGoxSecret = "mtgox secret";
            BtceApi = "btce api";
            BtceSecret = "btce secret";
        }

        #region metodi builder
            /*
             */ 

        /// <summary>
        /// legge tutti i file di una cartella e compila la configurazione
        /// se la directory non esiste, ritorna la configurazione base
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Configuration BuildFromDirectory(string dir = "conf") {

            if (System.IO.Directory.Exists(dir))
            {
                var files = System.IO.Directory.GetFiles(dir);
                return BuildFromFile(files);
            }
            else { 
                //ritorno al configurazione base
                return new Configuration();
            }

        }

        /// <summary>
        /// data una lista di file json, compila la configurazione
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static Configuration BuildFromFile(params string[] files) {

            //configurazione base
            var baseConf = new Configuration();

            //lettura da file
            Func<string, JObject> ReadFile = (file =>
            {
                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        var json = System.IO.File.ReadAllText(file);
                       // json = @"{""StartWalletMoney"": 0}";
                       // var conf = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                       
                        
                        var conf = Newtonsoft.Json.Linq.JObject.Parse(json);
                        return conf;
                    }

                }
                catch (Exception)
                {
                    //secondo me se fallisce la lettura di un file, è un problema
                    throw;
                }
                return null;

            });


            //estende una configurazione
            Func<object, JObject, object> Extend = null;
            Extend = ( ( object conf, JObject j) => {
                var t = conf.GetType();
          
                foreach(var field in j){
                    var p = t.GetProperty(field.Key);
                    if ( p != null && field.Value != null)
                    {
                        var t1 = p.PropertyType;
                        object v;
                        if (field.Value.GetType() ==typeof( JObject))
                        {
                            var o = Activator.CreateInstance(t1);
                             v = Extend(o, (JObject)field.Value);
                        }
                        else if (field.Value.GetType() == typeof(JArray))
                        {
                            var a = (JArray)field.Value;
                            var t2 = t1.GetElementType();
                            dynamic k;
                            switch (t1.Name) { 
                                case "Int32[]":
                                     k = a.Select(x => (Int32)Convert.ChangeType(x, typeof(Int32))).ToArray();
                                     v = Convert.ChangeType(k, t1);
                                    break;
                                case "Decimal[][]":
                                    var  k1 = a.Select(x => (Decimal[])Convert.ChangeType(x, typeof(Decimal[]))).ToArray();
                                    k = k1.Select(x => (Decimal)Convert.ChangeType(x, typeof(Decimal))).ToArray();
                                    v = Convert.ChangeType(k, t1);
                                    break;
                                case "String[]":
                                    k = a.Select(x => (string)Convert.ChangeType(x, typeof(string))).ToArray();
                                    v = Convert.ChangeType(k, t1);
                                    break;
                                default:
                                    v = null;
                                    break;                                   
                            }
                          /*  var k = a.Select(x => Convert.ChangeType(x, t2)).ToArray();
                            v = Convert.ChangeType(k, t1); */
                        }
                        else {
                             v = Convert.ChangeType(field.Value, t1);
                        }
                        

                        p.SetValue(conf, v);
                    }
                }

                return conf;
            });
 
           
            //eseguo l'operazione su tutti i files
            foreach (var file in files.Where(f=> ! f.Contains(".result.json"))) {

                var newConf = ReadFile(file);

                baseConf = (Configuration)Extend(baseConf, newConf);

            }

            return baseConf;


        }

        /// <summary>
        /// Serializza la configurazione e la salva su file
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="filename"></param>
        public static void SaveToFile(Configuration conf, string filename) {

            
            string json = JsonConvert.SerializeObject(conf, Formatting.Indented);

            var file = new FileInfo(filename);

            if (!file.Directory.Exists) {
                file.Directory.Create();
            }
            File.WriteAllText(filename , json);

        
        }


        #endregion metodi builder

    }

    public static class MainConfiguration
    {
        public static Configuration Configuration { get; set; }
    }

}
