using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Bubbles.Bubble;
using Bubbles.Market;

namespace Bubbles.Manager
{
    class CManager
    {

        private readonly CConfiguration _configuration;
        private readonly CMarket _market;
        private readonly System.Timers.Timer _heartBeatTimer = new System.Timers.Timer();

        private int _tickCounter = 0;
        private int _ticksPerHearthBeat = 0;

        private const bool EnableBubbleDestruction = false;
        private const decimal BubbleDestructionLevel = (decimal) 1.25;

        private int _bubblesCreated = 0;
        private int _bubblesExploded = 0;
        private int _bubblesCanceled = 0;

        // whishlist è l'elenco delle bolle che il manager vorrebbe
        private readonly List<CBubble> _wishlist = new List<CBubble>();
        public List<CBubble> WishList { get { return _wishlist; } }

        //private int _trend = -1;
        //private decimal _ema = -1;

        Stopwatch _stopwatch = new Stopwatch();

        public  CManager(CConfiguration configuration, CMarket market)
        {
            this._configuration = configuration;
            this._market = market;

            // il manager potrebbe cambiare il valore settato nella configurazione
            _ticksPerHearthBeat = configuration.Tick;
        }

        public void Start()
        {
            _stopwatch.Start();
            // create an heartbeat
            _heartBeatTimer.Elapsed += OnHeartBeat;
            _heartBeatTimer.Interval = (double)_configuration.HeartBeat * 60 * 1000;
            _heartBeatTimer.Enabled = true;

        }

        // Specify what you want to happen when the Elapsed event is raised.
        private  void OnHeartBeat(object source, ElapsedEventArgs e)
        {
            try
            {


                // è scattato un heartbeat
                //var stop = false;
                CUtility.Log("*** heartbeat");

                // verifico lo stato del mercato
                _market.GetTicker();

                // aggiorno la mia situazione
                _market.GetWallet();

                /*
                // trend
                _trend = _market.Trend.CalculateTrend();

                // ema
                _ema = _market.Trend.Ema;

                // il trend deve essere affidabile altrimenti esce
                if (!_market.Trend.TrendIsValid)
                {
                    return;
                }
                */
                if (!_market.CandleMaker.Analyze.IsValid())
                {
                    return;
                }
                

                // elimina le bolle (ad ogni tick.. prima si fa e meglio è)
                var hasExplodedBubbles = CheckBubbles();

                // check wishlist
                var hasResolvedWishList = CheckWishList();

                if (!hasExplodedBubbles && !hasResolvedWishList)
                {
                    // strategia per il tick
                    if (_configuration.TickDynamic)
                    {
                        _ticksPerHearthBeat = NextTick(_ticksPerHearthBeat);
                    }

                    // verifico se deve scattare un tick
                    _tickCounter++;
                    if (_tickCounter >= _ticksPerHearthBeat)
                    {
                        // non uso il mod perchè potrei dover usare un 
                        // cambio dinamico al tickcounter
                        _tickCounter = 0;
                        OnTick();

                    }
                }
                // al termine dei task decido se fermare il processo oppure no

                if (_market.Stop)
                {
                    _stopwatch.Stop();
                    _heartBeatTimer.Enabled = false;
                    CUtility.Log("Time Elapsed : " + _stopwatch.Elapsed);
                }

                // secondo me questo è il momento in cui vanno salvate le persistenze
            }
            catch (Exception)
            {
                CUtility.Log("Error in hearthbeat : ");
                //throw;
            }
            finally
            {
                CUtility.Log("Created  : " + this._bubblesCreated);
                CUtility.Log("Exploded : " + this._bubblesExploded);
                CUtility.Log("Canceled : " + this._bubblesCanceled);
                CUtility.Log("Whish    : " + this.WishList.Count);
            }
        }

        /// <summary>
        /// il tick è il momento in cui vengono prese le decisioni di buy/sell
        /// </summary>
        private void OnTick()
        {
            CUtility.Log("*** tick");

            // in questo simulatore se il mercato scende non ci muoviamo
            if (_trend < 0)
            {

                // verifico se ci sono bolle irraggiungibili
                if (EnableBubbleDestruction)
                {
                    CheckBubblesToCancel();
                }

                CUtility.Log("*** trend down... skip");
                return;
            }

            // analizzo il numero di bolle, per crearne almeno una
            var numBubbles = _market.Bubbles.Count() + WishList.Count();
            if (numBubbles < _configuration.MaxBubbles)
            {
                CreateBubble();
            }

            // mostro le bolle
            _market.ShowBubbles();
        }


        private void CreateBubble()
        {

            CUtility.Log("*** order buy");
            var marketValue = _market.Bid;
            string orderId = "";

            // non compro perchè il valore di mercato è circa il 10% di altre bolle presenti
            if (_market.Bubbles.Any(o => Math.Abs(o.MarketValue - marketValue) <= marketValue/10))
            {
                return;
            }


            // soldi nel wallet, ma non piu' del massimo consentito
            decimal availableMoney = _market.TotMoney;
            if (availableMoney > _configuration.MaxMoney)
            {
                availableMoney = _configuration.MaxMoney;
            }

            // come creo una bolla ? 
            var money = availableMoney / _configuration.MaxBubbles;
            var coins = money / marketValue;

            if (coins > _configuration.MaxCoinsPerBubble)
            {
                coins = _configuration.MaxCoinsPerBubble;
            }

            // verifico di non aver impiegato troppi soldi
            

            // verifico di non aver superato le soglie minime di coins e soldi

            _market.DoOrder(CUtility.OrderType.Buy, marketValue, coins, ref orderId, true);
            CUtility.Log("*** Bubble added");
            _bubblesCreated++;
            _market.Bubbles.Add(CBubble.Create(_market, CUtility.OrderType.Buy, coins, marketValue,
                _configuration.Gain, orderId));           
        }


        /// <summary>
        /// il manager esamina il mercato e decide quando sarà il prossimo tick
        /// </summary>
        /// <returns></returns>
        private int NextTick(int ticksPerHeartbeat)
        {
            //var ema = _market.Ema;
            // prende una decisione
            //return (int)Math.Round(currentTick * (ema < 0 ? -ema : ema)+(decimal)0.5);
            /*
            decimal newValue = ticksPerHeartbeat*(-(1+_ema));
            if (newValue < 0) newValue = 1;
            return decimal.ToInt32(newValue + (decimal) 0.5);
            */
            return ticksPerHeartbeat;
        }

        /// <summary>
        /// elimina le bolle che hanno raggiunto il guadagno
        /// </summary>
        private bool CheckBubbles()
        {
            if (_trend >= 0)
            {
                // se il trend è in salita aspetto (dovrebbe ottimizzare i guadagni)
                //return false;
            }

            if (_market.Bubbles.Count > 0 )
            {
                // scoppio le bolle
                foreach (CBubble bubble in _market.Bubbles)
                {
                    if (bubble.SellAt < _market.Ask)
                    {
                        //scoppio la bolla: cioè vendo i bitcoin per poterli riutilizzare
                        var marketValue = _market.Ask;
                        var orderId = "";
                        _market.DoOrder(CUtility.OrderType.Sell, marketValue, bubble.Coins, ref orderId, true);
                        CUtility.Log("*** Bubble removed");
                        _bubblesExploded++;
                        bubble.Deleted = true;

                        // occorrerebbe mettere un acquisto di btc appena sono convenienti

                    }
                }
                // rimuovo le bolle scoppiate
                var removed = _market.Bubbles.RemoveAll(o => o.Deleted == true);
                // se ho scoppiato bolle aspetto un tick
                if (removed > 0)
                {
                    _market.ShowBubbles();
                    //return;
                }
                return removed > 0;
            }
            return false;

        }

        private bool  CheckWishList()
        {
            // analizzo il numero di bolle, per crearne almeno una
            var numBubbles =  WishList.Count();
            if (numBubbles > 0 )
            {

                // cerco una wishlist soddisfatta
                foreach (CBubble bubble in WishList)
                {
                    if (bubble.ValueOfCoins*bubble.Coins > _market.Bid)
                    {
                        // rimuovo la bolla dalla wishlist e creo un bolla
                        bubble.Deleted = true;

                        CreateBubble();

                        //break;
                    }
                    
                }
                var removed = WishList.RemoveAll(o => o.Deleted == true);
                return removed > 0;
            }
            return false;
        }


        /// <summary>
        /// Vrifico se ci sono bolle particolamente vecchie da eliminare
        /// </summary>
        private void CheckBubblesToCancel()
        {
            //recupero le vecchie bolle solo se sono in una situazione di stallo e mi mancano bolle
            if (_market.Bubbles.Count + this.WishList.Count < _configuration.MaxBubbles*(decimal) 0.8)
            {
                return;
            }

            // ne creo solo una (distruggere le bolle costa)
            foreach (CBubble bubble in _market.Bubbles)
            {
                if (bubble.SellAt > _market.Ask * BubbleDestructionLevel)
                {
                    // distruggi la bolla 
                    var marketValue = _market.Ask;
                    var orderId = "";
                    _market.DoOrder(CUtility.OrderType.Sell, marketValue, bubble.Coins, ref orderId, true);
                    CUtility.Log("*** Bubble destroyed");
                    bubble.Deleted = true;
                    _bubblesCanceled++;

                    // crea una bolla in wishlist
                    WishList.Add(CBubble.Create(_market, CUtility.OrderType.Buy, bubble.Coins, bubble.BuyAt, _configuration.Gain,orderId));

                    break;
                }
            }
            var removed = _market.Bubbles.RemoveAll(o => o.Deleted == true);
        }



    }
}
