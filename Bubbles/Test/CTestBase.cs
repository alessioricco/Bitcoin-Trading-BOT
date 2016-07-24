using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Test
{
    /// <summary>
    /// Classe base per i test da effettuare
    /// </summary>
    public abstract class CTestBase// : ITest
    {
        public abstract void Run();
        public abstract void Customize(TestTrading.TestParam testParam);
        public abstract List<TestTrading.TestParam> GetTestData();
    }
}
