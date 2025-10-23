using Microsoft.AspNetCore.Mvc;
using StudentApp.Models;
using StudentApp.Services;

namespace StudentApp.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // GET: Student
        public async Task<IActionResult> Index()
        {
            var students = await _studentService.GetAllStudentsAsync();
            return View(students);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _studentService.AddStudentAsync(student);
                    TempData["SuccessMessage"] = "Öğrenci başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Öğrenci eklenirken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(student);
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updatedStudent = await _studentService.UpdateStudentAsync(student);
                    if (updatedStudent == null)
                    {
                        return NotFound();
                    }

                    TempData["SuccessMessage"] = "Öğrenci başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Öğrenci güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _studentService.DeleteStudentAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Öğrenci başarıyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Öğrenci bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Öğrenci silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
