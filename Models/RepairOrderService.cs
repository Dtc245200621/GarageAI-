using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class RepairOrderService
{
    public int Id { get; set; }

    [Required]
    public int RepairOrderId { get; set; }

    public RepairOrder? RepairOrder { get; set; }

    [Required]
    public int GarageServiceId { get; set; }

    public GarageService? GarageService { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitPrice { get; set; }
}