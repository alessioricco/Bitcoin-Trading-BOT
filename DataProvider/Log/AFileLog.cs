using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{
    

    /// <summary>
    /// Classe astratta che implementa un log su file gestito tramite buffer
    ///  delega alla classe figlia di definire la formattazione dell'ouput
    /// </summary>
    public abstract class AFileLog<T> : ABufferedLog<T>
    {
       

        /// <summary>
        /// flag che blocca/sblocca la scrittura del file
        /// </summary>
        protected bool Lock { get; set; }


        /// <summary>
        /// cartella di default che contiene i log
        /// </summary>
        public  const string DEFAULT_FOLDER = @"data\output";

        /// <summary>
        /// file su cui scrivere
        /// </summary>
        private string Filename { get; set; }

        /// <summary>
        /// costruttore
        /// </summary>
        /// <param name="filename">nome del file su cui scrivere il log</param>
        /// <param name="ifFileExists">azione da intraprendere in caso il file si già esistente</param>
        public AFileLog(string filename, FileAccess ifFileExists = FileAccess.Overwrite ) : base() {
            this.Filename = filename;

            //operazioni da fare in caso di file già esistente
            var path = GetFullPath();
            if (File.Exists(path)) { 
                switch(ifFileExists){
                    case FileAccess.Append:
                        //scrittura abilitata
                        this.Lock = false;
                        break;
                    case FileAccess.Overwrite:
                        //cancello il vecchio file
                        File.Delete(path);
                        break;
                    case FileAccess.Throw:
                        //genero un'eccezione
                        throw new Exception("Log.AFileLog - file già esistente, file: " + this.Filename);
                        break;
                    case FileAccess.Ignore:
                        //scrittura disabilitata
                        this.Lock = true;
                        break;
                }
            }
        }

      

        /// <summary>
        /// implementa la formattazione testuale che descrive l'oggetto
        /// </summary>
        /// <param name="item">oggetto da formattare</param>
        /// <returns></returns>
        protected abstract string FormatItem(T item);


        protected override void WriteAll()
        {
            //controllo il lock del log
            if (this.Lock) return;

            if (!this.Enabled) return;

            var path = GetFullPath();

            //apro il file
            using (var fs = new FileStream(path,
                System.IO.FileMode.Append,
                System.IO.FileAccess.Write,
                System.IO.FileShare.ReadWrite))
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fs))
                {
                    //itero il contenuto del buffer
                    foreach (T i in this.Buffer)
                    {
                        //appendo il contenuto al file
                        var line = this.FormatItem(i);
                        file.WriteLine(line);                    
                    }
                }//chiudo il file

        }


        /// <summary>
        /// semplige getter per formattare il full path del file
        /// </summary>
        /// <returns></returns>
        protected string GetFullPath() {

            return string.Format(@"{0}\{1}", DEFAULT_FOLDER, this.Filename); 
        }

        
    }
}
