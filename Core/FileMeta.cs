using System;

namespace FileSync.Core
{
    public class FileMeta
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ETag { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string Crc64 { get; set; }
        public string Md5 { get; set; }
    }
}