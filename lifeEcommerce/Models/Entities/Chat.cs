using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace lifeEcommerce.Models.Entities
{
    public class Chat
    {
        [Key]
        public string Id { get; set; }
        public string User1Id { get; set; }
        public string User2Id { get; set; }
        public DateTime StartDate { get; set; }
    }
}
