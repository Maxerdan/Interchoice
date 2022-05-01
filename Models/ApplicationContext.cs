﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Interchoice.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<ProjectInfo> ProjectsInfo { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
