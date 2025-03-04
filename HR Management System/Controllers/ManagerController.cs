using HR_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Crypto.Generators;
using ClosedXML.Excel;


using HR_Management_System.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;




//using HR_Management_System.Models;

namespace ManagerDashbord.Controllers
{
    public class ManagerController : Controller
    {
        private readonly HrmanagementSystemContext _context;
        private readonly EmailService _emailService;

        public ManagerController(HrmanagementSystemContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        public async Task<IActionResult> Employees()
        {

            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();

            var lastEvaluations = await _context.Evaluations
                .GroupBy(e => e.EmployeeId)
                .Select(g => g.OrderByDescending(e => e.Id).FirstOrDefault())
                .ToDictionaryAsync(e => e.EmployeeId);

            ViewBag.LastEvaluations = lastEvaluations;


            var pendingLeaves = await _context.LeaveRequests
                .Where(l => l.Status == "Pending")
                .GroupBy(l => l.EmployeeId)
                .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.EmployeeId, g => g.Count);

            ViewBag.PendingLeaves = pendingLeaves;

            return View(employees);
        }





        public async Task<IActionResult> ViewTasks(int id)
        {
            var tasks = await _context.Tasks
     .Include(t => t.AssignedToEmployee)
     .Where(t => t.AssignedToEmployeeId == id)
     .ToListAsync();


            return View(tasks);
        }



        [HttpPost]
        public async Task<IActionResult> EditTask(HR_Management_System.Models.Task task)
        {
            if (task == null || task.Id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid task.";
                return RedirectToAction("ViewTasks", new { id = task.AssignedToEmployeeId });
            }

            var existingTask = await _context.Tasks.FindAsync(task.Id);
            if (existingTask == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction("ViewTasks", new { id = task.AssignedToEmployeeId });
            }

            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;

            try
            {
                _context.Tasks.Update(existingTask);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Task updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating task: " + (ex.InnerException?.Message ?? ex.Message);
            }


            return RedirectToAction("ViewTasks", new { id = existingTask.AssignedToEmployeeId });
        }



        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                TempData["ErrorMessage"] = "Task not found.";
                return RedirectToAction("ViewTasks");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Task deleted successfully!";

            return RedirectToAction("ViewTasks");
        }




        public async Task<IActionResult> ViewAttendance(int id)
        {
            var attendance = await _context.Attendances.Where(a => a.EmployeeId == id).ToListAsync();
            return View(attendance);
        }


        [HttpGet]
        public IActionResult AddEmployee()
        {
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name");
            ViewBag.Managers = new SelectList(_context.Employees.Where(e => e.Position == "Manager"), "Id", "Name");
            return View();
        }


        [HttpPost]
        //public async Task<IActionResult> AddEmployee(Employee employee)

        public async Task<IActionResult> AddEmployee(Employee employee, IFormFile ImageFile)


        {
            try
            {

                //if (string.IsNullOrEmpty(employee.PasswordHash))
                //{
                //    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mohammad2002");
                //}

                if (string.IsNullOrEmpty(employee.PasswordHash))
                {
                    employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employee.PasswordHash);
                }


                if (ImageFile != null)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        ImageFile.CopyTo(stream);
                    }

                    employee.ProfileImage = fileName;
                }


                employee.Position = string.IsNullOrEmpty(employee.Position) ? "Employee" : employee.Position;
                employee.ProfileImage = string.IsNullOrEmpty(employee.ProfileImage) ? "default.jpg" : employee.ProfileImage;
                employee.Address = string.IsNullOrEmpty(employee.Address) ? "Not Provided" : employee.Address;
                employee.CreatedAt = DateTime.Now;


                ////employee.ProfileImage = employee.ProfileImage ?? "default.jpg"; // إذا كانت null سيتم تعيين default.jpg
                //employee.Position = string.IsNullOrEmpty(employee.Position) ? "Employee" : employee.Position;
                //employee.ProfileImage = string.IsNullOrEmpty(employee.ProfileImage) ? "default.jpg" : employee.ProfileImage;
                //employee.Address = string.IsNullOrEmpty(employee.Address) ? "Not Provided" : employee.Address;
                //employee.CreatedAt = DateTime.Now;


                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee added successfully!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error adding employee: " + ex.Message;
                return View(employee);
            }
        }






        [HttpGet]
        public IActionResult AssignTask(int id, string name)
        {
            ViewBag.EmployeeId = id;
            ViewBag.EmployeeName = name;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignTask(HR_Management_System.Models.Task task, string Title, string Description, DateOnly StartDate, DateOnly DueDate)
        {
            if (task == null || task.AssignedToEmployeeId <= 0)
            {
                ViewBag.ErrorMessage = "Invalid Employee.";
                return View(task);
            }

            var employee = await _context.Employees.FindAsync(task.AssignedToEmployeeId);
            if (employee == null)
            {
                ViewBag.ErrorMessage = "Employee not found.";
                return View(task);
            }

            var newTask = new HR_Management_System.Models.Task
            {
                Title = Title,
                Description = Description,
                AssignedToEmployeeId = task.AssignedToEmployeeId,
                Status = "To Do",
                StartDate = StartDate,
                DueDate = DueDate,
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Tasks.Add(newTask);
                await _context.SaveChangesAsync();

                // 🔹 إرسال البريد الإلكتروني بعد حفظ المهمة
                try
                {
                    string subject = "New Task Assigned: " + Title;
                    string body = $@"
                        <h3>Dear {employee.Name},</h3>
                        <p>You have been assigned a new task: <b>{Title}</b></p>
                        <p>Description: {Description}</p>
                        <p><b>Start Date:</b> {StartDate}</p>
                        <p><b>Due Date:</b> {DueDate}</p>
                        <p><b>Status:</b> Doing</p>
                        <br>
                        <p>Best regards,</p>
                        <p>HR Management System</p>";

                    await _emailService.SendEmailAsync(employee.Email, subject, body);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine("Email sending failed: " + emailEx.Message);
                }

                TempData["SuccessMessage"] = "Task assigned successfully, and email notification sent!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error saving task: " + (ex.InnerException?.Message ?? ex.Message);
                return View(task);
            }
        }












        public async Task<IActionResult> ReviewLeaves(int id, string name)
        {
            var leaveRequests = await _context.LeaveRequests
                .Where(l => l.EmployeeId == id)
                .OrderBy(l => l.Status == "Pending" ? 0 : 1)
                .ThenByDescending(l => l.StartTime)
                .ToListAsync();

            ViewBag.EmployeeName = name;
            ViewBag.EmployeeId = id;

            return View(leaveRequests);
        }




        [HttpPost]
        public async Task<IActionResult> ApproveLeave(int id, int employeeId, string name)
        {
            var leaveRequest = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id);
            if (leaveRequest != null)
            {
                leaveRequest.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Leave request for {name} has been approved.";
            }

            return RedirectToAction("ReviewLeaves", new { id = employeeId, name = name });
        }

        [HttpPost]
        public async Task<IActionResult> RejectLeave(int id, int employeeId, string name)
        {
            var leaveRequest = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id);
            if (leaveRequest != null)
            {
                leaveRequest.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Leave request for {name} has been rejected.";
            }

            return RedirectToAction("ReviewLeaves", new { id = employeeId, name = name });
        }







        [HttpGet]
        public async Task<IActionResult> EvaluateEmployee()
        {
            var currentYear = DateTime.Now.Year;


            var evaluatedEmployeeIds = await _context.Evaluations
                .Where(e => e.DateEvaluate.Value.Year == currentYear)
                .Select(e => e.EmployeeId)
                .ToListAsync();


            var employees = await _context.Employees
                .Where(emp => !evaluatedEmployeeIds.Contains(emp.Id))
                .ToListAsync();

            ViewBag.Employees = employees;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> EvaluateEmployee(Evaluation evaluation, List<int> Scores)
        {
            if (evaluation.EmployeeId <= 0)
            {
                ViewBag.Employees = await _context.Employees.ToListAsync();
                ViewBag.ErrorMessage = "Please select a valid employee.";
                return View(evaluation);
            }

            var currentYear = DateTime.Now.Year;

            var existingEvaluation = await _context.Evaluations
                .FirstOrDefaultAsync(e => e.EmployeeId == evaluation.EmployeeId && e.DateEvaluate.Value.Year == currentYear);


            int totalScore = Scores.Sum();


            if (totalScore >= 40)
                evaluation.Status = "Excellent";
            else if (totalScore >= 30)
                evaluation.Status = "Very Good";
            else if (totalScore >= 20)
                evaluation.Status = "Good";
            else
                evaluation.Status = "Fair";

            evaluation.DateEvaluate = DateTime.Now;

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Evaluation submitted successfully!";
            return RedirectToAction("Employees");
        }


        public IActionResult DownloadEmployeeReport()
        {
            // var employees = _context.Employees.ToList();

            var employees = _context.Employees
            .Include(e => e.Department)
            .ToList();


            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, stream);
                document.Open();

                document.Add(new Paragraph("Employee Report"));
                document.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(3);
                table.AddCell("Name");
                table.AddCell("Email");
                table.AddCell("Department");

                foreach (var emp in employees)
                {
                    table.AddCell(emp.Name);
                    table.AddCell(emp.Email);
                    string departmentName = emp.Department != null ? emp.Department.Name : "No Department";
                    table.AddCell(departmentName);


                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "EmployeeReport.pdf");
            }
        }



        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(Employee employee)
        {
            try
            {
                var existingEmployee = await _context.Employees.FindAsync(employee.Id);
                if (existingEmployee == null)
                {
                    ViewBag.ErrorMessage = "Employee not found.";
                    return View(employee);
                }

                // تحديث بيانات الموظف
                existingEmployee.Name = employee.Name;
                existingEmployee.Email = employee.Email;
                existingEmployee.DepartmentId = employee.DepartmentId;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee details updated successfully!";
                return RedirectToAction("Employees");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error saving employee details: " + ex.Message;
                return View(employee);
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();


                TempData["SuccessMessage"] = "Employee deleted successfully!";
            }
            return RedirectToAction("Employees");
        }



        public IActionResult ExportEmployeeData()
        {
            var employees = _context.Employees.Select(e => new
            {
                e.Name,
                e.Email,
                Department = e.Department.Name
            }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employees");
                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Email";
                worksheet.Cell(1, 3).Value = "Department";

                for (int i = 0; i < employees.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = employees[i].Name;
                    worksheet.Cell(i + 2, 2).Value = employees[i].Email;
                    worksheet.Cell(i + 2, 3).Value = employees[i].Department;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
                }
            }
        }


        public async Task<IActionResult> Dashboard()
        {
            var totalTasks = await _context.Tasks.CountAsync();
            var totalAbsences = await _context.Attendances.Where(a => a.PunchInTime == TimeOnly.MinValue).CountAsync();
            var totalLeaves = await _context.LeaveRequests.CountAsync();
            var totalEmployees = await _context.Employees.CountAsync();
            var totalDepartments = await _context.Departments.CountAsync();

            ViewBag.TotalTasks = totalTasks;
            ViewBag.TotalAbsences = totalAbsences;
            ViewBag.TotalLeaves = totalLeaves;
            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.TotalDepartments = totalDepartments;

            return View();
        }





        public async Task<IActionResult> ViewVaccation(int id, string name)
        {
            var leaveRequests = await _context.VaccationRequests.ToListAsync();


            var vacations = await _context.VaccationRequests
         .Include(v => v.Employee)
         .Where(v => v.Status == "Pending")
         .ToListAsync();



            return View(leaveRequests);
        }

       
        public async Task<IActionResult> Approved(int Id)
        {
            var vacation = await _context.VaccationRequests.FindAsync(Id);
            if (vacation != null)
            {
                vacation.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vacation request approved successfully!";
            }
            return RedirectToAction("ViewVaccation");
        }


        public async Task<IActionResult> Rejected(int Id)
        {
            var vacation = await _context.VaccationRequests.FindAsync(Id);
            if (vacation != null)
            {
                vacation.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vacation request Rejected successfully!";
            }
            return RedirectToAction("ViewVaccation");
        }











    }
}
