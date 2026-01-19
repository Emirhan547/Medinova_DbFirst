using Medinova.Attributes;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [CustomAuthorize("Admin")]
    public class SearchSectionController : Controller
    {
        MedinovaContext context =new MedinovaContext();
        public ActionResult Index()
        {
            var searchSections=context.SearchSections.ToList();
            return View(searchSections);
        }
        [HttpGet]
        public ActionResult CreateSearchSection()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateSearchSection(SearchSection searchSection)
        {
            context.SearchSections.Add(searchSection);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateSearchSection(int id)
        {
            var values = context.SearchSections.Find(id);
            return View(values);
        }
        [HttpPost]
        public ActionResult UpdateSearchSection(SearchSection searchSection)
        {
            var values = context.SearchSections.Find(searchSection.SearchSectionId);
            values.Title = searchSection.Title;
            values.Description = searchSection.Description;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeleteSearchSection(int id)
        {
            var searchSection=context.SearchSections.Find(id);
            context.SearchSections.Remove(searchSection);
            context.SaveChanges();
            return RedirectToAction("Index");   
        }
    }
}