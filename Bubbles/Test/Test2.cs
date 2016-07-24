using Bubbles.Manager;
using Bubbles.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Test
{
    class Test2 : CTestBase
    {

        public override  void Run() {

            //creo il file per l'esecuzione corrente
            var filename = string.Format(@"log\{0}_{1}.txt", "Test2", DateTime.Now.ToString("ddMMyyyy_hhmm"));
         //   System.IO.File.Create(filename);


            //apro lo stream writer
         //   using (System.IO.StreamWriter sw = new System.IO.StreamWriter(@"..\..\log\mtgoxcandlesMonth-15m", true))
            {
               
            



            //lettura delle configurazioni
                var configuration = new ConfigurationHoldAndBuy()
                {
                    Gain = (decimal)0.05, // 1%
                    //HeartBeat = (decimal)0.001, // 1m
                    MaxBubbles = 1, //(j+1),
                    MaxCoinsPerBubble = (decimal)(0.6),
                    MinimumCoinsPerBubble = (decimal)0.01,
                    //MaxMoney = 100,
                    //Tick = 1, //w+1,
                    //TickDynamic = false
                };
        
            //parametri MACD
            var arr = new int[]{0,1,2,3,4,5,6,7,8,9,10};
            var param = from i in arr
                    from j in arr
                    from k in arr
                    select new { i, j, k };

            //iterazione lungo i parametri
            param.ToList().ForEach(p =>
            {
                //generazione del market
                var market = new CMarketSimulator(); //(15, 17, 12 + p.i, 20 + p.j, 5 + p.k);

                //istanzio il manager
                var manager = new Manager.CManagerHoldBuy()
                {
                    ConfigurationHoldAndBuy = configuration,
                    Market = market
                };//(configuration, market, true);

                //lancio il test

                // salvo il risultato
                var test = new TestUnit()
                {
                    Bubbles = configuration.MaxBubbles,
                    Coins = market.TotCoins,
                    Money = market.TotMoney,
                    Gain = configuration.Gain,
                    Maxcoinsperbubble = configuration.MaxCoinsPerBubble,
                    Coinvalue = market.Bid,
                    //Ticks = configuration.Tick,
                    Value = market.TotValue,
                    Average = market.Average,
                    i = p.i,
                    j = p.j,
                    //w=w,
                    k = p.k

                };

                //stampo il test
                CUtility.Log("*** TEST");
              //  test.PrintOnFile(sw);
                //test.PrintOnConsole();
                test.Print();


            });

            }

        }

    }
}
