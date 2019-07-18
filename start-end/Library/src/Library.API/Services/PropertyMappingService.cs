// ------------------------------------------------------------------------------
//     <copyright file="PropertyMappingService.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService

    {
        private readonly Dictionary<string, PropertyMappingValue> c_authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Id", new PropertyMappingValue(
                    new List<string>
                    {
                        "Id"
                    })
            },
            {
                "Genre", new PropertyMappingValue(
                    new List<string>
                    {
                        "Genre"
                    })
            },
            {
                "Age", new PropertyMappingValue(
                    new List<string>
                    {
                        "DateOfBirth"
                    },
                    true)
            },
            {
                "Name", new PropertyMappingValue(
                    new List<string>
                    {
                        "FirstName",
                        "LastName"
                    })
            }
        };

        private readonly IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(c_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<AuthorDto, Author>>().ToList();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.Single().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            Dictionary<string, PropertyMappingValue> propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            string[] fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (string field in fieldsAfterSplit)
            {
                // trim
                string trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                int indexOfFirstSpace = trimmedField.IndexOf(" ");
                string propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}