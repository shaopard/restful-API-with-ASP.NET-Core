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
    private Dictionary<string, PropertyMappingValue> _authorPropertyMapping = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
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

    private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

    public PropertyMappingService()
    {
        propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
    }

    public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
    {
        var mathcingMapping = propertyMappings.OfType<PropertyMapping<AuthorDto, Author>>();

        if (mathcingMapping.Count() == 1)
        {
            return mathcingMapping.Single().MappingDictionary;
        }

        throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
    }
    }
}