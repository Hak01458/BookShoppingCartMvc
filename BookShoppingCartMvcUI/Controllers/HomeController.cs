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

        public async Task<IActionResult> Index(string sterm = "", int genreId = 0, int step = 1, int page = 1)
        {
            // get all books from repository and genres
            var books = (await _homeRepository.GetBooks(sterm, genreId)).ToList();
            var genres = await _homeRepository.Genres();

            // apply iterator step (filter) first
            IEnumerable<Book> filtered = books;
            if (step > 1)
            {
                filtered = books.Where((b, idx) => idx % step == 0).ToList();
            }

            // pagination parameters
            const int pageSize = 15;
            var totalBooks = filtered.Count();
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages == 0 ? 1 : totalPages;

            var pagedBooks = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            BookDisplayModel bookModel = new BookDisplayModel
            {
                Books = pagedBooks,
                Genres = genres,
                STerm = sterm,
                GenreId = genreId
            };

            // map displayed (paged) Books -> domain cart items so view doesn't need to map
            var items = pagedBooks.Select(b => new BookShoppingCartMvcUI.Domain.BookLeaf(
                b.Id,
                b.BookName,
                (decimal)b.Price,
                b.Quantity,
                b.AuthorName,
                b.GenreName,
                b.Image)).ToList<BookShoppingCartMvcUI.Domain.ICartItem>();
            bookModel.Items = items;

            ViewData["IteratorStep"] = step;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            return View(bookModel);
        }

        // Contact page (GET)
        public IActionResult Contact()
        {
            return View(new Models.ViewModels.ContactFormViewModel());
        }

        // Contact form POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IndexContact(Models.ViewModels.ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Contact", model);
            }

            // TODO: save message/send email. For now, set TempData and redirect
            TempData["ContactSuccess"] = "Cảm ơn bạn đã gửi tin. Chúng tôi sẽ liên hệ sớm.";
            return RedirectToAction("Contact");
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