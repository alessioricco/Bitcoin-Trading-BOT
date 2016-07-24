using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    public class Sma
    {
        private readonly Queue<decimal> _queue = new Queue<decimal>();

        public int NumberOfDays { get; set; }
        //public int QueueLength { get; set; }

        public bool IsValid()
        {
            return (_queue.Count == NumberOfDays);
        }

        public void Add(decimal value)
        {
            _queue.Enqueue(value);
            if (_queue.Count > NumberOfDays)
            {
                _queue.Dequeue();
            }
        }

        public decimal Calculate()
        {
            return _queue.Average();
        }

    }
}
