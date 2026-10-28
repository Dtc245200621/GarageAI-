using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class RepairOrderPart
{
    public int Id { get; set; }

    [Required]
    public int RepairOrderId { get; set; }

    public RepairOrder? RepairOrder { get; set; }

    [Required]
    public int PartId { get; set; }

    public Part? Part { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; } = 1;

    [Range(typeof(decimal), "0", "999999999",
        ErrorMessage = "Đơn giá không được âm")]
    public decimal UnitPrice { get; set; }
}