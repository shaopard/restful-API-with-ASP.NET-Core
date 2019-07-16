// ------------------------------------------------------------------------------
//     <copyright file="AuthorsResourceParameters.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        const int maxPageSize = 20;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize
        {
            get => _pageSize;
            set { _pageSize = (value > AuthorsResourceParameters.maxPageSize) ? AuthorsResourceParameters.maxPageSize : value; }
        }

        public int AuthorsToSkip => PageSize * (PageNumber - 1);

        public string Genre { get; set; }

        public string SearchQuery { get; set; }
    }
}