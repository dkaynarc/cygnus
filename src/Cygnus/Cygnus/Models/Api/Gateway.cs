using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cygnus.Models.Api
{
    public class Gateway
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}