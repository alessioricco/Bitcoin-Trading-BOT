using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Manager
{
    /// <summary>
    /// implementa il builder che, dato il nome di una classe manager, ne costruisce un'istanza
    /// </summary>
    class Builder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz">identificativo (nome) della classe</param>
        /// <returns></returns>
        public static CManagerBase Create(string clazz)
        {

            switch (clazz)
            {
                case "CManagerTrading":
                    return new CManagerTrading()
                        {

                            Conservative = MainConfiguration.Configuration.ManagerIsConservative, // cerca sempre di investire la quantità di capitale
                            PercCapital = MainConfiguration.Configuration.ManagerInvestCapitalPercent, // quantità di capitale da investire
                            Market = Market.Builder.Create(MainConfiguration.Configuration.MarketClass) //market
                        };
                    break;
                default:
                    return null;
                    break;
            }

        }

    }
}
