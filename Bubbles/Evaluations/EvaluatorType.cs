using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Evaluations
{
    /// <summary>
    /// Tipi di valutatore
    /// diverse logiche di valutazione 
    /// </summary>
    enum EvaluatorType
    {
        /// <summary>
        /// la predizione viene valutata alla scadenza
        /// </summary>
        Point,
        /// <summary>
        /// la predizione viene valutata in un intorno della scadenza, e gli viene assegnato comunque il massimo punteggio
        /// </summary>
        Rectangular,
        /// <summary>
        /// la predizione viene valutata in un intorno della scadenza, e gli viene assegnato un punteggio proporzionale a quanto distante  
        /// </summary>
        Circle
    }
}
