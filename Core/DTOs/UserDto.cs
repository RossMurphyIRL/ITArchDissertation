using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class UserDto
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public virtual IList<TaskDto> Tasks { get; set; }

        public string GetFullName()
        {
            return this.FirstName + " " + this.LastName;
        }
        public override string ToString()
        {
            return "User [id=" + this.UserId + ", name=" + this.GetFullName() + "]";
        }
    }
}
