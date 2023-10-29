// ======================================
// <copyright file="APIDiscussionForumPost.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Courses.API.APIModel.DiscussionForum
{
    public class APIDiscussionForum
    {
        public int? Id { get; set; }
        public int PostThreadId { get; set; }
        public int CourseId { get; set; }
        public int PostParentId { get; set; }
        public int PostLevel { get; set; }
        public int SortOrder { get; set; }
        public string SubjectText { get; set; }
        public int UserId { get; set; }
        public string UserProfilePicture { get; set; }
        public string UseName { get; set; }
        public DateTime Date { get; set; }
        public List<APIDiscussionForum> Comments { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsCommentAddedByUser { get; set; }
        public string PrePostUsename { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string Gender { get; set; }
        public string UserName { get; set; }
    }

    public class APIDiscussionForumPost
    {
        public int? Id { get; set; }
        public int? PostId { get; set; }
        public int CourseId { get; set; }
        public string SubjectText { get; set; }
        public string FilePath { get; set; }

        public string FileType { get; set; }
        public string fileForUpload { get; set; }
    }

}