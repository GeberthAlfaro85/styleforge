using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyleForge.Application.DTOs.Tenants
{
    public class UpdateTenant
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TypBusiness { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>Solo lectura en este DTO: se ignora al recibir un Update, se cambia vía el endpoint dedicado de logo.</summary>
        public string? LogoUrl { get; set; }
    }
}
