using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace flaschenpost_exercise_5.ViewModels
{
    public class ArticleSummaryViewModel
    {
        /// <summary>
        /// The results returned by [productdata/article/min-and-max-price-per-litre" 
        /// </summary>
        public ArticleMinMaxPriceByLitreViewModel? MinAndMaxPricesPerLitre { get; set; }

        /// <summary>
        /// The results returned by [productdata/article/price]
        /// </summary>
        public ArticleProductViewModel[]? ByPrice { get; set; }

        /// <summary>
        /// The results returned by [productdata/article/most-bottles]
        /// </summary>
        public ArticleProductViewModel[]? MostBottles { get; set; }
    }

    public static class ArticleSummaryViewModelExtensions
    {
        public static JsonResult SerializeToJsonResult(this ArticleSummaryViewModel value)
        {
            return new JsonResult(value, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}
