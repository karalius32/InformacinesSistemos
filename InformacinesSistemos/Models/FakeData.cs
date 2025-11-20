using System;
using System.Collections.Generic;

namespace InformacinesSistemos.Models
{
    public static class FakeData
    {

        public static readonly List<BookSimple> Books = new()
        {
            new BookSimple
            {
                Id = 1,
                Title = "Banga",
                Author = "J. Autorius",
                Year = 2021,
            },
            new BookSimple
            {
                Id = 2,
                Title = "Bitės kelias",
                Author = "A. Bitininkas",
                Year = 2019,
            },
            new BookSimple
            {
                Id = 3,
                Title = "Atlasas",
                Author = "R. Kartografas",
                Year = 2023,
            }
        };
    }
}