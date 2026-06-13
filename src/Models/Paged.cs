namespace SdtdServerKit.Models
{
    /// <summary>
    /// Paged result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Paged<T>
    {
        /// <summary>
        /// Items
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Total
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="items"></param>
        /// <param name="total"></param>
        public Paged(IEnumerable<T> items, int total)
        {
            Items = items;
            Total = total;
        }
    }
}
