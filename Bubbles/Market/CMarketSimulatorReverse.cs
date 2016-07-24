using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubbles.Market
{
    class CMarketSimulatorReverse :CMarketSimulator
    {

        protected override ISimulatedDataSource GetDataSource()
        {
            ISimulatedDataSource _simulatedDataSource;
            if (this.UseFile)
            {
                var dataSource = new SimulateddataSourceTextFileReverse
                {
                    FileName = this.FileName
                };
                _simulatedDataSource = dataSource;
            }
            else
            {
                //TODO: IMPLEMENTARE IL REVERSE SULLE QUERY SQL

                var dataSource = new SimulateddataSourceSqLite 
                {
                    DeltaTime = this.DeltaTime,
                    StartTime = this.StartTime,
                    EndTime = this.EndTime,
                    Currency = this.Currency
                };
                _simulatedDataSource = dataSource;
            }
            return _simulatedDataSource;
        }

    }
}
