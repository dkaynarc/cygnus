using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cygnus.Models.Api
{
    public class Gateway
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}