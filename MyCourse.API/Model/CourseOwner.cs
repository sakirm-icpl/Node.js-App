﻿using System;

namespace MyCourse.API.Model
{
    public class CourseOwner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string logoUrl { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
