
using System.ComponentModel.DataAnnotations;

namespace GarageAI.Models;

public class Part
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã phụ tùng")]
    [StringLength(50)]
    [Display(Name = "Mã phụ tùng")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên phụ tùng")]
    [StringLength(150)]
    [Display(Name = "Tên phụ tùng")]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999",
        ErrorMessage = "Đơn giá phải từ 0 trở lên")]
    [Display(Name = "Đơn giá")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "Số lượng tồn kho không được âm")]
    [Display(Name = "Số lượng tồn kho")]
    public int StockQuantity { get; set; }
}
