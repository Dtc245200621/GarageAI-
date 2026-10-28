
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class GarageService
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
    [StringLength(150)]
    [Display(Name = "Tên dịch vụ")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Mô tả")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999",
        ErrorMessage = "Đơn giá phải từ 0 trở lên")]
    [Display(Name = "Đơn giá")]
    public decimal Price { get; set; }
}
