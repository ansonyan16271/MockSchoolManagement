using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MockSchoolManagement.DataRepositories;
using MockSchoolManagement.Models;
using MockSchoolManagement.ViewModels;
using System.Diagnostics;
using System.Reflection;

namespace MockSchoolManagement.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IStudentRepository _studentRepository;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, IStudentRepository studentRepository)
        {
            _logger = logger;

            _webHostEnvironment = webHostEnvironment;

            _studentRepository = studentRepository;
        }

        //public IActionResult Index()
        //{
        //    ViewData["key"] = _configuration["MyKey"];
        //    ViewData["environmentName"] = _webHostEnvironment.EnvironmentName;
        //    MockStudentRepository _studentRepository = new MockStudentRepository();
        //    string? studentName = _studentRepository.GetStudent(1).Name;
        //    return View();
        //}
        //[Route("")]
        //[Route("Home")]
        //[Route("Home/Index")]
        
        public IActionResult Index()
        {
            IEnumerable<Student> model = _studentRepository.GetAllStudents();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //public ViewResult Details()
        //{
        //    Student? model = _studentRepository.GetStudent(1);
        //    return View("", model);
        //}
        public ViewResult Details(int id)
        {
            _logger.LogTrace("Trace(跟踪) Log");
            _logger.LogDebug("Debug(调试) Log");
            _logger.LogInformation("信息(Information) Log");
            _logger.LogWarning("警告(Warning) Log");
            _logger.LogError("错误(Error) Log");
            _logger.LogCritical("严重(Critical) Log");
            
            var student = _studentRepository.GetStudentById(id);
            //throw new Exception("在Details视图中抛出异常！");
            //判断学生信息是否存在
            if (student == null)
            {
                //Response.StatusCode = 404;
                ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试！";
                //return View("StudentNotFound", id);
                return View("NotFound");
            }
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Student = student,
                PageTitle = "学生详情",
            };

            return View(homeDetailsViewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(StudentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? uniqueFileName = null;
                if (model.Photos != null && model.Photos.Count > 0)
                {
                    foreach (IFormFile photo in model.Photos)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        await photo.CopyToAsync(new FileStream(filePath, FileMode.Create));

                    };
                };
                Student newStudent = new Student
                {
                    Name = model.Name,
                    Email = model.Email,
                    Major = model.Major,
                    PhotoPath = uniqueFileName
                };
                _studentRepository.Insert(newStudent);
                return RedirectToAction("Details", new { id = newStudent.Id });
            }
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Student student = _studentRepository.GetStudentById(id);
            if (student == null)
            {
                Response.StatusCode = 404;
                return View("NotFound");
            }
            StudentEditViewModel studentEditViewModel = new StudentEditViewModel
            {
                Id = id,
                Name = student.Name,
                Email = student.Email,
                Major = student.Major,
                ExistingPhotoPath = student.PhotoPath,
            };
            return View(studentEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StudentEditViewModel studentEditViewModel)
        {
            if (ModelState.IsValid)
            {
                Student student = _studentRepository.GetStudentById(studentEditViewModel.Id);
                student.Name = studentEditViewModel.Name;
                student.Email = studentEditViewModel.Email;
                student.Major = studentEditViewModel.Major;

                if (studentEditViewModel.Photos?.Count > 0)
                {
                    if (studentEditViewModel.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars", studentEditViewModel.ExistingPhotoPath);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    student.PhotoPath = await ProcessUploadedFile(studentEditViewModel);

                }
                Student updatedstudent = _studentRepository.Update(student);
                return RedirectToAction("index");

            }
            return View(studentEditViewModel);
        }

        /// <summary>
        /// 将照片保存到指定的路径中，并返回唯一的文件名
        /// </summary>
        /// <returns></returns>
        private async Task<string> ProcessUploadedFile(StudentCreateViewModel model)
        {
            string uniqueFileName = null;

            if (model.Photos.Count > 0)
            {
                foreach (var photo in model.Photos)
                {
                    //必须将图像上传到wwwroot中的images/avatars文件夹
                    //而要获取wwwroot文件夹的路径，我们需要注入 ASP.NET Core提供的webHostEnvironment服务
                    //通过webHostEnvironment服务去获取wwwroot文件夹的路径
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
                    //为了确保文件名是唯一的，我们在文件名后附加一个新的GUID值和一个下划线
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    //因为使用了非托管资源，所以需要手动进行释放
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        //使用IFormFile接口提供的CopyTo()方法将文件复制到wwwroot/images/avatars 文件夹
                        await photo.CopyToAsync(fileStream);
                    }


                }
            }
            return uniqueFileName;
        }

        [AllowAnonymous]
        [Route("/Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

            //获取异常详情信息
            var exceptionHandlerPathFeature =
                    HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            //ViewBag.ExceptionPath = exceptionHandlerPathFeature.Path;
            //ViewBag.ExceptionMessage = exceptionHandlerPathFeature.Error.Message;
            //ViewBag.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;
            //LogError() 方法将异常记录作为日志中的错误类别记录
            _logger.LogError($"路径 {exceptionHandlerPathFeature.Path} " +
                $"产生了一个错误{exceptionHandlerPathFeature.Error}");
            return View("Error");
        }
    }
}