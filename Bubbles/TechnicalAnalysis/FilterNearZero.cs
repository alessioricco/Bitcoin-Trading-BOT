using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// Cerca di eliminare le oscillazioni di valore 
    /// intorno allo zero
    /// viene usato in macd per difendersi dalle oscillazioni
    /// </summary>
    public class FilterNearZero
    {
        private readonly Queue<decimal> _queue = new Queue<decimal>();

        public int QueueLen { get; set; }

        public bool CrossUp { get; set; }
        public bool CrossDown { get; set; }

        public FilterNearZero()
        {
            // più il valore è alto e più l'ordine viene fatto cautamente
            QueueLen = MainConfiguration.Configuration.ZeroFilterSequenceLen;
        }

        public decimal? Tolerance { get; set; }

        public TradeAction SuggestedAction(Trend trend)
        {
            if (trend == TechnicalAnalysis.Trend.Raise)
                return TradeAction.Buy;
            if (trend == TechnicalAnalysis.Trend.Fall)
                return TradeAction.Sell;
            return TradeAction.Hold;
        }

        public Trend Trend(decimal value)
        {

            if (Tolerance.HasValue)
            {
                if (Math.Abs(value) < Tolerance)
                {
                    value = 0;
                }
            }

            CrossDown = false;
            CrossUp = false;


            //const int max = QueueLen;
            _queue.Enqueue(value);
            if (_queue.Count > QueueLen)
            {
                _queue.Dequeue();
            }
            else
            {
                return TechnicalAnalysis.Trend.Unknown;
            }

            // l'azione suggerita dipende da un trend di cambio segno
            // esamino quindi se esiste un pattern di metà dati tutti di un segno
            // e l'altra metà del segno opposto
            //decimal firstvalue = _queue.Peek();
            var firstValueSign = Math.Sign(_queue.Peek());

            var arrayQueue = _queue.ToArray();
            var len = arrayQueue.Count();

            int i = 0;
            while (i < len)
            {
                // controlla il segno e calcola un eventuale disturbo
                if (Math.Sign(arrayQueue[i]) != firstValueSign)
                {
                    // il segno è cambiato
                    if (i >= (len * MainConfiguration.Configuration.ZeroFilterSequenceChangeSignPercent))
                    {
                        // i è oltre la metà
                        return TechnicalAnalysis.Trend.Stable;
                    }
                    // altrimenti controlla al prossimo ciclo
                    break;
                }
                i++;
            }
            // in questo ciclo il segno non deve cambiare
            while (i < len)
            {
                if (Math.Sign(arrayQueue[i]) == firstValueSign)
                {
                    return TechnicalAnalysis.Trend.Stable;
                }
                i++;
            }

            var last = arrayQueue[len - 1]; // il piu' vecchio
            var first = arrayQueue[0]; // l'ultimo inserito
            if (Math.Sign(first) != Math.Sign(last))
            {
                CrossUp = Math.Sign(last) > 0;
                CrossDown = ! CrossUp;
            }
            else
            {

            }

            // se il primo valore è positivo allora l'ultimo è negativo e ho un Sell
            //return Math.Sign(first) > 0 ? TechnicalAnalysis.Trend.Fall :TechnicalAnalysis.Trend.Raise;

            if (CrossUp) return TechnicalAnalysis.Trend.Raise;
            if (CrossDown) return TechnicalAnalysis.Trend.Fall;
            // hanno lo stesso segno
            if (! CrossDown && ! CrossUp)
            {
                //return Math.Sign(first) == 1 ? TechnicalAnalysis.Trend.Raise : TechnicalAnalysis.Trend.Fall;
                if (Math.Sign(first) == 1) return TechnicalAnalysis.Trend.Raise;
                if (Math.Sign(first) == -1) return TechnicalAnalysis.Trend.Fall;
            }
            return TechnicalAnalysis.Trend.Stable;

        }
    }
}
