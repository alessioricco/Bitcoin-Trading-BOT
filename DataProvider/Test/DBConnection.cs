using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DataProvider.Test
{
    
    /// <summary>
    /// raccolta di test per le connessioni a db
    /// </summary>
    [TestFixture]
    class DBConnection
    {

        /// <summary>
        /// testa la connessione al db TradingData
        /// </summary>
        [Test]
        public void TradingDataDBConnection()
        {
            using (var db = new Models.TradingData.TradingDataDB())
            {
                var q =
                    from c in db.Trade
                    select c;

                foreach (var c in q) ;
                   // Console.WriteLine(c);
                
            }
                    
        }

    }
}
