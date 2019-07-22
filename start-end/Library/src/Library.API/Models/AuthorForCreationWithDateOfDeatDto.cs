// ------------------------------------------------------------------------------
//     <copyright file="AuthorForCreationWithDateOfDeatDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;

namespace Library.API.Models
{
    public class AuthorForCreationWithDateOfDeathDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public DateTimeOffset? DateOfDeath { get; set; }
        public string Genre { get; set; }
    }
}