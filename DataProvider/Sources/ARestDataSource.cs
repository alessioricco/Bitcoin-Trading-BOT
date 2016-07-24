using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataProvider.Sources
{
    /// <summary>
    /// Classe base per i datasource basati su chiamate rest
    /// </summary>
    public abstract class ARestDataSource : ADataSource
    {


        /// <summary>
        /// ottiene la url per le compravendite da una data
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract string GetTradesUrl(DateTime from);

        /// <summary>
        /// metodo di deserializzazione delle response
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected abstract IEnumerable<Models.TradingData.Trade> DeserializeData(string data);


        public override IEnumerable<Models.TradingData.Trade> GetTrades(DateTime from, DateTime to)
        {
            //istanzia un http client
            var client = new RestSharp.RestClient();
            //inizializza la collezione
            var trades = new List<Models.TradingData.Trade>();
            //puntatore dell'ultima data
            var last = from;

            //eseguo le chimate rest fino a quando non copro il range di date
            try
            {
                while (last < to)
                {
                    Console.WriteLine("get trades... " + last.ToString(CultureInfo.InvariantCulture) + " --> " + to.ToString(CultureInfo.InvariantCulture));

                    var url = GetTradesUrl(last);

                    var request = new RestSharp.RestRequest(url);

                    var response = client.Execute(request);

                    var content = this.DeserializeData(response.Content).ToList();

                    trades.AddRange(content);

                    var newLast = content.Max(o => o.Date);

                    //potrei aver raggiunto l'ultima transazione disponibile
                    last = newLast == last ? to : newLast;
                    Thread.Sleep(1000);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.InnerException.Message);
            }
            return trades.OrderBy(o => o.Date).ToList() ;
            


        }
    }
}
