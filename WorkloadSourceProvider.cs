using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.Sky;

namespace WorkloadService.ActivityCalculate
{
    public class WorkloadSourceProvider : CalculateSourceProvider
    {
        private WorkloadDataSource _source;
        private SqlServerTransportHelicopter sqlserverhe;
        private WorkloadDataSourceContext _cotext;

        public WorkloadSourceProvider(ICalculateSource source, ITransport transport, IContext context)
        {
            _source = source as WorkloadDataSource;
            sqlserverhe = transport as SqlServerTransportHelicopter;
            _cotext = context as WorkloadDataSourceContext;
            if (_source == null)
            {
                throw new InvalidOperationException();
            }
            if (sqlserverhe == null)
            {
                throw new InvalidOperationException();
            }
            if (_cotext == null)
            {
                throw new InvalidOperationException();
            }
        }
        public override void LoadData()
        {
            _source.studyWorkHours = sqlserverhe.GetStudyWorkhours(_cotext.Study.Id);
        }
    }
}
