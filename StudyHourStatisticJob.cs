using Infrastructure.ConvertorHelper;
using log4net;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkloadService.ActivityCalculate;
using WorkloadService.FlatModel;
using WorkloadService.Sky;
using WorkloadService.Struct;
using WorkloadService.Utils;

namespace WorkloadService.Jobs
{
    /// <summary>
    /// 工时同步思路：每天凌晨晚点都会同步 在第一次同步完成之后记录这次同步时间
    /// 项目初始会有预估时间，有些预估时间会作为实际时间参与工时的计算
    /// Step1 找到所有项目 筛选条件：项目未结束、未删除并且已分配房间
    /// Step2 遍历所有的项目 追加项目的工时（根据上次同步时间到当前时间之内项目在Studylog中所有的操作记录）
    /// Step3 获取当前项目的当前实际总工时，获取当前项目所分配的小组，将实际总工时平均分配到小组下的所有成员
    /// 例如 总工时1000，小组A下有10人，那么每人平均分配到1000/10个工时
    /// 追加小组成员的工时
    /// 
    /// 问题1：是否要判断小组人员是否离职
    /// 问题2：如果有个项目一直同步不到IdFromStudyLog ，是否根据同步次数，超过7次就不再同步
    /// 问题3：repeat项目的问题解决方案
    /// 问题4：一个Study的工时计算需要考虑老鼠类型 rat = mice * 3,但是房间还没分配
    /// 问题5：最大的问题是Activity会变动，以及Activity的计算方式也会变动，这对同步会造成影响
    /// ---造成问题的5的主要原因是实际数据依赖于Studylog，以及没有自定义计算方式引擎。
    /// ---自定义计算方式引擎：例如 Activity 文本工作 贴本计算公式 => 1.5h+分组后的总天数*工时
    /// 对数据源加工（studylog）  
    /// 设计计算公式  A + C*B
    /// 对于计算公式中每个涉及到的参数（例如A），都需要告知如何得到。
    /// 1.5h => 常数（单位h）
    /// 分组后的总天数 => 来自StudyLog 当前时间-分组时间]
    /// 问题6 次数和鼠数是否需要记录
    /// 问题7 小组人员变动导致工时计算不准确    解决方案 step2 
    /// 但是例如 part2 剃毛 => 1.6/mouse,分组后一星期按照一次剃毛的比例进行（分组后小鼠数）。
    /// 解决思路 建立同步记录表，记录上次study所有的实际工时，当前同步工时减去上次同步工时得到差值，
    /// 差值平均到项目当前每个成员下面
    /// </summary>
    public class StudyHourStatisticJob : IJob
    {
        private readonly ILog mylogrecoder = LogManager.GetLogger(typeof(StudyHourStatisticJob));

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



        //E3468-T1904
        private Task Sync()
        {
            SqlServerTransportHelicopter sqlhe = new SqlServerTransportHelicopter();
            StudyLogTransportHelicopter studyloghe = new StudyLogTransportHelicopter();
            try
            {
                List<WorkHourSyncLog> logs = new List<WorkHourSyncLog>();
                List<StudyMainReport> studys = sqlhe.GetStudysWithindays(100).Where(study => !study.IdFromStudyLog.IsEmpty()).ToList();
                List<ProjectWorkHourStatistic> studyHours = new List<ProjectWorkHourStatistic>();
                List<SampleCollection> sampleCollections = sqlhe.GetSampleCollections();
                List<ActivityStandard> activitys = sqlhe.GetActivitys().Where(a => a.OperatorType == (int)ActivityOperateType.Laber).ToList();

                foreach (var study in studys)
                {
                    ActivityActualValueCalculator calculator = new ActivityActualValueCalculator();
                    calculator.AddTransport("workload", sqlhe);
                    calculator.AddTransport("studylog", studyloghe);

                    //获取Study老鼠房间的动物
                    double multiple = sqlhe.GetMultiple(study.Room);

               
                    float summaryTime = 0;
                    bool isbuild = true;
                    //合计study的所有工时
                    foreach (var activity in activitys.Where(a=>a.CompanySite == study.Site))
                    {
                        calculator.SetFactor(new Factor {Activity = activity,Study = study,Miltiple = multiple });
           
                        float hour = 0;
                        hour = calculator.Calcute(isbuild);
                        summaryTime += hour;
                        ProjectWorkHourStatistic studyhour = new ProjectWorkHourStatistic();
                        studyhour.StudyId = study.Id;
                        studyhour.ActivityId = activity.Id;
                        studyhour.ActualValue = hour;
                        studyhour.GetTime = DateTime.Now;
                        studyHours.Add(studyhour);

                        isbuild = false;
                    }

                    //重新计算study的总工时
                    //获取Sample Collection相关的工时
                    float sampleCollectionHour = (float)Math.Round(sampleCollections.Where(sc => sc.StudyId == study.Id).Sum(r => r.WorkHour), 2);
                    summaryTime += sampleCollectionHour;
                    study.SummaryTime = summaryTime;

                    logs.Add(new WorkHourSyncLog { StudyId = study.Id, StudyHour = summaryTime, CreateTime = DateTime.Now });
                }

                sqlhe.AddOrUpdateStudyHours(studyHours);
                sqlhe.BatchUpdate(studys);
                sqlhe.BatchAdd(logs);
                sqlhe.Commit();
                sqlhe.Dispose();
                studyloghe.Dispose();
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

        private void StudyHourSync(StudyMainReport study, DateTime lastSyncTime)
        {

        }
    }
}
