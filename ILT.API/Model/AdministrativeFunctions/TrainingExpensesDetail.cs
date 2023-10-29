// ======================================
// <copyright file="TrainingExpensesDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.AdministrativeFunctions
{
    public class TrainingExpensesDetail : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int TrainingExpenseId { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [MaxLength(100)]
        public String ExpenseHead { get; set; }
        [MaxLength(300)]
        public String Description { get; set; }
        [Required]
        [MaxLength(100)]
        public String VendorName { get; set; }
        [Required]
        public Decimal AmountPaid { get; set; }
        [MaxLength(50)]
        public String Currency { get; set; }
        public TrainingExpenses TrainingExpense { get; set; }
    }
}
