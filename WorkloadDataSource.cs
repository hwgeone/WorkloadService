using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;

namespace WorkloadService.ActivityCalculate
{
    public class WorkloadDataSource : ICalculateSource
    {
        public List<ProjectWorkHourStatistic> studyWorkHours = new List<ProjectWorkHourStatistic>();
        public ICalculatorSourceProvider Build(ICalculateBuilder builder)
        {
            Option option = builder.Options.FirstOrDefault(o => o.key == "workload");
            return new WorkloadSourceProvider(option.source, option.transport, option.cotext);
        }

        public ISourceCalculator Delegate(ICalculateBuilder builder)
        {
            Option option = builder.Options.FirstOrDefault(o => o.key == "workload");
            return new WorkloadDataSourceCalculator(option.source, option.cotext);
        }
    }
}
