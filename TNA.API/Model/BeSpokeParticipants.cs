//using Assessment.API.Models;

namespace TNA.API.Model
{
    public class BespokeParticipants : CommonFields
    {
        public int Id { get; set; }
        public int BespokeRequestId { get; set; }
        public int UserMasterId { get; set; }
        public string UserName { get; set; }
    }
}
