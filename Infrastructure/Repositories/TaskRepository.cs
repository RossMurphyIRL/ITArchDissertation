using Core.DTOs;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Repositories
{
    public class TaskRepository: ITaskRepository
    {
        private readonly CourseContext _context;
        public TaskRepository(CourseContext context)
        {
            _context = context;
        }
        public TaskDto AddTask(string title, bool isComplete, DateTime dueDate)
        {
            var newTask = new TaskDto() { Title = title, IsComplete = isComplete, DueDate = dueDate };
            _context.Tasks.Add(newTask);
            _context.SaveChanges();
            return newTask;
        }

        public void AssignTask(TaskDto task, UserDto user)
        {
            task.AssignedTo = user;
            _context.SaveChanges();
        }

        public void UpdateFirstTaskDueDate(DateTime dueDate)
        {
            var taskToUpdate = _context.Tasks.First();
            taskToUpdate.DueDate = dueDate;
            _context.SaveChanges();
        }

        public void DeleteTask(int taskId)
        {
            var task = _context.Tasks.Single(x => x.TaskId == taskId);
            _context.Tasks.Remove(task);
            _context.SaveChanges();
        }
    }
}
