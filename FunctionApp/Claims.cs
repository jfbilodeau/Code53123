using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FunctionApp
{
    public static class Claims
    {
        private static ActionResult UnauthorizedResult(string message)
        {
            var result = new ObjectResult(message)
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return result;
        }

        [FunctionName("Claims")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            ClaimsPrincipal principal = req.HttpContext.User;

            if (principal.Identity.IsAuthenticated == false)
            {
                return UnauthorizedResult("Not authenticated");
            }

            var message = $"Hello, {principal.Identity.Name}\n";
            message += "Your claims are:\n";
            foreach (var claim in principal.Claims)
            {
                message += $"  {claim.Type} = {claim.Value}\n";
            }

            return new OkObjectResult(message);
        }
    }
}
