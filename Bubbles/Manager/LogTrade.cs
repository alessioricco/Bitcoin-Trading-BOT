using Bubbles.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Manager
{
    /// <summary>
    /// Classe che descrive un elemento log per i trade
    /// </summary>
    public static class LogTrade
    {
        static public DateTime CandleDate { get; set; }
        static public decimal Bid { get; set; }
        static public decimal EmaDiff { get; set; }
        static public decimal Macd { get; set; }
        static public decimal MacdStandard { get; set; }
        static public decimal Roc { get; set; }
        static public decimal RocSpeed { get; set; }
        static public Trend CurrentTrend { get; set; }
        static public Trend CurrentLongTermTrend { get; set; }
        static public TradeAction SuggestedAction { get; set; }
        static public TradeAction Action { get; set; }
        static public string Motivation { get; set; }
        static public string Note { get; set; }
        static public string Indicator { get; set; }
        static public decimal TotMoney { get; set; }
        static public decimal TotCoins { get; set; }
        static public decimal TotValue { get; set; }
        static public decimal SellAt { get; set; }
        static public decimal BuyAt { get; set; }

        //static public decimal SavedMoney { get; set; }
        //static public decimal SavedCoins { get; set; }

        public static string CsvRow()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}",

                CandleDate.ToString(CultureInfo.InvariantCulture),
                Bid.ToString(CultureInfo.InvariantCulture),

                Math.Round(EmaDiff, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(Macd, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(MacdStandard, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(Roc, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(RocSpeed, 4).ToString(CultureInfo.InvariantCulture),

                CurrentTrend.ToString(),
                CurrentLongTermTrend.ToString(),
                SuggestedAction.ToString(),
                (Action == TradeAction.Unknown || Action == TradeAction.Hold) ? "" : Action.ToString(),
                Indicator,
                Motivation,
                Note,
                Math.Round(TotMoney, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(TotCoins, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(TotValue, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(SellAt, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(BuyAt, 4).ToString(CultureInfo.InvariantCulture)

                );
        }

        public static void Init()
        {
            CandleDate = DateTime.MinValue;
            Bid = decimal.MinValue;
            EmaDiff = decimal.MinValue;
            Macd = decimal.MinValue;
            MacdStandard = decimal.MinValue;
            Roc = decimal.MinValue;
            RocSpeed = decimal.MinValue;
            CurrentTrend = Trend.Unknown;
            CurrentLongTermTrend = Trend.Unknown;
            SuggestedAction = TradeAction.Unknown;
            Action = TradeAction.Unknown;
            Motivation = "";
            Note = "";
            TotMoney = decimal.MinValue;
            TotCoins = decimal.MinValue;
            TotValue = decimal.MinValue;
            Indicator = "";
            //SellAt = 0;
            //BuyAt = 0;
            //SavedCoins = 0;
            //SavedMoney = 0;
        }

        /// <summary>
        /// esporta lo stato attuale della classe in un'istanza di oggetto
        /// </summary>
        public static object ToObjectInstance(){

            return new
            {
                CandleDate = CandleDate,
                Bid = Bid,
                EmaDiff = EmaDiff,
                Macd = Macd,
                MacdStandard = MacdStandard,
                Roc = Roc,
                RocSpeed = RocSpeed,
                CurrentTrend = CurrentTrend,
                CurrentLongTermTrend = CurrentLongTermTrend,
                SuggestedAction = SuggestedAction,
                Action = Action,
                Motivation = Motivation,
                Note = Note,
                Indicator = Indicator,
                TotMoney = TotMoney,
                TotCoins = TotCoins,
                TotValue = TotValue,
                SellAt = SellAt,
                BuyAt = BuyAt
            };
        }

    }
}
