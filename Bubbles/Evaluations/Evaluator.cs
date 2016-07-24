using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Evaluations
{
    class Evaluator
    {
        /// <summary>
        /// Massimo punteggio assegnabile ad una singola preedizione
        /// </summary>
        public const int MAX_POINTS_PER_PREDICTION = 1;

        /// <summary>
        /// punteggio assoluto totale del valutatore
        /// </summary>
        public int TotPoints { get; protected set; }

        /// <summary>
        /// punteggio assoluto attuale
        /// </summary>
        public int Points { get; protected set; }

        /// <summary>
        /// Esprime, in percentuale, il punteggio del valutore
        /// Ritorna -1 se non ci sono state valutazioni
        /// </summary>
        public Decimal Score {
            get {
                if (TotPoints == 0) return -1;
                return ((decimal)Points / TotPoints);
            }
        }

        /// <summary>
        /// quante predizioni ho fatto?
        /// </summary>
        public int NumberOfPredictions { get {
            return this.TotPoints / MAX_POINTS_PER_PREDICTION;
        } }

        /// <summary>
        /// mandien
        /// </summary>
        protected List<Prediction> PendingPredictions { get; set; }

        /// <summary>
        /// Esprime la durata della predizione in secondi (in candele?)
        /// </summary>
        public int PredictionDuration { get; set; }

        public Evaluator() {
            this.PendingPredictions = new List<Prediction>();
            this.Points = 0;
            this.TotPoints = 0;
        }


        /// <summary>
        /// Aggiunge una predizione al valutatore
        /// </summary>
        /// <param name="p"></param>
        public void Add(Prediction p) {
            this.PendingPredictions.Add(p);
        }

        /// <summary>
        /// smaltisce la coda di pending predictions esaminando le scadute
        /// </summary>
        /// <param name="date"></param>
        /// <param name="currentMarketValue">valore corrente nel mercato</param>
        public void Evaluate(DateTime now , decimal currentMarketValue) {

            var expired = this.PendingPredictions.Where(o => this.IsExpired(now, o.Date)).ToList();

            foreach (var p in expired) {
                if (p.Type != PredictionType.Unknown) //se non so che dire, non faccio nulla
                {
                    this.TotPoints += MAX_POINTS_PER_PREDICTION;
                    this.Points += this.EvaluatePrediction(p.Type, p.Value, currentMarketValue);
                }
            }


            this.PendingPredictions.RemoveAll(o => expired.Contains(o));
            

        }

        /// <summary>
        /// Valuta la singola predizione
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="marketValue"></param>
        /// <returns></returns>
        protected int EvaluatePrediction(PredictionType type, decimal value, decimal marketValue) {

            switch (type) { 

                case PredictionType.Unknown:
                    return 0;
                    break;
                case PredictionType.Equal:
                    var tollerance = 0.01M;
                    if (value * (1 + tollerance) >= marketValue && value * (1 - tollerance) <= marketValue) {
                        return MAX_POINTS_PER_PREDICTION;
                    }
                    break;
                case PredictionType.GreatherThan:
                    if (marketValue  >= value )
                    {
                        return MAX_POINTS_PER_PREDICTION;
                    }
                    break;
                case PredictionType.LessThan:
                    if (marketValue <= value)
                    {
                        return MAX_POINTS_PER_PREDICTION;
                    }
                    break;

            }

            return 0;

        }
        

        /// <summary>
        /// condizione per cui una predizione è scaduta
        /// </summary>
        /// <param name="now"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        protected bool IsExpired(DateTime now, DateTime d) {
            return (now - d).TotalSeconds >= this.PredictionDuration;
        }


    }
}
