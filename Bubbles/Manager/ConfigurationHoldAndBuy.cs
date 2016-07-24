using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Manager
{
    class ConfigurationHoldAndBuy
    {
        // massima quantità di soldi da investire
        //public decimal MaxMoney { get; set; }

        // numero massimo di bolle
        public decimal MaxBubbles { get; set; }
        // massima quantità di coins per bolla
        public decimal MaxCoinsPerBubble { get; set; }
        //
        public decimal MinimumCoinsPerBubble { get; set; }

        // battito del cuore (1m)
        //public decimal HeartBeat { get; set; }
        // durata del tick rispetto all'hearthbeat (5m)
        //public int Tick { get; set; }
        // il tick dinamico: se true il tick viene calcolato in base all'EMA, se false viene usato quello di default
        //public bool TickDynamic { get; set; }

        // guadagno desiderato per transazione
        public decimal Gain { get; set; }


    }
}
