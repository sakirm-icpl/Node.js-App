//using Assessment.API.APIModel;
//using Assessment.API.Models;
using AutoMapper;
//using MyCourse.API.APIModel.ActivitiesManagement;
//using MyCourse.API.APIModel.AdministrativeFunctions;
using MyCourse.API.APIModel.Competency;
//using MyCourse.API.APIModel.ILT;
//using MyCourse.API.APIModel.TrainersFunctions;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourse.API.Model.ActivitiesManagement;
//using MyCourse.API.Model.AdministrativeFunctions;
//using MyCourse.API.Model.Competency;
//using MyCourse.API.Model.ILT;
//using MyCourse.API.Model.TrainersFunctions;
//using Feedback.API.APIModel;
//using Feedback.API.Model;
using MyCourse.API.APIModel;
//using MyCourse.API.APIModel.NodalManagement;

namespace MyCourse.API.APIModel
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<Category, APICourseCategory>()
                .ReverseMap();
            //CreateMap<ModuleLevelPlanning, APIModuleLevelPlanning>()
            //    .ReverseMap();
            //CreateMap<ModuleLevelPlanningDetail, APIModuleLevelPlanningDetail>()
            //    .ReverseMap();
            //CreateMap<BatchesFormation, APIBatchesFormation>()
            //   .ReverseMap();
            //CreateMap<BatchesFormationDetail, APIBatchesFormationDetail>()
            //   .ReverseMap();
            //CreateMap<TrainingExpenses, APITrainingExpenses>()
            //  .ReverseMap();
            //CreateMap<TrainingExpensesDetail, APITrainingExpensesDetail>()
            //   .ReverseMap();
            //CreateMap<Assignments, APIAssignments>()
            //   .ReverseMap();
            //CreateMap<AssignmentsDetail, APIAssignmentsDetail>()
            //  .ReverseMap();
            //CreateMap<TargetSetting, APITargetSetting>()
            //.ReverseMap();
            //CreateMap<TrainingAttendance, APITrainingAttendance>()
            //  .ReverseMap();
            //CreateMap<TrainingAttendanceDetail, APITrainingAttendanceDetail>()
            // .ReverseMap();
            //CreateMap<OfflineAssessmentScores, APIOfflineAssessmentScores>()
            // .ReverseMap();
            //CreateMap<OfflineAssessmentScoresDetail, APIOfflineAssessmentScoresDetail>()
            // .ReverseMap();
            CreateMap<CompetencyLevels, APICompetencyLevels>()
                .ReverseMap();
            CreateMap<CompetenciesMaster, APICompetenciesMaster>()
                .ReverseMap();
          /*  CreateMap<CompetenciesMapping, APICompetenciesMapping>()
                .ReverseMap();
            CreateMap<RolewiseCompetenciesMapping, APIRolewiseCompetenciesMapping>()
               .ReverseMap();
            CreateMap<AnswerSheetsEvaluation, APIAnswerSheetsEvaluation>();*/
            CreateMap<SubCategory, APICourseSubCategory>()
                .ReverseMap();
            //CreateMap<SubSubCategory, APICourseSubSubCategory>()
            //    .ReverseMap();
            //CreateMap<ILTTrainingAttendance, APIILTTrainingAttendance>().ReverseMap();
            //CreateMap<ILTRequestResponse, APIILTRequestResponse>().ReverseMap();

            CreateMap<AuthoringMaster, ApiAuthoringMaster>().ReverseMap();
            //CreateMap<AuthoringMasterDetails, ApiAuthoringMasterDetails>().ReverseMap();

            //CreateMap<CourseWiseEmailReminder, APICourseWiseEmailReminder>().ReverseMap();
            //CreateMap<UserWiseCourseEmailReminder, APICourseWiseEmailReminderNew>().ReverseMap();
            //CreateMap<AssessmentAttemptManagement, ApiAssessmentAttemptManagement>().ReverseMap();
            CreateMap<Model.Course, APICourse>()
                 .ForMember(d => d.ModuleAssociation, map => map.Ignore())
                .ReverseMap();
            CreateMap<CompetencyJobRole, APICompetencyJobRole>().ReverseMap();
           
            //CreateMap<ScormVars, APIScorm>()
            //   .ReverseMap();

            //CreateMap<LCMS, LCMSAPI>()
            //.ForMember(d => d.Duration, map => map.Ignore())
            //.ForMember(d => d.IsBuiltInAssesment, map => map.Ignore())
            //.ForMember(d => d.IsMobileCompatible, map => map.Ignore())
            //.ReverseMap();

            CreateMap<APIModuleCompletionStatus, ModuleCompletionStatus>()
           .ReverseMap();

            //CreateMap<APIModule, Module>()
            //.ReverseMap();
            //CreateMap<TrainerFeedback, TrainerFeedbackAPI>()
            //    .ReverseMap()
            //    .ForSourceMember(d => d.Options, map => map.Ignore())
            //    .ForMember(d => d.Option1, map => map.Ignore())
            //    .ForMember(d => d.Option2, map => map.Ignore())
            //    .ForMember(d => d.Option3, map => map.Ignore())
            //    .ForMember(d => d.Option4, map => map.Ignore())
            //    .ForMember(d => d.Option5, map => map.Ignore());

            CreateMap<APIContentCompletionStatus, ContentCompletionStatus>()
          .ReverseMap();

            //CreateMap<APIAssessmentQuestion, AssessmentQuestion>()
            //    .ForSourceMember(Q => Q.aPIassessmentOptions, map => map.Ignore())
            //    .ReverseMap();

            //CreateMap<APIILTSchedular, ILTSchedule>()
            //.ReverseMap();
            //CreateMap<ApiSection, Section>()
            //    .ReverseMap();
            //CreateMap<Faq, ApiFaq>()
            //  .ReverseMap();
            //CreateMap<ToDoPriorityList, ApiToDoPriorityList>()
            //    .ReverseMap();
            //CreateMap<AssignmentDetails, ApiAssignmentDetails>()
            //    .ReverseMap();
            //CreateMap<Module, APIModuleInput>()
            //   .ReverseMap();
            //CreateMap<Model.Course, APIxternalCourse>()
            //.ReverseMap();
            //CreateMap<ILTBatch, APIILTBatch>()
            //.ReverseMap();
            //CreateMap<APINominationResponse, APINominationUserResponse>()
            //    .ForMember(dest => dest.UserId, mapoption => mapoption.MapFrom(src => Security.Decrypt(src.UserIdEncrypted)))
            //.ReverseMap();
            //CreateMap<APIILTAttendanceResponse, APINominationUserResponse>()
            //    .ForMember(dest => dest.UserId, mapoption => mapoption.MapFrom(src => Security.Decrypt(src.UserId)))
            //    .ForMember(dest => dest.Status, mapoption => mapoption.MapFrom(src => src.RecordStatus))
            //.ReverseMap();
            //CreateMap<Model.ProcessResult, APIPostProcessEvaluationDisplay>()
            //    .ForMember(d => d.aPIQuestionDetails, map => map.Ignore())
            //   .ReverseMap();
            //CreateMap<Model.KitchenAuditResult, APIPostProcessEvaluationDisplay>()
            //   .ForMember(d => d.aPIQuestionDetails, map => map.Ignore())
            //   .ReverseMap();
            //CreateMap<APIILTTrainingAttendance, APIILTTrainingAttendanceUsers>()
            //    .ForMember(dest=>dest.EUserID,mapoption=>mapoption.MapFrom(src=> Security.Encrypt(Security.DecryptForUI(src.EUserID))))
            //.ReverseMap();
            //CreateMap<APITrainingNominationImportResult,TrainingNominationRejected>()
            //.ForMember(dest=>dest.ErrMessage,mapoption=>mapoption.MapFrom(src=>src.ErrorMessage))
            //.ReverseMap();
          //  CreateMap<TrainingNominationRejected, APITrainingNominationRejected > ()
          //      .ForMember(dest => dest.UserId, mapoption => mapoption.MapFrom(src => Security.Decrypt(src.UserId)))
          //  .ReverseMap();
          //  CreateMap<ILTBatchRejected, APIILTBatchRejected>()
          //  .ReverseMap();
          //  CreateMap<ILTScheduleRejected, APIILTScheduleRejected>()
          //  .ReverseMap();
          //  CreateMap<Model.OERProcessResult, APIPostOERProcessEvaluationDisplay>()
          //     .ForMember(d => d.aPIQuestionDetails, map => map.Ignore())
          //    .ReverseMap();
          //  CreateMap<CriticalAuditProcessResult, APIPostProcessEvaluationDisplay > ()
          //  .ReverseMap();
          //  CreateMap<NightAuditProcessResult, APIPostProcessEvaluationDisplay>()
          //  .ReverseMap();
          //  CreateMap<OpsAuditProcessResult, APIPostProcessEvaluationDisplay>()
          //  .ReverseMap();
          //  CreateMap<CourseVendorDetail, APICourseVendor>()
          //.ReverseMap();
            CreateMap<DarwinboxConfiguration, APIDarwinboxConfiguration>()
         .ReverseMap();

      //      CreateMap<TeamsScheduleDetailsV2, TeamsScheduleDetails>()
      //  .ReverseMap();
      //      CreateMap<ZoomMeetingDetailsV2, ZoomMeetingDetails>()
      //  .ReverseMap();
      //      CreateMap<InvoicePaymentResponse, PaymentResponceJson>()
      //.ReverseMap();
      //      CreateMap<GoogleMeetDetailsV2, GoogleMeetDetails>()
      // .ReverseMap();
        }


    }
}
