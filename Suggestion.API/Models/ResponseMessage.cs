// ======================================
// <copyright file="ResponseMessage.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel;


namespace Suggestion.API.Models
{
    public class ResponseMessage
    {
        public string Message { get; set; }
        public string Description { get; set; }
        public int StatusCode { get; set; }
    }

    internal enum MessageType
    {
        [Description("Record was saved successfully")]
        Success,
        [Description("Failed to save")]
        Fail,
        [Description("Duplicate! Already exist.")]
        Duplicate,
        [Description("Deleted record")]
        Delete,
        [Description("Duplicate! Object Title Already exit.")]
        DuplicateTitle,
        [Description("No record found!")]
        NotFound,
        [Description("Quiz Does Not Contain Any Questions!")]
        EmptyQuiz,
        [Description("Record does not exist!")]
        NotExist,
        [Description("Data not available")]
        DataNotAvailable,
        [Description("Invalid data!")]
        InvalidData,
        [Description("Internal Server Error!")]
        InternalServerError,
        [Description("Quiz in Use Do not edit Record!")]
        QuizInUse,
        [Description("Survey in Use Do not edit Record!")]
        SurveyInUse,
        [Description("Poll in Use Do not edit Record!")]
        PollInUse,
        [Description("This Survey already submitted!")]
        SurveyAlreadySubmitted,
        [Description("Invalid file!")]
        InvalidFile,
        [Description("Duplicate Options!")]
        DuplicateOptions,
        [Description("Question already exist!")]
        QuestionExist,
        [Description("Question is empty!")]
        QuestionEmpty,
        [Description("Your opinion is already submitted!")]
        DuplicatePollResponse,
        [Description("You already submitted this quiz!")]
        DuplicateQuizResponse,
        [Description("Options must in the range of 2 to 10!")]
        InvalidOptionRange,
        [Description("Options are miss match with number of option!")]
        OptionAndOptionsMissMatch,     
        [Description("Status Changed Successfully!")]
        StatusChanged,
        [Description("3 challenges already submitted by you")]
        CompletedAttempts,
        [Description("Thank you. You have successfully submitted your all the challanges")]
        SubmittedAttempts,
        [Description("3 attempts already submitted by you.")]
        CompletedAttemptsofApplication,
        [Description("Jury Score is already Submitted.")]
        JuryScoreAlreadySubmitted,    
        [Description("Invalid post request")]
        InvalidPostRequest,
        [Description("You Cannot Review Yourself")]
        ReviewYourself,
        [Description("Award already exist for given project and date")]
        DuplicateAward
    }

enum FileMessages
{
    [Description("Excel file is not in required format. Please check sample excel file format and try uploading again.")]
    FileFormatError,
    [Description("Imported file does not contain data.")]
    FileEmpty,
    [Description("Error while importing file.Please contact support.")]
    FileErrorInImport,
   
}
}
