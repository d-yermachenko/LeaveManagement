﻿using System;
using LeaveManagement.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(LeaveManagement.Areas.Identity.IdentityHostingStartup))]
namespace LeaveManagement.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                /*services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();*/
            });


        }
    }
}