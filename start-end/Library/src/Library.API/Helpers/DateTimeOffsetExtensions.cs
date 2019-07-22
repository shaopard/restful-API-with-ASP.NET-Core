// ------------------------------------------------------------------------------
//     <copyright file="DateTimeOffsetExtensions.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;

namespace Library.API.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset, DateTimeOffset? dateOfDeath)
        {
            DateTime dateToCalculate = DateTime.UtcNow;

            if (dateOfDeath != null)
            {
                dateToCalculate = dateOfDeath.Value.UtcDateTime;
            }

            int age = dateToCalculate.Year - dateTimeOffset.Year;
            if (dateToCalculate < dateTimeOffset.AddYears(age))
            {
                age--;
            }

            return age;
        }
    }
}