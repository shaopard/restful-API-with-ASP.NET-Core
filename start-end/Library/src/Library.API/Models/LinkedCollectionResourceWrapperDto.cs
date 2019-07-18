// ------------------------------------------------------------------------------
//     <copyright file="LinkedCollectionResourceWrapperDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Library.API.Models
{
    public class LinkedCollectionResourceWrapperDto<T> : LinkedResourceBaseDto
        where T : LinkedResourceBaseDto
    {
        public IEnumerable<T> Value { get; set; }

        public LinkedCollectionResourceWrapperDto(IEnumerable<T> value)
        {
            Value = value;
        }
    }
}