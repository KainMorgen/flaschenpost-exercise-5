using flaschenpost_exercise_5.Controllers;
using flaschenpost_exercise_5.ViewModels;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProductDataTests
{
    public class ProductDataTests
    {
        ProductData[]? productData;

        [SetUp]
        public void Setup()
        {

            // HttpClient is intended to be instantiated once per application, rather than per-use.
            using var client = new HttpClient();
            var url = "https://flapotest.blob.core.windows.net/test/ProductData.json";
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var jsonString = response.Content.ReadAsStringAsync().Result;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            productData = JsonSerializer.Deserialize<ProductData[]>(jsonString, options);
        }

        [Test]
        public void ProductData_AllPricesInEuroPerLitre()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                var allPricesInEuroPerLitre = testProductData.All(p => p.Articles.All(a => a.PricePerUnitText.Contains("€/Liter")));

                Assert.That(allPricesInEuroPerLitre, Is.True);
            }
        }

        [Test]
        public void ProductData_AllPricesMatchRegex()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                // decimal price with comma and two following digits a space and '€/Liter' in parentheses. => (2,10 €/Liter)
                string pat = @"((^\()(\d+)\,(\d{2}))( €/Liter\))$";

                // Instantiate the regular expression object.
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);

                var allPricesMatchRegexPattern = testProductData.All(p => p.Articles.All(a => r.Match(a.PricePerUnitText).Success));

                Assert.That(allPricesMatchRegexPattern, Is.True);
            }
        }


        [Test]
        public void ProductData_ForeignCurrency()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                // TEST: change pricePerUnit of an article
                var krombacher = testProductData.Where(x => x.Id == 556).FirstOrDefault();
                if (krombacher != null)
                {
                    krombacher.Articles[0].PricePerUnitText = "(1,00 $/Liter)";
                    testProductData = testProductData.Except(testProductData.Where(x => x.Id == 556)).Append(krombacher).ToArray();
                }

                var allPricesInEuroPerLitre = testProductData.All(p => p.Articles.All(a => a.PricePerUnitText.Contains("€/Liter")));

                Assert.That(allPricesInEuroPerLitre, Is.False);
            }
        }

        [Test]
        public void ProductData_AllShortDescriptionsMatchRegex()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                // Amount of bottles  followed by space x space and the capacity in L followed by bottle material in parentheses. => 20 x 0,5L (Glas)
                string pat = @"((\d+) x (\d+)(,\d+)?)L \([A-Za-z]*\)$";

                // Instantiate the regular expression object.
                Regex r = new Regex(pat, RegexOptions.IgnoreCase);

                var allShortDescriptionsMatchRegexPattern = testProductData.All(p => p.Articles.All(a => r.Match(a.ShortDescription).Success));

                Assert.That(allShortDescriptionsMatchRegexPattern, Is.True);
            }
        }

        [Test]
        public void ProductData_MultipleCheapestAndMostExpensive()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                // TEST: change pricePerUnit of an article
                var krombacher = testProductData.Where(x => x.Id == 556).FirstOrDefault();
                if (krombacher != null)
                {
                    krombacher.Articles[0].PricePerUnitText = "(1,00 €/Liter)";
                    krombacher.Articles[1].PricePerUnitText = "(3,78 €/Liter)";
                    testProductData = testProductData.Except(testProductData.Where(x => x.Id == 556)).Append(krombacher).ToArray();
                }

                var productDataController = new ProductDataController();
                var response = productDataController.GetArticlesWithMinAndMaxPricePerLitre(testProductData);

                if (response != null && response.Value != null)
                {
                    ArticleMinMaxPriceByLitreViewModel responseProductData = (ArticleMinMaxPriceByLitreViewModel)response.Value;
                    var responseCount = responseProductData.MinPrice.Length + responseProductData.MaxPrice.Length;

                    Assert.That(responseCount, Is.EqualTo(4));
                }
            }
        }

        [Test]
        public void ProductData_MultipleMostBottles()
        {
            if (productData != null)
            {
                var testProductData = productData.ToArray();

                // TEST: change the amount of bottles of somes articles
                var krombacher = testProductData.Where(x => x.Id == 556).FirstOrDefault();
                if (krombacher != null)
                {
                    krombacher.Articles[0].ShortDescription = "99 x 0,5L (Glas)";
                    krombacher.Articles[1].ShortDescription = "99 x 0,5L (Glas)";
                    var krombacherArticles = new Article[] { krombacher.Articles[0], krombacher.Articles[1] };
                    krombacher.Articles = krombacherArticles;
                    testProductData = testProductData.Except(testProductData.Where(x => x.Id == 556)).Append(krombacher).ToArray();
                }

                var productDataController = new ProductDataController();
                var response = productDataController.GetArticlesWithMostBottles(testProductData);

                if (response != null && response.Value != null)
                {
                    ArticleProductViewModel[] responseProductData = ((IEnumerable<ArticleProductViewModel>)response.Value).ToArray();
                    var responseCount = responseProductData.Length;

                    Assert.That(responseCount, Is.EqualTo(2));
                }
            }
        }
    }
}