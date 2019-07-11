// ------------------------------------------------------------------------------
//     <copyright file="BookForCreationDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

namespace Library.API.Models
{
    public class BookForCreationDto
    {
        public string Description { get; set; }
        // Not a good idea to also add the AuthorId as a field for the book to create as it's already a part of the URI
        // and in the request body it might be different from the one in the URI.
        //public Guid AuthorId { get; set; }

        public string Title { get; set; }
    }
}