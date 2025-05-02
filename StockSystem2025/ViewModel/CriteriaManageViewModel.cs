using StockSystem2025.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Formula1 = StockSystem2025.ViewModel.Formula1;
using Formula2 = StockSystem2025.ViewModel.Formula2;
using Formula3 = StockSystem2025.ViewModel.Formula3;
using Formula4 = StockSystem2025.ViewModel.Formula4;
using Formula5 = StockSystem2025.ViewModel.Formula5;
using Formula6 = StockSystem2025.ViewModel.Formula6;
using Formula7 = StockSystem2025.ViewModel.Formula7;
using Formula8 = StockSystem2025.ViewModel.Formula8;
using Formula9 = StockSystem2025.ViewModel.Formula9;
using Formula10 = StockSystem2025.ViewModel.Formula10;
using Formula11 = StockSystem2025.ViewModel.Formula11;
using Formula12 = StockSystem2025.ViewModel.Formula12;
using Formula13 = StockSystem2025.ViewModel.Formula13;
using Formula14 = StockSystem2025.ViewModel.Formula14;
using Formula15 = StockSystem2025.ViewModel.Formula15;
using Formula16 = StockSystem2025.ViewModel.Formula16;
using Formula17 = StockSystem2025.ViewModel.Formula17;

namespace StockSystem2025.ViewModels
{
    public class CriteriaManageViewModel
    {
        public int Id { get; set; }

      [Required(ErrorMessage = "الرجاء إدخال اسم الإستراتيجية")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(100)]
        public string? Type { get; set; }

        public string? Note { get; set; }

        [StringLength(100)]
        public string? Separater { get; set; }

        [Range(0, int.MaxValue)]
        public int? OrderNo { get; set; }

        public string? Description { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public string? ImageUrl { get; set; }

       [Range(0, 2, ErrorMessage = "يجب اختيار نوع مؤشر صالح")]
        public int IsIndicator { get; set; }

        public bool IsGeneral { get; set; }

        public bool InsertToMarketPage { get; set; }

        public List<FormulaManageViewModel> Formulas { get; set; } = new List<FormulaManageViewModel>();
    }

    public class FormulaManageViewModel
    {
        public int Day { get; set; }
        public int FormulaType { get; set; }
        public Formula1 Formula1 { get; set; } = new Formula1();
        public Formula2 Formula2 { get; set; } = new Formula2();
        public Formula3 Formula3 { get; set; } = new Formula3();
        public Formula4 Formula4 { get; set; } = new Formula4();
        public Formula5 Formula5 { get; set; } = new Formula5();
        public Formula6 Formula6 { get; set; } = new Formula6();
        public Formula7 Formula7 { get; set; } = new Formula7();
        public Formula8 Formula8 { get; set; } = new Formula8();
        public Formula9 Formula9 { get; set; } = new Formula9();
        public Formula10 Formula10 { get; set; } = new Formula10();
        public Formula11 Formula11 { get; set; } = new Formula11();
        public Formula12 Formula12 { get; set; } = new Formula12();
        public Formula13 Formula13 { get; set; } = new Formula13();
        public Formula14 Formula14 { get; set; } = new Formula14();
        public Formula15 Formula15 { get; set; } = new Formula15();
        public Formula16 Formula16 { get; set; } = new Formula16();
        public Formula17 Formula17 { get; set; } = new Formula17();
    }
}