// ------------------------------------------------------------------------------
//     <copyright file="IPropertyMappingService.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Library.API.Services
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();

        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}