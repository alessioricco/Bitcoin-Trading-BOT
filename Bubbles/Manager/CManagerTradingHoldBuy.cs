using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Bubbles.Bubble;
using Bubbles.Market;

 

using Bubbles.TechnicalAnalysis;

namespace Bubbles.Manager
{
    class CManagerTradingHoldBuy : CManagerBase
    {
        // percentuale di capitale che posso investire
        public decimal PercCapital { get; set; }
        public bool Conservative { get; set; }

        public int TotBid { get; set; }
        public int TotAsk { get; set; }
        public int TotStopLoss { get; set; }
        public decimal MaxValue { get; set; }
        public decimal MaxCoins { get; set; }

        private const decimal MinBitCoin = 0.01M;

        private CBubble _bubble;// = new CBubble();

        public override bool OnTick()
        {

            return true;

        }

        private TradeAction _lastTradeActionSuggested = TradeAction.Unknown;
        private CUtility.OrderType? _lastTradeActionExecuted = null;
        private decimal _lastMoney = -1; // non si puo' usare decimal.MinValue
        private decimal _lastCoin = -1; // non si puo' usare decimal.MinValue

        
        public override bool OnHeartBeat()
        {
            // devo vendere o devo comprare?             
            var action = this.SuggestedAction; 

            // mi salvo il massimo value
            if (Market.TotValue > this.MaxValue)
            {
                this.MaxValue = Market.TotValue;
            }

            if (Market.TotCoins > this.MaxCoins)
            {
                this.MaxCoins = Market.TotCoins;
            }

            /**************************************************************
            CHECK CONFERMA (DOPPIO COMANDO)
             **************************************************************/
            
            // questa parte di codice esegue l'operazione se c'è una conferma
            if (action != TechnicalAnalysis.TradeAction.SellStopLoss && action != TradeAction.StrongBuy && action != TradeAction.StrongSell)
            {
                if (action != _lastTradeActionSuggested)
                {
                    _lastTradeActionSuggested = action;
                    action = TradeAction.Unknown;
                }
            }
            else
            {
                _lastTradeActionSuggested = TradeAction.Unknown;
            }
            
            /**************************************************************
             VERIFICA BOLLA
             **************************************************************/

            const decimal gain = 0.02M;

            if (_bubble == null)
            {
                if (Market.TotCoins > 0)
                {
                    // ho comprato quindi ho coins
                    _bubble = CBubble.Create(Market, CUtility.OrderType.Buy, Market.TotCoins, Market.Bid, gain, "");
                }
                else
                {
                    // ho venduto, quindi ho soldi
                   // _bubble = CBubble.Create(Market, CUtility.OrderType.Sell, 0, Market.Ask, gain, "");
                }

            }

            /**************************************************************
             DEVO VENDERE
             **************************************************************/
            if (_bubble != null)
            {
                // è una bolla creata comprando bitcoins, quindi devo venderli
                if (Market.Ask > _bubble.SellAt)
                {
                    string orderId = "";
                    var amount = Market.TotCoins;

                    if (amount*Market.Ask >
                        MainConfiguration.Configuration.StartWalletCoins*
                        MainConfiguration.Configuration.StartWalletMoney)
                        if (amount > MinBitCoin)
                        {
                            Market.DoOrder(CUtility.OrderType.Sell, Market.Ask, amount, ref orderId, true);
                            _bubble = null;
                            TotAsk++;
                        }
                    goto exitif;
                }
            }

            /**************************************************************
             DEVO COMPRARE
             **************************************************************/
            if (_bubble == null)
            {
                if (SuggestedAction == TradeAction.StrongBuy || (SuggestedAction == TradeAction.Buy))
                {
                    string orderId = "";
                    var amount = Market.TotMoney/Market.Bid;

                    if (amount > MinBitCoin)
                    {
                        Market.DoOrder(CUtility.OrderType.Buy, Market.Bid, amount, ref orderId, true);
                        _bubble = CBubble.Create(Market, CUtility.OrderType.Buy, amount, Market.Bid, gain, orderId);
                        TotBid++;
                    }
                }
            }

            exitif:

            /**************************************************************
            LOG
             **************************************************************/
            Log(string.Format("Bids:{0} Asks:{1} Stop:{2} Max$:{3} Max:{4}", TotBid, TotAsk, TotStopLoss, Math.Round(this.MaxValue, 4).ToString(CultureInfo.InvariantCulture), Math.Round(this.MaxCoins, 4).ToString(CultureInfo.InvariantCulture)));

            return true;
        }

        protected decimal GreedyNess(decimal longTermCoefficienteAngolare)
        {
            //trasformazione lineare tra il coefficente di long term trend e il valore di greedynes
            //faccio una semplice proporzione  0 : minGreedyNess = LongTermMaxCoeff : maxGreedyNess

            const decimal maxGreedyNess = 0.6M;
            const decimal minGreedyNess = 0.4M;
            const decimal minCoeff = 0M;
            var maxCoeff = Market.CandleMaker.LongTermMaxCoeff();
            var greedyness = longTermCoefficienteAngolare * ((maxGreedyNess - minGreedyNess) / (maxCoeff - minCoeff)) + minGreedyNess;
            return 1 + Math.Abs(greedyness);
        }
    }
}
