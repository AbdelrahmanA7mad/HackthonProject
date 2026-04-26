using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class AiMessage
    {
        [Key]
        public int Id { get; set; }

        public int AiConversationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } // "user" or "model"

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AiConversationId")]
        public virtual AiConversation Conversation { get; set; }
    }
}
