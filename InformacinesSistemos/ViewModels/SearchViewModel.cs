using InformacinesSistemos.Models;

namespace InformacinesSistemos.ViewModels
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public List<Book> Results { get; set; } = new();

        public bool ShowResults { get; set; }
        public Dictionary<int, RatingSummaryVm> RatingByBookId { get; set; } = new();

        public class RatingSummaryVm
        {
            public int AvgRounded { get; set; }
            public int Count { get; set; }
        }
        public List<FilterOption> CategoryOptions { get; set; } = new();
        public List<FilterOption> AuthorOptions { get; set; } = new();

        public int[] SelectedCategoryIds { get; set; } = Array.Empty<int>();
        public int[] SelectedAuthorIds { get; set; } = Array.Empty<int>();

        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
    }
    public class FilterOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}