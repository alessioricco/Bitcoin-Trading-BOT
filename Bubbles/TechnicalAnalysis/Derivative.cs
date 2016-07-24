using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    public class Derivative
    {
        readonly Queue<decimal> _queue = new Queue<decimal>();
        public int QueueLength { get; set; }
        public decimal CurrentSpeed { get; set; }

        // per verificare se la derivata ha incrociato lo zero
        private readonly FilterNearZero _filterNearZero = new FilterNearZero() {  };
        public bool IsCrossUp { get; set; }
        public bool IsCrossDown { get; set; }

        public void Add(decimal value)
        {
           
            _queue.Enqueue(value);
            if (_queue.Count > QueueLength)
            {
                _queue.Dequeue();            
            }
            CurrentSpeed = Speed();

            // calcolo dello zero per le derivate
            _filterNearZero.QueueLen = MainConfiguration.Configuration.ZeroFilterSequenceLenDerivative;
            var trend = _filterNearZero.Trend(CurrentSpeed);
            this.IsCrossDown = _filterNearZero.CrossDown;
            this.IsCrossUp = _filterNearZero.CrossUp;
            //_filterNearZero.SuggestedAction(trend);

        }

       // private decimal _lastValue = 0;
        private decimal Speed()
        {
            /*
            // http://stackoverflow.com/questions/373186/mathematical-derivation-with-c
            var diff = value - _lastValue;
            _lastValue = value;
            return diff;
             */
            // [f(x+h) - f(x-h)] / 2h
            //var h = 1M;
            var list = _queue.ToArray();
            return (list[0] - list[2]) / 2;
        }

        public bool IsValid()
        {
            return (_queue.Count == QueueLength);
        }

        public Derivative()
        {
            this.QueueLength = 3;
        }

    }
}
