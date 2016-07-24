using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 //    calculation (based on tick/day):
//  EMA = Price(t) * k + EMA(y) * (1 – k)
//  t = today, y = yesterday, N = number of days in EMA, k = 2 / (N+1)
TradingMethod.prototype.calculateEMA = function(type) {
  var price = _.last(this.candles.close);

  var k = 2 / (settings[type] + 1);
  var ema, y;

  var current = _.size(this.candles.close);

  if(current === 1)
    // we don't have any 'yesterday'
    y = price;
  else
    y = this.ema[type][current - 2];
  
  ema = price * k + y * (1 - k);
  
  if(!ema){
    //in case of empty ema value (e.g. bitcoincharts downtime) take the last ema value
    ema = _.last(this.ema[type]);
    log.debug('WARNING: Unable to calculate EMA on current candle. Using last defined value.');
  }
  
  this.ema[type].push(ema);
}

// @link https://github.com/virtimus/GoxTradingBot/blob/85a67d27b856949cf27440ae77a56d4a83e0bfbe/background.js#L145
TradingMethod.prototype.calculateEMAdiff = function() {
  var shortEMA = _.last(this.ema.short);
  var longEMA = _.last(this.ema.long);

  var diff = 100 * (shortEMA - longEMA) / ((shortEMA + longEMA) / 2);
  this.ema.diff.push(diff);
}
 */

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Exponential Moving Average
    /// http://www.iexplain.org/ema-how-to-calculate/
    /// </summary>
    public class Ema
    {
        //private const int Max = 150;
        private readonly Queue<decimal> _queue = new Queue<decimal>();

        public int NumberOfDays { get; set; }
        public int QueueLength { get; set; }

        public void Add(decimal value)
        {
            _queue.Enqueue(value);
            if (_queue.Count > QueueLength)
            {
                _queue.Dequeue();
            }
        }

        public bool IsValid()
        {
            return (_queue.Count == QueueLength);
        }

        private decimal CalculateEma(decimal todaysPrice, decimal emaYesterday)
        {
            decimal k = 2M / (NumberOfDays + 1M);
            return todaysPrice * k + (1 - k) * emaYesterday;
        }

        public decimal Calculate()
        {
            decimal yesterdayEma = decimal.MinValue;

            foreach (var d in _queue)
            {
                if (yesterdayEma == decimal.MinValue)
                {
                    yesterdayEma = d;
                }

                // prima
                //var price = d;
                //var ema = CalculateEma(price, yesterdayEma);
                //yesterdayEma = ema;
                // dopo
                yesterdayEma = CalculateEma(d, yesterdayEma);
            }
            return yesterdayEma;
        }

    }
}
