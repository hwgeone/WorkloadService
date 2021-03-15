using Infrastructure.ConvertorHelper;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkloadService.FlatModel;
using WorkloadService.Sky;
using WorkloadService.Struct;
using WorkloadService.Utils;

namespace WorkloadService.ActivityCalculate
{
    public class StudyLogSourceProvider : CalculateSourceProvider
    {
        private StduyLogDataSource _source;
        private StudyLogTransportHelicopter studyloghe;
        private StudyLogSourceContext _cotext;

        public StudyLogSourceProvider(ICalculateSource source, ITransport transport, IContext context)
        {
            _source = source as StduyLogDataSource;
            studyloghe = transport as StudyLogTransportHelicopter;
            _cotext = context as StudyLogSourceContext;
            if (_source == null)
            {
                throw new InvalidOperationException();
            }
            if (studyloghe == null)
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
            BuildSLDataSource();
        }

        private int GetDaysFromToNow(DateTime? groupDate,DateTime? endDate)
        {
            bool hasGroup = false, hasEnd = false;
            DateTime minDate = DateTimeFactory.GetMinDate();

            if (groupDate != null && groupDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasGroup = true;
            }
            if (endDate != null && endDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasEnd = true;
            }

            if (!hasGroup)
            {
                return 0;
            }

            int diff = 0;

            if (!hasEnd)
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }
            else if ((DateTime)endDate >= DateTime.Now)
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }
            else if ((DateTime)endDate >= groupDate)
            {
                diff = ((DateTime)endDate - (DateTime)groupDate).Days;
            }
            else
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }

            if (diff <= 0)
                return 0;
            return diff;
        }

        private int GetWeeksFromToNow(DateTime? groupDate, DateTime? endDate)
        {
            bool hasGroup = false, hasEnd = false;
            DateTime minDate = DateTimeFactory.GetMinDate();

            if (groupDate != null && groupDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasGroup = true;
            }
            if (endDate != null && endDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasEnd = true;
            }

            if (!hasGroup)
            {
                return 0;
            }

            int diff = 0;

            if (!hasEnd)
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }
            else if ((DateTime)endDate >= DateTime.Now)
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }
            else if ((DateTime)endDate >= groupDate)
            {
                diff = ((DateTime)endDate - (DateTime)groupDate).Days;
            }
            else
            {
                diff = (DateTime.Now - (DateTime)groupDate).Days;
            }

            if (diff <= 0)
                return 0;
            return (int)(diff / 7);
        }

        private void BuildSLDataSource()
        {
            try
            {
                List<StudyAnimal> animals = studyloghe.GetAnimals(_cotext.Study.IdFromStudyLog);
                _source.GroupedAnimalNumber = animals.Where(a => !a.IsUnassigned).Count();
                _source.TotalAnimalNumber = animals.Count();
                _source.TotalNonHairlessAnimalNumber = animals.Where(a => !a.IsUnassigned && a.Strain?.Trim() != "BALB/c nude" && a.Strain?.Trim() != "Nu/nu").Count();
                _source.TotalNonHairlessAnimalNumberUnGroup = animals.Where(a => a.Strain?.Trim() != "BALB/c nude" && a.Strain?.Trim() != "Nu/nu").Count();

                DateTime? startDate = _cotext.Study.StartDate;
                DateTime? groupDate = _cotext.Study.GroupingDate;
                DateTime? endDate = _cotext.Study.EndDate;

                bool hasStart = false, hasGroup = false, hasEnd = false;
                DateTime minDate = DateTimeFactory.GetMinDate();
                if (startDate != null && startDate.ToDateTime().CompareTo(minDate) > 0)
                {
                    hasStart = true;
                }
                if (groupDate != null && groupDate.ToDateTime().CompareTo(minDate) > 0)
                {
                    hasGroup = true;
                }
                if (endDate != null && endDate.ToDateTime().CompareTo(minDate) > 0)
                {
                    hasEnd = true;
                }

                _source.DaysFromToNow = GetDaysFromToNow(groupDate,endDate);
                _source.WeeksFromToNow = GetWeeksFromToNow(groupDate,endDate);

                if (hasStart)
                {
                    if (!hasGroup)
                    {
                        if (hasEnd)
                        {
                            _source.TVGroupBefore = studyloghe.GetTV(_cotext.Study.IdFromStudyLog, DateTime.MinValue, (DateTime)endDate);
                            _source.BWGroupBefore = studyloghe.GetBW(_cotext.Study.IdFromStudyLog, DateTime.MinValue, (DateTime)endDate);
                            _source.TVGroupAfter = 0;
                            _source.BWGroupAfter = 0;
                        }
                        else
                        {
                            _source.TVGroupBefore = studyloghe.GetTV(_cotext.Study.IdFromStudyLog, DateTime.MinValue, DateTime.Now);
                            _source.BWGroupBefore = studyloghe.GetBW(_cotext.Study.IdFromStudyLog, DateTime.MinValue, DateTime.Now);
                            _source.TVGroupAfter = 0;
                            _source.BWGroupAfter = 0;
                        }
                    }
                    else
                    {
                        _source.BWGroupAfter = studyloghe.GetBW(_cotext.Study.IdFromStudyLog, (DateTime)_cotext.Study.GroupingDate, DateTime.Now);
                        _source.TVGroupAfter = studyloghe.GetTV(_cotext.Study.IdFromStudyLog, (DateTime)_cotext.Study.GroupingDate, DateTime.Now);
                        _source.BWGroupBefore = studyloghe.GetBW(_cotext.Study.IdFromStudyLog, DateTime.MinValue, (DateTime)_cotext.Study.GroupingDate);
                        _source.TVGroupBefore = studyloghe.GetTV(_cotext.Study.IdFromStudyLog, DateTime.MinValue, (DateTime)_cotext.Study.GroupingDate);
                        _source.IM = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.m.");
                        _source.IP = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.p.");
                        _source.IT = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.t.");
                        _source.PO = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "p.o.");
                        _source.SC = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "s.c.");
                        _source.IV = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.v.");
                        _source.ID = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.d.");
                        _source.IC = studyloghe.GetDosing(_cotext.Study.IdFromStudyLog, "i.c.");
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(StudyLogSourceProvider)).Error("Error happend:", ex); ;
            }
        }
    }
}
