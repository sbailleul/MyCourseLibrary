namespace CourseLibrary.API.Helpers.Pagination
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }


        public static PaginationMetadata Create<T>(PagedList<T> pagedList)
        {
            var metadata = new PaginationMetadata
            {
                CurrentPage = pagedList.CurrentPage,
                TotalPages = pagedList.TotalPages,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
            };
            return metadata;
        }
    }
}