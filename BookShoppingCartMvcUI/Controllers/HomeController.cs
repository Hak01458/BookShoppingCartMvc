using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookShoppingCartMvcUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _homeRepository = homeRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sterm = "", int genreId = 0, int step = 1)
        {
            // get all books from repository
            var books = (await _homeRepository.GetBooks(sterm, genreId)).ToList();
            var genres = await _homeRepository.Genres();

            // If a step > 1 is selected, show every Nth book (0-based index)
            IEnumerable<Book> displayedBooks = books;
            if (step > 1)
            {
                displayedBooks = books.Where((b, idx) => idx % step == 0).ToList();
            }

            BookDisplayModel bookModel = new BookDisplayModel
            {
                Books = displayedBooks,
                Genres = genres,
                STerm = sterm,
                GenreId = genreId
            };

            // map displayed Books -> domain cart items so view doesn't need to map
            var items = displayedBooks.Select(b => new BookShoppingCartMvcUI.Domain.BookLeaf(
                b.Id,
                b.BookName,
                (decimal)b.Price,
                b.Quantity,
                b.AuthorName,
                b.GenreName,
                b.Image)).ToList<BookShoppingCartMvcUI.Domain.ICartItem>();
            bookModel.Items = items;

            ViewData["IteratorStep"] = step;
            return View(bookModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}