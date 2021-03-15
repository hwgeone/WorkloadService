using System;
using System.Collections.Generic;
using System.Linq;
using WorkloadService.CalculateEngine;
using WorkloadService.FlatModel;
using WorkloadService.Jobs;
using WorkloadService.Sky;
using WorkloadService.Struct;

namespace WorkloadService.ActivityCalculate
{
    public class Factor
    {
        public ActivityStandard Activity { get; set; }
        public StudyMainReport Study { get; set; }
        public double Miltiple { get; set; }
    }

    public class ActivityActualValueCalculator
    {
        private CalculateBuilder _builder;
        private Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();
        private Factor _factor;
        public ActivityActualValueCalculator()
        {
            _builder = new CalculateBuilder();
        }

        public void AddTransport(string key, ITransport transport)
        {
            _transports.Add(key, transport);
        }

        public void SetFactor(Factor factor)
        {
            _factor = factor;
        }

        public float Calcute(bool isbuild)
        {
            float hour = 0;

            if (_factor.Activity.OperatorType == (int)ActivityOperateType.Laber)
            {
                _transports.TryGetValue("workload", out ITransport wtransport);
                if (wtransport == null)
                    throw new NullReferenceException();
                _builder.Add(new Option { key = "workload",source = new WorkloadDataSource() ,
                    cotext = new WorkloadDataSourceContext
                {
                    Study = _factor.Study,
                    Activity = _factor.Activity,
                },
                    transport = wtransport
                });

                _transports.TryGetValue("studylog", out ITransport stransport);
                if (stransport == null)
                    throw new NullReferenceException();
                StduyLogDataSource studylogsource = new StduyLogDataSource();

                _builder.Add(new Option
                {
                    key = "studylog",
                    source = studylogsource,
                    cotext = new StudyLogSourceContext
                    {
                        Study = _factor.Study,
                        Activity = _factor.Activity,
                        Multiple = _factor.Miltiple
                    },
                    transport = stransport
                });

                if (isbuild)
                {
                    _builder.Build();
                }
                if (string.IsNullOrEmpty(_factor.Activity.FormulaString) && string.IsNullOrEmpty(_factor.Activity.FunctionString))
                {
                    hour = _builder.Calculate<WorkloadDataSource>();
                }
                else
                {
                    hour = _builder.Calculate<StduyLogDataSource>();
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return hour;
        }
    }
}
