using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Sources
{

    /// <summary>
    /// Estende la classe dei bitcoin charts per restituire un'istanza che legga i dati relativi a 
    /// mtgox, usd, btc
    /// </summary>
    public class CBitcoinChartsMtgoxBtcusd : CBitcoinCharts
    {
        public CBitcoinChartsMtgoxBtcusd() : base("USD", "mtgox"){}
    }

    /// <summary>
    /// Estende la classe dei bitcoin charts per restituire un'istanza che legga i dati relativi a 
    /// mtgox, usd, btc
    /// </summary>
    public class CBitcoinChartsBtceBtcusd : CBitcoinCharts
    {
        public CBitcoinChartsBtceBtcusd() : base("USD", "btce") { }
    }

}
