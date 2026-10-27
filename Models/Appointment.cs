
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class Appointment
{
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Vui lòng chọn xe")]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày hẹn")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả tình trạng xe")]
    [StringLength(1000)]
    public string Description { get; set; } = "";

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "ChoXacNhan";
}
