using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models
{
    public static class FakeData
    {

        public static readonly List<Book> Books = new()
        {
            new Book
            {
                Id = 1,
                Title = "Banga",
                Author = "J. Autorius",
                Year = 2021,
            },
            new Book
            {
                Id = 2,
                Title = "Bitės kelias",
                Author = "A. Bitininkas",
                Year = 2019,
            },
            new Book
            {
                Id = 3,
                Title = "Atlasas",
                Author = "R. Kartografas",
                Year = 2023,
            }
        };
    }
}