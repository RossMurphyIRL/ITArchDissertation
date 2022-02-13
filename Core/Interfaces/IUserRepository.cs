using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        public UserDto AddUser(string firstName, string lastName);
        public void DeleteUser(int userId);
    }
}
