using Assessment.API.Models;

namespace Courses.API.Model
{
    public class BespokeParticipants : CommonFields
    {
        public int Id { get; set; }
        public int BespokeRequestId { get; set; }
        public int UserMasterId { get; set; }
        public string UserName { get; set; }
    }
}
