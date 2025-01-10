namespace FileSign.DTOs
{
    public class DocumentWithFileDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string UploadedAt { get; set; }
        public string FileData { get; set; } // Base64 encoded file content
    }
}
