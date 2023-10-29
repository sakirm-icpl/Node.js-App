using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Courses.API.Model
{
    public class CourseCompletionMailReminder : BaseModel
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public int FirstRemDays { get; set; }

        public string FirstRemTemplate { get; set; }

        public int? SecondRemDays { get; set; }

        public string SecondRemTemplate { get; set; }

        public int? ThirdRemDays { get; set; }

        public string? ThirdRemTemplate { get; set; }

        public int? FourthRemDays { get; set; }

        public string? FourthRemTemplate { get; set; }

        public int? FifthRemDays { get; set; }

        public string? FifthRemTemplate { get; set; }

    }

    public class CourseCompletionMailReminderListandCount
    {
        public List<CourseCompletionMailReminderGet> CompletionMailReminder { get; set; }
        public int Count { get; set; }
    }

    public class CourseCompletionMailReminderGet : CourseCompletionMailReminder
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }

        public string UserName { get; set; }

    }


}
