// ------------------------------------------------------------------------------
//     <copyright file="AuthorsController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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
        private readonly ILibraryRepository c_libraryRepository;

        private readonly IPropertyMappingService c_propertyMappingService;

        private readonly ITypeHelperService c_typeHelperService;

        private readonly IUrlHelper c_urlHelper;

        public AuthorsController(ILibraryRepository libraryRepository, IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
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
        [RequestHeaderMatchesMediaType("Content-Type", new[] { "application/vnd.marvin.author.full+json" })]
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

            var authorDto = Mapper.Map<AuthorDto>(authorEntity);

            IEnumerable<LinkDto> links = CreateLinksForAuthor(authorDto.Id, null);

            var linkedResourcesToReturn = authorDto.ShapeData(null) as IDictionary<string, object>;
            linkedResourcesToReturn.Add("links", links);

            return CreatedAtRoute(
                RouteNames.GetAuthorRoute,
                new
                {
                    id = linkedResourcesToReturn["Id"]
                },
                linkedResourcesToReturn); //the Response also holds the URI for the newly created resource, so its ID as well.
        }

        [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
        [RequestHeaderMatchesMediaType("Content-Type", 
            new[] { "application/vnd.marvin.authorwithdateofdeath.full+json", "application/vnd.marvin.authorwithdateofdeath.full+xml" })]
        public IActionResult CreateAuthorWithDateOfDeath([FromBody] AuthorForCreationWithDateOfDeathDto author)
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

            var authorDto = Mapper.Map<AuthorDto>(authorEntity);

            IEnumerable<LinkDto> links = CreateLinksForAuthor(authorDto.Id, null);

            var linkedResourcesToReturn = authorDto.ShapeData(null) as IDictionary<string, object>;
            linkedResourcesToReturn.Add("links", links);

            return CreatedAtRoute(
                RouteNames.GetAuthorRoute,
                new
                {
                    id = linkedResourcesToReturn["Id"]
                },
                linkedResourcesToReturn); //the Response also holds the URI for the newly created resource, so its ID as well.
        }

        [HttpDelete("{id}", Name = "DeleteAuthor")]
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

        [HttpGet("{id}", Name = "GetAuthor")]
        public IActionResult GetAuthor(Guid id, [FromQuery] string fields)
        {
            if (!c_typeHelperService.TypeHasProperties<AuthorDto>(fields))
            {
                return BadRequest();
            }

            Author authorEntity = c_libraryRepository.GetAuthor(id);

            if (authorEntity == null)
            {
                return NotFound();
            }

            var authorDto = Mapper.Map<AuthorDto>(authorEntity);

            IEnumerable<LinkDto> links = CreateLinksForAuthor(id, fields);

            var linkedResourcesToReturn = authorDto.ShapeData(fields) as IDictionary<string, object>;
            linkedResourcesToReturn.Add("links", links);

            return Ok(linkedResourcesToReturn);
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult
            GetAuthors(AuthorsResourceParameters authorsResourceParameters, [FromHeader(Name = "Accept")] string mediaType) // Frameworkul stie sa faca bindingul la query string parameters la proprietati din clasa asta.
        {
            if (!c_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!c_typeHelperService.TypeHasProperties<AuthorDto>(authorsResourceParameters.Fields))
            {
                return BadRequest();
            }

            PagedList<Author> authorEntities = c_libraryRepository.GetAuthors(authorsResourceParameters);

            var authorDtos = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            if (mediaType == AcceptMediaTypes.MarvinHateoasPlusJson)
            {
                var paginationMetadata = new
                {
                    totalCount = authorEntities.TotalCount,
                    pageSize = authorEntities.PageSize,
                    currentPage = authorEntities.CurrentPage,
                    totalPages = authorEntities.TotalPages
                };

                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                IEnumerable<LinkDto> links = CreateLinksForAuthors(authorsResourceParameters, authorEntities.HasNext, authorEntities.HasPrevious);
                IEnumerable<ExpandoObject> shapedAuthors = authorDtos.ShapeData(authorsResourceParameters.Fields);

                IEnumerable<IDictionary<string, object>> shapedAuthorsWithLinks = shapedAuthors.Select(
                    authorDto =>
                    {
                        var authorAsDictionary = authorDto as IDictionary<string, object>;
                        IEnumerable<LinkDto> authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], authorsResourceParameters.Fields);

                        authorAsDictionary.Add("links", authorLinks);

                        return authorAsDictionary;
                    });

                var linkedCollectionResource = new
                {
                    value = shapedAuthorsWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                string previousPageLink = authorEntities.HasPrevious ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage) : null;

                string nextPageLink = authorEntities.HasNext ? CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = authorEntities.TotalCount,
                    pageSize = authorEntities.PageSize,
                    currentPage = authorEntities.CurrentPage,
                    totalPages = authorEntities.TotalPages
                };

                Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                return Ok(authorDtos.ShapeData(authorsResourceParameters.Fields));
            }
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
                case ResourceUriType.Current:
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

        private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string fields)
        {
            var links = new List<LinkDto>();
            const string c_httpGetMethod = "GET";

            if (string.IsNullOrWhiteSpace(fields))
            {
                string getAuthorUrl = c_urlHelper.Link(
                    "GetAuthor",
                    new
                    {
                        id = authorId
                    });
                links.Add(new LinkDto(getAuthorUrl, "self", c_httpGetMethod));
            }
            else
            {
                string getAuthorUrl = c_urlHelper.Link(
                    "GetAuthor",
                    new
                    {
                        id = authorId,
                        fields = fields
                    });
                links.Add(new LinkDto(getAuthorUrl, "self", c_httpGetMethod));
            }

            string deleteAuthorUrl = c_urlHelper.Link(
                "DeleteAuthor",
                new
                {
                    id = authorId
                });
            links.Add(new LinkDto(deleteAuthorUrl, "delete_author", "DELETE"));

            string createBookForAuthorUrl = c_urlHelper.Link(
                "CreateBookForAuthor",
                new
                {
                    authorId = authorId
                });
            links.Add(new LinkDto(createBookForAuthorUrl, "create_book_for_author", "POST"));

            string getBooksForAuthorUrl = c_urlHelper.Link(
                "GetBooksForAuthor",
                new
                {
                    authorId = authorId
                });
            links.Add(new LinkDto(getBooksForAuthorUrl, "books", c_httpGetMethod));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        [HttpOptions]
        public IActionResult GetAuthorOptions()
        {
            const string c_acceptHeaderKey = "Allow";
            const string c_supportedHttpMethods = "GET, OPTIONS, POST";

            Response.Headers.Add(c_acceptHeaderKey, c_supportedHttpMethods);
            return Ok();
        }
    }
}