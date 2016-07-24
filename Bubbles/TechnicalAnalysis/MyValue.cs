using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Questo indicatore va ad esaminare il valore complessivo del patrimonio
    /// e suggerisce quando potrebbe essere opportuno vendere
    /// </summary>
    public class MyValue : Indicator
    {
        public override void Add(decimal value)
        {
            this.Queue.Enqueue(value);
            if (Queue.Count() > this.QueueLength)
            {
                this.Queue.Dequeue();
            }


            this.Derivative.Add(Calculate());

        }

        public override bool IsValid()
        {
            //throw new NotImplementedException();
           return this.Queue.Count() >= this.QueueLength;
        }

        public override TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {

            var trend = FilterNearZero.Trend(value);
            if (trend == Trend.Fall && FilterNearZero.CrossDown || trend == Trend.Raise && FilterNearZero.CrossUp)
            {
                return FilterNearZero.SuggestedAction(trend);
            }
            
           return  TradeAction.Hold;
        }

        protected override decimal Calculate()
        {
            //throw new NotImplementedException();
            return this.Queue.Peek();
        }
    }
}
