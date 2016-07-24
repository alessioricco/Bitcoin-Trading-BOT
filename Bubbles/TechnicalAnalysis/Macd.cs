using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Moving Average Convergence Divergence
    /// http://www.iexplain.org/calculate-macd/
    /// </summary>
    public class Macd : Indicator
    {
        public Ema Short { get; set; }
        public Ema Long { get; set; }
        public Ema Signal { get; set; }

        public bool IsCrossUp { get; set; }
        public bool IsCrossDown { get; set; }
        //public decimal CurrentSpeed { get; set; }

        //private readonly FilterNearZero _filterNearZero = new FilterNearZero();

        public override void Add(decimal value)
        {
            this.Short.Add(value);
            this.Long.Add(value);
           // CurrentSpeed = Derivative(value);

            if (!this.Signal.IsValid())
            {
                var emaShort = Short.Calculate();
                var emaLong = Long.Calculate();
                var macd = emaShort - emaLong;
                Signal.Add(macd);
            }

            this.CurrentValue = Calculate();

            Derivative.Add(this.CurrentValue);

        }

        public override bool IsValid()
        {
            return Short.IsValid() && this.Long.IsValid() && this.Signal.IsValid();
        }

        private decimal currentmacd = 0;
        private decimal currentSignalLine = 0;
        protected override decimal Calculate()
        {

            var emaShort = Short.Calculate();
            var emaLong = Long.Calculate();
            var macd = emaShort - emaLong;
            Signal.Add(macd);
            var signalLine = Signal.Calculate();

            currentmacd = macd;
            currentSignalLine = signalLine;

            return macd-signalLine;
        }

        public override TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {
            var trend = FilterNearZero.Trend(value);
            this.IsCrossDown = FilterNearZero.CrossDown;
            this.IsCrossUp = FilterNearZero.CrossUp;

            return FilterNearZero.SuggestedAction(trend);
        }


        public static Macd Create(int shortPeriod, int longPeriod, int signalPeriod)
        {
            int queueLength = MainConfiguration.Configuration.QueueLength; 

            return new Macd()
            {
                Short = new Ema()
                {
                    NumberOfDays = shortPeriod,
                    QueueLength = queueLength
                },
                Long = new Ema()
                {
                    NumberOfDays = longPeriod,
                    QueueLength = queueLength
                },
                Signal = new Ema()
                {
                    NumberOfDays = signalPeriod,
                    QueueLength = queueLength
                }
            };

        }
    }
}
