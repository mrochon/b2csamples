using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using B2CUsingGraph.Models;
using Microsoft.Graph;
using Microsoft.AspNetCore.Authorization;

namespace B2CUsingGraph.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GraphServiceClient _graph;

    public HomeController(ILogger<HomeController> logger, GraphServiceClient graph)
    {
        _logger = logger;
        _graph = graph;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _graph.Users.Request().GetAsync();
        return View(users);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
