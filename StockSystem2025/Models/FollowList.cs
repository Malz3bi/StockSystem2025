using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StockSystem2025.Models;

public partial class FollowList
{
    public int Id { get; set; }

    [Required(ErrorMessage = "حقل الاسم مطلوب.")]
    [StringLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "حقل اللون مطلوب.")]
    [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "يرجى إدخال رمز لون هيكس صالح (مثل #FF0000).")]
    public string? Color { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون وقف الخسارة بين 0 و10.")]
    public double? StopLoss { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون الدعم الأول بين 0 و10.")]
    public double? FirstSupport { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون الدعم الثاني بين 0 و10.")]
    public double? SecondSupport { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون الهدف الأول بين 0 و10.")]
    public double? FirstTarget { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون الهدف الثاني بين 0 و10.")]
    public double? SecondTarget { get; set; }

    [Range(0, 10, ErrorMessage = "يجب أن يكون الهدف الثالث بين 0 و10.")]
    public double? ThirdTarget { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    public virtual ICollection<FollowListCompany> FollowListCompanies { get; set; } = new List<FollowListCompany>();

    public virtual AspNetUser User { get; set; } = null!;
}