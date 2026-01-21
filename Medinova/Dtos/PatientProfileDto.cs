using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class PatientProfileDto
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }
        [Url]
        public string ImageUrl { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        [Range(30, 250, ErrorMessage = "Boy değeri 30-250 cm arasında olmalıdır.")]
        public int? HeightCm { get; set; }

        [Range(2, 500, ErrorMessage = "Kilo değeri 2-500 kg arasında olmalıdır.")]
        public int? WeightKg { get; set; }

        [StringLength(10)]
        public string BloodType { get; set; }
        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}