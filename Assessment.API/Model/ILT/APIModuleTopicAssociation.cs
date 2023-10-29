namespace Assessment.API.APIModel.ILT
{
    public class APIModuleTopicAssociation
    {
        public int ID { get; set; }
        public int ModuleId { get; set; }
        public TopicList[] TopicList { get; set; }
    }
    public class TopicList
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
    }

    public class APIModuleDetails
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class APITopicDetialsByModuleId
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
    }

    public class ModuleTypeAhead
    {
        public int Id { get; set; }
        public string ModuleName { get; set; }
    }
}
