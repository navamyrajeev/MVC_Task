using MBG.Models;
using MBG.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MBG.Controllers
{
    public class HomeController : Controller
    {
        MBGEntities1 _context = new MBGEntities1();

        public ActionResult Index()
        {
            bool isHighlighted = false;
            var listofData = _context.Employees.ToList();
            foreach (var data in listofData)
            {
                int count = _context.Employee_Details.Where(x => x.EmpId == data.EmpId).ToList().Count;
                if (count < 3 && isHighlighted == false)
                {
                    isHighlighted = true;
                }
                data.DocCount = count;
            }
            if (isHighlighted)
            {
                ViewBag.isHighlighted = "Add atleast 3 documents for the highlighted Employees";
            }
            
            return View(listofData);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Employee model)
        {
            try
            {
                _context.Employees.Add(model);
                _context.SaveChanges();
                ViewBag.Message = "Employee Inserted Successfully";
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var data = _context.Employees.Where(x => x.EmpId == id).FirstOrDefault();
            return View(data);
        }
        [HttpPost]
        public ActionResult Edit(Employee Model)
        {
            var data = _context.Employees.Where(x => x.EmpId == Model.EmpId).FirstOrDefault();
            if (data != null)
            {
                data.Name = Model.Name;
                data.Designation = Model.Designation;
                data.Email = Model.Email;
                _context.SaveChanges();
                ViewBag.Message = "Employee Details Updated Successfully";
            }

            return RedirectToAction("index");
        }


        public ActionResult Detail(int id)
        {
            var empVM = new EmployeeVM();
            empVM.EmpObj = _context.Employees.Where(x => x.EmpId == id).FirstOrDefault();
            empVM.EmpDetails = _context.Employee_Details.Where(x => x.EmpId == id).ToList();
            return View(empVM);
        }


        public ActionResult Delete(int id)
        {
            try
            {
                var data = _context.Employees.Where(x => x.EmpId == id).FirstOrDefault();
                var empList = _context.Employee_Details.Where(x => x.EmpId == id).ToList();
                foreach (var doc in empList)
                {
                    _context.Employee_Details.Remove(doc);
                    if (System.IO.File.Exists(doc.FilePath))
                    {
                        System.IO.File.Delete(doc.FilePath);
                    }
                }
                _context.SaveChanges();

                _context.Employees.Remove(data);
                _context.SaveChanges();
                ViewBag.Messsage = "Record Deleted Successfully";
                return RedirectToAction("index");
            }
            catch (Exception)
            {
                ViewBag.Messsage = "Error deleting the record";
                return RedirectToAction("index");
            }
        }

        public ActionResult DeleteFile(int DetailsId, int EmpId)
        {
            try
            {
                var data = _context.Employee_Details.Where(x => x.DetailsId == DetailsId).FirstOrDefault();
                _context.Employee_Details.Remove(data);
                _context.SaveChanges();
                if (System.IO.File.Exists(data.FilePath))
                {
                    System.IO.File.Delete(data.FilePath);
                }
                ViewBag.Messsage = "Record Deleted Successfully";
                return RedirectToAction("Detail", new { id = EmpId });
            }
            catch
            {
                TempData["Error"] = "Error in deletion";
                return RedirectToAction("Detail", new { id = EmpId });
            }
        }

        public ActionResult ViewFile(int DetailsId, int EmpId)
        {
            try
            {
                var data = _context.Employee_Details.Where(x => x.DetailsId == DetailsId).FirstOrDefault();
                string ReportURL = data.FilePath;
                string extension = Path.GetExtension(ReportURL).Replace(".", "");
                string filename = Path.GetFileNameWithoutExtension(ReportURL) + "_decrypted." + extension;
                string decryptedFilePath = Server.MapPath("~/UploadedFiles/" + filename);
                if (System.IO.File.Exists(data.FilePath))
                {
                    Decrypt(data.FilePath, decryptedFilePath);

                    byte[] FileBytes = System.IO.File.ReadAllBytes(decryptedFilePath);

                    System.IO.File.Delete(decryptedFilePath);

                    if (extension == "jpg" || extension == "jpeg" || extension == "gif" || extension == "png")
                    {
                        return File(FileBytes, "image/" + extension);
                    }
                    else if (extension == "pdf")
                    {
                        return File(FileBytes, "application/pdf");
                    }
                    else
                    {
                        return File(FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, data.FileName);
                    }
                }
                else
                {
                    TempData["Error"] = "File not found";
                    return RedirectToAction("Detail", new { id = EmpId });
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading the file";
                return RedirectToAction("Detail", new { id = EmpId });
            }


        }

        public ActionResult DownloadFile(int DetailsId, int EmpId)
        {
            try
            {
                var data = _context.Employee_Details.Where(x => x.DetailsId == DetailsId).FirstOrDefault();
                string ReportURL = data.FilePath;
                string extension = Path.GetExtension(ReportURL).Replace(".", "");
                string filename = Path.GetFileNameWithoutExtension(ReportURL) + "_decrypted." + extension;
                string decryptedFilePath = Server.MapPath("~/UploadedFiles/" + filename);
                if (System.IO.File.Exists(data.FilePath))
                {
                    Decrypt(data.FilePath, decryptedFilePath);

                    byte[] FileBytes = System.IO.File.ReadAllBytes(decryptedFilePath);

                    System.IO.File.Delete(decryptedFilePath);

                    return File(FileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, data.FileName);
                }
                else
                {
                    TempData["Error"] = "File not found";
                    return RedirectToAction("Detail", new { id = EmpId });
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error dowmloading the file";
                return RedirectToAction("Detail", new { id = EmpId });
            }

        }

        [HttpPost]
        public ActionResult FileUpload(FormCollection fc, HttpPostedFileBase File)
        {
            string EMPID = fc["EMPID"];
            try
            {
                if (ModelState.IsValid)
                {
                    if (File == null)
                    {
                        TempData["Message"] = "Please Select file to upload";
                    }
                    else if (File.ContentLength > 0)
                    {

                        int MaxContentLength = 1024 * 1024 * 3; //3 MB
                        string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xlsx" };

                        if (!AllowedFileExtensions.Contains(File.FileName.Substring(File.FileName.LastIndexOf('.'))))
                        {
                            TempData["Message"] = "Please select file of type: " + string.Join(", ", AllowedFileExtensions);

                        }

                        else if (File.ContentLength > MaxContentLength)
                        {
                            TempData["Message"] = "Your file is too large, maximum allowed size is: 3 MB";
                        }
                        else
                        {

                            var fileName = Path.GetFileName(File.FileName);
                            var directoryPath = Path.Combine(Server.MapPath("~/UploadedFiles"));
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            var path = Path.Combine(directoryPath, fileName);

                            string extension = Path.GetExtension(path);
                            string filename = Path.GetFileNameWithoutExtension(path) + "_original" + extension;
                            string originalFilePath = Server.MapPath("~/UploadedFiles/" + filename);

                            File.SaveAs(originalFilePath);

                            Encrypt(originalFilePath, path);

                            System.IO.File.Delete(originalFilePath);

                            ModelState.Clear();
                            ViewBag.Message = "File uploaded successfully";
                            Employee_Details employee_Details = new Employee_Details
                            {
                                FileName = File.FileName,
                                EmpId = Convert.ToInt32(EMPID),
                                FilePath = path,
                                CreatedDate = DateTime.Now
                            };
                            _context.Employee_Details.Add(employee_Details);
                            _context.SaveChanges();
                        }


                    }
                }
                return RedirectToAction("Detail", new { id = EMPID });
            }
            catch (Exception)
            {
                TempData["Error"] = "Error while uploading document";
                return RedirectToAction("Detail", new { id = EMPID });
            }
        }

        private void Encrypt(string inputFilePath, string outputfilePath)
        {
            try
            {
                string EncryptionKey = "MAKV2SPBNI99212";
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (FileStream fsOutput = new FileStream(outputfilePath, FileMode.Create))
                    {
                        using (CryptoStream cs = new CryptoStream(fsOutput, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                            {
                                int data;
                                while ((data = fsInput.ReadByte()) != -1)
                                {
                                    cs.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Decrypt(string inputFilePath, string outputfilePath)
        {
            try
            {
                string EncryptionKey = "MAKV2SPBNI99212";
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                    {
                        using (CryptoStream cs = new CryptoStream(fsInput, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (FileStream fsOutput = new FileStream(outputfilePath, FileMode.Create))
                            {
                                int data;
                                while ((data = cs.ReadByte()) != -1)
                                {
                                    fsOutput.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }


    }
}