using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MockSchoolManagement.Controllers
{
    public class ErrorController : Controller
    {
        private ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this._logger = logger;
        }
        //[Route("/Home/Error")]
        //public IActionResult Error()
        //{
        //    var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        //    _logger.LogError($"路径{exceptionHandlerPathFeature.Path}" + $"产生了一个错误{exceptionHandlerPathFeature.Error}");
        //    return View("Error");
        //}

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int? statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "抱歉，您访问的页面不存在";
                    //ViewBag.Path = statusCodeResult.OriginalPath;
                    //ViewBag.QS = statusCodeResult.OriginalQueryString;
                    _logger.LogWarning($"发生了一个404错误，路径=" + $"{statusCodeResult.OriginalPath}以及查询字符串=" + $"{statusCodeResult.OriginalQueryString}");
                    break;
            }
            return View("NotFound");
        }
    }
}
