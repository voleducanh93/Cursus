﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursus.Data.DTO
{
    public class StepUpdateDTO
    {
        [Required]
        public int Id { get; set; }  
        [Required]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
