using flaschenpost_exercise_5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace flaschenpost_exercise_5.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProductDataController : Controller
    {
        /// <summary>
        /// Reads product data from given URL and checks which articles are the cheapest as well as the most expensive per litre. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <returns>The cheapest and most expensive articles.</returns>
        [HttpGet(Name = "GetCheapestAndMostExpensiveArticlesPerLitre")]
        public JsonResult GetCheapestAndMostExpensiveArticlesPerLitre(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            return GetCheapestAndMostExpensiveArticlesPerLitre(null, url);
        }

        [HttpGet(Name = "GetCheapestAndMostExpensiveArticlesPerLitre")]
        internal JsonResult GetCheapestAndMostExpensiveArticlesPerLitre(ProductData[]? productData, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            if (productData == null)
            {
                productData = GetProductData(url);
            }

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            var articleData = productData
                .SelectMany(p => p.Articles
                    .Select(a => new Article
                    {
                        Id = a.Id,
                        ProductBrandName = p.BrandName,
                        ProductName = p.Name,
                        ShortDescription = a.ShortDescription,
                        Price = a.Price,
                        Unit = a.Unit,
                        PricePerUnitText = a.PricePerUnitText,
                        Image = a.Image
                    })
                )
                .OrderBy(a => a.PricePerUnit);

            var minPriceLimit = articleData.First().PricePerUnit;
            var minPricesTakeWhile = articleData.TakeWhile(a => a.PricePerUnit <= minPriceLimit).OrderBy(a => a.ProductName);

            var articlesReverse = articleData.Reverse();
            var maxPriceLimit = articlesReverse.First().PricePerUnit;
            var maxPricesTakeWhile = articlesReverse.TakeWhile(a => a.PricePerUnit >= maxPriceLimit).OrderBy(a => a.ProductName);

            var minAndMaxPriceArticles = minPricesTakeWhile.Union(maxPricesTakeWhile);

            return SerializeJsonResult(minAndMaxPriceArticles);
        }

        /// <summary>
        /// Reads product data from given URL and checks which articles match a given price. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <param name="price">The price to check for.</param>
        /// <returns>The articles that match the given price ordered ascending by price per litre.</returns>
        [HttpGet(Name = "GetArticlesByPrice")]
        public JsonResult GetArticlesByPrice(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            return GetArticlesByPrice(null, url, price);
        }

        [HttpGet(Name = "GetArticlesByPrice")]
        internal JsonResult GetArticlesByPrice(ProductData[]? productData, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            if (productData == null)
            {
                productData = GetProductData(url);
            }

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            var productsWithArticlesWithExactPrice = productData.Select(pd => pd).Where(pd => pd.Articles.Any(a => a.Price == price)).ToArray();

            // add product name and brandname into article response
            var articleArraysWithExactPrice = productsWithArticlesWithExactPrice
                .Select(p => p.Articles
                    .Select(a => new Article
                    {
                        Id = a.Id,
                        ProductBrandName = p.BrandName,
                        ProductName = p.Name,
                        ShortDescription = a.ShortDescription,
                        Price = a.Price,
                        Unit = a.Unit,
                        PricePerUnitText = a.PricePerUnitText,
                        Image = a.Image
                    })
                .Where(a => a.Price == price))
                .ToArray();

            // Order the result by price per litre (cheapest first)
            var articlesWithExactPrice = articleArraysWithExactPrice
                .SelectMany(a => a)
                .OrderBy(a => a.PricePerUnit)
                .ToArray();

            return SerializeJsonResult(articlesWithExactPrice);
        }

        /// <summary>
        /// Reads product data from given URL and checks which articles come in the most bottles. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <returns>The articles that come in the most bottles.</returns>
        [HttpGet(Name = "GetArticlesWithMostBottles")]
        public JsonResult GetArticlesWithMostBottles(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            return GetArticlesWithMostBottles(null, url);
        }


        [HttpGet(Name = "GetArticlesWithMostBottles")]
        internal JsonResult GetArticlesWithMostBottles(ProductData[]? productData = null, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            if (productData == null)
            {
                productData = GetProductData(url);
            }

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            var articleData = productData
                .SelectMany(p => p.Articles
                    .Select(a => new Article
                    {
                        Id = a.Id,
                        ProductBrandName = p.BrandName,
                        ProductName = p.Name,
                        ShortDescription = a.ShortDescription,
                        Price = a.Price,
                        Unit = a.Unit,
                        PricePerUnitText = a.PricePerUnitText,
                        Image = a.Image
                    })
                )
                .OrderByDescending(a => a.AmountBottles);


            var maxAmountBottlesimit = articleData.First().AmountBottles;
            var maxAmountBottlesTakeWhile = articleData.TakeWhile(a => a.AmountBottles >= maxAmountBottlesimit).OrderBy(a => a.ProductName);

            return SerializeJsonResult(maxAmountBottlesTakeWhile);
        }

        /// <summary>
        /// Calls GetCheapestAndMostExpensiveArticlesPerLitre, GetArticlesByPrice, and GetArticlesWithMostBottles and combines their results into a single result.
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <param name="price">The price to check for.</param>
        /// <returns>The combined results.</returns>
        [HttpGet(Name = "DoAllThreeActions")]
        public JsonResult GetCheapestAndMostExpensive_GetByPrice_GetMostBottles(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            var productData = GetProductData(url);

            var minAndMaxPricesJson = GetCheapestAndMostExpensiveArticlesPerLitre(productData, url);
            var matchingPriceJson = GetArticlesByPrice(productData, url, price);
            var mostBottlesJson = GetArticlesWithMostBottles(productData, url);

            var minAndMaxPricesModel = ((IEnumerable<Article>)minAndMaxPricesJson.Value).ToArray();
            var matchingPricesModel = ((IEnumerable<Article>)matchingPriceJson.Value).ToArray();
            var mostBottlesModel = ((IEnumerable<Article>)mostBottlesJson.Value).ToArray();

            var combinedResults = new CombinedProductDataResult[3] {
                new CombinedProductDataResult { Action = "GetCheapestAndMostExpensiveArticlesPerLitre", Results = minAndMaxPricesModel },
                new CombinedProductDataResult { Action = "GetArticlesByPrice", Results = matchingPricesModel },
                new CombinedProductDataResult { Action = "GetArticlesWithMostBottles", Results = mostBottlesModel }
            };

            return SerializeCombinedJsonResult(combinedResults);
        }

        private static JsonResult SerializeJsonResult(IEnumerable<Article> resultArticles)
        {
            return new JsonResult(resultArticles, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private static JsonResult SerializeCombinedJsonResult(IEnumerable<CombinedProductDataResult> results)
        {
            return new JsonResult(results, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private ProductData[] GetProductData(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var jsonString = response.Content.ReadAsStringAsync().Result;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Usually you should check the data called from third parties for malicious code and sanitize your data.
                // I decided to skip this here since that would probably increase the scope of a take-home task too much.
                // Assumptions regarding the uniformity and well-formedness of the JSONs are partly covered by unit tests.
                // Here actions fail silently if there is an error.
                var productData = JsonSerializer.Deserialize<ProductData[]>(jsonString, options);

                return productData;
            }
            catch
            {
                return null;
            }


        }
    }
}

