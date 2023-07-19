using System.ComponentModel.DataAnnotations;

namespace SDServerTP2.Models
{
    public class Domicilio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Rua { get; set; }

        [Required]
        public string NPorta { get; set; }

    }
}
