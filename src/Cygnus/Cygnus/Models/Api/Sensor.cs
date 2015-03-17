﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cygnus.Models.Api
{
    public class Sensor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Location { get; set; }
    }
}