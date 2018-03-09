using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MvcVueClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// Logins the specified redirect.
        /// </summary>
        /// <param name="redirect">The redirect.</param>
        /// <returns></returns>
        [Route("/login")]
        [Authorize]
        public IActionResult Login([FromQuery()] string redirect = "")
        {
            if (string.IsNullOrEmpty(redirect))
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Redirect to profile by example
                return Redirect(redirect);
            }
        }

        [Route("/logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
