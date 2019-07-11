// ------------------------------------------------------------------------------
//     <copyright file="AuthorsController.cs" company="BlackLine">
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            IEnumerable<Author> authorEntities = _libraryRepository.GetAuthors();

            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorDtos);
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthors(Guid id)
        {
            Author authorEntity = _libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorEntity);

            return new JsonResult(author);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            _libraryRepository.AddAuthor(authorEntity);

            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute(RouteNames.GetAuthorRoute, new { id = authorToReturn.Id}, authorToReturn); //the Response also holds the URI for the newly created resource, so its ID as well.
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            Author authorEntity = _libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteAuthor(authorEntity);

            if (_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id} failed.");
            }

            return NoContent();
        }


    }
}