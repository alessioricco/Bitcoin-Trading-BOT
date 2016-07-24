using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Market;

namespace Bubbles.TechnicalAnalysis
{

    public enum TradeAction
    {
        Unknown,
        Sell,
        StrongSell,
        //SellStopLoss,
        Hold,
        StrongBuy,
        Buy
    };

    public enum Trend
    {
        Unknown,
        Fall,
        Stable,
        Raise
    };

    public enum StackTrend
    {
        Bearish,
        Bullish,
        Stable
    }

    public class MacdItem
    {
        internal decimal CurrentValue { get; set; }
        public Macd Macd { get; set; }
    }


}
