﻿// ------------------------------------------------------------------------------
//     <copyright file="RootController.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;

using Library.API.Enums;
using Library.API.Models;

using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api")]
    public class RootController : Controller
    {
        private readonly IUrlHelper c_urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            c_urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var links = new List<LinkDto>();

                links.Add(
                    new LinkDto(c_urlHelper.Link("GetRoot", new { }),
                                "self",
                                "GET"));

                links.Add(
                    new LinkDto(c_urlHelper.Link("GetAuthors", new { }),
                                "authors",
                                "GET"));

                links.Add(
                    new LinkDto(c_urlHelper.Link("CreateAuthor", new { }),
                                "create_author",
                                "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}