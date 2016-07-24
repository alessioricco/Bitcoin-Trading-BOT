using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataProvider.Extensions;

namespace DataProvider.Sources
{
    /// <summary>
    /// Classe che permette l'accesso ai dati di bitcoin charts
    /// http://bitcoincharts.com/about/markets-api/
    /// </summary>
   public class CBitcoinCharts : ARestDataSource
    {

        /// <summary>
        /// codice aluta virtuale (es: BTC)
        /// </summary>
        public string Currency { get {
            return "BTC"; //sono solo bitcoin su bicoincharts
        } }

        /// <summary>
        /// codice valuta ausiliaria (es: USD)
        /// </summary>
        public string AuxiliaryCurrency { get; protected set; }

        /// <summary>
        /// Nome del mercato
        /// </summary>
        public string MarketName { get; protected set; }



        public CBitcoinCharts( string auxcurrency, string marketname) {
            this.MarketName = marketname;
            
            this.AuxiliaryCurrency = auxcurrency;
        }


        protected override string GetTradesUrl(DateTime from)
        {
            return string.Format("http://api.bitcoincharts.com/v1/trades.csv?symbol={0}{1}&start={2}", 
                this.MarketName, 
                this.AuxiliaryCurrency,  
                from.ToUnixTime());

            /*return string.Format("v1/trades.csv?symbol={0}{1}&start={2}", 
                this.MarketName, 
                this.AuxiliaryCurrency,  
                from.ToUnixTime());*/
        }


        protected override IEnumerable<Models.TradingData.Trade> DeserializeData(string data)
        {
            //i dati sono in formato csv separati da virgola
            foreach (var line in data.Split('\n')) {

                Console.Write(".");

                //elemento
                var item = line.Split(',');
                var date = Utils.UnixTime.ConvertToDateTime(item[0]);
                var price = Convert.ToDecimal(item[1],CultureInfo.InvariantCulture);
                var amount = Convert.ToDecimal(item[2],CultureInfo.InvariantCulture);

                yield return new Models.TradingData.Trade() { 
                    Amount = amount ,
                    AuxiliaryCurrency = this.AuxiliaryCurrency,
                    Currency = this.Currency,
                    Date = date,
                    MarketName = this.MarketName,
                    Price = price,
                    Note = "importato il " + DateTime.Now
                    
                };

            
            }
            Console.WriteLine();
        }



    }
}
