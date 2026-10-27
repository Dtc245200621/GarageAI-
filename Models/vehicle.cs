
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập biển số")]
    [StringLength(20)]
    public string LicensePlate { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập hãng xe")]
    [StringLength(100)]
    public string Brand { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập dòng xe")]
    [StringLength(100)]
    public string Model { get; set; } = "";

    [Range(1886, 2100)]
    public int? Year { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Vui lòng chọn khách hàng")]
    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }
}

