// ------------------------------------------------------------------------------
//     <copyright file="BookForManipulationDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [MaxLength(500, ErrorMessage = "The description shouldn't exceed 500 characters.")]
        public virtual string Description { get; set; }
        // Not a good idea to also add the AuthorId as a field for the book to create as it's already a part of the URI
        // and in the request body it might be different from the one in the URI.
        //public Guid AuthorId { get; set; }

        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(100, ErrorMessage = "The title shouldn't exceed 100 characters.")]
        public string Title { get; set; }
    }
}