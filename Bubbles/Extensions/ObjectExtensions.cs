using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Extensions
{
    /// <summary>
    /// Raccolta di metodi estensione per  gli oggetti 
    /// </summary>
    static class ObjectExtensions
    {

        /// <summary>
        /// ottiene una rappresentazione in stringa dell'oggetto su una sola riga
        /// </summary>
        /// <param name="o"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string SerializeToLine(this object o,  char separator = ';') {
            StringBuilder linie = new StringBuilder();

            Type t = o.GetType();
            //FieldInfo[] fields = t.GetFields();
            PropertyInfo[] fields = t.GetProperties();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var v = f.GetValue(o);
                var k = f.Name;

                var s = string.Format("{0}:{1}", k, v == null ? "null" : v.ToString());

                    linie.Append(s);
            }

            return linie.ToString();
        }
        /// <summary>
        /// ottiene una rappresentazione in stringa dell'oggetto su più righe 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SerializeToMultiLine(this object o)
        {
            return o.SerializeToLine('\n');
        }


        /// <summary>
        /// estende l'oggetto o1 con le properietà  di o2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static void Extend<T>(this T o1, T o2) {

            if (o2 == null) return;

            Type t = typeof( T);

            PropertyInfo[] properties = t.GetProperties();

            foreach (var p in properties) {
                
                
                //nome della property
                var key = p.Name;
            
                //nuovo valore
                var newValue = p.GetValue(o2);

                //valuta se un oggetto è null, arricchiata con considerazioni sulle stringhe
                Func<object, bool> IsNull = (o => {
                    if (o == null) return true;
                    if (o is string && string.IsNullOrEmpty((string)o)) return true;
                    return false;
                });

                if (!IsNull(newValue)) {

                    p.SetValue(o1, newValue);

                }

            }

        }




        public static T To<T>(this object value)
        {
            Type t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Nullable type.

                if (value == null)
                {
                    // you may want to do something different here.
                    return default(T);
                }
                else
                {
                    // Get the type that was made nullable.
                    Type valueType = t.GetGenericArguments()[0];

                    // Convert to the value type.
                    object result = Convert.ChangeType(value, valueType);

                    // Cast the value type to the nullable type.
                    return (T)result;
                }
            }
            else
            {
                // Not nullable.
                return (T)Convert.ChangeType(value, typeof(T));
            }
        } 



    }
}
