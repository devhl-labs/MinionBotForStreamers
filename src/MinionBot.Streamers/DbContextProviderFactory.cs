using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SQLitePCL;

namespace MinionBot.Streamers
{
    public class DbContextProviderFactory : IDesignTimeDbContextFactory<CocApi.Cache.CacheDbContext>
    {
        public CacheDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CocApi.Cache.CacheDbContext>();

            optionsBuilder.UseSqlite($"Data Source={ Path.Combine(Program.DatabaseFolder, "cocapi.cache.sqlite") };Cache=Shared");

            return new CocApi.Cache.CacheDbContext(optionsBuilder.Options);
        }
    }
}
