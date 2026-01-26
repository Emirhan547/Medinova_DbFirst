using System;
using System.ComponentModel.DataAnnotations;

namespace Medinova.Dtos
{
    public class AppointmentCreateDto
    {
        [Required(ErrorMessage = "Doktor seçimi gereklidir")]
        public int? DoctorId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi gereklidir")]
        public DateTime? AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati gereklidir")]
        [StringLength(20)]
        public string AppointmentTime { get; set; }

        [Required(ErrorMessage = "Ad soyad gereklidir")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email gereklidir")]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
    }
}