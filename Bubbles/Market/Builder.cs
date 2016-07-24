using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Market
{
    /// <summary>
    /// implementa il builder che, dato il nome di una classe market, ne costruisce un'istanza
    /// </summary>
    class Builder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz">identificativo (nome) della classe</param>
        /// <returns></returns>
        public static CMarket Create(string clazz) {

            switch (clazz) { 
                case "CMarketSimulator":
                    return  CMarketSimulator.Create();
                    break;

                case "CMarketMtGoxUSDBTC":
                    return CMarketMtGoxUsdbtc.Create();
                    break;

                case "CMarketBtceUsdBtc":
                    return CMarketBtceUsdBtc.Create();
                    break;

                default :
                    return null;
                    break;
            }

        }

    }
}
