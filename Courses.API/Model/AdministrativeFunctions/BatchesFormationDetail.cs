// ======================================
// <copyright file="BatchesFormationDetail.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================


using System.ComponentModel.DataAnnotations;


namespace Courses.API.Model.AdministrativeFunctions
{
    public class BatchesFormationDetail : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [MaxLength(50)]
        public string Status { get; set; }
        public int BatchesFormationId { get; set; }
        public BatchesFormation BatchesFormations { get; set; }

    }
}
