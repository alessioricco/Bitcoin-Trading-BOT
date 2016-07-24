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
    /// Classe che permette l'accesso ai dati di crypto coin charts - rubati ;) 
    /// http://www.cryptocoincharts.info/v2/data/period/ltc/usd/btc-e/alltime/1h
    /// </summary>
   public class CCryptocoinCharts : ARestDataSource
    {

        /// <summary>
        /// codice aluta virtuale (es: BTC)
        /// </summary>
        public string Currency {  get; protected set; }

        /// <summary>
        /// codice valuta ausiliaria (es: USD)
        /// </summary>
        public string AuxiliaryCurrency { get; protected set; }

        /// <summary>
        /// Nome del mercato
        /// </summary>
        public string MarketName { get; protected set; }



        public CCryptocoinCharts(string currency, string auxcurrency, string marketname)
        {
            this.MarketName = marketname;

            this.Currency = currency;
            this.AuxiliaryCurrency = auxcurrency;
        }


        protected override string GetTradesUrl(DateTime from)
        {
           
            return string.Format("http://www.cryptocoincharts.info/v2/data/period/{0}/{1}/{2}/alltime/1h",
                this.Currency.ToLower(),
                this.AuxiliaryCurrency.ToLower(),
                this.MarketName.ToLower());

            /*return string.Format("v1/trades.csv?symbol={0}{1}&start={2}", 
                this.MarketName, 
                this.AuxiliaryCurrency,  
                from.ToUnixTime());*/
        }


        protected override IEnumerable<Models.TradingData.Trade> DeserializeData(string data)
        {
            //i dati sono in formato array di array
            var global = Newtonsoft.Json.Linq.JArray.Parse(data);

           
            foreach (var i in global) {

                yield return new Models.TradingData.Trade()
                {
                   /* Amount = amount,
                    AuxiliaryCurrency = this.AuxiliaryCurrency,
                    Currency = this.Currency,
                    Date = Convert.ChangeType(i[0], typeof(DateTime)),
                    MarketName = this.MarketName,
                    Price = price,
                    Note = "importato il " + DateTime.Now*/

                };
            }


           
        }



    }
}
