using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;

namespace WorkloadService.ActivityCalculate
{
    public interface IContext
    {
    }

    public class StudyLogSourceContext : IContext
    {
        public StudyMainReport Study { get; set; }
        public ActivityStandard Activity { get; set; }

        public double Multiple { get; set; }
    }

    public class WorkloadDataSourceContext : IContext
    {
        public StudyMainReport Study { get; set; }
        public ActivityStandard Activity { get; set; }
    }
}
