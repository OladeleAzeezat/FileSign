namespace FileSign.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }
        public string Role { get; set; }
        //public string PasswordHash { get; set; } = string.Empty;
        //public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }

}
