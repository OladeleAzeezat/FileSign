namespace FileSign.Models
{
    public class FileRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ContentType { get; set; }
        //public byte[] FileData { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }
}
