﻿using Microsoft.EntityFrameworkCore;

namespace Mate.DataCore.Data.Context
{
    public class HangfireDBContext : DbContext
    {
        public HangfireDBContext(DbContextOptions<HangfireDBContext> options) : base(options: options)
        {
        }

    }

    public static class HangfireDBInitializer
    {
        public static void DbInitialize(HangfireDBContext context)
        {
            //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Azure")
                   context.Database.EnsureDeleted();
            // else
            context.Database.EnsureCreated();
        }
    }
}