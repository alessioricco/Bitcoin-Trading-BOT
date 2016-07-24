using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Implementazione dell'indicatore Volume Price Trend
    /// http://en.wikipedia.org/wiki/Volume%E2%80%93price_trend
    /// </summary>
    public class Vpt : Indicator
    {
        /// <summary>
        /// Ultima candela analizzata
        /// </summary>
        protected Candle LastCandle { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        protected Candle CurrentCandle { get; set; }

        /// <summary>
        /// Valore precedente per questo indicatore, mi serve per calcolare il successivo
        /// </summary>
        protected decimal? PrevValue;

        /// <summary>
        /// Valore attuale dell'indicatore
        /// </summary>
        protected decimal? Value;


        public void Add(Candle candle)
        {
            //sto inserendo la stessa candela (o una precedente), annullo l'operazione
            if (this.CurrentCandle != null && this.CurrentCandle.Date >= candle.Date) return;

            //shift
            this.LastCandle = this.CurrentCandle;
            this.CurrentCandle = candle;

            this.Derivative.Add(Calculate());
           
        }

        public override void Add(decimal value)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid()
        {
            return LastCandle != null && LastCandle.Close > 0;
           // throw new NotImplementedException();
        }

        public override TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {
            if (this.IsValid() && this.Derivative.IsValid())
            {
                if (this.Derivative.IsCrossDown) return TradeAction.Sell;
                if (this.Derivative.IsCrossUp) return TradeAction.Buy;
                return TradeAction.Hold;
            }
            else {
                return TradeAction.Unknown;
            }
        }

        protected override decimal Calculate()
        {
            //posso ricalcolare il valore
            var prevClose = this.LastCandle.Close;
            var currClose = this.CurrentCandle.Close;
            var volume = this.CurrentCandle.Volume;
            var prevVPT = this.PrevValue ?? 0;

            var r = prevVPT + volume * (currClose - prevClose) / prevClose;
            this.PrevValue = r; //aggiorno il valore precedente
       
            return r;
        }
    }
}
