using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Bubbles.TestCases
{
    /// <summary>
    /// raccolta di test per i metodi di log
    /// </summary>
    [TestFixture]
    class TestLog
    {

        #region "utilità"
        private Bubbles.TestUnit GetFakeTestUnitData() {
            var data = new Bubbles.TestUnit()
            {
                Average = 1,
                Bubbles = (decimal)0.5,
                Coins = 100,
                Coinvalue = (decimal)700.56,
                Gain = (decimal)0.05,
                i = 5,
                k = 3,
                j = 10,
                w = 10,
                Money = 65,
                Ticks = 1,
                Value = 100

            };

            return data;
        
        }
        #endregion "utilità"

        /// <summary>
        /// test per la creazione e salvataggio di un file di log in formato csv
        /// </summary>
        [Test]
        public void CreateCsvLogFile()
        {
            
            //creazione di dati fake
            int count = 20;
            var data = GetFakeTestUnitData();


            var file = string.Format(@"test{0}.csv", DateTime.Now.ToString("ddMMyyyyhhmm"));

            try
            {

                //creazione del log
                using (var L = new Log.CCsvLog<Bubbles.TestUnit>(file, false) { BufferSize = 3 })
                {
                    for (var i = 0; i < count; i++) L.Add(data);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("creazione fallita, error: " + ex.Message, ex);
            }

            //lettura del file per controllo
            // il test è ben eseguito se:
            //  - il file esiste
            //  - il file contiene [count] righe
            var file2 = string.Format(@"{0}\{1}", Log.CCsvLog<Bubbles.TestUnit>.DEFAULT_FOLDER, file);
            int found = System.IO.File.ReadLines(file2).Count();
            Assert.AreEqual(count, found, "numero righe file");

            
        }

        /// <summary>
        /// test per la creazione e salvataggio di un file di log in formato csv con intestazione
        /// </summary>
        [Test]
        public void CreateCsvLogFileWithHeader()
        {

            //creazione di dati fake
            int count = 20;
            var data = GetFakeTestUnitData();
            var file = string.Format(@"test{0}.csv", DateTime.Now.ToString("ddMMyyyyhhmm2"));

            try
            {

                //creazione del log
                using (var L = new Log.CCsvLog<Bubbles.TestUnit>(file, true) { BufferSize = 3 })
                {
                    for (var i = 0; i < count; i++) L.Add(data);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("creazione fallita, error: " + ex.Message, ex);
            }

            //lettura del file per controllo
            // il test è ben eseguito se:
            //  - il file esiste
            //  - il file contiene [count+1] righe
            var file2 = string.Format(@"{0}\{1}", Log.CCsvLog<Bubbles.TestUnit>.DEFAULT_FOLDER, file);
            int found = System.IO.File.ReadLines(file2).Count();
            Assert.AreEqual(count + 1, found, "numero righe file");


        }

        /// <summary>
        /// test per la creazione di un log in modalità append
        /// </summary>
        [Test]
        public void CreateCsvLogFileAppend()
        {

            //creazione di dati fake
            int count = 20;
            var data = GetFakeTestUnitData();
            var file = string.Format(@"test{0}.csv", "FileAppend");
            var file2 = string.Format(@"{0}\{1}", Log.CCsvLog<Bubbles.TestUnit>.DEFAULT_FOLDER, file);

            //conteggio precedenti dati
            int prev = 0;
            if (System.IO.File.Exists(file2)) { prev = System.IO.File.ReadLines(file2).Count(); }

            //generazione log
            try
            {

                //creazione del log
                using (var L = new Log.CCsvLog<Bubbles.TestUnit>(file, false, Log.AFileLog<TestUnit>.FileAccess.Append) { BufferSize = 3 })
                {
                    for (var i = 0; i < count; i++) L.Add(data);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("creazione fallita, error: " + ex.Message, ex);
            }

            //lettura del file per controllo
            // il test è ben eseguito se:
            //  - il file esiste
            //  - il file contiene [count + prev] righe
            int found = System.IO.File.ReadLines(file2).Count();
            Assert.AreEqual(count + prev, found, "numero righe file");


        }

        /// <summary>
        /// test per la creazione di un log in modalità overwrite
        /// </summary>
        [Test]
        public void CreateCsvLogFileOverwrite()
        {

            //creazione di dati fake
            int count = 20;
            var data = GetFakeTestUnitData();
            var file = string.Format(@"test{0}.csv", "FileOverwrite");
            var file2 = string.Format(@"{0}\{1}", Log.CCsvLog<Bubbles.TestUnit>.DEFAULT_FOLDER, file);


            //generazione log
            try
            {

                //creazione del log
                using (var L = new Log.CCsvLog<Bubbles.TestUnit>(file, false, Log.AFileLog<TestUnit>.FileAccess.Overwrite) { BufferSize = 3 })
                {
                    for (var i = 0; i < count; i++) L.Add(data);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("creazione fallita, error: " + ex.Message, ex);
            }

            //lettura del file per controllo
            // il test è ben eseguito se:
            //  - il file esiste
            //  - il file contiene [count] righe
            int found = System.IO.File.ReadLines(file2).Count();
            Assert.AreEqual(count , found, "numero righe file");


        }

        /// <summary>
        /// test per la creazione di un log in modalità ignore
        /// </summary>
        [Test]
        public void CreateCsvLogFileIgnore()
        {

            //creazione di dati fake
            int count = 20;
            var data = GetFakeTestUnitData();
            var file = string.Format(@"test{0}.csv", "FileIgnore");
            var file2 = string.Format(@"{0}\{1}", Log.CCsvLog<Bubbles.TestUnit>.DEFAULT_FOLDER, file);


            //conteggio precedenti dati
            DateTime? lastEdit =null;
            if (System.IO.File.Exists(file2)) {
                var info = new System.IO.FileInfo(file2);
                lastEdit = info.LastWriteTime;
            }

            //generazione log
            try
            {

                //creazione del log
                using (var L = new Log.CCsvLog<Bubbles.TestUnit>(file, false, Log.AFileLog<TestUnit>.FileAccess.Ignore) { BufferSize = 3 })
                {
                    for (var i = 0; i < count; i++) L.Add(data);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("creazione fallita, error: " + ex.Message, ex);
            }

            //lettura del file per controllo
            // il test è ben eseguito se:
            //  - il file esiste
            //  - il file non è stato modificato dopo la data lastEdit
            if (System.IO.File.Exists(file2))
            {
                var infoAfter = new System.IO.FileInfo(file2);
                if (lastEdit.HasValue)
                {
                    Assert.AreEqual(lastEdit, infoAfter.LastWriteTime, "file modificato successivamente (potrebbe essere altro processo?)");
                }
            }
            else {
                Assert.Fail("file non creato");
            }
            


        }

    }
}
