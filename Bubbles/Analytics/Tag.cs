using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bubbles.Manager;
using Bubbles.Market;

namespace Bubbles.Analytics
{

    internal static class AnalyticsTools
    {
        private static readonly TagList Taglist = new TagList();
        private static string CurrentTagLabel { get; set; }
        public static void Call(string label)
        {
            LogTrade.Indicator = label;
            CurrentTagLabel = label;
            Taglist.Call(CurrentTagLabel);
        }

        public static void Confirm()
        {
            Taglist.Confirm(CurrentTagLabel);
            CurrentTagLabel = "";
        }

        public static void Print()
        {
            Taglist.Print();
        }

        public static ProgressList ProgressList = new ProgressList();

    }

    class Progress
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Money { get; set; }
        public decimal Coins { get; set; }
        public decimal Value { get; set; }
    }

    class ProgressList
    {
        public List<Progress> List = new List<Progress>();

        public void Print(StreamWriter log)
        {
            foreach (Progress progress in List)
            {
                log.WriteLine(string.Format("{0},{1},{2},{3},{4}",
                    progress.Year,
                    progress.Month,
                    Math.Round(progress.Money,4).ToString(CultureInfo.InvariantCulture),
                     Math.Round(progress.Coins,4).ToString(CultureInfo.InvariantCulture),
                      Math.Round(progress.Value,4).ToString(CultureInfo.InvariantCulture)
                    ));
            }
        }
    }

    class Tag
    {
        public string Label { get; set; }
        public int Called { get; set; }
        public int Confirmed { get; set; }
    }

    class TagList
    {
        private readonly Hashtable _tags = new Hashtable();

        public void Call(string label)
        {
            if (_tags.Contains(label))
            {
                var t = (Tag) _tags[label];
                t.Called++;
            }
            else
            {
                var t = new Tag() { Called = 1, Label = label };
                _tags.Add(label,t);
            }
        }

        public void Confirm(string label)
        {
            if (_tags.Contains(label))
            {
                var t = (Tag)_tags[label];
                t.Confirmed++;
            }
            else
            {
                var t = new Tag() { Confirmed = 1, Label = label };
                _tags.Add(label, t);
            }
        }

        public void Print()
        {
            foreach (Tag t in _tags.Values)
            {
                CUtility.Log(string.Format("{0} {1} {2}",t.Label,t.Called,t.Confirmed));
            }
        }
    }

    public class Statistics
    {
        public decimal StartValue { get; set; }
        public decimal EndValue { get; set; }
        public decimal Gain { get { return StartValue != 0 ? 100 * (EndValue - StartValue) / StartValue : 0; } }

        public decimal AverageDailyPrice { get; set; }


    }
}
