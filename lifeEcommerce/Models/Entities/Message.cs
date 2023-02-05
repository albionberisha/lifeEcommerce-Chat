using System.ComponentModel.DataAnnotations.Schema;

namespace lifeEcommerce.Models.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string ChatId { get; set; }
        public string SendBy { get; set; }
        public string Content { get; set; }
        public DateTime MessageTime { get; set; }

        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }
        [ForeignKey("SendBy")]
        public User User { get; set; }
    }
}
