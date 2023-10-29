// ======================================
// <copyright file="ProjectMaster.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Gadget.API.APIModel
{
    public class APIProjectMaster 
    {
        public Guid RowGuid { get; set; }

        public bool IsTeamEntry { get; set; }
        public int? TeamSize { get; set; } 
        [MaxLength(2000)]
        [Required]
        public string Answer1 { get; set; }
        [MaxLength(2000)]
        [Required]
        public string Answer2 { get; set; }
        [MaxLength(2000)]
        //[Required]
        public string Answer3 { get; set; }

        [MaxLength(1000)]
       // [Required]
        public string CategoryCode { get; set; }
    }

    public class APITileDetails
    {
        public int Id { get; set; }

        public string Name { get; set; }
       
        public string Type { get; set; }
        public string Tag { get; set; }
        public string File { get; set; }
        public int VolumeNumber { get; set; }
        public DateTime PublishedDate { get; set; }
        [MaxLength(2000)]
        public string Icon { get; set; }


    }
    public class APIGetTileDetails
    {
        public string Tag { get; set; }
    }
}
