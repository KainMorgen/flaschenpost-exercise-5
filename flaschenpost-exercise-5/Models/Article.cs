using System.Runtime.Serialization;

namespace flaschenpost_exercise_5.ViewModels
{
    public class Article
    {
        /// <summary>
        /// Article Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The brand name of the article's product.
        /// </summary>
        public string? ProductBrandName { get; set; }
        
        /// <summary>
        /// The name of the article's product.
        /// </summary>
        public string? ProductName { get; set; }

        /// <summary>
        /// Describes the articles amount of units, capacity per unit and the material of containers. 
        /// </summary>
        public string ShortDescription { get; set; }
        
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

        /// <summary>
        /// The price per Unit converted to decimal.
        /// Assumes that all PricePerUnitText match the same pattern.
        /// Assumption is covered by a unit test.
        /// </summary>
        [IgnoreDataMember]
        public decimal PricePerUnit => Decimal.Parse(PricePerUnitText.Replace("(", "").Substring(0, PricePerUnitText.IndexOf(" ")));

        /// <summary>
        /// The amount of bottles as integer obtained from the ShortDescription.
        /// Assumptions on the pattern of the ShortDescription are covered by a unit test.
        /// </summary>
        [IgnoreDataMember]
        public int AmountBottles => int.Parse(ShortDescription.Substring(0, ShortDescription.IndexOf(" ")));
    }
}
