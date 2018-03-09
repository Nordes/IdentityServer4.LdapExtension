using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MvcVueClient.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return Ok(new { Name = "abc" });
        }

        /// <summary>
        /// Get the current user payload.
        /// </summary>
        /// <returns>Returns the user claims</returns>
        [HttpGet]
        [Authorize]
        [Route("[action]")]
        public IActionResult Current()
        {
            return new JsonResult(
                from c in User.Claims
                select new { c.Type, c.Value });
        }
    }
}
