using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Manager;

namespace Bubbles.TechnicalAnalysis
{

    public class Roc : Indicator
    {

        public override void Add(decimal value)
        {

            Queue.Enqueue(value);
            if (Queue.Count > QueueLength)
            {
                Queue.Dequeue();
            }
            this.CurrentValue = Calculate();

            Derivative.Add(this.CurrentValue);

        }

        protected override decimal Calculate()
        {
            var last = Queue.First();
            var today = Queue.Last();
            return 100 * (today - last) / last;
        }

        public override bool IsValid()
        {
            return Queue.Count() >= QueueLength;
        }

        public override TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {
            //* se ci accorgiamo che il prezzo forma un nuovo massimo 
            //* mentre roc forma un massimo piu' basso siamo in presenza di una divergenza
            //* che potrebbe indicare una inversione di trend 
            // The conventional interpretation is to use momentum as a trend-following indicator. 
            // This means that when the indicator peaks and begins to descend, it can be considered a sell signal. 
            // The opposite conditions can be interpreted when the indicator bottoms out and begins to rise.[1

            var rocSpeedTrend = this.FilterNearZero.Trend(this.Derivative.CurrentSpeed);
            
            if (rocSpeedTrend == Trend.Fall && value > MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
            {
                LogTrade.Note += " SELL SIGNAL";
                return TradeAction.StrongSell;
            }

            if (rocSpeedTrend == Trend.Raise && value < -MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
            {
                LogTrade.Note += " BUY SIGNAL";
                return TradeAction.StrongBuy;
            }
            
            if (rocSpeedTrend == Trend.Fall && value > 0)
            {
                LogTrade.Note += " SELL SIGNAL";
                return TradeAction.Sell;
            }

            if (rocSpeedTrend == Trend.Raise && value < 0)
            {
                LogTrade.Note += " BUY SIGNAL";
                return TradeAction.Buy;
            }

            return TradeAction.Hold;
        }

    }

    public class RocOld
    {
        private readonly Queue<decimal> _queue = new Queue<decimal>();
        private readonly FilterNearZero _filterNearZero = new FilterNearZero();

        //public int NumberOfDays { get; set; }
        public int QueueLength { get; set; }
        //public decimal CurrentSpeed { get; set; }

        public Derivative Derivative = new Derivative();

        public void Add(decimal value)
        {
           
            _queue.Enqueue(value);
            if (_queue.Count > QueueLength)
            {
                _queue.Dequeue();
            }
            Derivative.Add(value);
            //CurrentSpeed = Derivative();
        }

        private Trend LongTermTrend()
        {
            if (_queue.Count < QueueLength) return Trend.Unknown;

            var percent = 1; //MainConfiguration.Configuration.LongTermTrendTolerance;

            var list = _queue.ToArray();

            var first = list[list.Count()*3/4]; //_queue.First();
            var last = list[list.Count()-1]; //_queue.Last();

            if (first > last * percent) return Trend.Fall;
            if (first < last * percent) return Trend.Raise;
            return Trend.Stable;
        }

        public bool IsValid()
        {
            return (_queue.Count == QueueLength);
        }

        public decimal Calculate()
        {
            var last = _queue.First();
            var today = _queue.Last();
            return 100*(today - last)/last;
        }

        
        public TradeAction SuggestedAction(decimal value, Trend moneyTrend)
        {      
             //* se ci accorgiamo che il prezzo forma un nuovo massimo 
             //* mentre roc forma un massimo piu' basso siamo in presenza di una divergenza
             //* che potrebbe indicare una inversione di trend 
            
            // trend della derivata
            //var currentRocTrend = _filterNearZero.Trend(this.Derivative.CurrentSpeed);
            if (moneyTrend == Trend.Fall || moneyTrend == Trend.Raise)
            {
                var rocTrend = LongTermTrend();
                if (rocTrend != moneyTrend)
                {
                    // inversione di tendenza?
                    if (rocTrend == Trend.Fall)
                    {
                        if (value > MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
                        {
                            // la derivata del roc diventa negativa e il valore del roc supera il limite
                            // a breve scende
                            LogTrade.Note += " INV TENDENZA ";
                            //LogTrade.Note += " (vendi perchè scenderà) ";
                            return TradeAction.Sell;
                        }

                        // inversione di tendenza?
                        if (value > 0)
                        {
                            // la derivata del roc diventa negativa e il valore del roc supera il limite
                            // a breve scende
                            LogTrade.Note += " INV TENDENZA ";
                            return TradeAction.Sell;
                        }

                    }

                    if (rocTrend == Trend.Raise)
                    {
                        if (value < -MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
                        {
                            // a breve sale
                            LogTrade.Note += " INV TENDENZA ";
                            //LogTrade.Note += " (compra perchè salirà) ";
                            return TradeAction.Buy;
                        }


                        if (value < 0)
                        {
                            // a breve sale
                            LogTrade.Note += " INV TENDENZA ";
                            return TradeAction.Buy;
                        }
                    }
                }
                else
                {
                    LogTrade.Note += " RAFFORZAMENTO TREND ";
                    //  conferma del trend
                    if (rocTrend == Trend.Raise && value > MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
                    {
                        LogTrade.Note += " (compra perchè salirà) ";
                        
                        return TradeAction.StrongBuy;
                    }
                    if (rocTrend == Trend.Fall && value < -MainConfiguration.Configuration.AlarmStrongSellBuyRocLimit)
                    {
                        LogTrade.Note += " (vendi perchè scenderà) ";
                        return TradeAction.StrongSell;
                    }
                }
            }

            return TradeAction.Hold;
        }
         
         

    }
}
