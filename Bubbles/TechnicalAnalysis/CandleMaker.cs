using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis.Analyzer;

namespace Bubbles.TechnicalAnalysis
{

    public class Candle
    {
        public enum CandleColor
        {
            Red,
            Green
        };

        public uint CandleId { get; set; }
        public DateTime Date { get; set; }
        public int Duration { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public string Label { get; set; }

        public CandleColor Color
        {
            get {
                return Open <= Close ? CandleColor.Green : CandleColor.Red;
            }
        }

        public bool IsDoji()
        {
            var perc = 1M;
            var epsilon = (perc) * (Open/100M);
            return Math.Abs(Open - Close) < epsilon;
        }

        public bool IsSpinningTop()
        {
            var perc = 5M;
            var epsilon = (perc) * (Open / 100M);
            return Math.Abs(Open - Close) < epsilon;
        }

        public bool IsMarubozuBullish()
        {
            if (Open < Close)
            {
                if (Open == Low)
                {
                    if (Close == High)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsMarubozuBearish()
        {
            if (Open > Close)
            {
                if (Open == High)
                {
                    if (Close == Low)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsInvertedHammer()
        {
            if (Color== CandleColor.Red)
            {
                if (Close == Low)
                {
                    if (High > Open && High > Close)
                        return true;
                }
            }
            return false;
        }

        public bool IsHammer()
        {
            if (Color == CandleColor.Green)
            {
                if (Open == High)
                {
                    if (Low < Close && Low < Open)
                    return true;
                }
            }
            return false;         
        }

        public bool IsHangingMan()
        {
            return IsHammer();
        }

        public bool IsShootingStar()
        {
            if (Color == CandleColor.Green)
            {
                if (Close == Low)
                {
                    if (High > Open && High > Close)
                    return true;
                }
            }
            return false;
        }

        public void Log(StreamWriter fileStreamOut)
        {
            fileStreamOut.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",

                this.CandleId.ToString(CultureInfo.InvariantCulture),
                Math.Round(this.Close, 4)
                    .ToString(CultureInfo.InvariantCulture),

                this.Date.ToString(CultureInfo.InvariantCulture),
                this.Duration
                    .ToString(CultureInfo.InvariantCulture),

                Math.Round(this.Open, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(this.Close, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(this.High, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(this.Low, 4).ToString(CultureInfo.InvariantCulture),
                Math.Round(this.Volume, 4).ToString(CultureInfo.InvariantCulture),
                this.Color.ToString()
                    
                );
        }

        public void Log()
        {
            CUtility.Log(string.Format("Candle {0} {1} {2}", this.Date.ToString(CultureInfo.InvariantCulture), Math.Round(this.Close, 4).ToString(CultureInfo.InvariantCulture), this.Color.ToString()));
            //CUtility.Log("Date  : " + this.Date.ToString(CultureInfo.InvariantCulture));
            //CUtility.Log("Bid   : " + Math.Round(this.Close, 4).ToString(CultureInfo.InvariantCulture));
            //CUtility.Log("Color : " + this.Color.ToString());
            CUtility.Log("");
        }

    }

    class InstantValue
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal Volume { get; set; }
 
    }

    /// <summary>
    /// Questa classe crea le candele
    /// </summary>
    public class CandleMaker
    {

        private readonly Queue<Candle> _candles = new Queue<Candle>();
        private readonly int _queueLength = MainConfiguration.Configuration.QueueLength; // 150; //60*24*5;

        public int CandleWidth {get; set;}
        public Analyze Analyze { get; set; }

        public uint CurrentCandleId { get; private set; }
        public DateTime CurrentCandleDate { get; private set; }

        public bool GenerateFile { get; set; }

        private string filename;
        //private readonly StreamWriter _fileStreamOut;

        public Candle[] GetLastCandles()
        {
            return _candles.Reverse().Take(12).ToArray();
        }

        public int QueueLength()
        {
            return _queueLength;
        }

        public bool IsValid()
        {
            return _candles.Count >= _queueLength;
        }

        private void Add(Candle frame)
        {
            _candles.Enqueue(frame);
            if (_candles.Count > _queueLength)
            {
                _candles.Dequeue();
            }
        }

        private readonly List<InstantValue> _values = new List<InstantValue>();

        public CandleMaker()
        {
             filename = string.Format("candles-{0}.csv", DateTime.Now.ToString("yyyyMMddhhmmss"));
            //_fileStreamOut = new StreamWriter(filename);
            CurrentCandleId = uint.MinValue;
        }

        /*
        ~CandleMaker()
        {
            try
            {
                if (_fileStreamOut != null)
                {
                    _fileStreamOut.Close();
                    _fileStreamOut.Dispose();
                }
            }
            catch (Exception ex)
            {
                    
                CUtility.Log(ex.Message);
            }

        }
         */ 

        private uint CandleId(DateTime date)
        {/*
            uint unixdate = CUtility.UnixTime.GetFromDateTime(date);
            uint diff = unixdate % (uint)CandleWidth;
            uint start = unixdate - diff;
            uint end = unixdate + (uint)CandleWidth;
            return start;
          * */
            return CandleId(date, (uint)CandleWidth);
        }

        private uint CandleId(DateTime date, uint width)
        {
            uint unixdate = CUtility.UnixTime.GetFromDateTime(date);
            uint diff = unixdate % (uint)width;
            uint start = unixdate - diff;
            uint end = unixdate + (uint)width;
            return start;
        }

        /// <summary>
        /// Questo è un long term trend che vale fino ad un certo punto
        /// andrebbe migliorato e calcolato con più attenzione
        /// </summary>
        /// <returns></returns>
        
        public Trend LongTermTrend()
        {
            if (_candles.Count < _queueLength) return Trend.Unknown;

            var percent = MainConfiguration.Configuration.LongTermTrendTolerance;

            var first = _candles.First().Close;
            var last = _candles.Last().Close;

            if (first > last*percent) return Trend.Fall;
            if (first < last*percent) return Trend.Raise;
            return Trend.Stable;
        }
        
        public Trend LongTermTrend(int candles)
        {
            if (_candles.Count < _queueLength) return Trend.Unknown;

            var percent = MainConfiguration.Configuration.LongTermTrendTolerance;

            var first = _candles.Reverse().Take(candles).Last().Close;
            var last = _candles.Last().Close;

            if (first > last * percent) return Trend.Fall;
            if (first < last * percent) return Trend.Raise;
            return Trend.Stable;
        }
        

        /// <summary>
        /// Massima escursione del valore della candela
        /// nelle candele a disposizione.
        /// viene usato per il calcolo del grado di avidità.
        /// in particolare nella proporsione tra il coefficiente (velocità)
        /// del trend a lungo termine
        /// </summary>
        /// <returns></returns>
        public decimal LongTermMaxCoeff()
        {
            return (_candles.Max(o=>o.High) - _candles.Min(o => o.Low))/_candles.Count;
        }

        public decimal LongTermTrendSpeed()
        {
            if (_candles.Count < _queueLength) return 0;

            //var percent = MainConfiguration.Configuration.LongTermTrendTolerance;

            var first = _candles.First().Close;
            var last = _candles.Last().Close;

            var m = (last - first)/_candles.Count;
            return m;

            //if (first > last * percent) return Trend.Fall;
            //if (first < last * percent) return Trend.Raise;
            //return Trend.Stable;
        }

        /// <summary>
        /// crea la candela aggiungendo il valore ad una lista.
        /// quando arriverà un valore per una data fuori dalla lista
        /// la lista sarà resettata e la candela calcolata e aggiunta
        /// </summary>
        /// <param name="date"></param>
        /// <param name="value"></param>
        public void CandleBuilder(DateTime date, decimal value, decimal volume)
        {
            var candleId = CandleId(date);
            
            // le candele attuali sono nel range?
            if (_values.Count > 0)
            {
                var currentCandle = CandleId(_values[0].Date);
                // verifico se devo archiviare la candela
                if (candleId > currentCandle)
                {
                    // devo archiviare la candela
                    // quindi calcolo le statistiche e archivio

                    var open = _values.OrderBy(o => o.Date).Select(o => o.Value).First();
                    var close = _values.OrderByDescending(o => o.Date).Select(o => o.Value).First();
                    var high = _values.OrderByDescending(o => o.Value).Select(o => o.Value).First();
                    var low = _values.OrderBy(o => o.Value).Select(o => o.Value).First();
                    var candledate = CUtility.UnixTime.ConvertToDateTime(currentCandle);
                    var amount = _values.Sum(o => o.Volume);

                    var candle = new Candle()
                    {
                        CandleId = currentCandle,
                        Date = candledate,
                        Duration = this.CandleWidth,
                        Close = close,
                        High = high,
                        Low = low,
                        Open = open,
                        Volume = volume
                    };
                    this.Add(candle);

                    // scrivo la candela su file
                    if (GenerateFile)
                    {
                        using (var fileStreamOut = new StreamWriter(filename,true))
                        {
                            //if (fileStreamOut != null)
                            //{
                                candle.Log(fileStreamOut);
                            //}
                        }
                    }

                    candle.Log();

                    // aggiorno l'analisi tecnica 
                    if (this.Analyze != null)
                    {
                        this.Analyze.OnNewCandle(candle);
                    }
                    // questi valori servono nel manager per sapere quando loggare
                    this.CurrentCandleId = candle.CandleId;
                    this.CurrentCandleDate = candledate;

                    // cancella la lista
                    _values.Clear();

                }
                else if (candleId < currentCandle)
                {
                    // c'è un errore
                    throw new Exception("Candela sbagliata");
                }

            }

            _values.Add(new InstantValue()
            {
                Date = date,
                Value = value,
                Volume = volume
            });

        }

    }
}
