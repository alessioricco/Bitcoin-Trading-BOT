using System;
using System.Globalization;
using System.Linq;
using Bubbles.Analytics;
using Bubbles.Market;

using Bubbles.TechnicalAnalysis;

namespace Bubbles.Manager
{
    class CManagerTrading : CManagerBase
    {
        // percentuale di capitale che posso investire
        public decimal PercCapital { get; set; }
        public bool Conservative { get; set; }

        public int TotBid { get; set; }
        public int TotAsk { get; set; }
        //public int TotStopLoss { get; set; }
        //public decimal MaxValue { get; set; }
        //public decimal MaxCoins { get; set; }

        private const decimal MinBitCoin = 0.01M;

        // percentale di ema che posso considerare situazione di stallo
        //public decimal PercNoAction { get; set; }
        // implementa lo stoploss
        //private readonly CStopLoss _stoploss;

        /*
        public CManagerTrading(ConfigurationHoldAndBuy configuration, CMarket market, bool verbose, decimal percCapital,
            decimal percNoAction, decimal stopLossPerc) : base(configuration, market, verbose)
        {
            this.PercCapital = (decimal) 1.0;
            //this.PercNoAction = percNoAction;
            _stoploss = new CStopLoss() { Percent = (decimal)stopLossPerc };
        }
        */

        public override bool OnTick()
        {

            return true;

        }

        private TradeAction _lastTradeActionSuggested = TradeAction.Unknown;
        private CUtility.OrderType? _lastTradeActionExecuted = null;
        private decimal _lastMoney = -1; // non si puo' usare decimal.MinValue
        private decimal _lastCoin = -1; // non si puo' usare decimal.MinValue

        
        public override bool OnHeartBeat()
        {
            const decimal gain = 0M; // sellat, buyat

            // devo vendere o devo comprare?             
            var action = this.SuggestedAction; 

            /*
            // mi salvo il massimo value
            if (Market.TotValue > this.MaxValue)
            {
                this.MaxValue = Market.TotValue;
            }
            
            if (Market.TotCoins > this.MaxCoins)
            {
                this.MaxCoins = Market.TotCoins;
            }
            */
            /**************************************************************
            CHECK CONFERMA (DOPPIO COMANDO)
             **************************************************************/
            // questa parte di codice esegue l'operazione se c'è una conferma
            if (action != TradeAction.StrongBuy && action != TradeAction.StrongSell)
            {
                if (action != _lastTradeActionSuggested)
                {
                    //CUtility.Log("refused: wait confirm with 2nd command");
                    LogTrade.Motivation = "refused: wait confirm with 2nd command";
                    _lastTradeActionSuggested = action;
                    action = TradeAction.Unknown;
                }
            }
            else
            {
                LogTrade.Motivation = ("refused: single command");
                _lastTradeActionSuggested = TradeAction.Unknown;
            }

            LogTrade.Action = action;

            /**************************************************************
            BUY
             **************************************************************/
            // compro o vendo?
            string orderId = "";
            if (action == TechnicalAnalysis.TradeAction.Buy || action == TechnicalAnalysis.TradeAction.StrongBuy)
            {

                /*
                 * per comprare devo usare il valore ask
                 */
                var buyvalue = Market.Buy;

                var money = (Market.TotMoney-SavedMoney)*PercCapital; // impegno solo una percentuale del capitale

                SavedMoney += (Market.TotMoney - SavedMoney) * (1-PercCapital);

                if (money == 0)
                {
                    LogTrade.Motivation = ("refused: no money");
                    goto exitif;
                }

                if (PercCapital < 1.0M)
                {
                    // se uso una percentuale, allora devo evitare che ripetuti comandi
                    // uguali, mi facciamo dissipare il risparmio accantonato
                    if (_lastTradeActionExecuted == CUtility.OrderType.Buy)
                    {
                        goto exitif;
                    }
                }

                if (Conservative)
                {
                    // impegno sempre non più di una certa cifra 
                    // sia per vendere che per comprare
                    if (money > MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest)
                    {
                        SavedMoney += (money - MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest);
                        money = MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest;
                        
                    }
                }

                var amount = money / buyvalue; // coins
                if (amount > MinBitCoin)
                {
                    // compro solo se i coin risultanti sono più dei precedenti
                    var coins = (amount) * (1 - Market.Fee) ;

                    /* onlyIfGain attiva o disattiva il blocco delle vendite se i soldi ricavati sono inferiori rispetto alla vendita precedente */
                    var onlyIfGain = MainConfiguration.Configuration.GreedyWithCoins; // --> disabilità il blocco
                    var greedyPerc = MainConfiguration.Configuration.GreedyWithCoinsGain;


                    if (MainConfiguration.Configuration.GreedyWithLongTermTrend)
                    {
                        // diventa avido se il trend a lungo termine è in discesa
                        onlyIfGain = this.CurrentLongTermTrend == Trend.Fall;
                        var longTermCoefficienteAngolare = this.Market.CandleMaker.LongTermTrendSpeed();

                        greedyPerc = GreedyNess(longTermCoefficienteAngolare);
                    }

                    if (!onlyIfGain || action == TechnicalAnalysis.TradeAction.StrongBuy || coins > _lastCoin*greedyPerc)
                    {

                        // evito che il trading mi mandi sotto del numero iniziale di coins

                        if (MainConfiguration.Configuration.UndervalueAllowed || (coins + SavedCoins) > MainConfiguration.Configuration.StartWalletCoins * (1 - MainConfiguration.Configuration.UndervalueTolerance))
                        {
                            _lastCoin = coins;

                            var result = Market.DoOrder(CUtility.OrderType.Buy, buyvalue, amount, ref orderId, true);
                            if (result.Length > 0)
                            {
                                LogTrade.Motivation = "BUY";
                                _lastTradeActionExecuted = CUtility.OrderType.Buy;
                                TotBid++;

                                // avvisa l'analitic che l'ultima label inserita con AnalyticsTools.Call(xxxx) è completata
                                AnalyticsTools.Confirm();

                                SellAt = (1 + gain) * (buyvalue * (1 + (2 * Market.Fee)));
                                BuyAt = 0;
                            }
                            else
                            {
                                // c'è stato un fallimento
                                LogTrade.Motivation = "BUY FAILED";
                            }
                        }
                        else
                        {
                            LogTrade.Motivation = ("under my initial value");
                        }
                    }
                    else
                    {
                        LogTrade.Motivation = ("greedy of coins: waiting for better performance " + Math.Round(_lastCoin * greedyPerc,4).ToString(CultureInfo.InvariantCulture));
                    }
                }
                // _stoploss.Reset();
            }


            /**************************************************************
            SELL
             **************************************************************/
            if (action == TechnicalAnalysis.TradeAction.Sell || action == TechnicalAnalysis.TradeAction.StrongSell )
            {

                var sellvalue = Market.Sell;

                var amount = (Market.TotCoins-SavedCoins)*PercCapital;// - SavedCoins;

                SavedCoins += (Market.TotCoins - SavedCoins) * (1-PercCapital);

                if (amount == 0)
                {
                    LogTrade.Motivation = ("refused: no coins");
                    goto exitif;
                }

                if (PercCapital < 1.0M)
                {
                    // se uso una percentuale, allora devo evitare che ripetuti comandi
                    // uguali, mi facciamo dissipare il risparmio accantonato
                    if (_lastTradeActionExecuted == CUtility.OrderType.Sell)
                    {
                        goto exitif;
                    }
                }


                if (Conservative)
                {
                    // impegno sempre non più di una certa cifra 
                    // sia per vendere che per comprare
                    var maxmoney = MainConfiguration.Configuration.ManagerConservativeMaxMoneyToInvest;
                    if (amount > maxmoney / sellvalue)
                    {
                        SavedCoins += (amount - maxmoney / sellvalue);
                        if (SavedCoins > 100)
                        {
                            CUtility.Log("error");
                        }
                        amount = maxmoney / sellvalue;
                    }
                }

                if (amount > MinBitCoin)
                {

                    var money = (amount * sellvalue) * (1 - Market.Fee);

                    /* onlyIfGain attiva o disattiva il blocco delle vendite se i soldi ricavati sono inferiori rispetto alla vendita precedente */
                    var onlyIfGain = MainConfiguration.Configuration.GreedyWithMoney; //  --> disabilita il blocco
                    var greedyPerc = MainConfiguration.Configuration.GreedyWithMoneyGain;

                    if (MainConfiguration.Configuration.GreedyWithLongTermTrend)
                    {
                        // diventa avido se il trend a lungo termine è in salita
                        onlyIfGain = this.CurrentLongTermTrend == Trend.Raise;
                        var longTermCoefficienteAngolare = this.Market.CandleMaker.LongTermTrendSpeed();

                        greedyPerc = GreedyNess(longTermCoefficienteAngolare);
                    }

                    if (!onlyIfGain || action == TechnicalAnalysis.TradeAction.StrongSell || money > _lastMoney * greedyPerc)
                    {

                        // evito che il trading mi butti sotto del numero iniziale di soldi
                        if (MainConfiguration.Configuration.UndervalueAllowed || (money + SavedMoney) > MainConfiguration.Configuration.StartWalletMoney * (1 - MainConfiguration.Configuration.UndervalueTolerance))
                        {

                            // vendo solo se la vendita produce piu' soldi di prima
                            _lastMoney = money;

                            var result = Market.DoOrder(CUtility.OrderType.Sell, sellvalue, amount, ref orderId, true);
                            if (result.Length > 0)
                            {
                                TotAsk++;

                                BuyAt = (1 - gain) * (sellvalue * (1 - (2 * Market.Fee)));
                                SellAt = 0;

                                // avvisa l'analitic che l'ultima label inserita con AnalyticsTools.Call(xxxx) è completata
                                AnalyticsTools.Confirm();

                                _lastTradeActionExecuted = CUtility.OrderType.Sell;
                                LogTrade.Motivation = "SELL";
                            }
                            else
                            {
                                LogTrade.Motivation = "SELL FAILED";
                            }
                            /*
                            if (action == TechnicalAnalysis.TradeAction.SellStopLoss)
                            {
                                TotStopLoss++;
                            }
                             */
                        }
                        else
                        {
                            LogTrade.Motivation = ("under my initial value");
                        }
                    }
                    else
                    {
                        LogTrade.Motivation = ("greedy of money: waiting for better performance " + Math.Round(_lastMoney * greedyPerc,4).ToString(CultureInfo.InvariantCulture));
                    }
                }
            }


            exitif:

            /**************************************************************
            LOG
             **************************************************************/
            Log(string.Format("B:{0} A:{1} SL:{2} Max$:{3} MaxC:{4} MaxV:{5}", TotBid, TotAsk, 0, Math.Round(Market.MaxMoney, 4).ToString(CultureInfo.InvariantCulture), Math.Round(Market.MaxCoins, 4).ToString(CultureInfo.InvariantCulture), Math.Round(Market.MaxValue, 4).ToString(CultureInfo.InvariantCulture)));
            LogTrade.BuyAt = BuyAt;
            LogTrade.SellAt = SellAt;

            return true;
        }

        protected decimal GreedyNess(decimal longTermCoefficienteAngolare)
        {
            //trasformazione lineare tra il coefficente di long term trend e il valore di greedynes
            //faccio una semplice proporzione  0 : minGreedyNess = LongTermMaxCoeff : maxGreedyNess

            const decimal maxGreedyNess = 0.6M;
            const decimal minGreedyNess = 0.4M;
            const decimal minCoeff = 0M;
            var maxCoeff = Market.CandleMaker.LongTermMaxCoeff();
            var greedyness = longTermCoefficienteAngolare * ((maxGreedyNess - minGreedyNess) / (maxCoeff - minCoeff)) + minGreedyNess;
            return 1 + Math.Abs(greedyness);
        }
    }
}
