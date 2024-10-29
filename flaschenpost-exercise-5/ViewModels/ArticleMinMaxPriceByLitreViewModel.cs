using flaschenpost_exercise_5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace flaschenpost_exercise_5.ViewModels
{
    public class ArticleMinMaxPriceByLitreViewModel
    {
        /// <summary>
        /// The article(s) with the lowest price per litre 
        /// </summary>
        public ArticleProductViewModel[]? MinPrice { get; set; }

        /// <summary>
        /// The article(s) with the highest price per litre 
        /// </summary>
        public ArticleProductViewModel[]? MaxPrice { get; set; }
    }

    public static class ArticleMinMaxPriceByLitreViewModelExtensions
    {
        public static JsonResult SerializeToJsonResult(this ArticleMinMaxPriceByLitreViewModel value)
        {
            return new JsonResult(value, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}