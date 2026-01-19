namespace LibraryManagement.Objs
{
    namespace LibraryManagement.Objs
    {
        public class PagedResult
        {
            public IEnumerable<BookReadDto> Items { get; set; } = Enumerable.Empty<BookReadDto>();
            public int Total { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
    }

}
