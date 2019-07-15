// ------------------------------------------------------------------------------
//     <copyright file="BooksController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

using AutoMapper;

using Library.API.Entities;
using Library.API.Enums;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository c_libraryRepository;

        private readonly ILogger<BooksController> c_logger;

        public BooksController(ILibraryRepository libraryRepository, ILogger<BooksController> logger)
        {
            c_logger = logger;
            c_libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto bookDto)
        {
            if (bookDto == null)
            {
                return BadRequest();
            }

            if (bookDto.Description == bookDto.Title)
            {
                ModelState.AddModelError(nameof(BookForCreationDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                // return 422;
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(bookDto);

            c_libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!c_libraryRepository.Save())
            {
                throw new Exception($"Creating a book for author {authorId} failed on save.");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            return CreatedAtRoute(
                RouteNames.GetBookForAuthor,
                new
                {
                    authorId = authorId,
                    id = bookToReturn.Id
                },
                bookToReturn);
        }

        [HttpDelete("{bookId}")]
        public IActionResult DeleteBook(Guid authorId, Guid bookId)
        {
            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book bookEntity = c_libraryRepository.GetBookForAuthor(authorId, bookId);

            if (bookEntity == null)
            {
                return NotFound();
            }

            c_libraryRepository.DeleteBook(bookEntity);

            if (!c_libraryRepository.Save())
            {
                throw new Exception($"Deleting book {bookId} for author {authorId} failed.");
            }
            c_logger.LogInformation((int) HttpStatusCode.Continue, $"Book {bookId} for author {authorId} was deleted.");

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetBooks(Guid authorId)
        {
            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            IEnumerable<Book> bookEntities = c_libraryRepository.GetBooksForAuthor(authorId);

            var bookDtos = Mapper.Map<IEnumerable<BookDto>>(bookEntities);

            return Ok(bookDtos);
        }

        [HttpGet("{bookId}", Name = "GetBookForAuthor")]
        public IActionResult GetBooks(Guid authorId, Guid bookId)
        {
            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book book = c_libraryRepository.GetBookForAuthor(authorId, bookId);

            if (book == null)
            {
                return NotFound();
            }

            var bookDto = Mapper.Map<BookDto>(book);

            return Ok(bookDto);
        }

        [HttpPatch("{bookId}")]
        public IActionResult PartiallyUpdateBookForAuthor(Guid authorId, Guid bookId, [FromBody] JsonPatchDocument<BookForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book bookEntity = c_libraryRepository.GetBookForAuthor(authorId, bookId);

            if (bookEntity == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDoc.ApplyTo(bookDto);

                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = bookId;

                c_libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!c_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {bookId} for author {authorId} failed when saving.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute(
                    "GetBookForAuthor",
                    new
                    {
                        authorId = authorId,
                        bookId = bookToReturn.Id
                    },
                    bookToReturn);
            }

            var bookToPatch = Mapper.Map<BookForUpdateDto>(bookEntity);

            patchDoc.ApplyTo(bookToPatch);

            Mapper.Map(bookToPatch, bookEntity);

            c_libraryRepository.UpdateBookForAuthor(bookEntity);

            if (!c_libraryRepository.Save())
            {
                throw new Exception($"Failed updating book {bookId} for author {authorId} failed when saving.");
            }

            return Ok(bookEntity);
        }

        [HttpPut("{bookId}")]
        public IActionResult UpdateBookForAuthor(Guid authorId, Guid bookId, [FromBody] BookForUpdateDto bookDto)
        {
            if (bookDto == null)
            {
                return BadRequest();
            }

            if (bookDto.Description == bookDto.Title)
            {
                ModelState.AddModelError(nameof(BookForUpdateDto), "The provided description should be different from the title.");
            }

            if (!ModelState.IsValid)
            {
                // return 422;
                return new UnprocessableEntityObjectResult(ModelState);
            }

            if (!c_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            Book bookEntity = c_libraryRepository.GetBookForAuthor(authorId, bookId);

            if (bookEntity == null)
            {
                var bookToAdd = Mapper.Map<Book>(bookDto);
                bookToAdd.Id = bookId;

                c_libraryRepository.AddBookForAuthor(authorId, bookToAdd);

                if (!c_libraryRepository.Save())
                {
                    throw new Exception($"Upserting book {bookId} for author {authorId} failed when saving.");
                }

                var bookToReturn = Mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute(
                    "GetBookForAuthor",
                    new
                    {
                        authorId = authorId,
                        bookId = bookToReturn.Id
                    },
                    bookToReturn);
            }

            Mapper.Map(bookDto, bookEntity);

            c_libraryRepository.UpdateBookForAuthor(bookEntity);

            if (!c_libraryRepository.Save())
            {
                throw new Exception($"Failed updating book {bookId} for author {authorId} failed when saving.");
            }

            return Ok(bookEntity);
        }
    }
}