using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories
{
    public interface IBookRepository
    {
        Task AddBook(Book book);
        Task DeleteBook(Book book);
        Task<Book?> GetBookById(int id);
        Task<IEnumerable<Book>> GetBooks();
        Task UpdateBook(Book book);
    }

    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;
        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBook(Book book)
        {
            // Load existing entity and update specific fields to avoid inserting a new record
            var existing = await _context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
            if (existing == null)
                throw new InvalidOperationException($"Book with id {book.Id} not found");

            existing.BookName = book.BookName;
            existing.AuthorName = book.AuthorName;
            existing.Price = book.Price;
            existing.Image = book.Image;
            existing.GenreId = book.GenreId;

            _context.Books.Update(existing);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBook(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task<Book?> GetBookById(int id) => await _context.Books.Include(b => b.Genre).FirstOrDefaultAsync(b => b.Id == id);

        public async Task<IEnumerable<Book>> GetBooks() => await _context.Books.Include(a => a.Genre).OrderByDescending(b => b.Id).ToListAsync();
    }
}
