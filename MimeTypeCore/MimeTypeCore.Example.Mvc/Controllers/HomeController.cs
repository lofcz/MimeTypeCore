using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MimeTypeCore.Example.Mvc.Models;

namespace MimeTypeCore.Example.Mvc.Controllers;

public class HomeController : Controller
{
    [HttpPost]
    public async Task<IActionResult> GetMime()
    {
        try
        {
            IFormCollection form = await Request.ReadFormAsync();
            IFormFileCollection files = form.Files;

            if (files.Count is 0)
            {
                return Json(new
                {
                    error = "Expected 1 file, got none"
                });
            }
            
            IFormFile file = files[0];
            await using Stream stream = file.OpenReadStream();
            string? mimeType = await MimeTypeMap.TryGetMimeTypeAsync(file.FileName, stream);

            return Json(new
            {
                mime = mimeType,
                mimeUnknown = mimeType is null
            });
        }
        catch (Exception e) // file size exceeds allowed limit, etc.
        {
            return Json(new
            {
                error = e.Message
            });
        }
    }
    
    public IActionResult Index()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}