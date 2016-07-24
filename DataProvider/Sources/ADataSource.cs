using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Sources
{
    /// <summary>
    /// classe base per le sorgenti di dati
    /// </summary>
    public abstract class ADataSource
    {
       /* /// <summary>
        /// codice aluta virtuale (es: BTC)
        /// </summary>
        public string Currency { get; protected set; }

        /// <summary>
        /// codice valuta ausiliaria (es: USD)
        /// </summary>
        public string AuxiliaryCurrency { get; protected set; }

        /// <summary>
        /// Nome del mercato
        /// </summary>
        public string MarketName { get; protected set; }


        public ADataSource( string currency, string aux, string marketName) { } */



        /// <summary>
        /// ottiene tutte le transazioni da una data ad oggi
        /// overload della funzione GetTrades(DateTime from, DateTime to) con to = DateTime.Now
        /// </summary>
        /// <param name="from">data di partenza</param>
        /// <returns></returns>
        public virtual IEnumerable<Models.TradingData.Trade> GetTrades(DateTime from) {
            return this.GetTrades(from, DateTime.Now);
        }

        /// <summary>
        /// ottiene tutte le transazioni in un range di date
        /// </summary>
        /// <param name="from">data di partenza</param>
        /// <param name="to">data di fine</param>
        /// <returns></returns>
        public abstract IEnumerable<Models.TradingData.Trade> GetTrades(DateTime from, DateTime to);

        
        
    }
}
