using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Utils
{
    /// <summary>
    /// classe che raccoglie metodi statici per la gestione dei csv
    /// </summary>
    class CsvUtility
    {
        /// <summary>
        /// Metodo che formatta un oggetto in una riga di csv
        ///  ispirato a http://stackoverflow.com/questions/1179816/best-practices-for-serializing-objects-to-a-custom-string-format-for-use-in-an-o
        /// </summary>
        /// <param name="o">oggetto da formattare</param>
        /// <param name="separator">separatore</param>
        /// <returns>stringa formattata csv</returns>
        public static string ObjectToCsvRow(object o, string separator = ",")
        {
            StringBuilder linie = new StringBuilder();

            Type t = o.GetType();
            //FieldInfo[] fields = t.GetFields();
            PropertyInfo[] fields = t.GetProperties();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }

        /// <summary>
        /// Metodo che estrae l'intestazion csv da un oggetto
        ///  ispirato a http://stackoverflow.com/questions/1179816/best-practices-for-serializing-objects-to-a-custom-string-format-for-use-in-an-o
        /// </summary>
        /// <param name="o">oggetto</param>
        /// <param name="separator">separatore</param>
        /// <returns>header csv</returns>
        public static string ObjectToCsvHeader(object o, string separator = ",")
        {

            Type t = o.GetType();

            return ObjectToCsvHeader(t, separator);

        }

        /// <summary>
        /// Metodo che estrae l'intestazion csv da un oggetto
        ///  ispirato a http://stackoverflow.com/questions/1179816/best-practices-for-serializing-objects-to-a-custom-string-format-for-use-in-an-o
        /// </summary>
        /// <param name="o">oggetto</param>
        /// <param name="separator">separatore</param>
        /// <returns>header csv</returns>
        public static string ObjectToCsvHeader(Type t, string separator = ",")
        {

            //  var fields = t.GetFields();
            var fields = t.GetProperties();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            return header;
        }


    }
}
