using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    public class EmaDiff : Indicator
    {
        public Ema Short { get; set; }
        public Ema Long { get; set; }

        public bool IsCrossUp { get; set; }
        public bool IsCrossDown { get; set; }

        

        public override void Add(decimal value)
        {
            this.Short.Add(value);
            this.Long.Add(value);
            this.CurrentValue = Calculate();
            Derivative.Add(this.CurrentValue);
        }

        public override bool IsValid()
        {
            return Short.IsValid() && this.Long.IsValid();
        }

        protected override decimal Calculate()
        {
            var emashort = this.Short.Calculate(); // 598.308
            var emalong = this.Long.Calculate(); //615.754
            
            var result = 100*(emashort - emalong)/((emashort + emalong)/2); // -2.874

            //Console.WriteLine(string.Format("EMA: {0} {1} --> {2}",Math.Round(emashort,2),Math.Round(emalong,2),Math.Round(result,2)));

            return result;
        }

        public override TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {
            var trend = FilterNearZero.Trend(value);

            this.IsCrossDown = FilterNearZero.CrossDown;
            this.IsCrossUp = FilterNearZero.CrossUp;

            return FilterNearZero.SuggestedAction(trend);
        }

        public static EmaDiff Create(int shortPeriod, int longPeriod)
        {
             int queueLength = MainConfiguration.Configuration.QueueLength;

            return new EmaDiff()
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
                }
            };

        }

    }
}
