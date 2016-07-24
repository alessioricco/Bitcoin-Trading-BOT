using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DataProvider.Test
{
    [TestFixture]
    class Sources
    {
        /// <summary>
        /// test per ottenere le ultime transazioni da BitcoinCharts, rispetto a mtgox con dollari
        /// </summary>
        [Test]
        public void BitcoinChartsMtgoxBTCUSD_GetTrades()
        {
            try
            {

                var source = new DataProvider.Sources.CBitcoinChartsMtgoxBtcusd();
                var from = DateTime.Now.AddDays(-1);
                var res = source.GetTrades(from);

                Assert.False(res.Any(o => o.Date < from), "date sbagliate");
                Assert.False(res.Count() == 0, "nessun dato" );
            }
            catch (Exception ex)
            {
                Assert.Fail("lettura fallita, error: " + ex.Message, ex);
            }

            

        }


        /// <summary>
        /// test per ottenere le ultime transazioni da cryptocoincharts, rispetto a btc-e con dollari/ltc
        /// </summary>
        [Test]
        public void CryptocoinChartsBtceLTCUSD_GetTrades()
        {
            try
            {

                var source = new DataProvider.Sources.CCryptocoinCharts("ltc", "usd", "btc-e");
                var from = DateTime.Now.AddDays(-1);
                var res = source.GetTrades(from); //la data non serve, è un dump

                Assert.False(res.Any(o => o.Date < from), "date sbagliate");
                Assert.False(res.Count() == 0, "nessun dato");
            }
            catch (Exception ex)
            {
                Assert.Fail("lettura fallita, error: " + ex.Message, ex);
            }



        }

    }
}
