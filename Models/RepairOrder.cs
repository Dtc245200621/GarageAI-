
using System;
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class RepairOrder
{
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Vui lòng chọn xe")]
    public int VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.Now;

    [Required(ErrorMessage = "Nhập tình trạng xe")]
    [StringLength(1000)]
    public string Description { get; set; } = "";

    [StringLength(100)]
    public string TechnicianName { get; set; } = "";

    [StringLength(30)]
    public string Status { get; set; } = "MoiTiepNhan";

    [StringLength(2000)]
    public string RepairNotes { get; set; } = "";
}
