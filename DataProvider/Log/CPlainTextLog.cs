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
   public class CPlainTextLog : AFileLog<String>
    {

        public CPlainTextLog(string filename, FileAccess ifFileExists = FileAccess.Overwrite)
            : base(filename, ifFileExists)
        {

        
        }




        protected override string FormatItem(string item)
        {
            return item ;
        }
    }
}
