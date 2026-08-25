using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelljonVibe.Identity.Api.Controllers;


/// <summary>
/// Controller for user management operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase {



}