using Assessment.API.APIModel;
using Assessment.API.Models;
using Assessment.API.Repositories.Interface;
using OfficeOpenXml;
using System.Text;
using log4net;
using Feedback.API.APIModel;
using System.Text.RegularExpressions;
using Assessment.API.Helper;

namespace Assessment.API.Common
{
    public class AssessmentsQuestionImport
    {
        public static class ProcessFile
        {

            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));
            static StringBuilder sb = new StringBuilder();
            static string[] header = { };
            static string[] headerStar = { };
            static string[] headerWithoutStar = { };
            static List<string> questionBankRecord = new List<string>();
            static AssessmentQuestion assessmentQuestionBank = new AssessmentQuestion();
            static AssessmentQuestionRejected assessmentQuestionBankRejected = new AssessmentQuestionRejected();
            static StringBuilder sbError = new StringBuilder();
            static int totalRecordInsert = 0;
            static int totalRecordRejected = 0;

            public static void Reset()
            {
                sb.Clear();
                header = new string[0];
                headerStar = new string[0];
                headerWithoutStar = new string[0];
                questionBankRecord.Clear();
                assessmentQuestionBank = new AssessmentQuestion();
                assessmentQuestionBankRejected = new AssessmentQuestionRejected();
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
                        bool isValidRecord = true;
                        for (int col = 1; col <= ColCount; col++)
                        {
                            string append = "";
                            if (worksheet.Cells[row, col].Value == null)
                            {
                                if (col == 1)
                                {
                                    //check all columns empty
                                    int emptyColumns = 0;
                                    for (int i = 1; i <= ColCount; i++)
                                    {
                                        if (worksheet.Cells[row, i].Value == null)
                                            emptyColumns++;
                                    }
                                    if (emptyColumns == ColCount)
                                    {
                                        isValidRecord = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                append = Convert.ToString(worksheet.Cells[row, col].Value.ToString().Trim());
                                //replacing \t with space
                                append = Regex.Replace(append, @"\t", " ");
                            }
                            string finalAppend = append + "\t";
                            sb.Append(finalAppend);
                        }
                        if (isValidRecord)
                            sb.Append(Environment.NewLine);

                    }

                    string fileInfo = sb.ToString();
                    // To implement multiline in questionText. Got all records in questionbankrecord and removed record which contains \t using forloop
                    questionBankRecord = new List<string>(fileInfo.Split("\r\n"));
                    int Count = questionBankRecord.Count - 1;
                    for (int i = Count; i > 0; i--)
                    {
                        if (questionBankRecord[i].StartsWith("\t"))
                            questionBankRecord.Remove(questionBankRecord[i]);
                    }
                    foreach (string record in questionBankRecord)
                    {
                        string[] mainsp = record.Split('\r');
                        string[] mainsp2 = mainsp[0].Split('\"');
                        header = mainsp2[0].Split('\t');
                        headerStar = mainsp2[0].Split('\t');
                        break;
                    }
                    questionBankRecord.RemoveAt(0);
                    for (int i = 0; i < questionBankRecord.Count - 1; i++)
                    {
                        if (questionBankRecord[i].StartsWith("\t"))
                            questionBankRecord.Remove(questionBankRecord[i]);
                    }

                }
                /////Remove Star from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("*", "");

                }

                // invalid file
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
                //return result;
            }

            public static async Task<string> ProcessRecordsAsync(IAssessmentQuestion _assessmentQuestion, IAssessmentQuestionRejectedRepository _assessmentQuestionBankRejectedRepository, IAsessmentQuestionOption _asessmentQuestionOption, int userid, string OrganisationCode)
            {


                if (questionBankRecord != null && questionBankRecord.Count > 0)
                {
                    foreach (string record in questionBankRecord)
                    {

                        int countLenght = record.Length;
                        if (record != null && countLenght > 1)
                        {
                            string[] textpart = record.Split('\t');

                            string[][] mainRecord = { header, textpart };
                            string txtSection = string.Empty;
                            string txtQuestionType = string.Empty;
                            string txtMetadata = string.Empty;
                            string txtDifficultyLevel = string.Empty;
                            string txtMarks = string.Empty;
                            string txtAnswerOptions = string.Empty;
                            string txtModelAnswer = string.Empty;
                            string txtAnswerOption1 = string.Empty;
                            string txtAnswerOption2 = string.Empty;
                            string txtAnswerOption3 = string.Empty;
                            string txtAnswerOption4 = string.Empty;
                            string txtAnswerOption5 = string.Empty;
                            string txtCorrectAnswer1 = string.Empty;
                            string txtCorrectAnswer2 = string.Empty;
                            string txtCorrectAnswer3 = string.Empty;
                            string txtCorrectAnswer4 = string.Empty;
                            string txtCorrectAnswer5 = string.Empty;
                            string txtTimeInMinutes = string.Empty;
                            string txtQuestionText = string.Empty;
                            string txtOptionType = string.Empty;
                            string txtStatus = string.Empty;
                            string txtRandomize = string.Empty;
                            bool isCorrectAnswer1 = false;
                            bool isCorrectAnswer2 = false;
                            bool isCorrectAnswer3 = false;
                            bool isCorrectAnswer4 = false;
                            bool isCorrectAnswer5 = false;

                            string txtCourseCode = string.Empty;

                            string headerText = "";

                            int arrasize = header.Count();

                            for (int j = 0; j < arrasize - 1; j++)
                            {
                                headerText = header[j];
                                string[] mainspilt = headerText.Split('\t');

                                headerText = mainspilt[0];

                                if (headerText == "Metadata")
                                {
                                    //EmailId
                                    try
                                    {

                                        string Metadata = mainRecord[1][j];
                                        string[] textMetadatasplit = mainRecord[1][j].Split('\t');
                                        txtMetadata = textMetadatasplit[0];
                                        txtMetadata = txtMetadata.Trim();

                                        bool valid = ValidateMetadata(headerText, txtMetadata);
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
                   if (headerText == "DifficultyLevel")
                                {
                                    //UserName
                                    try
                                    {
                                        string difficultylevel = mainRecord[1][j];
                                        string[] textdifficultylevelsplit = mainRecord[1][j].Split('\t');
                                        string DifficultyLevel = textdifficultylevelsplit[0];
                                        txtDifficultyLevel = DifficultyLevel.Trim();

                                        bool valid = ValidateDifficultyLevel(headerText, txtDifficultyLevel);
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
                   if (headerText == "AnswerOptions")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string textAnswerOptions = textanswerOptionssplit[0];
                                        txtAnswerOptions = textAnswerOptions.Trim();


                                        bool valid = ValidateAnswerOptions(headerText, txtAnswerOptions);
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
                   if (headerText == "Marks")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string Marks = mainRecord[1][j];
                                        string[] textMarkssplit = mainRecord[1][j].Split('\t');
                                        string textMarks = textMarkssplit[0];
                                        txtMarks = textMarks.Trim();

                                        bool valid = ValidateMarks(headerText, txtMarks);
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
                   if (headerText == "QuestionText")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string QuestionText = mainRecord[1][j];
                                        string[] textQuestionTextsplit = mainRecord[1][j].Split('\t');
                                        string textQuestionText = textQuestionTextsplit[0];
                                        txtQuestionText = textQuestionText.Trim();

                                        bool valid = ValidateQuestionText(headerText, txtQuestionText);
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

                   if (headerText == "AnswerOption1")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions1 = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string textAnswerOption1 = textanswerOptionssplit[0];
                                        txtAnswerOption1 = textAnswerOption1.Trim();


                                        bool valid = ValidateAnswerOption1(headerText, txtAnswerOption1);
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
                   if (headerText == "AnswerOption2")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions2 = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string texttxtAnswerOption2 = textanswerOptionssplit[0];
                                        txtAnswerOption2 = texttxtAnswerOption2.Trim();


                                        bool valid = ValidateAnswerOption2(headerText, txtAnswerOption2);
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
                   if (headerText == "AnswerOption3")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions3 = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string textAnswerOption3 = textanswerOptionssplit[0];
                                        txtAnswerOption3 = textAnswerOption3.Trim();

                                        bool valid = ValidateAnswerOption3(headerText, txtAnswerOption3);
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
                   if (headerText == "AnswerOption4")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions4 = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string textAnswerOption4 = textanswerOptionssplit[0];
                                        txtAnswerOption4 = textAnswerOption4.Trim();

                                        bool valid = ValidateAnswerOption4(headerText, txtAnswerOption4);
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
                   if (headerText == "AnswerOption5")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string answerOptions5 = mainRecord[1][j];
                                        string[] textanswerOptionssplit = mainRecord[1][j].Split('\t');
                                        string textAnswerOption5 = textanswerOptionssplit[0];
                                        txtAnswerOption5 = textAnswerOption5.Trim();


                                        bool valid = ValidateAnswerOption5(headerText, txtAnswerOption5);
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
                   if (headerText == "CorrectAnswer1")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string CorrectAnswer1 = mainRecord[1][j];
                                        string[] txtCorrectAnswersplit1 = mainRecord[1][j].Split('\t');
                                        string textCorrectAnswer1 = txtCorrectAnswersplit1[0];
                                        txtCorrectAnswer1 = textCorrectAnswer1.Trim();


                                        bool valid = ValidateCorrectAnswer(headerText, txtCorrectAnswer1, 1);
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
                   if (headerText == "CorrectAnswer2")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string CorrectAnswer2 = mainRecord[1][j];
                                        string[] txtCorrectAnswersplit2 = mainRecord[1][j].Split('\t');
                                        string textCorrectAnswer2 = txtCorrectAnswersplit2[0];
                                        txtCorrectAnswer2 = textCorrectAnswer2.Trim();


                                        bool valid = ValidateCorrectAnswer(headerText, txtCorrectAnswer2, 2);
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
                   if (headerText == "CorrectAnswer3")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string CorrectAnswer3 = mainRecord[1][j];
                                        string[] txtCorrectAnswersplit3 = mainRecord[1][j].Split('\t');
                                        string textCorrectAnswer3 = txtCorrectAnswersplit3[0];
                                        txtCorrectAnswer3 = textCorrectAnswer3.Trim();


                                        bool valid = ValidateCorrectAnswer(headerText, txtCorrectAnswer3, 3);
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
                   if (headerText == "CorrectAnswer4")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string CorrectAnswer4 = mainRecord[1][j];
                                        string[] txtCorrectAnswersplit4 = mainRecord[1][j].Split('\t');
                                        string textCorrectAnswer4 = txtCorrectAnswersplit4[0];
                                        txtCorrectAnswer4 = textCorrectAnswer4.Trim();


                                        bool valid = ValidateCorrectAnswer(headerText, txtCorrectAnswer4, 4);
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
                   if (headerText == "CorrectAnswer5")
                                {
                                    //MobileNumber
                                    try
                                    {

                                        string CorrectAnswer5 = mainRecord[1][j];
                                        string[] txtCorrectAnswersplit5 = mainRecord[1][j].Split('\t');
                                        string textCorrectAnswer5 = txtCorrectAnswersplit5[0];
                                        txtCorrectAnswer5 = textCorrectAnswer5.Trim();


                                        bool valid = ValidateCorrectAnswer(headerText, txtCorrectAnswer5, 5);
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
                   if (headerText == "CourseCode")
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
                                                APIFQCourse coursedata = await _assessmentQuestion.courseCodeExists(txtCourseCode);
                                                if (coursedata == null)
                                                {
                                                    sbError.Append(Record.CourseCodeNotExists);
                                                    break;
                                                }
                                                else
                                                {
                                                    assessmentQuestionBank.CourseId = coursedata.CourseId;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                            }
                            int NosOptions = 0;
                            {
                                for (int j = 0; j < 1; j++)
                                {
                                    if (sbError.ToString() == "" || string.IsNullOrEmpty(sbError.ToString()))
                                    {
                                        if ((Convert.ToInt32(txtAnswerOptions) > 5))
                                        {
                                            sbError.Append("Option and Option value does not match");
                                            break;
                                        }

                                        string[] CorrectAnswers = { txtCorrectAnswer1, txtCorrectAnswer2, txtCorrectAnswer3, txtCorrectAnswer4, txtCorrectAnswer5 };
                                        StringBuilder sb = new StringBuilder();
                                        StringBuilder sb1 = new StringBuilder();
                                        for (int i = 0; i < CorrectAnswers.Length - 1; i++)
                                        {
                                            if (string.IsNullOrEmpty(CorrectAnswers[i]) && !string.IsNullOrEmpty(CorrectAnswers[i + 1]))
                                            {
                                                if (sb.ToString().Equals(string.Empty))
                                                    sb.Append($"CorrectAnswer{i + 1}");
                                                else
                                                    sb.Append(", " + $"CorrectAnswer{i + 1}");
                                                //sbError.Append($"{sb.ToString()} cannot be blank.");
                                                //break;
                                                sb1.Append(sb.ToString());
                                            }
                                            else if (string.IsNullOrEmpty(CorrectAnswers[i]) && string.IsNullOrEmpty(CorrectAnswers[i + 1]))
                                            {
                                                if (sb.ToString().Equals(string.Empty))
                                                    sb.Append($"CorrectAnswer{i + 1}");
                                                else
                                                    sb.Append(", " + $"CorrectAnswer{i + 1}");
                                            }
                                        }
                                        if (sb1.ToString() != "")
                                        {
                                            sbError.Append($"{sb1.ToString()} cannot be blank.");
                                            break;
                                        }


                                        string[] NonEmptyCorrectAnswers = CorrectAnswers.AsEnumerable().Where(x => !string.IsNullOrEmpty(x)).ToArray();

                                        if (NonEmptyCorrectAnswers.Length == 0)
                                        {
                                            sbError.Append("No Correct Answer Option provided.");
                                            break;
                                        }

                                        NosOptions = NonEmptyCorrectAnswers.Length;

                                        string[] QuestionOptions = { txtAnswerOption1, txtAnswerOption2, txtAnswerOption3, txtAnswerOption4, txtAnswerOption5 };
                                        string[] NonEmptyQuestionOptions = QuestionOptions.AsEnumerable().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                                        bool IsInValidCorrectAnswerOption = false;
                                        for (int i = 0; i < NonEmptyCorrectAnswers.Length; i++)
                                        {
                                            if (!NonEmptyQuestionOptions.Contains(NonEmptyCorrectAnswers[i]))
                                                IsInValidCorrectAnswerOption = true;
                                        }
                                        if (IsInValidCorrectAnswerOption)
                                        {
                                            sbError.Append("Invalid Correct Answer Option provided.");
                                            break;
                                        }

                                        bool IsDuplicateOptions = NonEmptyQuestionOptions.GroupBy(x => x).Any(x => x.Count() > 1);
                                        if (IsDuplicateOptions)
                                        {
                                            sbError.Append("Duplicate Answer Options provided.");
                                            break;
                                        }

                                        bool IsDuplicateAnswerOptions = NonEmptyCorrectAnswers.GroupBy(x => x).Any(x => x.Count() > 1);
                                        if (IsDuplicateAnswerOptions)
                                        {
                                            sbError.Append("Duplicate Correct Answers provided.");
                                            break;
                                        }

                                        if (NonEmptyQuestionOptions.Length < 2)
                                        {
                                            sbError.Append("Minimum 2 Answer Options required.");
                                            break;
                                        }
                                        try
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) != NonEmptyQuestionOptions.Length)
                                            {
                                                sbError.Append("Answer Options value & provided Answer Options not matched.");
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.Error(Utilities.GetDetailedException(ex));
                                        }


                                        //option5
                                        if ((Convert.ToInt32(txtAnswerOptions) == 5))
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) == 5 && ((txtAnswerOption1 != null && !string.IsNullOrEmpty(txtAnswerOption1)) && (txtAnswerOption2 != null && !string.IsNullOrEmpty(txtAnswerOption2)) && (txtAnswerOption3 != null && !string.IsNullOrEmpty(txtAnswerOption3)) && (txtAnswerOption4 != null && !string.IsNullOrEmpty(txtAnswerOption4)) && (txtAnswerOption5 != null && !string.IsNullOrEmpty(txtAnswerOption5))))
                                            {
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption1)))
                                                {
                                                    isCorrectAnswer1 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption2)))
                                                {
                                                    isCorrectAnswer2 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption3)))
                                                {
                                                    isCorrectAnswer3 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption4)))
                                                {
                                                    isCorrectAnswer4 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption5)))
                                                {
                                                    isCorrectAnswer5 = true;
                                                }
                                                if (isCorrectAnswer1 == false && isCorrectAnswer2 == false && isCorrectAnswer3 == false && isCorrectAnswer4 == false && isCorrectAnswer5 == false)
                                                {
                                                    sbError.Append("Options and CorrectAnswers value does not match");
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                sbError.Append("Option and Option value does not match");
                                                break;
                                            }
                                        }
                                        //option4
                                        if ((Convert.ToInt32(txtAnswerOptions) == 4))
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) == 4 && ((txtAnswerOption1 != null && !string.IsNullOrEmpty(txtAnswerOption1)) && (txtAnswerOption2 != null && !string.IsNullOrEmpty(txtAnswerOption2)) && (txtAnswerOption3 != null && !string.IsNullOrEmpty(txtAnswerOption3)) && (txtAnswerOption4 != null && !string.IsNullOrEmpty(txtAnswerOption4))))
                                            {
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption1)))
                                                {
                                                    isCorrectAnswer1 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption2)))
                                                {
                                                    isCorrectAnswer2 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption3)))
                                                {
                                                    isCorrectAnswer3 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption4)))
                                                {
                                                    isCorrectAnswer4 = true;
                                                }
                                                if (isCorrectAnswer1 == false && isCorrectAnswer2 == false && isCorrectAnswer3 == false && isCorrectAnswer4 == false)
                                                {
                                                    sbError.Append("Options and CorrectAnswers value does not match");
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                sbError.Append("Option and Option value does not match");
                                                break;
                                            }
                                        }

                                        //option3
                                        if ((Convert.ToInt32(txtAnswerOptions) == 3))
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) == 3 && ((txtAnswerOption1 != null && !string.IsNullOrEmpty(txtAnswerOption1)) && (txtAnswerOption2 != null && !string.IsNullOrEmpty(txtAnswerOption2)) && (txtAnswerOption3 != null && !string.IsNullOrEmpty(txtAnswerOption3))))
                                            {
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption1)))
                                                {
                                                    isCorrectAnswer1 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption2)))
                                                {
                                                    isCorrectAnswer2 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption3)))
                                                {
                                                    isCorrectAnswer3 = true;
                                                }
                                                if (isCorrectAnswer1 == false && isCorrectAnswer2 == false && isCorrectAnswer3 == false)
                                                {
                                                    sbError.Append("Options and CorrectAnswers value does not match");
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                sbError.Append("Option and Option value does not match");
                                                break;
                                            }
                                        }

                                        //option2
                                        if ((Convert.ToInt32(txtAnswerOptions) == 2))
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) == 2 && ((txtAnswerOption1 != null && !string.IsNullOrEmpty(txtAnswerOption1)) && (txtAnswerOption2 != null && !string.IsNullOrEmpty(txtAnswerOption2))))
                                            {
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption1)))
                                                {
                                                    isCorrectAnswer1 = true;
                                                }
                                                if (NonEmptyCorrectAnswers.Contains(Convert.ToString(txtAnswerOption2)))
                                                {
                                                    isCorrectAnswer2 = true;
                                                }
                                                if (isCorrectAnswer1 == false && isCorrectAnswer2 == false)
                                                {
                                                    sbError.Append("Options and CorrectAnswer value does not match");
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                sbError.Append("Option and Option value does not match");
                                                break;
                                            }
                                        }

                                        //option1
                                        if ((Convert.ToInt32(txtAnswerOptions) <= 1))
                                        {
                                            if (Convert.ToInt32(txtAnswerOptions) <= 1 && ((txtAnswerOption1 != null && !string.IsNullOrEmpty(txtAnswerOption1))))
                                            {
                                                sbError.Append("Option and Option value does not match");
                                                break;
                                            }
                                            else
                                            {
                                                sbError.Append("Options and CorrectAnswer value does not match");
                                                break;
                                            }
                                        }


                                        APIAssessmentQuestion objAPIAssessmentQuestion = new APIAssessmentQuestion();
                                        objAPIAssessmentQuestion.QuestionText = assessmentQuestionBank.QuestionText;
                                        objAPIAssessmentQuestion.Metadata = assessmentQuestionBank.Metadata;

                                        AssessmentOptions[] options = new AssessmentOptions[(Convert.ToInt32(txtAnswerOptions))];

                                        AssessmentOptions objAssessmentOptions1 = new AssessmentOptions();
                                        objAssessmentOptions1.OptionText = txtAnswerOption1;
                                        options[0] = objAssessmentOptions1;

                                        AssessmentOptions objAssessmentOptions2 = new AssessmentOptions();
                                        objAssessmentOptions2.OptionText = txtAnswerOption2;
                                        options[1] = objAssessmentOptions2;

                                        if ((Convert.ToInt32(txtAnswerOptions) >= 3))
                                        {
                                            AssessmentOptions objAssessmentOptions3 = new AssessmentOptions();
                                            objAssessmentOptions3.OptionText = txtAnswerOption3;
                                            options[2] = objAssessmentOptions3;
                                        }

                                        if ((Convert.ToInt32(txtAnswerOptions) >= 4))
                                        {
                                            AssessmentOptions objAssessmentOptions4 = new AssessmentOptions();
                                            objAssessmentOptions4.OptionText = txtAnswerOption4;
                                            options[3] = objAssessmentOptions4;
                                        }
                                        if ((Convert.ToInt32(txtAnswerOptions) >= 5))
                                        {

                                            AssessmentOptions objAssessmentOptions5 = new AssessmentOptions();
                                            objAssessmentOptions5.OptionText = txtAnswerOption5;
                                            options[4] = objAssessmentOptions5;
                                        }

                                        objAPIAssessmentQuestion.aPIassessmentOptions = options;

                                        bool question = await _assessmentQuestion.ExistQuestionOption(objAPIAssessmentQuestion);
                                        if (question == true)
                                        {
                                            sbError.Append("Duplicate Question ");
                                            break;
                                        }

                                    }
                                }
                            }



                            bool validvalue = false;

                            if (validvalue == true)
                            {
                                sbError.Append("  Please enter valid data,");

                                assessmentQuestionBankRejected.ErrorMessage = sbError.ToString();
                                assessmentQuestionBankRejected.QuestionType = "Text";
                                assessmentQuestionBankRejected.CreatedDate = DateTime.UtcNow;
                                assessmentQuestionBankRejected.CreatedBy = userid;
                                assessmentQuestionBankRejected.ModifiedDate = DateTime.UtcNow;
                                assessmentQuestionBankRejected.ModifiedBy = userid;
                                assessmentQuestionBankRejected.ErrorMessage = sbError.ToString();
                                assessmentQuestionBankRejected.Section = sbError.ToString();
                                await _assessmentQuestionBankRejectedRepository.Add(assessmentQuestionBankRejected);
                                totalRecordRejected++;
                                assessmentQuestionBankRejected = new AssessmentQuestionRejected();
                                assessmentQuestionBank = new AssessmentQuestion();
                                sbError.Clear();

                            }
                            else
                            {
                                assessmentQuestionBankRejected.ErrorMessage = sbError.ToString();
                                assessmentQuestionBankRejected.QuestionType = "Text";
                                if (sbError.ToString() != "")
                                {
                                    assessmentQuestionBankRejected.CreatedDate = DateTime.UtcNow;
                                    assessmentQuestionBankRejected.CreatedBy = userid;
                                    assessmentQuestionBankRejected.ModifiedDate = DateTime.UtcNow;
                                    assessmentQuestionBankRejected.ModifiedBy = userid;
                                    assessmentQuestionBankRejected.ErrorMessage = sbError.ToString();
                                    assessmentQuestionBankRejected.Section = sbError.ToString();
                                    await _assessmentQuestionBankRejectedRepository.Add(assessmentQuestionBankRejected);
                                    totalRecordRejected++;
                                    assessmentQuestionBankRejected = new AssessmentQuestionRejected();
                                    assessmentQuestionBank = new AssessmentQuestion();
                                    sbError.Clear();
                                }
                                else
                                {
                                    //add Record 

                                    assessmentQuestionBank.CreatedDate = DateTime.UtcNow;
                                    assessmentQuestionBank.ModifiedDate = DateTime.UtcNow;
                                    assessmentQuestionBank.CreatedBy = userid;
                                    assessmentQuestionBank.Status = true;
                                    assessmentQuestionBank.ModifiedBy = userid;
                                    assessmentQuestionBank.IsDeleted = false;
                                    assessmentQuestionBank.Section = Record.Objective;
                                    if (NosOptions > 1)
                                        assessmentQuestionBank.OptionType = Record.MultipleSelection;
                                    else
                                        assessmentQuestionBank.OptionType = Record.SingleSelection;
                                    //for Import default contentType will be Objective
                                    assessmentQuestionBank.ContentType = "Objective";
                                    await _assessmentQuestion.Add(assessmentQuestionBank);
                                    if (assessmentQuestionBank != null || assessmentQuestionBank.Id != 0)
                                    {
                                        if (txtAnswerOption1 != null && Convert.ToInt32(txtAnswerOptions) >= 1)
                                        {
                                            AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                            assessmentOptiones.OptionText = txtAnswerOption1;
                                            assessmentOptiones.IsCorrectAnswer = isCorrectAnswer1;
                                            assessmentOptiones.QuestionID = assessmentQuestionBank.Id;
                                            assessmentOptiones.CreatedBy = userid;
                                            assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedBy = userid;
                                            await _asessmentQuestionOption.Add(assessmentOptiones);

                                        }


                                        if (txtAnswerOption2 != null && Convert.ToInt32(txtAnswerOptions) >= 2)
                                        {
                                            AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                            assessmentOptiones.OptionText = txtAnswerOption2;
                                            assessmentOptiones.IsCorrectAnswer = isCorrectAnswer2;
                                            assessmentOptiones.QuestionID = assessmentQuestionBank.Id;
                                            assessmentOptiones.CreatedBy = userid;
                                            assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedBy = userid;
                                            await _asessmentQuestionOption.Add(assessmentOptiones);

                                        }


                                        if (txtAnswerOption3 != null && Convert.ToInt32(txtAnswerOptions) >= 3)
                                        {
                                            AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                            assessmentOptiones.OptionText = txtAnswerOption3;
                                            assessmentOptiones.IsCorrectAnswer = isCorrectAnswer3;
                                            assessmentOptiones.QuestionID = assessmentQuestionBank.Id;
                                            assessmentOptiones.CreatedBy = userid;
                                            assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedBy = userid;
                                            await _asessmentQuestionOption.Add(assessmentOptiones);

                                        }

                                        if (txtAnswerOption4 != null && Convert.ToInt32(txtAnswerOptions) >= 4)
                                        {
                                            AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                            assessmentOptiones.OptionText = txtAnswerOption4;
                                            assessmentOptiones.IsCorrectAnswer = isCorrectAnswer4;
                                            assessmentOptiones.QuestionID = assessmentQuestionBank.Id;
                                            assessmentOptiones.CreatedBy = userid;
                                            assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedBy = userid;
                                            await _asessmentQuestionOption.Add(assessmentOptiones);

                                        }


                                        if (txtAnswerOption5 != null && Convert.ToInt32(txtAnswerOptions) >= 5)
                                        {
                                            AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                            assessmentOptiones.OptionText = txtAnswerOption5;
                                            assessmentOptiones.IsCorrectAnswer = isCorrectAnswer5;
                                            assessmentOptiones.QuestionID = assessmentQuestionBank.Id;
                                            assessmentOptiones.CreatedBy = userid;
                                            assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                            assessmentOptiones.ModifiedBy = userid;
                                            await _asessmentQuestionOption.Add(assessmentOptiones);

                                        }
                                    }
                                    totalRecordInsert++;
                                    assessmentQuestionBankRejected = new AssessmentQuestionRejected();
                                    assessmentQuestionBank = new AssessmentQuestion();
                                    sbError.Clear();
                                }

                            }
                        }
                        else
                        {
                        }

                    }
                }
                return "Total number of record inserted :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;

            }
            public static bool ValidateQuestionType(string headerText, string questionType)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (questionType != null && !string.IsNullOrEmpty(questionType))
                    {
                        assessmentQuestionBank.QuestionType = questionType;
                        assessmentQuestionBankRejected.QuestionType = questionType;
                    }

                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public static bool ValidateMetadata(string headerText, string metadata)
            {
                bool valid = false;

                //UserId
                try
                {
                    if (metadata != null && !string.IsNullOrEmpty(metadata))
                    {
                        if (metadata.Trim().Length >= 3)
                        {
                            assessmentQuestionBank.Metadata = metadata;
                            assessmentQuestionBankRejected.Metadata = metadata;
                        }
                        else
                        {
                            assessmentQuestionBank.Metadata = metadata;
                            assessmentQuestionBankRejected.Metadata = metadata;
                            sbError.Append("Please enter valid Metdata");
                            valid = true;
                        }
                    }
                    else
                    {
                        assessmentQuestionBank.Metadata = metadata;
                        assessmentQuestionBankRejected.Metadata = metadata;
                        sbError.Append("Metadata is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateDifficultyLevel(string headerText, string difficultyLevel)
            {
                bool valid = false;

                //UserId
                try
                {
                    if (difficultyLevel != null && !string.IsNullOrEmpty(difficultyLevel))
                    {
                        if ((difficultyLevel.ToLower()) == Record.Simple || (difficultyLevel.ToLower()) == Record.Difficult || (difficultyLevel.ToLower()) == Record.Tough)
                        {
                            assessmentQuestionBank.DifficultyLevel = difficultyLevel;
                            assessmentQuestionBankRejected.DifficultyLevel = difficultyLevel;
                        }
                        else
                        {
                            assessmentQuestionBank.DifficultyLevel = "Simple";
                            assessmentQuestionBankRejected.DifficultyLevel = "Simple";
                        }
                    }
                    else
                    {
                        assessmentQuestionBank.DifficultyLevel = "Simple";
                        assessmentQuestionBankRejected.DifficultyLevel = "Simple";
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateAnswerOptions(string headerText, string answerOptions)
            {

                bool valid = false;
                //UserId
                try
                {
                    if (answerOptions != null && !string.IsNullOrEmpty(answerOptions))
                    {
                        assessmentQuestionBankRejected.AnswerOptions = answerOptions;
                    }
                    else
                    {
                        sbError.Append("AnswerOptions is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateMarks(string headerText, string marks)
            {

                bool valid = false;
                //UserId
                try
                {
                    if (marks != null && !string.IsNullOrEmpty(marks))
                    {
                        int mark = Convert.ToInt32(marks);
                        if (mark > 25 || mark < 0)
                        {
                            sbError.Append("Enter Valid Marks. ");
                        }

                        else
                        {
                            assessmentQuestionBank.Marks = Convert.ToInt32(marks);
                            assessmentQuestionBankRejected.Marks = marks;
                        }
                    }
                    else
                    {
                        sbError.Append("Marks is null ");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateQuestionText(string headerText, string questionText)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (questionText.Length > 1000)
                    {
                        int Maxlenght = 1800;
                        if (questionText.Length >= Maxlenght)
                        {
                            questionText = questionText.Substring(0, Maxlenght);
                        }
                        sbError.Append("Question text must not be more than 1000");
                        valid = true;
                    }
                    if (questionText != null && !string.IsNullOrEmpty(questionText))
                    {
                        assessmentQuestionBank.QuestionText = questionText;
                        assessmentQuestionBankRejected.QuestionText = questionText;
                    }
                    else
                    {
                        sbError.Append("QuestionText is Wrong");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateAnswerOption1(string headerText, string option1)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option1 != null && !string.IsNullOrEmpty(option1))
                    {
                        assessmentQuestionBankRejected.AnswerOption1 = option1;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateAnswerOption2(string headerText, string option2)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option2 != null && !string.IsNullOrEmpty(option2))
                    {
                        assessmentQuestionBankRejected.AnswerOption2 = option2;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public static bool ValidateAnswerOption3(string headerText, string option3)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option3 != null && !string.IsNullOrEmpty(option3))
                    {
                        assessmentQuestionBankRejected.AnswerOption3 = option3;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public static bool ValidateAnswerOption4(string headerText, string option4)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option4 != null && !string.IsNullOrEmpty(option4))
                    {
                        assessmentQuestionBankRejected.AnswerOption4 = option4;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateAnswerOption5(string headerText, string option5)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option5 != null && !string.IsNullOrEmpty(option5))
                    {
                        assessmentQuestionBankRejected.AnswerOption5 = option5;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateCorrectAnswer(string headerText, string correctAnswer, int sequence)
            {
                bool valid = false;
                try
                {
                    if (correctAnswer != null && !string.IsNullOrEmpty(correctAnswer))
                    {
                        switch (sequence)
                        {
                            case 1:
                                assessmentQuestionBankRejected.CorrectAnswer1 = correctAnswer;
                                break;
                            case 2:
                                assessmentQuestionBankRejected.CorrectAnswer2 = correctAnswer;
                                break;
                            case 3:
                                assessmentQuestionBankRejected.CorrectAnswer3 = correctAnswer;
                                break;
                            case 4:
                                assessmentQuestionBankRejected.CorrectAnswer4 = correctAnswer;
                                break;
                            case 5:
                                assessmentQuestionBankRejected.CorrectAnswer5 = correctAnswer;
                                break;
                        }
                    }
                    //else
                    //{
                    //    assessmentQuestionBankRejected.CorrectAnswer = correctAnswer;
                    //    sbError.Append("CorrectAnswer is null,");
                    //    valid = true;
                    //}
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
                        assessmentQuestionBankRejected.CourseCode = coursecode;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
            public static bool ValidateOptionType(string headerText, string optinType)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (optinType != null && !string.IsNullOrEmpty(optinType))
                    {
                        if ((optinType.ToLower()) == "singleselection")
                        {
                            assessmentQuestionBank.OptionType = optinType;
                        }
                        else
                        {
                            sbError.Append("Please enter 'singleselection' in OptionType ");
                            valid = true;
                        }
                    }
                    else
                    {
                        sbError.Append("OptionType is null,");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateStatus(string headerText, string status)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (status != null && !string.IsNullOrEmpty(status))
                    {
                        if ((status.ToLower()) == "false" || (status.ToLower()) == "true")
                        {
                            assessmentQuestionBank.Status = Convert.ToBoolean(status);
                            assessmentQuestionBankRejected.Status = status;
                        }
                        else
                        {
                            sbError.Append("Please enter False or True for Status");
                            valid = true;
                        }
                    }
                    else
                    {
                        sbError.Append("Status is empty,");
                        valid = true;
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
