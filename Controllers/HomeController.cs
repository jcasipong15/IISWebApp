using IISWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Diagnostics;

namespace IISWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var sites = new List<SiteViewModel>();
            try
            {
                using var serverManager = new ServerManager();
                foreach (var site in serverManager.Sites)
                {
                    string physicalPath = "";
                    var rootApp = site.Applications.FirstOrDefault(a => a.Path == "/");
                    if (rootApp != null)
                    {
                        var rootVdir = rootApp.VirtualDirectories.FirstOrDefault(v => v.Path == "/");
                        if (rootVdir != null)
                        {
                            physicalPath = rootVdir.PhysicalPath;
                        }
                    }

                    sites.Add(new SiteViewModel
                    {
                        Id = site.Id,
                        Name = site.Name,
                        State = site.State.ToString(),
                        PhysicalPath = physicalPath,
                        Bindings = site.Bindings.Select(b => $"{b.Protocol} {b.BindingInformation}").ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load IIS Sites.");
                ViewBag.ErrorMessage = "Unable to connect to IIS. Make sure this application runs with Administrator privileges.";
            }

            return View(sites);
        }

        [HttpPost]
        public IActionResult ToggleState(string siteName, string command)
        {
            try
            {
                using var serverManager = new ServerManager();
                var site = serverManager.Sites.FirstOrDefault(s => s.Name == siteName);
                if (site == null)
                {
                    TempData["Error"] = $"Site '{siteName}' not found.";
                    return RedirectToAction(nameof(Index));
                }

                switch (command.ToLower())
                {
                    case "start":
                        site.Start();
                        break;
                    case "stop":
                        site.Stop();
                        break;
                    case "restart":
                        site.Stop();
                        System.Threading.Thread.Sleep(500);
                        site.Start();
                        break;
                    default:
                        TempData["Error"] = "Invalid command.";
                        return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = $"Successfully sent {command} command to '{siteName}'.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to {command} '{siteName}'.");
                TempData["Error"] = $"Error executing command: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
