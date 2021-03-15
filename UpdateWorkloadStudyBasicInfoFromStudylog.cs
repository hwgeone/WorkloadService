using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using WorkloadService.FlatModel;
using WorkloadService.Sky;
using WorkloadService.Struct;
using Infrastructure.ConvertorHelper;
using WorkloadService.Utils;
using log4net;

namespace WorkloadService.Jobs
{
    public class UpdateWorkloadStudyBasicInfoFromStudylog : IJob
    {
        private readonly ILog mylogrecoder = LogManager.GetLogger(typeof(UpdateWorkloadStudyBasicInfoFromStudylog));

        public virtual async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await Sync();
            }
            catch (Exception ex)
            {
                mylogrecoder.Error("Error happend:", ex);
            }
        }

        private Task Sync()
        {
            SqlServerTransportHelicopter sqlhe = new SqlServerTransportHelicopter();
            StudyLogTransportHelicopter studyloghe = new StudyLogTransportHelicopter();

            try
            {
                List<StudyMainReport> studys = sqlhe.GetStudysWithindays(30);
                List<StudyMainReport> copys = new List<StudyMainReport>();
                List<StudySyncLog> studySyncLogs = new List<StudySyncLog>();
                List<StudySyncLog> handledStudySyncLogs = new List<StudySyncLog>();
                foreach (var study in studys)
                {
                    Guid idfromstudylog = studyloghe.GetStudyId(study.StudyNumber);
                    if (!idfromstudylog.IsEmpty())
                    {
                        //判断study sync exception 是否处理
                        handledStudySyncLogs.Add(new StudySyncLog() { StudyId = study.Id});
                        StudyMainReport copy = CopyStudy(idfromstudylog, study, studyloghe);
                        copys.Add(copy);
                    }
                    else
                    {
                        StudySyncLog log = new StudySyncLog(study.Id,study.StudyNumber);
                        studySyncLogs.Add(log);
                    }
                }

                sqlhe.BatchUpdate(copys);
                sqlhe.BatchAdd(studySyncLogs);
                sqlhe.DeleteStudySyncLogs(handledStudySyncLogs);
                sqlhe.Commit();
                studyloghe.Dispose();
                sqlhe.Dispose();
            }
            catch (Exception ex)
            {
                sqlhe.Rollback();
                sqlhe.Dispose();
                studyloghe.Dispose();
                throw ex;
            }
            return Task.CompletedTask;
        }

        private StudyMainReport CopyStudy(Guid idfromstudylog, StudyMainReport study, StudyLogTransportHelicopter studyloghe)
        {
            StudyMainReport copy = new StudyMainReport();
            copy.Id = study.Id;
            copy.IdFromStudyLog = idfromstudylog;
            List<StudyDate> dates = studyloghe.GetDates(copy.IdFromStudyLog);
            copy.StartDate = dates.FirstOrDefault(d => d.DateType == (int)DateType.Start)?.CustomDate;
            copy.GroupingDate = dates.Find(d => d.DateType == (int)DateType.Group)?.CustomDate;
            copy.EndDate = dates.Find(d => d.DateType == (int)DateType.End)?.CustomDate;
            int groupNumber = studyloghe.GetGroupNumber(copy.IdFromStudyLog);
            if (!string.IsNullOrEmpty(study.StudyNumber) && study.StudyNumber.ToLower().StartsWith("e0244"))
            {
                groupNumber = groupNumber * 2;
            }
            copy.GroupNumber = groupNumber;
            copy.GroupingAnimals = studyloghe.GetAnimals(copy.IdFromStudyLog).Where(a => !a.IsUnassigned).Count();
            copy.StudyType = studyloghe.GetStudyType(copy.IdFromStudyLog);
            copy.TotalNumber = studyloghe.GetAnimals(copy.IdFromStudyLog).Count();
            DateTime minDate = DateTimeFactory.GetMinDate();
            int status = 0;
            bool hasStart = false, hasGroup = false, hasEnd = false;
            if (copy.StartDate != null && copy.StartDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasStart = true;
            }
            if (copy.GroupingDate != null && copy.GroupingDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasGroup = true;
            }
            if (copy.EndDate != null && copy.EndDate.ToDateTime().CompareTo(minDate) > 0)
            {
                hasEnd = true;
            }

            if (hasEnd)
            {
                status = 3;
            }
            else if (hasGroup)
            {
                status = 2;
            }
            else if (hasStart)
            {
                status = 1;
            }
            else {
                status = 0;
            }

            copy.Status = status;
            copy.StudyNumber = study.StudyNumber;
            copy.CreateTime = study.CreateTime;
            copy.SD = study.SD;
            copy.JSD = study.JSD;
            copy.GroupId = study.GroupId;
            copy.Room = study.Room;
            copy.AnimalSpecies = study.AnimalSpecies;
            copy.ModelName = study.ModelName;
            copy.SummaryTime = study.SummaryTime;
            copy.EstimateSummaryTime = study.EstimateSummaryTime;
            copy.FloatingValue = study.FloatingValue;
            copy.isRepeat = study.isRepeat;
            copy.Deptname = study.Deptname;
            copy.Site = study.Site;
            copy.SurviveStatus = study.SurviveStatus;
            copy.Operator = study.Operator;
            copy.IsEmailSend = study.IsEmailSend;
            return copy;
        }
    }
}
