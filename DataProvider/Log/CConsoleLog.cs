using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using DataProvider.Extensions;

namespace DataProvider.Log
{
    public class CConsoleLog<T> : ALogBase<T>
    {
        public override void Log(T item)
        {
            if (!this.Enabled) return;

            Console.WriteLine(string.Format("{0} {1}", 
                DateTime.Now.ToString(CultureInfo.InvariantCulture), 
                this.FormatItem(item)));
        }

        /// <summary>
        /// implementa la formattazione testuale che descrive l'oggetto
        /// </summary>
        /// <param name="item">oggetto da formattare</param>
        /// <returns></returns>
        protected virtual string FormatItem(T item) {
            return item.SerializeToMultiLine();
        }

    }
}
