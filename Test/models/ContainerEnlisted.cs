using System.Collections.Generic;

namespace Test.models
{
    public class ContainerEnlisted
    {
        public string Id { get; set; }
        public string ContainerName { get; set; }
        public List<string> ListBlob { get; set; }
        public List<string> ListId { get; set; }
        public List<string> ListEA { get; set; }
        public string NewContainerName { get; set; }
        public bool IsFound { get; set; }
    }
}