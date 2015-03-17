using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cygnus.Models.Api
{
    public class Sensor
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Description { get; set; }

        // Foreign Key
        public Guid GatewayId { get; set; }
        // Navigation property
        public Gateway Gateway { get; set; }
    }
}