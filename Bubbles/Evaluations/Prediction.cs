using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Evaluations
{


    /// <summary>
    /// definisce una predizione, ovvero una scommessa 
    /// </summary>
    public class Prediction
    {
        /// <summary>
        /// Tipo di predizione
        /// </summary>
        public PredictionType Type { get; set; }

        /// <summary>
        /// Data in cui la predizione viene effettuata
        /// </summary>
       public  DateTime Date { get; set; }

        /// <summary>
        /// Valore di riferimento della predizione (ovvero il valore-soglia che ci si aspetta)
        /// </summary>
        public Decimal Value { get; set; }
    }
}
