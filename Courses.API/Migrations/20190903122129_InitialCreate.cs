using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Course");

            migrationBuilder.EnsureSchema(
                name: "Certification");

            migrationBuilder.EnsureSchema(
                name: "Masters");

            migrationBuilder.EnsureSchema(
                name: "User");

            migrationBuilder.CreateTable(
                name: "CertificateDownloadDetails",
                schema: "Certification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateDownloadDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTemplates",
                schema: "Certification",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    TemplateId = table.Column<int>(nullable: false),
                    TemplateDesign = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcademyAgencyMaster",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AcademyAgencyName = table.Column<string>(nullable: true),
                    TrainerType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademyAgencyMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessibilityRule",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(nullable: true),
                    EmailID = table.Column<string>(maxLength: 100, nullable: true),
                    MobileNumber = table.Column<string>(maxLength: 25, nullable: true),
                    ReportsTo = table.Column<string>(maxLength: 100, nullable: true),
                    UserType = table.Column<string>(maxLength: 10, nullable: true),
                    Business = table.Column<int>(nullable: true),
                    Group = table.Column<int>(nullable: true),
                    Area = table.Column<int>(nullable: true),
                    Location = table.Column<int>(nullable: true),
                    ConfigurationColumn1 = table.Column<int>(nullable: true),
                    ConfigurationColumn2 = table.Column<int>(nullable: true),
                    ConfigurationColumn3 = table.Column<int>(nullable: true),
                    ConfigurationColumn4 = table.Column<int>(nullable: true),
                    ConfigurationColumn5 = table.Column<int>(nullable: true),
                    ConfigurationColumn6 = table.Column<int>(nullable: true),
                    ConfigurationColumn7 = table.Column<int>(nullable: true),
                    ConfigurationColumn8 = table.Column<int>(nullable: true),
                    ConfigurationColumn9 = table.Column<int>(nullable: true),
                    ConfigurationColumn10 = table.Column<int>(nullable: true),
                    ConfigurationColumn11 = table.Column<int>(nullable: true),
                    ConfigurationColumn12 = table.Column<int>(nullable: true),
                    RuleAnticipation = table.Column<string>(maxLength: 100, nullable: true),
                    TargetPeriod = table.Column<int>(nullable: true),
                    IsCourseFee = table.Column<bool>(nullable: false),
                    ConditionForRules = table.Column<string>(maxLength: 10, nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    GroupTemplateId = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessibilityRule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessibilityRuleRejected",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseCode = table.Column<string>(nullable: true),
                    UserName = table.Column<string>(maxLength: 10, nullable: true),
                    ErrorMessage = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessibilityRuleRejected", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalLearning",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    CourseTitle = table.Column<string>(maxLength: 100, nullable: false),
                    ContentId = table.Column<string>(maxLength: 100, nullable: true),
                    Title = table.Column<string>(maxLength: 100, nullable: false),
                    FileForUpload = table.Column<string>(maxLength: 1000, nullable: false),
                    Foreword = table.Column<string>(maxLength: 100, nullable: true),
                    Author = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalLearning", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencyMaster",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AgencyName = table.Column<string>(nullable: true),
                    AgencyTrainerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnswerSheetsEvaluation",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AnswerSheetId = table.Column<int>(nullable: false),
                    QuestionId = table.Column<int>(nullable: false),
                    Marks = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerSheetsEvaluation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicabilityGroupTemplate",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicabilityGroupName = table.Column<string>(maxLength: 50, nullable: true),
                    ApplicabilityGroupDescription = table.Column<string>(maxLength: 150, nullable: true),
                    UserID = table.Column<int>(nullable: true),
                    EmailID = table.Column<string>(maxLength: 100, nullable: true),
                    MobileNumber = table.Column<string>(maxLength: 25, nullable: true),
                    ReportsTo = table.Column<string>(maxLength: 100, nullable: true),
                    UserType = table.Column<string>(maxLength: 10, nullable: true),
                    Business = table.Column<int>(nullable: true),
                    Group = table.Column<int>(nullable: true),
                    Area = table.Column<int>(nullable: true),
                    Location = table.Column<int>(nullable: true),
                    ConfigurationColumn1 = table.Column<int>(nullable: true),
                    ConfigurationColumn2 = table.Column<int>(nullable: true),
                    ConfigurationColumn3 = table.Column<int>(nullable: true),
                    ConfigurationColumn4 = table.Column<int>(nullable: true),
                    ConfigurationColumn5 = table.Column<int>(nullable: true),
                    ConfigurationColumn6 = table.Column<int>(nullable: true),
                    ConfigurationColumn7 = table.Column<int>(nullable: true),
                    ConfigurationColumn8 = table.Column<int>(nullable: true),
                    ConfigurationColumn9 = table.Column<int>(nullable: true),
                    ConfigurationColumn10 = table.Column<int>(nullable: true),
                    ConfigurationColumn11 = table.Column<int>(nullable: true),
                    ConfigurationColumn12 = table.Column<int>(nullable: true),
                    RuleAnticipation = table.Column<string>(maxLength: 100, nullable: true),
                    TargetPeriod = table.Column<int>(nullable: true),
                    IsCourseFee = table.Column<bool>(nullable: false),
                    ConditionForRules = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicabilityGroupTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAttemptManagement",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    CourseName = table.Column<string>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: false),
                    AdditionalAttempts = table.Column<int>(nullable: false),
                    IsExhausted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttemptManagement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentConfiguration",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Attribute = table.Column<string>(maxLength: 500, nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Value = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestion",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OptionType = table.Column<string>(maxLength: 50, nullable: false),
                    Section = table.Column<string>(maxLength: 50, nullable: true),
                    LearnerInstruction = table.Column<string>(maxLength: 200, nullable: true),
                    QuestionText = table.Column<string>(maxLength: 500, nullable: false),
                    DifficultyLevel = table.Column<string>(maxLength: 50, nullable: true),
                    ModelAnswer = table.Column<string>(maxLength: 500, nullable: true),
                    MediaFile = table.Column<string>(maxLength: 200, nullable: true),
                    AnswerAsImages = table.Column<string>(maxLength: 50, nullable: true),
                    Marks = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    QuestionStyle = table.Column<string>(maxLength: 100, nullable: true),
                    QuestionType = table.Column<string>(maxLength: 50, nullable: true),
                    Metadata = table.Column<string>(maxLength: 200, nullable: true),
                    IsMemoQuestion = table.Column<bool>(nullable: false),
                    ContentType = table.Column<string>(maxLength: 20, nullable: true),
                    ContentPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestionDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssessmentResultID = table.Column<int>(nullable: false),
                    ReferenceQuestionID = table.Column<int>(nullable: false),
                    Marks = table.Column<double>(nullable: true),
                    OptionAnswerId = table.Column<int>(nullable: true),
                    SelectedAnswer = table.Column<string>(maxLength: 500, nullable: true),
                    IsCorrectAnswer = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestionDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestionOption",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuestionID = table.Column<int>(nullable: false),
                    OptionText = table.Column<string>(maxLength: 500, nullable: true),
                    IsCorrectAnswer = table.Column<bool>(nullable: false),
                    UploadImage = table.Column<string>(maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    ContentPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestionOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestionRejected",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Section = table.Column<string>(maxLength: 250, nullable: true),
                    LearnerInstruction = table.Column<string>(maxLength: 200, nullable: true),
                    QuestionText = table.Column<string>(maxLength: 500, nullable: true),
                    DifficultyLevel = table.Column<string>(maxLength: 250, nullable: true),
                    Time = table.Column<string>(maxLength: 250, nullable: true),
                    ModelAnswer = table.Column<string>(maxLength: 500, nullable: true),
                    MediaFile = table.Column<string>(maxLength: 200, nullable: true),
                    AnswerAsImages = table.Column<string>(maxLength: 250, nullable: true),
                    Marks = table.Column<string>(maxLength: 250, nullable: true),
                    Status = table.Column<string>(maxLength: 250, nullable: true),
                    QuestionStyle = table.Column<string>(maxLength: 250, nullable: true),
                    QuestionType = table.Column<string>(maxLength: 250, nullable: true),
                    Metadata = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOptions = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOption1 = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOption2 = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOption3 = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOption4 = table.Column<string>(maxLength: 250, nullable: true),
                    AnswerOption5 = table.Column<string>(maxLength: 250, nullable: true),
                    CorrectAnswer = table.Column<string>(maxLength: 250, nullable: true),
                    ErrorMessage = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestionRejected", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentSheetConfiguration",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PassingPercentage = table.Column<int>(maxLength: 50, nullable: false),
                    MaximumNoOfAttempts = table.Column<int>(nullable: false),
                    Durations = table.Column<int>(maxLength: 50, nullable: false),
                    IsFixed = table.Column<bool>(nullable: true),
                    NoOfQuestionsToShow = table.Column<int>(nullable: true),
                    IsNegativeMarking = table.Column<bool>(nullable: true),
                    NegativeMarkingPercentage = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentSheetConfiguration", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentSheetConfigurationDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuestionID = table.Column<int>(nullable: false),
                    AssessmentSheetConfigID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentSheetConfigurationDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssignmentId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    FilePath = table.Column<string>(nullable: true),
                    FileType = table.Column<string>(nullable: true),
                    TextAnswer = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Remark = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: false),
                    PurposeOfExercise = table.Column<string>(maxLength: 500, nullable: false),
                    DesirableFormOutput = table.Column<string>(maxLength: 500, nullable: false),
                    DateOfSubmission = table.Column<DateTime>(nullable: false),
                    ReferenceDocumentPath = table.Column<string>(maxLength: 2000, nullable: true),
                    AdditionalReferences = table.Column<string>(maxLength: 2000, nullable: true),
                    Status = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OriginalFileName = table.Column<string>(maxLength: 100, nullable: true),
                    InternalName = table.Column<string>(maxLength: 100, nullable: true),
                    MimeType = table.Column<string>(maxLength: 100, nullable: true),
                    AttachmentPath = table.Column<string>(maxLength: 1000, nullable: true),
                    FileContents = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthoringMaster",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Skills = table.Column<string>(maxLength: 500, nullable: false),
                    Description = table.Column<string>(nullable: false),
                    LCMSId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoringMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthoringMasterDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthoringMasterId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    PageNumber = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoringMasterDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchAnnouncement",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BatchCode = table.Column<string>(maxLength: 100, nullable: false),
                    BatchTitle = table.Column<string>(maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    LastRegistrationDate = table.Column<DateTime>(nullable: false),
                    SelectValue = table.Column<string>(maxLength: 50, nullable: false),
                    RegistrationLimit = table.Column<string>(maxLength: 50, nullable: false),
                    EventBadgeFile = table.Column<string>(maxLength: 200, nullable: true),
                    CourseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchAnnouncement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchesFormation",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    BatchId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchesFormation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BespokeParticipants",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BespokeRequestId = table.Column<int>(nullable: false),
                    UserMasterId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BespokeParticipants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BespokeRequest",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserName = table.Column<string>(maxLength: 200, nullable: false),
                    Grade = table.Column<string>(maxLength: 100, nullable: true),
                    UserId = table.Column<string>(maxLength: 1000, nullable: false),
                    Department = table.Column<string>(maxLength: 200, nullable: true),
                    DOJ = table.Column<DateTime>(nullable: false),
                    CostCode = table.Column<string>(nullable: true),
                    TrainingRequestDescription = table.Column<string>(nullable: true),
                    DesiredOutcome = table.Column<string>(nullable: true),
                    Measure = table.Column<string>(nullable: true),
                    TotalNumberofParticipants = table.Column<int>(nullable: false),
                    NeedbyDate = table.Column<DateTime>(nullable: false),
                    TrainingMethod = table.Column<string>(maxLength: 100, nullable: true),
                    Competency = table.Column<string>(maxLength: 100, nullable: true),
                    ManagerialCompetancy = table.Column<string>(maxLength: 100, nullable: true),
                    TrainingName = table.Column<string>(nullable: true),
                    TrainingCosts = table.Column<string>(nullable: true),
                    AttachmentPath = table.Column<string>(nullable: true),
                    TrainingCostCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BespokeRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookCategory",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    ImagePath = table.Column<string>(nullable: true),
                    SequenceNo = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CentralBookLibrary",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<string>(maxLength: 100, nullable: true),
                    BookName = table.Column<string>(maxLength: 100, nullable: false),
                    Author = table.Column<string>(maxLength: 200, nullable: false),
                    Language = table.Column<string>(maxLength: 50, nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    Category = table.Column<string>(maxLength: 200, nullable: true),
                    AccessibleToAllUser = table.Column<bool>(nullable: false),
                    KeywordForSearch = table.Column<string>(maxLength: 50, nullable: true),
                    AccessibilityRuleId = table.Column<string>(maxLength: 100, nullable: true),
                    ConfigurationColumn = table.Column<string>(maxLength: 50, nullable: false),
                    ConfigurationValue = table.Column<string>(maxLength: 50, nullable: false),
                    BookFile = table.Column<string>(maxLength: 2000, nullable: true),
                    BookImage = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentralBookLibrary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommonSmileSheet",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuestionNumber = table.Column<string>(maxLength: 10, nullable: true),
                    Section = table.Column<string>(maxLength: 20, nullable: false),
                    QuestionText = table.Column<string>(nullable: false),
                    QuestionType = table.Column<string>(maxLength: 20, nullable: true),
                    ShowEmoji = table.Column<bool>(nullable: false),
                    Answer1 = table.Column<string>(nullable: false),
                    Answer2 = table.Column<string>(nullable: false),
                    Answer3 = table.Column<string>(nullable: true),
                    Answer4 = table.Column<string>(nullable: true),
                    Answer5 = table.Column<string>(nullable: true),
                    LimitAnswer = table.Column<int>(nullable: false),
                    Skip = table.Column<bool>(nullable: false),
                    FeedbackLevel = table.Column<string>(maxLength: 20, nullable: false),
                    TrainingType = table.Column<string>(maxLength: 20, nullable: false),
                    AnswerCounter = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonSmileSheet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetenciesMapping",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseCategoryId = table.Column<int>(nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: true),
                    CompetencyCategoryId = table.Column<int>(nullable: false),
                    CompetencyId = table.Column<int>(nullable: false),
                    CompetencyLevelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenciesMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetenciesMaster",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    CompetencyName = table.Column<string>(maxLength: 50, nullable: false),
                    CompetencyDescription = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenciesMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetencyCategory",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(maxLength: 50, nullable: false),
                    Category = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetencyJobRole",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    RoleColumn1 = table.Column<string>(nullable: false),
                    RoleColumn1value = table.Column<int>(nullable: false),
                    RoleColumn2 = table.Column<string>(nullable: true),
                    RoleColumn2value = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyJobRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompetencyLevels",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    CompetencyId = table.Column<int>(nullable: false),
                    LevelName = table.Column<string>(maxLength: 100, nullable: false),
                    BriefDescriptionCompetencyLevel = table.Column<string>(maxLength: 200, nullable: false),
                    DetailedDescriptionOfLevel = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetencyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentCompletionStatus",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    ScheduleId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentCompletionStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 30, nullable: false),
                    Title = table.Column<string>(maxLength: 150, nullable: false),
                    CourseType = table.Column<string>(maxLength: 30, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CourseAdminID = table.Column<int>(nullable: true),
                    LearningApproach = table.Column<bool>(maxLength: 15, nullable: true),
                    Language = table.Column<string>(maxLength: 40, nullable: true),
                    IsCertificateIssued = table.Column<bool>(nullable: false),
                    CompletionPeriodDays = table.Column<int>(nullable: false),
                    CourseFee = table.Column<float>(nullable: false),
                    Currency = table.Column<string>(maxLength: 25, nullable: true),
                    CreditsPoints = table.Column<float>(nullable: false),
                    ThumbnailPath = table.Column<string>(maxLength: 200, nullable: true),
                    CategoryId = table.Column<int>(nullable: true),
                    IsPreAssessment = table.Column<bool>(nullable: false),
                    PreAssessmentId = table.Column<int>(nullable: true),
                    IsAssessment = table.Column<bool>(nullable: false),
                    AssessmentId = table.Column<int>(nullable: true),
                    IsFeedback = table.Column<bool>(nullable: false),
                    FeedbackId = table.Column<int>(nullable: true),
                    IsAssignment = table.Column<bool>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: true),
                    IsApplicableToAll = table.Column<bool>(nullable: false),
                    AdminName = table.Column<string>(nullable: true),
                    SubCategoryId = table.Column<int>(nullable: true),
                    Metadata = table.Column<string>(maxLength: 500, nullable: true),
                    IsSection = table.Column<bool>(nullable: false),
                    IsDiscussionBoard = table.Column<bool>(nullable: false),
                    MemoId = table.Column<int>(nullable: true),
                    IsMemoCourse = table.Column<bool>(nullable: false),
                    Mission = table.Column<string>(nullable: true),
                    Points = table.Column<int>(nullable: false),
                    IsAchieveMastery = table.Column<bool>(nullable: false),
                    IsAdaptiveLearning = table.Column<bool>(nullable: false),
                    DurationInMinutes = table.Column<int>(nullable: false),
                    TotalModules = table.Column<int>(nullable: false),
                    IsShowInCatalogue = table.Column<bool>(nullable: false),
                    RowGuid = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    IsModuleHasAssFeed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseCertificateAssociation",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    CertificateImageName = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCertificateAssociation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseCode",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Prefix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseCompletionStatus",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCompletionStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseModuleAssociation",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    IsPreAssessment = table.Column<bool>(nullable: false),
                    PreAssessmentId = table.Column<int>(nullable: true),
                    IsAssessment = table.Column<bool>(nullable: false),
                    AssessmentId = table.Column<int>(nullable: true),
                    IsFeedback = table.Column<bool>(nullable: false),
                    FeedbackId = table.Column<int>(nullable: true),
                    SequenceNo = table.Column<int>(nullable: true),
                    SectionId = table.Column<int>(nullable: true),
                    Isdeleted = table.Column<bool>(nullable: false),
                    CompletionPeriodDays = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseModuleAssociation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseRating",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    OneStar = table.Column<int>(nullable: false),
                    TwoStar = table.Column<int>(nullable: false),
                    ThreeStar = table.Column<int>(nullable: false),
                    FourStar = table.Column<int>(nullable: false),
                    FiveStar = table.Column<int>(nullable: false),
                    Average = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRating", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseRequest",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(nullable: false),
                    CourseID = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    OtherCourseName = table.Column<string>(nullable: true),
                    OtherCourseDescription = table.Column<string>(nullable: true),
                    IsAccessGiven = table.Column<bool>(nullable: false),
                    IsRequestSendToBUHead = table.Column<bool>(nullable: false),
                    IsRequestSendToLM = table.Column<bool>(nullable: false),
                    IsRequestSendToHR = table.Column<bool>(nullable: false),
                    IsRequestSendFromHRTOBU = table.Column<bool>(nullable: false),
                    IsRequestSendFromTA = table.Column<bool>(nullable: false),
                    NewStatus = table.Column<string>(nullable: true),
                    TNAYear = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseReview",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    RatingId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ReviewRating = table.Column<int>(nullable: false),
                    ReviewText = table.Column<string>(maxLength: 500, nullable: false),
                    UseName = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReview", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseScheduleEnrollmentRequest",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ScheduleID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    RequestStatus = table.Column<string>(nullable: true),
                    IsRequestSendToLevel1 = table.Column<bool>(nullable: false),
                    IsRequestSendToLevel2 = table.Column<bool>(nullable: false),
                    IsRequestSendToLevel3 = table.Column<bool>(nullable: false),
                    IsRequestSendToLevel4 = table.Column<bool>(nullable: false),
                    IsRequestSendToLevel5 = table.Column<bool>(nullable: false),
                    IsRequestSendToLevel6 = table.Column<bool>(nullable: false),
                    UserStatusInfo = table.Column<string>(nullable: true),
                    RequestedFrom = table.Column<string>(nullable: true),
                    RequestedFromLevel = table.Column<int>(nullable: false),
                    SentBy = table.Column<string>(nullable: true),
                    BeSpokeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseScheduleEnrollmentRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseScheduleEnrollmentRequestDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseScheduleEnrollmentRequestID = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    StatusUpdatedBy = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    ApprovedLevel = table.Column<int>(nullable: false),
                    IsNominated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseScheduleEnrollmentRequestDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoursesEnrollRequestDetails",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    CoursesEnrollRequestId = table.Column<int>(nullable: false),
                    ActionTakenBy = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursesEnrollRequestDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoursesRequestDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseRequestId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    StatusUpdatedBy = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ReasonForRejection = table.Column<string>(nullable: true),
                    CourseID = table.Column<int>(nullable: false),
                    IsNominate = table.Column<bool>(nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursesRequestDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseWiseEmailReminder",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    MailSubject = table.Column<string>(maxLength: 500, nullable: true),
                    TemplateContent = table.Column<string>(nullable: true),
                    TotalUserCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseWiseEmailReminder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DegreedContent",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContentId = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DegreedContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionForum",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PostThreadId = table.Column<int>(nullable: false),
                    PostParentId = table.Column<int>(nullable: false),
                    PostLevel = table.Column<int>(nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    SubjectText = table.Column<string>(maxLength: 500, nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    UseName = table.Column<string>(maxLength: 200, nullable: false),
                    CourseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionForum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EBTDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    courseTitle = table.Column<string>(nullable: true),
                    FromData = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EBTDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ExceptionLog",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Source = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    StackTrace = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExceptionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faq",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    LcmsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faq", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackOption",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FeedbackQuestionID = table.Column<int>(nullable: false),
                    OptionText = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackQuestion",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Section = table.Column<string>(maxLength: 20, nullable: true),
                    QuestionText = table.Column<string>(nullable: true),
                    QuestionType = table.Column<string>(maxLength: 20, nullable: true),
                    SubjectiveAnswerLimit = table.Column<int>(nullable: true),
                    IsAllowSkipping = table.Column<bool>(nullable: false),
                    IsEmoji = table.Column<bool>(nullable: false),
                    IsSubjective = table.Column<bool>(nullable: false),
                    AnswersCounter = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackQuestionRejected",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseCode = table.Column<string>(maxLength: 250, nullable: true),
                    Section = table.Column<string>(maxLength: 250, nullable: true),
                    QuestionText = table.Column<string>(maxLength: 2000, nullable: true),
                    QuestionType = table.Column<string>(maxLength: 250, nullable: true),
                    SubjectiveAnswerLimit = table.Column<string>(maxLength: 250, nullable: true),
                    IsAllowSkipping = table.Column<string>(maxLength: 250, nullable: true),
                    AnswersCounter = table.Column<string>(maxLength: 250, nullable: true),
                    NoOfOptions = table.Column<string>(maxLength: 250, nullable: true),
                    Option1 = table.Column<string>(maxLength: 250, nullable: true),
                    Option2 = table.Column<string>(maxLength: 250, nullable: true),
                    Option3 = table.Column<string>(maxLength: 250, nullable: true),
                    Option4 = table.Column<string>(maxLength: 250, nullable: true),
                    Option5 = table.Column<string>(maxLength: 250, nullable: true),
                    ErrorMessage = table.Column<string>(maxLength: 2000, nullable: true),
                    IsEmoji = table.Column<string>(maxLength: 200, nullable: true),
                    IsSubjective = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackQuestionRejected", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackSheetConfiguration",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsEmoji = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSheetConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackSheetConfigurationDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ConfigurationSheetId = table.Column<int>(nullable: false),
                    FeedbackId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSheetConfigurationDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackStatus",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackStatusDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FeedbackStatusID = table.Column<int>(nullable: false),
                    FeedBackQuestionID = table.Column<int>(nullable: false),
                    FeedBackOptionID = table.Column<int>(nullable: true),
                    SubjectiveAnswer = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackStatusDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialYear",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    YearStartDate = table.Column<DateTime>(nullable: false),
                    YearEndDate = table.Column<DateTime>(nullable: false),
                    YearDescription = table.Column<string>(nullable: true),
                    Quarter = table.Column<string>(nullable: true),
                    QuarterSequence = table.Column<int>(nullable: false),
                    QuarterStartDate = table.Column<DateTime>(nullable: false),
                    Month = table.Column<string>(nullable: true),
                    MonthStartDate = table.Column<DateTime>(nullable: false),
                    MonthEndDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialYear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoToMeetingDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ScheduleID = table.Column<int>(nullable: false),
                    JoinURL = table.Column<string>(nullable: true),
                    StartMeetingURL = table.Column<string>(nullable: true),
                    UniqueMeetingId = table.Column<int>(nullable: false),
                    ConferenceCallInfo = table.Column<string>(nullable: true),
                    MaxParticipants = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoToMeetingDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GradingRules",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    ModelId = table.Column<int>(nullable: false),
                    GradingRuleID = table.Column<string>(maxLength: 100, nullable: true),
                    ScorePercentage = table.Column<string>(maxLength: 100, nullable: false),
                    Grade = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ILTOnlineSetting",
                schema: "Course",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    ClientID = table.Column<string>(nullable: true),
                    ClientSecret = table.Column<string>(nullable: true),
                    RedirectUri = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTOnlineSetting", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ILTRequestResponse",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    ScheduleID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    TrainingRequesStatus = table.Column<string>(nullable: true),
                    ReferenceRequestID = table.Column<int>(nullable: true),
                    Reason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTRequestResponse", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ILTSchedule",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduleCode = table.Column<string>(nullable: true),
                    ModuleId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    StartTime = table.Column<TimeSpan>(nullable: false),
                    EndTime = table.Column<TimeSpan>(nullable: false),
                    RegistrationEndDate = table.Column<DateTime>(nullable: false),
                    PlaceID = table.Column<int>(nullable: false),
                    TrainerType = table.Column<string>(nullable: true),
                    AcademyAgencyID = table.Column<int>(nullable: true),
                    AcademyTrainerID = table.Column<int>(nullable: true),
                    AcademyTrainerName = table.Column<string>(nullable: true),
                    AgencyTrainerName = table.Column<string>(nullable: true),
                    TrainerDescription = table.Column<string>(nullable: true),
                    ScheduleType = table.Column<string>(nullable: true),
                    ReasonForCancellation = table.Column<string>(nullable: true),
                    EventLogo = table.Column<string>(nullable: true),
                    Cost = table.Column<float>(nullable: false),
                    Currency = table.Column<string>(maxLength: 25, nullable: true),
                    WebinarType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTSchedule", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ILTScheduleTrainerBindings",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduleID = table.Column<int>(nullable: false),
                    TrainerID = table.Column<int>(nullable: true),
                    TrainerName = table.Column<string>(nullable: true),
                    TrainerType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTScheduleTrainerBindings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ILTTrainingAttendance",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduleID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    CourseID = table.Column<int>(nullable: false),
                    IsPresent = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTTrainingAttendance", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ILTTrainingAttendanceDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AttendanceId = table.Column<int>(nullable: false),
                    AttendanceDate = table.Column<DateTime>(nullable: false),
                    InTime = table.Column<TimeSpan>(nullable: false),
                    OutTime = table.Column<TimeSpan>(nullable: false),
                    AttendanceStatus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ILTTrainingAttendanceDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LCMS",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(maxLength: 50, nullable: false),
                    ZipPath = table.Column<string>(maxLength: 300, nullable: true),
                    Path = table.Column<string>(maxLength: 300, nullable: true),
                    Version = table.Column<string>(maxLength: 50, nullable: true),
                    MetaData = table.Column<string>(maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(maxLength: 200, nullable: true),
                    InternalName = table.Column<string>(maxLength: 200, nullable: true),
                    MimeType = table.Column<string>(maxLength: 200, nullable: true),
                    Language = table.Column<string>(maxLength: 50, nullable: true),
                    Duration = table.Column<float>(nullable: false),
                    IsBuiltInAssesment = table.Column<bool>(nullable: false),
                    ThumbnailPath = table.Column<string>(maxLength: 300, nullable: true),
                    IsMobileCompatible = table.Column<bool>(nullable: false),
                    YoutubeVideoId = table.Column<string>(maxLength: 20, nullable: true),
                    AssessmentSheetConfigID = table.Column<int>(nullable: true),
                    FeedbackSheetConfigID = table.Column<int>(nullable: true),
                    LaunchData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LCMS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LcmsQuestionAssociation",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    LcmsId = table.Column<int>(nullable: false),
                    QuetionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LcmsQuestionAssociation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lesson",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    LessonNumber = table.Column<int>(nullable: false),
                    SectionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lesson", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    ModuleType = table.Column<string>(maxLength: 30, nullable: false),
                    CourseType = table.Column<string>(maxLength: 30, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    CreditPoints = table.Column<int>(nullable: true),
                    LCMSId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleCompletionStatus",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ModuleId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleCompletionStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleLevelPlanning",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    BatchId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleLevelPlanning", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleTopicAssociation",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ModuleId = table.Column<int>(nullable: false),
                    TopicId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleTopicAssociation", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Nomination",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    BactchCode = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    TrainingPlace = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false),
                    Nominate = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nomination", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfflineAssessmentScores",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    BatchId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false),
                    TotalMarks = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineAssessmentScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostAssessmentResult",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: true),
                    NoOfAttempts = table.Column<int>(nullable: false),
                    MarksObtained = table.Column<double>(nullable: false),
                    TotalMarks = table.Column<int>(nullable: false),
                    PassingPercentage = table.Column<int>(nullable: false),
                    AssessmentPercentage = table.Column<decimal>(nullable: false),
                    AssessmentResult = table.Column<string>(maxLength: 30, nullable: true),
                    TotalNoQuestions = table.Column<int>(nullable: false),
                    PostAssessmentStatus = table.Column<string>(maxLength: 30, nullable: true),
                    IsPreAssessment = table.Column<bool>(nullable: false),
                    IsContentAssessment = table.Column<bool>(nullable: false),
                    IsAdaptiveAssessment = table.Column<bool>(nullable: false),
                    AssessmentStartTime = table.Column<DateTime>(nullable: true),
                    AssessmentEndTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostAssessmentResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolewiseCompetenciesMapping",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(nullable: false),
                    RoleName = table.Column<string>(maxLength: 200, nullable: false),
                    CompetencyCategoryId = table.Column<int>(nullable: false),
                    CompetencyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolewiseCompetenciesMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleCode",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Prefix = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleCode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleHolidayDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ReferenceID = table.Column<int>(nullable: false),
                    IsHoliday = table.Column<bool>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleHolidayDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScormVarResult",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    Result = table.Column<string>(maxLength: 20, nullable: true),
                    NoOfAttempts = table.Column<int>(nullable: false),
                    Score = table.Column<float>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScormVarResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScormVars",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VarName = table.Column<string>(maxLength: 100, nullable: false),
                    VarValue = table.Column<string>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    NoOfAttempt = table.Column<int>(nullable: false),
                    CourseGuid = table.Column<string>(maxLength: 100, nullable: true),
                    ModuleId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScormVars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Section",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CourseCode = table.Column<string>(nullable: true),
                    SectionNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Section", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategory",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    Code = table.Column<string>(maxLength: 8, nullable: false),
                    Name = table.Column<string>(maxLength: 80, nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubjectiveAssessmentStatus",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AssessmentResultID = table.Column<int>(nullable: false),
                    HeaderID = table.Column<int>(nullable: false),
                    CourseID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    CheckerID = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectiveAssessmentStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TargetSetting",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    TargetDescription = table.Column<string>(maxLength: 500, nullable: false),
                    FrequencyOfAssessment = table.Column<string>(maxLength: 50, nullable: false),
                    DateOfAssessment = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempCourseScheduleEnrollmentRequest",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ScheduleID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    RequestStatus = table.Column<string>(nullable: true),
                    IsRequestSendToLevel1 = table.Column<bool>(nullable: false),
                    UserStatusInfo = table.Column<string>(nullable: true),
                    RequestedFrom = table.Column<string>(nullable: true),
                    SentBy = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempCourseScheduleEnrollmentRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TNAYear",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Year = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNAYear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToDoPriorityList",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RefId = table.Column<int>(nullable: false),
                    Priority = table.Column<bool>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    ScheduleDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToDoPriorityList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopicMaster",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TopicName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicMaster", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TrainerFeedback",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    QuestionNumber = table.Column<int>(nullable: false),
                    Section = table.Column<string>(nullable: false),
                    QuestionText = table.Column<string>(maxLength: 500, nullable: false),
                    QuestionType = table.Column<string>(maxLength: 10, nullable: true),
                    Option1 = table.Column<string>(nullable: false),
                    Option2 = table.Column<string>(nullable: false),
                    Option3 = table.Column<string>(nullable: true),
                    Option4 = table.Column<string>(nullable: true),
                    Option5 = table.Column<string>(nullable: true),
                    Limit = table.Column<string>(nullable: true),
                    FeedbackLevel = table.Column<string>(nullable: false),
                    SessionType = table.Column<string>(maxLength: 50, nullable: false),
                    Counter = table.Column<string>(nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerFeedback", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAttendance",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    BatchId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAttendance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingExpenses",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    BatchId = table.Column<int>(nullable: false),
                    ModuleId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingExpenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingNomination",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduleID = table.Column<int>(nullable: false),
                    TrainingRequestStatus = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: false),
                    ReferenceRequestID = table.Column<int>(nullable: true),
                    RequestCode = table.Column<string>(nullable: true),
                    ModuleID = table.Column<int>(nullable: false),
                    CourseID = table.Column<int>(nullable: false),
                    OTP = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingNomination", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TrainingPlace",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PlaceCode = table.Column<string>(maxLength: 50, nullable: true),
                    Cityname = table.Column<string>(maxLength: 50, nullable: false),
                    PlaceName = table.Column<string>(maxLength: 100, nullable: false),
                    PostalAddress = table.Column<string>(maxLength: 500, nullable: false),
                    TimeZone = table.Column<string>(maxLength: 50, nullable: true),
                    AccommodationCapacity = table.Column<string>(maxLength: 10, nullable: false),
                    Facilities = table.Column<string>(maxLength: 500, nullable: true),
                    PlaceType = table.Column<string>(maxLength: 50, nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    ContactNumber = table.Column<string>(maxLength: 20, nullable: true),
                    AlternateContact = table.Column<string>(maxLength: 20, nullable: true),
                    ContactPerson = table.Column<string>(maxLength: 50, nullable: false),
                    Status = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrendingCourse",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendingCourse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCoursesStatistics",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    NotStarted = table.Column<int>(nullable: false),
                    Inprogress = table.Column<int>(nullable: false),
                    Completed = table.Column<int>(nullable: false),
                    LastRefreshedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoursesStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCourseStatisticsDetails",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseType = table.Column<int>(nullable: false),
                    NotStarted = table.Column<int>(nullable: false),
                    NotStartedDuration = table.Column<int>(nullable: false),
                    Inprogress = table.Column<int>(nullable: false),
                    InprogressDuration = table.Column<int>(nullable: false),
                    Completed = table.Column<int>(nullable: false),
                    CompletedDuration = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCourseStatisticsDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMemo",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsSubmited = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMemo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOTPBindings",
                schema: "Course",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ScheduleID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    CourseID = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    OTP = table.Column<string>(nullable: true),
                    IsAddedInNomination = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOTPBindings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserWiseFeedbackAggregation",
                schema: "Course",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    FeedbackQuestionID = table.Column<int>(nullable: false),
                    FeedbackOptionID = table.Column<int>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CourseRating = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWiseFeedbackAggregation", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ZoomMeetingDetails",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseID = table.Column<int>(nullable: false),
                    ScheduleID = table.Column<int>(nullable: false),
                    Join_url = table.Column<string>(nullable: true),
                    Start_url = table.Column<string>(nullable: true),
                    UniqueMeetingId = table.Column<int>(nullable: false),
                    Host_id = table.Column<string>(nullable: true),
                    Start_time = table.Column<string>(nullable: true),
                    Topic = table.Column<string>(nullable: true),
                    Uuid = table.Column<string>(nullable: true),
                    Timezone = table.Column<string>(nullable: true),
                    Duration = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoomMeetingDetails", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerConnectionString",
                schema: "Masters",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    ConnectionString = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerConnectionString", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                schema: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentsDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    AssignmentId = table.Column<int>(nullable: false),
                    Accountable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentsDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentsDetail_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Course",
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchesFormationDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: true),
                    BatchesFormationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchesFormationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchesFormationDetail_BatchesFormation_BatchesFormationId",
                        column: x => x.BatchesFormationId,
                        principalSchema: "Course",
                        principalTable: "BatchesFormation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoursesEnrollRequest",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    CourseTitle = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    UserName = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursesEnrollRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoursesEnrollRequest_Course_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "Course",
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleLevelPlanningDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TrainingPlaceId = table.Column<int>(nullable: false),
                    ModuleLevelPlanningId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    StartTime = table.Column<string>(maxLength: 10, nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<string>(maxLength: 10, nullable: false),
                    CoTrainerId = table.Column<int>(nullable: false),
                    HRCoOrdinatorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleLevelPlanningDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleLevelPlanningDetail_ModuleLevelPlanning_ModuleLevelPlanningId",
                        column: x => x.ModuleLevelPlanningId,
                        principalSchema: "Course",
                        principalTable: "ModuleLevelPlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfflineAssessmentScoresDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    OfflineAssessmentScoresId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    ObtainedMarks = table.Column<int>(nullable: false),
                    Percentage = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfflineAssessmentScoresDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfflineAssessmentScoresDetail_OfflineAssessmentScores_OfflineAssessmentScoresId",
                        column: x => x.OfflineAssessmentScoresId,
                        principalSchema: "Course",
                        principalTable: "OfflineAssessmentScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAttendanceDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    TrainingAttendanceId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    IsPresent = table.Column<bool>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAttendanceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingAttendanceDetail_TrainingAttendance_TrainingAttendanceId",
                        column: x => x.TrainingAttendanceId,
                        principalSchema: "Course",
                        principalTable: "TrainingAttendance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingExpensesDetail",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TrainingExpenseId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ExpenseHead = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    VendorName = table.Column<string>(maxLength: 100, nullable: false),
                    AmountPaid = table.Column<decimal>(nullable: false),
                    Currency = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingExpensesDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingExpensesDetail_TrainingExpenses_TrainingExpenseId",
                        column: x => x.TrainingExpenseId,
                        principalSchema: "Course",
                        principalTable: "TrainingExpenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentsDetail_AssignmentId",
                schema: "Course",
                table: "AssignmentsDetail",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchesFormationDetail_BatchesFormationId",
                schema: "Course",
                table: "BatchesFormationDetail",
                column: "BatchesFormationId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursesEnrollRequest_CourseId",
                schema: "Course",
                table: "CoursesEnrollRequest",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleLevelPlanningDetail_ModuleLevelPlanningId",
                schema: "Course",
                table: "ModuleLevelPlanningDetail",
                column: "ModuleLevelPlanningId");

            migrationBuilder.CreateIndex(
                name: "IX_OfflineAssessmentScoresDetail_OfflineAssessmentScoresId",
                schema: "Course",
                table: "OfflineAssessmentScoresDetail",
                column: "OfflineAssessmentScoresId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttendanceDetail_TrainingAttendanceId",
                schema: "Course",
                table: "TrainingAttendanceDetail",
                column: "TrainingAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingExpensesDetail_TrainingExpenseId",
                schema: "Course",
                table: "TrainingExpensesDetail",
                column: "TrainingExpenseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateDownloadDetails",
                schema: "Certification");

            migrationBuilder.DropTable(
                name: "CertificateTemplates",
                schema: "Certification");

            migrationBuilder.DropTable(
                name: "AcademyAgencyMaster",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AccessibilityRule",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AccessibilityRuleRejected",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AdditionalLearning",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AgencyMaster",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AnswerSheetsEvaluation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ApplicabilityGroupTemplate",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentAttemptManagement",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentConfiguration",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentQuestion",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentQuestionDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentQuestionOption",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentQuestionRejected",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentSheetConfiguration",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssessmentSheetConfigurationDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssignmentDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AssignmentsDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AuthoringMaster",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "AuthoringMasterDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BatchAnnouncement",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BatchesFormationDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BespokeParticipants",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BespokeRequest",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BookCategory",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Category",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CentralBookLibrary",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CommonSmileSheet",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CompetenciesMapping",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CompetenciesMaster",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CompetencyCategory",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CompetencyJobRole",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CompetencyLevels",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ContentCompletionStatus",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseCertificateAssociation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseCode",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseCompletionStatus",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseModuleAssociation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseRating",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseRequest",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseReview",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseScheduleEnrollmentRequest",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseScheduleEnrollmentRequestDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CoursesEnrollRequest",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CoursesEnrollRequestDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CoursesRequestDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CourseWiseEmailReminder",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "DegreedContent",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "DiscussionForum",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "EBTDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ExceptionLog",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Faq",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackOption",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackQuestion",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackQuestionRejected",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackSheetConfiguration",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackSheetConfigurationDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackStatus",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FeedbackStatusDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "FinancialYear",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "GoToMeetingDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "GradingRules",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTOnlineSetting",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTRequestResponse",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTSchedule",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTScheduleTrainerBindings",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTTrainingAttendance",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ILTTrainingAttendanceDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "LCMS",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "LcmsQuestionAssociation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Lesson",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Module",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ModuleCompletionStatus",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ModuleLevelPlanningDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ModuleTopicAssociation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Nomination",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "OfflineAssessmentScoresDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "PostAssessmentResult",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "RolewiseCompetenciesMapping",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ScheduleCode",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ScheduleHolidayDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ScormVarResult",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ScormVars",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Section",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "SubCategory",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "SubjectiveAssessmentStatus",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TargetSetting",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TempCourseScheduleEnrollmentRequest",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TNAYear",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ToDoPriorityList",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TopicMaster",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainerFeedback",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingAttendanceDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingExpensesDetail",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingNomination",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingPlace",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrendingCourse",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "UserCoursesStatistics",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "UserCourseStatisticsDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "UserMemo",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "UserOTPBindings",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "UserWiseFeedbackAggregation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ZoomMeetingDetails",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "CustomerConnectionString",
                schema: "Masters");

            migrationBuilder.DropTable(
                name: "Tokens",
                schema: "User");

            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "BatchesFormation",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "Course",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ModuleLevelPlanning",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "OfflineAssessmentScores",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingAttendance",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "TrainingExpenses",
                schema: "Course");
        }
    }
}
