using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DTSystem.Models;
using System.IO;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Interface;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace DTSystem.Controllers
{

    public interface IParameterValue
    {
        string Name { get; }
        string Value { get; }
    }

    public class ParameterValue : IParameterValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public struct JsonPartsLists
    {
        public List<string> plainFields;
        public List<string> complexFields;
        public List<string> propertiesFields;
    }

    public struct JsonParts
    {
        public Dictionary<string, string> plainFields;
        public Dictionary<string, int> complexFields;
    }
    public struct StructforId
    {
        public long Id;
        public string Name;

        public StructforId(long Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }

    public class DocsController : Controller
    {
        private IntersectionDBEntities db = new IntersectionDBEntities();
        private readonly Regex _regexPlain = new Regex(@"{{\s*([-№()'єЄїЇіІа-яА-Яa-zA-Z0-9_ ,\.]*?)\s*}}");
        private readonly Regex _regexComplex = new Regex(@"{{\s*([-№()'єЄїЇіІа-яА-Яa-zA-Z0-9_ ,\.]*?)\s*:\s*([-№()'єЄїЇіІа-яА-Яa-zA-Z0-9_ ,\.]*?)\s*}}");
        
        // GET: Docs
        public ActionResult Index()
        {
            return View(db.Doc.ToList());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(Doc doc, JsonParts jsonparts)
        {

            HashSet<string> placeholders = new HashSet<string>();
            HashSet<string> contragentsHolder = new HashSet<string>();
            string concatJson = doc.PlaceHolders;
            
            var concatValues = JsonConvert.DeserializeObject<JsonParts>(concatJson);
            ViewBag.placeholders = placeholders;
            ViewBag.contragentsHolder = contragentsHolder;
            var doci = db.Doc.Find(doc.Id);
            Stream stream = new System.IO.MemoryStream();

            Document document1 = new Document();
            document1.LoadFromFile(doci.FilePath);

            var variables = concatValues.plainFields;
            var variables2 = concatValues.complexFields;
            
            var selections = document1.FindAllPattern(_regexPlain);
            if (selections != null)
                try
                {
                    foreach (var selection in selections)
                    {

                        var range = selection.GetAsOneRange();
                        var paragraph = range.OwnerParagraph;

                        var match = _regexPlain.Match(paragraph.Text);
                        while (match.Success)
                        {

                            paragraph.Text = paragraph.Text.Remove(match.Index, match.Length);

                            paragraph.Text = paragraph.Text.Insert(match.Index, variables[match.Groups[1].Value]);

                            match = _regexPlain.Match(paragraph.Text);
                            match = _regexPlain.Match(paragraph.Text);

                        }
                    }
                }
                catch
                {
                }

            var selections1 = document1.FindAllPattern(_regexComplex);
            if (selections != null)
                try
                {
                    foreach (var selection in selections1)
                    {
                        var range = selection.GetAsOneRange();
                        var paragraph = range.OwnerParagraph;
                        var match = _regexComplex.Match(paragraph.Text);

                        while (match.Success)
                        {
                            paragraph.Text = paragraph.Text.Remove(match.Index, match.Length);

                            int contragentId = variables2[match.Groups[1].Value];
                            string property = match.Groups[2].Value;
                            var propertiesToConvert = db.Contragent.Find(contragentId).Properties;
                            var propertiesJSON = JsonConvert.DeserializeObject<Dictionary<string, string>>(propertiesToConvert);

                            paragraph.Text = paragraph.Text.Insert(match.Index, propertiesJSON[property]); 
                            match = _regexComplex.Match(paragraph.Text);
                        }
                    }
                }
                catch
                {
                }

            document1.SaveToStream(stream, FileFormat.Docx);
            Response.Headers.Add("Content-type", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            Response.Headers.Add("Content-Disposition", "attachment;filename='" + "ll" + ".docx");
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(Response.OutputStream);
            Response.End();

            return null;
        }

        // GET: Docs/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doc doc = db.Doc.Find(id);
            if (doc == null)
            {
                return HttpNotFound();
            }
            ViewBag.contragentList = db.Contragent.Select(c => new StructforId { Id = c.Id, Name = c.Name });

            ViewBag.DownloadedDoc = new SelectList(db.Doc, "FilePath", "Name");
            ViewBag.contragents = new SelectList(db.Contragent, "Id", "Name");

            var deserializedJson = JsonConvert.DeserializeObject<JsonPartsLists>(doc.PlaceHolders);

            ViewBag.placeholders = deserializedJson.plainFields;
            ViewBag.contragentsHolder = deserializedJson.complexFields;
            return View(doc);
        }

        // GET: Docs/Create
        public ActionResult Create(HttpPostedFileBase file)
        {
            return View();
        }

        // POST: Docs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Doc doc, HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);

                    HashSet<string> placeholders = new HashSet<string>();
                    string pathSource = _path;
                    Document document = new Document();
                    document.LoadFromFile(pathSource);
                    var selections = document.FindAllPattern(_regexPlain);
                    if (selections != null)
                        try
                        {
                            foreach (var selection in selections)
                            {

                                var range = selection.GetAsOneRange();
                                var paragraph = range.OwnerParagraph;

                                var match = _regexPlain.Match(paragraph.Text);
                                while (match.Success)
                                {
                                    placeholders.Add(match.Groups[1].Value.ToString());

                                    paragraph.Text = paragraph.Text.Remove(match.Index, match.Length);

                                    match = _regexPlain.Match(paragraph.Text);


                                }
                            }
                        }
                        catch { }

                    HashSet<string> contragentsHolder = new HashSet<string>();
                    HashSet<string> contragentsProperties = new HashSet<string>();

                    selections = document.FindAllPattern(_regexComplex);
                    if (selections != null)
                        try
                        {
                            foreach (var selection in selections)
                            {
                                var range = selection.GetAsOneRange();
                                var paragraph = range.OwnerParagraph;
                                var match = _regexComplex.Match(paragraph.Text);
                                 while (match.Success)
                                  {
                                    contragentsHolder.Add(match.Groups[1].Value.ToString());
                                    contragentsProperties.Add(match.Groups[2].Value.ToString());
                                    paragraph.Text = paragraph.Text.Remove(match.Index, match.Length);

                                    match = _regexComplex.Match(paragraph.Text);

                                  }
                            }
                        }
                        catch
                        {
                        }

                    JsonPartsLists jsonParts = new JsonPartsLists();
                    jsonParts.plainFields = placeholders.ToList();
                    jsonParts.complexFields = contragentsHolder.ToList();
                    jsonParts.propertiesFields = contragentsProperties.ToList();
                    var storedProperties = db.Property.Select(c => c.Name).ToList();

                    if (jsonParts.propertiesFields.SequenceEqual(storedProperties))

                    {
                        doc.FilePath = _path;
                        doc.Name = _FileName;
                        var jsonSerialiser = new JavaScriptSerializer();
                        doc.PlaceHolders = jsonSerialiser.Serialize(jsonParts);

                        db.Doc.Add(doc);

                        db.SaveChanges();
                        ViewBag.Message = "File Uploaded Successfully!!";
                    }
                    else
                    {
                        ViewBag.Message = "File can not be uploaded, the properties does not match!";
                    }
                }
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
            return View();
        }


        public ActionResult Generate(long? id)
        {
            ViewBag.contragentList = db.Contragent.Select(c => c.Name).ToList();
            ViewBag.DownloadedDoc = new SelectList(db.Doc, "FilePath", "Name");
            return View(db.Doc.ToList());


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generate(Doc doc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(doc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DownloadedDoc = new SelectList(db.Doc, "FilePath", "Name");
            ViewBag.contragentList = db.Contragent.Select(c => c.Name).ToList();
            ViewBag.contragentProps = db.Contragent.Select(c => c.Properties).ToList();

            return Generate(doc);
        }

        // GET: Docs/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doc doc = db.Doc.Find(id);
            if (doc == null)
            {
                return HttpNotFound();
            }
            return View(doc);
        }

        // POST: Docs/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Doc doc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(doc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(doc);
        }

        // GET: Docs/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Doc doc = db.Doc.Find(id);
            if (doc == null)
            {
                return HttpNotFound();
            }
            return View(doc);
        }

        // POST: Docs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Doc doc = db.Doc.Find(id);
            db.Doc.Remove(doc);
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
