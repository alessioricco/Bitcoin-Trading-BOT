using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{
    /// <summary>
    /// classe astratta che accomuna gli oggetti capaci di loggare le attività
    /// in caso si vogliano accettare oggetti anonimi, specificare Object come tipo
    /// </summary>
   public  abstract class ALogBase<T> 
    {
        /// <summary>
        /// Logga un elemento
        /// </summary>
        /// <param name="item">elemento da aggiungere al log</param>
        public abstract void Log(T item);

        public bool Enabled { get; set; }

        public ALogBase() {
            this.Enabled = true;
        }

    }
}
