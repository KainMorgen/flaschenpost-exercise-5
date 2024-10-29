using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace flaschenpost_exercise_5.ViewModels
{
    public class ArticleProductViewModel
    {
        /// <summary>
        /// Product Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Article Id
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// The brand name of the product.
        /// </summary>
        public string? ProductBrandName { get; set; }

        /// <summary>
        /// The name of the product.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Optional description text of the product. 
        /// </summary>

        public string? ProductDescription { get; set; }

        /// <summary>
        /// Describes the articles amount of units, capacity per unit and the material of containers. 
        /// </summary>
        public string ArticleShortDescription { get; set; }

        /// <summary>
        /// The price per article.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The unit of the article.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// The price per unit represented by text.
        /// </summary>
        public string PricePerUnitText { get; set; }

        /// <summary>
        /// URL to an image of the article.
        /// </summary>
        public string Image { get; set; }
    }

    public static class ArticleProductViewModelExtensions
    {
        public static JsonResult SerializeToJsonResult(this IEnumerable<ArticleProductViewModel> value)
        {
            return new JsonResult(value, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
    }
}
