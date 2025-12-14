using InformacinesSistemos.Models;

namespace InformacinesSistemos.ViewModels
{
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public List<Book> Results { get; set; } = new();

        public bool ShowResults { get; set; }
    }
}