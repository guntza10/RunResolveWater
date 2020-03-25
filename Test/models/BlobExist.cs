using System.Collections.Generic;

namespace Test.models
{
    public class BlobExist
    {
        public string Id { get; set; }
        public List<string> listBlob { get; set; }
        public List<string> listEA { get; set; }
        public bool IsRun { get; set; }
    }
}