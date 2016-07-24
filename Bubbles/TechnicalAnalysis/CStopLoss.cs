namespace Bubbles.TechnicalAnalysis
{
    public class  CStopLoss
    {
        private decimal Max { get; set; }
        public decimal Percent { get; set; }
        public decimal InitialValue { get; set; }
        public decimal SellCounter { get; set; }

        public CStopLoss()
        {
            Max = decimal.MinValue;
            SellCounter = 0;
        }

        public void Add(decimal value)
        {
            if (value > Max)
            {

                if (Max == decimal.MinValue)
                {
                    InitialValue = value;
                }

                Max = value;
                
            }
        }

        public void Reset()
        {
            Max = decimal.MinValue;
            InitialValue = decimal.MinValue;
        }

        public bool IsSell(decimal value)
        {

            if (Percent == 0)
            {
                return false;
            }

            if (Max == decimal.MinValue)
            {
                return false;
            }


            if (value <= Max*(1 - Percent))
            {
                return true;
            }

            return false;
        }

    }
}
