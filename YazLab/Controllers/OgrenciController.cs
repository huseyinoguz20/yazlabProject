using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using YazLab.Identity;
using YazLab.Models;
using YazLab.Pdf;
using static iTextSharp.text.pdf.AcroFields;
using System.IO;
using System.Collections.ObjectModel;

namespace YazLab.Controllers
{
    [Authorize(Roles = "ogrenci")]
    public class OgrenciController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public OgrenciController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityContext()));
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new IdentityContext()));
        }


        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult StajBasvuru()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StajBasvuru(BasvuruModel.Staj model)
        {
            if (ModelState.IsValid)
            {
                TimeSpan gunFarki = model.StajBitisTarihi - model.StajBaslangicTarihi;
                double gun = gunFarki.TotalDays;
                if (model.IsGunu == 30 && gun==42)
                {
                    DataContext db = new DataContext();
                    ApplicationUser kullanici = userManager.FindByName(User.Identity.Name);

                    var basvuru = new BasvuruModel.Staj();
                    basvuru.Ad = model.Ad;
                    basvuru.Soyad = model.Soyad;
                    basvuru.TC = model.TC;
                    basvuru.TelefonNumarasi = model.TelefonNumarasi;
                    basvuru.Adres = model.Adres;
                    if (model.StajTuru == "1")
                    {
                        var kullaniciStajBilgi = db.Stajs.Where(x => x.User_Id == kullanici.Id && x.StajTuru == "Staj1").ToList();

                        if (kullaniciStajBilgi.Count() != 0)
                        {
                            foreach (var item in kullaniciStajBilgi)
                            {
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Red == true)
                                {
                                    basvuru.StajTuru = "Staj1";
                                    basvuru.Staj1Durum = true;
                                    basvuru.Staj1Red = false;
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == false && item.Staj1Red == false)
                                {
                                    ModelState.AddModelError("", "Onay bekleyen staj 1 başvurunuz bulunmaktadır ");
                                    return View(model);
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Red == false && item.Staj1Not == 0)
                                {
                                    ModelState.AddModelError("", "Staj 1 tamamlanmıştır not girişi bekleniyor ");
                                    return View(model);
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Red == false && item.Staj1Not != 0)
                                {
                                    ModelState.AddModelError("", "Staj 1 tamamlanmıştır tekrar başvuruda bulunamazsınız ");
                                    return View(model);
                                }

                            }
                        }
                        else
                        {
                            basvuru.StajTuru = "Staj1";
                            basvuru.Staj1Durum = true;
                            basvuru.Staj1Red = false;
                        }
                    }
                    if (model.StajTuru == "2")
                    {
                        var kullaniciStajBilgi = db.Stajs.Where(x => x.User_Id == kullanici.Id && x.StajTuru == "Staj2").ToList();
                        if (kullaniciStajBilgi.Count() != 0)
                        {
                            foreach (var item in kullaniciStajBilgi)
                            {
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Not != 0 && item.Staj2Durum == true && item.Staj2OnayDurum == true && item.Staj2Not == 0 && item.Staj1Red == false && item.Staj2Red == true)
                                {
                                    basvuru.StajTuru = "Staj2";
                                    basvuru.Staj2Durum = true;
                                    basvuru.Staj1Durum = true;
                                    basvuru.Staj1OnayDurum = true;
                                    basvuru.Staj1Not = item.Staj1Not;
                                    basvuru.Staj1RaporPdf = item.Staj1RaporPdf;
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Not != 0 && item.Staj2Durum == true && item.Staj2OnayDurum == true && item.Staj2Not == 0 && item.Staj1Red == false && item.Staj2Red == false)
                                {
                                    ModelState.AddModelError("", "Staj 2 tamamlanmıştır not girişi bekleniyor ");
                                    return View(model);
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Not != 0 && item.Staj1Red == false && item.Staj2Durum == true && item.Staj2Not == 0 && item.Staj2Red == false)
                                {
                                    ModelState.AddModelError("", "Onay bekleyen staj 2 başvurunuz bulunmaktadır.");
                                    return View(model);
                                }
                                if (item.Staj1Durum == true && item.Staj1OnayDurum == true && item.Staj1Not != 0 && item.Staj2Durum == true && item.Staj2OnayDurum == true && item.Staj2Not != 0 && item.Staj1Red == false && item.Staj2Red == false)
                                {
                                    ModelState.AddModelError("", "Staj 2 tamamlanmıştır tekrar başvuruda bulunamazsınız ");
                                    return View(model);
                                }

                            }
                        }
                        else
                        {
                            var kullaniciStaj1Bilgi = db.Stajs.Where(x => x.User_Id == kullanici.Id && x.Staj1Durum == true && x.Staj1OnayDurum == true && x.Staj1Not != 0 && x.StajTuru == "Staj1").ToList();
                            if (kullaniciStaj1Bilgi.Count() == 0)
                            {
                                ModelState.AddModelError("", "Önce staj 1 tamamlamalısınız");
                                return View(model);
                            }
                            else
                            {
                                foreach (var item in kullaniciStaj1Bilgi)
                                {
                                    if (kullaniciStaj1Bilgi.Count() != 0)
                                    {
                                        basvuru.Staj1RaporPdf = item.Staj1RaporPdf;
                                        basvuru.StajTuru = "Staj2";
                                        basvuru.Staj2Durum = true;
                                        basvuru.Staj1Durum = true;
                                        basvuru.Staj1OnayDurum = true;
                                        basvuru.Staj1Not = item.Staj1Not;
                                    }
                                }
                            }
                        }
                    }
                    basvuru.StajBaslangicTarihi = model.StajBaslangicTarihi;
                    basvuru.StajBitisTarihi = model.StajBitisTarihi;
                    basvuru.IsGunu = model.IsGunu;
                    basvuru.GenelSaglikSigortasi = model.GenelSaglikSigortasi;
                    basvuru.YasDoldurma = model.YasDoldurma;
                    basvuru.FirmaAd = model.FirmaAd;
                    basvuru.FirmaFaaliyetAlani = model.FirmaFaaliyetAlani;
                    basvuru.FirmaAdres = model.FirmaAdres;
                    basvuru.FirmaTelefon = model.FirmaTelefon;
                    basvuru.User_Id = kullanici.Id; //staj1 modelinde oluşturduğum user_ıd foreign key
                    db.Stajs.Add(basvuru);
                    db.SaveChanges();
                    TempData["Success"] = "Başvuru Başarılı";
                    return RedirectToAction("StajBasvurularim", "Ogrenci");
                }
                else
                {
                    ModelState.AddModelError("", "Başvuru tamamlanamadı");
                }


            }
            return View(model);
        }

        [HttpGet]
        public ActionResult StajBasvurularim()
        {
            DataContext db = new DataContext();
            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);


            return View(db.Stajs.Where(x => x.User_Id == kullaniciBilgi.Id).ToList());
        }

        public List<BasvuruModel.Staj> GetKullanicilar(int id)
        {
            DataContext db = new DataContext();
            List<BasvuruModel.Staj> stajListe = new List<BasvuruModel.Staj>();
            BasvuruModel.Staj stajBilgi = new BasvuruModel.Staj();

            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);
            var kullanici = db.Stajs.Single(x => x.Id == id);

            for (int i = 0; i < 1; i++)
            {
                stajBilgi.Ad = kullanici.Ad;
                stajBilgi.Soyad = kullanici.Soyad;
                stajBilgi.TC = kullanici.TC;
                stajBilgi.TelefonNumarasi = kullanici.TelefonNumarasi;
                stajBilgi.Adres = kullanici.Adres;
                if (kullanici.StajTuru == "Staj1")
                {
                    stajBilgi.StajTuru = "Staj 1";
                }
                else
                {
                    stajBilgi.StajTuru = "Staj 2";
                }
                stajBilgi.StajBaslangicTarihi = kullanici.StajBaslangicTarihi;
                stajBilgi.StajBitisTarihi = kullanici.StajBitisTarihi;
                stajBilgi.IsGunu = kullanici.IsGunu;
                stajBilgi.GenelSaglikSigortasi = kullanici.GenelSaglikSigortasi;
                stajBilgi.YasDoldurma = kullanici.YasDoldurma;
                stajBilgi.FirmaAd = kullanici.FirmaAd;
                stajBilgi.FirmaFaaliyetAlani = kullanici.FirmaFaaliyetAlani;
                stajBilgi.FirmaAdres = kullanici.FirmaAdres;
                stajBilgi.FirmaTelefon = kullanici.FirmaTelefon;
                stajListe.Add(stajBilgi);
            }

            return stajListe;
        }



        [AllowAnonymous]
        [HttpGet]
        public ActionResult StajBasvurumPDF(int id)
        {
            PdfAyarlari pdfAyarlari = new PdfAyarlari();//employeeReport
            byte[] abytes = pdfAyarlari.ReportPdf(GetKullanicilar(id));
            return File(abytes, "application/Pdf");
        }





        [HttpGet]
        public ActionResult ImeBasvuru()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ImeBasvuru(BasvuruModel.Ime model)
        {
            if (ModelState.IsValid)
            {
                TimeSpan gunFarki = model.ImeBitisTarihi - model.ImeBaslangicTarihi;
                double gun = gunFarki.TotalDays;
                if (model.IsGunu == 70)
                {
                    DataContext db = new DataContext();
                    ApplicationUser kullanici = userManager.FindByName(User.Identity.Name);

                    var kullaniciStajBilgi = db.Stajs.Where(x => x.User_Id == kullanici.Id && x.Staj1Durum == true && x.Staj1OnayDurum == true && x.Staj2Durum == true && x.Staj2OnayDurum == true && x.Staj1Not != 0 && x.Staj2Not != 0 && x.Staj1Red == false && x.Staj2Red == false).ToList();
                    if (kullaniciStajBilgi.Count() != 0)
                    {
                        var basvuru = new BasvuruModel.Ime();
                        basvuru.Ad = model.Ad;
                        basvuru.Soyad = model.Soyad;
                        basvuru.TC = model.TC;
                        basvuru.TelefonNumarasi = model.TelefonNumarasi;
                        basvuru.Adres = model.Adres;
                        if (model.ImeDonem == "1")
                        {
                            var kullaniciIMEbilgi = db.Imes.Where(x => x.User_Id == kullanici.Id).ToList();
                            if (kullaniciIMEbilgi.Count() != 0)
                            {
                                foreach (var item in kullaniciIMEbilgi)
                                {
                                    if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot == 0 && item.ImeRed == true)
                                    {
                                        basvuru.ImeDonem = "Güz";
                                        basvuru.ImeDurum = true;
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == false && item.ImeNot == 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "Onay bekleyen İME başvurunuz bulunmaktadır.");
                                        return View(model);
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot == 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "İme başvurunuz onaylanmıştır not girişi bekleniyor.");
                                        return View(model);
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot != 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "İme başvurunuz tamamlanmıştır tekrar başvuruda bulunamazsınız");
                                        return View(model);
                                    }


                                }
                            }
                            else
                            {
                                basvuru.ImeDonem = "Güz";
                                basvuru.ImeDurum = true;
                            }
                        }
                        if (model.ImeDonem == "2")
                        {
                            var kullaniciIMEbilgi = db.Imes.Where(x => x.User_Id == kullanici.Id).ToList();
                            if (kullaniciIMEbilgi.Count() != 0)
                            {
                                foreach (var item in kullaniciIMEbilgi)
                                {
                                    if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot == 0 && item.ImeRed == true)
                                    {
                                        basvuru.ImeDonem = "Bahar";
                                        basvuru.ImeDurum = true;
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == false && item.ImeNot == 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "Onay bekleyen İME başvurunuz bulunmaktadır.");
                                        return View(model);
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot == 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "İme başvurunuz onaylanmıştır not girişi bekleniyor.");
                                        return View(model);
                                    }
                                    else if (item.ImeDurum == true && item.ImeOnayDurum == true && item.ImeNot != 0 && item.ImeRed == false)
                                    {
                                        ModelState.AddModelError("", "İme başvurunuz tamamlanmıştır tekrar başvuruda bulunamazsınız");
                                        return View(model);
                                    }

                                }
                            }
                            else
                            {
                                basvuru.ImeDonem = "Bahar";
                                basvuru.ImeDurum = true;
                            }
                        }
                        basvuru.ImeBaslangicTarihi = model.ImeBaslangicTarihi;
                        basvuru.ImeBitisTarihi = model.ImeBitisTarihi;
                        basvuru.IsGunu = model.IsGunu;
                        basvuru.GenelSaglikSigortasi = model.GenelSaglikSigortasi;
                        basvuru.YasDoldurma = model.YasDoldurma;
                        basvuru.FirmaAd = model.FirmaAd;
                        basvuru.FirmaFaaliyetAlani = model.FirmaFaaliyetAlani;
                        basvuru.FirmaAdres = model.FirmaAdres;
                        basvuru.FirmaTelefon = model.FirmaTelefon;
                        basvuru.User_Id = kullanici.Id; //staj1 modelinde oluşturduğum user_ıd foreign key
                        db.Imes.Add(basvuru);
                        db.SaveChanges();
                        TempData["Success"] = "Başvuru Başarılı";
                        return RedirectToAction("Index", "Ogrenci");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Stajlarınız tamamlanmadan İME başvurusu yapamazsınız");
                        return View(model);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Çalışma aralığını doğru giriniz");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ImeBasvurularim()
        {
            DataContext db = new DataContext();
            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);


            return View(db.Imes.Where(x => x.User_Id == kullaniciBilgi.Id).ToList());
        }

        public List<BasvuruModel.Ime> GetKullanicilarIME(int id)
        {
            DataContext db = new DataContext();
            List<BasvuruModel.Ime> imeListe = new List<BasvuruModel.Ime>();
            BasvuruModel.Ime imeBilgi = new BasvuruModel.Ime();

            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);
            var kullanici = db.Imes.Single(x => x.Id == id);

            for (int i = 0; i < 1; i++)
            {
                imeBilgi.Ad = kullanici.Ad;
                imeBilgi.Soyad = kullanici.Soyad;
                imeBilgi.TC = kullanici.TC;
                imeBilgi.TelefonNumarasi = kullanici.TelefonNumarasi;
                imeBilgi.Adres = kullanici.Adres;
                if (kullanici.ImeDonem == "Güz")
                {
                    imeBilgi.ImeDonem = "Güz";
                }
                else
                {
                    imeBilgi.ImeDonem = "Bahar";
                }
                imeBilgi.ImeBaslangicTarihi = kullanici.ImeBaslangicTarihi;
                imeBilgi.ImeBitisTarihi = kullanici.ImeBitisTarihi;
                imeBilgi.IsGunu = kullanici.IsGunu;
                imeBilgi.GenelSaglikSigortasi = kullanici.GenelSaglikSigortasi;
                imeBilgi.YasDoldurma = kullanici.YasDoldurma;
                imeBilgi.FirmaAd = kullanici.FirmaAd;
                imeBilgi.FirmaFaaliyetAlani = kullanici.FirmaFaaliyetAlani;
                imeBilgi.FirmaAdres = kullanici.FirmaAdres;
                imeBilgi.FirmaTelefon = kullanici.FirmaTelefon;
                imeListe.Add(imeBilgi);
            }

            return imeListe;
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult ImeBasvurumPDF(int id)
        {
            PdfAyarlariIme pdfAyarlariIme = new PdfAyarlariIme();//employeeReport
            byte[] abytes = pdfAyarlariIme.ReportPdf(GetKullanicilarIME(id));
            return File(abytes, "application/Pdf");
        }


        [HttpGet]
        public ActionResult StajNotlarim()
        {
            DataContext db = new DataContext();
            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);

            //staj notu 0 ise notlandırılma bekleniyor yaz!!!!!
            return View(db.Stajs.Where(x => x.User_Id == kullaniciBilgi.Id).ToList());
        }

        [HttpGet]
        public ActionResult ImeNotum()
        {
            DataContext db = new DataContext();
            var kullaniciBilgi = userManager.FindByName(User.Identity.Name);


            return View(db.Imes.Where(x => x.User_Id == kullaniciBilgi.Id && x.ImeNot != 0).ToList());
        }




        DataContext db = new DataContext();
        [HttpGet]
        public ActionResult RaporYukleme()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RaporYukleme(string secim, string OgrenciPdf)
        {
            if (Request.Files.Count > 0)
            {
                string id = User.Identity.GetUserId();
                if (secim == "staj1")
                {
                    var kullanici = db.Stajs.Where(x => x.User_Id == id && x.StajTuru == "Staj1" && x.Staj1Durum == true && x.Staj1OnayDurum == true && x.Staj1Red == false && x.Staj1Not == 0 && x.Staj1RaporPdf == null).ToList();
                    if (kullanici.Count() != 0)
                    {
                        foreach (var item in kullanici)
                        {
                            string dosyaAdi = Path.GetFileName(Request.Files[0].FileName);
                            string uzanti = Path.GetExtension(Request.Files[0].FileName);
                            string yol = "~/PdfDepo/" + dosyaAdi;
                            Request.Files[0].SaveAs(Server.MapPath(yol));
                            item.Staj1RaporPdf = "/PdfDepo/" + dosyaAdi;
                            db.SaveChanges();
                            TempData["basarili"] = "Staj 1 rapor yüklemesi başarılıdır";
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Staj 1 rapor yüklemesi için gerekli koşulları sağlamıyorsunuz!!");
                        return View();
                    }

                }
                else if (secim == "staj2")
                {
                    var kullanici = db.Stajs.Where(x => x.User_Id == id && x.StajTuru == "Staj2" && x.Staj1Durum == true && x.Staj1OnayDurum == true && x.Staj1Red == false && x.Staj1Not != 0 && x.Staj1RaporPdf != null && x.Staj2OnayDurum == true && x.Staj2Durum == true && x.Staj2Red == false && x.Staj2RaporPdf == null).ToList();
                    if (kullanici.Count() != 0)
                    {
                        foreach (var item in kullanici)
                        {
                            string dosyaAdi = Path.GetFileName(Request.Files[0].FileName);
                            string uzanti = Path.GetExtension(Request.Files[0].FileName);
                            string yol = "~/PdfDepo/" + dosyaAdi;
                            Request.Files[0].SaveAs(Server.MapPath(yol));
                            item.Staj2RaporPdf = "/PdfDepo/" + dosyaAdi;
                            db.SaveChanges();
                            TempData["basarili"] = "Staj 2 rapor yüklemesi başarılıdır";
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Staj2 rapor yüklemesi için gerekli koşulları sağlamıyorsunuz!!");
                        return View();
                    }


                }
                else if (secim == "ime")
                {
                    var kullanici = db.Imes.Where(x => x.User_Id == id && x.ImeOnayDurum == true && x.ImeDurum == true && x.RaporPdf == null).ToList();
                    if (kullanici.Count() != 0)
                    {
                        foreach (var item in kullanici)
                        {
                            string dosyaAdi = Path.GetFileName(Request.Files[0].FileName);
                            string uzanti = Path.GetExtension(Request.Files[0].FileName);
                            string yol = "~/PdfDepo/" + dosyaAdi;
                            Request.Files[0].SaveAs(Server.MapPath(yol));
                            item.RaporPdf = "/PdfDepo/" + dosyaAdi;
                            db.SaveChanges();
                            TempData["basarili"] = "İme rapor yüklemesi başarılıdır";
                            return View();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Ime rapor yüklemesi için gerekli koşulları sağlamıyorsunuz!!");
                        return View();
                    }
                }
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Dosya yüklemesi yapmalısınız");
                return View();
            }





        }
    }
}

