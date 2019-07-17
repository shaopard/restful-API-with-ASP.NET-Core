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
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private ILibraryRepository c_libraryRepository;
        private IUrlHelper c_urlHelper;
        private IPropertyMappingService c_propertyMappingService;
        private ITypeHelperService c_typeHelperService;

        public AuthorsController(ILibraryRepository libraryRepository,
                                 IUrlHelper urlHelper,
                                 IPropertyMappingService propertyMappingService,
                                 ITypeHelperService typeHelperService)
        {
            c_libraryRepository = libraryRepository;
            c_urlHelper = urlHelper;
            c_propertyMappingService = propertyMappingService;
            c_typeHelperService = typeHelperService;
        }

        [HttpPost("{id}")]
        public IActionResult BlockAuthorCreation(Guid id)
        {
            if (c_libraryRepository.AuthorExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntity = Mapper.Map<Author>(author);

            c_libraryRepository.AddAuthor(authorEntity);

            if (!c_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save.");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute(
                RouteNames.GetAuthorRoute,
                new
                {
                    id = authorToReturn.Id
                },
                authorToReturn); //the Response also holds the URI for the newly created resource, so its ID as well.
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthor(Guid id)
        {
            Author authorEntity = c_libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            c_libraryRepository.DeleteAuthor(authorEntity);

            if (c_libraryRepository.Save())
            {
                throw new Exception($"Deleting author {id} failed.");
            }

            return NoContent();
        }

        [HttpGet(Name = "GetAuthors")]
        // public IActionResult GetAuthors([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        public IActionResult GetAuthors(AuthorsResourceParameters authorsResourceParameters) // Frameworkul stie sa faca bindingul la query string parameters la proprietati din clasa asta.
        {
            // if (!c_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>
            //         (authorsResourceParameters.OrderBy))
            // {
            //     return BadRequest();
            // }

            if (!c_typeHelperService.TypeHasProperties<AuthorDto>
                    (authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            PagedList<Author> authorEntities = c_libraryRepository.GetAuthors(authorsResourceParameters);

            string previousPageLink = authorEntities.HasPrevious ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;

            string nextPageLink = authorEntities.HasNext ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = authorEntities.TotalCount,
                pageSize = authorEntities.PageSize,
                currentPage = authorEntities.CurrentPage,
                totalPages = authorEntities.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };
            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorDtos.ShapeData(authorsResourceParameters.Fields));
        }

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthors(Guid id, [FromQuery] string fields)
        {
            if (!c_typeHelperService.TypeHasProperties<AuthorDto>
                    (fields))
            {
                return BadRequest();
            }

            Author authorEntity = c_libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            var author = Mapper.Map<AuthorDto>(authorEntity);
            return Ok(author.ShapeData(fields));
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType resourceType)
        {
            switch (resourceType)
            {
                case ResourceUriType.PreviousPage:
                    return c_urlHelper.Link(
                        "GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber - 1,
                            pageSize = authorsResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return c_urlHelper.Link(
                        "GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber + 1,
                            pageSize = authorsResourceParameters.PageSize
                        });

                default:
                    return c_urlHelper.Link(
                        "GetAuthors",
                        new
                        {
                            fields = authorsResourceParameters.Fields,
                            orderBy = authorsResourceParameters.OrderBy,
                            searchQuery = authorsResourceParameters.SearchQuery,
                            genre = authorsResourceParameters.Genre,
                            pageNumber = authorsResourceParameters.PageNumber,
                            pageSize = authorsResourceParameters.PageSize
                        });
            }
        }
    }
}