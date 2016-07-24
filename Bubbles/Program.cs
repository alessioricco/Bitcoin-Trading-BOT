using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bubbles.Manager;
using Bubbles.Market;
using Bubbles.SQLite;
using Bubbles.Test;

namespace Bubbles
{

    class TestUnit
    {

        public decimal Money { get; set; }
        public decimal Coins { get; set; }
        public decimal Coinvalue { get; set; }

        public decimal Average { get; set; }
        public decimal Value { get; set; }

        public int Bids { get; set; }
        public int Ask { get; set; }
        public decimal MaxCoins { get; set; }
        public decimal MaxMoney { get; set; }
        public decimal MaxValue { get; set; }

        public decimal i { get; set; }
        public decimal j { get; set; }
        public decimal w { get; set; }
        public decimal k { get; set; }

        public decimal Gain { get; set; }

        public void Print()
        {

            CUtility.Log("money     : " + Money.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("coins     : " + Coins.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("coinvalue : " + Coinvalue.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("value     : " + Value.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("i         : " + i.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("j         : " + j.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("w         : " + w.ToString(CultureInfo.InvariantCulture));
            CUtility.Log("k         : " + k.ToString(CultureInfo.InvariantCulture));
        }

        public void Print(StreamWriter log)
        {


            log.Write(Money.ToString(CultureInfo.InvariantCulture));
            log.Write("," + Coins.ToString(CultureInfo.InvariantCulture));
            log.Write("," + Coinvalue.ToString(CultureInfo.InvariantCulture));
            log.Write("," + Value.ToString(CultureInfo.InvariantCulture));
            log.Write("," + i.ToString(CultureInfo.InvariantCulture));
            log.Write("," + j.ToString(CultureInfo.InvariantCulture));
            log.Write("," + w.ToString(CultureInfo.InvariantCulture));
            log.WriteLine("," + k.ToString(CultureInfo.InvariantCulture));
            
        }

        public void PrintCode(StreamWriter log)
        {
            log.WriteLine(@"testLoad.Add(new TestParam({0}, {1}, {2}, {3})); // v:{4} avg:{5} b:{6} a:{7} mv:{8} mm:{9} mc:{10} g:{11}", i, j, k, w, Math.Round(Value, 4).ToString(CultureInfo.InvariantCulture), Math.Round(Average, 4).ToString(CultureInfo.InvariantCulture), Bids, Ask, Math.Round(MaxValue, 4).ToString(CultureInfo.InvariantCulture), Math.Round(MaxMoney, 4).ToString(CultureInfo.InvariantCulture), Math.Round(MaxCoins, 4).ToString(CultureInfo.InvariantCulture), Math.Round(Gain, 2).ToString(CultureInfo.InvariantCulture));
        }

        public void Log(StreamWriter logstream)
        {
            
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
           
            try
            {

                // Questo fa partire tutto il processo
                //Builders.BuildTrading().Run();
                var t = new TestTradingCustomized(); //TestTrading();
                t.Run();

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                Console.WriteLine(MainConfiguration.Configuration.MarketSimulatorUseMtgox
                    ? "*** Let's make some buck$ !!!!"
                    : "*** PRESS ANY KEY");

                Console.ReadLine();
            }
        }
    }
}
