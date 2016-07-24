using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Market
{
    class Trend
    {
        private const int Max = 150;
        private readonly Queue<decimal> _queue = new Queue<decimal>();
        private readonly Queue<decimal> _queueMacd = new Queue<decimal>();

        public decimal Ema { get; set; }
        public decimal macd { get; set; }
        public bool TrendIsValid { get; set; }

        private int EmaShort { get; set; }
        private int EmaLong { get; set; }

        private int Macd1 { get; set; }
        private int Macd2 { get; set; }
        private int Macd3 { get; set; }

        public Trend() : this(10, 22)
        {
            
        }

        public Trend(int emaShort, int emaLong) : this(emaShort,emaLong,12,26,9)
        {
        }

        public Trend(int emaShort, int emaLong, int macd1,int macd2, int macd3)
        {
            this.EmaLong = emaLong;
            this.EmaShort = emaShort;

            this.Macd1 = macd1;
            this.Macd2 = macd2;
            this.Macd3 = macd3;
        }

        public void Add(decimal value)
        {
            _queue.Enqueue(value);
            if (_queue.Count > Max)
            {
                _queue.Dequeue();
            }
        }


        private decimal CalculateEma(decimal todaysPrice, int numberOfDays, decimal emaYesterday)
        {
            var k = (decimal)2/(numberOfDays + 1);
            return todaysPrice*k + (1 - k)*emaYesterday;
        }

        private decimal CalculateEma(int numberOfDays, IEnumerable<decimal> list)
        {
            //var n = 100;
            decimal yesterdayEma = -1;
            //var list = _queue.Skip(Math.Max(0, _queue.Count() - n)).Take(n);
            //var list = _queue;
            foreach (var d in list)
            {
                if (yesterdayEma == -1) yesterdayEma = d;
                var price = d;
                var ema = CalculateEma(price, numberOfDays, yesterdayEma);
                yesterdayEma = ema;
            }
            return yesterdayEma;
        }

        private decimal CalculateEma(int numberOfDays)
        {
            return CalculateEma(numberOfDays, _queue);
        }

        private decimal Macd()
        {
            var ema12 = CalculateEma(this.Macd1);
            var ema26 = CalculateEma(this.Macd2);
            var sub = ema26 - ema12;
            _queueMacd.Enqueue(sub);
            var signalLine = CalculateEma(this.Macd3, _queueMacd);
            return -(sub - signalLine);
        }

        public int CalculateTrend()
        {
            if (_queue.Count < Max)
            {
                TrendIsValid = false;
                return 0;
            }

            TrendIsValid = true;

            var ema22 = CalculateEma(this.EmaLong);
            var ema10 = CalculateEma(this.EmaShort);

            var ema = ((ema10/ema22)-1) * 100;
            this.Ema = ema;
            this.macd = Macd();

            /*
            CUtility.Log("EMA 10 : " + Math.Round(ema10, 4).ToString(CultureInfo.InvariantCulture));
            CUtility.Log("EMA 22 : " + Math.Round(ema22, 4).ToString(CultureInfo.InvariantCulture)); 
            CUtility.Log("EMA    : " + Math.Round(ema, 4).ToString(CultureInfo.InvariantCulture));
            CUtility.Log("MACD   : " + Math.Round(macd, 4).ToString(CultureInfo.InvariantCulture));
            */
            
            // su cosa calcolo il trend? 
            // ema o macd?

            var trend = this.macd;

            // situazione statica
            if (Math.Abs(trend) < (decimal) 0.1)
            {
               return 0;
            }
            return trend > 0 ? 1 : -1;
        }



    }
}
