using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Log
{
    /// <summary>
    /// Elenco di operazioni in caso di file già esistente
    /// </summary>
    public enum FileAccess
    {
        Append, //apre il file e comincia a scrivere dalla fine
        Overwrite, //cancella il precedente file e ne crea uno nuovo
        Ignore, //non permette la creazione del nuovo file, ma non esegue alcuna azione
        Throw //non permette la creazione di un nuovo file, e lancia una eccezione
    };
}
