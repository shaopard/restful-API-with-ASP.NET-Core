// ------------------------------------------------------------------------------
//     <copyright file="AuthorCollectionsController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
        private ILibraryRepository _libraryRepository;

        public AuthorCollectionsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpPost]
        public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorDtos)
        {
            if (authorDtos == null)
            {
                return BadRequest();
            }

            var authorEntities = Mapper.Map<IEnumerable<Author>>(authorDtos);

            foreach (Author authorEntity in authorEntities)
            {
                _libraryRepository.AddAuthor(authorEntity);
            }

            if (!_libraryRepository.Save())
            { 
                throw new Exception($"Creating an author collection failed on save.");
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            string idsAsString = string.Join(",", authorsToReturn.Select(author => author.Id));

            return CreatedAtRoute("GetAuthorCollection", new { ids = idsAsString }, authorsToReturn);
        }

        [HttpGet("({ids})", Name = "GetAuthorCollection")]
        public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest(); 
            }

            IEnumerable<Author> authorEntities = _libraryRepository.GetAuthors(ids);

            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            return Ok(authorsToReturn);
        }

    }
}