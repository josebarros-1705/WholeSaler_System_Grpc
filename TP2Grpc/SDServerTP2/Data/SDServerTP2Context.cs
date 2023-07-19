using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SDServerTP2.Models;

namespace SDServerTP2.Data
{
    public sealed class SDServerTP2Context : DbContext
    {
        public SDServerTP2Context (DbContextOptions<SDServerTP2Context> options)
            : base(options)
        {
        }

        public DbSet<SDServerTP2.Models.Servicos> Servico { get; set; } = default!;
        public DbSet<SDServerTP2.Models.Operador> Operador { get; set; } = default!;
        public DbSet<SDServerTP2.Models.Domicilio> Domicilio { get; set; } = default!;
    }
}
