using StockSystem2025.Models;
using System.ComponentModel.DataAnnotations;

namespace StockSystem2025.ViewModel
{
    public class UploadFileViewModel
    {
        public List<DateTime?> ExistingDates { get; set; }
        public List<string> StockData { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال تاريخ البدء")]
        public DateTime? FromDate { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال تاريخ الانتهاء")]
        [Compare("FromDate", ErrorMessage = "تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء")]
        public DateTime? ToDate { get; set; }

        // Optional: Add if you want to bind files in the model
        public List<IFormFile> Files { get; set; }
    }

}