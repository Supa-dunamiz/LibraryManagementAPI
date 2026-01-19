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

        /// <summary>
        /// Create a new book record.
        /// Business logic:
        /// - Validate incoming DTO (delegates to ValidateBookDtoAsync).
        /// - If validation fails, return the validation message without touching the database.
        /// - If valid, map DTO -> Entity, save to DB and return a read DTO.
        /// - Exceptions are logged and returned as error messages in the operation result.
        /// </summary>
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

        /// <summary>
        /// Retrieve paged list of books with optional search.
        /// Business logic:
        /// - Validate paging params (pageNumber must be > 0).
        /// - Apply optional search filter against Title and Author.
        /// - Return total count and the paged items (DTOs).
        /// - Uses AsNoTracking for read-only performance.
        /// </summary>
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

        /// <summary>
        /// Retrieve a single book by id.
        /// Business logic:
        /// - Validate id (must be positive).
        /// - Use FindAsync which returns null if not found.
        /// - Map to DTO and return or return not-found message.
        /// </summary>
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

        /// <summary>
        /// Update a book. Only supplied fields on the DTO are applied.
        /// Business logic:
        /// - Validate DTO for update semantics (id present and at least one updatable field).
        /// - If validation fails, return message without DB changes.
        /// - Retrieve existing entity, apply only provided fields, and save.
        /// - Any exception is logged and returned as an error message.
        /// </summary>
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

        /// <summary>
        /// Delete a book by id.
        /// Business logic:
        /// - Validate id, attempt to find entity, remove and persist.
        /// - Return appropriate success/failure message.
        /// - Exceptions are logged and surfaced as error messages.
        /// </summary>
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

        // Simple mapper from entity to read DTO. Kept private to encapsulate mapping logic.
        private static BookReadDto ToReadDto(Book b) =>
            new BookReadDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                PublishedDate = b.PublishedDate
            };

        /// <summary>
        /// Validate the incoming BookDto for both create and update scenarios.
        /// Business logic:
        /// - For create: all fields are required.
        /// - For update: id must be valid and at least one updatable field must be supplied.
        /// - ISBN uniqueness is enforced only when an ISBN value is provided.
        /// - Returns a boolean operation result with a message describing validation state.
        /// </summary>
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
