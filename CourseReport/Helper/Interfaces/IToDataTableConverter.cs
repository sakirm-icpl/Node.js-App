using CourseReport.API.APIModel;
using System.Collections.Generic;
using System.Data;

namespace CourseReport.API.Helper
{
    public interface IToDataTableConverter
    {
        DataTable ToDataTableOpinionPollReport<APIOpinionPollReport>(IEnumerable<APIOpinionPollReport> items);

        // CourseReport/
        DataTable ToDataTableAssessmentResultSheetReport<APIAssessmentResultSheetReport>(IEnumerable<APIAssessmentResultSheetReport> items,string OrgCode);
        DataTable ToDataTableTrainingProgramReport<APIRecommondedTrainingReport>(IEnumerable<APIRecommondedTrainingReport> items, string OrgCode);
        DataTable ToDataTableUserAssessmentSheetReport<APIUserAssessmentSheet>(IEnumerable<APIUserAssessmentSheet> items, string OrgCode);
        DataTable ToDataTableUserManagerEvaluationReport<APIUserAssessmentSheet>(List<APIManagerEvaluation> items, string OrgCode);
        DataTable ToDataTableTestResultReport<APITestResultReport>(IEnumerable<APITestResultReport> items, string OrgCode);
        DataTable ToDataTableAttendanceSummaryReport<ApiAttendanceSummuryReport>(IEnumerable<ApiAttendanceSummuryReport> items, string OrgCode);
        DataTable ToDataTableCourseCompletionReport<APICourseCompletionReport>(IEnumerable<APICourseCompletionReport> items, string OrgCode,string SHOW_CONFCOLUMNS_INREPORT, List<UserSettings> CourseCompletionHeaderss);
        DataTable ToDataTableUserLoginStatisticReport<APIUserLoginStatistic>(IEnumerable<APIUserLoginStatistic> items);
        DataTable ToDataTableWorkDiaryReport<APIWorkDiary>(IEnumerable<APIWorkDiary> items);
        DataTable ToDataTableReportProfabAccessibility<APIReportProfabAccessibility>(IEnumerable<APIReportProfabAccessibility> items);
        DataTable ToDataTableReportProfabAccessibilityCount<APIReportProfabAccessibilityCount>(IEnumerable<APIReportProfabAccessibilityCount> items);
        DataTable ToDataTableManagerEvaluationReport<APIManagerEvaluationReport>(IEnumerable<APIManagerEvaluationReport> items);

        // SurveyReport/
        DataTable ToDataTableSurveyReport<APISurveyModel>(IEnumerable<APISurveyModel> items, string OrgCode);
        DataTable ToDataTableUserSurveySheetReport<APIUserSurveySheetReport>(IEnumerable<APIUserSurveySheetReport> items, string OrgCode);
        DataTable ToDataTableSurveyApplicabilityReport<APISurveyApplicabilityReport>(IEnumerable<APISurveyApplicabilityReport> items, string OrgCode);

        // UserWiseLoginDetails/
        DataTable ToDataTableUserWiseLoginDetailsReport<APIUserWiseLoginDetails>(IEnumerable<APIUserWiseLoginDetails> items, string OrgCode);
        DataTable ToDataTableUserLoginReport<APIUserLoginReport>(IEnumerable<APIUserLoginReport> items, string OrgCode);
        DataTable ToDataTableUserDatewiseLoginFromDetailsReport<APIUserDatewiseLoginFromDetails>(IEnumerable<APIUserDatewiseLoginFromDetails> items, string OrgCode);
        DataTable ToDataTableUserLogin2Report<APIUserLoginModuleReport>(IEnumerable<APIUserLoginModuleReport> items, string OrgCode);
        DataTable ToDataTableUserTotalTimeSpentReport<APIUserTotalTimeSpent>(IEnumerable<APIUserTotalTimeSpent> items, string OrgCode);
        DataTable ToDataTableUserNotLoginReport<APIUserNotLoginReport>(IEnumerable<APIUserNotLoginReport> items, string OrgCode);
        DataTable ToDataTableUserLoginHistoryReport<APIUserLoginHistoryReport>(IEnumerable<APIUserLoginHistoryReport> items);

        // Dealer/
        DataTable ToDataTableDealerDetailLoginMonitoringReport<APIDealerDetailLoginMonitoringReport>(IEnumerable<APIDealerDetailLoginMonitoringReport> items, string OrgCode);
        DataTable ToDataTableDealerReport<APIDealerReport>(IEnumerable<APIDealerReport> items, string OrgCode);
        DataTable ToDataTableRewardPointsSummeryReport<APIRewardPointsSummeryReport>(IEnumerable<APIRewardPointsSummeryReport> items, string OrgCode);
        DataTable ToDataTableRewardPointsDetailsReport<APIRewardPointsDetailsReport>(IEnumerable<APIRewardPointsDetailsReport> items, string OrgCode);

        //Feedback/
        DataTable ToDataTableUserFeedbackReport<APIUserFeedbackReport>(IEnumerable<APIUserFeedbackReport> items, string OrgCode);
        DataTable ToDataTableFeedbackStatusReport<APIFeedbackStatusReport>(IEnumerable<APIFeedbackStatusReport> items, string OrgCode);
        DataTable ToDataTableFeedbackAggregationReport<APIFeedbackAggregationReport>(IEnumerable<APIFeedbackAggregationReport> items, string OrgCode);

        // CourseReport/
        DataTable ToDataTableCourseRatingReport<APICourseRatingReport>(IEnumerable<APICourseRatingReport> items, string OrgCode);
        DataTable ToDataTableCourseWiseCompletionReport<APICourseWiseCompletionReport>(IEnumerable<APICourseWiseCompletionReport> items, string OrgCode, List<APIUserSetting> userSetting);
        DataTable ToDataTableUserwiseCourseStatusReportResult<APIUserwiseCourseStatusReportResult>(IEnumerable<APIUserwiseCourseStatusReportResult> items);
        DataTable ToDataTableExportTcnsRetrainingReport<ApiExportTcnsRetrainingReport>(IEnumerable<ApiExportTcnsRetrainingReport> items,string orgCode);
        DataTable ToDataTableUserLearningReport<APIUserLearningReport>(IEnumerable<APIUserLearningReport> items,string OrgCode);
        
        // Scheduler/
        DataTable ToDataTableSchedulerReport<APISchedulerReport>(IEnumerable<APISchedulerReport> items, string OrgCode);

        // DataMigrationReport
        DataTable ToDataTableDataMigrationReport<APIDataMigrationReport>(IEnumerable<APIDataMigrationReport> items, string OrgCode);

        // ILTReport/
        DataTable ToDataTableBatchWiseAttendanceDataViewReport<APIBatchWiseAttendanceDataView>(IEnumerable<APIBatchWiseAttendanceDataView> items, string OrgCode);
        DataTable ToDataTableScheduleWiseAttendanceViewReport<APIScheduleWiseAttendanceView>(IEnumerable<APIScheduleWiseAttendanceView> items, string OrgCode);
        DataTable ToDataTableInternalTrainersScheduleReport<APIInternalTrainersScheduleReport>(IEnumerable<APIInternalTrainersScheduleReport> items, string OrgCode);
        DataTable ToDataTableTrainerWiseCourseDetailsReport<APITrainerWiseCourseDetails>(IEnumerable<APITrainerWiseCourseDetails> items, string OrgCode);
        DataTable ToDataTableTopicAttendanceReport<APITopicAttendanceReport>(IEnumerable<APITopicAttendanceReport> items, string OrgCode);
        DataTable ToDataTableTopicFeedbackReport<APITopicFeedbackReport>(IEnumerable<APITopicFeedbackReport> items, string OrgCode);
        DataTable ToDataTableAttendanceDetailsReport<APIAttendanceDetailsReport>(IEnumerable<APIAttendanceDetailsReport> items, string OrgCode);
        DataTable ToDataTableILTTrainingReport<APIILTTrainingReport>(IEnumerable<APIILTTrainingReport> items, string OrgCode);
        DataTable ToDataTableRegistrationReport<APIRegistrationReport>(IEnumerable<APIRegistrationReport> items, string OrgCode);
        DataTable ToDataTablePastTrainingSummeryDetailsReport<APIPastTrainingSummeryDetails>(IEnumerable<APIPastTrainingSummeryDetails> items);
        DataTable ToDataTableProgramAttendanceReport<ApiProgramAttendanceReportView>(IEnumerable<ApiProgramAttendanceReportView> items);
        DataTable ToDataTableTrainerProductivityReport<APITrainerProductivityReport>(IEnumerable<APITrainerProductivityReport> items);
        DataTable ToDataTableAverageProductivityReport<APIAverageProductivityReport>(IEnumerable<APIAverageProductivityReport> items);
        DataTable ToDataTableTrainingPassportReport<APITrainingPassportReport>(IEnumerable<APITrainingPassportReport> items);

        // Statistics/
        DataTable ToDataTableAllCoursesSummaryReport<APIAllCoursesSummaryReport>(IEnumerable<APIAllCoursesSummaryReport> items);
        DataTable ToDataTableAllUserTimeSpentReport<APIAllUserTimeSpentReport>(IEnumerable<APIAllUserTimeSpentReport> items);
        DataTable ToDataTableILTUserDetailsReport<APIILTUserDetailsReport>(IEnumerable<APIILTUserDetailsReport> items);
        DataTable ToDataTableActivityReport<APIActivityReport>(IEnumerable<APIActivityReport> items, List<APIUserSetting> userSetting);
        DataTable ToDataTableUserWiseCourseCompletionReport<APIUserWiseCourseCompletionReport>(IEnumerable<APIUserWiseCourseCompletionReport> items);
        DataTable ToDataTableILTDashboardReport<APIILTDashboardReport>(IEnumerable<APIILTDashboardReport> items);
        DataTable ToDataTableCourseModuleReport<APICourseModuleReport>(IEnumerable<APICourseModuleReport> items);
        DataTable ToDataTableCourseAssessmentReport<APICourseAssessmentReport>(IEnumerable<APICourseAssessmentReport> items);
        
        // SpecificReportTLS/
        DataTable ToDataTableTNASupervisionReport<TnaSupervisionReportExport>(IEnumerable<TnaSupervisionReportExport> items, string OrgCode);
        DataTable ToDataTableReportUserWiseCourseDurationReport<APIReportUserWiseCourseDuration>(IEnumerable<APIReportUserWiseCourseDuration> items);

        DataTable ToDataTableLaLearningReport<APILaLearningReportData>(IEnumerable<APILaLearningReportData> items);
        DataTable ToDataTableLearningAcademyDashboardReport<APILearningAcademyDashboardReportData>(IEnumerable<APILearningAcademyDashboardReportData> items);
        DataTable ToDataTableNPSDashboardReport<APINPSDashboardReportData>(IEnumerable<APINPSDashboardReportData> items);
        DataTable ToDataTableTrainerUtilizationReport<APITrainerUtilizationReportData>(IEnumerable<APITrainerUtilizationReportData> items);

        DataTable ToDataTableDevPlanReport<APIDevPlanCompletionReport>(IEnumerable<APIDevPlanCompletionReport> items);

        DataTable ToDataTableTrainingFeedbackSurveyReport<APITrainingFeedbackSurveyReportData>(IEnumerable<APITrainingFeedbackSurveyReportData> items);
        DataTable ToDataTableLABPLearningJourneyReport<APILABPLearningJourneyReportData>(IEnumerable<APILABPLearningJourneyReportData> items);
        DataTable ToDataTableLABPLearningJourneyReportMonthlyView<APILABPLearningJourneyReportMonthlyViewData>(IEnumerable<APILABPLearningJourneyReportMonthlyViewData> items);
        DataTable ToDataTableLABPLearningJourneyReportRawData<APILABPLearningJourneyReportRawData>(IEnumerable<APILABPLearningJourneyReportRawData> items);
        DataTable ToDataTableLABPCardReport<APILABPCardReportData>(IEnumerable<APILABPCardReportData> items);
        DataTable ToDataTableILTConsolidatedReport<ILTConsolidatedReport>(IEnumerable<ILTConsolidatedReport> items);

    }
}