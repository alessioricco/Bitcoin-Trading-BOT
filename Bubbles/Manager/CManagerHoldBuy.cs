using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Bubbles.Bubble;
using Bubbles.Market;
using Bubbles.TechnicalAnalysis;

namespace Bubbles.Manager
{
    class CManagerHoldBuy : CManagerBase
    {

        private int _bubblesCreated = 0;
        private int _bubblesExploded = 0;

        //public CManagerHoldBuy(ConfigurationHoldAndBuy configuration, CMarket market, bool verbose) : base(configuration,market){}

        public override bool OnHeartBeat()
        {
            // elimina le bolle (ad ogni tick.. prima si fa e meglio è)
            var hasExplodedBubbles = CheckBubbles();
            return true;
        }

        /// <summary>
        /// il tick è il momento in cui vengono prese le decisioni di buy/sell
        /// </summary>
        public override bool OnTick()
        {
            //CUtility.Log("*** tick");

            // in questo simulatore se il mercato scende non ci muoviamo
            //if (Trend < 0)
            if (Market.CandleMaker.Analyze.Trend() == Trend.Fall)
            {
                //CUtility.Log("*** trend down... skip");
                return false;
            }

            // analizzo il numero di bolle, per crearne almeno una
            var numBubbles = Market.Bubbles.Count();
            if (numBubbles < ConfigurationHoldAndBuy.MaxBubbles)
            {
                CreateBubble();
            }

            // mostro le bolle
            Market.ShowBubbles();
            return true;
        }

        /// <summary>
        /// elimina le bolle che hanno raggiunto il guadagno
        /// </summary>
        private bool CheckBubbles()
        {
            if (Market.CandleMaker.Analyze.Trend() == Trend.Raise)
            {
                // se il trend è in salita aspetto (dovrebbe ottimizzare i guadagni)
                //return false;
            }

            if (Market.Bubbles.Count > 0)
            {
                // scoppio le bolle
                foreach (CBubble bubble in Market.Bubbles)
                {
                    if (bubble.SellAt < Market.Ask)
                    {
                        //scoppio la bolla: cioè vendo i bitcoin per poterli riutilizzare
                        var marketValue = Market.Ask;
                        var orderId = "";
                        Market.DoOrder(CUtility.OrderType.Sell, marketValue, bubble.Coins, ref orderId, true);
                        //CUtility.Log("*** Bubble removed");
                        _bubblesExploded++;
                        bubble.Deleted = true;

                        // occorrerebbe mettere un acquisto di btc appena sono convenienti

                    }
                }
                // rimuovo le bolle scoppiate
                var removed = Market.Bubbles.RemoveAll(o => o.Deleted == true);
                // se ho scoppiato bolle aspetto un tick
                if (removed > 0)
                {
                    Market.ShowBubbles();
                    //return;
                }
                return removed > 0;
            }
            return false;

        }


        private void CreateBubble()
        {

           // CUtility.Log("*** order buy");
            var marketValue = Market.Bid;
            string orderId = "";

            // non compro perchè il valore di mercato è circa il 10% di altre bolle presenti
            if (Market.Bubbles.Any(o => Math.Abs(o.MarketValue - marketValue) / marketValue <= (decimal)0.25))
            {
                return;
            }

            // soldi nel wallet, ma non piu' del massimo consentito
            decimal availableMoney = Market.TotMoney;
            if (availableMoney > MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest)
            {
                availableMoney = MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest;
            }

            // come creo una bolla ? 
            var money = availableMoney / ConfigurationHoldAndBuy.MaxBubbles;
            var coins = money / marketValue;

            if (coins > ConfigurationHoldAndBuy.MaxCoinsPerBubble)
            {
                coins = ConfigurationHoldAndBuy.MaxCoinsPerBubble;
            }

            // verifico di non aver impiegato troppi soldi
            if (coins < ConfigurationHoldAndBuy.MinimumCoinsPerBubble)
            {
                // bolla troppo piccola... 
                if ((decimal)ConfigurationHoldAndBuy.MinimumCoinsPerBubble * marketValue <= availableMoney)
                {
                    coins = ConfigurationHoldAndBuy.MinimumCoinsPerBubble;
                }
            }

            // verifico di non aver superato le soglie minime di coins e soldi

            Market.DoOrder(CUtility.OrderType.Buy, marketValue, coins, ref orderId, true);
           // CUtility.Log("*** Bubble added");
            _bubblesCreated++;
            Market.Bubbles.Add(CBubble.Create(Market, CUtility.OrderType.Buy, coins, marketValue,
                ConfigurationHoldAndBuy.Gain, orderId));
        }





    }
}
