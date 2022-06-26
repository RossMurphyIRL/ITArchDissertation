using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Globalization;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Extensions.Configuration;

namespace DissertationMSSQLEF.Controllers
{
    [ApiController]
    [EnableCors("CorsApi")]
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
        [Authorize(AuthenticationSchemes = SchemesNamesConst.TokenAuthenticationDefaultScheme)]
        public ActionResult RunTaskAssignmentOpp()
        {
            
            Console.WriteLine("** C# CRUD sample with Entity Framework and SQL Server **\n");
            try
            {
                var taskguid = Guid.NewGuid();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                _taskRepository.AddTask("Second App Start " + timestamp, false, DateTime.Parse("04-01-2017"), taskguid);
                for (int i = 0; i < 10; i++)
                {
                    var newUser = _userRepository.AddUser("Anna", "Shrestinian");
                    Console.WriteLine("\nCreated User: " + newUser.ToString());

                    var newTask = _taskRepository.AddTask("Ship Helsinki", false, DateTime.Parse("04-01-2017"));
                    Console.WriteLine("\nCreated Task: " + newTask.ToString());

                    _taskRepository.AssignTask(newTask, newUser);
                    Console.WriteLine("\nAssigned Task: '" + newTask.Title + "' to user '" + newUser.GetFullName() + "'");

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
                _taskRepository.AddTask("Second App End " + timestamp, false, DateTime.Parse("04-01-2017"), taskguid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return Ok("Execution Complete");
        }
    }


    public class ValidateBearerToken : IAuthorizationRequirement
    {
    }

    public class ValidateTokenHandler : AuthorizationHandler<ValidateBearerToken>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public ValidateTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidateBearerToken requirement)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var requestHeaders = httpContext.Request.Headers;
            string token = "";
            if (requestHeaders.TryGetValue("Authorization", out var tokens)){

                token = tokens.FirstOrDefault() ?? string.Empty;
            }

            if(string.IsNullOrEmpty(token))
            {
                context.Fail();
            }


            return Task.CompletedTask;
        }
    }

    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        public IServiceProvider ServiceProvider { get; set; }
        public IOptionsMonitor<TokenAuthenticationOptions> Options { get; set; }
        public IConfiguration Configuration { get; set; }

        public TokenAuthenticationHandler(IConfiguration configuration, IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IServiceProvider serviceProvider)
            : base(options, logger, encoder, clock)
        {
            ServiceProvider = serviceProvider;
            Options = options;
            Configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = Request.Headers;
            var token = "";
            if (headers.TryGetValue("Authorization", out var tokens))
            {
                token = tokens.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token is null"));
            }

            token = token.Split(" ")[1];

            var myIssuer = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/v2.0", new object[] { Options.CurrentValue.Instance, Options.CurrentValue.TenantId });
            var mySecret = Options.CurrentValue.Secret;
            var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/.well-known/openid-configuration", Options.CurrentValue.TenantId);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = configManager.GetConfigurationAsync().Result;
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = Options.CurrentValue.ClientId,
                ValidIssuer = myIssuer,
                ValidateLifetime = false,
                IssuerSigningKey = mySecurityKey,
                IssuerSigningKeys = config.SigningKeys

            };

            var validatedToken = (SecurityToken)new JwtSecurityToken();

            // Throws an Exception as the token is invalid (expired, invalid-formatted, etc.)  
            try
            {
                System.Diagnostics.Debug.WriteLine("Trying to validate token");
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Token");
            }

            var claims = new[] { new Claim("token", token) };
            var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), this.Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class TokenAuthenticationOptions: AuthenticationSchemeOptions
    {
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
    }

}
