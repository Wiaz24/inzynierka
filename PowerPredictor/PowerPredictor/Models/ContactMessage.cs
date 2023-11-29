using System.ComponentModel.DataAnnotations;

namespace PowerPredictor.Models
{
    /// <summary>
    /// Contact message model
    /// </summary>
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
    }
}
