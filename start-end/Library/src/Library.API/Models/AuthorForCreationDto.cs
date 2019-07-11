// ------------------------------------------------------------------------------
//     <copyright file="AuthorForCreationDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Library.API.Models
{
    public class AuthorForCreationDto
    {
        public DateTimeOffset DateOfBirth { get; set; }

        public string FirstName { get; set; }

        public string Genre { get; set; }

        public string LastName { get; set; }

        public ICollection<BookForCreationDto> Books { get; set; } = new List<BookForCreationDto>();
    }
}