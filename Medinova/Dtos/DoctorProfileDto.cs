using System.ComponentModel.DataAnnotations;

namespace Medinova.Dtos.Profiles
{
    public class DoctorProfileDto
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        public string DepartmentName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Url]
        public string ImageUrl { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}