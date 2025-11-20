using System;

namespace InformacinesSistemos.Models
{
    public class BookSimple
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public int Year { get; set; }
    }
}