using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using IO = System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tests.Models;
using ExcelWorker;
using VKAPIWorker;

namespace Tests.Controllers
{
    public class Order
    {
        public string User { get; set; }
    }
    public class Review
    {
        public string UserName { get; set; }
        public string TextReview { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        [HttpGet]
        public IActionResult Test(object id)
        {
            if (id == null)
                return RedirectToAction("Index");
            ViewBag.PhoneId = id;
            return View();
        }
        [HttpPost]
        public string Test(Review review)
        {
            return $"Cпасибо {review.UserName} за отзыв:{Environment.NewLine}{review.TextReview}";
        }

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string Index(Review review)
        {
            return $"Cпасибо {review.UserName} за отзыв:{Environment.NewLine}{review.TextReview}";
        }
        
        public IActionResult Result(string message)
        {
            if (VKAPIWorkerClass.TryGetGroupId(out string grop_id, message))
            {
                ViewData["Message"] = message;
                var data = VKAPIWorkerClass.GetActivities(message, 248);
                int i = 1;
                foreach (var item in data)
                {
                    ViewData[$"Topic{i}"] = item.Key;
                    ViewData[$"Dataset{i++}"] = item.Value;
                }
                return View();
            }
            else
            {
                if (string.IsNullOrEmpty(grop_id))
                    return RedirectToAction("PageError", new { error_message = "Паблика по указанному адресу не найдено" });                
                else
                    return RedirectToAction("PageError", new { error_message = $"Паблик c id{grop_id} не доступен." });
            }
        }

        public IActionResult Instruction()
        {
            return View();
        }

        static FileStreamResult stream;
        public IActionResult GetFile(string message)
        {
            if (string.IsNullOrEmpty(message))
                return RedirectToAction("PageError");
            else
            {
                stream = ExcelWorkerClass.GetFile(VKAPIWorkerClass.GetActivities(message, 248));
                if (stream == null)
                    return RedirectToAction("PageError", new { error_message = "Не получилось сделать файл. Попробуйте позже"});                
                return RedirectToAction("Download");
            }
        }

        [HttpGet]
        public FileStreamResult Download() 
            => stream;

        public IActionResult PageError(string error_message)
        {
            ViewData["ErrorMessage"] = error_message;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
