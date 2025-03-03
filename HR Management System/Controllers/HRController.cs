using HR_Management_System.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;
using DocumentFormat.OpenXml.InkML;


namespace HR_Management_System.Controllers
{
    public class HRController : Controller
    {
        private readonly HrmanagementSystemContext DB;

        public HRController(HrmanagementSystemContext db)
        {
            DB = db;
        }
        // GET: HRController
        public ActionResult ViewManager()

        {
            var manegers = DB.Managers.Include(e => e.Department).ToList();

            return View(manegers);
        }

        // GET: HRController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            // حساب إجمالي الموظفين
            int totalEmployees = DB.Employees.Count();

            // حساب إجمالي المديرين
            int totalManagers = DB.Managers.Count();

            // حساب إجمالي الأقسام
            int totalDepartments = DB.Departments.Count();

            // حساب عدد الإجازات المعلقة
            int pendingVacations = DB.VaccationRequests.Count(v => v.Status == "Pending");

            // تمرير القيم إلى View باستخدام ViewData
            ViewData["TotalEmployees"] = totalEmployees;
            ViewData["TotalManagers"] = totalManagers;
            ViewData["TotalDepartments"] = totalDepartments;
            ViewData["PendingVacations"] = pendingVacations;

            // إعادة عرض الـ Dashboard
            return View();
        }



        // GET: HRController/Create
        public ActionResult CreateManeger()
        {
            ViewBag.Department = new SelectList(DB.Departments, "Id", "Name");

            return View();
        }

        // POST: HRController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateManeger(Manager maneger, int dep)
        {


            maneger.Id = 0;
            maneger.DepartmentId = dep;
            maneger.PasswordHash = BCrypt.Net.BCrypt.HashPassword(maneger.PasswordHash);
            maneger.CreatedAt = System.DateTime.Now;
            DB.Managers.Add(maneger);
            DB.SaveChanges();
            return RedirectToAction(nameof(ViewManager));
        }


        // GET: HRController/Create
        public ActionResult CreateDepartment()
        {
            ViewBag.Maneagers = new SelectList(DB.Managers, "Id", "Name");

            return View();
        }

        // POST: HRController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDepartment(Department department)
        {

            department.CreatedAt = System.DateTime.Now;
            DB.Departments.Add(department);
            DB.SaveChanges();

            return RedirectToAction(nameof(ViewManager));


        }

        public ActionResult ViewDepartment()
        {
            var Department = DB.Departments.ToList();
            return View(Department);
        }

        public ActionResult ViewEmployee()
        {

            var Employee = DB.Employees.Include(e => e.Department).Include(e => e.Manager).ToList();
            return View(Employee);
        }

        public ActionResult ViewVaccation()
        {
            var Vaccation = DB.VaccationRequests.Include(e => e.Employee).ToList();

            //var Vaccation = DB.VaccationRequests.ToList();
            return View(Vaccation);
        }

        //public ActionResult ViewManager()
        //{
        //    var Manager = DB.Managers.ToList();

        //    return View(Manager);
        //}

        public ActionResult ViewEvaluation()
        {
            var evaluation = DB.Evaluations.Include(e => e.Employee).ToList();

            return View(evaluation);
        }

        public ActionResult ViewFeedback()
        {
            var feedback = DB.Feedbacks.ToList();

            return View(feedback);
        }


        public IActionResult ViewFeedbackSorted(string sortOrder)
        {
            // تعيين قيمة الترتيب الافتراضي
            ViewData["ReceivedAtSortOrder"] = sortOrder == "desc" ? "asc" : "desc";

            var feedbacks = from f in DB.Feedbacks
                            select f;

            // ترتيب البيانات حسب التاريخ بناءً على الترتيب المختار
            switch (sortOrder)
            {
                case "asc":
                    feedbacks = feedbacks.OrderBy(f => f.ReceivedAt);
                    break;
                case "desc":
                    feedbacks = feedbacks.OrderByDescending(f => f.ReceivedAt);
                    break;
                default:
                    feedbacks = feedbacks.OrderBy(f => f.ReceivedAt);
                    break;
            }

            return View("ViewFeedback", feedbacks.ToList());  // العودة إلى نفس الـ View
        }


    }
}
