using System;
using WorkloadService.CalculateEngine;

namespace WorkloadService.ActivityCalculate
{
    public class StudyLogSourceCalculator : ISourceCalculator
    {
        private StduyLogDataSource _source;
        private StudyLogSourceContext _cotext;
        public StudyLogSourceCalculator(ICalculateSource source, IContext context)
        {
            _source = source as StduyLogDataSource;
            _cotext = context as StudyLogSourceContext;
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
            _source.Coefficient = (float)_cotext.Activity?.Coefficient;
            float hour = 0;
            CalcSimulation<StduyLogDataSource> si = new CalcSimulation<StduyLogDataSource>(_source);

            try
            {
                hour = Convert.ToSingle(si.GetValue(_cotext.Activity.FunctionString, _cotext.Activity.FormulaString));
                hour = (float)Math.Round(hour * _cotext.Multiple, 2);
            }
            catch (CustomException cusex)
            {

            }

            return hour;
        }
    }
}
