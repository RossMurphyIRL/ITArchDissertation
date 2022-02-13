using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.DTOs
{
    public class TaskDto
    {
        [Key]
        public int TaskId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsComplete { get; set; }
        [NotMapped]
        public virtual UserDto AssignedTo { get; set; }
        public override string ToString()
        {
            return "Task [id=" + this.TaskId + ", title=" + this.Title + ", dueDate=" + this.DueDate.ToString() + ", IsComplete=" + this.IsComplete + "]";
        }
    }
}
