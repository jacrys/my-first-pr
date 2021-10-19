namespace my_first_pr.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
#if DEBUG
            Console.WriteLine(returnUrl);
#endif
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }
    }
}