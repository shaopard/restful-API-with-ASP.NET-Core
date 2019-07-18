// ------------------------------------------------------------------------------
//     <copyright file="LinkedResourceBaseDto.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Library.API.Models
{
    public abstract class LinkedResourceBaseDto
    {
        public List<LinkDto> Links { get; set; } = new List<LinkDto>();
    }
}