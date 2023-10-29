using System;

namespace Courses.API.APIModel.AdministrativeFunctions
{
    public class APITrainingExpenses
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
    }
    public class APITrainingExpensesDetail
    {
        public int? Id { get; set; }
        public int TrainingExpenseId { get; set; }
        public DateTime Date { get; set; }
        public String ExpenseHead { get; set; }
        public String Description { get; set; }
        public String VendorName { get; set; }
        public Decimal AmountPaid { get; set; }
        public String Currency { get; set; }
    }
}
