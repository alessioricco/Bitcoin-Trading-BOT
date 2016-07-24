using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Evaluations
{
    /// <summary>
    /// Racchiude short, mid e long term evaluator associati ad un solo oggetto esaminato
    /// </summary>
    class EvaluatorSet
    {
        
        /// <summary>
        /// Valutatore a corto raggio
        /// </summary>
        public Evaluator ShortTerm { get; protected set; }

        /// <summary>
        /// Valutatore a medio raggio
        /// </summary>
        public Evaluator MidTerm { get; protected set; }

        /// <summary>
        /// Valutatore a lungo raggio
        /// </summary>
        public Evaluator LongTerm { get; protected set; }


        public EvaluatorSet(int shortMinutes,int midMinutes, int longMinutes) {

            this.ShortTerm = new Evaluator()
            {
                PredictionDuration = shortMinutes * 60
            };

            this.MidTerm = new Evaluator()
            {
                PredictionDuration = midMinutes * 60
            };

            this.LongTerm = new Evaluator()
            {
                PredictionDuration = longMinutes * 60
            };

        
        }


        /// <summary>
        /// Aggiunge la predizione a tutti i valutatori del set
        /// </summary>
        /// <param name="p"></param>
        public void Add(Prediction p) {

            //eheheh ciao alessio questa è per te ;)
            (new []{ this.ShortTerm, this.MidTerm, this.LongTerm}).ToList().ForEach(o=>o.Add(p));

        }

        /// <summary>
        /// Aggiunge 3 predizioni, una per arco temporale
        /// </summary>
        /// <param name="st"></param>
        /// <param name="mt"></param>
        /// <param name="lt"></param>
        public void Add(Prediction st, Prediction mt, Prediction lt)
        {
            this.ShortTerm.Add(st);
            this.MidTerm.Add(mt);
            this.LongTerm.Add(lt);

        }

        /// <summary>
        /// Esegue la valutazione sui valutatori associati
        /// </summary>
        /// <param name="now">data attuale di riferimento</param>
        /// <param name="currentMarketValue">valore attuale del mercato</param>
        public void Evaluate(DateTime now, decimal currentMarketValue) {
            (new[] { this.ShortTerm, this.MidTerm, this.LongTerm }).ToList().ForEach(o => o.Evaluate(now, currentMarketValue));
        }


    }
}
