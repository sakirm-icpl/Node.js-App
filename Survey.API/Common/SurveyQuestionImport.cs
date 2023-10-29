using Survey.API.Helper;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survey.API.Common
{
    public class SurveyQuestionImport
    {
        public static class ProcessFile
        {
            private static StringBuilder sb = new StringBuilder();
            private static string[] header = { };
            private static string[] headerStar = { };
            private static string[] headerWithoutStar = { };
            private static List<string> surveyQuestionrecord = new List<string>();
            private static SurveyQuestionRejected surveyquestionrejected = new SurveyQuestionRejected();
            private static SurveyQuestion surveyquestion = new SurveyQuestion();
            private static StringBuilder sbError = new StringBuilder();
            private static int totalRecordInsert = 0;
            private static int totalRecordRejected = 0;

            public static void Reset()
            {
                sb.Clear();
                header = new string[0];
                headerStar = new string[0];
                headerWithoutStar = new string[0];
                surveyQuestionrecord.Clear();
                surveyquestionrejected = new SurveyQuestionRejected();
                surveyquestion = new SurveyQuestion();

                sbError.Clear();
                totalRecordInsert = 0;
                totalRecordRejected = 0;
            }



            public static async Task<int> InitilizeAsync(FileInfo file)
            {
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
                    surveyQuestionrecord = new List<string>(fileInfo.Split('\n'));
                    if (rowCount != surveyQuestionrecord.Count - 1)
                    {
                        sbError.Append("Cannot contain new line characters");
                        {
                            return 2;

                        }
                    }
                    foreach (string record in surveyQuestionrecord)
                    {
                        string[] mainsp = record.Split('\r');
                        string[] mainsp2 = mainsp[0].Split('\"');
                        header = mainsp2[0].Split('\t');
                        headerStar = mainsp2[0].Split('\t');
                        break;
                    }
                    surveyQuestionrecord.RemoveAt(0);


                }
                ////////////////////////////////////////
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
                /////////////////////////////////////


                // invalid file

                //for (int i = 0; i < header.Count() - 1; i++)
                //{
                //    string headerColumn = header[i].ToString();
                //    headerColumn = headerColumn.ToLower();
                //    if ((headerColumn == FedbackImportField.QuestionText.ToLower())    || (headerColumn == FedbackImportField.IsEmoji.ToLower())  || (headerColumn == FedbackImportField.NoOfOptions.ToLower()) || (headerColumn == FedbackImportField.Option1.ToLower()) || (headerColumn == FedbackImportField.Option2.ToLower()) || (headerColumn == FedbackImportField.Option3.ToLower()) || (headerColumn == FedbackImportField.Option4.ToLower()) || (headerColumn == FedbackImportField.Option5.ToLower()))
                //    {

                //    }
                //    else
                //    {
                //        result = true;
                //        return result;
                //    }

                //}

                int count = 0;
                for (int i = 0; i < header.Count() - 1; i++)
                {
                    string headerColumn = header[i].ToString().Trim();
                    if (!string.IsNullOrEmpty(headerColumn))
                    {
                        count++;
                    }

                }
                if (count == 15)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
                //   return result;
            }
            public static async Task<string> ProcessRecordsAsync(FileInfo file, ISurveyManagementRepository surveyManagementRepository, ISurveyQuestionRepository surveyQuestionRepository, ISurveyOptionRepository surveyOptionRepository, ISurveyQuestionRejectedRepository _surveyQuestionRejectedRepository, int UserId)
            {
                try
                {
                    if (surveyQuestionrecord != null && surveyQuestionrecord.Count > 0)
                    {
                        foreach (string record in surveyQuestionrecord)
                        {

                            int countLenght = record.Length;
                            if (record != null && countLenght > 1)
                            {
                                string[] textpart = record.Split('\t');
                                string[][] mainRecord = { header, textpart };
                                string txtQuestion = string.Empty;
                                string txtActiveQuestion = string.Empty;
                                string txtObjectiveQuestion = string.Empty;
                                string txtOptions = string.Empty;
                                string txtEnterOption1 = string.Empty;
                                string txtEnterOption2 = string.Empty;
                                string txtEnterOption3 = string.Empty;
                                string txtEnterOption4 = string.Empty;
                                string txtEnterOption5 = string.Empty;
                                string txtEnterOption6 = string.Empty;
                                string txtEnterOption7 = string.Empty;
                                string txtEnterOption8 = string.Empty;
                                string txtEnterOption9 = string.Empty;
                                string txtEnterOption10 = string.Empty;
                                string txtIsMultipleChoice = string.Empty;
                                string headerText = "";

                                int arrasize = header.Count();

                                for (int j = 0; j < arrasize; j++)
                                {
                                    headerText = header[j];
                                    string[] mainspilt = headerText.Split('\t');

                                    headerText = mainspilt[0];


                                    if (headerText == "Question")
                                    {
                                        //UserID
                                        try
                                        {
                                            string Question = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtQuestion = textuserIDsplit[0];
                                            string QuestionText = txtQuestion.Trim();
                                            surveyquestionrejected.Question = QuestionText;
                                            surveyquestion.Question = QuestionText;
                                            bool valid = ValidateQuestion(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }

                                    if (headerText == "Active Question")
                                    {
                                        //UserID
                                        try
                                        {
                                            string ActiveQuestion = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtQuestion = textuserIDsplit[0];
                                            //txtIsSubjective = textIsSubjectivesplit[0];
                                            string QuestionText = txtQuestion.Trim();
                                            surveyquestionrejected.ActiveQuestion = QuestionText;
                                            if (ActiveQuestion.ToLower() == "active" || ActiveQuestion.ToLower() == "yes" || ActiveQuestion.ToLower() == Record.Zero || ActiveQuestion.ToLower() == Record.One)
                                            {
                                                sbError.Append(Record.EnterTruefalseFileInvalid);

                                                string res = Record.EnterTruefalseFileInvalid;
                                                return res;
                                            }

                                            bool valid = ValidateActiveQuestion(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    
                                    if (headerText == "IsMultipleChoice")
                                    {
                                        //UserID
                                        try
                                        {
                                            if (Convert.ToBoolean(surveyquestionrejected.ObjectiveQuestion) == true)
                                            {
                                                string IsMultipleChoice = mainRecord[1][j];
                                                string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                                IsMultipleChoice = textuserIDsplit[0];
                                                //txtIsSubjective = textIsSubjectivesplit[0];
                                                string txtIsMultiple = IsMultipleChoice.Trim();
                                                surveyquestionrejected.IsMultipleChoice = Convert.ToBoolean(txtIsMultiple);
                                                // surveyquestionrejected.IsMultipleChoice = QuestionText;
                                                if (IsMultipleChoice.ToLower() == "true" || IsMultipleChoice.ToLower() == "false" || IsMultipleChoice == Record.Zero || IsMultipleChoice == Record.One)
                                                {

                                                    bool valid = ValidateIsMultipleChoice(headerText, txtIsMultiple);
                                                    if (valid == true)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    string res = Record.EnterTrueOrFalse;
                                                    return res;
                                                }

                                            }
                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    if (headerText == "Objective Question")
                                    {
                                        //UserID
                                        try
                                        {
                                            string ObjectiveQuestion = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtActiveQuestion = textuserIDsplit[0];
                                            string QuestionText = txtActiveQuestion.Trim();
                                            surveyquestionrejected.ObjectiveQuestion = QuestionText;
                                            if (ObjectiveQuestion.ToLower() == "active" || ObjectiveQuestion.ToLower() == "yes" || ObjectiveQuestion.ToLower() == Record.Zero || ObjectiveQuestion.ToLower() == Record.One)
                                            {
                                                sbError.Append(Record.EnterTruefalseFileInvalid);
                                                string res = Record.EnterTruefalseFileInvalid;
                                                return res;
                                            }
                                            bool valid = ValidateObjectiveQuestion(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "Options")
                                    {
                                        //UserID
                                        try
                                        {
                                           
                                            string Options = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtOptions = textuserIDsplit[0];
                                            string QuestionText = txtOptions.Trim();
                                            surveyquestionrejected.Options = QuestionText;
                                            bool valid = ValidateOptions(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption1")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption1 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption1 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption1.Trim();
                                            surveyquestionrejected.EnterOption1 = QuestionText;
                                            bool valid = ValidateEnterOption1(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption2")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption2 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption2 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption2.Trim();
                                            surveyquestionrejected.EnterOption2 = QuestionText;
                                            bool valid = ValidateEnterOption2(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption3")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption3 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption3 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption3.Trim();
                                            surveyquestionrejected.EnterOption3 = QuestionText;
                                            bool valid = ValidateEnterOption3(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption4")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption4 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption4 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption4.Trim();
                                            surveyquestionrejected.EnterOption4 = QuestionText;
                                            bool valid = ValidateEnterOption4(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption5")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption5 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption5 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption5.Trim();
                                            surveyquestionrejected.EnterOption5 = QuestionText;
                                            bool valid = ValidateEnterOption5(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption6")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption6 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption6 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption6.Trim();
                                            surveyquestionrejected.EnterOption6 = QuestionText;
                                            bool valid = ValidateEnterOption6(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption7")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption7 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption7 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption7.Trim();
                                            surveyquestionrejected.EnterOption7 = QuestionText;
                                            bool valid = ValidateEnterOption7(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption8")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption8 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption8 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption8.Trim();
                                            surveyquestionrejected.EnterOption8 = QuestionText;
                                            bool valid = ValidateEnterOption8(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption9")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption9 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption9 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption9.Trim();
                                            surveyquestionrejected.EnterOption9 = QuestionText;
                                            bool valid = ValidateEnterOption9(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                    else
                                    if (headerText == "EnterOption10")
                                    {
                                        //UserID
                                        try
                                        {
                                            string EnterOption10 = mainRecord[1][j];
                                            string[] textuserIDsplit = mainRecord[1][j].Split('\t');
                                            txtEnterOption10 = textuserIDsplit[0];
                                            string QuestionText = txtEnterOption10.Trim();
                                            surveyquestionrejected.EnterOption10 = QuestionText;
                                            bool valid = ValidateEnterOption10(headerText, QuestionText);
                                            if (valid == true)
                                            {
                                                break;
                                            }

                                        }
                                        catch (Exception)
                                        {
                                            //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                                        }

                                    }
                                }



                                {
                                    for (int j = 0; j < 1; j++)
                                    {
                                        if (sbError.ToString() == "" || string.IsNullOrEmpty(sbError.ToString()))
                                        {
                                            if (Convert.ToBoolean(surveyquestionrejected.ObjectiveQuestion) == true )
                                               
                                            {
                                                //    if (Convert.ToBoolean(txtIsEmoji) == false && Convert.ToBoolean(txtIsSubjective) == false)
                                                //    {
                                                //        //Check No of Option Min 2 to 5
                                                //if (txtOptions != null && txtOptions != string.Empty)
                                                // {
                                                //if (Convert.ToInt32(txtOptions) >= 1 && Convert.ToInt32(txtOptions) < 10)
                                                //{

                                                //}
                                                //else
                                                //{
                                                //    sbError.Append(Record.EnterMinimumOption);
                                                //    break;
                                                //}
                                                // }
                                                //option10
                                                if ((Convert.ToInt32(txtOptions) == 10))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 10 && ((txtEnterOption10 != null && !string.IsNullOrEmpty(txtEnterOption10)) && (txtEnterOption9 != null && !string.IsNullOrEmpty(txtEnterOption9) && (txtEnterOption8 != null && !string.IsNullOrEmpty(txtEnterOption8) && (txtEnterOption7 != null && !string.IsNullOrEmpty(txtEnterOption7) && (txtEnterOption6 != null && !string.IsNullOrEmpty(txtEnterOption6)) && (txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1)))))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption6.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption7.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption10.ToLower())
                                                            || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption10.ToLower())
                                                            || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption10.ToLower())
                                                            || (txtEnterOption3.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower())
                                                            || (txtEnterOption4.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption10.ToLower())
                                                            || (txtEnterOption5.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption10.ToLower()))))
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
                                                if ((Convert.ToInt32(txtOptions) == 9))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 9 && ((txtEnterOption9 != null && !string.IsNullOrEmpty(txtEnterOption9)) && (txtEnterOption8 != null && !string.IsNullOrEmpty(txtEnterOption8) && (txtEnterOption7 != null && !string.IsNullOrEmpty(txtEnterOption7) && (txtEnterOption6 != null && !string.IsNullOrEmpty(txtEnterOption6)) && (txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption6.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption7.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption9.ToLower())
                                                            || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption7.ToLower())
                                                            || (txtEnterOption3.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption9.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower())
                                                            || (txtEnterOption4.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption9.ToLower())
                                                            || (txtEnterOption5.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption8.ToLower()))))
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
                                                if ((Convert.ToInt32(txtOptions) == 8))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 8 && ((txtEnterOption8 != null && !string.IsNullOrEmpty(txtEnterOption8)) && (txtEnterOption7 != null && !string.IsNullOrEmpty(txtEnterOption7) && (txtEnterOption6 != null && !string.IsNullOrEmpty(txtEnterOption6)) && (txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1)))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption6.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption7.ToLower() || (txtEnterOption1.ToLower() == txtEnterOption8.ToLower())
                                                            || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption7.ToLower())
                                                            || (txtEnterOption3.ToLower() == txtEnterOption8.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower())
                                                            || (txtEnterOption4.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption8.ToLower())
                                                            || (txtEnterOption5.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption7.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption8.ToLower()))))
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
                                                if ((Convert.ToInt32(txtOptions) == 7))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 7 && ((txtEnterOption7 != null && !string.IsNullOrEmpty(txtEnterOption7)) && (txtEnterOption6 != null && !string.IsNullOrEmpty(txtEnterOption6)) && (txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower())
                                                            || (txtEnterOption4.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption6.ToLower()))
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
                                                if ((Convert.ToInt32(txtOptions) == 6))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 6 && ((txtEnterOption6 != null && !string.IsNullOrEmpty(txtEnterOption6)) && (txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower())
                                                            || (txtEnterOption4.ToLower() == txtEnterOption6.ToLower()) || (txtEnterOption5.ToLower() == txtEnterOption6.ToLower()))
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
                                                if ((Convert.ToInt32(txtOptions) == 5))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 5 && ((txtEnterOption5 != null && !string.IsNullOrEmpty(txtEnterOption5)) && (txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption5.ToLower()) || (txtEnterOption4.ToLower() == txtEnterOption5.ToLower()))
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
                                                ////option4
                                                if ((Convert.ToInt32(txtOptions) == 4))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 4 && ((txtEnterOption4 != null && !string.IsNullOrEmpty(txtEnterOption4)) && (txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption4.ToLower()) || (txtEnterOption3.ToLower() == txtEnterOption4.ToLower()))
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
                                                if ((Convert.ToInt32(txtOptions) == 3))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 3 && ((txtEnterOption3 != null && !string.IsNullOrEmpty(txtEnterOption3)) && (txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()) || (txtEnterOption1.ToLower() == txtEnterOption3.ToLower()) || (txtEnterOption2.ToLower() == txtEnterOption3.ToLower()))
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
                                                if ((Convert.ToInt32(txtOptions) == 2))
                                                {
                                                    if (Convert.ToInt32(txtOptions) == 2 && ((txtEnterOption2 != null && !string.IsNullOrEmpty(txtEnterOption2)) && (txtEnterOption1 != null && !string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        if ((txtEnterOption1.ToLower() == txtEnterOption2.ToLower()))
                                                        {
                                                            sbError.Append(Record.OptionareDuplicate);
                                                            break;
                                                        }
                                                        //sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        //break;
                                                    }
                                                    else
                                                    {
                                                        sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }
                                                }

                                                if ((Convert.ToInt32(txtOptions) <= 1))
                                                {
                                                    if (Convert.ToInt32(txtOptions) <= 1 && ((txtEnterOption1 == null && string.IsNullOrEmpty(txtEnterOption1))))
                                                    {
                                                        //sbError.Append(Record.OptionOptionvaluedoesnotMatch);
                                                        break;
                                                    }

                                                }
                                            }
                                            bool question = surveyQuestionRepository.QuestionExists(surveyquestion.Question);
                                            if (question == true)
                                            {
                                                sbError.Append("Duplicate Question ");
                                                break;
                                            }



                                        }
                                    }
                                }
                                bool validvalue = false;
                                //if (CSVInjectionAttribute.CheckForSQLInjection(txtQuestion))
                                //    validvalue = true;
                                //else
                                //if (FileValidation.CheckForSQLInjection(txtActiveQuestion))
                                //    validvalue = true;
                                //else
                                //if (FileValidation.CheckForSQLInjection(txtObjectiveQuestion))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption1))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtOptions.ToString()))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption2))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption3))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption4))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption5))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption6))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption7))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption8))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption9))
                                //    validvalue = true;
                                //else
                                //  if (FileValidation.CheckForSQLInjection(txtEnterOption10.ToString()))
                                //    validvalue = true;

                                if (validvalue == true)
                                {
                                    sbError.Append("  Please enter valid data,");

                                    surveyquestionrejected.ErrorMessage = sbError.ToString();

                                    surveyquestionrejected.CreatedDate = DateTime.UtcNow;
                                    surveyquestionrejected.CreatedBy = UserId;
                                    surveyquestionrejected.ModifiedDate = DateTime.UtcNow;
                                    surveyquestionrejected.ModifiedBy = UserId;
                                    surveyquestionrejected.ErrorMessage = sbError.ToString();
                                    await surveyQuestionRepository.Add(surveyquestion);
                                    totalRecordRejected++;
                                    surveyquestion = new SurveyQuestion();
                                    surveyquestionrejected = new SurveyQuestionRejected();

                                    //feedbackQuestionRejected = new FeedbackQuestionRejected();
                                    sbError.Clear();
                                }
                                else
                                {

                                    surveyquestionrejected.ErrorMessage = sbError.ToString();

                                    if (sbError.ToString() != "")
                                    {
                                        surveyquestionrejected.CreatedDate = DateTime.UtcNow;
                                        surveyquestionrejected.CreatedBy = UserId;
                                        surveyquestionrejected.ModifiedDate = DateTime.UtcNow;
                                        surveyquestionrejected.ModifiedBy = UserId;
                                        surveyquestionrejected.ErrorMessage = sbError.ToString();
                                        await _surveyQuestionRejectedRepository.Add(surveyquestionrejected);
                                        totalRecordRejected++;
                                        surveyquestion = new SurveyQuestion();
                                        surveyquestionrejected = new SurveyQuestionRejected();

                                        sbError.Clear();

                                    }
                                    else
                                    {
                                        //add Record 
                                        //bool valid1 = false;
                                        surveyquestion.CreatedDate = DateTime.UtcNow;
                                        // surveyquestion.CreatedBy = UserId;
                                        surveyquestion.Status = true;
                                        //surveyquestion.IsAllowSkipping = false;
                                        surveyquestion.ModifiedBy = UserId;
                                        surveyquestion.ModifiedDate = DateTime.UtcNow;
                                        surveyquestion.Question = surveyquestionrejected.Question;
                                        surveyquestion.Section = surveyquestionrejected.ObjectiveQuestion;
                                        if (Convert.ToBoolean(surveyquestionrejected.ObjectiveQuestion) == true)
                                        {
                                            surveyquestion.IsMultipleChoice = surveyquestionrejected.IsMultipleChoice;
                                            if (surveyquestion.IsMultipleChoice == true)
                                            {
                                                surveyquestion.OptionType = "MultipleSelection";
                                            }
                                            else
                                            {
                                                surveyquestion.OptionType = "SingleSelection";
                                            }
                                        }
                                      
                                        if (Convert.ToBoolean(surveyquestionrejected.ObjectiveQuestion) == true)
                                        {
                                            surveyquestion.Section = Record.Objective;
                                        }
                                        else
                                        { surveyquestion.Section = Record.Subjective; }
                                        if (Convert.ToBoolean(surveyquestionrejected.ActiveQuestion) == true)
                                        {
                                            surveyquestion.Status = Convert.ToBoolean(Record.True);
                                        }
                                        else
                                        { surveyquestion.Status = Convert.ToBoolean(Record.False); }



                                        await surveyQuestionRepository.Add(surveyquestion);

                                        if (surveyquestion.Section == Record.Objective)
                                        {
                                            if (surveyquestionrejected.Id != null || surveyquestionrejected.Id != 0)
                                            {
                                                if (txtEnterOption1 != null && Convert.ToInt32(txtOptions) >= 1)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption1,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption2 != null && Convert.ToInt32(txtOptions) >= 2)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption2,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption3 != null && Convert.ToInt32(txtOptions) >= 3)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption3,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption4 != null && Convert.ToInt32(txtOptions) >= 4)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption4,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption5 != null && Convert.ToInt32(txtOptions) >= 5)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption5,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption6 != null && Convert.ToInt32(txtOptions) >= 6)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption6,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                                if (txtEnterOption7 != null && Convert.ToInt32(txtOptions) >= 7)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption7,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);

                                                }
                                                if (txtEnterOption8 != null && Convert.ToInt32(txtOptions) >= 8)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption8,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);

                                                }
                                                if (txtEnterOption9 != null && Convert.ToInt32(txtOptions) >= 9)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption9,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);

                                                }
                                                if (txtEnterOption10 != null && Convert.ToInt32(txtOptions) >= 10)
                                                {
                                                    SurveyOption surveyoption = new SurveyOption
                                                    {
                                                        OptionText = txtEnterOption10,
                                                        QuestionId = surveyquestion.Id,
                                                        CreatedBy = UserId,
                                                        CreatedDate = DateTime.UtcNow,
                                                        ModifiedDate = DateTime.UtcNow,
                                                        ModifiedBy = UserId
                                                    };
                                                    await surveyOptionRepository.Add(surveyoption);
                                                }
                                            }

                                        }
                                        totalRecordInsert++;
                                        surveyquestionrejected = new SurveyQuestionRejected();
                                        surveyquestion = new SurveyQuestion();
                                        sbError.Clear();

                                    }

                                }
                            }

                            else
                            {
                                // Logger.WriteLog(null, "Total records : " + totalRecordInsert.ToString() + "  Inserted and  " + totalRecordUpdate.ToString() + "  Updated ", null, "Operation");
                            }
                        }

                    }
                    return "Total number of record inserted :" + totalRecordInsert + ",  Total number of record  rejected : " + totalRecordRejected;
                }

                catch (Exception)
                {
                    //Logger.WriteLog(null, null, "Title Exception for User ID: " + Convert.ToString(textuserID) + " Error message: " + ex.ToString(), "Failed");
                }
                return (Record.TotalRecordInserted + totalRecordInsert + Record.TotalRecordRejected + totalRecordRejected);
            }

            public static bool ValidateQuestion(string headerText, string questionText)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (questionText.Length > 1000)
                    {
                        valid = true;
                        sbError.Append("Question length must not more than 1000 character");
                        
                    }
                    if (questionText != null && !string.IsNullOrEmpty(questionText))
                    {
                        surveyquestionrejected.Question = questionText;
                    }
                    else
                    {
                        surveyquestion.Question = questionText;
                        surveyquestionrejected.Question = questionText;
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateActiveQuestion(string headerText, string activequestion)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (activequestion != null && !string.IsNullOrEmpty(activequestion))
                    {

                        surveyquestionrejected.ActiveQuestion = activequestion;
                    }
                    else
                    {
                        surveyquestionrejected.ActiveQuestion = activequestion;
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }



                }

                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }

            private static bool ValidateObjectiveQuestion(string headerText, string objectivequestion)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (objectivequestion != null && !string.IsNullOrEmpty(objectivequestion))
                    {
                        //surveyquestion.Question = objectivequestion;
                        surveyquestionrejected.ObjectiveQuestion = objectivequestion;
                    }
                    else
                    {
                        surveyquestionrejected.ObjectiveQuestion = objectivequestion;
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }


                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateOptions(string headerText, string options)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (options != null && !string.IsNullOrEmpty(options))
                    {
                        try
                        {
                            if (Convert.ToInt32(options) > 0)
                            {

                                surveyquestionrejected.Options = options;
                            }
                            else

                            {
                                sbError.Append("Please eneter valid " + headerText);
                                valid = true;
                            }
                        }
                        catch (Exception)
                        {
                            sbError.Append("Please eneter valid " + headerText);

                            valid = true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption1(string headerText, string option1)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option1 != null && !string.IsNullOrEmpty(option1))
                    {
                        surveyquestionrejected.EnterOption1 = option1;
                    }

                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption2(string headerText, string option2)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option2 != null && !string.IsNullOrEmpty(option2))
                    {
                        surveyquestionrejected.EnterOption2 = option2;
                    }


                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption3(string headerText, string option3)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option3 != null && !string.IsNullOrEmpty(option3))
                    {
                        surveyquestionrejected.EnterOption3 = option3;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption4(string headerText, string option4)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option4 != null && !string.IsNullOrEmpty(option4))
                    {
                        surveyquestionrejected.EnterOption4 = option4;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption5(string headerText, string option5)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option5 != null && !string.IsNullOrEmpty(option5))
                    {
                        surveyquestionrejected.EnterOption5 = option5;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption6(string headerText, string option6)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option6 != null && !string.IsNullOrEmpty(option6))
                    {
                        surveyquestionrejected.EnterOption6 = option6;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption7(string headerText, string option7)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option7 != null && !string.IsNullOrEmpty(option7))
                    {
                        surveyquestionrejected.EnterOption7 = option7;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption8(string headerText, string option8)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option8 != null && !string.IsNullOrEmpty(option8))
                    {
                        surveyquestionrejected.EnterOption8 = option8;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption9(string headerText, string option9)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option9 != null && !string.IsNullOrEmpty(option9))
                    {
                        surveyquestionrejected.EnterOption9 = option9;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
            public static bool ValidateEnterOption10(string headerText, string option10)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (option10 != null && !string.IsNullOrEmpty(option10))
                    {
                        surveyquestionrejected.EnterOption10 = option10;
                    }
                }
                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }

            public static bool ValidateIsMultipleChoice(string headerText, string IsMultipleChoice)
            {
                bool valid = false;
                //questionType
                try
                {
                    if (IsMultipleChoice != null && !string.IsNullOrEmpty(IsMultipleChoice))
                    {

                        surveyquestionrejected.IsMultipleChoice = Convert.ToBoolean(IsMultipleChoice);
                       
                    }
                    else
                    {
                        surveyquestionrejected.IsMultipleChoice = Convert.ToBoolean(IsMultipleChoice);
                        sbError.Append(headerText + Record.EmptyMessage);
                        valid = true;
                    }



                }

                catch (Exception)
                {
                    // Logger.WriteLog(null, null, "Title Exception for Customer Code: " + Convert.ToString(textcustomerCode) + " Error message: " + ex.ToString(), "Failed");
                }
                return valid;
            }
        }
    }
}












