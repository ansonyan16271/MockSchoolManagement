﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockSchoolManagement.Models;
using MockSchoolManagement.ViewModels;
using System.Net.Mail;
using System.Security.Claims;
using Zhaoxi.Manage.Common.Notice;

namespace MockSchoolManagement.Controllers
{

    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private ILogger<AccountController> _logger;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                //将数据从RegisterViewModel复制到IdentityUser
                var user = new ApplicationUser
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    City = registerViewModel.City,
                };
                //将用户数据存储在AspNetUsers数据库表中
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);
                //如果成功创建用户，则使用登录服务登录用户信息
                //并重定向到HomeController的Index操作方法中
                if (result.Succeeded)
                {
                    //生成电子邮件确认令牌
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //生成电子邮件的确认链接
                    var confirmationLink = Url.Action("ConfirmEmail", "Account",new { userId = user.Id, token = token }, Request.Scheme);
                    //需要注入ILogger<AccountController> _logger;服务，记录生成的URL链接
                    _logger.Log(LogLevel.Warning, confirmationLink);

                    //如果用户已登录并属于Admin角色。
                    //那么就是Admin正在创建新用户。
                    //所以重定向Admin用户到ListRoles的视图列表
                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Admin");
                    }
                    string emailAddress = user.Email.ToString();
                    string sendBody = $"<p>请单击下面链接确认邮件。</P><bt><br><a href='{confirmationLink}'>确认邮件地址</a> ";

                    EMailTool mailTool = new EMailTool(init =>
                    {
                        init.Host = "smtp.163.com";
                        init.EnableSsl = true;
                        init.Port = 25;
                        init.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                        init.Credentials = new System.Net.NetworkCredential("ansonyan16271@163.com", "KNTOKMUWXWBUFNIT");
                    });
                   
                    bool boolResult = mailTool.SendMail("ansonyan16271@163.com","测试",new string[1] { emailAddress },"确认邮件", sendBody, MailPriority.High,isHtml: true);
                    if(boolResult)
                    {
                        ViewBag.ErrorTitle = "注册成功";
                        ViewBag.ErrorMessage = $"在你登入系统前,我们已经给您发了一份邮件，需要您先进行邮件验证，点击确认链接即可完成。";
                        return View("Error");
                    }
                    

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("Index", "Home");
                }
                //如果有任何错误，将它们添加到ModelState对象中
                //将由验证摘要标记助手显示到视图中
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(registerViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        //public IActionResult Login()
        //{
        //    return View();
        //}
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };
            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            loginViewModel.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(loginViewModel.Email);
                if (user != null && !user.EmailConfirmed && await _userManager.CheckPasswordAsync(user,loginViewModel.Password))
                {
                    ModelState.AddModelError(string.Empty, "您的电子邮箱还未进行验证!");
                    return View(loginViewModel);
                }

                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }

                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }

                }
                ModelState.AddModelError(string.Empty, "登陆失败，请重试！");
            }
            return View(loginViewModel);
        }

        //[HttpGet]
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}


        [AcceptVerbs("Get", "Post")]
        
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"邮箱：{email}已经被注册使用了");
            }
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback","Account",new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"外部提供程序错误:{remoteError}");
                return View("Login", loginViewModel);
            }
            // 从外部登录提供者,即微软账户体系中，获取关于用户的登录信息。
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "加载外部登录信息出错！");
                return View("Login", loginViewModel);
            }
            // 获取邮箱地址
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;
            if (email != null)
            {
                // 通过邮箱地址去查询用户是否已存在
                user = await _userManager.FindByEmailAsync(email);

                // 如果电子邮件没有被确认，返回登录视图与验证错误
                if(user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "您的电子邮箱还未进行验证!");

                    return View("Login", loginViewModel);
                }
            }

            //如果用户之前已经登录过了，会在AspNetUserLogins表有对应的记录，这个时候无需创建新的记录，直接使用当前记录登录系统即可。
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,info.ProviderKey,isPersistent:false,bypassTwoFactor:true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            //如果AspNetUserLogins表中没有记录，则代表用户没有一个本地帐户，这个时候我们就需要创建一个记录了。 
            else
            {
                //获取邮箱地址
                //var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    // 通过邮箱地址去查询用户是否已存在
                    //var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new ApplicationUser 
                        { 
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        };
                        //如果不存在，则创建一个用户，但是这个用户没有密码
                        await _userManager.CreateAsync(user);
                    }
                    // 在AspNetUserLogins表中,添加一行用户数据，然后将当前用户登录到系统中
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent:false);

                    return LocalRedirect(returnUrl);
                }

                // 如果我们获取不到电子邮件地址，我们需要将请求重定向到错误视图中。
                ViewBag.ErrorTitle = $"我们无法从提供商:{info.LoginProvider}中解析到您的邮件地址 ";
                ViewBag.ErrorMessage = "请通过联系 ltm@ddxc.org 寻求技术支持。";

                return View("Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"当前{userId}无效";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "您的电子邮箱还未进行验证";
            return View("Error");
        }
    }
}
