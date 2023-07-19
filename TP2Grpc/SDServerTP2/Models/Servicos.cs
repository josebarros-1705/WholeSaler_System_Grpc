using System.ComponentModel.DataAnnotations;

namespace SDServerTP2.Models
{
    public class Servicos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OperadorUsername { get; set; }

        [Required]
        public int DomicilioId { get; set; }

        public Domicilio? Domicilio { get; set; }

        [Required]
        public string Modalidade { get; set; }

        [Required]
        [RegularExpression("RESERVED|ACTIVE|DEACTIVATED")]
        public string Estado { get; set; }

        [Required]
        public string CodAdministrativo { get; set; }

    }
}
