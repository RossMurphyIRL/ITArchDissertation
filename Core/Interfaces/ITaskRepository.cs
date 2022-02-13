using Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface ITaskRepository
    {
        public TaskDto AddTask(string title, bool isComplete, DateTime dueDate);
        public void AssignTask(TaskDto taskDto, UserDto userDto);
        public void UpdateFirstTaskDueDate(DateTime dueDate);
        public void DeleteTask(int taskId);
    }
}
