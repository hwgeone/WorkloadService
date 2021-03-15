using System;
using System.Text;

namespace WorkloadService.CalculateEngine
{
    public class CalcSimulation<T> where T:class
    {
        private CalcEngine _ce;
        public CalcSimulation(T data)
        {
            if (data == null)
                throw new NullReferenceException("data");
            _ce = new CalcEngine();
            _ce.DataContext = data;
        }

        public object GetValue(string functionString,string formulaString)
        {
            if (string.IsNullOrEmpty(functionString) && string.IsNullOrEmpty(formulaString))
                throw new ArgumentException();

            var outerData = _ce.Evaluate(functionString);
            if(string.IsNullOrEmpty(formulaString))
            {
                return outerData;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(formulaString,outerData);
            return _ce.Evaluate(sb.ToString());
        }
    }
}
