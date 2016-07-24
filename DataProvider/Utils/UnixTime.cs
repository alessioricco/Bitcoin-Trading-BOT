using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Utils
{
    /// <summary>
    /// classe per le conversioni da datetime a unixtime e viceversa
    /// </summary>
    public static class UnixTime
    {
        static DateTime _unixEpoch = new DateTime(1970, 1, 1);

        public static UInt32 Now
        {
            get
            {
                return GetFromDateTime(DateTime.UtcNow);
            }
        }

        public static UInt32 GetFromDateTime(DateTime d)
        {
            var dif = d - _unixEpoch;
            return (UInt32)dif.TotalSeconds;
        }

        public static DateTime ConvertToDateTime(UInt32 unixtime)
        {
            return _unixEpoch.AddSeconds(unixtime);
        }

        public static DateTime ConvertToDateTime(string unixtime)
        {
            UInt32 t = UInt32.Parse(unixtime);
            return ConvertToDateTime(t);
        }
    }
}
