using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.ViewModels.Account
{
    public class LoginVM
    {
        [Required]
        [StringLength(maximumLength: 30)]
        public string Email { get; set; }

        [Required]
        [StringLength(maximumLength: 30)]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }
    }
}
