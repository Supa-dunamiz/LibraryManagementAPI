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

        [HttpPost]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (dto is null) return BadRequest("Invalid book dto");

            var result = await _repo.CreateAsync(dto);

            BookReadDto book = result.Data;

            return book is not null
                    ? CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data)
                    : BadRequest(result.StatusMessage);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, int pageNumber = 1, int pageSize = 10)
        {
            var result = await _repo.GetAllAsync(search, pageNumber, pageSize);
            if (result.Data == null) return BadRequest(result.StatusMessage);

            PagedResult pagedResult = result.Data;

            return Ok(pagedResult);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0) return BadRequest("Book id cannot be 0");

            var result = await _repo.GetByIdAsync(id);

            if (result.Data == null) return NotFound(result.StatusMessage);

            return Ok(result.Data);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, BookDto dto)
        {
            if (id <= 0 || dto is null)
                return BadRequest("Invalid request.");

            var result = await _repo.UpdateAsync(id, dto);

            //if Data == true then operation was successful 
            return result.Data
                ? Ok(result.StatusMessage)
                : NotFound(result.StatusMessage);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid id.");

            var result = await _repo.DeleteAsync(id);

            //if Data == true then operation was successful 
            return result.Data
                ? Ok(result.StatusMessage)
                : NotFound(result.StatusMessage);
        }

    }
}
