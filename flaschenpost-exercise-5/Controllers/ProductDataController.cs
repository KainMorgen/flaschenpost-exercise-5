using flaschenpost_exercise_5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace flaschenpost_exercise_5.Controllers
{
    /// <summary>
    /// This ProductDataController contains business logic because I want to keep the scope of the take-home task limited.
    /// In regular projects the business logic should be moved from the controller to services.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductDataController : Controller
    {
        /// <summary>
        /// Reads product data from given URL and checks which articles are the cheapest as well as the most expensive per litre. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <returns>The cheapest and most expensive articles regarding the price per litre.</returns>
        [HttpGet]
        [Route("Article/min-max-price-per-litre")]
        public JsonResult GetArticlesWithMinAndMaxPricePerLitre(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            return GetArticlesWithMinAndMaxPricePerLitre(null, url);
        }

        [HttpGet]
        [Route("Article/min-max-price-per-litre")]
        internal JsonResult GetArticlesWithMinAndMaxPricePerLitre(ProductData[]? productData, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            productData ??= GetProductData(url);

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            // Traverses productData and collects best candidates for min and max prices in O(n) using little memory.
            var currentMinPrice = decimal.MaxValue;
            var currentMaxPrice = decimal.MinValue;
            var minPriceProducts = new List<ProductData>();
            var maxPriceProducts = new List<ProductData>();
            for (int productIndex = 0; productIndex < productData.Length; productIndex++)
            {
                for (int articleIndex = 0; articleIndex < productData[productIndex].Articles.Length; articleIndex++)
                {
                    // If there is no minPriceProduct candidate yet or the current article is tied with it's price
                    if (minPriceProducts.Count == 0 || productData[productIndex].Articles[articleIndex].PricePerUnit == minPriceProducts.First().Articles.First().PricePerUnit)
                    {
                        // add current product with only the current article to minPriceProducts without clearing minPriceProducts.
                        (minPriceProducts, currentMinPrice) = AddMinOrMaxPriceProduct(productData, minPriceProducts, currentMinPrice, productIndex, articleIndex);
                    }
                    // If the current article is cheaper than the so far best minPriceProduct candidates
                    else if (productData[productIndex].Articles[articleIndex].PricePerUnit < minPriceProducts.First().Articles.First().PricePerUnit)
                    {
                        // clear the candidates and add the current product with only the current article.
                        minPriceProducts.Clear();
                        (minPriceProducts, currentMinPrice) = AddMinOrMaxPriceProduct(productData, minPriceProducts, currentMinPrice, productIndex, articleIndex);
                    }

                    // If there is no maxPriceProduct candidate yet or the current article is tied with it's price
                    if (maxPriceProducts.Count == 0 || productData[productIndex].Articles[articleIndex].PricePerUnit == maxPriceProducts.First().Articles.First().PricePerUnit)
                    {
                        // add current product with only the current article to minPriceProducts without clearing maxPriceProducts.
                        (maxPriceProducts, currentMaxPrice) = AddMinOrMaxPriceProduct(productData, maxPriceProducts, currentMaxPrice, productIndex, articleIndex);
                    }
                    // If the current article is more expensive than the so far best maxPriceProduct candidates
                    else if (productData[productIndex].Articles[articleIndex].PricePerUnit > maxPriceProducts.First().Articles.First().PricePerUnit)
                    {
                        // clear the candidates and add the current product with only the current article.
                        maxPriceProducts.Clear();
                        (maxPriceProducts, currentMaxPrice) = AddMinOrMaxPriceProduct(productData, maxPriceProducts, currentMaxPrice, productIndex, articleIndex);
                    }
                } 
            }

            var minPriceArticleProducts = minPriceProducts.SelectMany(productData => productData.Articles.Select(article => (ProductData: productData, Article: article))).ToArray();
            var maxPriceArticleProducts = maxPriceProducts.SelectMany(productData => productData.Articles.Select(article => (ProductData: productData, Article: article))).ToArray();

            var minAndMaxPriceArticleProductViewModel = new ArticleMinMaxPriceByLitreViewModel
            {
                MinPrice = MapArticleProductToArticleProductViewModel(minPriceArticleProducts).ToArray(),
                MaxPrice = MapArticleProductToArticleProductViewModel(maxPriceArticleProducts).ToArray()
            };

            return minAndMaxPriceArticleProductViewModel.SerializeToJsonResult();

            static (List<ProductData>, decimal) AddMinOrMaxPriceProduct(ProductData[]? productData, List<ProductData> minOrMaxPriceProducts, decimal currentMinOrMaxPrice, int productIndex, int articleIndex)
            {
                var newMinOrMaxCopy = productData[productIndex].ShallowCopy();
                minOrMaxPriceProducts.Add(newMinOrMaxCopy);
                minOrMaxPriceProducts.Last().Articles = new[] { productData[productIndex].Articles[articleIndex] };
                currentMinOrMaxPrice = minOrMaxPriceProducts.First().Articles.First().PricePerUnit;

                return (minOrMaxPriceProducts, currentMinOrMaxPrice);
            }
        }

        /// <summary>
        /// Reads product data from given URL and checks which articles match a given price. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <param name="price">The price to check for.</param>
        /// <returns>The articles that match the given price ordered ascending by price per litre.</returns>
        [HttpGet]
        [Route("Article/price")]
        public JsonResult GetArticlesByPrice(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            return GetArticlesByPrice(null, url, price);
        }

        [HttpGet]
        [Route("Article/price")]
        internal JsonResult GetArticlesByPrice(ProductData[]? productData, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            productData ??= GetProductData(url);

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            var priceMatchingArticleProducts = 
                productData
                .SelectMany(productData => productData.Articles
                    .Where(article => article.Price == price)
                    .Select(article => (ProductData: productData, Article: article)))
                .OrderBy(articleProduct => articleProduct.Article.PricePerUnit)
                .ToArray();

            var priceMatchingArticleProductViewmodels = MapArticleProductToArticleProductViewModel(priceMatchingArticleProducts);

            return priceMatchingArticleProductViewmodels.SerializeToJsonResult();
        }

        /// <summary>
        /// Reads product data from given URL and checks which articles come in the most bottles. 
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <returns>The articles that come in the most bottles.</returns>
        [HttpGet]
        [Route("Article/most-bottles")]
        public JsonResult GetArticlesWithMostBottles(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            return GetArticlesWithMostBottles(null, url);
        }

        [HttpGet]
        [Route("Article/most-bottles")]
        internal JsonResult GetArticlesWithMostBottles(ProductData[]? productData = null, string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
        {
            productData ??= GetProductData(url);

            if (productData == null)
            {
                return new JsonResult("Invalid JSON or timeout.");
            }

            // Traverses productData and collects best candidates for most bottles in O(n) using little memory.
            var currentMaxBottles = 0;
            var maxBottlesProducts = new List<ProductData>();
            for (int productIndex = 0; productIndex < productData.Length; productIndex++)
            {
                for (int articleIndex = 0; articleIndex < productData[productIndex].Articles.Length; articleIndex++)
                {
                    // If there is no maxPriceProduct candidate yet or the current article is tied with it's price
                    if (maxBottlesProducts.Count == 0 || productData[productIndex].Articles[articleIndex].AmountBottles == maxBottlesProducts.First().Articles.First().AmountBottles)
                    {
                        // add current product with only the current article to minPriceProducts without clearing maxPriceProducts.
                        (maxBottlesProducts, currentMaxBottles) = AddMaxBottlesProduct(productData, maxBottlesProducts, currentMaxBottles, productIndex, articleIndex);
                    }
                    // If the current article is more expensive than the so far best maxPriceProduct candidates
                    else if (productData[productIndex].Articles[articleIndex].AmountBottles > maxBottlesProducts.First().Articles.First().AmountBottles)
                    {
                        // clear the candidates and add the current product with only the current article.
                        maxBottlesProducts.Clear();
                        (maxBottlesProducts, currentMaxBottles) = AddMaxBottlesProduct(productData, maxBottlesProducts, currentMaxBottles, productIndex, articleIndex);
                    }
                }
            }

            var maxAmountBottlesArticleProducts = maxBottlesProducts.SelectMany(productData => productData.Articles.Select(article => (ProductData: productData, Article: article))).ToArray();
            var maxAmountBottlesArticleProductViewModels = MapArticleProductToArticleProductViewModel(maxAmountBottlesArticleProducts);

            return maxAmountBottlesArticleProductViewModels.SerializeToJsonResult();

            static (List<ProductData>, int) AddMaxBottlesProduct(ProductData[]? productData, List<ProductData> maxBottlesProducts, int currentMaxBottles, int productIndex, int articleIndex)
            {
                var newMaxBottlesCopy = productData[productIndex].ShallowCopy();
                maxBottlesProducts.Add(newMaxBottlesCopy);
                maxBottlesProducts.Last().Articles = new[] { productData[productIndex].Articles[articleIndex] };
                currentMaxBottles = maxBottlesProducts.First().Articles.First().AmountBottles;

                return (maxBottlesProducts, currentMaxBottles);
            }
        }

        /// <summary>
        /// Calls productdata/article/min-max-price-per-litre, productdata/article/price, productdata/article/most-bottles and combines their results into a single result.
        /// </summary>
        /// <param name="url">The URL to read the product data from.</param>
        /// <param name="price">The price to check for.</param>
        /// <returns>The combined results.</returns>
        [HttpGet]
        [Route("Article/summary")]

        public JsonResult GetArticlesSummary(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json", decimal price = 17.99m)
        {
            var productData = GetProductData(url);

            var minAndMaxPricesJson = GetArticlesWithMinAndMaxPricePerLitre(productData, url);
            var matchingPriceJson = GetArticlesByPrice(productData, url, price);
            var mostBottlesJson = GetArticlesWithMostBottles(productData, url);

            var minAndMaxPricesModel = (ArticleMinMaxPriceByLitreViewModel)minAndMaxPricesJson.Value;
            var matchingPricesModel = ((IEnumerable<ArticleProductViewModel>)matchingPriceJson.Value).ToArray();
            var mostBottlesModel = ((IEnumerable<ArticleProductViewModel>)mostBottlesJson.Value).ToArray();

            var articleSummaryViewModel = new ArticleSummaryViewModel {
                MinAndMaxPricesPerLitre = minAndMaxPricesModel,
                ByPrice = matchingPricesModel,
                MostBottles = mostBottlesModel
            };

            return articleSummaryViewModel.SerializeToJsonResult();
        }

        private static ProductData[] GetProductData(string url = "https://flapotest.blob.core.windows.net/test/ProductData.json")
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

        private static IEnumerable<ArticleProductViewModel> MapArticleProductToArticleProductViewModel((ProductData ProductData, Article Article)[] articleProducts)
        {
            return articleProducts.Select(articleProduct => new ArticleProductViewModel
            {
                ProductId = articleProduct.ProductData.Id,
                ArticleId = articleProduct.Article.Id,
                ProductBrandName = articleProduct.ProductData.BrandName,
                ProductName = articleProduct.ProductData.Name,
                ProductDescription = articleProduct.ProductData.DescriptionText,
                ArticleShortDescription = articleProduct.Article.ShortDescription,
                Price = articleProduct.Article.Price,
                PricePerUnitText = articleProduct.Article.PricePerUnitText,
                Unit = articleProduct.Article.Unit,
                Image = articleProduct.Article.Image
            });
        }

    }
}

