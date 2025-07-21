using System;
using System.ComponentModel.DataAnnotations;

namespace KasKelasApi.DTOs
{
    public class KasSettingDto
    {
        public int Id { get; set; }
        [Required]
        public int ClassId { get; set; }
        [Required]
        public decimal AmountPerWeek { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
    }
}
