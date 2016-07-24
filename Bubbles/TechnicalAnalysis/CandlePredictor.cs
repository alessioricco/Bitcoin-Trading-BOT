using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Market;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// il predittore si riene un archivio di candele in configurazioni da 8
    /// e prova a predirre le prossime 4 combinazioni di candele che usciranno
    /// occorre fare un test di verifica delle predizioni
    /// 
    /// forse 8-4 è troppo ottimistico, magari un 10-2 sarebbe meglio
    /// </summary>
    class CandlePredictor
    {
        // http://stackoverflow.com/questions/4448063/how-can-i-convert-an-int-to-an-array-of-bool
        
        private readonly Queue<Candle> _candles = new Queue<Candle>();
        private const int Dim1 = 10;
        private const int Dim2 = 4;
        private const int QueueLength = Dim1+Dim2;
        readonly uint[,] _pattern = new uint[Dim1 * Dim1, Dim2 * Dim2];

        public void Add(Candle frame)
        {
            _candles.Enqueue(frame);
            if (_candles.Count > QueueLength)
            {
                _candles.Dequeue();
            }
            if (IsValid())
            {
                Learn();
            }
        }

        public bool IsValid()
        {
            return _candles.Count == QueueLength;
        }

        

        private void Learn()
        {
            var candleArray = _candles.ToArray();
            var bv1 = new BitVector32(0);
            for (var i = 0; i < Dim1; i++)
            {
                bv1[i] = candleArray[i].Color == Candle.CandleColor.Green;
            }
            var x = bv1.Data;

            var bv2 = new BitVector32(0);
            for (var j = Dim1; j < Dim1+Dim2; j++)
            {
                bv2[j] = candleArray[j].Color == Candle.CandleColor.Green;
            }
            var y = bv2.Data;

            _pattern[x, y]++;

        }

        public BitVector32 Guess(out double perc)
        {
            var candleArray = _candles.ToArray();
            var bv1 = new BitVector32(0);
            for (var i = 0; i < Dim1; i++)
            {
                bv1[i] = candleArray[i].Color == Candle.CandleColor.Green;
            }
            var x = bv1.Data;

            
            uint max = uint.MinValue;
            uint sum = 0;
            var y = -1;
            for (var j = 0; j < Dim2*Dim2; j++)
            {
                sum += _pattern[x, j];
                if (_pattern[x, j] > max)
                {
                    max = _pattern[x, j];
                    y = j;

                }
                
            }

            // la candela per essere affidabile si deve essere presentata un bel po' di volte
            if (sum > 16)
            {
                perc = (double) max/(double) sum;
            }
            else
            {
                perc = 0;
            }
            return new BitVector32(y);
        }

        public Trend SuggestedTrend()
        {
            double perc = 0;
            var prediction = Guess(out perc);
            CUtility.Log("Perc   : " + perc);
            CUtility.Log("Pattern: " + prediction.ToString().Replace("BitVector32{","").Replace("}","").Substring(28));
            if (perc > 0.33)
            {
                var numGreen = 0;
                for (var i = 0; i < Dim2;i++)
                {
                    if (prediction[0])
                    {
                        numGreen++;
                    }
                }
                if (numGreen >= Dim2*3/4)
                {
                    return Trend.Raise;
                }
                else
                {
                    if (numGreen == Dim2/2)
                    {
                        return Trend.Stable;
                    }
                    else
                    {
                        return Trend.Fall;
                    }
                }
                
            }
            else
            {
                return Trend.Unknown;
            }
        }
    }
}
