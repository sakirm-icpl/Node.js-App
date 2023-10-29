namespace ILT.API.Model.ILT
{
    public class ModuleTopicAssociation : BaseModel
    {
        public int ID { get; set; }
        public int ModuleId { get; set; }
        public int TopicId { get; set; }
    }
}
