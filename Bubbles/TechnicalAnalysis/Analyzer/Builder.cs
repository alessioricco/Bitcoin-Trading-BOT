using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.TechnicalAnalysis.Analyzer
{
    /// <summary>
    /// implementa il builder che, dato il nome di una classe analyze, ne costruisce un'istanza
    /// </summary>
    class Builder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz">identificativo (nome) della classe</param>
        /// <returns></returns>
        public static Analyze Create(string clazz) {

            var C = MainConfiguration.Configuration;

            switch (clazz) {
                case "AnalyzeWithCommands":
                    return new AnalyzeWithCommands()
                    {
                        //MacdList = MainConfiguration.Configuration.MacdList,
                        // EmaDiff = MainConfiguration.Configuration.EmaDiff, // ema buoni: 10,20 10,21,  8,20
                        EmaDiff = EmaDiff.Create(C.EmaDiff[0], C.EmaDiff[1]), // ema buoni: 10,20 10,21,  8,20
                        StopLoss = new CStopLoss()
                        {
                            Percent = C.StopLossPercent
                        }
                    };
                    break;
                default :
                    return null;
                    break;
            }

            
           

        }

    }
}
