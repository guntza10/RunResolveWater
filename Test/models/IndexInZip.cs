using System.Collections.Generic;

namespace Test.models
{
    public class IndexInZip
    {
        public string Id { get; set; }
        public string ZipName { get; set; }
        public string ContainerName { get; set; }
        public List<string> Filelist { get; set; }
    }
}