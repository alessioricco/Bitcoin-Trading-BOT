using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Market;

namespace Bubbles.Bubble
{
    [Serializable]
    public class CBubble
    {
        // coins acquistati
        public decimal Coins { get; set; }
        // valore al momento dell'acquisto
        public decimal ValueOfCoins { get; set; }
        //
        public decimal MarketValue { get; set; }
        // ora dell'acquisto
        public DateTime Created { get; set; }
        // mercato di provenienza
        public Market.CMarket Market { get; set; }


        // tipo di bolla (bid/ask) ovvero bolla di vendita o bolla di acquisto (per default è bid)
        public Market.CUtility.OrderType Type { get; set; }
        // valore minimo per la vendita (se type == buy)
        public decimal SellAt { get; set; }
        // valore minimo per l'acquisto (se type == sell)
        public decimal BuyAt { get; set; }
        // ordine collegato alla bolla
        public string OrderId { get; set; }

        public bool Deleted { get; set; }

        public static CBubble Create(Market.CMarket market,  Market.CUtility.OrderType type, decimal coins, decimal currentMarketValue, decimal gain, string orderId)
        {
            return new CBubble()
            {
                Created = DateTime.Now,
                Coins = coins * (1-market.Fee), // ricordarsi di togliere la percentuale
                ValueOfCoins = coins*currentMarketValue,
                OrderId = orderId,
                MarketValue = currentMarketValue,
                Type = type,
                Deleted = false,
                Market = market,
                SellAt = (1+gain) * (currentMarketValue * (1 + (2 * market.Fee))),
                BuyAt = (1-gain) * (currentMarketValue * (1 - (2 * market.Fee)))

                //SellAt = currentMarketValue* (1 + (2 * market.Fee + gain) / 1),
                //BuyAt  = currentMarketValue* (1 - (2 * market.Fee + gain) / 1)
            };
        }
    }
}
