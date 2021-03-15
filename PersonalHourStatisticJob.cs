using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkloadService.FlatModel;
using WorkloadService.Sky;
using WorkloadService.Utils;

namespace WorkloadService.Jobs
{
    /// <summary>
    /// 个人工时同步
    /// 
    /// </summary>
    public class PersonalHourStatisticJob : IJob
    {
        private readonly ILog mylogrecoder = LogManager.GetLogger(typeof(PersonalHourStatisticJob));
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
            try
            {
                List<StudyMainReport> studys = sqlhe.GetStudysWithindays(100).FindAll(study => !study.GroupId.IsEmpty() && !study.IdFromStudyLog.IsEmpty());
                List<PersonalWorkHourStatistic> personalHours = new List<PersonalWorkHourStatistic>();
                foreach (var study in studys)
                {
                    List<WorkHourSyncLog> logs = sqlhe.GetStudyLastSyncLogs(study.Id,2);
                    if (logs == null || logs.Count == 0)
                    {
                        throw new Exception("Error happend:未找到StudyNumber:" + study.StudyNumber+"的同步记录。");
                    }
                    List<PersonalWorkHourStatistic> studyPersonalWorkHours = sqlhe.GetPersonalWorkHours(study.Id);
                    float firstValue = 0, secondValue = 0;

                    if (studyPersonalWorkHours == null || studyPersonalWorkHours.Count == 0)
                    {
                        firstValue = logs[0].StudyHour;
                    }
                    else
                    {
                        if (logs.Count == 1)
                        {
                            WorkHourSyncLog today = logs[0];
                            firstValue = today.StudyHour;
                        }

                        if (logs.Count == 2)
                        {
                            WorkHourSyncLog today = logs[0];
                            WorkHourSyncLog yestoday = logs[1];

                            firstValue = today.StudyHour;
                            secondValue = yestoday.StudyHour;
                        }
                    }

                    List<WebUser> users = sqlhe.GetGroupMembers((Guid)study.GroupId);

                    if (study.JSD.IsEmpty())
                    {
                        mylogrecoder.WarnFormat("study:{0} 下还未分配JSD！", study.StudyNumber);
                    }

                    if (users.Any())
                    {
                        float workHour = (float)Math.Round((firstValue - secondValue) / users.Count,2);
                        foreach (var user in users)
                        {
                            if (workHour > 0)
                            {
                                PersonalWorkHourStatistic personalHour = new PersonalWorkHourStatistic();
                                personalHour.PeopleId = user.Id;
                                personalHour.StudyId = study.Id;
                                personalHour.WorkHour += workHour;
                                personalHour.SettleType = "";
                                personalHour.GetTime = DateTime.Now;
                                personalHours.Add(personalHour);
                            }
                        }
                    }
                    else
                    {
                        mylogrecoder.WarnFormat("study:{0} 下的分配的组没有任何成员！", study.StudyNumber);
                    }
                }

                sqlhe.AddOrUpdatePersonalHours(personalHours);
                sqlhe.Commit();
                sqlhe.Dispose();
            }
            catch (Exception ex)
            {
                sqlhe.Rollback();
                sqlhe.Dispose();
                throw ex;
            }
            return Task.CompletedTask;
        }
    }
}
