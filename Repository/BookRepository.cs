using LibraryManagement.Data;
using LibraryManagement.Entities;
using LibraryManagement.Objs;
using LibraryManagement.Objs.LibraryManagement.Objs;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Repository
{
    public interface IBookRepository
    {
        Task<OperationResult<BookReadDto>> CreateAsync(BookDto dto);
        Task<OperationResult<PagedResult>> GetAllAsync(string? search, int pageNumber, int pageSize);
        Task<OperationResult<BookReadDto?>> GetByIdAsync(int id);
        Task<OperationResult<bool>> UpdateAsync(int id, BookDto dto);
        Task<OperationResult<bool>> DeleteAsync(int id);
    }

    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _ctx;
        private readonly ILogger<AuthService> _logger;


        public BookRepository(AppDbContext ctx, ILogger<AuthService> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        public async Task<OperationResult<BookReadDto>> CreateAsync(BookDto dto)
        {
            var validationResult = await ValidateBookDtoAsync(dto);
            if (!validationResult.Data)
            {
                return new OperationResult<BookReadDto>
                {
                    StatusMessage = validationResult.StatusMessage,
                    Data = null
                };
            }

            try
            {
                var book = new Book
                {
                    Title = dto.Title.Trim(),
                    Author = dto.Author.Trim(),
                    ISBN = dto.ISBN.Trim(),
                    PublishedDate = dto.PublishedDate
                };

                _ctx.Books.Add(book);
                await _ctx.SaveChangesAsync();

                return new OperationResult<BookReadDto>("Book created successfully.", ToReadDto(book));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<BookReadDto>($"An error occurred while creating the book: {ex.Message}", null);
            }
        }

        public async Task<OperationResult<PagedResult>> GetAllAsync(string? search, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                return new OperationResult<PagedResult>("pageNumber must be greater than 0.", null);

            try
            {
                var query = _ctx.Books.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();
                    query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
                }

                var total = await query.CountAsync();

                var items = await query
                    .OrderBy(b => b.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .Select(b => ToReadDto(b))
                    .ToListAsync();

                var paged = new PagedResult
                {
                    Items = items,
                    Total = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return new OperationResult<PagedResult>("Books retrieved successfully.", paged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<PagedResult>($"An error occurred while retrieving books: {ex.Message}", null);
            }
        }

        public async Task<OperationResult<BookReadDto?>> GetByIdAsync(int id)
        {
            if (id <= 0)
                return new OperationResult<BookReadDto?>("Invalid book id.", null);

            try
            {
                var book = await _ctx.Books.FindAsync(id);
                if (book == null)
                    return new OperationResult<BookReadDto?>("Book not found.", null);

                return new OperationResult<BookReadDto?>("Book retrieved successfully.", ToReadDto(book));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<BookReadDto?>($"An error occurred while retrieving the book: {ex.Message}", null);
            }
        }

        public async Task<OperationResult<bool>> UpdateAsync(int id, BookDto dto)
        {
            // Validate incoming DTO (including the "no field supplied" rule)
            var validationResult = await ValidateBookDtoAsync(dto, isUpdate: true, id: id);

            if (!validationResult.Data)
            {
                return new OperationResult<bool>
                {
                    StatusMessage = validationResult.StatusMessage,
                    Data = false
                };
            }

            try
            {
                var book = await _ctx.Books.FindAsync(id);
                if (book == null)
                    return new OperationResult<bool>("Book not found.", false);

                // Update only fields that were supplied
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    book.Title = dto.Title.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Author))
                    book.Author = dto.Author.Trim();

                if (!string.IsNullOrWhiteSpace(dto.ISBN))
                    book.ISBN = dto.ISBN.Trim();

                if (dto.PublishedDate != default)
                    book.PublishedDate = dto.PublishedDate;

                await _ctx.SaveChangesAsync();

                return new OperationResult<bool>("Book updated successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<bool>($"An error occurred while updating the book: {ex.Message}", false);
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
                return new OperationResult<bool>("Invalid book id.", false);

            try
            {
                var book = await _ctx.Books.FindAsync(id);
                if (book == null)
                    return new OperationResult<bool>("Book not found.", false);

                _ctx.Books.Remove(book);
                await _ctx.SaveChangesAsync();

                return new OperationResult<bool>("Book deleted successfully.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred");
                return new OperationResult<bool>($"An error occurred while deleting the book: {ex.Message}", false);
            }
        }

        private static BookReadDto ToReadDto(Book b) =>
            new BookReadDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                PublishedDate = b.PublishedDate
            };

        private async Task<OperationResult<bool>> ValidateBookDtoAsync(BookDto dto, bool isUpdate = false, int? id = null)
        {

            // Common validations
            if (dto == null)
                return new OperationResult<bool>("Request body is required.", false);

            if (isUpdate)
            {
                if (id == null || id <= 0)
                    return new OperationResult<bool>("Invalid book id.", false);

                if (string.IsNullOrWhiteSpace(dto.Title) && string.IsNullOrWhiteSpace(dto.Author)
                    && string.IsNullOrWhiteSpace(dto.ISBN) && dto.PublishedDate == default)
                    return new OperationResult<bool>("No valid field supplied for update", false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Title))
                    return new OperationResult<bool>("Title is required.", false);

                if (string.IsNullOrWhiteSpace(dto.Author))
                    return new OperationResult<bool>("Author is required.", false);

                if (string.IsNullOrWhiteSpace(dto.ISBN))
                    return new OperationResult<bool>("ISBN is required.", false);

                if (dto.PublishedDate == default)
                    return new OperationResult<bool>("PublishedDate is required.", false);
            }

            // ISBN uniqueness:
            // For create → check if any record already has this ISBN  
            // For update → exclude the current book id
            if (!string.IsNullOrWhiteSpace(dto.ISBN))
            {
                bool isbnExists = await _ctx.Books
                    .AnyAsync(b => b.ISBN == dto.ISBN && (!isUpdate || b.Id != id));

                if (isbnExists)
                    return new OperationResult<bool>("A book with the same ISBN already exists.", false);
            }

            return new OperationResult<bool>("Valid.", true);
        }

    }

}
