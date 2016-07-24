using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Market;

namespace Bubbles.SQLite
{
    class SQLiteMtGox
    {
        public string Currency { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CurrentTime { get; set; }


        public decimal Ask { get; set; }
        public decimal Bid { get; set; }
        public DateTime MarketTime { get; set; }

       private readonly SqLiteDatabase _db = new SqLiteDatabase();

        public bool Start()
        {
            CurrentTime = StartTime;
            return true;
        }

        public bool Next(long seconds)
        {


            var currentTimeUx = CUtility.UnixTime.GetFromDateTime(CurrentTime);

            var nextTime = CurrentTime.AddSeconds(seconds);

            if (nextTime > this.EndTime)
            {
                return false;
            }

            var nextTimeUx = CUtility.UnixTime.GetFromDateTime(nextTime);

            var error = false;
            try
            {
                var dt =
                    _db.GetDataTable(
                        string.Format(
                            "select date,price from trades where currency='{0}' and {1} < date and date <= {2} and real=1 order by tid desc limit 1",
                            this.Currency, currentTimeUx, nextTimeUx));

                var dateUx = (long) dt.Rows[0].ItemArray[0];
                var date = CUtility.UnixTime.ConvertToDateTime((uint) dateUx);
                this.MarketTime = date;

                var ask = (double) dt.Rows[0].ItemArray[1];
                this.Ask = (decimal) ask;
                this.Bid = this.Ask;

            }
            catch (Exception ex)
            {
                error = true;

            }
            finally
            {
                using (var sw = new StreamWriter(@"data\output\raw.csv", true))
                {
                    sw.WriteLine(string.Format("{0}, {1}, {2}, {3}", this.MarketTime.ToString("s"),
                        this.Ask.ToString(CultureInfo.InvariantCulture), this.Ask.ToString(CultureInfo.InvariantCulture),
                        error ? 1 : 0));
                    sw.Flush();
                }
                CurrentTime = nextTime;
            }


            return true;

        }



    }
}
