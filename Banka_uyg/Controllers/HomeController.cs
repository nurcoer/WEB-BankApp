﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Banka_uyg.Models;


namespace Banka_uyg.Controllers
{
    public class HomeController : Controller
    {
        BankaEntities2 db = new BankaEntities2();
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
           
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Login()
        {
           
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
           
                Musteriler mus = new Musteriler();
                mus.TC = form["tc"];
                
                mus.Sifre = form["sifre"];
                var musteri = db.Musteriler.Where(q => q.TC == mus.TC && q.Sifre == mus.Sifre).FirstOrDefault();
                
                if (musteri != null)
                {

                Session["UserAdSoyad"] = musteri.Ad + musteri.Soyad;
                Session["UserTc"] = musteri.TC;

                return RedirectToAction("About");

                }


                else
                {
                    ViewBag.giris = "Hatalı Tc Kimlik ve şifre girdiniz.";
                    return View();
                }


        }

        public ActionResult KayitOl()
        {

            return View();
        }

        [HttpPost]
        public ActionResult KayitOl(FormCollection frm)
        {
            Musteriler musteri = new Musteriler();
            musteri.TC = frm["tc"];
            ViewBag.mesaj = "Hatalı TC girilmiştir. ";
            String Tc = frm["tc"];
            


            if ((!(db.Musteriler.Where(m => m.TC == musteri.TC).Any()))&&(Tc.Length==11))
            {
                musteri.Ad = frm["ad"];
                musteri.Soyad = frm["soyad"];
                musteri.Cinsiyet = frm["cinsiyet"];
                musteri.DoğumTarihi = frm["tarih"];

                musteri.Telefon = frm["telefon"];
                musteri.EMail = frm["email"];
                musteri.Adres = frm["adres"];
                musteri.Sifre = frm["sifre"];

                db.Musteriler.Add(musteri);
                db.SaveChanges();
                return RedirectToAction("Login", "Home");
            }
            else
           
            return View(); 

            
        }

        public ActionResult HesapEkle()
        {
            int sayac = 1001;
            string tc = Session["UserTc"].ToString();
            List<Hesaplar> Hesaplar = db.Hesaplar.Where(x => x.MusteriTc == tc).ToList();

            foreach (var item in Hesaplar)
            {
                sayac++;
            }
            if (Hesaplar.Count == 0)
            {
                Random r = new Random();

                ViewBag.HesapNo = r.Next(10000, 90000).ToString() + "-" + sayac;
                Session["HesapNo"] = r.Next(10000, 90000).ToString();

            }

            else
            {


                var hesap = db.Hesaplar.Where(x => x.MusteriTc == tc).FirstOrDefault();
                Session["HesapNoTamami"] = hesap.HesapNo + "-" + sayac;
                //ViewBag.HesapNo = hesap.HesapNo + "-" + sayac;
                Session["HesapNo"] = hesap.HesapNo;

            }

            return View(db.Hesaplar.Where(x => x.HesapDurum == true).ToList());
        }

        [HttpPost]
        public ActionResult HesapEkle(FormCollection frm)
        {
            Hesaplar hesap = new Hesaplar();
            int sayac = 1001;
            string tc = Session["UserTc"].ToString();
            List<Hesaplar> Hesaplar = db.Hesaplar.Where(x => x.MusteriTc == tc).ToList();
            if (Hesaplar.Count == 0)
            {

                String hesapNo = Session["HesapNo"].ToString();
                hesap.HesapNo = hesapNo;
                hesap.EkNumara = sayac;

            }
            else
            {
                foreach (var item in Hesaplar)
                {
                    sayac++;
                    String hesapNo = Session["HesapNo"].ToString();
                    hesap.HesapNo = hesapNo;
                    hesap.EkNumara = sayac;

                }

            }




            hesap.MusteriTc = Session["UserTc"].ToString();
            hesap.Bakiye = Convert.ToInt32(frm["bakiye"]);
            hesap.HesapDurum = true;
            db.Hesaplar.Add(hesap);
            db.SaveChanges();
            var hesap2 = db.Hesaplar.Where(x => x.MusteriTc == tc).FirstOrDefault();
            Session["HesapNoTamami"] = hesap2.HesapNo + "-" + (sayac + 1);
            frm.Clear();
            return View(db.Hesaplar.Where(x => x.HesapDurum == true).ToList());
        }
        public ActionResult HesapKapat(int? id)
        {
            string Tc = Session["UserTc"].ToString();
            Hesaplar hesap = db.Hesaplar.Where(x => x.MusteriTc == Tc && x.EkNumara == id).FirstOrDefault();
            if (hesap.Bakiye > 0)
            {
                ViewData["HesapKapat"] = "Bakiyeniz 0 olmadığı için hesabınızı kapatamazsınız.";
            }
            else
            {
                hesap.HesapDurum = false;
                db.SaveChanges();
            }


            return RedirectToAction("HesapEkle");
        }


        public ActionResult LogOut()
        {
            Session["UserAdSoyad"] = null;
            Session["UserTc"] = null;
            Session["HesapNo"] = null;
            Session["HesapNoTamami"] = null;
            Session.Abandon();

            return RedirectToAction("Login", "Home");
        }



        public ActionResult BakiyeEkle(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string tc = Session["UserTc"].ToString();
            Hesaplar hesap = db.Hesaplar.Where(x => x.MusteriTc == tc && x.EkNumara == id).FirstOrDefault();
            if (hesap == null)
            {
                return HttpNotFound();
            }
            ViewBag.Hesap = hesap.HesapNo + "-" + id;
            Session["Hesap"] = ViewBag.Hesap;
            Session["eknumara"] = id;
            return View();

        }

        [HttpPost]
        public ActionResult BakiyeEkle(FormCollection frm)
        {
            string tc = Session["UserTc"].ToString();
            int ek = Convert.ToInt32(Session["eknumara"]);
            Hesaplar hesap = db.Hesaplar.Where(x => x.MusteriTc == tc && x.EkNumara == ek).FirstOrDefault();
            int bakiye = Convert.ToInt32(frm["bakiye"]);
            hesap.Bakiye = hesap.Bakiye + bakiye;
            db.SaveChanges();
            return RedirectToAction("HesapEkle");
        }


        public ActionResult BakiyeCikar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string tc = Session["UserTc"].ToString();
            Hesaplar hesap = db.Hesaplar.Where(x => x.MusteriTc == tc && x.EkNumara == id).FirstOrDefault();
            if (hesap == null)
            {
                return HttpNotFound();
            }
            ViewBag.Hesap = hesap.HesapNo + "-" + id;
            Session["Hesap"] = ViewBag.Hesap;
            Session["eknumara"] = id;
            return View();

        }
        [HttpPost]
        public ActionResult BakiyeCikar(FormCollection frm)
        {
            string tc = Session["UserTc"].ToString();
            int ek = Convert.ToInt32(Session["eknumara"]);
            Hesaplar hesap = db.Hesaplar.Where(x => x.MusteriTc == tc && x.EkNumara == ek).FirstOrDefault();
            int bakiye = Convert.ToInt32(frm["bakiye"]);
            hesap.Bakiye = hesap.Bakiye - bakiye;
            db.SaveChanges();
            return RedirectToAction("HesapEkle");
        }


    }
}