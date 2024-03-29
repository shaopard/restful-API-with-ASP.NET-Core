﻿// ------------------------------------------------------------------------------
//     <copyright file="AuthorDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;

namespace Library.API.Models
{
    public class AuthorDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public string Genre { get; set; }
    }
}