using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Extensions
{
    /// <summary>
    /// Metodi di estensione per il tipo DateTime
    /// </summary>
    static class DateTimeExtensions
    {
       


        /// <summary>
        /// converte la data in unix time
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static UInt32 ToUnixTime(this DateTime d) {
            return Utils.UnixTime.GetFromDateTime(d);
        }



    }
}
