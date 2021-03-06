﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TestAmericanFootball2.Models;

namespace TestAmericanFootball2.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AFDbContext>
    {
        public AFDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<AFDbContext>();
            var connectionString = configuration.GetConnectionString("TestAmericanFootball2Context");
            builder.UseSqlServer(connectionString);
            return new AFDbContext(builder.Options);
        }
    }
}