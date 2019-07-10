// ------------------------------------------------------------------------------
//     <copyright file="AuthorsController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

using AutoMapper;

using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;

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

        [HttpGet("{id}")]
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
    }
}