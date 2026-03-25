using BookShoppingCartMvcUI.Domain;

namespace BookShoppingCartMvcUI.Models.DTOs
{
    public class BookDisplayModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public string STerm { get; set; } = "";
        public int GenreId { get; set; } = 0;

        // Prepared cart/domain items mapped from Books (populated in controller)
        public IList<ICartItem> Items { get; set; } = new List<ICartItem>();
    }
}
