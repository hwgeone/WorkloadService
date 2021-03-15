using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;

namespace WorkloadService.ActivityCalculate
{
    public class StduyLogDataSource : ICalculateSource
    {
        /// <summary>
        /// 分组鼠数
        /// </summary>
        public int GroupedAnimalNumber { get; set; }
        /// <summary>
        /// 鼠总数
        /// </summary>
        public int TotalAnimalNumber { get; set; }
        /// <summary>
        /// 有毛鼠总数
        /// </summary>
        public int TotalNonHairlessAnimalNumber { get; set; }

        public int TotalNonHairlessAnimalNumberUnGroup { get; set; }

        public int DaysFromToNow { get;set; }

        public int WeeksFromToNow { get; set; }

      //  public List<DateTime?> Dates { get; set; }
        public int BWGroupBefore { get; set; }
        public int BWGroupAfter { get; set; }

        public int TVGroupBefore { get; set; }
        public int TVGroupAfter { get; set; }

        public int PO { get; set; }
        public int IP { get; set; }
        public int SC { get; set; }
        public int IM { get; set; }
        public int IV { get; set; }
        public int IT { get; set; }
        public int ID { get; set; }
        public int IC { get; set; }
        public List<ActivityStandard> Activities = new List<ActivityStandard>();
        public float Coefficient { get; set; }
        public StduyLogDataSource()
        {
        }

        public ICalculatorSourceProvider Build(ICalculateBuilder builder)
        {
            Option option = builder.Options.FirstOrDefault(o => o.key == "studylog");
            return new StudyLogSourceProvider(option.source, option.transport, option.cotext);
        }

        public ISourceCalculator Delegate(ICalculateBuilder builder)
        {
            Option option = builder.Options.FirstOrDefault(o => o.key == "studylog");
            return new StudyLogSourceCalculator(option.source, option.cotext);
        }
    }
}
