﻿// ------------------------------------------------------------------------------
//     <copyright file="LibraryRepository.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;

namespace Library.API.Services
{
    public class LibraryRepository : ILibraryRepository
    {
        private readonly LibraryContext c_context;

        private readonly IPropertyMappingService c_propertyMappingService;

        public LibraryRepository(LibraryContext context, IPropertyMappingService propertyMappingService)
        {
            c_context = context;
            c_propertyMappingService = propertyMappingService;
        }

        public void AddAuthor(Author author)
        {
            author.Id = Guid.NewGuid();
            c_context.Authors.Add(author);

            // the repository fills the id (instead of using identity columns)
            if (author.Books.Any())
            {
                foreach (Book book in author.Books)
                {
                    book.Id = Guid.NewGuid();
                }
            }
        }

        public void AddBookForAuthor(Guid authorId, Book book)
        {
            Author author = GetAuthor(authorId);
            if (author != null)
            {
                // if there isn't an id filled out (ie: we're not upserting),
                // we should generate one
                if (book.Id == Guid.Empty)
                {
                    book.Id = Guid.NewGuid();
                }

                author.Books.Add(book);
            }
        }

        public bool AuthorExists(Guid authorId)
        {
            return c_context.Authors.Any(a => a.Id == authorId);
        }

        public void DeleteAuthor(Author author)
        {
            c_context.Authors.Remove(author);
        }

        public void DeleteBook(Book book)
        {
            c_context.Books.Remove(book);
        }

        public Author GetAuthor(Guid authorId)
        {
            return c_context.Authors.FirstOrDefault(a => a.Id == authorId);
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            //IQueryable<Author> collectionBeforePaging = _context.Authors.OrderBy(a => a.FirstName).ThenBy(a => a.LastName);

            IQueryable<Author> collectionBeforePaging = c_context.Authors.ApplySort(authorsResourceParameters.OrderBy, c_propertyMappingService.GetPropertyMapping<AuthorDto, Author>());

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.Genre))
            {
                string genreForWhereClause = authorsResourceParameters.Genre.Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging.Where(author => author.Genre.ToLowerInvariant() == genreForWhereClause);
            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
                string searchQueryForWhereClause = authorsResourceParameters.SearchQuery.Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging.Where(
                    author => author.Genre.ToLowerInvariant().Contains(searchQueryForWhereClause) || author.FirstName.ToLowerInvariant().Contains(searchQueryForWhereClause) ||
                              author.LastName.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            PagedList<Author> authorsToReturn = PagedList<Author>.Create(collectionBeforePaging, authorsResourceParameters.PageNumber, authorsResourceParameters.PageSize);

            return authorsToReturn;
        }

        public IEnumerable<Author> GetAuthors(IEnumerable<Guid> authorIds)
        {
            return c_context.Authors.Where(a => authorIds.Contains(a.Id)).OrderBy(a => a.FirstName).ThenBy(a => a.LastName).ToList();
        }

        public Book GetBookForAuthor(Guid authorId, Guid bookId)
        {
            return c_context.Books.FirstOrDefault(b => b.AuthorId == authorId && b.Id == bookId);
        }

        public IEnumerable<Book> GetBooksForAuthor(Guid authorId)
        {
            return c_context.Books.Where(b => b.AuthorId == authorId).OrderBy(b => b.Title).ToList();
        }

        public bool Save()
        {
            return (c_context.SaveChanges() >= 0);
        }

        public void UpdateAuthor(Author author)
        {
            // no code in this implementation
        }

        public void UpdateBookForAuthor(Book book)
        {
            // no code in this implementation
        }
    }
}