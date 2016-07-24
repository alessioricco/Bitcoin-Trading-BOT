using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    public abstract class Indicator
    {
        protected readonly Queue<decimal> Queue = new Queue<decimal>();
        protected readonly FilterNearZero FilterNearZero = new FilterNearZero();
        public int QueueLength { get; set; }
        public Derivative Derivative = new Derivative();

        public abstract void Add(decimal value);

        public decimal ? Tolerance {get { return this.FilterNearZero.Tolerance; } set { this.FilterNearZero.Tolerance = value; }}

        protected Trend LongTermTrend()
        {
            if (Queue.Count < QueueLength) return Trend.Unknown;

            var percent = 1; //MainConfiguration.Configuration.LongTermTrendTolerance;

            var list = Queue.ToArray();

            var first = list[list.Count() * 3 / 4]; //_queue.First();
            var last = list[list.Count() - 1]; //_queue.Last();

            if (first > last * percent) return Trend.Fall;
            if (first < last * percent) return Trend.Raise;
            return Trend.Stable;
        }

        /*
        public bool IsValid()
        {
            return (Queue.Count == QueueLength);
        }
         * */
        public abstract bool IsValid();

        public abstract TradeAction SuggestedAction(decimal value, Trend moneyTrend);
        protected abstract decimal Calculate();
        public decimal CurrentValue { get; protected set; }
    }
}
