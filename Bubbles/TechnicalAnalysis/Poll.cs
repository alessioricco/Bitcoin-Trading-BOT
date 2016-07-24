using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Manager;

namespace Bubbles.TechnicalAnalysis
{
    /// <summary>
    /// riceve i dati di un sondaggio : utile per le azioni
    /// </summary>
    class Poll
    {
        private decimal _sell = 0;
        private decimal _buy = 0;
        private decimal _hold = 0;

        public void Add(TradeAction action)
        {
            switch (action)
            {
                    case TradeAction.Buy:
                    _buy++;
                    break;
                
                    case TradeAction.Sell:
                    _sell++;
                    break;

                    case TradeAction.Hold:
                    _hold++;
                    break;

                    //case TradeAction.SellStopLoss:
                    //_sell++;
                    //_sell++;
                    //break;

                    case TradeAction.StrongBuy:
                    _buy++;
                    _buy++;
                    break;

                    case TradeAction.StrongSell:
                    _sell++;
                    _sell++;
                    break;

                    case TradeAction.Unknown:
                    _hold++;
                    break;
            }
        }

        public TradeAction Result()
        {

            var s = string.Format(" b:{0} h:{1} s:{2}", _buy, _hold, _sell);
            /*
             * questo sistema non va bene perchè ammazza il rendimento
            if (_sell > _hold && _hold == _buy && _hold == 0)
            {
                LogTrade.Note += s;
                return TradeAction.StrongSell;
            }
            if (_buy > _hold && _hold == _sell && _hold == 0)
            {
                LogTrade.Note += s;
                return TradeAction.StrongBuy;
            }
            */

            if (false && _sell + _hold + _buy > 3)
            {

                if (_sell > _hold && _hold == _buy && _buy == 0)
                    return TradeAction.StrongSell;

                if (_buy > _hold && _hold == _sell && _sell == 0)
                    return TradeAction.StrongBuy;
            }

            if (_sell > _hold && _sell > _buy)
            {
                LogTrade.Note += s;
                return TradeAction.Sell;
            }
            if (_buy > _hold && _buy > _sell)
            {
                LogTrade.Note += s;
                return TradeAction.Buy;
            }
            return TradeAction.Hold;
        }

    }
}
