// ------------------------------------------------------------------------------
//     <copyright file="BookDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;

namespace Library.API.Models
{
    public class BookDto : LinkedResourceBaseDto
    {
        public string Description { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public Guid AuthorId { get; set; }
    }
}