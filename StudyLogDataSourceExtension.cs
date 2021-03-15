using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;

namespace WorkloadService.ActivityCalculate
{
    public static class StudyLogDataSourceExtension
    {
        public static void SetActivities(this StduyLogDataSource source, List<ActivityStandard> activities)
        {
            if (activities == null || activities.Count == 0)
                throw new ArgumentNullException(nameof(activities));

            if(source == null)
                throw new ArgumentNullException(nameof(source));

            source.Activities = activities;
        }
    }
}
