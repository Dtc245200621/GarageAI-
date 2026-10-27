using System.ComponentModel.DataAnnotations;
namespace GarageAI.Models;
public class Customer {
 public int Id { get; set; }
 [Required(ErrorMessage="Vui lòng nhập họ tên")][StringLength(120)] public string FullName { get; set; } = "";
 [Required(ErrorMessage="Vui lòng nhập số điện thoại")][Phone] public string Phone { get; set; } = "";
 [EmailAddress] public string? Email { get; set; }
 public string? Address { get; set; }
}
