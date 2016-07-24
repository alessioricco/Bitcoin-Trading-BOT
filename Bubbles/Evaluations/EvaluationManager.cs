using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Log = DataProvider.Log;

namespace Bubbles.Evaluations
{
    /// <summary>
    /// Classe singleton che gestisce tutti i valutatori dell'applicazione, li raggruppa e scrive i log
    /// </summary>
    class EvaluationManager
    {

        #region singleton pattern

        private EvaluationManager() {
            this.Evaluators = new Dictionary<string, EvaluatorSet>();
        }

        private static EvaluationManager _instance;

        public static EvaluationManager Instance
        {
            get
            {
                //if (_instance == null) _instance = new EvaluationManager();
                    return _instance ?? (_instance = new EvaluationManager());
            }
        }

        #endregion singleton pattern

        /// <summary>
        /// Collezione di tutti i valutatori
        /// </summary>
        protected Dictionary<string, EvaluatorSet> Evaluators { get; set; }

        /// <summary>
        /// Aggiunge la predizione al set di valutatori a cui fa riferimento name
        /// Se il set non esiste lo crea just-in-time
        /// </summary>
        /// <param name="name">nome UNIVOCO della valutazione, generalmente un descrittore dell'indicatore in analisi</param>
        /// <param name="p">predizione</param>
        /// <param name="timeUnit">indica quanto è grande l'unità temporale, in minuti (ovvero la grandezza delle candele a cui l'indicatore fa riferimento)</param>
        public void Add(string name, Prediction p, int timeUnit) { 
            
            //normalizzo
            name = name.ToLower();
            
            //
            if (!this.Evaluators.Keys.Contains(name) ||  this.Evaluators[name] == null) {
                this.Evaluators[name] = new EvaluatorSet(timeUnit * 10, timeUnit * 20, timeUnit * 40);
            }
            var set = this.Evaluators[name] ;

            set.Add(p);

            
        }


        /// <summary>
        /// Esegue la valutazione sui valutatori associati
        /// </summary>
        /// <param name="now">data attuale di riferimento</param>
        /// <param name="currentMarketValue">valore attuale del mercato</param>
        public void Evaluate(DateTime now, decimal currentMarketValue)
        {

            foreach (var e in this.Evaluators) {
                e.Value.Evaluate(now, currentMarketValue);
            }

        }


        /// <summary>
        /// fondamentalmente, questa funzione chiude i log
        /// </summary>
        public void Finalize() {

            using (var logger = new Log.CPlainTextLog(string.Format("evaluation{0}.csv", MainConfiguration.Configuration.SessionId), Log.FileAccess.Overwrite) { BufferSize = -1 })
            {

                //creo un dizionario co tutti i punteggi
                var dict = new Dictionary<string, string>();

                Func< Evaluator, string> serialize = (o =>
                {
                    var v = new[] { Math.Round(o.Score, 2).ToString(CultureInfo.InvariantCulture),
                         o.Points.ToString(CultureInfo.InvariantCulture),
                         o.TotPoints.ToString(CultureInfo.InvariantCulture),
                         o.NumberOfPredictions.ToString(CultureInfo.InvariantCulture) };
                    return string.Join(",", v);
                });


                foreach (var e in this.Evaluators)
                {
                    dict[string.Format("{0}-st", e.Key)] = serialize(e.Value.ShortTerm); //e.Value.ShortTerm.Score;
                    dict[string.Format("{0}-mt", e.Key)] = serialize(e.Value.MidTerm); //e.Value.MidTerm.Score;
                    dict[string.Format("{0}-lt", e.Key)] = serialize(e.Value.LongTerm); //e.Value.LongTerm.Score;
                }

                //loggo i valori
                dict.ToList().ForEach(o =>
                {
                    var row = string.Format("{0},{1}", o.Key, o.Value.ToString());
                    logger.Log(row);
                });

            }
        }

        /// <summary>
        /// svuota tutti i valutatori
        /// </summary>
        public void Clear() {
            this.Evaluators = new Dictionary<string, EvaluatorSet>();
        }






    }
}
