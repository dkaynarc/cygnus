using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cygnus.Models.Api
{
    public class Resource
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Description { get; set; }

        // Foreign Key
        public Guid GatewayId { get; set; }
        // Navigation property
        public virtual Gateway Gateway { get; set; }
    }

    public class ResourceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Description { get; set; }
        public Guid GatewayId { get; set; }
    }
}