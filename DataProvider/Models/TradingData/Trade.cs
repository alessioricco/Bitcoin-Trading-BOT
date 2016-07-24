using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

namespace DataProvider.Models.TradingData
{
    /// <summary>
    /// Classe che mappa la tabella dello storico delle compravendite (trades)
    /// </summary>
    [Table(Name = "Products")]
   public partial class Trade
    {
        /// <summary>
        /// 
        /// </summary>
      //  [PrimaryKey, Identity]
       // public int Id { get; set; }

        [Column(Name = "Date"), NotNull]
        public DateTime Date { get; set; }

        [Column(Name = "Currency"), NotNull]
        public string Currency { get; set; }

        [Column(Name = "AuxiliaryCurrency"), NotNull]
        public string AuxiliaryCurrency { get; set; }

        [Column(Name = "Price"), NotNull]
        public Decimal Price { get; set; }

        [Column(Name = "Amount"), NotNull]
        public Decimal Amount { get; set; }

        [Column(Name = "MarketName"), NotNull]
        public string MarketName { get; set; }

        [Column(Name = "Type"), NotNull]
        public string Type { get; set; }

        [Column(Name = "Note"), NotNull]
        public string Note { get; set; }

        
    }
}
