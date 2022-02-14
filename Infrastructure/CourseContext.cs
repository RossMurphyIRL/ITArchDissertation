using Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure
{
    public class CourseContext : DbContext
    {
        public CourseContext()
        {
        }
        public CourseContext(DbContextOptions<CourseContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<UserDto> Users { get; set; }
        public DbSet<TaskDto> Tasks { get; set; }
    }
}
