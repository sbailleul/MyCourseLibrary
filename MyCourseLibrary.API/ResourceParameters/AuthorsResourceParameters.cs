namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        private const int _maxPageSize = 20;
        private int _pageSize = 10;
        public string MainCategory { get; set; }
        public string SearchQuery { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { 
            get => _pageSize;
            set => _pageSize = value > _maxPageSize ? _maxPageSize : value;
        }
        
        public string Fields { get; set; }

        public string OrderBy { get; set; } = "Name";

    }
} 