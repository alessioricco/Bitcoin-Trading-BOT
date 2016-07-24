using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Models.TradingData
{
    /// <summary>
    /// classe parziale che aggiunge feature a quanto creato tramite i t4
    /// </summary>
    public partial class TradingDataDB : LinqToDB.Data.DataConnection
    {
        public TradingDataDB()
        {
        }

        public TradingDataDB(string configuration)
            : base(configuration)
        {
        }

        #region  tables

        /// <summary>
        /// Tabella con lo storico delle compravendite
        /// </summary>
        public Table<Trade> Trade { get; set; }

        #endregion  tables
    }
}
