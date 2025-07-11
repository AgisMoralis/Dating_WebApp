using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[ServiceFilter(typeof(Helpers.LogUserActivity))]
[ApiController]
[Route("api/[controller]")]
public class BaseAPIController : ControllerBase
{

}
