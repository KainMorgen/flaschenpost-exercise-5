using System;

namespace flaschenpost_exercise_5.ViewModels
{
    /// <summary>
    /// Product data
    /// </summary>
    public class ProductData
    {
        /// <summary>
        /// Product id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product brand name
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Articles / variants the product is sold by
        /// </summary>
        public Article[] Articles { get; set; }

        /// <summary>
        /// Optional description text of the product. 
        /// </summary>
        public string DescriptionText { get; set; }

        public ProductData ShallowCopy()
        {
            return (ProductData)MemberwiseClone();
        }
    }
}
