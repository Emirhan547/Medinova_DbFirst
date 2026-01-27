using System.Linq;
using System.Web.Mvc;
using Medinova.Models;
using System.Data.Entity;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        [HttpGet]
        public JsonResult Popular()
        {
            var popularDoctorIds = context.Appointments
                .Where(a => a.DoctorId.HasValue)
                .GroupBy(a => a.DoctorId.Value)
                .OrderByDescending(g => g.Count())
                .Take(4)
                .Select(g => g.Key)
                .ToList();

            var popularDoctors = context.Doctors
                .Include(d => d.Department) // 🔥 KRİTİK SATIR
                .Where(d => popularDoctorIds.Contains(d.DoctorId))
                .OrderBy(d => d.DoctorId)
                .Select(d => new
                {
                    d.DoctorId,
                    d.FullName,
                    DepartmentName = d.Department != null
                        ? d.Department.Name
                        : "Bölüm belirtilmemiş"
                })
                .ToList();

            if (!popularDoctors.Any())
            {
                popularDoctors = context.Doctors
                    .Include(d => d.Department)
                    .OrderBy(d => d.DoctorId)
                    .Take(4)
                    .Select(d => new
                    {
                        d.DoctorId,
                        d.FullName,
                        DepartmentName = d.Department != null
                            ? d.Department.Name
                            : "Bölüm belirtilmemiş"
                    })
                    .ToList();
            }

            var popularServices = context.Services
                .OrderBy(s => s.SeviceId)
                .Take(4)
                .Select(s => new
                {
                    s.SeviceId,
                    s.Title
                })
                .ToList();

            return Json(new
            {
                doctors = popularDoctors,
                services = popularServices
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Search(string keyword, int? departmentId)
        {
            keyword = (keyword ?? "").Trim();

            var query = context.Doctors
                .Include(d => d.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.FullName.Contains(keyword) ||
                    d.Department.Name.Contains(keyword));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            var doctors = query
                .OrderBy(d => d.FullName)
                .Take(10)
                .Select(d => new
                {
                    d.DoctorId,
                    d.FullName,
                    DepartmentName = d.Department != null
                        ? d.Department.Name
                        : "Bölüm belirtilmemiş"
                })
                .ToList();

            return Json(new
            {
                doctors = doctors
            }, JsonRequestBehavior.AllowGet);
        }

    }
}