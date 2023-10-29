// ======================================
// <copyright file="DiscussionForumPost.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model.DiscussionForum
{
    public class DiscussionForum : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int PostThreadId { get; set; }
        [Required]
        public int PostParentId { get; set; }
        [Required]
        public int PostLevel { get; set; }
        [Required]
        public int SortOrder { get; set; }
        [Required]
        [MaxLength(500)]
        public string SubjectText { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [MaxLength(200)]
        public string UseName { get; set; }
        [Required]
        public int CourseId { get; set; }

        public string FilePath { get; set; }

        public string FileType { get; set; }

    }

    public class DiscussionForumUsers
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; }
    }
}
