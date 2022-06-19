using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Interfaces;
using System.Globalization;

namespace DissertationMSSQLEF.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskAssignmentController : ControllerBase
    {

        private readonly ILogger<TaskAssignmentController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskAssignmentController(ILogger<TaskAssignmentController> logger, IUserRepository userRepository, ITaskRepository taskRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        [HttpGet]
        public ActionResult RunTaskAssignmentOpp()
        {
            Console.WriteLine("** C# CRUD sample with Entity Framework and SQL Server **\n");
            try
            {
                var taskguid = Guid.NewGuid();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                _taskRepository.AddTask("First App Start " + timestamp, false, DateTime.Parse("04-01-2017"), taskguid);
                for (int i = 0; i < 10; i++)
                {
                    var newUser = _userRepository.AddUser("Anna", "Shrestinian");
                    Console.WriteLine("\nCreated User: " + newUser.ToString());

                    var newTask = _taskRepository.AddTask("Ship Helsinki", false, DateTime.Parse("04-01-2017"));
                    Console.WriteLine("\nCreated Task: " + newTask.ToString());

                    _taskRepository.AssignTask(newTask, newUser);
                    Console.WriteLine("\nAssigned Task: '" + newTask.Title + "' to user '" + newUser.GetFullName() + "'");

                    _taskRepository.UpdateFirstTaskDueDate(DateTime.Parse("05-06-2016"));
                    Console.WriteLine("\nUpdate Task: ");

                    _taskRepository.DeleteTask(newTask.TaskId);
                    Console.WriteLine("\nDelete Task: " + newTask.TaskId);

                    _userRepository.DeleteUser(newUser.UserId);
                    Console.WriteLine("\nDelete User: " + newUser.UserId);
                }

                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                _taskRepository.AddTask("First App End " + timestamp, false, DateTime.Parse("04-01-2017"), taskguid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return Ok("Execution Complete");
        }
    }
}
