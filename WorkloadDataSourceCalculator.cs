using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;

namespace WorkloadService.ActivityCalculate
{

    public class WorkloadDataSourceCalculator : ISourceCalculator
    {
        private WorkloadDataSource _source;
        private WorkloadDataSourceContext _cotext;
        public WorkloadDataSourceCalculator(ICalculateSource source, IContext context)
        {
            _source = source as WorkloadDataSource;
            _cotext = context as WorkloadDataSourceContext;
            if (_source == null)
            {
                throw new InvalidOperationException();
            }
            if (_cotext == null)
            {
                throw new InvalidOperationException();
            }
        }

        public float Calculate()
        {
            float hour = 0;
            ProjectWorkHourStatistic workhour = _source.studyWorkHours
                .FirstOrDefault(s => s.ActivityId == _cotext.Activity.Id);
            //重新计算实际工时            
            if (workhour != null)
            {
                if (_cotext.Activity.IsTextWork)
                {
                    hour = (float)Math.Round(workhour.EValue1 * _cotext.Activity.Coefficient, 2);
                }
                else
                {
                    hour = (float)Math.Round(workhour.EValue1 * workhour.EValue2 * _cotext.Activity.Coefficient, 2);
                }
            }
            return hour;
        }
    }
}
