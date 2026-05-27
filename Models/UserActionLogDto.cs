namespace naif_katalog.Models
{
    public class UserActionLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; }
        public int? ProductId { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public System.DateTime CreatedAt { get; set; }
        
        public UsersDto User { get; set; }
        public Product Product { get; set; }
    }
}
