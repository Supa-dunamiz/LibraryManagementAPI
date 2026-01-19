using AutoMapper;
using LibraryManagement.Data;
using LibraryManagement.Entities;
using LibraryManagement.Objs;
using LibraryManagement.Objs.LibraryManagement.Objs;
using LibraryManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _repo;

        public BooksController(IBookRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a new book record.
        /// Business logic:
        /// - Validate the incoming DTO in the controller (basic null check).
        /// - Delegate full validation and persistence to the repository.
        /// - On success return 201 Created pointing to GetById; on failure return 400 Bad Request with message.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (dto is null) return BadRequest("Invalid book dto");

            var result = await _repo.CreateAsync(dto);

            BookReadDto book = result.Data;

            // If creation succeeded result.Data contains the created resource DTO,
            // otherwise return a BadRequest with the repository-provided message.
            return book is not null
                    ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data)
                    : BadRequest(result.StatusMessage);
        }

        /// <summary>
        /// Retrieve a paged list of books with optional search.
        /// Business logic:
        /// - Accepts optional `search` and paging parameters (pageNumber, pageSize).
        /// - Delegates filtering, paging and total count calculation to repository.
        /// - Returns 200 OK with PagedResult on success, or 400 BadRequest if repository reports an error.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _repo.GetAllAsync(search, pageNumber, pageSize);
            if (result.Data == null) return BadRequest(result.StatusMessage);

            PagedResult pagedResult = result.Data;

            // Return the paged result which includes Items, Total, PageNumber and PageSize.
            return Ok(pagedResult);
        }

        /// <summary>
        /// Retrieve a single book by id.
        /// Business logic:
        /// - Validate id (cannot be 0).
        /// - Ask repository for the entity; return 404 if not found.
        /// - On success return 200 with the BookReadDto.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0) return BadRequest("Book id cannot be 0");

            var result = await _repo.GetByIdAsync(id);

            if (result.Data == null) return NotFound(result.StatusMessage);

            return Ok(result.Data);
        }

        /// <summary>
        /// Update an existing book.
        /// Business logic:
        /// - Validate request basic parameters (id and dto presence).
        /// - Delegate validation and partial-update logic to the repository.
        /// - Repository returns a boolean Data flag indicating success.
        /// - If Data == true, return 200 OK; if false, return 404 Not Found (resource missing) with message.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, BookDto dto)
        {
            if (id <= 0 || dto is null)
                return BadRequest("Invalid request.");

            var result = await _repo.UpdateAsync(id, dto);

            // if Data == true then operation was successful 
            return result.Data
                ? Ok(result.StatusMessage)
                : NotFound(result.StatusMessage);
        }

        /// <summary>
        /// Delete a book by id.
        /// Business logic:
        /// - Validate id.
        /// - Delegate delete operation to repository.
        /// - Repository returns boolean indicating whether deletion succeeded.
        /// - Success => 200 OK, Failure (not found) => 404 Not Found.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            var result = await _repo.DeleteAsync(id);

            // if Data == true then operation was successful 
            return result.Data
                ? Ok(result.StatusMessage)
                : NotFound(result.StatusMessage);
        }

    }
}
