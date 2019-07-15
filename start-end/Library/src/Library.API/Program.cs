// ------------------------------------------------------------------------------
//     <copyright file="Program.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using NLog.Web;

namespace Library.API
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args) => 
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>()
                   .UseNLog()
                   .Build();

        public static void Main(string[] args)
        {
            Program.BuildWebHost(args).Run();
        }
    }
}