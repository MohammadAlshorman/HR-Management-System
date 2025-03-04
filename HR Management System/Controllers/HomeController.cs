using System.Diagnostics;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using static System.Net.WebRequestMethods;


namespace HR_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HrmanagementSystemContext _context;

        public HomeController(HrmanagementSystemContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult OurTeam()
        {
            return View();
        }

        // هذه هي النقطة الرئيسية لحفظ البيانات في قاعدة البيانات
        [HttpPost]
        //public async Task<IActionResult> Contact(Feedback feedback)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // تعيين التاريخ الحالي لحقل ReceivedAt
        //        feedback.ReceivedAt = DateTime.Now;

        //        // إضافة البيانات إلى قاعدة البيانات
        //        _context.Feedbacks.Add(feedback);
        //        await _context.SaveChangesAsync();

        //        // إعادة التوجيه إلى صفحة "شكرًا لك" بعد إرسال البيانات
        //        return View(feedback);
        //    }

        //    // إذا كانت البيانات غير صالحة، يتم إعادة عرض النموذج مع الأخطاء
        //    return View(feedback);
        //}

        //// صفحة الشكر بعد الإرسال الناجح
        ////public IActionResult ThankYou()
        ////{
        ////    return View();
        ////}


        [HttpPost]
        public async Task<IActionResult> Contact(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.ReceivedAt = DateTime.Now;
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // إضافة رسالة إلى ViewData لإظهارها في الـ View بعد إرسال البيانات بنجاح
                TempData["Message"] = "Your message has been sent successfully.";

                return RedirectToAction("Contact");  // إعادة التوجيه إلى نفس الصفحة بعد الإرسال الناجح
            }

            return View(feedback);
        }

        public IActionResult Testimonials()
        {
            return View();
        }

        public IActionResult Accounts()
        {
            return View();
        }

        public IActionResult HRLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HRLogin(string username, string password)
        {
            // البحث عن موظف HR في قاعدة البيانات
            var hrUser = _context.Hrs.FirstOrDefault(hr => hr.Email == username && hr.PasswordHash == password);

            if (hrUser != null)
            {
                // تخزين معلومات HR في الجلسة
                HttpContext.Session.SetInt32("HRId", hrUser.Id);
                HttpContext.Session.SetString("HRName", hrUser.Name);

                //HttpContext.Session.SetString("Role", "HR");
                //HttpContext.Session.SetString("ProfileImage", hrUser.ProfileImage);

                // توجيهه إلى صفحة التحكم الخاصة بـ HR
                return RedirectToAction("Dashboard", "HR");
            }

            ViewBag.Error = "Incorrect username or password";
            return View();
        }


        public IActionResult ManagersLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ManagersLogin(string username, string password)
        {
            var manager = _context.Managers.FirstOrDefault(m => m.Email == username);

            if (manager != null && BCrypt.Net.BCrypt.Verify(password, manager.PasswordHash))
            {
                if (manager != null)
                {
                    // تخزين معلومات المدير في الجلسة
                    HttpContext.Session.SetInt32("ManagerId", manager.Id);
                    HttpContext.Session.SetString("ManagerName", manager.Name);

                    //HttpContext.Session.SetString("Role", "Manager");
                    //HttpContext.Session.SetString("ProfileImage", manager.ProfileImage);

                    // توجيه المستخدم إلى لوحة التحكم
                    return RedirectToAction("Dashboard", "Manager");
                }

            }
            ViewBag.Error = "Incorrect username or password";
            return View();
        }


        public IActionResult EmployeesLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmployeesLogin(string username, string password, bool rememberme)
        {
            //var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == username);

            var employee = _context.Employees.FirstOrDefault(e => e.Email == username && e.PasswordHash==password);

            //var manager = _context.Managers.FirstOrDefault(m => m.Email == username);

            //if (employee != null && BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash))
            //{

            if (employee == null)
                {
                    ViewBag.Error = "Incorrect username or password";
                    return View();
                }
            //}
            // التحقق مما إذا كانت كلمة المرور مشفرة بشكل صحيح
            //if (string.IsNullOrEmpty(employee.PasswordHash) || !employee.PasswordHash.StartsWith("$2a$10$"))
            //{
            //    ViewBag.Error = "Your password is outdated. Please reset your password.";
            //    return View();
            //}

            try
            {
                //bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash);

                //if (isPasswordCorrect)
                //{
                    // تخزين معلومات الموظف في الجلسة
                    HttpContext.Session.SetInt32("EmployeeId", employee.Id);
                    HttpContext.Session.SetString("EmployeeName", employee.Name);

                    //HttpContext.Session.SetString("Role", "employee");
                    //HttpContext.Session.SetString("ProfileImage", employee.ProfileImage);

                    if (rememberme)
                    {
                        CookieOptions options = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = true
                        };

                        Response.Cookies.Append("EmployeeId", employee.Id.ToString(), options);
                        Response.Cookies.Append("EmployeeName", employee.Name, options);
                    }

                    return RedirectToAction("EmpVacRequest", "Employees");
                //}
                //else
                //{
                //    ViewBag.Error = "Incorrect username or password";
                //    return View();
                //}
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while verifying your password. Please try again later.";
                Console.WriteLine("Error: " + ex.Message); // طباعة الخطأ في الـ Console
                return View();
            }
        }




        public IActionResult PasswordResetn()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PasswordResetn(string email)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

            if (user == null)
            {
                ViewBag.Error = "No account found with this email.";
                return View();
            }

            HttpContext.Session.SetString("userEmail", email);

            string resetToken = Guid.NewGuid().ToString();

            // حفظ الرمز في قاعدة البيانات (أضف حقلًا في جدول `Employees` إن لم يكن موجودًا)
            user.PasswordHash = resetToken;
            await _context.SaveChangesAsync();

            // رابط إعادة التعيين
            string resetLink = "https://localhost:7138/Home/ResetPassword";

            // إرسال البريد الإلكتروني
            SendResetEmail(user.Email, resetLink);

            ViewBag.Message = "A password reset link has been sent to your email.";
            return View();
        }

        public void SendResetEmail(string email, string resetLink)
        {
            try
            {
                var fromAddress = new MailAddress("mhmdshhadhalshrman95@gmail.com", "HR Management");
                var toAddress = new MailAddress(email);
                const string subject = "Password Reset Request";
                string body = $"Click the link below to reset your password:\n {"https://localhost:7138/Home/ResetPassword"}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // SMTP لجوجل
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("mhmdshhadhalshrman95@gmail.com", "hrqh bggv oxxl huas") // استبدل بكلمة المرور الخاصة بالتطبيق
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false // اجعله `true` إذا كنت تريد إرسال رسالة HTML
                })
                {
                    smtp.Send(message);
                }

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // تأكيد أمان النموذج
        public async Task<IActionResult> ResetPassword(string PasswordHash)
        {
            if (string.IsNullOrEmpty(PasswordHash))
            {
                ViewBag.Error = "Invalid request. Please try again.";
                return View();
            }


            var userEmail = HttpContext.Session.GetString("userEmail");

            var user = await _context.Employees.FirstOrDefaultAsync(e => e.Email == userEmail);

            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
                _context.Update(user);

            }
            else
            {
                var user2 = await _context.Managers.FirstOrDefaultAsync(e => e.Email == userEmail);
                if (user2 != null)
                {
                    user2.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
                    _context.Update(user2);

                }
                else
                {
                    var user3 = await _context.Hrs.FirstOrDefaultAsync(e => e.Email == userEmail);
                    user3.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
                    _context.Update(user3);

                }
            }

            // تحديث كلمة المرور وتشفيرها

            await _context.SaveChangesAsync();

            ViewBag.Message = "Your password has been successfully reset. You can now log in with your new password.";
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("EmployeeId");
            int? userIdManager = HttpContext.Session.GetInt32("ManagerId");
            int? userIdHr = HttpContext.Session.GetInt32("HRId");
            string role = HttpContext.Session.GetString("Role");

            //if (!userId.HasValue || string.IsNullOrEmpty(role))
            //{
            //    return RedirectToAction("EmployeesLogin", "Home");
            //}

            object user = null;

            if (role == "employee")
            {
                user = await _context.Employees.FirstOrDefaultAsync(e => e.Id == userId.Value);
                return RedirectToAction("EmployeeProfile");
            }
            else if (role == "Manager")
            {
                user = await _context.Managers.FirstOrDefaultAsync(m => m.Id == userIdManager.Value);
                return RedirectToAction("ManagerProfile");
            }
            else if (role == "HR")
            {
                user = await _context.Hrs.FirstOrDefaultAsync(hr => hr.Id == userIdHr.Value);
                return RedirectToAction("HRProfile");
            }

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found!";
                return RedirectToAction("Dashboard", "Home");
            }

            return View(user);
        }




        [HttpGet]
        public async Task<IActionResult> EmployeeProfile()
        {
            int? userId = HttpContext.Session.GetInt32("EmployeeId");
            var employee = await _context.Employees
        .Include(e => e.Department)
        .FirstOrDefaultAsync(e => e.Id == userId.Value);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found!";
                return RedirectToAction("Dashboard", "Home");
            }

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> ManagerProfile()
        {
            int? userIdManager = HttpContext.Session.GetInt32("ManagerId");
            string role = HttpContext.Session.GetString("Role");

            if (!userIdManager.HasValue || role != "Manager")
            {
                return RedirectToAction("EmployeesLogin", "Home");
            }


            var manager = await _context.Managers.FirstOrDefaultAsync(e => e.Id == userIdManager.Value);

            if (manager == null)
            {
                TempData["ErrorMessage"] = "Manager not found!";
                return RedirectToAction("Dashboard", "Home");
            }

            return View(manager);
        }

        [HttpGet]
        public async Task<IActionResult> HRProfile()
        {
            int? HRsID = HttpContext.Session.GetInt32("HRId");
            string role = HttpContext.Session.GetString("Role");

            //if (!HRsID.HasValue || role != "HR")
            //{
            //    return RedirectToAction("EmployeesLogin", "Home");
            //}

            var HR = await _context.Hrs.FindAsync(HRsID);


            //if (HRsID == null)
            //{
            //    TempData["ErrorMessage"] = "HR not found!";
            //    return RedirectToAction("Dashboard", "Home");
            //}

            return View(HR);
        }






        //[HttpPost]
        //public async Task<IActionResult> UpdateProfile(string Name, string Email, string Position, string Address , IFormFile ImageFile)
        //{
        //    int? hrId = HttpContext.Session.GetInt32("HRId");
        //    int? managerId = HttpContext.Session.GetInt32("ManagerId");
        //    int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
        //    string fileName = "";

        //    if (ImageFile != null)
        //    {
        //        fileName = Path.GetFileName(ImageFile.FileName);
        //        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

        //        using (var stream = new FileStream(path, FileMode.Create))
        //        {
        //            ImageFile.CopyTo(stream);
        //        }


        //    }

        //    if (hrId.HasValue)
        //    {
        //        var hrUser = await _context.Hrs.FirstOrDefaultAsync(hr => hr.Id == hrId.Value);
        //        if (hrUser != null)
        //        {
        //            hrUser.Name = Name;
        //            hrUser.Email = Email;
        //            hrUser.Address = Address;
        //            hrUser.ProfileImage = fileName;
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction("HRProfile");
        //        }
        //    }

        //    if (managerId.HasValue)
        //    {
        //        var manager = await _context.Managers.FirstOrDefaultAsync(m => m.Id == managerId.Value);
        //        if (manager != null)
        //        {
        //            manager.Name = Name;
        //            manager.Email = Email;
        //            // manager.Position = Position;
        //            manager.Address = Address;
        //            manager.ProfileImage = fileName;
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction("ManagerProfile");
        //        }
        //    }

        //    if (employeeId.HasValue)
        //    {
        //        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId.Value);
        //        if (employee != null)
        //        {
        //            employee.Name = Name;
        //            employee.Email = Email;
        //            employee.Position = Position;
        //            employee.Address = Address;
        //            employee.ProfileImage = fileName;
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction("EmployeeProfile");
        //        }
        //    }

        //    return RedirectToAction("Profile");
        //}


        [HttpGet]
        public async Task<IActionResult> UpdateProfileHR(int Id)
        {
            var hr = await _context.Hrs.FindAsync(Id);

            if (hr == null)
            {
                TempData["ErrorMessage"] = "HR User not found!";
                return RedirectToAction("HRProfile");
            }

            return View(hr);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfileHR(Hr hr, IFormFile ImageFile)
        {
            var existingHR = await _context.Hrs.FindAsync(hr.Id);

            if (existingHR == null)
            {
                TempData["ErrorMessage"] = "HR User not found!";
                return RedirectToAction("HRProfile");
            }

            // تحديث البيانات الأساسية
            existingHR.Name = hr.Name;
            existingHR.Email = hr.Email;
            existingHR.Address = hr.Address;

            if (ImageFile != null)
            {
                string fileName = Path.GetFileName(ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // تحديث صورة البروفايل فقط إذا تم تحميل صورة جديدة
                existingHR.ProfileImage = "/img/" + fileName;
            }

            _context.Hrs.Update(existingHR);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("HRProfile");
        }


        [HttpGet]
        public async Task<IActionResult> UpdateProfileEmployee(int Id)
        {
            var employee = await _context.Employees.FindAsync(Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found!";
                return RedirectToAction("EmployeeProfile");
            }

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileEmployee(Employee employee, IFormFile ImageFile)
        {
            var existingEmployee = await _context.Employees.FindAsync(employee.Id);

            if (existingEmployee == null)
            {
                TempData["ErrorMessage"] = "Employee not found!";
                return RedirectToAction("EmployeeProfile");
            }

            // تحديث البيانات الأساسية
            existingEmployee.Name = employee.Name;
            existingEmployee.Email = employee.Email;
            existingEmployee.Address = employee.Address;

            if (ImageFile != null)
            {
                string fileName = Path.GetFileName(ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingEmployee.ProfileImage = fileName;
            }

            _context.Employees.Update(existingEmployee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("EmployeeProfile");
        }


        [HttpGet]
        public async Task<IActionResult> UpdateProfileManager(int Id)
        {
            var manager = await _context.Managers.FindAsync(Id);

            if (manager == null)
            {
                TempData["ErrorMessage"] = "Manager not found!";
                return RedirectToAction("ManagerProfile");
            }

            return View(manager);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileManager(Models.Manager manager, IFormFile ImageFile)
        {
            var existingManager = await _context.Managers.FindAsync(manager.Id);

            if (existingManager == null)
            {
                TempData["ErrorMessage"] = "Manager not found!";
                return RedirectToAction("ManagerProfile");
            }

            // تحديث البيانات الأساسية
            existingManager.Name = manager.Name;
            existingManager.Email = manager.Email;
            existingManager.Address = manager.Address;

            if (ImageFile != null)
            {
                string fileName = Path.GetFileName(ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingManager.ProfileImage = fileName;
            }

            _context.Managers.Update(existingManager);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("ManagerProfile");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }









    }
}
