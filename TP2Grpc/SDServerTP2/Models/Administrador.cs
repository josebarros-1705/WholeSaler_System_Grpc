using System.ComponentModel.DataAnnotations;

namespace SDServerTP2.Models
{
    public class Administrador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
