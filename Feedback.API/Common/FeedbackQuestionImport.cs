using Feedback.API.Model;
using Feedback.API.Repositories.Interfaces;
using OfficeOpenXml;
using System.Text;
using log4net;
using Feedback.API.APIModel;
using Feedback.API.Helper;

namespace Feedback.API.Common
{
    public class FeedbackQuestionImport
    {
        public static class ProcessFile
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));
            static StringBuilder sb = new StringBuilder();
            static string[] header = { };
            static string[] headerStar = { };
            static string[] headerWithoutStar = { };
            static List<string> feedbackQuestionRecord = new List<string>();
            static FeedbackQuestion feedbackQuestion = new FeedbackQuestion();
            static FeedbackQuestionRejected feedbackQuestionRejected = new FeedbackQuestionRejected();
            static StringBuilder sbError = new StringBuilder();
            static int totalRecordInsert = 0;
            static int totalRecordRejected = 0;

            public static void Reset()
            {
                sb.Clear();
                header = new string[0];
                headerStar = new string[0];
                headerWithoutStar = new string[0];
                feedbackQuestionRecord.Clear();
                feedbackQuestion = new FeedbackQuestion();
                feedbackQuestionRejected = new FeedbackQuestionRejected();
                sbError.Clear();
                totalRecordInsert = 0;
                totalRecordRejected = 0;
            }




            public static async Task<int> InitilizeAsync(FileInfo file)

            {
                bool result = false;
                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= ColCount; col++)
                        {
                            string append = "";
                            if (worksheet.Cells[row, col].Value == null)
                            {

                            }
                            else
                            {
                                append = Convert.ToString(worksheet.Cells[row, col].Value.ToString());
                            }
                            string finalAppend = append + "\t";
                            sb.Append(finalAppend);

                        }
                        sb.Append(Environment.NewLine);
                    }

                    string fileInfo = sb.ToString();
                    feedbackQuestionRecord = new List<string>(fileInfo.Split('\n'));
                    if (rowCount != feedbackQuestionRecord.Count - 1)
                    {
                        sbError.Append("Cannot contain new line characters");
                        {
                            return 2;

                        }
                    }
                    foreach (string record in feedbackQuestionRecord)
                    {
                        string[] mainsp = record.Split('\r');
                        string[] mainsp2 = mainsp[0].Split('\"');
                        header = mainsp2[0].Split('\t');
                        headerStar = mainsp2[0].Split('\t');
                        break;
                    }
                    feedbackQuestionRecord.RemoveAt(0);


                }
                /////Remove Star from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("*", "");

                }

                /////Remove Hash from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("#", "");

                }

                int count = 0;
                for (int i = 0; i < header.Count() - 1; i++)
                {
                    string headerColumn = header[i].ToString().Trim();
                    if (!string.IsNullOrEmpty(headerColumn))
                    {
                        count++;
                    }

                }
                if (count == 16)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }

            }

            public static async Task<string> ProcessRecordsAsync(IFeedbackQuestion _feedbackQuestion, IFeedbackQuestionRejectedRepository _feedbackQuestionRejectedRepository, IFeedbackOption _feedbackOption, int UserId)
            {
                try
                {
                    if (feedbackQuestionRecord != null && feedbackQuestionRecord.Count > 0)
                    {
                        foreach (string record in feedbackQuestionRecord)
                        {

                            int countLenght = record.Length;
                            if (record != null && countLenght > 1)
                            {
                                string[] textpart = record.Split('\t');
                                string[][] mainRecord = { header, textpart };
                                string txtQuestioText = string.Empty;
                                string txtCourseCode = string.Empty;
                                string txtQuestionType = string.Empty;
                                string txtSection = string.Empty;
                                string txtNoOfOptions = string.Empty;
                                string txtIsAllowSkipping = string.Empty;
                                string txtIsActive = string.Empty;
                                string txtOption3 = string.Empty;
                                string txtOption4 = string.Empty;
                                string txtOption5 = string.Empty;
                                string txtOption1 = string.Empty;
                                string txtOption2 = string.Empty;

                                string txtOption6 = string.Empty;
                                string txtOption7 = string.Empty;
                                string txtOption8 = string.Empty;
                                string txtOption9 = string.Empty;
                                string txtOption10 = string.Empty;
                                string txtMetadata = string.Empty;


                                string txtIsEmoji = string.Empty;
                                string txtIsSubjective = string.Empty;

                                string headerText = "";

                                int arrasize = header.Count();

                                for (int j = 0; j < arrasize; j++)
                                {
                                    headerText = header[j];
                                    string[] mainspilt = headerText.Split('\t');

                                    headerText = mainspilt[0];

                                    if (headerText == FedbackImportField.QuestionText)
                                    {
                                        //UserID
                                        try
                                        {
                                            string textQuestionText = mainRecord[1][j];
                                            string[] QuestionTextsplit = mainRecord[1][j].Split('\t');
                                            txtQuestioText = QuestionTextsplit[0];
                                            string QuestionText = txtQuestioText.Trim();

                                            bool valid = ValidateQuestionText(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }

                                    }

                                    if (headerText == FedbackImportField.IsEmoji)
                                    {
                                        //UserID
                                        try
                                        {
                                            string IsEmoji = mainRecord[1][j];
                                            string[] textIsEmojisplit = mainRecord[1][j].Split('\t');
                                            txtIsEmoji = textIsEmojisplit[0];
                                            string IsEmojiText = txtIsEmoji.Trim();

                                            bool valid = ValidateIsEmojiText(headerText, IsEmojiText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }

                                    }

                                    if (headerText == FedbackImportField.IsSubjective)
                                    {
                                        //UserID
                                        try
                                        {
                                            string IsSubjective = mainRecord[1][j];
                                            string[] textIsSubjectivesplit = mainRecord[1][j].Split('\t');
                                            txtIsSubjective = textIsSubjectivesplit[0];
                                            string IsSubjectiveText = txtIsSubjective.Trim();

                                            bool valid = ValidateIsSubjectiveText(headerText, IsSubjectiveText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }

                                    }

                                    else

                                         if (headerText == FedbackImportField.NoOfOptions)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions = mainRecord[1][j];
                                            string[] textuserMobilesplit = mainRecord[1][j].Split('\t');
                                            string textNoOfOptions = textuserMobilesplit[0];
                                            txtNoOfOptions = textNoOfOptions.Trim();

                                            bool valid = ValidateNoOfOptions(headerText, txtNoOfOptions, txtIsEmoji, txtIsSubjective);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option1)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions1 = mainRecord[1][j];
                                            string[] textOption1split = mainRecord[1][j].Split('\t');
                                            string textOption1 = textOption1split[0];
                                            txtOption1 = textOption1.Trim();

                                            bool valid = ValidateOption1(headerText, txtOption1, txtIsEmoji, txtIsSubjective);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option2)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions2 = mainRecord[1][j];
                                            string[] textOption2split = mainRecord[1][j].Split('\t');
                                            string textOption2 = textOption2split[0];
                                            txtOption2 = textOption2.Trim();

                                            bool valid = ValidateOption2(headerText, txtOption2, txtIsEmoji, txtIsSubjective);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option3)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions = mainRecord[1][j];
                                            string[] textOption3split = mainRecord[1][j].Split('\t');
                                            string textOption3 = textOption3split[0];
                                            txtOption3 = textOption3.Trim();

                                            bool valid = ValidateOption3(headerText, txtOption3);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option4)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions4 = mainRecord[1][j];
                                            string[] textOption4split = mainRecord[1][j].Split('\t');
                                            string textOption4 = textOption4split[0];
                                            txtOption4 = textOption4.Trim();

                                            bool valid = ValidateOption4(headerText, txtOption4);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option5)
                                    {
                                        //MobileNumber
                                        try
                                        {

                                            string answerOptions5 = mainRecord[1][j];
                                            string[] textOption5split = mainRecord[1][j].Split('\t');
                                            string textOption5 = textOption5split[0];
                                            txtOption5 = textOption5.Trim();

                                            bool valid = ValidateOption5(headerText, txtOption5);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.Option6)
                                    {

                                        try
                                        {

                                            string answerOptions6 = mainRecord[1][j];
                                            string[] textOption6split = mainRecord[1][j].Split('\t');
                                            string textOption6 = textOption6split[0];
                                            txtOption6 = textOption6.Trim();

                                            bool valid = ValidateOption6(headerText, txtOption6);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }

                                    else
                                         if (headerText == FedbackImportField.Option7)
                                    {

                                        try
                                        {

                                            string answerOptions7 = mainRecord[1][j];
                                            string[] textOption7split = mainRecord[1][j].Split('\t');
                                            string textOption7 = textOption7split[0];
                                            txtOption7 = textOption7.Trim();

                                            bool valid = ValidateOption7(headerText, txtOption7);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }

                                    else
                                         if (headerText == FedbackImportField.Option8)
                                    {

                                        try
                                        {

                                            string answerOptions8 = mainRecord[1][j];
                                            string[] textOption8split = mainRecord[1][j].Split('\t');
                                            string textOption8 = textOption8split[0];
                                            txtOption8 = textOption8.Trim();

                                            bool valid = ValidateOption8(headerText, txtOption8);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }

                                    else
                                         if (headerText == FedbackImportField.Option9)
                                    {

                                        try
                                        {

                                            string answerOptions9 = mainRecord[1][j];
                                            string[] textOption9split = mainRecord[1][j].Split('\t');
                                            string textOption9 = textOption9split[0];
                                            txtOption9 = textOption9.Trim();

                                            bool valid = ValidateOption9(headerText, txtOption9);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }

                                    else
                                         if (headerText == FedbackImportField.Option10)
                                    {

                                        try
                                        {

                                            string answerOptions10 = mainRecord[1][j];
                                            string[] textOption10split = mainRecord[1][j].Split('\t');
                                            string textOption10 = textOption10split[0];
                                            txtOption10 = textOption10.Trim();

                                            bool valid = ValidateOption10(headerText, txtOption10);
                                            if (valid == true)
                                            {
                                                break;
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    else
                                         if (headerText == FedbackImportField.CourseCode)
                                    {

                                        try
                                        {

                                            string ttxtCourseCode = mainRecord[1][j];
                                            string[] txtCourseCodesplit = mainRecord[1][j].Split('\t');
                                            string textCourseCode = txtCourseCodesplit[0];
                                            txtCourseCode = textCourseCode.Trim();

                                            bool valid = ValidateCourseCode(headerText, txtCourseCode);
                                            if (valid == true)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (txtCourseCode != null && !string.IsNullOrEmpty(txtCourseCode))
                                                {
                                                    APIFQCourse coursedata = await _feedbackQuestion.courseCodeExists(txtCourseCode);
                                                    if (coursedata == null)
                                                    {
                                                        sbError.Append(Record.CourseCodeNotExists);
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        feedbackQuestion.CourseId = coursedata.CourseId;
                                                        // break;
                                                    }
                                                }
                                                else
                                                {
                                                    // break;
                                                }

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }
                                    }
                                    if (headerText == FedbackImportField.Metadata)
                                    {
                                        //metadata
                                        try
                                        {
                                            string textmetadataText = mainRecord[1][j];
                                            string[] MetadataTextsplit = mainRecord[1][j].Split('\t');
                                            txtMetadata = MetadataTextsplit[0];
                                            string MetadataText = txtMetadata.Trim();

                                            bool valid = ValidateMetadataText(headerText, txtMetadata);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }

                                    }

                                }



                                {
                                    for (int j = 0; j < 1; j++)
                                    {
                                        if (sbError.ToString() == "" || string.IsNullOrEmpty(sbError.ToString()))
                                        {
                                            if (Convert.ToBoolean(txtIsEmoji) == false && Convert.ToBoolean(txtIsSubjective) == false)
                                            {
                                                //Check No of Option Min 2 to 10
                                                if (txtNoOfOptions != null && txtNoOfOptions != string.Empty)
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) > 1 && Convert.ToInt32(txtNoOfOptions) < 11)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.EnterNoOfOptionsBetween);
                                                        break;
                                                    }
                                                }
                                                // Check Question Type is Objective or Subjective
                                                if (Convert.ToInt32(txtNoOfOptions) <= 1 || txtNoOfOptions == null || txtNoOfOptions == string.Empty || txtNoOfOptions == "")
                                                {
                                                    sbError.Append(Record.EnterMinimumOption);
                                                    break;
                                                }


                                                //Check Option is null or not
                                                //option10
                                                if ((Convert.ToInt32(txtNoOfOptions) == 10))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 10 && ((txtOption10 != null && !string.IsNullOrEmpty(txtOption10)) && (txtOption9 != null && !string.IsNullOrEmpty(txtOption9)) && (txtOption8 != null && !string.IsNullOrEmpty(txtOption8)) && (txtOption7 != null && !string.IsNullOrEmpty(txtOption7)) && (txtOption6 != null && !string.IsNullOrEmpty(txtOption6)) && (txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption1.ToLower() == txtOption6.ToLower()) || (txtOption1.ToLower() == txtOption7.ToLower()) || (txtOption1.ToLower() == txtOption8.ToLower()) || (txtOption1.ToLower() == txtOption9.ToLower()) || (txtOption1.ToLower() == txtOption10.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption6.ToLower()) || (txtOption2.ToLower() == txtOption7.ToLower()) || (txtOption2.ToLower() == txtOption8.ToLower()) || (txtOption2.ToLower() == txtOption9.ToLower()) || (txtOption2.ToLower() == txtOption10.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption6.ToLower()) || (txtOption3.ToLower() == txtOption7.ToLower()) || (txtOption3.ToLower() == txtOption8.ToLower()) || (txtOption3.ToLower() == txtOption9.ToLower()) || (txtOption3.ToLower() == txtOption10.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption6.ToLower()) || (txtOption4.ToLower() == txtOption7.ToLower()) || (txtOption4.ToLower() == txtOption8.ToLower()) || (txtOption4.ToLower() == txtOption9.ToLower()) || (txtOption4.ToLower() == txtOption10.ToLower()) || (txtOption5.ToLower() == txtOption6.ToLower()) || (txtOption5.ToLower() == txtOption7.ToLower()) || (txtOption5.ToLower() == txtOption8.ToLower()) || (txtOption5.ToLower() == txtOption9.ToLower()) || (txtOption5.ToLower() == txtOption10.ToLower()) || (txtOption6.ToLower() == txtOption7.ToLower()) || (txtOption6.ToLower() == txtOption8.ToLower()) || (txtOption6.ToLower() == txtOption9.ToLower()) || (txtOption6.ToLower() == txtOption10.ToLower()) || (txtOption7.ToLower() == txtOption8.ToLower()) || (txtOption7.ToLower() == txtOption9.ToLower()) || (txtOption7.ToLower() == txtOption10.ToLower()) || (txtOption8.ToLower() == txtOption9.ToLower()) || (txtOption8.ToLower() == txtOption10.ToLower()) || (txtOption9.ToLower() == txtOption10.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option9
                                                if ((Convert.ToInt32(txtNoOfOptions) == 9))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 9 && ((txtOption9 != null && !string.IsNullOrEmpty(txtOption9)) && (txtOption8 != null && !string.IsNullOrEmpty(txtOption8)) && (txtOption7 != null && !string.IsNullOrEmpty(txtOption7)) && (txtOption6 != null && !string.IsNullOrEmpty(txtOption6)) && (txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption1.ToLower() == txtOption6.ToLower()) || (txtOption1.ToLower() == txtOption7.ToLower()) || (txtOption1.ToLower() == txtOption8.ToLower()) || (txtOption1.ToLower() == txtOption9.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption6.ToLower()) || (txtOption2.ToLower() == txtOption7.ToLower()) || (txtOption2.ToLower() == txtOption8.ToLower()) || (txtOption2.ToLower() == txtOption9.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption6.ToLower()) || (txtOption3.ToLower() == txtOption7.ToLower()) || (txtOption3.ToLower() == txtOption8.ToLower()) || (txtOption3.ToLower() == txtOption9.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption6.ToLower()) || (txtOption4.ToLower() == txtOption7.ToLower()) || (txtOption4.ToLower() == txtOption8.ToLower()) || (txtOption4.ToLower() == txtOption9.ToLower()) || (txtOption5.ToLower() == txtOption6.ToLower()) || (txtOption5.ToLower() == txtOption7.ToLower()) || (txtOption5.ToLower() == txtOption8.ToLower()) || (txtOption5.ToLower() == txtOption9.ToLower()) || (txtOption6.ToLower() == txtOption7.ToLower()) || (txtOption6.ToLower() == txtOption8.ToLower()) || (txtOption6.ToLower() == txtOption9.ToLower()) || (txtOption7.ToLower() == txtOption8.ToLower()) || (txtOption7.ToLower() == txtOption9.ToLower()) || (txtOption8.ToLower() == txtOption9.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option8
                                                if ((Convert.ToInt32(txtNoOfOptions) == 8))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 8 && ((txtOption8 != null && !string.IsNullOrEmpty(txtOption8)) && (txtOption7 != null && !string.IsNullOrEmpty(txtOption7)) && (txtOption6 != null && !string.IsNullOrEmpty(txtOption6)) && (txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption1.ToLower() == txtOption6.ToLower()) || (txtOption1.ToLower() == txtOption7.ToLower()) || (txtOption1.ToLower() == txtOption8.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption6.ToLower()) || (txtOption2.ToLower() == txtOption7.ToLower()) || (txtOption2.ToLower() == txtOption8.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption6.ToLower()) || (txtOption3.ToLower() == txtOption7.ToLower()) || (txtOption3.ToLower() == txtOption8.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption6.ToLower()) || (txtOption4.ToLower() == txtOption7.ToLower()) || (txtOption4.ToLower() == txtOption8.ToLower()) || (txtOption5.ToLower() == txtOption6.ToLower()) || (txtOption5.ToLower() == txtOption7.ToLower()) || (txtOption5.ToLower() == txtOption8.ToLower()) || (txtOption6.ToLower() == txtOption7.ToLower()) || (txtOption6.ToLower() == txtOption8.ToLower()) || (txtOption7.ToLower() == txtOption8.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option7
                                                if ((Convert.ToInt32(txtNoOfOptions) == 7))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 7 && ((txtOption7 != null && !string.IsNullOrEmpty(txtOption7)) && (txtOption6 != null && !string.IsNullOrEmpty(txtOption6)) && (txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption1.ToLower() == txtOption6.ToLower()) || (txtOption1.ToLower() == txtOption7.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption6.ToLower()) || (txtOption2.ToLower() == txtOption7.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption6.ToLower()) || (txtOption3.ToLower() == txtOption7.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption6.ToLower()) || (txtOption4.ToLower() == txtOption7.ToLower()) || (txtOption5.ToLower() == txtOption6.ToLower()) || (txtOption5.ToLower() == txtOption7.ToLower()) || (txtOption6.ToLower() == txtOption7.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option6
                                                if ((Convert.ToInt32(txtNoOfOptions) == 6))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 6 && ((txtOption6 != null && !string.IsNullOrEmpty(txtOption6)) && (txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption1.ToLower() == txtOption6.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption6.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption6.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption6.ToLower()) || (txtOption5.ToLower() == txtOption6.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option5
                                                if ((Convert.ToInt32(txtNoOfOptions) == 5))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 5 && ((txtOption5 != null && !string.IsNullOrEmpty(txtOption5)) && (txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption1.ToLower() == txtOption5.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption5.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption5.ToLower()) || (txtOption4.ToLower() == txtOption5.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }
                                                //option4
                                                if ((Convert.ToInt32(txtNoOfOptions) == 4))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 4 && ((txtOption4 != null && !string.IsNullOrEmpty(txtOption4)) && (txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption1.ToLower() == txtOption4.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption4.ToLower()) || (txtOption3.ToLower() == txtOption4.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }

                                                //option3
                                                if ((Convert.ToInt32(txtNoOfOptions) == 3))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 3 && ((txtOption3 != null && !string.IsNullOrEmpty(txtOption3)) && (txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()) || (txtOption1.ToLower() == txtOption3.ToLower()) || (txtOption2.ToLower() == txtOption3.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }

                                                //option2
                                                if ((Convert.ToInt32(txtNoOfOptions) == 2))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) == 2 && ((txtOption2 != null && !string.IsNullOrEmpty(txtOption2)) && (txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        if ((txtOption1.ToLower() == txtOption2.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }

                                                //option1
                                                if ((Convert.ToInt32(txtNoOfOptions) <= 1))
                                                {
                                                    if (Convert.ToInt32(txtNoOfOptions) <= 1 && ((txtOption1 != null && !string.IsNullOrEmpty(txtOption1))))
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }

                                            }

                                            bool question = _feedbackQuestion.QuestionExists(feedbackQuestion.QuestionText);
                                            if (question == true)
                                            {
                                                sbError.Append(Record.DuplicateQuestion);
                                                break;
                                            }
                                        }
                                    }
                                }

                                bool validvalue = false;


                                if (validvalue == true)
                                {
                                    sbError.Append("  Please enter valid data,");

                                    feedbackQuestionRejected.ErrorMessage = sbError.ToString();

                                    feedbackQuestionRejected.CreatedDate = DateTime.UtcNow;
                                    feedbackQuestionRejected.CreatedBy = UserId;
                                    feedbackQuestionRejected.ModifiedDate = DateTime.UtcNow;
                                    feedbackQuestionRejected.ModifiedBy = UserId;
                                    feedbackQuestionRejected.ErrorMessage = sbError.ToString();
                                    await _feedbackQuestionRejectedRepository.Add(feedbackQuestionRejected);
                                    totalRecordRejected++;
                                    feedbackQuestion = new FeedbackQuestion();
                                    feedbackQuestionRejected = new FeedbackQuestionRejected();
                                    sbError.Clear();
                                }
                                else
                                {

                                    feedbackQuestionRejected.ErrorMessage = sbError.ToString();

                                    if (sbError.ToString() != "")
                                    {
                                        feedbackQuestionRejected.CreatedDate = DateTime.UtcNow;
                                        feedbackQuestionRejected.CreatedBy = UserId;
                                        feedbackQuestionRejected.ModifiedDate = DateTime.UtcNow;
                                        feedbackQuestionRejected.ModifiedBy = UserId;
                                        feedbackQuestionRejected.ErrorMessage = sbError.ToString();
                                        await _feedbackQuestionRejectedRepository.Add(feedbackQuestionRejected);
                                        totalRecordRejected++;
                                        feedbackQuestion = new FeedbackQuestion();
                                        feedbackQuestionRejected = new FeedbackQuestionRejected();
                                        sbError.Clear();

                                    }
                                    else
                                    {
                                        //add Record 

                                        feedbackQuestion.CreatedDate = DateTime.UtcNow;
                                        feedbackQuestion.CreatedBy = UserId;
                                        feedbackQuestion.IsActive = true;
                                        feedbackQuestion.IsAllowSkipping = false;
                                        feedbackQuestion.ModifiedBy = UserId;
                                        feedbackQuestion.ModifiedDate = DateTime.UtcNow;
                                        if (feedbackQuestion.IsEmoji == false && feedbackQuestion.IsSubjective == false)
                                        {
                                            feedbackQuestion.QuestionType = Record.Objective;
                                        }
                                        else if (feedbackQuestion.IsEmoji == true)
                                        {
                                            feedbackQuestion.QuestionType = Record.Emoji;
                                        }
                                        else if (feedbackQuestion.IsSubjective == true)
                                        {
                                            feedbackQuestion.QuestionType = Record.Subjective;
                                        }

                                        await _feedbackQuestion.Add(feedbackQuestion);

                                        if (feedbackQuestion.IsEmoji == false && feedbackQuestion.IsSubjective == false)
                                        {
                                            if (feedbackQuestion.Id != null || feedbackQuestion.Id != 0)
                                            {
                                                if (txtOption1 != null && Convert.ToInt32(txtNoOfOptions) >= 1)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption1;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }

                                                if (txtOption2 != null && Convert.ToInt32(txtNoOfOptions) >= 2)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption2;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }

                                                if (txtOption3 != null && Convert.ToInt32(txtNoOfOptions) >= 3)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption3;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }

                                                if (txtOption4 != null && Convert.ToInt32(txtNoOfOptions) >= 4)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption4;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }

                                                if (txtOption5 != null && Convert.ToInt32(txtNoOfOptions) >= 5)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption5;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }

                                                if (txtOption6 != null && Convert.ToInt32(txtNoOfOptions) >= 6)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption6;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }
                                                if (txtOption7 != null && Convert.ToInt32(txtNoOfOptions) >= 7)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption7;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }
                                                if (txtOption8 != null && Convert.ToInt32(txtNoOfOptions) >= 8)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption8;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }
                                                if (txtOption9 != null && Convert.ToInt32(txtNoOfOptions) >= 9)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption9;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }
                                                if (txtOption10 != null && Convert.ToInt32(txtNoOfOptions) >= 10)
                                                {
                                                    FeedbackOption feedbackOption = new FeedbackOption();
                                                    feedbackOption.OptionText = txtOption10;
                                                    feedbackOption.FeedbackQuestionID = feedbackQuestion.Id;
                                                    feedbackOption.CreatedBy = UserId;
                                                    feedbackOption.CreatedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedDate = DateTime.UtcNow;
                                                    feedbackOption.ModifiedBy = UserId;
                                                    await _feedbackOption.Add(feedbackOption);
                                                }
                                            }
                                        }
                                        totalRecordInsert++;
                                        feedbackQuestion = new FeedbackQuestion();
                                        feedbackQuestionRejected = new FeedbackQuestionRejected();
                                        sbError.Clear();
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    return (Record.TotalRecordInserted + totalRecordInsert + Record.TotalRecordRejected + totalRecordRejected);
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return (Record.TotalRecordInserted + totalRecordInsert + Record.TotalRecordRejected + totalRecordRejected);
            }
            public static bool ValidateMetadataText(string headerText, string metadataText)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (metadataText != null && !string.IsNullOrEmpty(metadataText))
                    {
                        if (metadataText.Length <= 200)
                        {
                            feedbackQuestion.Metadata = metadataText;
                            feedbackQuestionRejected.Metadata = metadataText;

                        }
                        else
                        {
                            feedbackQuestion.Metadata = metadataText;
                            feedbackQuestionRejected.Metadata = metadataText;
                            sbError.Append(Record.TextMoreThan200);
                            valid = true;
                            return valid;
                        }
                    }


                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            //Validate IsEmoji
            public static bool ValidateQuestionText(string headerText, string questionText)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (questionText != null && !string.IsNullOrEmpty(questionText))
                    {
                        if (questionText.Length <= 500)
                        {
                            feedbackQuestion.QuestionText = questionText;
                            feedbackQuestionRejected.QuestionText = questionText;

                        }
                        else
                        {
                            feedbackQuestion.QuestionText = questionText;
                            feedbackQuestionRejected.QuestionText = questionText;
                            sbError.Append(Record.TextMoreThan500);
                            return valid;
                        }
                    }
                    else
                    {
                        feedbackQuestion.QuestionText = questionText;
                        feedbackQuestionRejected.QuestionText = questionText;
                        sbError.Append("QuestionText is null");
                        valid = true;
                    }

                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            //Validate IsEmoji
            public static bool ValidateIsEmojiText(string headerText, string IsEmojiText)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (IsEmojiText != null && !string.IsNullOrEmpty(IsEmojiText))
                    {
                        if ((IsEmojiText.ToLower()) == Record.False || (IsEmojiText.ToLower()) == Record.True)
                        {
                            feedbackQuestion.IsEmoji = Convert.ToBoolean(IsEmojiText);
                            feedbackQuestionRejected.IsEmoji = IsEmojiText;
                        }
                        else
                        {
                            feedbackQuestionRejected.IsEmoji = IsEmojiText;
                            sbError.Append(Record.PleaseFalseTrueIsEmoji);
                            valid = true;
                        }
                    }
                    else
                    {
                        feedbackQuestionRejected.IsEmoji = IsEmojiText;
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }

                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            //Validate IsSubjective
            public static bool ValidateIsSubjectiveText(string headerText, string IsSubjectiveText)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (IsSubjectiveText != null && !string.IsNullOrEmpty(IsSubjectiveText))
                    {
                        if ((IsSubjectiveText.ToLower()) == Record.False || (IsSubjectiveText.ToLower()) == Record.True)
                        {
                            feedbackQuestion.IsSubjective = Convert.ToBoolean(IsSubjectiveText);
                            feedbackQuestionRejected.IsSubjective = IsSubjectiveText;
                        }
                        else
                        {
                            feedbackQuestionRejected.IsSubjective = IsSubjectiveText;
                            sbError.Append(Record.PleaseFalseTrueIsSubjective);
                            valid = true;
                        }
                    }
                    else
                    {
                        feedbackQuestionRejected.IsSubjective = IsSubjectiveText;
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }

                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateIsAllowSkipping(string headerText, string isAllowSkipping)
            {
                bool valid = false;

                //UserId
                try
                {
                    if (isAllowSkipping != null && !string.IsNullOrEmpty(isAllowSkipping))
                    {
                        if ((isAllowSkipping.ToLower()) == Record.False || (isAllowSkipping.ToLower()) == Record.True)
                        {
                            feedbackQuestion.IsAllowSkipping = Convert.ToBoolean(isAllowSkipping);
                            feedbackQuestionRejected.IsAllowSkipping = isAllowSkipping;
                        }
                        else
                        {
                            feedbackQuestion.IsAllowSkipping = false;
                            feedbackQuestionRejected.IsAllowSkipping = Convert.ToString(false);
                        }
                    }
                    else
                    {
                        feedbackQuestion.IsAllowSkipping = false;
                        feedbackQuestionRejected.IsAllowSkipping = Convert.ToString(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateIsActive(string headerText, string isActive)
            {
                bool valid = false;

                //UserId
                try
                {
                    if (isActive != null && !string.IsNullOrEmpty(isActive))
                    {
                        if ((isActive.ToLower()) == Record.False || (isActive.ToLower()) == Record.True)
                        {
                            feedbackQuestion.IsActive = Convert.ToBoolean(isActive);
                            feedbackQuestionRejected.IsActive = Convert.ToBoolean(isActive);
                        }
                        else
                        {
                            feedbackQuestionRejected.IsActive = Convert.ToBoolean(isActive);
                            sbError.Append(Record.PleaseFalseTrueIsActive);
                            valid = true;
                        }
                    }
                    else
                    {
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateNoOfOptions(string headerText, string noOfOptions, string isImoji, string isSubjective)
            {

                bool valid = false;
                //UserId
                try
                {
                    if (noOfOptions != null && !string.IsNullOrEmpty(noOfOptions))
                    {
                        try
                        {
                            if (Convert.ToInt32(noOfOptions) > 0)
                            {

                                feedbackQuestionRejected.NoOfOptions = noOfOptions;
                            }
                            else

                            {
                                sbError.Append("Please eneter valid " + headerText);
                                valid = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                            sbError.Append("Please eneter valid " + headerText);
                            valid = true;
                        }
                    }
                    else
                    {
                        if ((isImoji.ToLower()) == Record.False || (isImoji.ToLower()) == Record.True || (isSubjective.ToLower()) == Record.False || (isSubjective.ToLower()) == Record.True)
                        {
                            if (Convert.ToBoolean(isImoji) == false)
                            {
                                if (Convert.ToBoolean(isSubjective) == false)
                                {
                                    sbError.Append(headerText + Record.EmptyMessage);
                                    valid = true;
                                }
                            }
                        }
                        else
                        {
                            feedbackQuestionRejected.NoOfOptions = noOfOptions;
                            sbError.Append(Record.PleaseFalseTrueIsEmoji);
                            sbError.Append(" ");
                            sbError.Append(Record.PleaseFalseTrueIsSubjective);
                            valid = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption1(string headerText, string opton1, string isImoji, string IsSubjective)
            {

                bool valid = false;
                //UserId
                try
                {
                    if (opton1 != null && !string.IsNullOrEmpty(opton1))
                    {
                        feedbackQuestionRejected.Option1 = opton1;
                    }
                    else
                    {
                        if ((isImoji.ToLower()) == Record.False || (isImoji.ToLower()) == Record.True || (IsSubjective.ToLower()) == Record.False || (IsSubjective.ToLower()) == Record.True)
                        {
                            if (Convert.ToBoolean(isImoji) == false)
                            {
                                if (Convert.ToBoolean(IsSubjective) == false)
                                {
                                    sbError.Append(headerText + Record.EmptyMessage);
                                    valid = true;
                                }
                            }
                        }
                        else
                        {
                            feedbackQuestionRejected.IsEmoji = isImoji;
                            sbError.Append(Record.PleaseFalseTrueIsEmoji);
                            sbError.Append(" ");
                            sbError.Append(Record.PleaseFalseTrueIsSubjective);
                            valid = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption2(string headerText, string opton2, string isImoji, string IsSubjective)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (opton2 != null && !string.IsNullOrEmpty(opton2))
                    {
                        feedbackQuestionRejected.Option2 = opton2;
                    }
                    else
                    {
                        if ((isImoji.ToLower()) == Record.False || (isImoji.ToLower()) == Record.True || (IsSubjective.ToLower()) == Record.False || (IsSubjective.ToLower()) == Record.True)
                        {
                            if (Convert.ToBoolean(isImoji) == false)
                            {
                                if (Convert.ToBoolean(IsSubjective) == false)
                                {
                                    sbError.Append(headerText + Record.EmptyMessage);
                                    valid = true;
                                }
                            }
                        }
                        else
                        {
                            feedbackQuestionRejected.IsEmoji = isImoji;
                            sbError.Append(Record.PleaseFalseTrueIsEmoji);
                            sbError.Append(" ");
                            sbError.Append(Record.PleaseFalseTrueIsSubjective);
                            valid = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption3(string headerText, string option3)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option3 != null && !string.IsNullOrEmpty(option3))
                    {
                        feedbackQuestionRejected.Option3 = option3;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption4(string headerText, string option4)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option4 != null && !string.IsNullOrEmpty(option4))
                    {
                        feedbackQuestionRejected.Option4 = option4;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public static bool ValidateOption5(string headerText, string option5)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option5 != null && !string.IsNullOrEmpty(option5))
                    {
                        feedbackQuestionRejected.Option5 = option5;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption6(string headerText, string option6)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option6 != null && !string.IsNullOrEmpty(option6))
                    {
                        feedbackQuestionRejected.Option6 = option6;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption7(string headerText, string option7)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option7 != null && !string.IsNullOrEmpty(option7))
                    {
                        feedbackQuestionRejected.Option7 = option7;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption8(string headerText, string option8)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option8 != null && !string.IsNullOrEmpty(option8))
                    {
                        feedbackQuestionRejected.Option8 = option8;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption9(string headerText, string option9)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option9 != null && !string.IsNullOrEmpty(option9))
                    {
                        feedbackQuestionRejected.Option9 = option9;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateOption10(string headerText, string option10)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option10 != null && !string.IsNullOrEmpty(option10))
                    {
                        feedbackQuestionRejected.Option10 = option10;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateCourseCode(string headerText, string coursecode)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (coursecode != null && !string.IsNullOrEmpty(coursecode))
                    {
                        feedbackQuestionRejected.CourseCode = coursecode;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

        }
    }
}
