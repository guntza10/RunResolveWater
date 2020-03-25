using System.Collections.Generic;

namespace Test.models
{
    public class BlobNameExist
    {
        public string Id { get; set; }
        public string EA { get; set; }
        public string SampleType { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public bool Exists { get; set; }
    }
}