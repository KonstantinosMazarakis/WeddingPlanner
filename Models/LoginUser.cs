using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class LoginUser
    {

        [Required]
        [EmailAddress]
        public string LoginEmail {get;set;}
        
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string LoginPassword {get;set;}

    }
}