using Core.DTOs;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CourseContext _context;
        public UserRepository(CourseContext context)
        {
            _context = context;
        }
        public UserDto AddUser(string firstName, string lastName)
        {
            var newUser = new UserDto { FirstName = firstName, LastName = lastName };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return newUser;
        }
        public void DeleteUser(int userId)
        {
            var user = _context.Users.Single(x => x.UserId == userId);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
