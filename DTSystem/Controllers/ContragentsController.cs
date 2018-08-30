using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DTSystem.Models;
using Newtonsoft.Json;

namespace DTSystem.Controllers
{
    public class ContragentsController : Controller
    {
        private IntersectionDBEntities db = new IntersectionDBEntities();

        // GET: Contragents
        public ActionResult Index()
        {
            return View(db.Contragent.ToList());
        }

        // GET: Contragents/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contragent contragent = db.Contragent.Find(id);
            if (contragent == null)
            {
                return HttpNotFound();
            }
            return View(contragent);
        }

        // GET: Contragents/Create
        public ActionResult Create()
        {
            ViewBag.propList = db.Property.ToList();                   //id, name
            return View();
        }

        // POST: Contragents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Contragent contragent)
        {
          
            if (ModelState.IsValid)
            {
                db.Contragent.Add(contragent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            ViewBag.propList = db.Property.ToList();                   //id, name
            return View(contragent);
        }

        // GET: Contragents/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contragent contragent = db.Contragent.Find(id);
            if (contragent == null)
            {
                return HttpNotFound();
            }
            string json = contragent.Properties;
            ViewBag.propList = db.Property.ToList();                   //id, name
            try
            {
                ViewBag.propValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch
            {
            }
            return View(contragent);
        }

        // POST: Contragents/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Contragent contragent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contragent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contragent);
        }

        // GET: Contragents/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contragent contragent = db.Contragent.Find(id);
            if (contragent == null)
            {
                return HttpNotFound();
            }
            return View(contragent);
        }

        // POST: Contragents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Contragent contragent = db.Contragent.Find(id);
            db.Contragent.Remove(contragent);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
