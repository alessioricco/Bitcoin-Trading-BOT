using DataProvider.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{

    /// <summary>
    /// classe che scrive un csv su file
    /// </summary>
    public class CCsvLog<T> : AFileLog<T>
    {
        /// <summary>
        /// dice se bisogna scrive l'header
        /// </summary>
        protected bool HasToWriteHeader; 

        public CCsvLog(string filename, bool writeHeader = true, FileAccess ifFileExists = FileAccess.Overwrite) : base(filename, ifFileExists) {

            this.HasToWriteHeader = writeHeader;
        
        }


        protected override string FormatItem(T item)
        {
            return item.ToCsvRow();
        }

        protected override void WriteAll()
        {
            WriteHeader(); //già incapsula la logica per capire se scrivere o meno l'header
            base.WriteAll();
        }

        /// <summary>
        /// Scrive l'header del csv all'inizio del file
        /// </summary>
        protected void WriteHeader()
        {
            if (!this.HasToWriteHeader) return;

            //controllo il lock del log
            if (this.Lock) return;

            if (!this.Enabled) return;


            var path = GetFullPath();

            //apro il file
            using (var fs = new FileStream(path, 
                    System.IO.FileMode.OpenOrCreate, 
                    System.IO.FileAccess.ReadWrite, 
                    System.IO.FileShare.ReadWrite))
            using (StreamWriter file = new System.IO.StreamWriter(fs))
            {
                
                    //scrivo l'header in prima riga
                var e = this.Buffer.FirstOrDefault();
                if (e != null) {
                    var line = e.ToCsvHeader();
                    this.HasToWriteHeader = false;
                }
                

            }//chiudo il file  


        }
    }
}
