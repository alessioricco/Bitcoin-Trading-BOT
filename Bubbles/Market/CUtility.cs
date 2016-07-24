using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bubbles.Market
{
    public class CUtility
    {
        public enum OrderType
        {
            Buy,
            Sell
        };

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
        }
        /*
        public static void Pause(int seconds = 5)
        {
            for (var s = 0; s < seconds; s++)
            {
                System.Windows.Forms.Application.DoEvents();
                int pauseTime = 1000;
                Thread.Sleep(pauseTime);

            }
        }
        */
        public static void Log(string s)
        {
            //Console.WriteLine( DateTime.Now.ToString(CultureInfo.InvariantCulture) + " " + s);
            Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), s));
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime(); //.ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        public static string Base64Encode(string str)
        {
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        public static string Base64Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(decbuff);
        }
    }
}
