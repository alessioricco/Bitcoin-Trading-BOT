using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{
    /// <summary>
    /// classe astratta che definisce il comportamento di un logger che implementa un buffer
    /// </summary>
    /// <typeparam name="T">tipo degli oggetti da loggare</typeparam>
    public abstract class ABufferedLog<T> : ALogBase<T> , IDisposable
    {

        /// <summary>
        /// buffer del logger
        /// </summary>
        protected List<T> Buffer { get; set; }

        /// <summary>
        /// dimensione massima del buffer
        ///  valori speciali:
        ///   0     -   ogni elemento viene scritto immediatamente
        ///   -1    -   gli elementi vengono scritti solo al flush   
        /// </summary>
        public int BufferSize { get; set; }

        public ABufferedLog() {
            this.Buffer = new List<T>();
        }


        /// <summary>
        /// aggiunge un elemento al buffer
        ///  se il buffer è pieno, lo svuota 
        /// </summary>
        /// <param name="item">oggetto da inserire nel buffer</param>
        public override void Log(T item) {

            if (!this.Enabled) return;


            this.Buffer.Add(item);
            if (this.BufferSize > 0 //esclude il valore -1 che corrisponde a buffer infinito
                &&  this.Buffer.Count > this.BufferSize) //gestisce il raggiunto limite del buffer (BufferSize+1)
            {
                this.Flush();
            }
        }

        /// <summary>
        /// Metodo che scrive il contentuto attuale del buffer sull'ouptut selezionato, e poi ne cancella il contenuto
        /// </summary>
        public virtual void Flush()
        {
            this.WriteAll();
            this.ClearBuffer();
        }

        /// <summary>
        /// pulisce il buffer corrente
        /// </summary>
        public virtual void ClearBuffer()
        {
            this.Buffer.Clear();
        }

        /// <summary>
        /// metodo che ricorre il buffer e richiama il metodo di scrittura specifico per l'output selezionato
        /// </summary>
        protected abstract void WriteAll();

        
      
        /// <summary>
        /// metodi da eseguire al dispose dell'oggetto
        /// </summary>
        void IDisposable.Dispose()
        {
            //al dispose, scrivo quello che ho da scrivere
            this.Flush();
        }
    }
}
