namespace flaschenpost_exercise_5.Models
{
    public class CombinedProductDataResult
    {
        /// <summary>
        /// The called API action
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// The articles / variants returned by that action
        /// </summary>
        public Article[]? Results { get; set; }
    }
}
