// ------------------------------------------------------------------------------
//     <copyright file="BooksController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using AutoMapper;

using Library.API.Entities;
using Library.API.Enums;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetBooks(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            IEnumerable<Book> bookEntities = _libraryRepository.GetBooksForAuthor(authorId);

            var bookDtos = Mapper.Map<IEnumerable<BookDto>>(bookEntities);

            return Ok(bookDtos);
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public IActionResult GetBooks(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book book = _libraryRepository.GetBookForAuthor(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }

            var bookDto = Mapper.Map<BookDto>(book);

            return Ok(bookDto);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto bookDto)
        {
            if (bookDto == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(bookDto);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(RouteNames.GetBookForAuthor, new { authorId = authorId, id = bookToReturn.Id }, bookToReturn);
        }

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBook(Guid authorId, Guid bookId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book bookEntity = _libraryRepository.GetBookForAuthor(authorId, bookId);

            if (bookEntity == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {bookId} for author {authorId} failed.");
            }

            return NoContent();
        }
    }
}