namespace CRM_mvc.Utilities.Config
{
    public class Pagination
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public string? PreviousPage { get; set; }
        public string? NextPage { get; set; }

        public Pagination(int page, int pageSize, int total)
        {
            Page = page;
            PageSize = pageSize;
            Total = total;
            TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            PreviousPage = page > 1 ? $"?page={page - 1}&pageSize={pageSize}" : null;
            NextPage = page < TotalPages ? $"?page={page + 1}&pageSize={pageSize}" : null;
        }
    }
}
