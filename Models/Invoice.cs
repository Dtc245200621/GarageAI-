using System;
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class Invoice
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int RepairOrderId { get; set; }

    public RepairOrder? RepairOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(30)]
    public string PaymentStatus { get; set; } = "ChuaThanhToan";
}