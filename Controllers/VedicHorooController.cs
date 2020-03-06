using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyyPub.Models;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TimeZoneConverter;
using Microsoft.Extensions.Logging;

namespace MyyPub.VedicHorro
{
    [Route("api")]
    [ApiController]
    public class VedicHorooController : ControllerBase
    {
        private IWebHostEnvironment _env;
		private readonly ILogger<VedicHorooController> _logger;

        public VedicHorooController(IWebHostEnvironment env, ILogger<VedicHorooController> logger)
        {
			_logger = logger;
            _env = env;
        }
        [HttpGet("CalcVim")]
        public IActionResult CalcVim(string dob, string lord, double mpos, double nsp, int msi, int nsi, string lang)
        {
            Dictionary<string, int> dashas = new Dictionary<string, int>();
            Dictionary<string, Dasha> dctVim = new Dictionary<string, Dasha>();
            const int d_yr = 360;
            const double m_dy = 29.5;
            try
            {
                dashas.Add("su", 6);
                dashas.Add("mo", 10);
                dashas.Add("ma", 7);
                dashas.Add("ra", 18);
                dashas.Add("ju", 16);
                dashas.Add("sa", 19);
                dashas.Add("me", 17);
                dashas.Add("ke", 7);
                dashas.Add("ve", 20);
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string astf = string.Empty, adct = string.Empty;
                switch (lang)
                {
                    case "ta":
                        astf = string.Format(@"{0}\ta-dct.json", astClient);
                        break;
                    case "en":
                        astf = string.Format(@"{0}\en-dct.json", astClient);
                        break;
                    case "te":
                        astf = string.Format(@"{0}\te-dct.json", astClient);
                        break;
                    case "hi":
                        astf = string.Format(@"{0}\hi-dct.json", astClient);
                        break;
                    default:
                        astf = string.Format(@"{0}\en-dct.json", astClient);
                        break;
                }
                using (StreamReader rdra = new StreamReader(astf, Encoding.UTF8))
                {
                    adct = rdra.ReadToEnd();
                }
                Dictionary<string, string> dctAst = null;
                try
                {
                    dctAst = JsonConvert.DeserializeObject<Dictionary<string, string>>(adct);
                }
                catch
                {
                }
                var ras_num = msi;
                var mon_crs = (ras_num - 1) * 30;
                var lb = mpos + mon_crs;
                var ras_num2 = nsi;
                var mon_crs2 = (ras_num2 - 1) * 30;
                var sp = nsp + mon_crs2;
                var b = (lb - sp) / 13.20;
                Console.WriteLine(string.Format("bal in degrees: {0}", b));
                var bal_das = b * dashas[lord.Substring(0, 2).ToLower()];
                Console.WriteLine(string.Format("bal_das: {0}", bal_das));
                var adp = bal_das;
                string adp_s = adp.ToString();
                int ady = Convert.ToInt32(adp_s.IndexOf('.') > -1 ? adp_s.Split('.')[0] : adp_s);
                double adm = adp_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adp_s.Split('.')[1]) * 12 : 0;
                string adm_s = adm.ToString();
                int adm1 = Convert.ToInt32(adm_s.IndexOf('.') > -1 ? adm_s.Split('.')[0] : adm_s);
                double adys = adm_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adm_s.Split('.')[1]) * 30 : 0;
                string adys_s = adys.ToString();
                adys = Convert.ToDouble(adys_s.IndexOf('.') > -1 ? adys_s.Split('.')[0] : adys_s);
                double adhs = adys_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adys_s.Split('.')[1]) * 24 : 0;
                string adhs_s = adhs.ToString();
                adhs = Convert.ToDouble(adhs_s.IndexOf('.') > -1 ? adhs_s.Split('.')[0] : adhs_s);
                double adhms = adhs_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adhs_s.Split('.')[1]) * 60 : 0;
                string adhms_s = adhms.ToString();
                adhms = Convert.ToDouble(adhms_s.IndexOf('.') > -1 ? adhms_s.Split('.')[0] : adhms_s);
                var elp_dys = ady * d_yr + adm1 * m_dy + adys;
                var rem_days = dashas[lord.Substring(0, 2).ToLower()] * d_yr - elp_dys;

                int year = Convert.ToInt32(dob.Split('T')[0].Split('-')[0]);
                int mon = Convert.ToInt32(dob.Split('T')[0].Split('-')[1]);
                int day = Convert.ToInt32(dob.Split('T')[0].Split('-')[2]);
                int hour = Convert.ToInt32(dob.Split('T')[1].Split(':')[0]);
                int min = Convert.ToInt32(dob.Split('T')[1].Split(':')[1]);
                //int sec = Convert.ToInt32(dob.Split('T')[1].Split('-')[2]);
                var odob = new DateTime(year, mon, day, hour, min, 0);
                var dob_c = new DateTime(year, mon, day, hour, min, 0);
                dob_c = dob_c.AddDays(rem_days);
                string pbeg = string.Format("{0}/{1}/{2}", year, mon, day);
                string pend = string.Format("{0}/{1}/{2}", dob_c.Day, dob_c.Month, dob_c.Year);
                var sty = "mdas";
                var cur_date = DateTime.Now;
                if (cur_date >= odob && cur_date <= dob_c) sty = "mdasc";
                var dsa = new Dasha
                {
                    lord = dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lord.ToLower())],
                    per = string.Format("{0} To {1}", pbeg, pend),
                    type = "MDAS",
                    style = sty,
                    subs = true,
                    show = true,
                    icon = "add"
                };
                dctVim.Add(lord, dsa);
                buildAntarDasha(lord, new DateTime(year, mon, day, hour, min, 0), rem_days, dashas, dctAst, ref dctVim);
                string[] arr = { "sun", "moon", "mars", "rahu", "jupiter", "saturn", "mercury", "ketu", "venus" };
                var v_start = 0;
                var v_iter = 0;
                for (var vi = 0; vi < 9; vi++)
                {
                    if (v_start > 0)
                    {
                        v_iter++;
                        var startdt = new DateTime(dob_c.Year, dob_c.Month, dob_c.Day, dob_c.Hour, dob_c.Month, dob_c.Second);
                        var m = (dob_c.Month).ToString();
                        var dd = dob_c.Day.ToString();
                        var y = dob_c.Year.ToString();
                        dob_c = dob_c.AddYears(Convert.ToInt32(dashas[arr[vi].Substring(0, 2)]));
                        pbeg = string.Format("{0}/{1}/{2}", startdt.Day.ToString(), (startdt.Month).ToString(), startdt.Year.ToString());
                        pend = string.Format("{0}/{1}/{2}", dob_c.Day.ToString(), (dob_c.Month).ToString(), dob_c.Year.ToString());
                        sty = "mdas";
                        if (cur_date >= startdt && cur_date <= dob_c) sty = "mdasc";
                        var dsa2 = new Dasha
                        {
                            lord = dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(arr[vi].ToLower())],
                            per = string.Format("{0} To {1}", pbeg, pend),
                            type = "MDAS",
                            style = sty,
                            subs = true,
                            show = true,
                            icon = "add"
                        };
                        dctVim.Add(arr[vi], dsa2);
                        buildAntarDasha(arr[vi], startdt, 0, dashas, dctAst, ref dctVim);
                    }
                    if (arr[vi] == lord.ToLower())
                    {
                        v_start = 1;
                    }
                    if (vi == 8) vi = -1;
                    if (v_iter == 8) break;
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);
                var dsa2 = new Dasha
                {
                    lord = string.Format("ERROR: {0} LINE {1}", eX.Message, line),
                    per = "",
                    type = "MDAS",
                    style = "",
                    subs = true,
                    show = true,
                    icon = ""
                };

                dctVim.Add("eX.Message", dsa2);
            }
			
            return new JsonResult(dctVim);
        }

        void buildAntarDasha(string lord, DateTime startdt, double remdays, Dictionary<string, int> dashas, Dictionary<string, string> dctAst, ref Dictionary<string, Dasha> dctVim)
        {
            //string akys = "";
            double m_dy = 29.530588;//this.days_in_month(startdt.getMonth()+1, startdt.getFullYear());//30.436875;//29.59421013;//29.530588;//30.436875;//29.530588;
            int d_yr = 360;////this.days_of_a_year(startdt.getFullYear());//365.2425;//366;//354.367056;//;////354.367056;//365.2425;
            double e_dys = 0.0;
            var a_per = 0;
            var cur_date = DateTime.Now;
            Dictionary<string, string> curdas = new Dictionary<string, string>();
            string dasp = string.Empty;
            string astClient = Path.Combine(_env.ContentRootPath, @"Content/astroclient");
            using (StreamReader rdr = new StreamReader(string.Format(@"{0}\{1}_das.json", astClient, lord), Encoding.UTF8))
            {
                dasp = rdr.ReadToEnd();
            }
            try
            {
                curdas = JsonConvert.DeserializeObject<Dictionary<string, string>>(dasp);
            }
            catch
            {
            }

            if (remdays > 0)
            {
                var s_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
                var rem_d = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                rem_d = rem_d.AddDays(remdays);
                var tot_dys = Convert.ToInt32(dashas[lord.Substring(0, 2).ToLower()]) * d_yr;
                e_dys = tot_dys - remdays;
                var ffd = 0.0;
                //var r_dys = 0.0;
                foreach (var das in curdas)
                {
                    var ads = das.Value;
                    double adp = (Convert.ToDouble(dashas[lord.Substring(0, 2).ToLower()]) / 120) * dashas[das.Key];
                    string adp_s = adp.ToString();
                    int ady = Convert.ToInt32(adp_s.IndexOf('.') > -1 ? adp_s.Split('.')[0] : adp_s);
                    double adm = adp_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adp_s.Split('.')[1]) * 12 : 0;
                    string adm_s = adm.ToString();
                    int adm1 = Convert.ToInt32(adm_s.IndexOf('.') > -1 ? adm_s.Split('.')[0] : adm_s);
                    double adys = adm_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adm_s.Split('.')[1]) * 30 : 0;
                    string adys_s = adys.ToString();
                    adys = Convert.ToDouble(adys_s.IndexOf('.') > -1 ? adys_s.Split('.')[0] : adys_s);
                    double adhs = adys_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adys_s.Split('.')[1]) * 24 : 0;
                    string adhs_s = adhs.ToString();
                    adhs = Convert.ToDouble(adhs_s.IndexOf('.') > -1 ? adhs_s.Split('.')[0] : adhs_s);
                    double adhms = adhs_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adhs_s.Split('.')[1]) * 60 : 0;
                    string adhms_s = adhms.ToString();
                    adhms = Convert.ToDouble(adhms_s.IndexOf('.') > -1 ? adhms_s.Split('.')[0] : adhms_s);
                    var a_dys = Convert.ToInt32(ads.Split('|')[0]) * d_yr + Convert.ToInt32(ads.Split('|')[1]) * m_dy + Convert.ToInt32(ads.Split('|')[2]);
                    ffd += a_dys;
                    if (ffd >= e_dys)
                    {
                        var start_das = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                        var m = (s_dt.Month).ToString();
                        var dd = s_dt.Day.ToString();
                        var y = s_dt.Year.ToString();
                        s_dt = s_dt.AddDays(a_dys);
                        string sty = "adas";
                        if (cur_date >= start_das && cur_date <= s_dt)
                        {
                            sty = "adasc";
                        }
                        a_per++;
                        string pbeg = string.Format("{0}/{1}/{2}", dd, m, y);
                        string pend = string.Format("{0}/{1}/{2}", s_dt.Day.ToString(), (s_dt.Month).ToString(), s_dt.Year.ToString());
                        var adas = new Dasha
                        {
                            lord = string.Format("{0}-{1}", dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lord.ToLower())], dctAst[das.Key]),
                            per = string.Format("{0} To {1}", pbeg, pend),
                            type = "ADAS",
                            style = sty,
                            subs = true,
                            show = false,
                            icon = "add"
                        };
                        dctVim.Add(string.Format("{0}-{1}", lord, das.Key), adas);
                        double vdays = buildPratyantarDasha(lord, das.Key, new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second), rem_d, ffd - a_dys, e_dys, dashas, dctAst, ref dctVim);
                        if (vdays >= remdays) break;
                    }
                }
            }
            else
            {
                var s_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
                foreach (var das in curdas)
                {
                    a_per++;
                    var start_das = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                    var m = (s_dt.Month).ToString();
                    var dd = s_dt.Day.ToString();
                    var y = s_dt.Year.ToString();
                    double adp = (Convert.ToDouble(dashas[lord.Substring(0, 2).ToLower()]) / 120) * dashas[das.Key];
                    string adp_s = adp.ToString();
                    int ady = Convert.ToInt32(adp_s.IndexOf('.') > -1 ? adp_s.Split('.')[0] : adp_s);
                    double adm = adp_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adp_s.Split('.')[1]) * 12 : 0;
                    string adm_s = adm.ToString();
                    int adm1 = Convert.ToInt32(adm_s.IndexOf('.') > -1 ? adm_s.Split('.')[0] : adm_s);
                    double adys = adm_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adm_s.Split('.')[1]) * 30 : 0;
                    string adys_s = adys.ToString();
                    adys = Convert.ToDouble(adys_s.IndexOf('.') > -1 ? adys_s.Split('.')[0] : adys_s);
                    double adhs = adys_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adys_s.Split('.')[1]) * 24 : 0;
                    string adhs_s = adhs.ToString();
                    adhs = Convert.ToDouble(adhs_s.IndexOf('.') > -1 ? adhs_s.Split('.')[0] : adhs_s);
                    double adhms = adhs_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adhs_s.Split('.')[1]) * 60 : 0;
                    string adhms_s = adhms.ToString();
                    adhms = Convert.ToDouble(adhms_s.IndexOf('.') > -1 ? adhms_s.Split('.')[0] : adhms_s);
                    //var tot = ady*d_yr + adm*m_dy + adys;
                    s_dt = s_dt.AddYears(ady);
                    s_dt = s_dt.AddMonths(adm1);
                    s_dt = s_dt.AddDays(adys);
                    s_dt = s_dt.AddHours(adhs);
                    s_dt = s_dt.AddMinutes(adhms);
                    string sty = "adas";
                    if (cur_date >= start_das && cur_date <= s_dt)
                    {
                        sty = "adasc";
                    }
                    string pbeg = string.Format("{0}/{1}/{2}", dd, m, y);
                    string pend = string.Format("{0}/{1}/{2}", s_dt.Day.ToString(), (s_dt.Month).ToString(), s_dt.Year.ToString());
                    var adas = new Dasha
                    {
                        lord = string.Format("{0}-{1}", dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lord.ToLower())], dctAst[das.Key]),
                        per = string.Format("{0} To {1}", pbeg, pend),
                        type = "ADAS",
                        style = sty,
                        subs = true,
                        show = false,
                        icon = "add"
                    };
                    dctVim.Add(string.Format("{0}-{1}", lord, das.Key), adas);
                    buildPratDas(lord, das.Key, new DateTime(start_das.Year, start_das.Month, start_das.Day, start_das.Hour, start_das.Minute, start_das.Second), s_dt, dashas, dctAst, ref dctVim);
                }
            }
        }
        static void buildPratDas(string mainlord, string sublord, DateTime startdt, DateTime enddt, Dictionary<string, int> dashas, Dictionary<string, string> dctAst, ref Dictionary<string, Dasha> dctVim)
        {
            string[] arr = { "su", "mo", "ma", "ra", "ju", "sa", "me", "ke", "ve" };
            var v_start = 0;
            var v_iter = 0;
            //var a_per = 0;
            var s_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
            var cur_date = DateTime.Now;
            for (var vi = 0; vi < 9; vi++)
            {
                if (arr[vi] == sublord || v_start == 1)
                {
                    v_iter++;
                    var b_dt = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                    //d_yr = this.days_of_a_year(b_dt.getFullYear());
                    var m = (s_dt.Month).ToString();
                    var dd = s_dt.Day.ToString();
                    var y = s_dt.Year.ToString();
                    if (v_iter == 9) s_dt = enddt;
                    else
                    {
                        double adp = (Convert.ToInt32(dashas[mainlord.Substring(0, 2).ToLower()]) * Convert.ToInt32(dashas[sublord]) * Convert.ToInt32(dashas[arr[vi]])) / (double)(120 * 120);
                        string adp_s = adp.ToString();
                        int ady = Convert.ToInt32(adp_s.IndexOf('.') > -1 ? adp_s.Split('.')[0] : adp_s);
                        double adm = adp_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adp_s.Split('.')[1]) * 12 : 0;
                        string adm_s = adm.ToString();
                        int adm1 = Convert.ToInt32(adm_s.IndexOf('.') > -1 ? adm_s.Split('.')[0] : adm_s);
                        double adys = adm_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adm_s.Split('.')[1]) * 30 : 0;
                        string adys_s = adys.ToString();
                        //console.log('adys_s=',adys_s);
                        adys = Convert.ToDouble(adys_s.IndexOf('.') > -1 ? adys_s.Split('.')[0] : adys_s);
                        double adhs = adys_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adys_s.Split('.')[1]) * 24 : 0;
                        string adhs_s = adhs.ToString();
                        adhs = Convert.ToDouble(adhs_s.IndexOf('.') > -1 ? adhs_s.Split('.')[0] : adhs_s);
                        double adhms = adhs_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adhs_s.Split('.')[1]) * 60 : 0;
                        string adhms_s = adhms.ToString();
                        adhms = Convert.ToDouble(adhms_s.IndexOf('.') > -1 ? adhms_s.Split('.')[0] : adhms_s);
                        s_dt = s_dt.AddYears(ady);
                        s_dt = s_dt.AddMonths(adm1);
                        s_dt = s_dt.AddDays(adys);
                        s_dt = s_dt.AddHours(adhs);
                        s_dt = s_dt.AddMinutes(adhms);
                    }
                    string pbeg = string.Format("{0}/{1}/{2}", dd, m, y);
                    string pend = string.Format("{0}/{1}/{2}", s_dt.Day.ToString(), (s_dt.Month).ToString(), s_dt.Year.ToString());
                    var sty = "pdas";
                    if (cur_date >= b_dt && cur_date <= s_dt) sty = "pdasc";
                    var pdas = new Dasha
                    {
                        lord = string.Format("{0}-{1}-{2}", dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mainlord.ToLower())], sublord, arr[vi]),
                        per = string.Format("{0} To {1}", pbeg, pend),
                        type = "PDAS",
                        style = sty,
                        subs = false,
                        show = false,
                        icon = ""
                    };
                    dctVim.Add(string.Format("{0}-{1}-{2}", mainlord, sublord, arr[vi]), pdas);
                    if (s_dt >= enddt) break;
                    v_start = 1;
                }
                if (vi == 8) vi = -1;
                if (v_iter == 9) break;
            }
        }
        static double buildPratyantarDasha(string mainlord, string sublord, DateTime startdt, DateTime enddt, double ofset, double e_dys, Dictionary<string, int> dashas, Dictionary<string, string> dctAst, ref Dictionary<string, Dasha> dctVim)
        {
            //var d_yr = 360;//365.2425;//this.days_of_a_year(startdt.getFullYear());//365.2425;//366;//354.367056;//;////354.367056;//365.2425;
            string[] arr = { "su", "mo", "ma", "ra", "ju", "sa", "me", "ke", "ve" };
            var v_start = 0;
            var v_iter = 0;
            //var a_per = 0;
            double vdays = 0.0;
            double pdays = 0.0;
            var s_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
            var p_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
            var e_dt = new DateTime(startdt.Year, startdt.Month, startdt.Day, startdt.Hour, startdt.Minute, startdt.Second);
            var cur_date = DateTime.Now;
            // var oned = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
            double diffdys = ofset;
            for (var vi = 0; vi < 9; vi++)
            {
                if (arr[vi] == sublord || v_start == 1)
                {
                    var b_dt = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                    double adp = Convert.ToInt32(dashas[mainlord.Substring(0, 2).ToLower()]) * Convert.ToInt32(dashas[sublord]) * Convert.ToInt32(dashas[arr[vi]]) / (double)(120 * 120);
                    string adp_s = adp.ToString();
                    int ady = Convert.ToInt32(adp_s.IndexOf('.') > -1 ? adp_s.Split('.')[0] : adp_s);
                    double adm = adp_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adp_s.Split('.')[1]) * 12 : 0;
                    string adm_s = adm.ToString();
                    int adm1 = Convert.ToInt32(adm_s.IndexOf('.') > -1 ? adm_s.Split('.')[0] : adm_s);
                    double adys = adm_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adm_s.Split('.')[1]) * 30 : 0;
                    string adys_s = adys.ToString();
                    //console.log('adys_s=',adys_s);
                    adys = Convert.ToDouble(adys_s.IndexOf('.') > -1 ? adys_s.Split('.')[0] : adys_s);
                    double adhs = adys_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adys_s.Split('.')[1]) * 24 : 0;
                    string adhs_s = adhs.ToString();
                    adhs = Convert.ToDouble(adhs_s.IndexOf('.') > -1 ? adhs_s.Split('.')[0] : adhs_s);
                    double adhms = adhs_s.IndexOf('.') > -1 ? Convert.ToDouble("0." + adhs_s.Split('.')[1]) * 60 : 0;
                    string adhms_s = adhms.ToString();
                    adhms = Convert.ToDouble(adhms_s.IndexOf('.') > -1 ? adhms_s.Split('.')[0] : adhms_s);
                    s_dt = s_dt.AddYears(ady);
                    s_dt = s_dt.AddMonths(adm1);
                    s_dt = s_dt.AddDays(adys);
                    s_dt = s_dt.AddHours(adhs);
                    s_dt = s_dt.AddMinutes(adhms);
                    //var b_dt = new DateTime(s_dt.Year, s_dt.Month, s_dt.Day, s_dt.Hour, s_dt.Minute, s_dt.Second);
                    pdays = (s_dt - p_dt).TotalDays;
                    diffdys += (s_dt - p_dt).TotalDays;
                    p_dt = p_dt.AddYears(ady);
                    p_dt = p_dt.AddMonths(adm1);
                    p_dt = p_dt.AddDays(adys);
                    p_dt = p_dt.AddHours(adhs);
                    p_dt = p_dt.AddMinutes(adhms);
                    v_iter++;
                    if (diffdys >= e_dys)
                    {
                        var m = e_dt.Month.ToString();
                        var dd = e_dt.Day.ToString();
                        var y = e_dt.Year.ToString();
                        e_dt = e_dt.AddYears(ady);
                        e_dt = e_dt.AddMonths(adm1);
                        e_dt = e_dt.AddDays(adys);
                        e_dt = e_dt.AddHours(adhs);
                        e_dt = e_dt.AddMinutes(adhms);
                        string pbeg = string.Format("{0}/{1}/{2}", dd, m, y);
                        string pend = "";
                        if (e_dt <= enddt)
                            pend = string.Format("{0}/{1}/{2}", e_dt.Day.ToString(), (e_dt.Month).ToString(), e_dt.Year.ToString());
                        else
                            pend = string.Format("{0}/{1}/{2}", enddt.Day.ToString(), (enddt.Month).ToString(), enddt.Year.ToString());
                        var sty = "pdas";
                        if (cur_date >= b_dt && cur_date <= s_dt) sty = "pdasc";
                        var pdas = new Dasha
                        {
                            lord = string.Format("{0}-{1}-{2}", dctAst[System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(mainlord.ToLower())], sublord, arr[vi]),
                            per = string.Format("{0} To {1}", pbeg, pend),
                            type = "PDAS",
                            style = sty,
                            subs = false,
                            show = false,
                            icon = ""
                        };
                        dctVim.Add(string.Format("{0}-{1}-{2}", mainlord, sublord, arr[vi]), pdas);
                        vdays += pdays;
                    }
                    v_start = 1;
                    if (e_dt >= enddt) break;
                }
                if (vi == 8) vi = -1;
                if (v_iter == 9) break;
            }
            return vdays;
        }

        [HttpGet("DailyHoroscope")]
        public IActionResult DailyHoroscope(string sign)
        {
            //DBLog(string.Format("DailyHoroscope-{0}", sign));
            string liStr = string.Empty;
            var posts = Enumerable.Empty<Post>();
            posts = new RSSFeedReader().ReadFeed(@"http://feeds.feedburner.com/dayhoroscope?format=xml");
			

            string result = "Daily Predictions";
            if (sign.ToLower() == "saggitarius") sign = "sagittarius";
            foreach (var post in posts.ToList())
            {
                if (post.Title.Split(' ')[0].ToLower() == sign.ToLower())
                {
                    result = post.Description.Replace("AstroSage.com,", "").Split('\n')[1];
                    break;
                }
            }
            return new JsonResult(result);

        }

        [HttpGet("SubscribeAstroUser")]
        public IActionResult SubscribeAstroUser(string token, string sign, string deg)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var astUsers = db.GetCollection<AstroUser>("AstroUser");
                var filter = Builders<AstroUser>.Filter.Eq("devicetoken", token);
                var update = Builders<AstroUser>.Update.Set("moonsign", sign);
                update = update.Set("moondeg", deg);
                try
                {
                    long cnt = astUsers.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        astUsers.FindOneAndUpdate(filter, update);
                    }
                    else
                    {
                        var astUser = new AstroUser
                        {
                            devicetoken = token,
                            moonsign = sign,
                            moondeg = float.Parse(deg)
                        };
                        astUsers.InsertOneAsync(astUser);
                    }
                }
                catch
                {
                    var astUser = new AstroUser
                    {
                        devicetoken = token,
                        moonsign = sign,
                        moondeg = float.Parse(deg)
                    };
                    astUsers.InsertOneAsync(astUser);
                }
                return new JsonResult("success");
            }
            catch (Exception eX)
            {
                return new JsonResult(eX.Message);
            }
        }
        [HttpGet("TalkToAstro")]
        public ActionResult TalkToAstro(string uid, string uuid, string aid)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var infc = db.GetCollection<AstroTalk>("AstroTalk");
                var inf = new AstroTalk
                {
                    uuid = uuid,
                    uid = uid,
                    aid = aid,
                    date = DateTime.Now.ToShortDateString()
                };
                infc.InsertOne(inf);
                return new JsonResult("success");
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }

        [HttpGet("BirthchartPro")]
        public async Task<IActionResult> BirthchartPro(string dob, string tob, string latlng, string timezone, double tzofset, string name, string eml, int ayanid)
        {
            try
            {

				string plst = string.Empty;
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
                double u7 = Convert.ToDouble(latlng.Split('|')[0]);
                double u8 = Convert.ToDouble(latlng.Split('|')[1]);
                string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                // checkYogs(mHoro);
                //return PartialView("Birthchart", mHoro);
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
				Horo horo = new Horo();
				for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (mHoro.planetsPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = mHoro.planetsPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }
							string code = pl.Split(' ')[1];
							if (code != "Ur" && code != "Pl" && code != "me" && code != "os" && code != "Ne" && code != "AC" && code != "TRUE_NODE")
							{  //consider only true planets
								PlanetHouse pHou = new PlanetHouse
								{
									code = code,
									name = "",
									hno = 0,
									mhno = 0,
									shno = 0,
									pos = 0,
									sign = zod_nam[i],
									signtype = "",
									lordship = "",
									houselord = ""
								};
								string desc = descStrength(pHou);
								if(desc.Trim() != string.Empty)
									horo.plStren += string.Format("{0},", desc);
							}
							
						}
					}
                }
                horo.planetPos = dctPls;
                horo.retroPls = mHoro.retroPls;
				//await Task.Run(() =>
				//{
				//	horo.astkPts = CalcAstak(dob, tob, latlng, timezone, tzofset, ayanid);
				//});
				
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1} Params {2}", eX.Message, line, string.Format("{0},{1},{2},{3},{4},{5}", dob, tob, latlng, timezone, name, eml)));
            }
        }
        [HttpGet("Birthchart")]
        public ActionResult Birthchart(string dob, string tob, string latlng, string timezone, string name, string eml)
        {
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, tz, false, string.Empty);
                mHoro.calc_planets_pos(false, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                // checkYogs(mHoro);
                //return PartialView("Birthchart", mHoro);
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (mHoro.planetsPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = mHoro.planetsPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }

                        }
                    }
                }
                mHoro.planetsPos = dctPls;
                return new JsonResult(mHoro.planetsPos);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1} Params {2}", eX.Message, line, string.Format("{0},{1},{2},{3},{4},{5}", dob, tob, latlng, timezone, name, eml)));
            }
        }

        [HttpGet("Birthstars")]
        public ActionResult Birthstars(string dob, string tob)
        {
            try
            {
                //DBLog(string.Format("Birthstars-{0}", dob));
                string bsn = string.Empty;
                string pbsn = string.Empty;
                string latlng = "17.23|78.29";
                string timezone = "India Standard Time";
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('-')[0].Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('-')[0].Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('-')[0].Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('-')[0].Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('-')[0].Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('-')[0].Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, timezone, false, string.Empty);
                mHoro.calc_planets_pos(false, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                Horoscope wHoro = new Horoscope();
                uint w1 = Convert.ToUInt32(dob.Split('-')[1].Split('|')[0]);
                uint w2 = Convert.ToUInt32(dob.Split('-')[1].Split('|')[1]);
                int w3 = Convert.ToInt32(dob.Split('-')[1].Split('|')[2]);
                uint w4 = Convert.ToUInt32(tob.Split('-')[1].Split('|')[0]);
                uint w5 = Convert.ToUInt32(tob.Split('-')[1].Split('|')[1]);
                uint w6 = Convert.ToUInt32(tob.Split('-')[1].Split('|')[2]);
				double w7 = Convert.ToDouble(latlng.Split('|')[0]);
				double w8 = Convert.ToDouble(latlng.Split('|')[1]);
				wHoro.init_data(w1, w2, w3, w4, w5, w6, w7, w8, timezone, false, string.Empty);
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                wHoro.calc_planets_pos(false, astClient);
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                BirthStar bStar = new BirthStar();
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    bool bmon = false;
                    bool bwmon = false;
                    foreach (var sign in signs)
                    {
                        if (!bmon && mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            float moonDeg = 0;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bStar.birthSignDeg = string.Format("{0}.{1}", pl.Split(' ')[0].Split('.')[0], pl.Split(' ')[0].Split('.')[1]);
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bStar.birthSignDeg = string.Format("{0}.{1}", pls.Split(' ')[0].Split('.')[0], pls.Split(' ')[0].Split('.')[1]);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                string nO = string.Format(@"{0}\nakshatra_o.json", astClient);
                                using (StreamReader r5 = new StreamReader(nO))
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json5 = r5.ReadToEnd();
                                    string json4 = r4.ReadToEnd();
                                    dynamic nak_o = JsonConvert.DeserializeObject(json5);
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    bsn = sign.ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    bsn = sign.ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    bsn = sign.ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!bwmon && wHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            float moonDeg = 0;
                            string pls = wHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bStar.partnerBirthSignDeg = string.Format("{0}.{1}", pl.Split(' ')[0].Split('.')[0], pl.Split(' ')[0].Split('.')[1]);
                                        bwmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bStar.partnerBirthSignDeg = string.Format("{0}.{1}", pls.Split(' ')[0].Split('.')[0], pls.Split(' ')[0].Split('.')[1]);
                                bwmon = true;
                            }
                            if (bwmon)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                string nO = string.Format(@"{0}\nakshatra_o.json", astClient);
                                using (StreamReader r5 = new StreamReader(nO))
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json5 = r5.ReadToEnd();
                                    string json4 = r4.ReadToEnd();
                                    dynamic nak_o = JsonConvert.DeserializeObject(json5);
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    bStar.partnerBirthStar = nak.name;
                                                    bStar.partnerBirthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    pbsn = sign.ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    bStar.partnerBirthStar = nak.name;
                                                    bStar.partnerBirthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    pbsn = sign.ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    bStar.partnerBirthStar = nak.name;
                                                    bStar.partnerBirthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    pbsn = sign.ToString();
                                                    break;
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        if (bmon && bwmon) break;
                    }
                }
                bStar.manglik = "0";
                foreach (var pl in mHoro.planetsPos)
                {
                    if (pl.Value.Contains("Ma"))
                    {
                        int hno = calcHno(bsn, pl.Key);
                        if (hno == 1 || hno == 2 || hno == 4 || hno == 7 || hno == 12)
                        {
                            bStar.manglik = string.Format("1|{0}", hno);
                        }
                    }
                }
                bStar.partnerManglik = "0";
                foreach (var pl in wHoro.planetsPos)
                {
                    if (pl.Value.Contains("Ma"))
                    {
                        int hno = calcHno(pbsn, pl.Key);
                        if (hno == 1 || hno == 2 || hno == 4 || hno == 7 || hno == 12)
                        {
                            bStar.partnerManglik = string.Format("1|{0}", hno);
                        }
                    }
                }
                return new JsonResult(bStar);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        public double dmsToDec(int d, int m, int s)
        {
            double v = d + (m / (double)60) + (s / (double)3600);
            return Math.Round(v, 2);
        }
        public int calcHno(string ss, string ds)
        {
			if (ss == ds) return 1;
            string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
            int r1 = 0, r2 = 0;
            bool asc = false;
            for (r1 = 0; r1 < 12; r1++)
            {
                if (asc) r2++;
                if (ras[r1] == ss)
                {
                    asc = true;
                    r2++;
                }
                else if (asc && ras[r1] == ds) return r2;
                if (r2 == 12) break;
                if (r1 == 11) r1 = -1;
            }
            return -1;
        }
		public string calcSHno(string ss, int hp)
		{
			string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
			int r1 = 0, r2 = 0;
			bool asc = false;
			for (r1 = 0; r1 < 12; r1++)
			{
				if (asc) r2++;
				if (ras[r1] == ss)
				{
					asc = true;
					r2++;
				}
				else if (asc && r2 == hp) return string.Format("{0}|{1}",ras[r1], r2.ToString()) ;
				if (r2 == 12) break;
				if (r1 == 11) r1 = -1;
			}
			return "-1";
		}
		[HttpGet("StarsForMonth")]
        public ActionResult StarsForMonth(string star, string sign, string moondeg)
        {
            try
            {
                //DBLog(string.Format("StarsForMonth-{0}", star));
                Moon moon = new Moon();
                moon.moonSign = sign;
                moon.birthStar = star;
                string[] mds = moondeg.Split('.');
                moon.moonDeg = Convert.ToInt32(mds[0]) + Convert.ToInt32(mds[1]) / 60 + ((mds.Length > 2) ? Convert.ToInt32(mds[2]) : 0);
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                DateTime today = DateTime.Now;
                int totdys = 0;
                List<StarConst> strConst = new List<StarConst>();
                //int pyr = 0;
                //double ray = 0;
                string ayan = string.Empty;
                while (totdys < 30)
                {
                    //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    //calcStar(tday);
                    //if (pyr != today.Year)
                    //{
                    //ray = (double)((today.Year - 397) * eqt);
                    ////tdms = dms(ray);
                    ////double d = (double)(ray / (double)3600);
                    ////int m = (int)Math.Floor(ray/3600-d) * 60;
                    ////int s = (int)Math.Floor(ray - d * 3600 - (m * 60));
                    ////int sec = (int)Math.Round(d * 3600);
                    //int deg = (int)(ray / 3600);
                    //int sec = (int)Math.Abs(ray % 3600);
                    //int min = sec / 60;
                    //sec %= 60;
                    //ayan = string.Format("{0}.{1}.{2}", deg, min, sec);
                    //}
                    //pyr = today.Year;
                    BirthStar cStar = GetBStar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute), ayan, (int)AYANMSAS.LAHIRI);
                    moon.curSign = cStar.birthSign;
                    moon.curStar = cStar.birthStar;
                    calcStarStrength(ref moon);
                    calcLunarStrengh(ref moon);
                    StarConst str = new StarConst();
                    str.date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today); //string.Format(@"{0} {1}/{2}/{3}", today.DayOfWeek.ToString( today.Day, today.Month, today.Year);
                    str.star = moon.curStar;
                    str.starStrength = moon.starStrength;
                    str.lunarStrength = moon.moonStrength;
                    double bsd = Convert.ToInt32(cStar.birthSignDeg.Split('.')[0]) + Convert.ToInt32(cStar.birthSignDeg.Split('.')[1]) / 60 + Convert.ToInt32(cStar.birthSignDeg.Split('.')[2]) / 3600;
                    double sud = Convert.ToInt32(cStar.sunDeg.Split('.')[0]) + Convert.ToInt32(cStar.sunDeg.Split('.')[1]) / 60 + Convert.ToInt32(cStar.sunDeg.Split('.')[2]) / 3600;
                    string tithi = calcTithi(cStar.birthSign, bsd, cStar.sunSign, sud);
                    str.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    str.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : tithi;
                    strConst.Add(str);
                    totdys++;
                    today = today.AddDays(1);
                }
                return new JsonResult(strConst.ToList());
            }
            catch (Exception eX)
            {
                return new JsonResult(eX.Message);
            }
        }
        [HttpGet("StarsForMonthEx")]
        public ActionResult StarsForMonthEx(string star, string sign, string moondeg, string timezone, int ayanid)
        {
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                Moon moon = new Moon();
                moon.moonSign = sign;
                moon.birthStar = star;
                string[] mds = moondeg.Split('.');
                moon.moonDeg = Convert.ToInt32(mds[0]) + Convert.ToInt32(mds[1]) / 60 + ((mds.Length > 2) ? Convert.ToInt32(mds[2]) : 0);
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                DateTime today = DateTime.Now;
                TimeSpan ts = new TimeSpan(10, 30, 0);
                today = today.Date + ts;
                int totdys = 0;
                List<StarConst> strConst = new List<StarConst>();
                //int pyr = 0;
                //double ray = 0;
                string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx(today.Day, today.Month, today.Year, tzofset, (AYANMSAS)ayanid);
                }
                while (totdys < 30)
                {
                    //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    //calcStar(tday);
                    //if (pyr != today.Year)
                    //{
                    //ray = (double)((today.Year - 397) * eqt);
                    ////tdms = dms(ray);
                    ////double d = (double)(ray / (double)3600);
                    ////int m = (int)Math.Floor(ray/3600-d) * 60;
                    ////int s = (int)Math.Floor(ray - d * 3600 - (m * 60));
                    ////int sec = (int)Math.Round(d * 3600);
                    //int deg = (int)(ray / 3600);
                    //int sec = (int)Math.Abs(ray % 3600);
                    //int min = sec / 60;
                    //sec %= 60;
                    //ayan = string.Format("{0}.{1}.{2}", deg, min, sec);
                    //}
                    //pyr = today.Year;
                    //string ayan = string.Empty;

                    //TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    //TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    //double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    // ayan = Ayanmsa.CalcEx(today.Day, today.Month, today.Year, tzofset, (AYANMSAS)ayanid);
                    // ayan = Ayanmsa.Calc(today.Day, today.Month, today.Year, AYANMSAS.BVRAMAN);
                    //JsonResult json1 = (JsonResult)BirthstarEx(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute), ayan);
                    BirthStar cStar = GetBStar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute), ayan, ayanid);
                    moon.curSign = cStar.birthSign;
                    moon.curStar = cStar.birthStar;
                    calcStarStrength(ref moon);
                    calcLunarStrengh(ref moon);
                    StarConst str = new StarConst();
                    str.date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today); //string.Format(@"{0} {1}/{2}/{3}", today.DayOfWeek.ToString( today.Day, today.Month, today.Year);
                    str.star = moon.curStar;
                    str.starStrength = moon.starStrength;
                    str.lunarStrength = moon.moonStrength;
                    double bsd = Convert.ToInt32(cStar.birthSignDeg.Split('.')[0]) + Convert.ToInt32(cStar.birthSignDeg.Split('.')[1]) / 60 + Convert.ToInt32(cStar.birthSignDeg.Split('.')[2]) / 3600;
                    double sud = Convert.ToInt32(cStar.sunDeg.Split('.')[0]) + Convert.ToInt32(cStar.sunDeg.Split('.')[1]) / 60 + Convert.ToInt32(cStar.sunDeg.Split('.')[2]) / 3600;
                    string tithi = calcTithi(cStar.birthSign, bsd, cStar.sunSign, sud);
                    str.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    str.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : tithi;
                    strConst.Add(str);
                    totdys++;
                    today = today.AddDays(1);
                }
                return new JsonResult(strConst.ToList());
            }
            catch (Exception eX)
            {
                return new JsonResult(eX.Message);
            }
        }
        private BirthStar GetBStar(string dob, string tob, string ayan, int ayanid)
        {
            try
            {
                //DBLog("BirthstarEx"); 
                string latlng = "17.23|78.29";
                string timezone = "India Standard Time";
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, timezone, ayan, (uint)ayanid);
                mHoro.calc_houses();
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                mHoro.calc_planets_pos(true, astClient);
                //mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, u9, u10, timezone, (ayan == string.Empty) ? false : true, ayan);
                //mHoro.calc_planets_pos(false);
                BirthStar bStar = new BirthStar();
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            float moonDeg = 0;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bStar.birthSignDeg = pl.Split(' ')[0];
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bStar.birthSignDeg = pls.Split(' ')[0];
                                bmon = true;
                            }
                            if (bmon)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg < nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg < nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            float sunDeg = 0;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bStar.sunDeg = pl.Split(' ')[0];
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bStar.sunDeg = pls.Split(' ')[0];
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    bStar.sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                }

                return bStar;
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return null;
            }
        }

        string calcTithi(string ms, double md, string ss, double sd)
        {
            int tithi = 0;
            try
            {
                var tDict = new Dictionary<int, string> { { 0, "Prathama" }, { 1, "Dwitiya" }, { 2, "Tritiya" }, { 3, "Chaturthi" }, { 4, "Panchami" }, { 5, "Shashthi" }, { 6, "Sapthami" }, { 7, "Asthami" }, { 8, "Navami" }, { 9, "Dasami" }, { 10, "Ekadashi" }, { 11, "Dwadashi" }, { 12, "Trayodashi" }, { 13, "Chaturdashi" }, { 14, "Purnima" } };
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string rJ = string.Format(@"{0}\rashis.json", astClient);
                using (StreamReader r4 = new StreamReader(rJ))
                {
                    string json4 = r4.ReadToEnd();
                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                    int rnum = Convert.ToInt32(rashis[ms.ToLower()].ToString());
                    int mdeg = (rnum - 1) * 30;
                    rnum = Convert.ToInt32(rashis[ss.ToLower()].ToString());
                    int sdeg = (rnum - 1) * 30;
                    //float mcd = float.Parse(md) + mdeg;
                    sd += sdeg;
                    md += mdeg;
                    //float scd = float.Parse(sd) + sdeg;
                    //mcd = ConvertDegreeAngleToDouble((mcd.ToString().Contains('.')) ? Convert.ToInt32(mcd.ToString().Split('.')[0]) : Convert.ToInt32(mcd), (mcd.ToString().Contains('.')) ? Convert.ToInt32(mcd.ToString().Split('.')[1]) : 0, 0);
                    //scd = ConvertDegreeAngleToDouble((scd.ToString().Contains('.')) ? Convert.ToInt32(scd.ToString().Split('.')[0]) : Convert.ToInt32(scd), (scd.ToString().Contains('.')) ? Convert.ToInt32(scd.ToString().Split('.')[1]) : 0, 0);
                    double diff = md - sd;
                    if (diff < 0) diff += 360;
                    diff = (diff / 12);
                    double dth = Math.Round(diff, 0, MidpointRounding.AwayFromZero);
                    bool dh = false;
                    if (dth > 15)
                    {
                        dh = true;
                        dth -= 15;
                    }
                    tithi = (int)dth;
                    if (tithi == 0) tithi++;
                    string thi = string.Empty;
                    //if (tithi == 0) thi = "Amavasya";
                    //else
                    //{
                    thi = tDict[tithi - 1];
                    if (thi == "Purnima" && dh == true)
                        thi = "Amavasya";
                    //}
                    double dec = diff - (int)diff;
                    double rem = 1 - dec;
                    double remp = Math.Ceiling(rem * 100);
                    //var regex = new System.Text.RegularExpressions.Regex("(?<=[\\.])[0-9]+");
                    //double remp = 0.0;
                    //if (regex.IsMatch(diff.ToString()))
                    //{
                    //string sdec = string.Format("0.{1}", regex.Match(diff.ToString()).Value);
                    // double rem = 1 - Convert.ToDouble(sdec);
                    //remp = Math.Ceiling(rem * 100);
                    //}
                    return string.Format("{0}|{1}|{2}", thi, (dh == true) ? "wanning" : "waxing", remp);
                }
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return string.Format("ERROR: {0} {1} {2} {3} LINE {4}", eX.Message, tithi, ms, ss, line);
            }
        }
        public BirthStar calcBirthStar(double pp, string ps)
        {
            BirthStar bStar = new BirthStar();
            string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
            string rJ = string.Format(@"{0}\o_rashis.json", astClient);
            using (StreamReader r4 = new StreamReader(rJ))
            {
                string json4 = r4.ReadToEnd();
                dynamic rashis = JsonConvert.DeserializeObject(json4);
                // int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                    foreach (var nak in nakshatras)
                    {
                        string[] snak = nak.location.start.Split(',')[0].Split('.');
                        double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                        string[] enak = nak.location.end.Split(',')[0].Split('.');
                        double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                        if (nak.location.start.Split(',')[1] == ps.ToString() && nak.location.end.Split(',')[1] == ps.ToString())
                        {
                            if (pp >= nakd1 && pp <= nakd2)
                            {
                                bStar.birthStar = nak.name;
                                bStar.birthSign = rashis[ps.ToString()].ToString().Split('|')[1].ToString();
                                bStar.startSign = nak.location.start.Split(',')[1];
                                bStar.endSign = nak.location.end.Split(',')[1];
                                bStar.startDeg = nak.location.start.Split(',')[0];
                                bStar.ruler = nak.ruler;
                                break;
                            }
                        }
                        else if (nak.location.start.Split(',')[1] == ps.ToLower())
                        {
                            if (pp >= nakd1)
                            {
                                bStar.birthStar = nak.name;
                                bStar.birthSign = rashis[ps.ToString()].ToString().Split('|')[1].ToString();
                                bStar.startSign = nak.location.start.Split(',')[1];
                                bStar.endSign = nak.location.end.Split(',')[1];
                                bStar.startDeg = nak.location.start.Split(',')[0];
                                bStar.ruler = nak.ruler;
                                break;
                            }
                        }
                        else if (nak.location.end.Split(',')[1] == ps.ToLower())
                        {
                            if (pp <= nakd2)
                            {
                                bStar.birthStar = nak.name;
                                bStar.birthSign = rashis[ps.ToString()].ToString().Split('|')[1].ToString();
                                bStar.startSign = nak.location.start.Split(',')[1];
                                bStar.endSign = nak.location.end.Split(',')[1];
                                bStar.startDeg = nak.location.start.Split(',')[0];
                                bStar.ruler = nak.ruler;
                                break;
                            }
                        }
                    }
                }
            }
            return bStar;
        }
        public float ConvertDegreeAngleToDouble(float degrees, float minutes, float seconds)
        {
            //Decimal degrees = 
            //   whole number of degrees, 
            //   plus minutes divided by 60, 
            //   plus seconds divided by 3600

            return degrees + (minutes / 60);// +(seconds / 3600);
        }
        void calcStarStrength(ref Moon moon)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                using (StreamReader r3 = new StreamReader(string.Format(@"{0}\nakshatras.json", astClient)))
                {
                    string json3 = r3.ReadToEnd();
                    List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json3);
                    int b_num = 0;
                    int c_num = 0;

                    foreach (Star nak in nakshatras)
                    {
                        if (nak.name == moon.birthStar)
                        {
                            b_num = nak.order;
                        }
                        else if (nak.name == moon.curStar)
                        {
                            c_num = nak.order;
                        }
                        if (b_num != 0 && c_num != 0)
                            break;
                    }
                    int m_num = Math.Abs(b_num - c_num);
                    if (m_num <= 9)
                        moon.starWeight = m_num;
                    else
                    {
                        moon.starWeight = m_num % 9;
                    }
                    switch (moon.starWeight)
                    {
                        case 1:
                            moon.starStrength = "Janma/ danger to body";
                            break;
                        case 2:
                            moon.starStrength = "Sampat/ Wealth and Prosperity";
                            break;
                        case 3:
                            moon.starStrength = "Vipat/ Dangers, Losses, Accidents";
                            break;
                        case 4:
                            moon.starStrength = "Kshema/ Prosperity";
                            break;
                        case 5:
                            moon.starStrength = "Pratyak/ Obstacles";
                            break;
                        case 6:
                            moon.starStrength = "Sadhana/ Realisation and Ambitions";
                            break;
                        case 7:
                            moon.starStrength = "Naidhana/ Dangers";
                            break;
                        case 8:
                            moon.starStrength = "Mitra/ Good";
                            break;
                        case 9:
                        case 0:
                            moon.starStrength = "Prama Mitra/ Very Favourable";
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
            }
        }
        void calcLunarStrengh(ref Moon moon)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                using (StreamReader r3 = new StreamReader(string.Format(@"{0}\rashis.json", astClient)))
                {
                    string json3 = r3.ReadToEnd();
                    Console.WriteLine(json3);
                    dynamic rashis = JsonConvert.DeserializeObject(json3);
                    Console.WriteLine(moon.curStar);
                    Console.WriteLine(moon.curSign);
                    int b_num = Convert.ToInt32(rashis[moon.moonSign.ToLower()].ToString());
                    Console.WriteLine(string.Format("The rashi no. of your moon sign is {0}, current sign is {1}", b_num.ToString(), moon.curSign.ToLower()));
                    int c_num = Convert.ToInt32(rashis[moon.curSign.ToLower()].ToString());
                    Console.WriteLine("The rashi no. of your current moon sign is " + c_num.ToString());

                    int m_num = Math.Abs(b_num - c_num);
                    moon.moonWeight = m_num;
                    if (m_num == 6 || m_num == 8 || m_num == 12)
                    {
                        if (m_num == 8)
                            moon.moonStrength = "Chandrastama, Bad";
                        else
                            moon.moonStrength = "Bad";
                    }
                    else
                    {
                        moon.moonStrength = "-";
                    }
                }
            }
            catch (Exception eX)
            {
                Console.WriteLine(eX.Message);
            }
        }
        [HttpGet("Getcusps")]
        public ActionResult Getcusps(string dob, string tob, string latlng, string timezone)
        {
            try
            {
                // log.Info(string.Format("GETCUSPS-{0},{1},{2},{3}", dob, tob, latlng, timezone));
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                string ayan = string.Empty;
                //using (StreamReader r = new StreamReader(sF))
                //{
                //    string jsa = r.ReadToEnd();
                //    dynamic ayans = JsonConvert.DeserializeObject(jsa);
                //    ayan = ayans[i3.ToString()].ToString();
                //}
                ayan = Ayanmsa.Calc((int)u1, (int)u2, i3, AYANMSAS.KPNEW);
                mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, TZConvert.IanaToWindows(timezone), true, ayan);
                mHoro.calc_planets_pos(true, astClient);
                mHoro.calc_houses();
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                horo.housePos = mHoro.housePos;
                horo.ascPos = mHoro.ascPos;
                float moonDeg = 0;
                string moonSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                //string astClient = System.Web.HttpContext.Current.Server.MapPath("~/Content/astroclient");
                sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                moonSign = calcStar(moonDeg, sign.ToString());
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                    string tithi = calcTithi(moonSign, moonDeg, sunSign, sunDeg);
                    horo.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    horo.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                    horo.tithiRem = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[2] : "";
                }
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (horo.planetPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = horo.planetPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }
                        }
                    }
                }
                horo.planetPos = dctPls;
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetcuspsEx")]
        public ActionResult GetcuspsEx(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
        {
            try
            {

                //log.Info(string.Format("GETCUSPS-{0},{1},{2},{3}", dob, tob, latlng, timezone));
                Horoscope mHoro = new Horoscope();
                string tz = TZConvert.IanaToWindows(timezone);
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				int u7 = Convert.ToInt32(latlng.Split('|')[0].Split('.')[0]);
				uint u8 = Convert.ToUInt32(latlng.Split('|')[0].Split('.')[1]);
				int u9 = Convert.ToInt32(latlng.Split('|')[1].Split('.')[0]);
				uint u10 = Convert.ToUInt32(latlng.Split('|')[1].Split('.')[1]);
				string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                //string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                //string ayan = string.Empty;
                //using (StreamReader r = new StreamReader(sF))
                //{
                //    string jsa = r.ReadToEnd();
                //    dynamic ayans = JsonConvert.DeserializeObject(jsa);
                //    ayan = ayans[i3.ToString()].ToString();
                //}
                string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_houses();
                mHoro.calc_planets_pos(true, astClient);
                //ayan = Ayanmsa.Calc((int)u1, (int)u2, i3, AYANMSAS.KPNEW);
                //mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, u9, u10, TZConvert.IanaToWindows(timezone), true, ayan);
                //mHoro.calc_planets_pos(true);
                //mHoro.calc_houses();
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                horo.housePos = mHoro.housePos;
                horo.ascPos = mHoro.ascPos;
                float moonDeg = 0;
                string moonSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                //string astClient = System.Web.HttpContext.Current.Server.MapPath("~/Content/astroclient");
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                moonSign = calcStar(moonDeg, sign.ToString());
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                    string tithi = calcTithi(moonSign, moonDeg, sunSign, sunDeg);
                    horo.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    horo.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                    horo.tithiRem = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[2] : "";
                }
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (horo.planetPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = horo.planetPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }
                        }
                    }
                }
                horo.planetPos = dctPls;
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                //DBLog(string.Format("GetcuspsEx-EXCEPTION"));
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetcuspsEx2")]
        public ActionResult GetcuspsEx2(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
        {
            try
            {
                //log.Info(string.Format("GETCUSPS-{0},{1},{2},{3}", dob, tob, latlng, timezone));
                Horoscope mHoro = new Horoscope();
                string tz = TZConvert.IanaToWindows(timezone);
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
                double u7 = Convert.ToDouble(latlng.Split('|')[0]);
                double u8 = Convert.ToDouble(latlng.Split('|')[1]);
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                //string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                //string ayan = string.Empty;
                //using (StreamReader r = new StreamReader(sF))
                //{
                //    string jsa = r.ReadToEnd();
                //    dynamic ayans = JsonConvert.DeserializeObject(jsa);
                //    ayan = ayans[i3.ToString()].ToString();
                //}
                string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_houses();
                mHoro.calc_planets_pos(true, astClient);
                //ayan = Ayanmsa.Calc((int)u1, (int)u2, i3, AYANMSAS.KPNEW);
                //mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, u9, u10, TZConvert.IanaToWindows(timezone), true, ayan);
                //mHoro.calc_planets_pos(true);
                //mHoro.calc_houses();
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
				horo.planetDecl = mHoro.planetsDecl;
				horo.planetSped = mHoro.planetSped;
                horo.housePos = mHoro.housePos;
                horo.ascPos = mHoro.ascPos;
                float moonDeg = 0;
                string moonSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                //string astClient = System.Web.HttpContext.Current.Server.MapPath("~/Content/astroclient");
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                moonSign = calcStar(moonDeg, sign.ToString());
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                    string tithi = calcTithi(moonSign, moonDeg, sunSign, sunDeg);
                    horo.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    horo.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                    horo.tithiRem = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[2] : "";
                }
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                //DBLog(string.Format("GetcuspsEx-EXCEPTION"));
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        public string calcStar(double plpos, string sign)
        {
            //Dictionary<string, int> zods = new Dictionary<string, int>();
            //zods["ar"] = 0;
            //zods["ta"] = 30;
            //zods["ge"] = 60;
            //zods["cn"] = 90;
            //zods["le"] = 120;
            //zods["vi"] = 150;
            //zods["li"] = 180;
            //zods["sc"] = 210;
            //zods["sa"] = 240;
            //zods["cp"] = 270;
            //zods["aq"] = 300;
            //zods["pi"] = 330;
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                    foreach (var nak in nakshatras)
                    {
                        if (nak.location.start.Split(',')[1] == sign && nak.location.end.Split(',')[1] == sign)
                        {
                            if (plpos >= float.Parse(nak.location.start.Split(',')[0]) && plpos < float.Parse(nak.location.end.Split(',')[0]))
                            {
                                return nak.name;
                            }
                        }
                        else if (nak.location.start.Split(',')[1] == sign.ToString())
                        {
                            if (plpos >= float.Parse(nak.location.start.Split(',')[0]))
                            {
                                return nak.name;
                            }
                        }
                        else if (nak.location.end.Split(',')[1] == sign.ToString())
                        {
                            if (plpos < float.Parse(nak.location.end.Split(',')[0]))
                            {
                                return nak.name;
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception eX)
            {
                return eX.Message;
            }
        }

        public string calcStarL(double plpos, string sign)
        {
            //Dictionary<string, int> zods = new Dictionary<string, int>();
            //zods["ar"] = 0;
            //zods["ta"] = 30;
            //zods["ge"] = 60;
            //zods["cn"] = 90;
            //zods["le"] = 120;
            //zods["vi"] = 150;
            //zods["li"] = 180;
            //zods["sc"] = 210;
            //zods["sa"] = 240;
            //zods["cp"] = 270;
            //zods["aq"] = 300;
            //zods["pi"] = 330;
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                    foreach (var nak in nakshatras)
                    {
                        if (nak.location.start.Split(',')[1] == sign && nak.location.end.Split(',')[1] == sign)
                        {
                            if (plpos >= float.Parse(nak.location.start.Split(',')[0]) && plpos <= float.Parse(nak.location.end.Split(',')[0]))
                            {
                                return nak.ruler;
                            }
                        }
                        else if (nak.location.start.Split(',')[1] == sign.ToString())
                        {
                            if (plpos >= float.Parse(nak.location.start.Split(',')[0]))
                            {
                                return nak.ruler;
                            }
                        }
                        else if (nak.location.end.Split(',')[1] == sign.ToString())
                        {
                            if (plpos <= float.Parse(nak.location.end.Split(',')[0]))
                            {
                                return nak.ruler;
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception eX)
            {
                return eX.Message;
            }
        }
        [HttpGet("GetTransits")]
        public ActionResult GetTransits(string mdas, string adas, string pdas, string pend)
        {
            try
            {
                mdas = mdas.ToLower();
                adas = adas.ToLower();
                pdas = pdas.ToLower();
                List<Transit> trans = new List<Transit>();
                DateTime today = DateTime.Now;
                DateTime eday = new DateTime(Convert.ToInt32(pend.Split('-')[2]), Convert.ToInt32(pend.Split('-')[1]), Convert.ToInt32(pend.Split('-')[0]));
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string nJ = string.Format(@"{0}\sublordz.json", astClient);
                string pN = string.Format(@"{0}\planet_stars.json", astClient);
                string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string cnstls = string.Empty;
                string sssl = string.Empty;
                using (StreamReader r3 = new StreamReader(pN))
                {
                    string json = r3.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json);
                    var exists = data.Property(mdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(adas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(pdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    cnstls = cnstls.Remove(cnstls.Length - 1).Trim();
                }
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json2);
                    string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                    string ayan = string.Empty;
                    using (StreamReader r = new StreamReader(sF))
                    {
                        string jsa = r.ReadToEnd();
                        dynamic ayans = JsonConvert.DeserializeObject(jsa);
                        ayan = ayans[today.Year.ToString()].ToString();
                    }
                    while (today <= eday)
                    {
                        //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                        //calcStar(tday);
                        //JsonResult json1 = (JsonResult)Birthstar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute));
                        string latlng = "17.23|78.29";
                        string timezone = "India Standard Time";
                        Horoscope mHoro = new Horoscope();
                        uint u1 = Convert.ToUInt32(today.Day);
                        uint u2 = Convert.ToUInt32(today.Month);
                        int i3 = Convert.ToInt32(today.Year);
                        uint u4 = 5;//Convert.ToUInt32(today.Hour);
                        uint u5 = 0;//Convert.ToUInt32(today.Minute);
                        uint u6 = 0;
						double u7 = Convert.ToDouble(latlng.Split('|')[0]);
						double u8 = Convert.ToDouble(latlng.Split('|')[1]);
						mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, timezone, true, ayan);
                        mHoro.calc_planets_pos(true, astClient);
                        int rpos = 0;
                        foreach (string sign in signs) {
                            rpos++;
                            if (mHoro.planetsPos.ContainsKey(sign)) {
                                var pls = mHoro.planetsPos[sign].Split('|');
                                //var ePls = '';
                                //var mnode = '';
                                for (var k = 0; k < pls.Length; k++) {
                                    if (pls[k].Split(' ')[1] == "MEAN_NODE") {
                                        var kpos = rpos + 6;
                                        if (kpos > 12) kpos = (kpos - 12);
                                        //var mn = i + 11;
                                        //if (mn > 15) mn -= 15;
                                        if (mHoro.planetsPos.ContainsKey(signs[kpos - 1])) {
                                            var eP = mHoro.planetsPos[signs[kpos - 1]];
                                            mHoro.planetsPos[signs[kpos - 1]] = eP + '|' + pls[k].Split(' ')[0] + ' ' + "Ke";
                                        } else {
                                            mHoro.planetsPos[signs[kpos - 1]] = pls[k].Split(' ')[0] + ' ' + "Ke";
                                        }
                                        // plPos[sign] = ePls;
                                        mHoro.planetsPos[sign] = mHoro.planetsPos[sign].Replace("MEAN_NODE", "Ra");
                                    }
                                }
                            }
                        }
                        sssl = getSSSL2(cnstls, mHoro);
                        if (sssl != string.Empty)
                        {
                            //string[] keys = { sssl };
                            foreach (string key in sssl.Split(','))
                            {
                                try
                                {
                                    string lords = key.Split('|')[0];
                                    Transit transit = new Transit
                                    {
                                        date = string.Format("{0}-{1}-{2},{3}", today.Day, today.Month, today.Year, key.Split('|')[1].Split('-')[2]),
                                        signL = lords.Split('-')[0],
                                        starL = lords.Split('-')[1],
                                        subL = lords.Split('-')[2],
                                        star = key.Split('|')[1].Split('-')[0],
                                        sign = key.Split('|')[1].Split('-')[1]
                                    };
                                    trans.Add(transit);
                                }
                                catch (Exception eX)
                                {
                                    Console.WriteLine(eX.Message);
                                }
                            }
                        }
                        else
                        {
                            Transit transit = new Transit
                            {
                                date = string.Format("{0}-{1}-{2}", today.Day, today.Month, today.Year),
                                signL = "",
                                starL = "",
                                subL = "",
                                star = "",
                                sign = ""
                            };
                            trans.Add(transit);
                        }


                        //Dictionary<string, bool> zvisits = new Dictionary<string,bool>();
                        //zvisits["ar"] = false;
                        //zvisits["ta"] = false;
                        //zvisits["ge"] = false;
                        //zvisits["cn"] = false;
                        //zvisits["le"] = false;
                        //zvisits["vi"] = false;
                        //zvisits["li"] = false;
                        //zvisits["sc"] = false;
                        //zvisits["sa"] = false;
                        //zvisits["cp"] = false;
                        //zvisits["aq"] = false;
                        //zvisits["pi"] = false;
                        //string[] keys = {mdas + "-" + adas + "-" + pdas, mdas + "-" + pdas + "-" + adas, adas + "-" + mdas + "-" + pdas,adas + "-" + pdas + "-" + mdas,pdas + "-" + mdas + "-" + adas,pdas + "-" + adas + "-" + mdas};
                        // bool btran = false;
                        //foreach(string key in keys) {
                        //    var exists = data.Property(key);
                        //    if(exists != null) {
                        //        string per = exists.Value.ToString();
                        //        string deg = per.Split('|')[0];
                        //        //if(mHoro.planetsPos.ContainsKey(per.Split('|')[1]) && zvisits[per.Split('|')[1]] == false) {
                        //        if (mHoro.planetsPos.ContainsKey(per.Split('|')[1]))
                        //        {
                        //            string planetpos = "";
                        //            foreach(string plpos in mHoro.planetsPos[per.Split('|')[1]].Split('|')) {
                        //                string pl = plpos.Split(' ')[1];
                        //                if (pl != "Ur" && pl != "Pl" && pl != "me" && pl != "os" && pl != "Ne" && pl != "AC" && pl != "TRUE_NODE") {  //consider only true planets
                        //                    int st_sm = Convert.ToInt32(deg.Split('-')[0].Split('.')[2]);
                        //                    int st_em = Convert.ToInt32(deg.Split('-')[1].Split('.')[1]);
                        //                    double sl_b = Convert.ToInt32(deg.Split('-')[0].Split('.')[0])*60 + Convert.ToInt32(deg.Split('-')[0].Split('.')[1]) + ((st_sm > 0) ? st_sm/60 : 0);
                        //                    double sl_e = Convert.ToInt32(deg.Split('-')[1].Split('.')[0]) * 60 + Convert.ToInt32(deg.Split('-')[1].Split('.')[1]) + ((st_em > 0) ? st_em / 60 : 0);
                        //                    double ppos = (zstart[per.Split('|')[1]] + Convert.ToInt32(plpos.Split(' ')[0].Split('.')[0])) * 60 + ((plpos.Split(' ')[0].Split('.').Length > 1) ? ((plpos.Split(' ')[0].Split('.')[1].Trim() != string.Empty) ? Convert.ToInt32(plpos.Split(' ')[0].Split('.')[1]) : 0) : 0);
                        //                    if(ppos >= sl_b && ppos <= sl_e) {
                        //                        string star = calcStar(float.Parse(plpos.Split(' ')[0]), per.Split('|')[1]);
                        //                        planetpos += string.Format("{0}", plpos.Split(' ')[0]);//, star, per.Split('|')[1]);
                        //                        Transit transit = new Transit
                        //                        {
                        //                            date = string.Format("{0}-{1}-{2},{3}", today.Day, today.Month, today.Year, plpos),
                        //                            signL = key.Split('-')[0],
                        //                            starL = key.Split('-')[1],
                        //                            subL = key.Split('-')[2],
                        //                            star = star,
                        //                            sign = per.Split('|')[1]
                        //                        };
                        //                        trans.Add(transit);
                        //                        btran = true;
                        //                    }
                        //                }
                        //            }

                        //            zvisits[per.Split('|')[1]] = true;

                        //        }
                        //    }
                        //}
                        //if (!btran)
                        //{
                        //    Transit transit = new Transit
                        //    {
                        //        date = string.Format("{0}-{1}-{2}", today.Day, today.Month, today.Year),
                        //        signL = "",
                        //        starL = "",
                        //        subL = "",
                        //        star = "",
                        //        sign = ""
                        //    };
                        //    trans.Add(transit);

                        //}
                        today = today.AddDays(1);
                    }
                }
                return new JsonResult(trans.ToList());
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetDashTrans")]
        public ActionResult GetDashTrans(string mdas, string adas, string pdas, string pend)
        {
            try
            {
                mdas = mdas.ToLower();
                adas = adas.ToLower();
                pdas = pdas.ToLower();
                List<Transit2> trans = new List<Transit2>();
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                DateTime today = DateTime.Now;
                DateTime eday = new DateTime(Convert.ToInt32(pend.Split('-')[2]), Convert.ToInt32(pend.Split('-')[1]), Convert.ToInt32(pend.Split('-')[0]));
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string nJ = string.Format(@"{0}\sublordz.json", astClient);
                string pN = string.Format(@"{0}\planet_stars.json", astClient);
                string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string cnstls = string.Empty;
                string sssl = string.Empty;
                using (StreamReader r3 = new StreamReader(pN))
                {
                    string json = r3.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json);
                    var exists = data.Property(mdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(adas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(pdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    cnstls = cnstls.Remove(cnstls.Length - 1).Trim();
                }
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json2);
                    string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                    string ayan = string.Empty;
                    using (StreamReader r = new StreamReader(sF))
                    {
                        string jsa = r.ReadToEnd();
                        dynamic ayans = JsonConvert.DeserializeObject(jsa);
                        while (today <= eday)
                        {
                            ayan = ayans[today.Year.ToString()].ToString();
                            //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                            //calcStar(tday);
                            //JsonResult json1 = (JsonResult)Birthstar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute));
                            string latlng = "17.23|78.29";
                            string timezone = "India Standard Time";
                            Horoscope mHoro = new Horoscope();
                            uint u1 = Convert.ToUInt32(today.Day);
                            uint u2 = Convert.ToUInt32(today.Month);
                            int i3 = Convert.ToInt32(today.Year);
                            uint u4 = 5;//Convert.ToUInt32(today.Hour);
                            uint u5 = 0;//Convert.ToUInt32(today.Minute);
                            uint u6 = 0;
							double u7 = Convert.ToDouble(latlng.Split('|')[0]);
							double u8 = Convert.ToDouble(latlng.Split('|')[1]);
							ayan = Ayanmsa.Calc((int)u1, (int)u2, i3, AYANMSAS.KPNEW);
                            mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, timezone, true, ayan);
                            mHoro.calc_planets_pos(true, astClient);
                            int rpos = 0;
                            foreach (string sign in signs)
                            {
                                rpos++;
                                if (mHoro.planetsPos.ContainsKey(sign))
                                {
                                    var pls = mHoro.planetsPos[sign].Split('|');
                                    //var ePls = '';
                                    //var mnode = '';
                                    for (var k = 0; k < pls.Length; k++)
                                    {
                                        if (pls[k].Split(' ')[1] == "MEAN_NODE")
                                        {
                                            var kpos = rpos + 6;
                                            if (kpos > 12) kpos = (kpos - 12);
                                            //var mn = i + 11;
                                            //if (mn > 15) mn -= 15;
                                            if (mHoro.planetsPos.ContainsKey(signs[kpos - 1]))
                                            {
                                                var eP = mHoro.planetsPos[signs[kpos - 1]];
                                                mHoro.planetsPos[signs[kpos - 1]] = eP + '|' + pls[k].Split(' ')[0] + ' ' + "Ke";
                                            }
                                            else
                                            {
                                                mHoro.planetsPos[signs[kpos - 1]] = pls[k].Split(' ')[0] + ' ' + "Ke";
                                            }
                                            // plPos[sign] = ePls;
                                            mHoro.planetsPos[sign] = mHoro.planetsPos[sign].Replace("MEAN_NODE", "Ra");
                                        }
                                    }
                                }
                            }
                            sssl = getSSSL2(cnstls, mHoro);
                            if (sssl != string.Empty)
                            {
                                try
                                {
                                    Transit2 transit = new Transit2
                                    {
                                        date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today),//string.Format("{0}-{1}-{2},{3}", today.Day, today.Month, today.Year, key.Split('|')[1].Split('-')[2]),
                                        sssl = sssl
                                    };
                                    trans.Add(transit);
                                }
                                catch (Exception eX)
                                {
                                    Console.WriteLine(eX.Message);
                                }
                            }
                            else
                            {
                                Transit2 transit = new Transit2
                                {
                                    date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today),
                                    sssl = ""
                                };
                                trans.Add(transit);
                            }
                            today = today.AddDays(1);
                        }
                    }
                }
                return new JsonResult(trans.ToList());
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetDashTransEx")]
        public ActionResult GetDashTransEx(string mdas, string adas, string pdas, string pend, string latlng, string timezone, int ayanid)
        {
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                mdas = mdas.ToLower();
                adas = adas.ToLower();
                pdas = pdas.ToLower();
                List<Transit2> trans = new List<Transit2>();
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                DateTime today = DateTime.Now;
                DateTime eday = new DateTime(Convert.ToInt32(pend.Split('-')[2]), Convert.ToInt32(pend.Split('-')[1]), Convert.ToInt32(pend.Split('-')[0]));
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string nJ = string.Format(@"{0}\sublordz.json", astClient);
                string pN = string.Format(@"{0}\planet_stars.json", astClient);
                string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string cnstls = string.Empty;
                string sssl = string.Empty;
                using (StreamReader r3 = new StreamReader(pN))
                {
                    string json = r3.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json);
                    var exists = data.Property(mdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(adas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    exists = data.Property(pdas);
                    if (exists != null)
                    {
                        cnstls += string.Format("{0}|", exists.Value.ToString().Trim());
                    }
                    cnstls = cnstls.Remove(cnstls.Length - 1).Trim();
                }
                using (StreamReader r2 = new StreamReader(nJ))
                {
                    string json2 = r2.ReadToEnd();
                    var data = (JObject)JsonConvert.DeserializeObject(json2);
                    //string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                    //string ayan = string.Empty;
                    //using (StreamReader r = new StreamReader(sF))
                    //{
                    // string jsa = r.ReadToEnd();
                    //dynamic ayans = JsonConvert.DeserializeObject(jsa);
                    while (today <= eday)
                    {
                        //ayan = ayans[today.Year.ToString()].ToString();
                        //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                        //calcStar(tday);
                        //JsonResult json1 = (JsonResult)Birthstar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute));
                        //string latlng = "17.23|78.29";
                        //string timezone = "India Standard Time";
                        Horoscope mHoro = new Horoscope();
                        uint u1 = Convert.ToUInt32(today.Day);
                        uint u2 = Convert.ToUInt32(today.Month);
                        int i3 = Convert.ToInt32(today.Year);
                        uint u4 = 5;//Convert.ToUInt32(today.Hour);
                        uint u5 = 0;//Convert.ToUInt32(today.Minute);
                        uint u6 = 0;
						double u7 = Convert.ToDouble(latlng.Split('|')[0]);
						double u8 = Convert.ToDouble(latlng.Split('|')[1]);
						//ayan = Ayanmsa.Calc((int)u1, (int)u2, i3, AYANMSAS.KPNEW);
						string ayan = string.Empty;
                        if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                        {
                            TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                            TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                            double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                            ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                        }
                        mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                        mHoro.calc_houses();
                        mHoro.calc_planets_pos(true, astClient);
                        //mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, u9, u10, timezone, true, ayan);
                        //mHoro.calc_planets_pos(true);
                        int rpos = 0;
                        foreach (string sign in signs)
                        {
                            rpos++;
                            if (mHoro.planetsPos.ContainsKey(sign))
                            {
                                var pls = mHoro.planetsPos[sign].Split('|');
                                //var ePls = '';
                                //var mnode = '';
                                for (var k = 0; k < pls.Length; k++)
                                {
                                    if (pls[k].Split(' ')[1] == "MEAN_NODE")
                                    {
                                        var kpos = rpos + 6;
                                        if (kpos > 12) kpos = (kpos - 12);
                                        //var mn = i + 11;
                                        //if (mn > 15) mn -= 15;
                                        if (mHoro.planetsPos.ContainsKey(signs[kpos - 1]))
                                        {
                                            var eP = mHoro.planetsPos[signs[kpos - 1]];
                                            mHoro.planetsPos[signs[kpos - 1]] = eP + '|' + pls[k].Split(' ')[0] + ' ' + "Ke";
                                        }
                                        else
                                        {
                                            mHoro.planetsPos[signs[kpos - 1]] = pls[k].Split(' ')[0] + ' ' + "Ke";
                                        }
                                        // plPos[sign] = ePls;
                                        mHoro.planetsPos[sign] = mHoro.planetsPos[sign].Replace("MEAN_NODE", "Ra");
                                    }
                                }
                            }
                        }
                        sssl = getSSSL2(cnstls, mHoro);
                        if (sssl != string.Empty)
                        {
                            try
                            {
                                Transit2 transit = new Transit2
                                {
                                    date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today),//string.Format("{0}-{1}-{2},{3}", today.Day, today.Month, today.Year, key.Split('|')[1].Split('-')[2]),
                                    sssl = sssl
                                };
                                trans.Add(transit);
                            }
                            catch (Exception eX)
                            {
                                Console.WriteLine(eX.Message);
                            }
                        }
                        else
                        {
                            Transit2 transit = new Transit2
                            {
                                date = String.Format(ci, "{0:ddd MMM dd,yyyy}", today),
                                sssl = ""
                            };
                            trans.Add(transit);
                        }
                        today = today.AddDays(1);
                    }
                    //}
                }
                return new JsonResult(trans.ToList());
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        public string getSUBZ(string sign, double pos)
        {
            //double ppos = (pos.ToString().IndexOf('.') != -1) ? Convert.ToInt32(pos.ToString().Split('.')[0]) * 60 + Convert.ToInt32(pos.ToString().Split('.')[1]) : pos * 60;
            string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
            string nJ = string.Format(@"{0}\sublordz.json", astClient);
            using (StreamReader r = new StreamReader(nJ))
            {
                string json = r.ReadToEnd();
                Dictionary<string, string> items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                string subz = string.Empty;
                foreach (var item in items)
                {
                    if (item.Value.Split('|')[1] == sign)
                    {
                        string sd = item.Value.Split('|')[0].Split('-')[0];
                        string ed = item.Value.Split('|')[0].Split('-')[1];
                        //double sm = Convert.ToInt32(sd.Split('.')[0]) * 60 + Convert.ToInt32(sd.Split('.')[1]) + Convert.ToInt32(sd.Split('.')[2]) / 60;
                        //double em = Convert.ToInt32(ed.Split('.')[0]) * 60 + Convert.ToInt32(ed.Split('.')[1]) + Convert.ToInt32(ed.Split('.')[2]) / 60;
                        double sm = dmsToDec(Convert.ToInt32(sd.Split('.')[0]), Convert.ToInt32(sd.Split('.')[1]), Convert.ToInt32(sd.Split('.')[2]));
                        double em = dmsToDec(Convert.ToInt32(ed.Split('.')[0]), Convert.ToInt32(ed.Split('.')[1]), Convert.ToInt32(ed.Split('.')[2]));
                        if (pos >= sm && pos <= em) subz += item.Key + ",";
                    }
                }
                return (subz.Length > 0) ? subz.Remove(subz.Length - 1).Trim() : subz;
            }
        }

        public string getSSSL2(string naks, Horoscope mH)
        {
            string sssl = string.Empty;
            Dictionary<string, int> zstart = new Dictionary<string, int>();
            zstart["ar"] = 0;
            zstart["ta"] = 30;
            zstart["ge"] = 60;
            zstart["cn"] = 90;
            zstart["le"] = 120;
            zstart["vi"] = 150;
            zstart["li"] = 180;
            zstart["sc"] = 210;
            zstart["sa"] = 240;
            zstart["cp"] = 270;
            zstart["aq"] = 300;
            zstart["pi"] = 330;
            foreach (var ppos in mH.planetsPos)
            {
                foreach (string pl in ppos.Value.Split('|'))
                {
                    string pn = pl.Split(' ')[1];
                    if (pn != "Ur" && pn != "Pl" && pn != "me" && pn != "os" && pn != "Ne" && pn != "AC" && pn != "TRUE_NODE")
                    {  //consider only true planets
                        string[] pds = pl.Split(' ')[0].Split('.');
                        string star = calcStar(dmsToDec(Convert.ToInt32(pds[0]), Convert.ToInt32(pds[1]), Convert.ToInt32(pds[2])), ppos.Key);
                        if (star.Trim() == string.Empty) continue;
                        if (naks.Contains(star))
                        {
                            double zd = dmsToDec(Convert.ToInt32(zstart[ppos.Key]), 0, 0);
                            string[] p = pl.Split(' ')[0].TrimEnd('.').Split('.');
                            double plpos = zd + dmsToDec(Convert.ToInt32(p[0]), Convert.ToInt32(p[1]), Convert.ToInt32(p[2]));
                            sssl += string.Format("{0}|{1}-{2}-{3},", getSUBZ(ppos.Key, plpos), star, ppos.Key, pl);
                        }
                    }
                }
            }
            return (sssl.Length > 0) ? sssl.Remove(sssl.Length - 1).Trim() : sssl;
        }
        [HttpGet("GetYogas")]
        public ActionResult GetYogas(string dob, string tob, string latlng, string timezone, string lang)
        {
            //DBLog(string.Format("GetYogas-{0}", dob));
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            Dictionary<string, string> dctConjYogs = new Dictionary<string, string>();
            Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string yogf = string.Empty, astf = string.Empty;
                switch (lang)
                {
                    case "ta":
                        yogf = string.Format(@"{0}\ta-yogs.json", astClient);
                        astf = string.Format(@"{0}\ta-dct.json", astClient);
                        break;
                    case "en":
                        yogf = string.Format(@"{0}\en-yogs.json", astClient);
                        astf = string.Format(@"{0}\en-dct.json", astClient);
                        break;
                    case "te":
                        yogf = string.Format(@"{0}\te-yogs.json", astClient);
                        astf = string.Format(@"{0}\te-dct.json", astClient);
                        break;
                    case "hi":
                        yogf = string.Format(@"{0}\hi-yogs.json", astClient);
                        astf = string.Format(@"{0}\hi-dct.json", astClient);
                        break;
                    default:
                        yogf = string.Format(@"{0}\en-yogs.json", astClient);
                        astf = string.Format(@"{0}\en-dct.json", astClient);
                        break;
                }
                string yogs = string.Empty, adct = string.Empty;
                using (StreamReader rdra = new StreamReader(astf, Encoding.UTF8))
                using (StreamReader rdr = new StreamReader(yogf, Encoding.UTF8))
                {
                    yogs = rdr.ReadToEnd();
                    adct = rdra.ReadToEnd();
                }
                Dictionary<string, string> dctAst;
                Dictionary<string, string> dctSrc;
                try
                {
                    dctAst = JsonConvert.DeserializeObject<Dictionary<string, string>>(adct);
                    dctSrc = JsonConvert.DeserializeObject<Dictionary<string, string>>(yogs);
                }
                catch (JsonException eX)
                {
                    var st = new StackTrace(eX, true);
                    var frame = st.GetFrame(st.FrameCount - 1);
                    var line = frame.GetFileLineNumber();
                    dctYogs.Add("eX.Message", string.Format("{0} {1}", eX.Message, line.ToString()));
                    return new JsonResult(dctYogs);
                }
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, tz, false, string.Empty);
                mHoro.calc_planets_pos(false, astClient);
                Dictionary<string, string> plpos = mHoro.planetsPos;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plpos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plpos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plpos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plpos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                bool ves = false, vos = false;
                string ves_pl = string.Empty, vos_pl = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plpos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plpos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                            else
                            {
                                if (pl.Split(' ')[1] != "Mo" && pl.Split(' ')[1] != "MEAN_NODE" && pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                                {  //consider only true  
                                    switch (r2)
                                    {
                                        case 2: ////veshi yoga
                                            ves = true;
                                            ves_pl += pl.Split(' ')[1] + ",";
                                            break;
                                        case 12: //voshi yoga
                                            vos = true;
                                            vos_pl += pl.Split(' ')[1] + ",";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                if (ves && vos)
                {//ubhachari yoga
                    bool ben = false, mel = false;
                    foreach (string pl in ves_pl.Split(','))
                    {
                        switch (pl)
                        {
                            case "Mo":
                            case "Me":
                            case "Ve":
                            case "Ju":
                                ben = true;
                                break;
                            case "Su":
                            case "Ma":
                            case "Sa":
                            case "Ra":
                            case "Ke":
                                mel = true;
                                break;
                            default:
                                break;
                        }
                    }
                    foreach (string pl in vos_pl.Split(','))
                    {
                        switch (pl)
                        {
                            case "Mo":
                            case "Me":
                            case "Ve":
                            case "Ju":
                                ben = true;
                                break;
                            case "Su":
                            case "Ma":
                            case "Sa":
                            case "Ra":
                            case "Ke":
                                mel = true;
                                break;
                            default:
                                break;
                        }
                    }
                    try
                    {
                        string pls_ves = string.Empty;
                        foreach (string pl in ves_pl.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_ves += dctAst[dctPlNames[pl]] + ",";
                        }
                        string pls_vos = string.Empty;
                        foreach (string pl in vos_pl.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_vos += dctAst[dctPlNames[pl]] + ",";
                        }

                        dctYogs["UBAHAYACHARI-YOGA"] = string.Format("{0} {1}", dctSrc["UBAHAYACHARI-YOGA"].Replace("[1]", pls_ves.TrimEnd(',')).Replace("[2]", pls_vos.TrimEnd(',')).Replace("[3]", dctAst["UBAHAYACHARI-YOGA"]), (ben == true) ? dctSrc["UBAHAYACHARI-YOGA,BEN"] : dctSrc["UBAHAYACHARI-YOGA,MEL"]);
                    }
                    catch
                    {
                        dctYogs["UBAHAYACHARI-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (ves)
                {
                    bool ben = false, mel = false;
                    foreach (string pl in ves_pl.Split(','))
                    {
                        switch (pl)
                        {
                            case "Mo":
                            case "Me":
                            case "Ve":
                            case "Ju":
                                ben = true;
                                break;
                            case "Su":
                            case "Ma":
                            case "Sa":
                            case "Ra":
                            case "Ke":
                                mel = true;
                                break;
                            default:
                                break;
                        }
                    }
                    try
                    {
                        string pls_ves = string.Empty;
                        foreach (string pl in ves_pl.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_ves += dctAst[dctPlNames[pl]] + ",";
                        }
                        dctYogs["VESHI-YOGA"] = string.Format("{0} {1}", dctSrc["VESHI-YOGA"].Replace("[1]", pls_ves.TrimEnd(',')).Replace("[2]", dctAst["VESHI-YOGA"]), (mel == true) ? dctSrc["VESHI-YOGA,MEL"] : "");
                    }
                    catch
                    {
                        dctYogs["VESHI-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (vos)
                {
                    bool ben = false, mel = false;
                    foreach (string pl in vos_pl.Split(','))
                    {
                        switch (pl)
                        {
                            case "Mo":
                            case "Me":
                            case "Ve":
                            case "Ju":
                                ben = true;
                                break;
                            case "Su":
                            case "Ma":
                            case "Sa":
                            case "Ra":
                            case "Ke":
                                mel = true;
                                break;
                            default:
                                break;
                        }
                    }
                    try
                    {
                        string pls_vos = string.Empty;
                        foreach (string pl in vos_pl.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_vos += dctAst[dctPlNames[pl]] + ",";
                        }
                        dctYogs["VOSHI-YOGA"] = string.Format("{0} {1}", dctSrc["VOSHI-YOGA"].Replace("[1]", pls_vos.TrimEnd(',')).Replace("[2]", dctAst["VOSHI-YOGA"]), (mel == true) ? dctSrc["VOSHI-YOGA,MEL"] : "");
                    }
                    catch
                    {
                        dctYogs["VOSHI-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                int nsank = 0;
                bool bsnp = false;
                bool banp = false;
                bool ben_8 = false, ben_7 = false, ben_6 = false, mel_8 = false, mel_6 = false;
                string snp_pls = string.Empty;
                string anp_pls = string.Empty;
                bool bcan_kem = false;
                bool malefic_in_ken = false;
                bool benefic_in_ken = false;
                bool one7shak = true;
                bool four10pak = true;
                int one_hou_pl = 0;
                int four_hou_pl = 0;
                int svn_hou_pl = 0;
                int ten_hou_pl = 0;

                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (plpos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in plpos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                                if (pHou.code != "MEAN_NODE" && pHou.code != "Ke" && pHou.code != "Ur" && pHou.code != "Pl" && pHou.code != "me" && pHou.code != "os" && pHou.code != "Ne" && pHou.code != "AC" && pHou.code != "TRUE_NODE")
                                {  //consider only true  
                                    bpl = true;
                                    pkey += pHou.code + "-";
                                    if (pHou.hno != 1 || pHou.hno != 7) one7shak = false;
                                    if (pHou.hno != 4 || pHou.hno != 10) four10pak = false;
                                    switch (pHou.hno)
                                    {
                                        case 1:
                                            one_hou_pl++;
                                            break;
                                        case 4:
                                            four_hou_pl++;
                                            break;
                                        case 7:
                                            svn_hou_pl++;
                                            break;
                                        case 10:
                                            ten_hou_pl++;
                                            break;
                                        default:
                                            break;
                                    }
                                    if (pHou.lordship == "KEN" || pHou.lordship == "BOTH") //panchmaha purusha yoga applies only if in kendra
                                    {
                                        if (pHou.code == "Sa" || pHou.code == "Me" || pHou.code == "Su")
                                        {
                                            malefic_in_ken = true;
                                        }
                                        else if (pHou.code == "Ju" || pHou.code == "Ma" || pHou.code == "Ve" || pHou.code == "Mo")
                                        {
                                            benefic_in_ken = true;
                                        }
                                    }
                                    if (pHou.code != "Su" && pHou.code != "Mo")
                                    {
                                        int mhno = Convert.ToInt32(ra.Split('|')[4]);
                                        switch (mhno)
                                        {
                                            case 2:
                                                bsnp = true;
                                                snp_pls += pHou.name + ",";
                                                break;
                                            case 12:
                                                banp = true;
                                                anp_pls += pHou.name + ",";
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    if (pHou.code == "Su" || pHou.code == "Ma" || pHou.code == "Sa")
                                    { //malefics
                                        if (pHou.hno == 4 || pHou.mhno == 4)
                                        { //4th house from lagna or moon occupied by a malefic
                                        }
                                        else if (pHou.hno == 6)
                                        {
                                            mel_6 = true;
                                        }
                                        else if (pHou.hno == 8)
                                        {
                                            mel_8 = true;
                                        }
                                    }
                                    if (pHou.mhno == 1 || pHou.mhno == 4 || pHou.mhno == 7 || pHou.mhno == 10)
                                    { //precense of planets in kendra house from moon which cancels kemadrupha yoga
                                        bcan_kem = true;
                                    }
                                    if (pHou.code == "Me" || pHou.code == "Ve" || pHou.code == "Ju")
                                    {
                                        if (pHou.hno == 7) ben_7 = true;
                                        else if (pHou.hno == 8) ben_8 = true;

                                    }
                                }
                            }
                        }
                        if (bpl) nsank++;
                    }
                    if (pkey.Split('-').Count() - 1 > 1) //yoga2 formed through conjunction
                    {
                        pkey = pkey.Remove(pkey.Length - 1).Trim();
                        pkey = sortPls(pkey);
                        string pckey = string.Format("{0}|{1}", pkey, ra.Split('|')[3]);
                        // dctConjYogs.Adpckey, "");
                        string yog = (dctSrc.ContainsKey(pckey) == true) ? dctSrc[pckey] : (dctSrc.ContainsKey(pkey) == true) ? dctSrc[pkey] : "";
                        if (yog != string.Empty)
                        {
                            string plk = string.Empty;
                            foreach (string key in pkey.Split('-'))
                                plk += dctPlNames[key] + "-";
                            plk = plk.Remove(plk.Length - 1).Trim();
                            dctYogs[plk] = yog;
                        }
                    }
                }
                foreach (var plh in dctPlHou)
                {
                    PlanetHouse pHou = plh.Value;
                    switch (pHou.code)
                    {
                        case "Ma":
                            if (pHou.sign == "cp" || pHou.sign == "ar" || pHou.sign == "sc")
                            {
                                //mars is exalted or own sign
                                if (asc_h.Split('|')[1] != "D")
                                { //ruchaka yoga
                                    bool bsun = false, bmoon = false;
                                    if (dctPlHou["Su"].hno == pHou.hno)
                                    {//sun is combined
                                        bsun = true;
                                    }
                                    if (dctPlHou["Mo"].hno == pHou.hno)
                                    {//moon is combined
                                        bmoon = true;
                                    }
                                    try
                                    {
                                        dctYogs["RUCHAKA-YOGA"] = string.Format("{0} {1}", dctSrc["RUCHAKA-YOGA"].Replace("[1]", (pHou.sign == "cp") ? dctAst["exalted"] : dctAst["in own sign"]).Replace("[2]", (asc_h.Split('|')[1] == "F") ? dctAst["fixed"] : dctAst["movable"]).Replace("[3]", dctAst["RUCHAKA-YOGA"]).Replace("[4]", dctAst["PANCH-MAHAPURUSHA-YOGAS"]), (bsun) ? dctSrc["RUCHAKA-YOGA,SU"] : (bmoon) ? dctSrc["RUCHAKA-YOGA,MO"] : "");
                                    }
                                    catch
                                    {
                                        dctYogs["RUCHAKA-YOGA"] = "Internal error. Please report to help desk.";
                                    }
                                }
                            }
                            break;
                        case "Me":
                            if (pHou.sign == "vi" || pHou.sign == "ta")
                            {  //mercury exalted or own sign
                                bool bsun = false, bmoon = false;
                                if (dctPlHou["Su"].hno == pHou.hno)
                                {//sun is combined
                                    bsun = true;
                                }
                                if (dctPlHou["Mo"].hno == pHou.hno)
                                {//moon is combined
                                    bmoon = true;
                                }
                                if (asc_h.Split('|')[1] == "D")
                                { //bhadra yoga
                                    try
                                    {
                                        dctYogs["BHADRA-YOGA"] = string.Format("{0} {1}", dctSrc["BHADRA-YOGA"].Replace("[1]", (pHou.sign == "vi") ? dctAst["exalted"] : dctAst["in own sign"]).Replace("[2]", (asc_h.Split('|')[1] == "F") ? dctAst["fixed"] : dctAst["movable"]).Replace("[3]", dctAst["BHADRA-YOGA"]).Replace("[4]", dctAst["PANCH-MAHAPURUSHA-YOGAS"]), (bsun) ? dctSrc["BHADRA-YOGA,SU"] : (bmoon) ? dctSrc["BHADRA-YOGA,MO"] : "");
                                    }
                                    catch
                                    {
                                        dctYogs["BHADRA-YOGA"] = "Internal error. Please report to help desk.";
                                    }

                                }
                            }
                            break;
                        case "Ju":
                            if (pHou.sign == "cn" || pHou.sign == "sa" || pHou.sign == "pi")
                            { //jupiter exalted or own sign
                                bool bsun = false, bmoon = false;
                                if (dctPlHou["Su"].hno == pHou.hno)
                                {//sun is combined
                                    bsun = true;
                                }
                                if (dctPlHou["Mo"].hno == pHou.hno)
                                {//moon is combined
                                    bmoon = true;
                                }
                                if (asc_h.Split('|')[1] == "D" || asc_h.Split('|')[1] == "M")
                                { //hamsa yoga
                                    try
                                    {
                                        dctYogs["HAMSA-YOGA"] = string.Format("{0} {1}", dctSrc["HAMSA-YOGA"].Replace("[1]", (pHou.sign == "cn") ? dctAst["exalted"] : dctAst["in own sign"]).Replace("[2]", (asc_h.Split('|')[1] == "F") ? dctAst["fixed"] : dctAst["movable"]).Replace("[3]", dctAst["HAMSA-YOGA"]).Replace("[4]", dctAst["PANCH-MAHAPURUSHA-YOGAS"]), (bsun) ? dctSrc["HAMSA-YOGA,SU"] : (bmoon) ? dctSrc["HAMSA-YOGA,MO"] : "");
                                    }
                                    catch
                                    {
                                        dctYogs["HAMSA-YOGA"] = "Internal error. Please report to help desk.";
                                    }
                                }
                            }
                            break;
                        case "Ve":
                            if (pHou.sign == "pi" || pHou.sign == "ta" || pHou.sign == "li")
                            { //venus exalted or own sign
                                bool bsun = false, bmoon = false;
                                if (dctPlHou["Su"].hno == pHou.hno)
                                {//sun is combined
                                    bsun = true;
                                }
                                if (dctPlHou["Mo"].hno == pHou.hno)
                                {//moon is combined
                                    bmoon = true;
                                }
                                //malva yoga
                                try
                                {
                                    dctYogs["MALVA-YOGA"] = string.Format("{0} {1}", dctSrc["MALVA-YOGA"].Replace("[1]", (pHou.sign == "pi") ? dctAst["exalted"] : dctAst["in own sign"]).Replace("[2]", (asc_h.Split('|')[1] == "F") ? dctAst["fixed"] : dctAst["movable"]).Replace("[3]", dctAst["MALVA-YOGA"]).Replace("[4]", dctAst["PANCH-MAHAPURUSHA-YOGAS"]), (bsun) ? dctSrc["MALVA-YOGA,SU"] : (bmoon) ? dctSrc["MALVA-YOGA,MO"] : "");
                                }
                                catch
                                {
                                    dctYogs["MALVA-YOGA"] = "Internal error. Please report to help desk.";
                                }

                            }
                            break;
                        case "Sa":
                            if (pHou.sign == "li" || pHou.sign == "cp" || pHou.sign == "aq")
                            { //saturn exalted or own sign
                                bool bsun = false, bmoon = false;
                                if (dctPlHou["Su"].hno == pHou.hno)
                                {//sun is combined
                                    bsun = true;
                                }
                                if (dctPlHou["Mo"].hno == pHou.hno)
                                {//moon is combined
                                    bmoon = true;
                                }
                                if (asc_h.Split('|')[1] != "D")
                                { //shasha yoga
                                    try
                                    {
                                        dctYogs["SHASHA-YOGA"] = string.Format("{0}", dctSrc["SHASHA-YOGA"].Replace("[1]", (pHou.sign == "li") ? dctAst["exalted"] : dctAst["in own sign"]).Replace("[2]", (asc_h.Split('|')[1] == "F") ? dctAst["fixed"] : dctAst["movable"]).Replace("[3]", dctAst["SHASHA-YOGA"]).Replace("[4]", dctAst["PANCH-MAHAPURUSHA-YOGAS"]), (bsun) ? dctSrc["SHASHA-YOGA,SU"] : (bmoon) ? dctSrc["SHASHA-YOGA,MO"] : "");
                                    }
                                    catch (Exception eX)
                                    {
                                        dctYogs["SHASHA-YOGA"] = eX.Message; //"Internal error. Please report to help desk.";
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                //GAJkesari yoga
                if (((dctPlHou["Mo"].sign != "sc") && (dctPlHou["Ju"].sign != "cp")) && (dctPlHou["Mo"].lordship == "KEN" || dctPlHou["Mo"].lordship == "BOTH") && (dctPlHou["Ju"].lordship == "KEN" || dctPlHou["Ju"].lordship == "BOTH"))
                { //moon & jupiter in kendra
                    int kno = (dctPlHou["Ju"].mhno - dctPlHou["Mo"].mhno) + 1;
                    switch (kno)
                    {
                        case 1:
                            try
                            {
                                dctYogs["GAJKESARI-YOGA"] = string.Format("{0} {1}", dctSrc["GAJKESARI-YOGA"].Replace("[1]", kno.ToString()).Replace("[2]", dctAst["GAJKESARI-YOGA"]), dctSrc["GAJKESARI-YOGA|1"]);
                            }
                            catch
                            {
                                dctYogs["GAJKESARI-YOGA"] = "Internal error. Please report to help desk.";
                            }
                            break;
                        case 4:
                        case 7:
                        case 10:
                            try
                            {
                                dctYogs["GAJKESARI-YOGA,JU"] = string.Format("{0} {1}", dctSrc["GAJKESARI-YOGA,JU"].Replace("[1]", kno.ToString()).Replace("[2]", dctAst["GAJKESARI-YOGA"]), dctSrc["GAJKESARI-YOGA,JU|" + kno.ToString()]);
                            }
                            catch
                            {
                                dctYogs["GAJKESARI-YOGA,JU"] = "Internal error. Please report to help desk.";
                            }
                            break;
                        default:
                            break;
                    }
                }
                //chandra yogas
                int ncad = 0;
                string adh_p = string.Empty, ad_h = string.Empty;
                switch (dctPlHou["Me"].mhno)
                {
                    case 6:
                    case 7:
                    case 8:
                        adh_p += "Me" + ",";
                        ad_h += dctPlHou["Me"].mhno.ToString() + ",";
                        ncad++;
                        break;
                    default:
                        break;
                }
                switch (dctPlHou["Ju"].mhno)
                {
                    case 6:
                    case 7:
                    case 8:
                        adh_p += "Ju" + ",";
                        ad_h += dctPlHou["Ju"].mhno.ToString() + ",";
                        ncad++;
                        break;
                    default:
                        break;
                }
                switch (dctPlHou["Ve"].mhno)
                {
                    case 6:
                    case 7:
                    case 8:
                        adh_p += "Ve" + ",";
                        ad_h += dctPlHou["Ve"].mhno.ToString() + ",";
                        ncad++;
                        break;
                    default:
                        break;
                }
                if (ncad > 1)
                { //minmum for adhi yoga
                    try
                    {
                        string pls_adh = string.Empty;
                        foreach (string pl in adh_p.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_adh += dctAst[dctPlNames[pl]] + ",";
                        }
                        dctYogs["ADHI-YOGA"] = string.Format("{0}", dctSrc["ADHI-YOGA"].Replace("[1]", pls_adh.TrimEnd(',')).Replace("[2]", ad_h.TrimEnd(',')).Replace("[3]", dctAst["ADHI-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["ADHI-YOGA"] = "Internal error. Please report to help desk.";
                    }

                }
                if (bsnp && banp)
                {  //durudhara yoga
                    try
                    {
                        string pls_snp = string.Empty;
                        foreach (string pl in snp_pls.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_snp += dctAst[dctPlNames[pl]] + ",";
                        }
                        string pls_anp = string.Empty;
                        foreach (string pl in anp_pls.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_anp += dctAst[dctPlNames[pl]] + ",";
                        }
                        dctYogs["DURUDHARA-YOGA"] = string.Format("{0}", dctSrc["DURUDHARA-YOGA"].Replace("[1]", (pls_snp + pls_anp).TrimEnd(',')).Replace("[2]", anp_pls).Replace("[3]", dctAst["DURUDHARA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["DURUDHARA-YOGA"] = "Internal error. Please report to help desk.";
                    }

                }
                else if (bsnp)
                { //sunapa yoga
                    try
                    {
                        string pls_snp = string.Empty;
                        foreach (string pl in snp_pls.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_snp += dctAst[pl] + ",";
                        }
                        dctYogs["SUNAPHA-YOGA"] = string.Format("{0}", dctSrc["SUNAPHA-YOGA"].Replace("[1]", pls_snp.TrimEnd(',')).Replace("[2]", dctAst["SUNAPHA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["SUNAPHA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (banp)
                { //anapa yoga
                    string pls_anp = string.Empty;
                    try
                    {
                        foreach (string pl in anp_pls.Split(','))
                        {
                            if (pl.Trim() != string.Empty)
                                pls_anp += dctAst[pl] + ",";
                        }
                        dctYogs["ANAPHA-YOGA"] = string.Format("{0}", dctSrc["ANAPHA-YOGA"].Replace("[1]", pls_anp.TrimEnd(',')).Replace("[2]", dctAst["ANAPHA-YOGA"]));
                    }
                    catch (Exception eX)
                    {
                        dctYogs["ANAPHA-YOGA"] = string.Format("{0} {1}", eX.Message, pls_anp);//"Internal error. Please report to help desk.";
                    }
                }
                else
                { //kemadruma yoga
                }
                //moon is in rahu ketu axis
                bool bmon_in_rk = false;
                if (dctPlHou["MEAN_NODE"].hno + 6 > 12)
                {
                    if (dctPlHou["Mo"].hno > dctPlHou["MEAN_NODE"].hno)
                    {
                        bmon_in_rk = true;
                    }
                    else if (dctPlHou["Mo"].hno < 12 - (dctPlHou["MEAN_NODE"].hno + 6))
                    {
                        bmon_in_rk = true;
                    }
                    else if (dctPlHou["Mo"].hno == 12 - (dctPlHou["MEAN_NODE"].hno + 6))
                    {
                        if (dctPlHou["MEAN_NODE"].pos > dctPlHou["Mo"].pos)
                        {
                            bmon_in_rk = true;
                        }
                    }
                    else if (dctPlHou["Mo"].hno == dctPlHou["MEAN_NODE"].hno)
                    {
                        if (dctPlHou["MEAN_NODE"].pos < dctPlHou["Mo"].pos)
                        {
                            bmon_in_rk = true;
                        }
                    }
                }
                else if (dctPlHou["Mo"].hno < (dctPlHou["MEAN_NODE"].hno + 6))
                {
                    bmon_in_rk = true;
                }
                else if (dctPlHou["Mo"].hno == dctPlHou["MEAN_NODE"].hno)
                {
                    if (dctPlHou["MEAN_NODE"].pos < dctPlHou["Mo"].pos)
                    {
                        bmon_in_rk = true;
                    }
                }
                else if (dctPlHou["Mo"].hno == (dctPlHou["MEAN_NODE"].hno + 6))
                {
                    if (dctPlHou["MEAN_NODE"].pos > dctPlHou["Mo"].pos)
                    {
                        bmon_in_rk = true;
                    }
                }
                bool bmon_inmic = false;
                string inmic_asp = string.Empty;
                if (dctPlHou["Mo"].sign == "li" && (dctPlHou["Mo"].houselord == "Me" || dctPlHou["Mo"].houselord == "Ve" || dctPlHou["Mo"].houselord == "Sa"))
                { //moon is in varga of inimical planet
                    if ((dctPlHou["Mo"].hno == dctPlHou["Me"].hno) || (dctPlHou["Mo"].hno == dctPlHou["Ve"].hno) || (dctPlHou["Mo"].hno == dctPlHou["Sa"].hno))
                    { //moon aspected by inimical planet
                        bmon_inmic = true;
                        if (dctPlHou["Mo"].hno == dctPlHou["Me"].hno)
                        {
                            inmic_asp += string.Format("{0},", dctAst["Mercury"]);
                        }
                        if (dctPlHou["Mo"].hno == dctPlHou["Ve"].hno)
                        {
                            inmic_asp += string.Format("{0},", dctAst["Venus"]);
                        }
                        if (dctPlHou["Mo"].hno == dctPlHou["Sa"].hno)
                        {
                            inmic_asp += string.Format("{0},", dctAst["Saturn"]);
                        }
                    }
                    int svn = (dctPlHou["Me"].hno + 6) > 12 ? (dctPlHou["Me"].hno + 6) - 12 : (dctPlHou["Me"].hno + 6);
                    if (dctPlHou["Mo"].hno == svn)
                    {//7th aspect from mercury
                        bmon_inmic = true;
                        inmic_asp += string.Format("{0},", dctAst["Mercury"]);
                    }
                    svn = (dctPlHou["Me"].hno + 6) > 12 ? (dctPlHou["Ve"].hno + 6) - 12 : (dctPlHou["Ve"].hno + 6);
                    if (dctPlHou["Mo"].hno == svn)
                    {//7th aspect from venus
                        bmon_inmic = true;
                        inmic_asp += string.Format("{0},", dctAst["Venus"]);
                    }
                    svn = (dctPlHou["Sa"].hno + 6) > 12 ? (dctPlHou["Sa"].hno + 6) - 12 : (dctPlHou["Sa"].hno + 6);
                    if (dctPlHou["Mo"].hno == svn)
                    {//7th aspect from saturn
                        bmon_inmic = true;
                        inmic_asp += string.Format("{0},", dctAst["Saturn"]);
                    }
                    int sat_asp = (dctPlHou["Sa"].hno + 2) > 12 ? (dctPlHou["Sa"].hno + 2) - 12 : (dctPlHou["Sa"].hno + 2);
                    if (dctPlHou["Mo"].hno == sat_asp)
                    {//3rd aspect from saturn
                        bmon_inmic = true;
                        inmic_asp += string.Format("{0},", dctAst["Saturn"]);
                    }
                    sat_asp = (dctPlHou["Sa"].hno + 9) > 12 ? (dctPlHou["Sa"].hno + 9) - 12 : (dctPlHou["Sa"].hno + 9);
                    if (dctPlHou["Mo"].hno == sat_asp)
                    {//10th aspect from saturn
                        bmon_inmic = true;
                        inmic_asp += string.Format("{0},", dctAst["Saturn"]);
                    }
                }
                //cancellation of kemadruma yoga
                if (dctPlHou["Mo"].lordship == "KEN" || dctPlHou["Mo"].lordship == "BOTH")
                {//moon in kendra
                    if (dctPlHou["Ju"].hno == dctPlHou["Mo"].hno)
                    {
                        bcan_kem = true;
                    }
                    else
                    {
                        int ju_asp = (dctPlHou["Ju"].hno + 6) > 12 ? (dctPlHou["Ju"].hno + 6) - 12 : (dctPlHou["Ju"].hno + 6);
                        if (dctPlHou["Mo"].hno == ju_asp)
                        {//7th aspect from jupiter
                            bcan_kem = true;
                        }
                        ju_asp = (dctPlHou["Ju"].hno + 4) > 12 ? (dctPlHou["Ju"].hno + 4) - 12 : (dctPlHou["Ju"].hno + 4);
                        if (dctPlHou["Mo"].hno == ju_asp)
                        {//5th aspect from jupiter
                            bcan_kem = true;
                        }
                        ju_asp = (dctPlHou["Ju"].hno + 8) > 12 ? (dctPlHou["Ju"].hno + 8) - 12 : (dctPlHou["Ju"].hno + 8);
                        if (dctPlHou["Mo"].hno == ju_asp)
                        {//9th aspect from jupiter
                            bcan_kem = true;
                        }
                    }

                }
                if (dctPlHou["Ve"].lordship == "KEN" || dctPlHou["Ve"].lordship == "BOTH")
                {//venus in kendra
                    if (dctPlHou["Ju"].hno == dctPlHou["Ve"].hno)
                    {
                        bcan_kem = true;
                    }
                    else
                    {
                        int ju_asp = (dctPlHou["Ju"].hno + 6) > 12 ? (dctPlHou["Ju"].hno + 6) - 12 : (dctPlHou["Ju"].hno + 6);
                        if (dctPlHou["Ve"].hno == ju_asp)
                        {//7th aspect from jupiter
                            bcan_kem = true;
                        }
                        ju_asp = (dctPlHou["Ju"].hno + 4) > 12 ? (dctPlHou["Ju"].hno + 4) - 12 : (dctPlHou["Ju"].hno + 4);
                        if (dctPlHou["Ve"].hno == ju_asp)
                        {//5th aspect from jupiter
                            bcan_kem = true;
                        }
                        ju_asp = (dctPlHou["Ju"].hno + 8) > 12 ? (dctPlHou["Ju"].hno + 8) - 12 : (dctPlHou["Ju"].hno + 8);
                        if (dctPlHou["Ve"].hno == ju_asp)
                        {//9th aspect from jupiter
                            bcan_kem = true;
                        }
                    }
                }
                if (dctPlHou["Mo"].sign == "ta" && dctPlHou["Mo"].hno == 10)
                { //exalted moon in 10th house
                    int asp = (dctPlHou["Ju"].hno + 6) > 12 ? (dctPlHou["Ju"].hno + 6) - 12 : (dctPlHou["Ju"].hno + 6);
                    if (dctPlHou["Mo"].hno == asp)
                    {//7th aspect from jupiter
                        bcan_kem = true;
                    }
                    asp = (dctPlHou["Ju"].hno + 4) > 12 ? (dctPlHou["Ju"].hno + 4) - 12 : (dctPlHou["Ju"].hno + 4);
                    if (dctPlHou["Mo"].hno == asp)
                    {//5th aspect from jupiter
                        bcan_kem = true;
                    }
                    asp = (dctPlHou["Ju"].hno + 8) > 12 ? (dctPlHou["Ju"].hno + 8) - 12 : (dctPlHou["Ju"].hno + 8);
                    if (dctPlHou["Mo"].hno == asp)
                    {//9th aspect from jupiter
                        bcan_kem = true;
                    }
                    if (dctPlHou["Mo"].hno == dctPlHou["Ju"].hno)
                    {//1st aspect from jupiter
                    }
                    asp = (dctPlHou["Me"].hno + 6) > 12 ? (dctPlHou["Me"].hno + 6) - 12 : (dctPlHou["Me"].hno + 6);
                    if (dctPlHou["Mo"].hno == asp)
                    {//7th aspect from mercury
                        bcan_kem = true;
                    }
                    if (dctPlHou["Mo"].hno == dctPlHou["Me"].hno)
                    {//1st aspect from mercury
                        bcan_kem = true;
                    }
                    asp = (dctPlHou["Ve"].hno + 6) > 12 ? (dctPlHou["Ve"].hno + 6) - 12 : (dctPlHou["Ve"].hno + 6);
                    if (dctPlHou["Mo"].hno == asp)
                    {//7th aspect from venus
                        bcan_kem = true;
                    }
                    if (dctPlHou["Mo"].hno == dctPlHou["Ve"].hno)
                    {//1st aspect from venus
                        bcan_kem = true;
                    }
                }
                if (dctPlHou["Ma"].sign == "li" && dctPlHou["Ju"].sign == "li" && dctPlHou["Su"].sign == "vi" && dctPlHou["Mo"].sign == "ta")
                {//mars & jupiter in libra, sun in virgo & moon in taurus
                    bcan_kem = true;
                }
                //vasuman yoga
                //bool mer_upa = false, ven_upa = false, jup_upa = false;
                int nupa = 0;
                string upa_pl = string.Empty;
                if (dctPlHou["Me"].mhno == 3 || dctPlHou["Me"].mhno == 6 || dctPlHou["Me"].mhno == 10 || dctPlHou["Me"].mhno == 11)
                {//mercury occupy upachaya house from moon
                 //mer_upa = true;
                    nupa++;
                    upa_pl += "Mercury,";
                }
                if (dctPlHou["Ve"].mhno == 3 || dctPlHou["Ve"].mhno == 6 || dctPlHou["Ve"].mhno == 10 || dctPlHou["Ve"].mhno == 11)
                {//venus occupy upachaya house from moon
                 //ven_upa = true;
                    nupa++;
                    upa_pl += "Venus,";
                }
                if (dctPlHou["Ju"].mhno == 3 || dctPlHou["Ju"].mhno == 6 || dctPlHou["Ju"].mhno == 10 || dctPlHou["Ju"].mhno == 11)
                {//jupiter occupy upachaya house from moon
                 //jup_upa = true;
                    nupa++;
                    upa_pl += "Jupiter";
                }
                if (nupa == 3)
                { //all benefics in upachaya houses
                    try
                    {
                        dctYogs["VASUMAN-YOGA,UPA"] = string.Format("{0}", dctSrc["VASUMAN-YOGA,UPA"].Replace("[1]", dctAst["UPACHAYA-HOUSES"]).Replace("[2]", dctAst["VASUMAN-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["VASUMAN-YOGA,UPA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (nupa == 2)
                {
                    try
                    {
                        dctYogs["VASUMAN-YOGA"] = string.Format("{0}", dctSrc["VASUMAN-YOGA"].Replace("[1]", upa_pl).Replace("[2]", dctAst["UPACHAYA-HOUSES"]).Replace("[3]", dctAst["VASUMAN-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["VASUMAN-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                //chandra dhana yogas
                string cdn = string.Empty;
                string c = string.Empty;
                switch (dctPlHou["Mo"].shno)
                {
                    case 1:
                    case 4:
                    case 7:
                    case 10:
                        //moon in kendra from sun
                        break;
                    case 2:
                    case 5:
                    case 8:
                    case 11:
                        //moon in panapahara from sun
                        break;
                    case 3:
                    case 6:
                    case 9:
                    case 12:
                        try
                        {
                            dctYogs["UTTAMADI-YOGA"] = string.Format("{0}", dctSrc["UTTAMADI-YOGA"].Replace("[1]", dctPlHou["Mo"].shno.ToString()).Replace("[2]", dctAst["APOKLIMA-HOUSE"]).Replace("[3]", dctAst["UTTAMADI-YOGA"]).Replace("[4]", dctSrc["UTTAMADI-YOGA," + dctPlHou["Mo"].shno.ToString()]));
                        }
                        catch
                        {
                            dctYogs["UTTAMADI-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    default:
                        break;
                }
                if (dctPlHou["Ju"].mhno == 6 || dctPlHou["Ju"].mhno == 8 || dctPlHou["Ju"].mhno == 12)
                {//shakata yoga
                    if (ncad < 1 && (dctPlHou["Ju"].lordship != "KEN" || dctPlHou["Ju"].lordship != "BOTH"))
                    {
                        switch (dctPlHou["Ju"].mhno)
                        {
                            case 6:
                            case 8:
                            case 12:
                                try
                                {
                                    dctYogs["SHAKATA-YOGA,JU"] = string.Format("{0} {1}", dctSrc["SHAKATA-YOGA"].Replace("[1]", dctPlHou["Jo"].shno.ToString()).Replace("[2]", dctAst["SHAKATA-YOGA"]), dctSrc[string.Format("SHAKATA-YOGA,JU|{0}", dctPlHou["Jo"].shno)]);
                                }
                                catch
                                {
                                    dctYogs["SHAKATA-YOGA,JU"] = "Internal error. Please report to help desk.";
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (one7shak)
                {//all planents occupying lagna and 7th house
                    try
                    {
                        dctYogs["SHAKATA-YOGA,1-7"] = string.Format("{0}", dctSrc["SHAKATA-YOGA,1-7"].Replace("[1]", dctAst["SHAKATA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["SHAKATA-YOGA,1-7"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (four10pak)
                {//all planents occupying 4th and 10th house
                    try
                    {
                        dctYogs["PAKSHI-YOGA"] = string.Format("{0}", dctSrc["PAKSHI-YOGA"].Replace("[1]", dctAst["PAKSHI-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["PAKSHI-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                switch (nsank)
                {
                    case 7:
                        //veena yoga
                        try
                        {
                            dctYogs["VEENA-YOGA"] = string.Format("{0}", dctSrc["VEENA-YOGA"].Replace("[1]", dctAst["VEENA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["VEENA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 6:
                        //daama yoga
                        try
                        {
                            dctYogs["DAAMA-YOGA"] = string.Format("{0}", dctSrc["DAAMA-YOGA"].Replace("[1]", dctAst["DAAMA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["DAAMA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 5:
                        //paasha yoga
                        try
                        {
                            dctYogs["PAASHA-YOGA"] = string.Format("{0}", dctSrc["PAASHA-YOGA"].Replace("[1]", dctAst["PAASHA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["PAASHA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 4:
                        //keedara yoga
                        try
                        {
                            dctYogs["KEDHARA-YOGA"] = string.Format("{0}", dctSrc["KEDHARA-YOGA"].Replace("[1]", dctAst["KEDHARA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["KEDHARA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 3:
                        //shoola yoga
                        try
                        {
                            dctYogs["SHOOLA-YOGA"] = string.Format("{0}", dctSrc["SHOOLA-YOGA"].Replace("[1]", dctAst["SHOOLA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["SHOOLA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 2:
                        //yuga yoga
                        try
                        {
                            dctYogs["YUGA-YOGA"] = string.Format("{0}", dctSrc["YUGA-YOGA"].Replace("[1]", dctAst["YUGA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["YUGA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                        break;
                    case 1:
                        //gola yoga
                        dctYogs["GOLA-YOGA"] = string.Format("{0}", dctSrc["GOLA-YOGA"].Replace("[1]", dctAst["GOLA-YOGA"]).Replace("[2]", dctAst["SANKHYA-YOGAS"]));
                        break;
                    default:
                        break;

                }
                //Aashraya yogas
                if (msgn_pls != string.Empty && fsgn_pls == string.Empty && dsgn_pls == string.Empty)
                {//rajju yoga
                    try
                    {
                        dctYogs["RAJJU-YOGA"] = string.Format("{0}", dctSrc["RAJJU-YOGA"].Replace("[1]", dctAst["RAJJU-YOGA"]).Replace("[2]", dctAst["AASHRAYA-YOGAS"]));
                    }
                    catch
                    {
                        dctYogs["RAJJU-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (msgn_pls == string.Empty && fsgn_pls != string.Empty && dsgn_pls == string.Empty)
                {//musala yoga
                    try
                    {
                        dctYogs["MUSALA-YOGA"] = string.Format("{0}", dctSrc["MUSALA-YOGA"].Replace("[1]", dctAst["MUSALA-YOGA"]).Replace("[2]", dctAst["AASHRAYA-YOGAS"]));
                    }
                    catch
                    {
                        dctYogs["MUSALA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (msgn_pls == string.Empty && fsgn_pls == string.Empty && dsgn_pls != string.Empty)
                {//nala yoga
                    try
                    {
                        dctYogs["NALA-YOGA"] = string.Format("{0}", dctSrc["NALA-YOGA"].Replace("[1]", dctAst["NALA-YOGA"]).Replace("[2]", dctAst["AASHRAYA-YOGAS"]));
                    }
                    catch
                    {
                        dctYogs["NALA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                //Dala yogas
                if (dctPlHou["Ju"].lordship == "KEN" && dctPlHou["Me"].lordship == "KEN" && dctPlHou["Ve"].lordship == "KEN" && dctPlHou["Mo"].lordship == "KEN")
                {//maala yoga
                    if (!malefic_in_ken)
                    {
                        try
                        {
                            dctYogs["MAALA-YOGA"] = string.Format("{0}", dctSrc["MAALA-YOGA"].Replace("[1]", dctAst["MAALA-YOGA"]).Replace("[2]", dctAst["DALA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["MAALA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                    }
                }
                else if (dctPlHou["Sa"].lordship == "KEN" && dctPlHou["Su"].lordship == "KEN" && dctPlHou["Ma"].lordship == "Ke" && dctPlHou["Ra"].lordship == "KEN" && dctPlHou["Ke"].lordship == "KEN")
                {//sarpa yoga
                    if (!benefic_in_ken)
                    {
                        try
                        {
                            dctYogs["SARPA-YOGA"] = string.Format("{0}", dctSrc["SARPA-YOGA"].Replace("[1]", dctAst["SARPA-YOGA"]).Replace("[2]", dctAst["DALA-YOGAS"]));
                        }
                        catch
                        {
                            dctYogs["SARPA-YOGA"] = "Internal error. Please report to help desk.";
                        }
                    }
                }
                bool gad = false;
                string gad_1h = string.Empty, gad_2h = string.Empty;
                if (one_hou_pl + four_hou_pl == 7)
                {
                    //all planets in 1 & 4 gada yoga
                    gad = true;
                    gad_1h = dctAst["LAGNA"];
                    gad_2h = "2";
                }
                else if (four_hou_pl + svn_hou_pl == 7)
                {
                    gad = true;
                    gad_1h = "4";
                    gad_2h = "7";
                }
                else if (svn_hou_pl + ten_hou_pl == 7)
                {
                    gad = true;
                    gad_1h = "7";
                    gad_2h = "10";
                }
                else if (ten_hou_pl + one_hou_pl == 7)
                {
                    gad = true;
                    gad_1h = "10";
                    gad_2h = dctAst["LAGNA"];
                }
                if (gad)
                {
                    try
                    {
                        //gada yoga
                        dctYogs["GADA-YOGA"] = string.Format("{0}", dctSrc["GADA-YOGA"].Replace("[1]", gad_1h).Replace("[2]", gad_2h).Replace("[3]", dctAst["GADA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["GADA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                string ken_hou = string.Empty;
                string tri_hou = string.Empty;
                string lag_hou = string.Empty;
                string trin1_hou = string.Empty;
                string trin2_hou = string.Empty;
                string trin3_hou = string.Empty;
                string yupa_hou = string.Empty;
                string shara_hou = string.Empty;
                string shakti_hou = string.Empty;
                string danda_hou = string.Empty;
                string nauka_hou = string.Empty;
                string koota_hou = string.Empty;
                string chatra_hou = string.Empty;
                string danusta_hou = string.Empty;
                string achand_hou = string.Empty;
                string chakra_hou = string.Empty;
                string samudra_hou = string.Empty;
                bool bken = true, btri = true, blag = true, btrin = true, byupa = true, bshara = true, bshakti = true, bdanda = true, bnauka = true, bkoota = true, bchatra = true, bdanusta = true, bachand = true, bchakra = true, bsamudra = true;
                bool isAdj = false;
                //angular group
                string ben_1hou = string.Empty, ben_7hou = string.Empty, ben_8hou = string.Empty, ben_9hou = string.Empty, ben_10hou = string.Empty;
                //parivarthan yoga
                bool bheri = true;
                PlanetHouse p10L = dctPlHou[dctPlHou[hou10.Split('|')[2]].houselord];
                foreach (KeyValuePair<string, PlanetHouse> item in dctPlHou)
                {
                    PlanetHouse pH = item.Value;
                    if (pH.code == dctPlHou[pH.houselord].code)
                    {//parivarthan/exchange yoga
                        PlanetHouse pLL = dctPlHou[asc_h.Split('|')[2]];
                        PlanetHouse p2L = dctPlHou[hou2.Split('|')[2]];
                        PlanetHouse p4L = dctPlHou[hou4.Split('|')[2]];
                        PlanetHouse p5L = dctPlHou[hou5.Split('|')[2]];
                        PlanetHouse p7L = dctPlHou[hou7.Split('|')[2]];
                        PlanetHouse p9L = dctPlHou[hou9.Split('|')[2]];
                        int maha_yog = -1;
                        switch (pH.hno)
                        {
                            case 2:
                            case 4:
                            case 5:
                            case 7:
                            case 9:
                            case 10:
                            case 11:
                                if ((pH.houselord == pLL.code) && (pH.hno == dctPlHou[pLL.houselord].hno))
                                {//lagna lord xchange with 2,4,5,7,9,10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,1"] = string.Format("{0}", dctSrc["MAHA-YOGA,1"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,1"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2) && (pH.houselord == p2L.code) && (pH.hno == dctPlHou[p2L.houselord].hno))
                                {//second lord xchange with 4,5,7,9,10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,2"] = string.Format("{0}", dctSrc["MAHA-YOGA,2"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,2"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2 && pH.hno != 4) && (pH.houselord == p4L.code) && (pH.hno == dctPlHou[p4L.houselord].hno))
                                {//forth lord xchange with 4,5,7,9,10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,4"] = string.Format("{0}", dctSrc["MAHA-YOGA,4"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAhA-YOGA,4"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2 && pH.hno != 4 && pH.hno != 5) && (pH.houselord == p5L.code) && (pH.hno == dctPlHou[p5L.houselord].hno))
                                {//fifth lord xchange with 7,9,10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,5"] = string.Format("{0}", dctSrc["MAHA-YOGA,5"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,5"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2 && pH.hno != 4 && pH.hno != 5 && pH.hno != 7) && (pH.houselord == p7L.code) && (pH.hno == dctPlHou[p7L.houselord].hno))
                                {//seventh lord xchange with 9,10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,7"] = string.Format("{0}", dctSrc["MAHA-YOGA,7"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,7"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2 && pH.hno != 4 && pH.hno != 5 && pH.hno != 7 && pH.hno != 9) && (pH.houselord == p9L.code) && (pH.hno == dctPlHou[p9L.houselord].hno))
                                {//ninth lord xchange with 10,11 lords
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,9"] = string.Format("{0}", dctSrc["MAHA-YOGA,9"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,9"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                else if ((pH.hno != 2 && pH.hno != 4 && pH.hno != 5 && pH.hno != 7 && pH.hno != 9 && pH.hno != 10) && (pH.houselord == p5L.code) && (pH.hno == dctPlHou[p5L.houselord].hno))
                                {//tenth lord xchange with 11 lord
                                    try
                                    {
                                        dctYogs["MAHA-YOGA,10"] = string.Format("{0}", dctSrc["MAHA-YOGA,10"].Replace("[1]", dctAst[pLL.name]).Replace("[2]", pH.hno.ToString()).Replace("[3]", dctAst[dctPlNames[pH.houselord]]).Replace("[4]", dctAst["MAHA-YOGA"]).Replace("[5]", dctAst["PARIVARTANA-YOGAS"]));
                                    }
                                    catch
                                    {
                                        dctYogs["MAHA-YOGA,10"] = "Internal error. Please report to help desk.";
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        if ((pH.hno == 2 && dctPlHou[pH.houselord].hno == 9) || (pH.hno == 9 && dctPlHou[pH.houselord].hno == 2))
                        {
                            if ((pH.lordship == "KEN" || pH.lordship == "TRI" || pH.lordship == "BOTH") && (dctPlHou[pH.houselord].lordship == "KEN" || dctPlHou[pH.houselord].lordship == "TRI" || dctPlHou[pH.houselord].lordship == "BOTH"))
                            {//khadga yoga
                                try
                                {
                                    dctYogs["KHADGA-YOGA"] = string.Format("{0}", dctSrc["KHADGA-YOGA"].Replace("[1]", dctAst[pH.lordship]).Replace("[2]", dctAst[pH.name]).Replace("[3]", dctAst[dctPlHou[pH.houselord].lordship]).Replace("[4]", dctAst[dctPlHou[pH.houselord].name]).Replace("[5]", dctAst["MAHA-YOGA"]));
                                }
                                catch
                                {
                                    dctYogs["KHADGA-YOGA"] = "Internal error. Please report to help desk.";
                                }
                            }
                        }
                    }
                    if (pH.lordship == "KEN")
                    {
                        ken_hou += pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else if (pH.lordship != "BOTH")
                    {
                        bken = false;
                    }
                    if (pH.lordship == "TRI")
                    {
                        tri_hou += pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else if (pH.lordship != "BOTH")
                    {
                        btri = false;
                    }
                    if (pH.lordship == "BOTH")
                    {
                        lag_hou += pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        blag = false;
                    }
                    if (pH.hno == 2 || pH.hno == 6 || pH.hno == 10)
                    {
                        trin1_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else if (pH.hno == 3 || pH.hno == 7 || pH.hno == 11)
                    {
                        trin2_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else if (pH.hno == 4 || pH.hno == 8 || pH.hno == 12)
                    {
                        trin3_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        btrin = false;
                    }
                    if (pH.hno > 0 && pH.hno < 5)
                    {
                        yupa_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        byupa = false;
                    }
                    if (pH.hno > 3 && pH.hno < 8)
                    {
                        shara_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bshara = false;
                    }
                    if (pH.hno > 6 && pH.hno < 11)
                    {
                        shakti_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bshakti = false;
                    }
                    if (pH.hno == 1 || (pH.hno > 9 && pH.hno < 13))
                    {
                        danda_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bdanda = false;
                    }
                    if (pH.hno > 0 && pH.hno < 8)
                    {
                        nauka_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bnauka = false;
                    }
                    if (pH.hno > 3 && pH.hno < 11)
                    {
                        koota_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bkoota = false;
                    }
                    if (pH.hno == 1 || (pH.hno > 6 && pH.hno < 13))
                    {
                        chatra_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bchatra = false;
                    }
                    if ((pH.hno > 0 && pH.hno < 5) || (pH.hno > 9 && pH.hno < 13))
                    {
                        danusta_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bdanusta = false;
                    }
                    if (pH.lordship != "KEN" || pH.lordship != "BOTH")
                    {
                        achand_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bachand = false;
                    }
                    if (pH.hno == 1 || pH.hno == 3 || pH.hno == 5 || pH.hno == 7 || pH.hno == 9 || pH.hno == 11)
                    {
                        chakra_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bchakra = false;
                    }
                    if (pH.hno == 2 || pH.hno == 4 || pH.hno == 6 || pH.hno == 8 || pH.hno == 10 || pH.hno == 12)
                    {
                        samudra_hou = pH.code + "," + pH.hno.ToString() + "|";
                    }
                    else
                    {
                        bsamudra = false;
                    }
                    if (pH.hno == 1 && (pH.code == "Me" || pH.code == "Ve" || pH.code == "Ju" || pH.code == "Mo"))
                    {
                        ben_1hou += pH.code + ",";
                    }
                    else if (pH.hno == 7 && (pH.code == "Me" || pH.code == "Ve" || pH.code == "Ju" || pH.code == "Mo"))
                    {
                        ben_7hou += pH.code + ",";
                    }
                    else if (pH.hno == 8 && (pH.code == "Me" || pH.code == "Ve" || pH.code == "Ju" || pH.code == "Mo"))
                    {
                        ben_8hou += pH.code + ",";
                    }
                    else if (pH.hno == 9 && (pH.code == "Me" || pH.code == "Ve" || pH.code == "Ju" || pH.code == "Mo"))
                    {
                        ben_9hou += pH.code + ",";
                    }
                    else if (pH.hno == 10 && (pH.code == "Me" || pH.code == "Ve" || pH.code == "Ju" || pH.code == "Mo"))
                    {
                        ben_10hou += pH.code + ",";
                    }
                    if (pH.hno != 1 && pH.hno != 2 && pH.hno != 7 && pH.hno != 12)
                    {
                        bheri = false;
                    }
                }
                //ravi yogas
                if (ben_7 && ben_8)
                {//lagnadhi yoga
                    bool bcan_lag = false;
                    if (dctPlHou["Sa"].hno == 7 || dctPlHou["Sa"].hno == 8)
                    {//saturn associates 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Sa"].hno + 6 == 7) || (dctPlHou["Sa"].hno + 6 == 8))
                    {//saturn 7th aspect to 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Sa"].hno + 2 == 7) || (dctPlHou["Sa"].hno + 2 == 8))
                    {//saturn 3rd aspect to 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Sa"].hno + 9 == 7) || (dctPlHou["Sa"].hno + 9 == 8))
                    {//saturn 10th aspect to 7 or 8
                        bcan_lag = true;
                    }
                    if (dctPlHou["Ma"].hno == 7 || dctPlHou["Ma"].hno == 8)
                    {//mars associates 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Ma"].hno + 6 == 7) || (dctPlHou["Ma"].hno + 6 == 8))
                    {//mars 7th aspect to 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Ma"].hno + 3 == 4) || (dctPlHou["Sa"].hno + 3 == 4))
                    {//mars 4th aspect to 7 or 8
                        bcan_lag = true;
                    }
                    if (dctPlHou["Su"].hno == 7 || dctPlHou["Su"].hno == 8)
                    {//Sun associates 7 or 8
                        bcan_lag = true;
                    }
                    else if ((dctPlHou["Su"].hno + 6 == 7) || (dctPlHou["Su"].hno + 6 == 8))
                    {//Sun 7th aspect to 7 or 8
                        bcan_lag = true;
                    }
                    if (!bcan_lag)
                    {
                        try
                        {
                            dctYogs["LAGNADHI-YOGA"] = string.Format("{0}", dctSrc["LAGNADHI-YOGA"].Replace("[1]", ben_7hou).Replace("[2]", ben_8hou).Replace("[3]", dctAst["LAGNADHI-YOGA"]));
                        }
                        catch
                        {
                            dctYogs["LAGNADHI-YOGA"] = "Internal error. Please report to help desk.";
                        }
                    }
                }
                if (bken)
                {
                    bool kh1 = false, kh2 = false, kh3 = false, kh4 = false;
                    bool kh1_isbenf = false, kh2_isbenf = false, kh3_isbenf = false, kh4_isbenf = false;
                    string kh1_p = string.Empty, kh2_p = string.Empty, kh3_p = string.Empty, kh4_p = string.Empty;
                    foreach (string kph in ken_hou.Split('|'))
                    {
                        //foreach (string ph in kph.Split(','))
                        if (kph.Split(',')[0] != "Ra" && kph.Split(',')[0] != "Ke")
                        {
                            int h = Convert.ToInt32(kph.Split(',')[1]);
                            switch (h)
                            {
                                case 1:
                                    kh1 = true;
                                    kh1_p += dctAst[dctPlNames[kph.Split(',')[0]]] + ',';
                                    if (kph.Split(',')[0] == "Ju" || kph.Split(',')[0] == "Ve" || kph.Split(',')[0] == "Me" || kph.Split(',')[0] == "Mo")
                                        kh1_isbenf = true;
                                    break;
                                case 4:
                                    kh2 = true;
                                    kh2_p += dctAst[dctPlNames[kph.Split(',')[0]]] + ',';
                                    if (kph.Split(',')[0] == "Ju" || kph.Split(',')[0] == "Ve" || kph.Split(',')[0] == "Me" || kph.Split(',')[0] == "Mo")
                                        kh2_isbenf = true;
                                    break;
                                case 7:
                                    kh3 = true;
                                    kh3_p += dctAst[dctPlNames[kph.Split(',')[0]]] + ',';
                                    if (kph.Split(',')[0] == "Ju" || kph.Split(',')[0] == "Ve" || kph.Split(',')[0] == "Me" || kph.Split(',')[0] == "Mo")
                                        kh3_isbenf = true;
                                    break;
                                case 10:
                                    kh4 = true;
                                    kh4_p += dctAst[dctPlNames[kph.Split(',')[0]]] + ',';
                                    if (kph.Split(',')[0] == "Ju" || kph.Split(',')[0] == "Ve" || kph.Split(',')[0] == "Me" || kph.Split(',')[0] == "Mo")
                                        kh4_isbenf = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    foreach (string lph in lag_hou.Split('|'))
                    {
                        if (lph.Split(',')[0] != "Ra" && lph.Split(',')[0] != "Ke")
                        {
                            int h = Convert.ToInt32(lph.Split(',')[1]);
                            switch (h)
                            {
                                case 1:
                                    kh1 = true;
                                    kh1_p += dctAst[dctPlNames[lph.Split(',')[0]]] + ',';
                                    if (lph.Split(',')[0] == "Ju" || lph.Split(',')[0] == "Ve" || lph.Split(',')[0] == "Me" || lph.Split(',')[0] == "Mo")
                                        kh1_isbenf = true;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    //if (bken)
                    {
                        //check if shak
                        //if (kh1 && kh3 && kh2 == false && kh4 == false)
                        //{
                        //shakata yoga
                        //}
                        //if (kh3 && kh4 && kh1 == false && kh2 == false)
                        //{
                        //pakshi yoga
                        //}
                        //if ((kh1 && kh2  && kh3 == false && kh4 == false) || (kh3 && kh4  && kh1 == false && kh2 == false))
                        //{
                        //gada yoga
                        //}
                        if (kh1 && kh1_isbenf && kh3 && kh3_isbenf && kh2 && !kh2_isbenf && kh4 && !kh4_isbenf)
                        {//vajra yoga
                            try
                            {
                                dctYogs["VAJRA-YOGA,1"] = string.Format("{0}", dctSrc["VAJRA-YOGA,1"].Replace("[1]", kh1_p + kh3_p).Replace("[2]", kh2_p + kh4_p).Replace("[3]", dctAst["VAJRA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["VAJRA-YOGA,1"] = "Internal error. Please report to help desk.";
                            }
                        }
                        else if (kh1 && !kh1_isbenf && kh3 && !kh3_isbenf && kh2 && kh2_isbenf && kh4 && kh4_isbenf)
                        {//vajra yoga
                            try
                            {
                                dctYogs["VAJRA-YOGA,2"] = string.Format("{0}", dctSrc["VAJRA-YOGA,2"].Replace("[1]", kh1_p + kh3_p).Replace("[2]", kh2_p + kh4_p).Replace("[3]", dctAst["VAJRA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["VAJRA-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }
                        else if (kh1 && kh3 && kh2 && kh4)
                        {//kamala yoga
                            try
                            {
                                dctYogs["KAMALA-YOGA"] = string.Format("{0}", dctSrc["KAMALA-YOGA"].Replace("[1]", dctAst["KAMALA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["KAMALA-YOGA"] = "Internal error. Please report to help desk.";
                            }
                        }
                        if (kh1 || kh2 || kh3 || kh4)
                        { //cancellation of kemadrupa yoga due to precense of planets in kendra from lagna
                            bcan_kem = true;
                        }
                    }
                    bool ben_in_ken = false;
                    if (!kh1 || (kh1 && kh1_isbenf))
                    {
                        ben_in_ken = true;
                    }
                    else
                    {
                        ben_in_ken = false;
                    }
                    if (!kh2 || (kh2 && kh2_isbenf))
                    {
                        ben_in_ken = true;
                    }
                    else
                    {
                        ben_in_ken = false;
                    }
                    if (!kh3 || (kh3 && kh3_isbenf))
                    {
                        ben_in_ken = true;
                    }
                    else
                    {
                        ben_in_ken = false;
                    }
                    if (!kh4 || (kh4 && kh4_isbenf))
                    {
                        ben_in_ken = true;
                    }
                    else
                    {
                        ben_in_ken = false;
                    }
                    if (ben_in_ken && (!mel_6) && (!mel_8))
                    {//parvatha yoga
                        try
                        {
                            dctYogs["PARVATA-YOGA,BEN|6,8"] = string.Format("{0}", dctSrc["PARVATHA-YOGA,BEN|6,8"].Replace("[1]", dctAst["PARVATHA-YOGA"]));
                        }
                        catch
                        {
                            dctYogs["PARVATA-YOGA,BEN|6-8"] = "Internal error. Please report to help desk.";
                        }
                    }
                    if ((dctPlHou[asc_h.Split('|')[2]].lordship == "KEN" || dctPlHou[asc_h.Split('|')[2]].lordship == "BOTH") && (dctPlHou[hou12.Split('|')[2]].lordship == "KEN" || dctPlHou[hou12.Split('|')[2]].lordship == "BOTH"))
                    {
                        bool basp1 = false, basp2 = false;
                        int asp = (dctPlHou["Ju"].hno + 6) > 12 ? (dctPlHou["Ju"].hno + 6) - 12 : (dctPlHou["Ju"].hno + 6);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 7th aspect from jupiter
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == asp)
                        {//12 lord gets 7th aspect from jupiter
                            basp2 = true;
                        }
                        asp = (dctPlHou["Ju"].hno + 4) > 12 ? (dctPlHou["Ju"].hno + 4) - 12 : (dctPlHou["Ju"].hno + 4);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 5th aspect from jupiter
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == asp)
                        {//12 lord gets 5th aspect from jupiter
                            basp2 = true;
                        }
                        asp = (dctPlHou["Ju"].hno + 8) > 12 ? (dctPlHou["Ju"].hno + 8) - 12 : (dctPlHou["Ju"].hno + 8);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 9th aspect from jupiter
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == asp)
                        {//12 lord gets 9th aspect from jupiter
                            basp2 = true;
                        }
                        if (dctPlHou[asc_h.Split('|')[2]].hno == dctPlHou["Ju"].hno)
                        {//asc & jupiter in conj
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == dctPlHou["Ju"].hno)
                        {//12 lord & jupiter in conj
                            basp2 = true;
                        }
                        asp = (dctPlHou["Me"].hno + 6) > 12 ? (dctPlHou["Me"].hno + 6) - 12 : (dctPlHou["Me"].hno + 6);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 7th aspect from mercury
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == asp)
                        {//12 lord gets 7th aspect from mercury
                            basp2 = true;
                        }
                        if (dctPlHou[asc_h.Split('|')[2]].hno == dctPlHou["Me"].hno)
                        {//asc & mercury in conj
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == dctPlHou["Me"].hno)
                        {//12 lord & mercury in conj
                            basp2 = true;
                        }
                        asp = (dctPlHou["Ve"].hno + 6) > 12 ? (dctPlHou["Ve"].hno + 6) - 12 : (dctPlHou["Ve"].hno + 6);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 7th aspect from venus
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == asp)
                        {//12 lord gets 7th aspect from venus
                            basp2 = true;
                        }
                        if (dctPlHou[asc_h.Split('|')[2]].hno == dctPlHou["Ve"].hno)
                        {//asc & venus in conj
                            basp1 = true;
                        }
                        if (dctPlHou[hou12.Split('|')[2]].hno == dctPlHou["Ve"].hno)
                        {//12 lord & venus in conj
                            basp2 = true;
                        }
                        if (basp1 && basp2)
                        {//parvata yoga
                            try
                            {
                                dctYogs["PARVATA-YOGA,KEN|1,12"] = string.Format("{0}", dctSrc["PARVATHA-YOGA,KEN|1,12"].Replace("[1]", dctAst[dctPlHou[asc_h.Split('|')[2]].houselord]).Replace("[2]", dctAst[dctPlHou[hou12.Split('|')[2]].houselord]).Replace("[3]", dctAst["PARVATHA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["PARVATA-YOGA,KEN|1,12"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                    PlanetHouse p4L = dctPlHou[hou4.Split('|')[2]];
                    PlanetHouse p9L = dctPlHou[hou9.Split('|')[2]];
                    if ((p4L.lordship == "KEN" || p4L.lordship == "BOTH") && (p9L.lordship == "KEN" || p9L.lordship == "BOTH"))
                    {
                        PlanetStrength pS = checkStrength(dctPlHou[asc_h.Split('|')[2]]);
                        if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                        {//kaahala yoga
                            try
                            {
                                dctYogs["KAAHALA-YOGA,1"] = string.Format("{0}", dctSrc["KAAHALA-YOGA,1"].Replace("[1]", dctAst[dctPlHou[hou4.Split('|')[2]].houselord]).Replace("[2]", dctAst[dctPlHou[hou9.Split('|')[2]].houselord]).Replace("[3]", string.Format("{0},{1}", dctPlHou[hou4.Split('|')[2]].hno, dctPlHou[hou9.Split('|')[2]].hno)).Replace("[4]", dctAst[dctPlHou[asc_h.Split('|')[2]].houselord]).Replace("[5]", dctAst["KAAHALA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["KAAHALA-YOGA,1"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                    PlanetHouse p4 = dctPlHou[hou4.Split('|')[2]];
                    PlanetStrength pS4 = checkStrength(p4);
                    if (pS4 == PlanetStrength.EXALTED || pS4 == PlanetStrength.OWN)
                    {
                        PlanetHouse p10 = dctPlHou[hou10.Split('|')[2]];
                        bool asp = false;
                        if (p4.hno == p10.hno)
                        {//associated by 10 lord
                            asp = true;
                        }
                        switch (p10.code)
                        {
                            case "Su":
                            case "Mo":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                break;
                            case "Ju":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                asp = checkAspect(p10.hno, p4.hno, 5);
                                asp = checkAspect(p10.hno, p4.hno, 9);
                                break;
                            case "Me":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                break;
                            case "Ma":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                asp = checkAspect(p10.hno, p4.hno, 4);
                                break;
                            case "Ve":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                break;
                            case "Sa":
                                asp = checkAspect(p10.hno, p4.hno, 7);
                                asp = checkAspect(p10.hno, p4.hno, 3);
                                asp = checkAspect(p10.hno, p4.hno, 10);
                                break;
                            default:
                                break;
                        }
                        if (asp)
                        {//kaahala yoga
                            try
                            {
                                dctYogs["KAAHALA-YOGA,2"] = string.Format("{0}", dctSrc["KAAHALA-YOGA,2"].Replace("[1]", dctAst[dctPlHou[hou4.Split('|')[2]].houselord]).Replace("[2]", (pS4 == PlanetStrength.EXALTED) ? dctAst["exalted"] : dctAst["in own house"]).Replace("[3]", dctAst[dctPlHou[hou10.Split('|')[2]].houselord]).Replace("[4]", dctAst["KAAHALA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["KAAHALA-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }

                    }
                    PlanetHouse pHL = dctPlHou[asc_h.Split('|')[2]];
                    PlanetStrength pSL = checkStrength(pHL);
                    if (pSL == PlanetStrength.EXALTED && (pHL.lordship == "KEN" || pHL.lordship == "BOTH"))
                    {
                        bool basp = false;
                        int jasp = 0;
                        if (pHL.hno == dctPlHou["Ju"].hno)
                        {
                            basp = true;
                            jasp = 1;
                        }
                        int asp = (dctPlHou["Ju"].hno + 4) > 12 ? (dctPlHou["Ju"].hno + 4) - 12 : (dctPlHou["Ju"].hno + 4);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 5th aspect from jupiter
                            basp = true;
                            jasp = 5;
                        }
                        asp = (dctPlHou["Ju"].hno + 6) > 12 ? (dctPlHou["Ju"].hno + 6) - 12 : (dctPlHou["Ju"].hno + 6);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 7th aspect from jupiter
                            basp = true;
                            jasp = 7;
                        }
                        asp = (dctPlHou["Ju"].hno + 8) > 12 ? (dctPlHou["Ju"].hno + 8) - 12 : (dctPlHou["Ju"].hno + 8);
                        if (dctPlHou[asc_h.Split('|')[2]].hno == asp)
                        {//asc lord gets 9th aspect from jupiter
                            basp = true;
                            jasp = 9;
                        }
                        if (basp)
                        {//chamara yoga
                            try
                            {
                                dctYogs["CHAAMARA-YOGA,2"] = string.Format("{0}", dctSrc["CHAAMARA-YOGA,2"].Replace("[1]", dctAst[dctPlHou[asc_h.Split('|')[2]].houselord]).Replace("[2]", jasp.ToString()).Replace("[3]", dctAst["CHAAMARA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["CHAMARA-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                    if ((dctPlHou[hou4.Split('|')[2]].lordship == "KEN" || dctPlHou[hou4.Split('|')[2]].lordship == "BOTH") && (dctPlHou[hou9.Split('|')[2]].lordship == "KEN" || dctPlHou[hou9.Split('|')[2]].lordship == "BOTH"))
                    {
                        PlanetStrength pS = checkStrength(dctPlHou[asc_h.Split('|')[2]]);
                        if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                        {//kaahala yoga
                            try
                            {
                                dctYogs["KAAHALA-YOGA,1"] = string.Format("{0}", dctSrc["KAAHALA-YOGA,1"].Replace("[1]", dctAst[dctPlHou[hou4.Split('|')[2]].houselord]).Replace("[2]", dctAst[dctPlHou[hou9.Split('|')[2]].houselord]).Replace("[3]", string.Format("{0},{1}", dctPlHou[hou4.Split('|')[2]].hno, dctPlHou[hou9.Split('|')[2]].hno)).Replace("[4]", dctAst[dctPlHou[asc_h.Split('|')[2]].houselord]).Replace("[5]", dctAst["KAAHALA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["KAAHALA-YOGA,1"] = "Internal error. Please report to help desk.";
                            }
                            //dctYogs["KAAHALA-YOGA,1"] = string.Format("{0}", dctSrc["KAAHALA-YOGA,1"].Replace("[1]", dctAst[dctPlNames[asc_h.Split('|')[2]]]).Replace("[2]", dctAst[dctPlNames[hou9.Split('|')[2]]]).Replace("[3]", string.Format("{0},{1}", dctPlHou[hou4.Split('|')[2]].hno, dctPlHou[hou9.Split('|')[2]].hno)).Replace("[4]", dctAst[dctPlNames[asc_h.Split('|')[2]]]).Replace("[5]", dctAst["KHAALA-YOGA"]));
                        }
                    }
                    PlanetHouse p5L = dctPlHou[hou5.Split('|')[2]];
                    PlanetHouse p6L = dctPlHou[hou6.Split('|')[2]];
                    if ((p5L.lordship == "KEN" || p5L.lordship == "BOTH") && (p6L.lordship == "KEN" || p6L.lordship == "BOTH"))
                    {
                        PlanetStrength pS = checkStrength(dctPlHou[asc_h.Split('|')[2]]);
                        if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                        {//shankha yoga
                            try
                            {
                                dctYogs["SHANKHA-YOGA,2"] = string.Format("{0}", dctSrc["SHANKHA-YOGA,2"].Replace("[1]", dctAst[dctPlNames[hou5.Split('|')[2]]])).Replace("[2]", dctAst[dctPlNames[hou6.Split('|')[2]]]).Replace("[3]", string.Format("{0},{1}", p5L.hno, p6L.hno).Replace("[4]", dctAst[dctPlNames[asc_h.Split('|')[2]]]).Replace("[5]", dctAst["SHANKHA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["SHANKHA-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                    if ((pHL.lordship == "KEN" || pHL.lordship == "BOTH") && (dctPlHou["Ju"].lordship == "KEN" || dctPlHou["Ju"].lordship == "BOTH") && (dctPlHou["Ve"].lordship == "KEN" || dctPlHou["Ve"].lordship == "BOTH"))
                    {
                        PlanetStrength pS = checkStrength(p9L);
                        if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                        {//bheri yoga
                            try
                            {
                                dctYogs["BHERI-YOGA,2"] = string.Format("{0}", dctSrc["BHERI-YOGA,2"].Replace("[1]", dctAst[dctPlNames[asc_h.Split('|')[2]]]).Replace("[2]", dctAst[dctPlNames[hou9.Split('|')[2]]]).Replace("[3]", string.Format("{0},{1},{2}", pHL.hno, dctPlHou["Ju"].hno, dctPlHou["Ve"].hno)).Replace("[3]", dctAst["BHERI-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["BHERI-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                }
                else if (ken_hou == string.Empty && lag_hou == string.Empty)
                {//vapi yoga
                 //dctYogs["VAPI-YOGA"] = string.Format("{0}", dctSrc["VAPI-YOGA"].Replace("[1]", dctAst["VEPI-YOGA"]));
                }
                if (btri)
                {//shringataka yoga
                    try
                    {
                        dctYogs["SHRINGATAKA-YOGA"] = string.Format("{0}", dctSrc["SHRINGATAKA-YOGA"].Replace("[1]", tri_hou).Replace("[2]", dctAst["SHRINGATAKA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["SHRINGATAKA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                if (btrin)
                {//hala yoga
                    try
                    {
                        dctYogs["HALA-YOGA"] = string.Format("{0}", dctSrc["HALA-YOGA"].Replace("[1]", (trin1_hou != string.Empty) ? trin1_hou : (trin2_hou != string.Empty) ? trin2_hou : trin3_hou).Replace("[2]", dctAst["HALA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["HALA-YOGA"] = "Internal error. Please report to help desk.";
                    }
                }
                if (ben_1hou.Split(',').Count() > 0 || ben_7hou.Split(',').Count() > 0 || ben_9hou.Split(',').Count() > 0 || ben_10hou.Split(',').Count() > 0)
                {//chamara yoga
                    string benfs = string.Empty;
                    string hses = string.Empty;
                    if (ben_1hou.Split(',').Count() > 0)
                    {
                        foreach (var bnf in ben_1hou.Split(','))
                        {
                            if (bnf != string.Empty)
                                benfs += dctAst[dctPlNames[bnf]];
                        }
                        hses += "1,";
                    }
                    if (ben_7hou.Split(',').Count() > 0)
                    {
                        foreach (var bnf in ben_7hou.Split(','))
                        {
                            if (bnf != string.Empty)
                                benfs += dctAst[dctPlNames[bnf]];
                        }
                        hses += "7,";
                    }
                    if (ben_9hou.Split(',').Count() > 0)
                    {
                        foreach (var bnf in ben_9hou.Split(','))
                        {
                            if (bnf != string.Empty)
                                benfs += dctAst[dctPlNames[bnf]];
                        }
                        hses += "9,";
                    }
                    if (ben_10hou.Split(',').Count() > 1)
                    {
                        foreach (var bnf in ben_1hou.Split(','))
                        {
                            if (bnf != string.Empty)
                                benfs += dctAst[dctPlNames[bnf]];
                        }
                        hses += "10,";
                    }
                    try
                    {
                        dctYogs["CHAAMARA-YOGA,1"] = string.Format("{0}", dctSrc["CHAAMARA-YOGA,1"].Replace("[1]", benfs).Replace("[2]", hses.Trim()).Replace("[3]", dctAst["CHAAMARA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["CHAMARA-YOGA,1"] = "Internal error. Please report to help desk.";
                    }
                }
                PlanetHouse pLAG = dctPlHou[asc_h.Split('|')[2]];
                //PlanetHouse p10L = dctPlHou[dctPlHou[hou10.Split('|')[2]].houselord];
                if (pLAG.signtype == "M" && p10L.signtype == "M")
                {
                    PlanetHouse p9L = dctPlHou[hou9.Split('|')[2]];
                    PlanetStrength pS = checkStrength(p9L);
                    if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                    {//shankha yoga
                        try
                        {
                            dctYogs["SHANKHA-YOGA,3"] = string.Format("{0}", dctSrc["SHANKHA-YOGA,3"].Replace("[1]", dctAst[dctPlNames[pLAG.code]]).Replace("[2]", dctAst[dctPlNames[p10L.code]]).Replace("[3]", dctAst["movable"]).Replace("[4]", dctAst[dctPlNames[p9L.code]]).Replace("[5]", dctAst["SHANKHA-YOGA"]));
                        }
                        catch
                        {
                            dctYogs["SHANKHA-YOGA,3"] = "Internal error. Please report to help desk.";
                        }
                    }
                }
                if (bheri)
                {
                    PlanetHouse p9L = dctPlHou[hou9.Split('|')[2]];
                    PlanetStrength pS = checkStrength(p9L);
                    if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN || pS == PlanetStrength.FRIEND)
                    {//bheri yoga
                        try
                        {
                            dctYogs["BHERI-YOGA"] = string.Format("{0}", dctSrc["BHERI-YOGA,2"].Replace("[1]", dctAst[dctPlNames[p9L.name]]).Replace("[2]", dctAst["BHERI-YOGA"]));
                        }
                        catch
                        {
                            dctYogs["BHERI-YOGA"] = "Internal error. Please report to help desk.";
                        }
                    }
                }
                PlanetStrength pLAGS = checkStrength(pLAG);
                if (pLAGS == PlanetStrength.EXALTED || pLAGS == PlanetStrength.MOOLTRIKONA || pLAGS == PlanetStrength.OWN || pLAGS == PlanetStrength.FRIEND)
                {
                    PlanetHouse p9L = dctPlHou[hou9.Split('|')[2]];
                    if (p9L.lordship == "KEN" || p9L.lordship == "BOTH")
                    {
                        PlanetStrength pS = checkStrength(p9L);
                        if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN)
                        {//lakshmi yoga
                            string exa = (pS == PlanetStrength.EXALTED) ? "exalted" : (pS == PlanetStrength.MOOLTRIKONA) ? "mooltrikona" : "own";
                            try
                            {
                                dctYogs["LAKSHMI-YOGA"] = string.Format("{0}", dctSrc["LAKSHMI-YOGA"].Replace("[1]", dctAst[dctPlNames[pLAG.code]]).Replace("[2]", dctAst[dctPlNames[p9L.code]]).Replace("[3]", p9L.hno.ToString()).Replace("[4]", dctAst[exa]).Replace("[5]", dctAst["LAKSHMI-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["LAKSHMI-YOGA"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                }
                if (!bsnp && !banp && !bcan_kem)
                {
                    try
                    {
                        dctYogs["KEMADRUMA-YOGA,1"] = string.Format("{0}", dctSrc["KEMADRUMA-YOGA,1"].Replace("[1]", dctAst["KEMADRUMA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["KEMADRUMA-YOGA,1"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (!bcan_kem)
                {
                    if (dctPlHou["Mo"].hno == 1 || dctPlHou["Mo"].hno == 7)
                    {
                        if ((dctPlHou["Mo"].hno == dctPlHou["Ju"].hno) || (dctPlHou["Ju"].hno + 4 == dctPlHou["Mo"].hno) || (dctPlHou["Ju"].hno + 6 == dctPlHou["Mo"].hno) || (dctPlHou["Ju"].hno + 8 == dctPlHou["Mo"].hno))
                        { //moon in asc or 7th house gains jupiter aspect
                          //kemadhruma2 yoga
                            try
                            {
                                dctYogs["KEMADRUMA-YOGA,2"] = string.Format("{0}", dctSrc["KEMADRUMA-YOGA,2"].Replace("[1]", dctPlHou["Mo"].hno.ToString()).Replace("[2]", dctAst["KEMADRUMA-YOGA"]));
                            }
                            catch
                            {
                                dctYogs["KEMADRUMA-YOGA,2"] = "Internal error. Please report to help desk.";
                            }
                        }
                    }
                }
                else if (!bcan_kem && bmon_inmic)
                {
                    try
                    {
                        dctYogs["KEMADRUMA-YOGA,3"] = string.Format("{0}", dctSrc["KEMARUMA-YOGA,3"].Replace("[1]", dctAst[dctPlHou["Mo"].sign]).Replace("[2]", inmic_asp).Replace("[3]", dctAst["KEMADRUMA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["KEMADRUMA-YOGA,3"] = "Internal error. Please report to help desk.";
                    }
                }
                else if (!bcan_kem && bmon_in_rk)
                {
                    try
                    {
                        dctYogs["KEMADRUMA-YOGA,4"] = string.Format("{0}", dctSrc["KEMARUMA-YOGA,4"].Replace("[1]", dctPlHou["Mo"].hno.ToString()).Replace("[2]", dctAst["KEMADRUMA-YOGA"]));
                    }
                    catch
                    {
                        dctYogs["KEMADRUMA-YOGA,4"] = "Internal error. Please report to help desk.";
                    }
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("eX.Message", line.ToString());
            }
            return new JsonResult(dctYogs);
        }
        string sortPls(string pls)
        {
            Dictionary<string, int> dctPls = new Dictionary<string, int>();
            dctPls["Su"] = 1;
            dctPls["Mo"] = 2;
            dctPls["Ju"] = 3;
            dctPls["Me"] = 4;
            dctPls["Ve"] = 5;
            dctPls["Ma"] = 6;
            dctPls["Sa"] = 7;
            string[] arrPls = pls.Split('-');
            int j = 0;
            string sortPls = string.Empty;
            for (int i = 0; i < arrPls.Length; i++)
            {
                for (j = i + 1; j < arrPls.Length; j++)
                {
                    if (dctPls[arrPls[j]] < dctPls[arrPls[i]])
                    {
                        string tmp = arrPls[i];
                        arrPls[i] = arrPls[j];
                        arrPls[j] = tmp;
                    }
                }
            }
            for (int k = 0; k < arrPls.Length; k++)
                sortPls += arrPls[k] + "-";
            return sortPls.Remove(sortPls.Length - 1);
        }
        bool checkAspect(int hno1, int hno2, int a)
        {
            int asp = (hno1 + a) > 12 ? (hno1 + a) - 12 : (hno1 + a);
            return (hno2 == asp);
        }
        PlanetStrength checkStrength(PlanetHouse pl)
        {
            //bool strong_asc = false;
            PlanetStrength pS = PlanetStrength.NORMAL;
            switch (pl.code)
            {
                case "Su":
                    if (pl.sign == "ar")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "le")
                    {
                        if (pl.pos >= 4 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "le")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "cn" || pl.sign == "ar" || pl.sign == "sa" || pl.sign == "sc")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "cp" || pl.sign == "aq") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "li") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Mo":
                    if (pl.sign == "ta")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "ta")
                    {
                        if (pl.pos >= 4 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "cn")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "le" || pl.sign == "ar" || pl.sign == "sc" || pl.sign == "sa" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "ge" || pl.sign == "vi" || pl.sign == "sa" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.ENEMY;
                    }
                    else if (pl.sign == "sc") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Ju":
                    if (pl.sign == "cn")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "sa")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 10)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "pi")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "le" || pl.sign == "sc" || pl.sign == "ar")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "ta" || pl.sign == "li") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "cp") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Ve":
                    if (pl.sign == "pi")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "li")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 15)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "ta")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "cp" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "le" || pl.sign == "ta" || pl.sign == "li") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "sc") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Ma":
                    if (pl.sign == "cp")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "ar")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 12)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "sc")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "cp" || pl.sign == "aq") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "le" || pl.sign == "cn" || pl.sign == "sa" || pl.sign == "pi")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    break;
                case "Me":
                    if (pl.sign == "vi")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "vi")
                    {//own
                        if (pl.pos >= 16 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "ge")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "cp" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "sa" || pl.sign == "pi" || pl.sign == "ar" || pl.sign == "sc")
                    {//friend house
                        pS = PlanetStrength.ENEMY;
                    }
                    else if (pl.sign == "pi") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Sa":
                    if (pl.sign == "li")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "aq")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                    }
                    else if (pl.sign == "cp")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "ta")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "le") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "ar") pS = PlanetStrength.DEBILIATED;
                    break;
                default:
                    break;
            }
            return pS;
        }
        [HttpGet("RecfyBTEx")]
        public ActionResult RecfyBTEx(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
        {
            try
            {
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                //update lunar nodes
                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                int r1 = 0, r2 = 0;
                bool asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1] == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                string asc_sls = string.Empty;
                string mo_sls = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + Convert.ToDouble(p);
                            asc_sls = getSUBZ(ppos.Key, plpos);

                        }
                        else if (pl.Split(' ')[1] == "Mo")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + Convert.ToDouble(p);
                            mo_sls = getSUBZ(ppos.Key, plpos);
                        }
                        if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                    }
                    if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                }
                //LEVEL 1
                string a_signL = asc_sls.Split('-')[0];
                string a_starL = asc_sls.Split('-')[1];
                string a_subL = asc_sls.Split('-')[2];
                string m_signL = mo_sls.Split('-')[0];
                string m_starL = mo_sls.Split('-')[1];
                string m_subL = mo_sls.Split('-')[2];
                bool bL1 = false, bL2 = false, bL3 = false;
                if (a_signL == m_signL)
                {
                    bL1 = true;
                }
                else
                {
                    string star = getSSSL3(a_signL, mHoro);
                    if (star.Split('|')[2] == m_signL) bL1 = true;
                    else if (star.Split('|')[1] == m_signL) bL1 = true;
                    else if (star.Split('|')[0] == m_signL) bL1 = true;
                    star = getSSSL3(m_signL, mHoro);
                    if (star.Split('|')[2] == a_signL) bL1 = true;
                    else if (star.Split('|')[1] == a_signL) bL1 = true;
                    else if (star.Split('|')[0] == a_signL) bL1 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_signL == star.Split('|')[1] || a_signL == star.Split('|')[2]) bL1 = true;
                    }
                }
                //LEVEL 2
                if (a_signL == m_signL)
                {
                    bL2 = true;
                }
                else
                {
                    string star = getSSSL3(a_starL, mHoro);
                    if (star.Split('|')[2] == m_starL) bL2 = true;
                    else if (star.Split('|')[1] == m_starL) bL2 = true;
                    else if (star.Split('|')[0] == m_starL) bL2 = true;
                    star = getSSSL3(m_starL, mHoro);
                    if (star.Split('|')[2] == a_starL) bL2 = true;
                    else if (star.Split('|')[1] == a_starL) bL2 = true;
                    else if (star.Split('|')[0] == a_starL) bL2 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_starL == star.Split('|')[1] || a_starL == star.Split('|')[2]) bL2 = true;
                    }
                }
                //LEVEL 3
                if (a_signL == m_signL)
                {
                    bL3 = true;
                }
                else
                {
                    string star = getSSSL3(a_starL, mHoro);
                    if (star.Split('|')[2] == m_subL) bL3 = true;
                    else if (star.Split('|')[1] == m_subL) bL3 = true;
                    else if (star.Split('|')[0] == m_subL) bL3 = true;
                    star = getSSSL3(m_starL, mHoro);
                    if (star.Split('|')[2] == a_subL) bL3 = true;
                    else if (star.Split('|')[1] == a_subL) bL3 = true;
                    else if (star.Split('|')[0] == a_subL) bL3 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_subL == star.Split('|')[1] || a_subL == star.Split('|')[2]) bL3 = true;
                    }
                }
                RecfyBirthTime rcfBT = new RecfyBirthTime();
                if (bL1 && bL2 && bL3)
                {
                    rcfBT.rem = "Birth Time Rectification not required as Ruling Planets agrees";
                    rcfBT.recfyDOB = "";
                }
                else
                {
                    JsonResult jO = (JsonResult)SuggestBT(dob, tob, latlng, timezone, tzofset, ayanid);
                    rcfBT = (RecfyBirthTime)jO.Value;
                    if (rcfBT.rem == "Birth Time Rectified")
                    {
                        rcfBT.rem = string.Format("Birth Time Rectification is required as Ruling Planets did not agree. Rectified Birth Time is {0}", rcfBT.recfyDOB);
                        //rcfBT.recfyDOB = "RECTIFIED BIRTHTIME IS: ";
                    }
                    else
                    {
                        rcfBT.rem = string.Format("Birth Time Rectification is required as Ruling Planets did not agree. Our App could not rectify within 5min range from the given DOB, please try providing a best match close to 5min range");
                    }
                }
                return new JsonResult(rcfBT);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        public string getSSSL3(string lord, Horoscope horo)
        {
            Dictionary<string, int> zstart = new Dictionary<string, int>();
            zstart["ar"] = 0;
            zstart["ta"] = 30;
            zstart["ge"] = 60;
            zstart["cn"] = 90;
            zstart["le"] = 120;
            zstart["vi"] = 150;
            zstart["li"] = 180;
            zstart["sc"] = 210;
            zstart["sa"] = 240;
            zstart["cp"] = 270;
            zstart["aq"] = 300;
            zstart["pi"] = 330;
            string star = string.Empty;
            foreach (var ppos in horo.planetsPos)
            {
                foreach (var pl in ppos.Value.Split('|'))
                {
                    if (pl.Split(' ')[1].ToLower() == lord.Substring(0, 2))
                    {
                        string p = pl.Split(' ')[0].TrimEnd('.');
                        p = (pl.Split(' ')[0].IndexOf('.') != -1) ? pl.Split(' ')[0] : pl.Split(' ')[0] + ".0";
                        //console.log('pos len=' + pos.split('.').length.toString());
                        int mins = 0;
                        if (p.IndexOf('.') > -1 && p.Split('.')[1] != "")
                            mins = (zstart[ppos.Key] + Convert.ToInt32(p.Split('.')[0], 10)) * 60 + Convert.ToInt32(p.Split('.')[1], 10);
                        else
                            mins = (zstart[ppos.Key] + Convert.ToInt32(p.Split(' ')[0].Split('.')[0], 10)) * 60;

                        return calcSSSL3(mins);
                    }
                }
            }
            return "";
        }
        public string calcSSSL3(double mins)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                using (StreamReader r3 = new StreamReader(string.Format(@"{0}\sublords.json", astClient)))
                {
                    string json = r3.ReadToEnd();
                    List<SubLord> sublords = JsonConvert.DeserializeObject<List<SubLord>>(json);
                    string subz = string.Empty;
                    foreach (var item in sublords)
                    {
                        var degs = item.deg;
                        var s_mins = Convert.ToInt32(degs.Split('-')[0].Split('.')[0], 10) * 60 + Convert.ToInt32(degs.Split('-')[0].Split('.')[1]);
                        var e_mins = Convert.ToInt32(degs.Split('-')[1].Split('.')[0], 10) * 60 + Convert.ToInt32(degs.Split('-')[1].Split('.')[1]);
                        //var deg_s = parseFloat(degs.split('-')[0].split('.')[0] + '.' + degs.split('-')[0].split('.')[1]);
                        //var deg_e = parseFloat(degs.split('-')[1].split('.')[0] + '.' + degs.split('-')[1].split('.')[1]);
                        //console.log(s_mins);
                        //console.log(e_mins);
                        if (mins >= s_mins && mins <= e_mins)
                        {
                            //console.log(s_mins);
                            //console.log(e_mins);
                            return item.sign + '|' + item.star + '|' + item.sub;
                        }
                    }
                    return "";
                }
            }
            catch (Exception eX)
            {
                return eX.Message;
            }
        }
        [HttpGet("SuggestBT")]
        public ActionResult SuggestBT(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
        {
            try
            {
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                int numTrys = 10;
                string ds = string.Format("{0}-{1}-{2} {3}:{4}:{5},0", i3, u2, u1, u4, u5, u6);
                DateTime dtBT = new DateTime(i3, (int)u2, (int)u1, (int)u4, (int)u5, (int)u6);
                int am = 0, sm = 5, atrys = 0, strys = 0;
                bool adfied = false;
                DateTime dtA = dtBT;
                while (numTrys > 0)
                {
                    int min = 0;
                    if (am < 6 && !adfied) { am++; min = am; atrys++; }
                    else if (sm > -1) { sm--; min -= sm; strys++; }
                    DateTime dtR = dtBT.AddMinutes(min);
                    mHoro.init_data_ex2((uint)dtR.Day, (uint)dtR.Month, dtR.Year, (uint)dtR.Hour, (uint)dtR.Minute, (uint)dtR.Second, u7, u8, tz, ayan, (uint)ayanid);
                    mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                    //update lunar nodes
                    string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                    foreach (var ppos in mHoro.planetsPos)
                    {
                        foreach (var pl in ppos.Value.Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                            else if (pl.Split(' ')[1] == "MEAN_NODE")
                            {
                                rsn = ppos.Key;
                                rdeg = pl.Split(' ')[0];
                            }
                            if (rsn != string.Empty && asn != string.Empty) break;
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                    int r1 = 0, r2 = 0;
                    bool asc = false;
                    for (r1 = 0; r1 < 12; r1++)
                    {
                        if (asc) r2++;
                        if (ras[r1] == rsn)
                        {
                            asc = true;
                            r2++;
                        }
                        if (r2 == 7)
                        {
                            ksn = ras[r1];
                            break;
                        }
                        if (r1 == 11) r1 = -1;
                    }

                    int rpos = calcHno(asn, rsn);
                    int kpos = calcHno(rsn, ksn);
                    //var mn = i + 11;
                    //if (mn > 15) mn -= 15;
                    if (mHoro.planetsPos.ContainsKey(ksn))
                    {
                        var eP = mHoro.planetsPos[ksn];
                        mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                    }
                    else
                    {
                        mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                    }
                    // plPos[sign] = ePls;
                    mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                    JsonResult jObj = (JsonResult)BTRL3Check(mHoro);
                    RecfyBirthTime rcfB = (RecfyBirthTime)jObj.Value;
                    if (rcfB.recfyDOB == string.Empty)
                    {
                        if (!adfied)
                        {
                            dtA = dtR;
                            adfied = true;
                        }
                        else
                        {
                            RecfyBirthTime rcfBT = new RecfyBirthTime();
                            rcfBT.rem = "Birth Time Rectified";
                            if (atrys < strys)
                                rcfBT.recfyDOB = string.Format("{0}/{1}/{2} {3}:{4}", dtA.Day, dtA.Month, dtA.Year, dtA.Hour, dtA.Minute);
                            else
                                rcfBT.recfyDOB = string.Format("{0}/{1}/{2} {3}:{4}", dtR.Day, dtR.Month, dtR.Year, dtR.Hour, dtR.Minute);
                            return new JsonResult(rcfBT);
                        }
                    }
                    numTrys--;
                }
                if (adfied)
                {
                    RecfyBirthTime rcf = new RecfyBirthTime();
                    rcf.rem = "Birth Time Rectified";
                    rcf.recfyDOB = string.Format("{0}/{1}/{2} {3}:{4}", dtA.Day, dtA.Month, dtA.Year, dtA.Hour, dtA.Minute);
                    return new JsonResult(rcf);
                }
                else
                {
                    RecfyBirthTime rcf = new RecfyBirthTime();
                    rcf.rem = "Birth Time Not Rectified";
                    rcf.recfyDOB = "Could not rectify birthtime within 5 minute range, currently our App can perform BTR upto 5 minute range.";
                    return new JsonResult(rcf);
                }
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                RecfyBirthTime rcf = new RecfyBirthTime();
                rcf.rem = "Birth Time Not Rectified";
                rcf.recfyDOB = "An exception has occurred while performing the check, please report the error to help desk.";
                return new JsonResult(rcf);
            }
        }
        [HttpGet("BTRL3Check")]
        public ActionResult BTRL3Check(Horoscope mHoro)
        {
            try
            {
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                string asc_sls = string.Empty;
                string mo_sls = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + Convert.ToDouble(p);
                            asc_sls = getSUBZ(ppos.Key, plpos);

                        }
                        else if (pl.Split(' ')[1] == "Mo")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + Convert.ToDouble(p);
                            mo_sls = getSUBZ(ppos.Key, plpos);
                        }
                        if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                    }
                    if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                }
                //LEVEL 1
                string a_signL = asc_sls.Split('-')[0];
                string a_starL = asc_sls.Split('-')[1];
                string a_subL = asc_sls.Split('-')[2];
                string m_signL = mo_sls.Split('-')[0];
                string m_starL = mo_sls.Split('-')[1];
                string m_subL = mo_sls.Split('-')[2];
                bool bL1 = false, bL2 = false, bL3 = false;
                if (a_signL == m_signL)
                {
                    bL1 = true;
                }
                else
                {
                    string star = getSSSL3(a_signL, mHoro);
                    if (star.Split('|')[2] == m_signL) bL1 = true;
                    else if (star.Split('|')[1] == m_signL) bL1 = true;
                    else if (star.Split('|')[0] == m_signL) bL1 = true;
                    star = getSSSL3(m_signL, mHoro);
                    if (star.Split('|')[2] == a_signL) bL1 = true;
                    else if (star.Split('|')[1] == a_signL) bL1 = true;
                    else if (star.Split('|')[0] == a_signL) bL1 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_signL == star.Split('|')[1] || a_signL == star.Split('|')[2]) bL1 = true;
                    }
                }
                //LEVEL 2
                if (a_signL == m_signL)
                {
                    bL2 = true;
                }
                else
                {
                    string star = getSSSL3(a_starL, mHoro);
                    if (star.Split('|')[2] == m_starL) bL2 = true;
                    else if (star.Split('|')[1] == m_starL) bL2 = true;
                    else if (star.Split('|')[0] == m_starL) bL2 = true;
                    star = getSSSL3(m_starL, mHoro);
                    if (star.Split('|')[2] == a_starL) bL2 = true;
                    else if (star.Split('|')[1] == a_starL) bL2 = true;
                    else if (star.Split('|')[0] == a_starL) bL2 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_starL == star.Split('|')[1] || a_starL == star.Split('|')[2]) bL2 = true;
                    }
                }
                //LEVEL 3
                if (a_signL == m_signL)
                {
                    bL3 = true;
                }
                else
                {
                    string star = getSSSL3(a_starL, mHoro);
                    if (star.Split('|')[2] == m_subL) bL3 = true;
                    else if (star.Split('|')[1] == m_subL) bL3 = true;
                    else if (star.Split('|')[0] == m_subL) bL3 = true;
                    star = getSSSL3(m_starL, mHoro);
                    if (star.Split('|')[2] == a_subL) bL3 = true;
                    else if (star.Split('|')[1] == a_subL) bL3 = true;
                    else if (star.Split('|')[0] == a_subL) bL3 = true;
                    else
                    {
                        star = string.Empty;
                        foreach (var ppos in mHoro.planetsPos)
                        {
                            foreach (var pl in ppos.Value.Split('|'))
                            {
                                if (pl.Split(' ')[1].ToLower() == m_subL.Substring(0, 2))
                                {
                                    var pos = pl.Split(' ')[0].Trim();
                                    //console.log('pos len=' + pos.split('.').length.toString());
                                    int mins = 0;
                                    if (pos.IndexOf('.') > -1 && pos.Split('.')[1] != "")
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60) + ((pos.Split('.')[1] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[1], 10));
                                    else
                                        mins = (zstart[ppos.Key] + ((pos.Split('.')[0] == "0") ? 0 : Convert.ToInt32(pos.Split('.')[0], 10)) * 60);

                                    star = calcSSSL3(mins);
                                    break;
                                }
                            }
                            if (star != string.Empty) break;
                        }
                        if (a_subL == star.Split('|')[1] || a_subL == star.Split('|')[2]) bL3 = true;
                    }
                }
                RecfyBirthTime rcfBT = new RecfyBirthTime();
                if (bL1 && bL2 && bL3)
                {
                    rcfBT.rem = "Birth Time Rectification not required as Ruling Planets agrees";
                    rcfBT.recfyDOB = "";
                }
                else
                {
                    rcfBT.rem = "Birth Time Rectification is required as Ruling Planets did not agree";
                    rcfBT.recfyDOB = "RECTIFY";
                }
                return new JsonResult(rcfBT);
            }
            catch (Exception eX)
            {
                RecfyBirthTime rcfBT = new RecfyBirthTime();
                rcfBT.rem = "An exception has occurred, please report this error to help desk.";
                rcfBT.recfyDOB = string.Format("ERROR:{0}", eX.Message);
                return new JsonResult(rcfBT);
            }
        }
        [HttpGet("GetPlan")]
        public ActionResult GetPlan(string uuid)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var astUsers = db.GetCollection<Plan>("Plan");
                var filter = Builders<Plan>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = astUsers.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var plan = astUsers.Find<Plan>(filter).FirstOrDefault();
                        if (plan.name != "com.mypubz.eportal.astrologer" && plan.credits < 2)
                        {
                            try
                            {
                                var trzc = db.GetCollection<Trendz>("Trendz");
                                var trz = new Trendz
                                {
                                    uuid = uuid,
                                    credits = plan.credits,
                                    date = DateTime.Now.ToShortDateString()
                                };
                                trzc.InsertOne(trz);
                            }
                            catch
                            {
                            }
                        }
                        return new JsonResult(plan);
                    }
                    else
                    {
                        var plan = new Plan
                        {
                            uuid = uuid,
                            name = "com.mypubz.eportal.dob",
                            credits = 100,
                            dobs = ""
                        };
                        astUsers.InsertOne(plan);
                        return new JsonResult(plan);
                    }
                }
                catch (Exception eX)
                {
                    var plan = new Plan
                    {
                        uuid = uuid,
                        name = eX.Message,
                        credits = 5,
                        dobs = ""
                    };
                    return new JsonResult(plan);
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var plan = new Plan
                {
                    uuid = uuid,
                    name = err,
                    credits = 5,
                    dobs = ""
                };
                return new JsonResult(plan);
            }
        }
        [HttpGet("AddDOB")]
        public ActionResult AddDOB(string uuid, string dob)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbPlan = db.GetCollection<Plan>("Plan");
                var filter = Builders<Plan>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbPlan.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var plan = dbPlan.Find<Plan>(filter).FirstOrDefault();
                        string dobs = (plan.dobs == string.Empty) ? dob : string.Format("{0}|{1}", plan.dobs, dob);
                        var update = Builders<Plan>.Update.Set("dobs", dobs);
                        int crds = (plan.credits > 0) ? plan.credits - 1 : plan.credits;
                        update = update.Set("credits", crds);
                        plan = dbPlan.FindOneAndUpdate<Plan>(filter, update);
                        return new JsonResult(plan);
                    }
                    else
                    {
                        var plan = new Plan
                        {
                            uuid = uuid,
                            name = "entry not found",
                            credits = -1,
                            dobs = ""
                        };
                        return new JsonResult(plan);
                    }
                }
                catch (Exception eX)
                {
                    var plan = new Plan
                    {
                        uuid = uuid,
                        name = eX.Message,
                        credits = -1,
                        dobs = ""
                    };
                    return new JsonResult(plan);
                }
            }
            catch (Exception eX)
            {
                var plan = new Plan
                {
                    uuid = uuid,
                    name = eX.Message,
                    credits = -1,
                    dobs = ""
                };
                return new JsonResult(plan);
            }
        }
        [HttpGet("SetPlan")]
        public ActionResult SetPlan(string uuid, string name)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbPlan = db.GetCollection<Plan>("Plan");
                var filter = Builders<Plan>.Filter.Eq("uuid", uuid);
                var update = Builders<Plan>.Update.Set("name", name);
                try
                {
                    long cnt = dbPlan.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        dbPlan.FindOneAndUpdate<Plan>(filter, update);
                        var dbOfr = db.GetCollection<Offer>("Offer");
                        var filter2 = Builders<Offer>.Filter.Eq("uuid", uuid);
                        long cnt2 = dbOfr.CountDocuments(filter2);
                        if (cnt2 > 0L)
                        {
                            var update2 = Builders<Offer>.Update.Set("avail", false);
                            dbOfr.FindOneAndUpdate<Offer>(filter2, update2);
                        }
                    }
                    else
                    {
                        var plan = new Plan
                        {
                            uuid = uuid,
                            name = name,
                            credits = -1
                        };
                        dbPlan.InsertOneAsync(plan);
                    }
                    return new JsonResult("success");
                }
                catch
                {
                    return new JsonResult("failed");
                }
            }
            catch (Exception eX)
            {
                return new JsonResult(string.Format("failed-{0}", eX.Message));
            }
        }
        [HttpGet("AddCredits")]
        public ActionResult AddCredits(string uuid, int credits)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbPlan = db.GetCollection<Plan>("Plan");
                var filter = Builders<Plan>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbPlan.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var plan = dbPlan.Find<Plan>(filter).FirstOrDefault();
                        var update = Builders<Plan>.Update.Set("credits", (plan.credits == -1) ? credits : plan.credits + credits);
                        plan = dbPlan.FindOneAndUpdate<Plan>(filter, update);
                        return new JsonResult(plan);

                    }
                    else
                    {//this should not happen
                        var plan = new Plan
                        {
                            uuid = uuid,
                            name = "entry not found",
                            credits = -1,
                            dobs = ""
                        };
                        return new JsonResult(plan);
                    }
                }
                catch
                {
                    var plan = new Plan
                    {
                        uuid = uuid,
                        name = "exception",
                        credits = -1,
                        dobs = ""
                    };
                    return new JsonResult(plan);
                }
            }
            catch (Exception eX)
            {
                var plan = new Plan
                {
                    uuid = uuid,
                    name = eX.Message,
                    credits = -1,
                    dobs = ""
                };
                return new JsonResult(plan);
            }
        }
        [HttpGet("AddTicket")]
        public ActionResult AddTicket(string uuid, string cat, string sub, string msg)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbTick = db.GetCollection<Ticket>("Ticket");
                var filter = Builders<Ticket>.Filter.Eq("uuid", uuid);
                try
                {
                    Guid guid = Guid.NewGuid();
                    var tick = new Ticket
                    {
                        uuid = uuid,
                        guid = guid.ToString(),
                        cat = cat,
                        sub = sub,
                        msg = msg
                    };
                    dbTick.InsertOneAsync(tick);
                    return new JsonResult(tick);
                }
                catch (Exception eX)
                {
                    var tick = new Ticket
                    {
                        uuid = uuid,
                        guid = "",
                        cat = cat,
                        sub = "failed",
                        msg = eX.Message
                    };
                    return new JsonResult(tick);
                }
            }
            catch (Exception eX)
            {
                var tick = new Ticket
                {
                    uuid = uuid,
                    guid = "",
                    cat = cat,
                    sub = "failed",
                    msg = eX.Message
                };
                return new JsonResult(tick);
            }
        }
        [HttpGet("FollowTicket")]
        public ActionResult FollowTicket(string uuid, string guid, string msg)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbTick = db.GetCollection<Ticket>("Ticket");
                var filter = Builders<Ticket>.Filter.Eq("guid", guid);
                var update = Builders<Ticket>.Update.Set("msg", msg);
                try
                {
                    long cnt = dbTick.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        dbTick.FindOneAndUpdate<Ticket>(filter, update);
                        var dbTickR = db.GetCollection<TicketResp>("TicketResp");
                        var filterR = Builders<TicketResp>.Filter.Eq("guid", guid);
                        var updateR = Builders<TicketResp>.Update.Set("status", "CR");
                        dbTickR.FindOneAndUpdate<TicketResp>(filterR, updateR);
                    }
                    return new JsonResult("success");
                }
                catch (Exception eX)
                {
                    var tick = new Ticket
                    {
                        uuid = uuid,
                        guid = guid,
                        cat = "",
                        sub = "failed",
                        msg = eX.Message
                    };
                    return new JsonResult(tick);
                }
            }
            catch (Exception eX)
            {
                var tick = new Ticket
                {
                    uuid = uuid,
                    guid = guid,
                    cat = "",
                    sub = "failed",
                    msg = eX.Message
                };
                return new JsonResult(tick);
            }
        }
        [HttpGet("GetNotif")]
        public ActionResult GetNotif(string uuid)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
				if (DateTime.Now.Year == 2020)
				{
					try
					{
						//var tick = new TicketResp
						//{
						//	uuid = uuid,
						//	guid = "",
						//	resp = "<div><img src='https://i.imgur.com/kOQaZae.png\' width='100% !important' height='auto'/></div>",
						//	status = "R"
						//};
						var tick = new TicketResp
						{
							uuid = uuid,
							guid = "",
							resp = "<div>IMPORTANT!!!DEAR USER, PLEASE MAKE SURE THAT YOU UPDATE 126 ASTROLOGY APP TO VERSION 126.20.1 or ABOVE AS GOOGLE SERVICE LICENSE WHICH THE APP IS USING HAS EXPIRED, OUR APP IS DEPENDENT ON GOOGLE SERVICE FOR BIRTH PLACE, GEOCODING & TIMEZONE, WE ARE SORRY FOR THIS INCONVENIENCE SHALL ENSURE IS NOT REPEATED AGAIN. THANK YOU FOR YOUR SUPPORT. PLEASE IGNORE IF YOU ALREADY HAS THE LATEST VERSION</div>",
							status = "R"
						};
						return new JsonResult(tick);
					}
					catch
					{
						var tick = new TicketResp
						{
							uuid = uuid,
							guid = "",
							resp = "",
							status = "X"
						};
						return new JsonResult(tick);

					}
				}
				try
				{

					var dbPlan = db.GetCollection<Plan>("Plan");
					var qAstUser =
									(from e in dbPlan.AsQueryable<Plan>()
									 where e.uuid == uuid && e.name.ToLower() == "com.mypubz.eportal.astrologer"
									 select e).ToList().FirstOrDefault();
					if (qAstUser != null)
					{
						var dbSub = db.GetCollection<Subscriber>("Subscriber");
						var filter = Builders<Subscriber>.Filter.Eq("uuid", qAstUser.uuid);
						long cnt = dbSub.CountDocuments(filter);
						if (cnt > 0L)
						{
							var dbTick = db.GetCollection<TicketResp>("TicketResp");
							var tickResp =
											(from e in dbTick.AsQueryable<TicketResp>()
											 where e.uuid == uuid && e.status == "R"
											 select e).ToList().FirstOrDefault();
							if (tickResp != null)
							{
								var update = Builders<TicketResp>.Update.Set("status", "A");
								var filter2 = Builders<TicketResp>.Filter.Eq("guid", tickResp.guid);
								var tick = dbTick.FindOneAndUpdate<TicketResp>(filter2, update);
								return new JsonResult(tickResp);
							}
							else
							{
								var tick = new TicketResp
								{
									uuid = uuid,
									guid = "",
									resp = "",
									status = "X"
								};
								return new JsonResult(tick);
							}
						}
						else
						{
							var tick = new TicketResp
							{
								uuid = uuid,
								guid = "",
								resp = "",
								status = "X"
							};
							return new JsonResult(tick);
						}
					}
					else
					{
						var dbTick = db.GetCollection<TicketResp>("TicketResp");
						var tickResp =
										(from e in dbTick.AsQueryable<TicketResp>()
										 where e.uuid == uuid && e.status == "R"
										 select e).ToList().FirstOrDefault();
						if (tickResp != null)
						{
							var update = Builders<TicketResp>.Update.Set("status", "A");
							var filter2 = Builders<TicketResp>.Filter.Eq("guid", tickResp.guid);
							var tick = dbTick.FindOneAndUpdate<TicketResp>(filter2, update);
							return new JsonResult(tickResp);
						}
						else
						{
							var dbOfr = db.GetCollection<Offer>("Offer");
							var ofr =
											(from e in dbOfr.AsQueryable<Offer>()
											 where e.uuid == uuid && e.impr == 0
											 select e).ToList().FirstOrDefault();
							if (ofr != null)
							{
								var tick = new TicketResp
								{
									uuid = uuid,
									guid = "",
									resp = "Dear User, you can now subscribe for just Rs. 499 & get unlimited access, please download the latest version of the App to get this offer price.",
									status = "R"
								};
								return new JsonResult(tick);
							}
							else
							{
								var tick = new TicketResp
								{
									uuid = uuid,
									guid = "",
									resp = "",
									status = "X"
								};
								return new JsonResult(tick);
							}
						}
					}
				}
				catch (Exception eX)
				{
					var tick = new TicketResp
					{
						uuid = uuid,
						guid = "",
						resp = eX.Message,
						status = "E"
					};
					return new JsonResult(tick);
				}

			}
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var tick = new TicketResp
                {
                    uuid = uuid,
                    guid = "",
                    resp = eX.Message,
                    status = "E"
                };
                return new JsonResult(tick);
            }
        }
        [HttpGet("AddSubscriber")]
        public ActionResult AddSubscriber(string uuid, string nam, string mob, string eml)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbSub = db.GetCollection<Subscriber>("Subscriber");
                var filter = Builders<Subscriber>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbSub.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var sub = new Subscriber
                        {
                            uuid = uuid,
                            nam = "subscriber already exists",
                            mob = mob,
                            eml = eml
                        };
                        return new JsonResult(sub);
                    }
                    else
                    {
                        var sub = new Subscriber
                        {
                            uuid = uuid,
                            nam = nam,
                            mob = mob,
                            eml = eml
                        };
                        dbSub.InsertOne(sub);
                        return new JsonResult(sub);
                    }
                }
                catch (Exception eX)
                {
                    var sub = new Subscriber
                    {
                        uuid = uuid,
                        nam = eX.Message,
                        mob = "",
                        eml = ""
                    };
                    return new JsonResult(sub);
                }
            }
            catch (Exception eX)
            {
                var sub = new Subscriber
                {
                    uuid = uuid,
                    nam = eX.Message,
                    mob = "",
                    eml = ""
                };
                return new JsonResult(sub);
            }
        }
        [HttpGet("Birthinfo")]
        public ActionResult Birthinfo(string dob, string tob, string latlng, string timezone)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, string.Empty, 4);
                mHoro.calc_planets_pos(true, astClient);
                DateTime dtDOB = new DateTime(i3, (int)u2, (int)u1, (int)u4, (int)u5, (int)u6);
                Birth binf = new Birth();
                binf.dob = dtDOB.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                float moonDeg = 0;
                float ascDeg = 0;
                string moonSign = string.Empty, ascSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                bool bmon = false;
                bool basc = false;
                bool bsun = false;
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "AC")
                                    {
                                        ascDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        ascSign = sign;
                                        basc = true;
                                    }
                                    else if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        moonSign = sign;
                                        bmon = true;
                                        // break;
                                    }
                                    else if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        sunSign = sign;
                                        bsun = true;
                                        //break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                sunSign = sign;
                                bsun = true;
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                moonSign = sign;
                                bmon = true;
                            }
                            else if (pls.Split(' ')[1] == "AC")
                            {
                                ascDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                ascSign = sign;
                                basc = true;
                            }
                        }
                        if (bsun && bmon && basc) break;
                    }
                    if (bsun && bmon && basc)
                    {
                        Dictionary<string, string> dctRasL = new Dictionary<string, string>();
                        dctRasL.Add("ar", "Mars");
                        dctRasL.Add("ta", "Venus");
                        dctRasL.Add("ge", "Mercury");
                        dctRasL.Add("cn", "Moon");
                        dctRasL.Add("le", "Sun");
                        dctRasL.Add("vi", "Mercury");
                        dctRasL.Add("li", "Venus");
                        dctRasL.Add("sc", "Mars");
                        dctRasL.Add("sa", "Jupiter");
                        dctRasL.Add("cp", "Saturn");
                        dctRasL.Add("aq", "Saturn");
                        dctRasL.Add("pi", "Jupiter");

                        binf.birth_star = calcStar(moonDeg, moonSign.ToString());
                        binf.star_lord = calcStarL(moonDeg, moonSign.ToString());
                        binf.lagna_lord = dctRasL[ascSign.ToLower()];////calcStarL(Convert.ToDouble(ascDeg), ascSign.ToString());

                        //binf.moon_sign = moonSign;
                        string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                        using (StreamReader r4 = new StreamReader(rJ))
                        {
                            string json4 = r4.ReadToEnd();
                            dynamic rashis = JsonConvert.DeserializeObject(json4);
                            binf.lagna = rashis[ascSign.ToString()].ToString().Split('|')[1].ToString();
                            binf.moon_sign = rashis[moonSign.ToString()].ToString().Split('|')[1].ToString();
                            binf.sun_sign = rashis[sunSign.ToString()].ToString().Split('|')[1].ToString();
                            //int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                        }
                    }

                    string tithi = calcTithi(binf.moon_sign, moonDeg, binf.sun_sign, sunDeg);
                    binf.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    binf.moon_phase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                }
                return new JsonResult(binf);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetAstrologer")]
        public async Task<Astrologer> GetAstrologer(string uuid)
        {
            try
            {
				//Task.Run(() =>
				//{
				var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
				MongoClient client = new MongoClient(connectionString); // connect to localhost
				Console.WriteLine("Getting DB...");
				var db = client.GetDatabase("myypub");
				try
				{
					var dbAst = await Task.Run(() => { return db.GetCollection<Astrologer>("Astrologer"); });
					var qAstUser =
									(from e in dbAst.AsQueryable<Astrologer>()
									 where e.uuid == uuid
									 select e).ToList().FirstOrDefault();
					if (qAstUser != null)
					{
						return qAstUser;
					}
					else
					{
						dbAst = db.GetCollection<Astrologer>("HobbyAstro");
						qAstUser =
										(from e in dbAst.AsQueryable<Astrologer>()
										 where e.uuid == uuid
										 select e).ToList().FirstOrDefault();
						if (qAstUser != null)
						{
							return qAstUser;
						}
						else
						{
							var ast = new Astrologer
							{
								tagline = "This feature is currently available only for our subscribed Astrologers",
								status = "X"
							};
							return ast;
						}
					}

				}
				catch (Exception eX)
				{
					var ast = new Astrologer
					{
						tagline = eX.Message,
						status = "E"
					};
					return ast;
				}
				//});

			}
			catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var ast = new Astrologer
                {
                    tagline = eX.Message,
                    status = "E"
                };
                return ast;
            }
        }
		[HttpGet("AstroBio")]
		public async Task<IActionResult> AstroBio(string uid)
		{
			try
			{
				//Task.Run(() =>
				//{
				var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
				MongoClient client = new MongoClient(connectionString); // connect to localhost
				Console.WriteLine("Getting DB...");
				var db = client.GetDatabase("myypub");
				try
				{
					var dbAst = await Task.Run(() => { return db.GetCollection<AstroBio>("AstroBio"); });
					var qAstUser =
									(from e in dbAst.AsQueryable<AstroBio>()
									 where e.uid == uid
									 select e).ToList().FirstOrDefault();
					if (qAstUser != null)
					{
						return new JsonResult(qAstUser);
					}
					else
					{
						var ast = new AstroBio
						{
							uid = uid,
							bio = "Not Available"
						};
						return new JsonResult(ast);
					}

				}
				catch (Exception eX)
				{
					var ast = new AstroBio
					{
						uid = uid,
						bio = eX.Message
					};
					return new JsonResult(ast);
				}
				//});

			}
			catch (Exception eX)
			{
				var st = new StackTrace(eX, true);
				// Get the top stack frame
				var frame = st.GetFrame(st.FrameCount - 1);
				// Get the line number from the stack frame
				var line = frame.GetFileLineNumber();
				string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
				var ast = new AstroBio
				{
					uid = uid,
					bio = eX.Message
				};
				return new JsonResult(ast);
			}
		}
		[HttpGet("Shadbala")]
		public async Task<IActionResult> Shadbala(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
		{
			Shadbala sdb = new Shadbala();
			try
			{
				string asg = string.Empty, ssg = string.Empty, msg = string.Empty, jsg = string.Empty, mesg = string.Empty, masg = string.Empty, vsg = string.Empty, sasg = string.Empty;
				double sup = 0.0, mop = 0.0, jup = 0.0, mep = 0.0, map = 0.0, vep = 0.0, sap = 0.0;
				Horo horo = null;
				string asc_h = string.Empty;
				string mon_h = string.Empty;
				string sun_h = string.Empty;
				string hou12 = string.Empty;
				string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou11 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
				Task tf = new Task(() =>
				{
					JsonResult jRes = (JsonResult)GetcuspsEx2(dob, tob, latlng, timezone, tzofset, ayanid);
					horo = (Horo)jRes.Value;
					//SUN
					string astClient = Path.Combine(_env.ContentRootPath, @"Content/astroclient");
					foreach (var pls in horo.planetPos)
					{
						foreach (var pl in pls.Value.Split('|'))
						{
							double d = Convert.ToDouble(dmsToDec(Convert.ToInt16(pl.Split(' ')[0].Split('.')[0]), Convert.ToInt16(pl.Split(' ')[0].Split('.')[1]), Convert.ToInt16(pl.Split(' ')[0].Split('.')[1])));
							if (pl.Split(' ')[1] == "AC")
							{
								asg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Su")
							{
								ssg = pls.Key;
								sup = d;
							}
							else if (pl.Split(' ')[1] == "Mo")
							{
								msg = pls.Key;
								mop = d;
							}
							else if (pl.Split(' ')[1] == "Ju")
							{
								jsg = pls.Key;
								jup = d;
							}
							else if (pl.Split(' ')[1] == "Me")
							{
								mesg = pls.Key;
								mep = d;
							}
							else if (pl.Split(' ')[1] == "Ma")
							{
								masg = pls.Key;
								map = d;
							}
							else if (pl.Split(' ')[1] == "Ve")
							{
								vsg = pls.Key;
								vep = d;
							}
							else if (pl.Split(' ')[1] == "Sa")
							{
								sasg = pls.Key;
								sap = d;
							}
						}
					}
					string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
					int r1 = 0, r2 = 0;
					bool asc = false;
					for (r1 = 0; r1 < 12; r1++)
					{
						if (asc)
						{
							r2++;
						}
						if (horo.planetPos.ContainsKey(ras[r1]))
						{
							foreach (string pl in horo.planetPos[ras[r1]].Split('|'))
							{
								if (pl.Split(' ')[1] == "AC")
								{
									asc = true;
									r2++;
									asc_h = ras[r1];
								}
							}
						}
						if (r2 == 2) hou2 = ras[r1];
						else if (r2 == 4) hou4 = ras[r1];
						else if (r2 == 5) hou5 = ras[r1];
						else if (r2 == 6) hou6 = ras[r1];
						else if (r2 == 7) hou7 = ras[r1];
						else if (r2 == 9) hou9 = ras[r1];
						else if (r2 == 10) hou10 = ras[r1];
						else if (r2 == 12)
						{
							hou12 = ras[r1];
							break;
						}
						if (r1 == 11) { hou11 = ras[r1]; r1 = -1; }
					}

				});
				tf.RunSynchronously();
				Task.WaitAll(tf);
				string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
				string rJ = string.Format(@"{0}\exalt.json", astClient);
				dynamic plo;
				using (StreamReader r = new StreamReader(rJ))
				{
					string json = r.ReadToEnd();
					plo = JsonConvert.DeserializeObject(json);

				}
				//SUN
				Task<double> t1 = calcUnchabala("Su", sup, ssg, plo);
				//MOON
				Task<double> t2 = calcUnchabala("Mo", mop, msg, plo);
				//JUPITER
				Task<double> t3 = calcUnchabala("Ju", jup, jsg, plo);
				//VENUS
				Task<double> t4 = calcUnchabala("Ve", vep, vsg, plo);
				//MERCURY
				Task<double> t5 = calcUnchabala("Me", mep, mesg, plo);
				//MARS
				Task<double> t6 = calcUnchabala("Ma", map, masg, plo);
				//SATURN
				Task<double> t7 = calcUnchabala("Sa", sap, sasg, plo);

				double d1 = await t1;
				double d2 = await t2;
				double d3 = await t3;
				double d4 = await t4;
				double d5 = await t5;
				double d6 = await t6;
				double d7 = await t7;
				sdb.uchBala = new Dictionary<string, double>();
				sdb.uchBala.Add("Su", Math.Round(d1));
				sdb.uchBala.Add("Mo", Math.Round(d2));
				sdb.uchBala.Add("Ju", Math.Round(d3));
				sdb.uchBala.Add("Ve", Math.Round(d4));
				sdb.uchBala.Add("Me", Math.Round(d5));
				sdb.uchBala.Add("Ma", Math.Round(d6));
				sdb.uchBala.Add("Sa", Math.Round(d7));
				int[] divs = { 1, 2, 3, 7,9,11,60};
				sdb.sptvBala = new Dictionary<string, double>();
				Parallel.ForEach(divs, async (d) =>
				{
					Task<double> tSu = calcSapvbala("Su", ssg, horo.planetPos, plo["Su"].ToString(), d);
					Task<double> tMo = calcSapvbala("Mo", msg, horo.planetPos, plo["Mo"].ToString(), d);
					Task<double> tJu = calcSapvbala("Ju", jsg, horo.planetPos, plo["Ju"].ToString(), d);
					Task<double> tVe = calcSapvbala("Ve", vsg, horo.planetPos, plo["Ve"].ToString(), d);
					Task<double> tMe = calcSapvbala("Me", mesg, horo.planetPos, plo["Me"].ToString(), d);
					Task<double> tMa = calcSapvbala("Ma", masg, horo.planetPos, plo["Ma"].ToString(), d);
					Task<double> tSa = calcSapvbala("Sa", sasg, horo.planetPos, plo["Sa"].ToString(), d);

					double[] vrp = await Task.WhenAll(tSu, tMo, tJu, tVe, tMe, tMa, tSa);
					lock (sdb)
					{
						if (sdb.sptvBala.ContainsKey("Su"))
						{
							sdb.sptvBala["Su"] += Math.Round(vrp[0]);
						}
						else
						{
							sdb.sptvBala.Add("Su", Math.Round(vrp[0]));
						}
						if (sdb.sptvBala.ContainsKey("Mo"))
						{
							sdb.sptvBala["Mo"] += Math.Round(vrp[1]);
						}
						else
						{
							sdb.sptvBala.Add("Mo", Math.Round(vrp[1]));
						}
						if (sdb.sptvBala.ContainsKey("Ju"))
						{
							sdb.sptvBala["Ju"] += Math.Round(vrp[2]);
						}
						else
						{
							sdb.sptvBala.Add("Ju", Math.Round(vrp[2]));
						}
						if (sdb.sptvBala.ContainsKey("Ve"))
						{
							sdb.sptvBala["Ve"] += Math.Round(vrp[3]);
						}
						else
						{
							sdb.sptvBala.Add("Ve", Math.Round(vrp[3]));
						}
						if (sdb.sptvBala.ContainsKey("Me"))
						{
							sdb.sptvBala["Me"] += Math.Round(vrp[4]);
						}
						else
						{
							sdb.sptvBala.Add("Me", Math.Round(vrp[4]));
						}
						if (sdb.sptvBala.ContainsKey("Ma"))
						{
							sdb.sptvBala["Ma"] += Math.Round(vrp[5]);
						}
						else
						{
							sdb.sptvBala.Add("Ma", Math.Round(vrp[5]));
						}
						if (sdb.sptvBala.ContainsKey("Sa"))
						{
							sdb.sptvBala["Sa"] += Math.Round(vrp[6]);
						}
						else
						{
							sdb.sptvBala.Add("Sa", (int)Math.Round(vrp[6]));
						}
					}

				});
				int[] divs2 = { 1, 9};
				sdb.ojayBala = new Dictionary<string, double>();
				Parallel.ForEach(divs2, async (d) =>
				{
					Task<double> tSu = calcOjhabala("Su", ssg, asg, horo.planetPos, d);
					Task<double> tMo = calcOjhabala("Mo", msg, asg, horo.planetPos, d);
					Task<double> tJu = calcOjhabala("Ju", jsg, asg, horo.planetPos, d);
					Task<double> tVe = calcOjhabala("Ve", vsg, asg, horo.planetPos, d);
					Task<double> tMe = calcOjhabala("Me", mesg, asg, horo.planetPos, d);
					Task<double> tMa = calcOjhabala("Ma", masg, asg, horo.planetPos, d);
					Task<double> tSa = calcOjhabala("Sa", sasg, asg, horo.planetPos, d);

					double[] vrp = await Task.WhenAll(tSu, tMo, tJu, tVe, tMe, tMa, tSa);
					lock (sdb)
					{
						if (sdb.ojayBala.ContainsKey("Su"))
						{
							sdb.ojayBala["Su"] += Math.Round(vrp[0]);
						}
						else
						{
							sdb.ojayBala.Add("Su", Math.Round(vrp[0]));
						}
						if (sdb.ojayBala.ContainsKey("Mo"))
						{
							sdb.ojayBala["Mo"] += Math.Round(vrp[1]);
						}
						else
						{
							sdb.ojayBala.Add("Mo", Math.Round(vrp[1]));
						}
						if (sdb.ojayBala.ContainsKey("Ju"))
						{
							sdb.ojayBala["Ju"] += Math.Round(vrp[2]);
						}
						else
						{
							sdb.ojayBala.Add("Ju", Math.Round(vrp[2]));
						}
						if (sdb.ojayBala.ContainsKey("Ve"))
						{
							sdb.ojayBala["Ve"] += Math.Round(vrp[3]);
						}
						else
						{
							sdb.ojayBala.Add("Ve", Math.Round(vrp[3]));
						}
						if (sdb.ojayBala.ContainsKey("Me"))
						{
							sdb.ojayBala["Me"] += Math.Round(vrp[4]);
						}
						else
						{
							sdb.ojayBala.Add("Me", Math.Round(vrp[4]));
						}
						if (sdb.ojayBala.ContainsKey("Ma"))
						{
							sdb.ojayBala["Ma"] += Math.Round(vrp[5]);
						}
						else
						{
							sdb.ojayBala.Add("Ma", Math.Round(vrp[5]));
						}
						if (sdb.ojayBala.ContainsKey("Sa"))
						{
							sdb.ojayBala["Sa"] += Math.Round(vrp[6]);
						}
						else
						{
							sdb.ojayBala.Add("Sa", Math.Round(vrp[6]));
						}
					}

				});
				Task<double> t8 = calcKenbala("Su", ssg, asg);
				//MOON
				Task<double> t9 = calcKenbala("Mo", msg, asg);
				//JUPITER
				Task<double> t10 = calcKenbala("Ju", jsg, asg);
				//VENUS
				Task<double> t11 = calcKenbala("Ve", vsg, asg);
				//MERCURY
				Task<double> t12 = calcKenbala("Me", mesg, asg);
				//MARS
				Task<double> t13 = calcKenbala("Ma", masg, asg);
				//SATURN
				Task<double> t14 = calcKenbala("Sa", sasg, asg);

				double d8 = await t8;
				double d9 = await t9;
				double d10 = await t10;
				double d11 = await t11;
				double d12 = await t12;
				double d13 = await t13;
				double d14 = await t14;
				sdb.kenBala = new Dictionary<string, double>();
				sdb.kenBala.Add("Su", Math.Round(d8));
				sdb.kenBala.Add("Mo", Math.Round(d9));
				sdb.kenBala.Add("Ju", Math.Round(d10));
				sdb.kenBala.Add("Ve", Math.Round(d11));
				sdb.kenBala.Add("Me", Math.Round(d12));
				sdb.kenBala.Add("Ma", Math.Round(d13));
				sdb.kenBala.Add("Sa", Math.Round(d14));

				string[] lrds = { "Su", "Mo", "Ju", "Ve", "Me", "Ma", "Sa" };
				double[] lpo = { sup, mop, jup, vep, mep, map, sap };
				int i = 0;
				Task<double>[] tk = new Task<double>[7];
				foreach (var lord in lrds)
				{
					tk[i] = calcDrekbala(lord, lpo[i]);
					i++;
				}
				Task.WaitAll(tk);
				sdb.drekBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.drekBala.Add(l, Math.Round(tk[i++].Result));
				i = 0;
				string[] kh  = { hou10, hou4, asc_h, hou4, asc_h, hou10, hou7 };
				string[] lh = { ssg, msg, jsg, vsg, mesg, masg, sasg };
				//Parallel.ForEach(lrds, (lord, state) =>
				foreach(var keh in kh )
				{
					tk[i] = calcDikbala(lh[i], keh);
					i++;
				}
				Task.WaitAll(tk);
				sdb.dikBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.dikBala.Add(l, Math.Round(tk[i++].Result));
				DateTime dtDob = new DateTime(Convert.ToInt32(dob.Split('|')[2]), Convert.ToInt32(dob.Split('|')[1]), Convert.ToInt32(dob.Split('|')[0]), Convert.ToInt32(tob.Split('|')[0]), Convert.ToInt32(tob.Split('|')[1]), Convert.ToInt32(tob.Split('|')[2]));
				Sunrise sR = new Sunrise();
				int jD = sR.getJD(Convert.ToInt32(dob.Split('|')[0]), Convert.ToInt32(dob.Split('|')[1]), Convert.ToInt32(dob.Split('|')[2]));
				//int u7 = Convert.ToInt32(latlng.Split('|')[0].Split('.')[0]);
				//int u8 = Convert.ToInt32(latlng.Split('|')[0].Split('.')[1]);
				//int u9 = Convert.ToInt32(latlng.Split('|')[1].Split('.')[0]);
				//int u10 = Convert.ToInt32(latlng.Split('|')[1].Split('.')[1]);
				double dlat = Convert.ToDouble(latlng.Split('|')[0]);
				double dlng = Convert.ToDouble(latlng.Split('|')[1]);
				TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(TZConvert.IanaToWindows(timezone));
				TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
				tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
				string rise = sR.calcSunriseSet(true, jD, dlat, dlng, tzofset, false);
				string set = sR.calcSunriseSet(false, jD, dlat, dlng, tzofset, false);
				string rise2 = sR.calcSunriseSet(true, jD+1, dlat, dlng, tzofset, false);
				DateTime dt1 = new DateTime(Convert.ToInt32(dob.Split('|')[2]), Convert.ToInt32(dob.Split('|')[1]), Convert.ToInt32(dob.Split('|')[0]), Convert.ToInt32(rise.Split(':')[0]), Convert.ToInt32(rise.Split(':')[1]), 0);
				DateTime dt2 = new DateTime(Convert.ToInt32(dob.Split('|')[2]), Convert.ToInt32(dob.Split('|')[1]), Convert.ToInt32(dob.Split('|')[0])+1, Convert.ToInt32(rise2.Split(':')[0]), Convert.ToInt32(rise2.Split(':')[1]), 0);
				DateTime dt3 = new DateTime(Convert.ToInt32(dob.Split('|')[2]), Convert.ToInt32(dob.Split('|')[1]), Convert.ToInt32(dob.Split('|')[0]), Convert.ToInt32(set.Split(':')[0]), Convert.ToInt32(set.Split(':')[1]), 0);
				TimeSpan ts = dt2.Subtract(dt1);
				TimeSpan ts2 = dt3.Subtract(dt1);
				DateTime mday = dt1.AddHours(ts2.TotalHours / 2);
				DateTime mngt = mday.AddHours(ts.TotalHours / 2);
				i = 0;
				//Parallel.ForEach(lrds, (lord, state) =>
				foreach(var lord in lrds)
				{
					tk[i] = calcNatbala(lord, dtDob, dt1, dt3, mday, mngt);
					i++;
				}
				Task.WaitAll(tk);
				sdb.natoBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.natoBala.Add(l, Math.Round(tk[i++].Result));
				i = 0;
				foreach (var lord in lrds)
				{
					tk[i] = calcTribala(lord, dtDob, dt1, dt3, mday, mngt);
					i++;
				}
				Task.WaitAll(tk);
				sdb.triBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.triBala.Add(l, Math.Round(tk[i++].Result));
				i = 0;
				int tid = 0;
				switch(horo.tithi)
				{
					case "Prathama":
						tid = 1;
						break;
					case "Dwitiya":
						tid = 2;
						break;
					case "Tritiya":
						tid = 3;
						break;
					case "Chaturthi":
						tid = 4;
						break;
					case "Panchami":
						tid = 5;
						break;
					case "Shashthi":
						tid = 6;
						break;
					case "Sapthami":
						tid = 7;
						break;
					case "Asthami":
						tid = 8;
						break;
					case "Navami":
						tid = 9;
						break;
					case "Dasami":
						tid = 10;
						break;
					case "Ekadashi":
						tid = 11;
						break;
					case "Dwadashi":
						tid = 12;
						break;
					case "Trayodashi":
						tid = 13;
						break;
					case "Chaturdashi":
						tid = 14;
						break;
					case "Purnima":
						tid = 15;
						break;
				};

				foreach (var lord in lrds)
				{
					tk[i] = calcPakbala(lord, horo.tithi, tid, horo.moonPhase);
					i++;
				}
				Task.WaitAll(tk);
				sdb.pakBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.pakBala.Add(l, Math.Round(tk[i++].Result));

				Int64 days_since_1Jan1860 = 714404108573;
				DateTime dt1Jan1860 = new DateTime(1860, 1, 1, 0, 0, 0);
				ts = dtDob.Subtract(dt1Jan1860);
				i = 0;
				foreach (var lord in lrds)
				{
					tk[i] = calcVmdhbala(lord, dtDob, dt1, dt2, days_since_1Jan1860 + (long)ts.TotalDays);
					i++;
				}
				Task.WaitAll(tk);
				sdb.hvmaBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.hvmaBala.Add(l, Math.Round(tk[i++].Result));
				i = 0;
				foreach (var lord in lrds)
				{
					tk[i] = calcAyanbala(lord, horo.planetDecl[lord]);
					i++;
				}
				Task.WaitAll(tk);
				sdb.ayanBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.ayanBala.Add(l, Math.Round(tk[i++].Result));
				i = 0;
				foreach (var lord in lrds)
				{
					if (lord != "Su" && lord != "Mo")
					{
						tk[i] = calcChestbala(lord, horo.planetSped[lord]);
						i++;
					}
				}
				Task.WaitAll(tk);
				sdb.chestBala = new Dictionary<string, double>();
				i = 0;
				sdb.chestBala.Add("Su", sdb.ayanBala["Su"]);
				sdb.chestBala.Add("Mo", sdb.pakBala["Mo"]);
				foreach (var l in lrds)
				{
					if (l != "Su" && l != "Mo")
					{
						sdb.chestBala.Add(l, Math.Round(tk[i++].Result));
					}
				}
				i = 0;
				double[] lnb = { 60, 51.4, 34, 42.8, 25.7, 17, 8.7 };
				sdb.naiskBala = new Dictionary<string, double>();
				foreach (var l in lrds)
				{
					sdb.naiskBala.Add(l, lnb[i]);
					i++;
				}
				rJ = string.Format(@"{0}\drgbala.json", astClient);
				string dro;
				using (StreamReader r = new StreamReader(rJ))
				{
					dro = r.ReadToEnd();
				}
				i = 0;
				foreach (var lord in lrds)
				{
					tk[i] = calcDrgbala(lord, lpo[i], lh[i], horo, dro);
					i++;
				}
				Task.WaitAll(tk);
				sdb.drgBala = new Dictionary<string, double>();
				i = 0;
				foreach (var l in lrds)
					sdb.drgBala.Add(l, Math.Round(tk[i++].Result));

				return new JsonResult(sdb);
			}
			catch (Exception eX)
			{
				sdb.uchBala[eX.Message] = -1;
				return new JsonResult(sdb);
			}
		}
		async public Task<double> calcUnchabala(string lord, double pos, string sign, dynamic plo)
		{
			Dictionary<string, int> zods = new Dictionary<string, int>();
			zods["ar"] = 0;
			zods["ta"] = 30;
			zods["ge"] = 60;
			zods["cn"] = 90;
			zods["le"] = 120;
			zods["vi"] = 150;
			zods["li"] = 180;
			zods["sc"] = 210;
			zods["sa"] = 240;
			zods["cp"] = 270;
			zods["aq"] = 300;
			zods["pi"] = 330;

			string json = plo[lord].ToString();
			dynamic pls = JsonConvert.DeserializeObject(json);
			double ed = Convert.ToDouble(pls["ex"].ToString().Split(',')[1]) + zods[pls["ex"].ToString().Split(',')[0]];
			double cd = pos + zods[sign];
			double vpas = Math.Abs((180 - Math.Abs(ed - cd))) / 3;
			return vpas;
		}
		async public Task<double> calcDikbala(string sign, string asn)
		{
			int h = 1;
			if (sign != asn)
			{
				int h1 = calcHno(asn, sign);
				int h2 = calcHno(sign, asn);
				h = Math.Min(h1, h2);
			}
			return (180 - (h - 1) * 30) / 3;
		}

		async public Task<double> calcNatbala(string lord, DateTime dob, DateTime srise, DateTime sset, DateTime mday, DateTime mngt)
		{
			double vpas = 0.0;
			TimeSpan ts = srise.Subtract(mngt);
			if(lord == "Su" || lord == "Ju" || lord == "Ve")
			{
				if (dob > sset) vpas = 0;
				else
				{
					TimeSpan ts2 = sset.Subtract(srise);
					//double mdv = ts2.TotalHours/2;
					int vp1 = (int)Math.Round(60 / ts2.TotalHours);
					if (dob > mday)
					{
						TimeSpan ts1 = dob.Subtract(mday);
						vpas = (60 - Math.Round(ts1.TotalHours)*vp1);
					}
					else
					{
						TimeSpan ts1 = mday.Subtract(dob);
						vpas = (60 - Math.Round(ts1.TotalHours)*vp1);
					}
				}
			}
			else if(lord == "Mo" || lord == "Ma" || lord == "Sa")
			{
				TimeSpan ts2 = mngt.Subtract(srise);
				//double mdv = ts2.TotalHours;
				int vp1 = (int)Math.Round(60 /ts2.TotalHours);
				if (dob > mngt)
				{
					TimeSpan ts1 = dob.Subtract(mngt);
					vpas = (60 - Math.Round(ts1.TotalHours)*vp1);
				}
				else
				{
					TimeSpan ts1 = mngt.Subtract(dob);
					vpas = (60 - Math.Round(ts1.TotalHours)*vp1);
				}
			}
			else if(lord == "Me")
			{
				vpas = 60;
			}
			return vpas;
		}

		async public Task<double> calcTribala(string lord, DateTime dob, DateTime srise, DateTime sset, DateTime mday, DateTime mngt)
		{
			if (lord == "Ju") return 60;
			double vpas = 0.0;
			int dh = (int)Math.Round(mday.Subtract(srise).TotalHours);
			int nh = (int)Math.Round(mngt.Subtract(sset).TotalHours);
			int dp = (int)Math.Round((double)(dh / 3));
			int np = (int)Math.Round((double)(nh / 3));
			if(lord =="Me" || lord == "Su" || lord == "Sa")
			{
				if (dob > sset) vpas = 0;
				else
				{
					if (lord == "Me")
					{
						if (dob >= srise && dob < srise.AddHours(dp)) vpas = 60;
						else vpas = 30;
					}
					else if (lord == "Su")
					{
						if (dob >= srise && dob < srise.AddHours(dp*2)) vpas = 60;
						else vpas = 30;
					}
					else if (lord == "Sa")
					{
						if (dob >= srise && dob < srise.AddHours(dp*3)) vpas = 60;
						else vpas = 30;
					}
				}
			}
			else if (lord == "Mo" || lord == "Ve" || lord == "Ma")
			{
				if (dob < sset) vpas = 0;
				else
				{
					if (lord == "Mo")
					{
						if (dob >= sset && dob < sset.AddHours(np)) vpas = 60;
						else vpas = 30;
					}
					else if (lord == "Ve")
					{
						if (dob >= sset && dob < sset.AddHours(np * 2)) vpas = 60;
						else vpas = 30;
					}
					else if (lord == "Ma")
					{
						if (dob >= sset && dob < sset.AddHours(np * 3)) vpas = 60;
						else vpas = 30;
					}
				}
			}
			return vpas;
		}
		async public Task<double> calcPakbala(string lord, string tithi, int tid, string mph)
		{
			double vpas = 0.0;
			int vp1 = 4;// (int)Math.Round((double)(60 / 15));

			if (lord == "Mo" || lord == "Me" || lord == "Ju" || lord == "Ve")
			{
				if(mph == "waxing") vpas = tid * vp1;
			}
			else
			{
				if (mph == "wanning") vpas = tid * vp1;
			}
			return vpas;
		}
		async public Task<double> calcSapvbala(string lord, string sign, Dictionary<string, string> ppos, string plo, int ndiv)// ing sign, dynamic plo, int lp, int slp)
		{
			double vpas = 0.0;
			//string json = plo[lord].ToString();
			dynamic pls = JsonConvert.DeserializeObject(plo);
			if (ndiv == 1)
			{
				if (sign == pls["mt"].ToString().Split(',')[0]) vpas += 45;
				if (pls["os"].ToString().Contains(sign)) vpas += 30;
			}
			string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
			string sl = string.Empty;
			foreach (var r in ras) {
				if (r.Contains(sign)) sl = r.Split('|')[2];
			}
			if (ndiv != 1)
			{
				JsonResult jOb = (JsonResult)CalcDivChart(ppos, ndiv);
				ppos = (Dictionary<string, string>)(jOb.Value);
			}
			string sls = string.Empty;
			sign = string.Empty;
			bool tf = false;
			if (lord != sl)
			{
				foreach (var pls2 in ppos)
				{
					foreach (var pl in pls2.Value.Split('|'))
					{
						if (pl.Split(' ')[1] == sl)
						{
							sls = pls2.Key;
						}
						else if (pl.Split(' ')[1] == lord)
						{
							sign = pls2.Key;
						}
						if (sls != string.Empty && sign != string.Empty) break;
					}
					if (sls != string.Empty && sign != string.Empty) break;
				}
				int lp = calcHno(sign, sls);
				int slp = calcHno(sls, sign);
				if (lp == 2 || lp == 3 || lp == 4 || lp == 10 || lp == 11 || lp == 12)
				{
					tf = true;
				}
				else if (slp == 2 || slp == 3 || slp == 4 || slp == 10 || slp == 11 || slp == 12)
				{
					tf = true;
				}
			}
			//int rp = Math.Abs(lp - slp);
			int nf = 0;
			Parallel.ForEach(ras, (r, state1) =>
				{
					string[] frs = pls["fr"].ToString().Split(',');
					Parallel.ForEach(frs, (f, state2) =>
					{
						if (r.Contains(f))
						{
							nf = 1;
							state2.Break();
						}
					});
					if (nf == 1) state1.Break();
				});
				if (nf == 0)
				{
					Parallel.ForEach(ras, (r, state1) =>
					{
						string[] ers = pls["en"].ToString().Split(',');
						Parallel.ForEach(ers, (e, state2) =>
						{
							if (r.Contains(e))
							{
								nf = 2;
								state2.Break();
							}
						});
						if (nf == 2) state1.Break();
					});

				}
				if (tf && nf == 1) vpas += 22.5;
				else if (tf && nf == 0) vpas += 15;
				else if (!tf && nf == 1) vpas += 15;
				else if (tf && nf == 2) vpas += 7.5;
				else if (!tf && nf == 0) vpas += 3.75;
				else if (!tf && nf == 2) vpas += 1.875;

			return vpas;

		}

		async public Task<double> calcOjhabala(string lord, string sign, string asn, Dictionary<string, string> ppos, int ndiv)
		{
		   double vpas = 0.0;
		   if(ndiv == 1)
		   {
				int hno = calcHno(asn, sign);
				if (((hno % 2) == 0) && (lord == "Mo" || lord == "Ve")) vpas += 15;
				else if (((hno % 2) != 0) && (lord != "Mo" && lord != "Ve")) vpas += 15;
		   }
		   else
		   {
				string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
				JsonResult jOb = (JsonResult)CalcDivChart(ppos, ndiv);
				ppos = (Dictionary<string, string>)(jOb.Value);
				asn = string.Empty;
				sign = string.Empty;
				Parallel.For(0, 12, (r1, state) =>
				{
				  if (ppos.ContainsKey(ras[r1]))
				  {
					  foreach (string pl in ppos[ras[r1]].Split('|'))
					  {
						  if (pl.Split(' ')[1] == "AC")
						  {
							  asn = ras[r1];
						  }
						  else if (pl.Split(' ')[1] == lord)
						  {
							  sign = ras[r1];
						  }
						  if (asn != string.Empty && sign != string.Empty) break;
					  }
				  }
				  if (asn != string.Empty && sign != string.Empty) state.Break();
			  });
				int hno = calcHno(asn, sign);
				if (((hno % 2) == 0) && (lord == "Mo" || lord == "Ve")) vpas += 15;
				else if (((hno % 2) != 0) && (lord != "Mo" && lord != "Ve")) vpas += 15;
			}
			return vpas;
		}
		async public Task<double> calcKenbala(string lord, string sign, string asn)
		{
			double vpas = 0.0;
			int hno = calcHno(asn, sign);
			if (hno == 1 || hno == 4 || hno == 7 || hno == 10) vpas = 60;
			else if (hno == 2 || hno == 5 || hno == 8 || hno == 11) vpas = 30;
			else if (hno == 12 || hno == 3 || hno == 6 || hno == 9) vpas = 15;
			return vpas;
		}

		async public Task<double> calcDrekbala(string lord, double pos)
		{
			double vrps = 0.0;
			switch(lord)
			{
				case "Su":
				case "Ma":
				case "Ju":
					if (pos >= 0.0 && pos < 10.0) vrps = 15;
					break;
				case "Mo":
				case "Ve":
					if (pos >= 10.0 && pos < 20.0) vrps = 15;
					break;
				case "Sa":
				case "Me":
					if (pos >= 20.0 && pos < 30.0) vrps = 15;
					break;
				default:
					break;
			}
			return vrps;
		}
		async public Task<double> calcVmdhbala(string lord, DateTime dob, DateTime srise1, DateTime srise2, Int64 doc)
		{
			double vpas = 0.0;
			string[] dL = { "Su", "Mo", "Ma", "Me", "Ju", "Ve", "Sa" };
			long lr = 0;
			long q = Math.DivRem(doc, 60, out lr);
			q = Math.DivRem(((q * 3) +1), 7, out lr);
			if (lord == dL[lr-1]) vpas += 15;
			q = Math.DivRem(doc, 30, out lr);
			long q2 = Math.DivRem((long)((q * 2) + 1), 7, out lr);
			if (lord == dL[lr]) vpas += 30;
			q = Math.DivRem(doc, 7, out lr);
			if (lord == dL[lr]) vpas += 45;
			TimeSpan ts = srise2.Subtract(srise1);
			int hora = (int)Math.Round(ts.TotalHours / 24);
			int bh = (int)Math.Round((double)(dob.Hour + dob.Minute / 60));
			int tik = 1;
			while (bh < hora * tik++) ;
			if (lord == dL[(hora * (tik - 1))-1]) vpas += 60;
			return vpas;
		}
		async public Task<double> calcAyanbala(string lord, string decl)
		{
			double vpas = 0.0;
			//const string MY_ODEGREE_STRIN = "°";
			int deg = Convert.ToInt32(decl.Split('°')[0]);
			int min = Convert.ToInt32(decl.Split('°')[1].Split('\'')[0]);
			int sec = Convert.ToInt32(decl.Split('°')[1].Split('\'')[1].Split('.')[0]);
			double ddec = dmsToDec(deg, min, sec);
			if(lord == "Me")
			{
				vpas = (23.27 + Math.Abs(ddec)) * 1.2793;
			}
			else if (lord == "Mo" || lord == "Sa")
			{
				vpas = (ddec < 0) ? 23.27 + (Math.Abs(ddec) * 1.2793) : 23.27 - (Math.Abs(ddec) * 1.2793);
			}
			else if (lord == "Su" || lord == "Ma" || lord == "Ju" || lord == "Ve")
			{
				vpas = (ddec > 0) ? 23.27 + (Math.Abs(ddec) * 1.2793) : 23.27 - (Math.Abs(ddec) * 1.2793);
			}
			return vpas;
		}

		async public Task<double> calcChestbala(string lord, string sped)
		{
			double vpas = 0.0;
			int deg = Convert.ToInt32(sped.Split('°')[0]);
			int min = Convert.ToInt32(sped.Split('°')[1].Split('\'')[0]);
			int sec = Convert.ToInt32(sped.Split('°')[1].Split('\'')[1].Split('.')[0]);
			double ddec = dmsToDec(deg, min, sec);
			//double rx = 0.0;
			if (ddec < 0) vpas = 60;
			else
			{
				switch (lord)
				{
					case "Ma":
						if (ddec > dmsToDec(0, 40, 0)) vpas = 45;
						else if (ddec < dmsToDec(0, 30, 0)) vpas = 30;
						else if (ddec > dmsToDec(0, 37, 0)) vpas = 7.5;
						else vpas = 15;
						break;
					case "Ju":
						if (ddec > dmsToDec(0, 10, 0)) vpas = 45;
						else if (ddec < dmsToDec(0, 5, 0)) vpas = 30;
						else if (ddec > dmsToDec(0, 4, 52)) vpas = 7.5;
						else vpas = 15;
						break;
					case "Me":
						if (ddec > dmsToDec(1, 30, 0)) vpas = 45;
						else if (ddec < dmsToDec(1, 0, 0)) vpas = 30;
						else if (ddec > dmsToDec(0, 59, 8)) vpas = 7.5;
						else vpas = 15;
						break;
					case "Ve":
						if (ddec > dmsToDec(1, 10, 0)) vpas = 45;
						else if (ddec < dmsToDec(0, 50, 0)) vpas = 30;
						else if (ddec > dmsToDec(0, 59, 8)) vpas = 7.5;
						else vpas = 15;
						break;
					case "Sa":
						if (ddec > dmsToDec(0, 5, 0)) vpas = 45;
						else if (ddec < dmsToDec(0, 2, 0)) vpas = 30;
						else if (ddec > dmsToDec(0, 2, 1)) vpas = 7.5;
						else vpas = 15;
						break;
					default:
						break;
				}
			}
			return vpas;
		}
		async public Task<double> calcNiskbala(string lord, double lvp)
		{
			return (8.571 * lvp);
		}
		async public Task<double> calcDrgbala(string lord, double pos, string sign, Horo horo, string dro )
		{
			dynamic plo = JsonConvert.DeserializeObject(dro);
			double vpas = 0.0;
			Dictionary<string, int> zods = new Dictionary<string, int>();
			zods["ar"] = 0;
			zods["ta"] = 30;
			zods["ge"] = 60;
			zods["cn"] = 90;
			zods["le"] = 120;
			zods["vi"] = 150;
			zods["li"] = 180;
			zods["sc"] = 210;
			zods["sa"] = 240;
			zods["cp"] = 270;
			zods["aq"] = 300;
			zods["pi"] = 330;
			JsonResult jO = (JsonResult)GetAspectsEx(horo, sign);
			Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
			double zp = zods[sign] + pos;
			foreach(var a in dctAsp)
			{
				if (a.Key == 1) continue;
				foreach(var b in a.Value.Split('|'))
				{
					double p = zods[b.Split(' ')[1].Split('&')[1]] + dmsToDec(Convert.ToInt16(b.Split(' ')[0].Split('.')[0]), Convert.ToInt16(b.Split(' ')[0].Split('.')[1]), Convert.ToInt16(b.Split(' ')[0].Split('.')[2]));

					double ld = Math.Abs(zp - p);
					for(int zs = 0; zs < 360; zs += 30)
					{
						if(ld >= zs && ld < zs + 30)
						{
							string json = plo[string.Format("{0}", (int)Math.Round(ld - zs))].ToString();
							dynamic pls = JsonConvert.DeserializeObject(json);
							vpas += Convert.ToDouble(pls[string.Format("{0}-{1}", zs, zs + 30)]);
						}
					}
				}
			}
			return vpas;
		}

		async public Task<Dictionary<string, string>> calcGrahyudh(Horo horo)
		{
			Dictionary<string, string> dctYud = new Dictionary<string, string>();

			return dctYud;
		}
		[HttpGet("AstrologerStatus")]
        public ActionResult AstrologerStatus(string uuid, string status)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbAst = db.GetCollection<Astrologer>("Astrologer");
                var filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbAst.Count(filter);
                    if (cnt > 0L)
                    {
                        var update = Builders<Astrologer>.Update.Set("status", status);
                        dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                    }
                    else
                    {
                        dbAst = db.GetCollection<Astrologer>("HobbyAstro");
                        filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                        cnt = dbAst.Count(filter);
                        if (cnt > 0L)
                        {
                            var update = Builders<Astrologer>.Update.Set("status", status);
                            dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                        }
                    }

                    return new JsonResult("success");
                }
                catch (Exception eX)
                {
                    var ast = new Astrologer
                    {
                        uuid = uuid,
                        tagline = eX.Message,
                        status = "E"
                    };
                    return new JsonResult(ast);
                }
            }
            catch (Exception eX)
            {
                var ast = new Astrologer
                {
                    uuid = uuid,
                    tagline = eX.Message,
                    status = "E"
                };
                return new JsonResult(ast);
            }
        }
        [HttpGet("AstrologerTagline")]
        public ActionResult AstrologerTagline(string uuid, string tagline)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbAst = db.GetCollection<Astrologer>("Astrologer");
                var filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbAst.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var update = Builders<Astrologer>.Update.Set("tagline", tagline);
                        dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                    }
                    else
                    {
                        dbAst = db.GetCollection<Astrologer>("HobbyAstro");
                        filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                        cnt = dbAst.CountDocuments(filter);
                        if (cnt > 0L)
                        {
                            var update = Builders<Astrologer>.Update.Set("tagline", tagline);
                            dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                        }
                    }

                    return new JsonResult("success");
                }
                catch (Exception eX)
                {
                    var ast = new Astrologer
                    {
                        uuid = uuid,
                        tagline = eX.Message,
                        status = "E"
                    };
                    return new JsonResult(ast);
                }
            }
            catch (Exception eX)
            {
                var ast = new Astrologer
                {
                    uuid = uuid,
                    tagline = eX.Message,
                    status = "E"
                };
                return new JsonResult(ast);
            }
        }
        [HttpGet("AstrologerAvatar")]
        public ActionResult AstrologerAvatar(string uuid, string avatar)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbAst = db.GetCollection<Astrologer>("Astrologer");
                var filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = dbAst.Count(filter);
                    if (cnt > 0L)
                    {
                        var update = Builders<Astrologer>.Update.Set("avatar", avatar);
                        dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                    }
                    else
                    {
                        dbAst = db.GetCollection<Astrologer>("HobbyAstro");
                        filter = Builders<Astrologer>.Filter.Eq("uuid", uuid);
                        cnt = dbAst.Count(filter);
                        if (cnt > 0L)
                        {
                            var update = Builders<Astrologer>.Update.Set("avatar", avatar);
                            dbAst.FindOneAndUpdate<Astrologer>(filter, update);
                        }
                    }

                    return new JsonResult("success");
                }
                catch (Exception eX)
                {
                    var ast = new Astrologer
                    {
                        uuid = uuid,
                        tagline = eX.Message,
                        status = "E"
                    };
                    return new JsonResult(ast);
                }
            }
            catch (Exception eX)
            {
                var ast = new Astrologer
                {
                    uuid = uuid,
                    tagline = eX.Message,
                    status = "E"
                };
                return new JsonResult(ast);
            }
        }
        [HttpGet("GetAllAstrologers")]
        public async Task<List<Astrologer>> GetAllAstrologers()
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                try
                {
					var dbAst = await Task.Run(() => { return db.GetCollection<Astrologer>("Astrologer"); });
                    var qAstUser =
                                    (from e in dbAst.AsQueryable<Astrologer>()
                                     select e).ToList();
                    if (qAstUser != null)
                    {
                        return qAstUser;
                    }
                    else
                    {
						List<Astrologer> lst = new List<Astrologer>();
                        var ast = new Astrologer
                        {
                            status = "X"
                        };
                        lst.Add(ast);
						return lst;
                    }
                }
                catch (Exception eX)
                {
					List<Astrologer> lst = new List<Astrologer>();
					var ast = new Astrologer
                    {
                        tagline = eX.Message,
                        status = "E"
                    };
					lst.Add(ast);
                    return lst;
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var ast = new Astrologer
                {
                    tagline = eX.Message,
                    status = "E"
                };
				List<Astrologer> lst = new List<Astrologer>();
				lst.Add(ast);
				return lst;
            }
        }
        [HttpGet("PrashnaJyotish")]
        public ActionResult PrashnaJyotish(string dob, string tob, string latlng, string timezone, int znum)
        {
            try
            {
                //DBLog("PrashnaJyotish");
                Dictionary<string, int> zstart = new Dictionary<string, int>();
                zstart["ar"] = 0;
                zstart["ta"] = 30;
                zstart["ge"] = 60;
                zstart["cn"] = 90;
                zstart["le"] = 120;
                zstart["vi"] = 150;
                zstart["li"] = 180;
                zstart["sc"] = 210;
                zstart["sa"] = 240;
                zstart["cp"] = 270;
                zstart["aq"] = 300;
                zstart["pi"] = 330;
                Dictionary<string, string> dlord = new Dictionary<string, string>();
                dlord["SUNDAY"] = "SUN";
                dlord["MONDAY"] = "MOON";
                dlord["TUESDAY"] = "MARS";
                dlord["WEDNESDAY"] = "MERCURY";
                dlord["THURSDAY"] = "JUPITER";
                dlord["FRIDAY"] = "VENUS";
                dlord["SATURDAY"] = "SATURN";
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				int ayanid = 3;
                string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                DateTime dtDOB = new DateTime(i3, (int)u2, (int)u1, (int)u4, (int)u5, (int)u6);
                string zdb = dtDOB.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                string rlord = dlord[zdb.Split(',')[0].ToUpper()];
                string zssl = getSSSL(znum);
                string asc_sls = string.Empty;
                string mo_sls = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            //string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + dmsToDec(Convert.ToInt32(pdg[0]), Convert.ToInt32(pdg[1]), Convert.ToInt32(pdg[2]));// Convert.ToDouble(p);
                            asc_sls = getSUBZ(ppos.Key, plpos);
                        }
                        else if (pl.Split(' ')[1] == "Mo")
                        {
                            string[] pdg = pl.Split(' ')[0].Split('.');
                            //string p = string.Format("{0}.{1}", pdg[0], pdg[1]);
                            double plpos = zstart[ppos.Key] + dmsToDec(Convert.ToInt32(pdg[0]), Convert.ToInt32(pdg[1]), Convert.ToInt32(pdg[2]));//Convert.ToDouble(p);
                            mo_sls = getSUBZ(ppos.Key, plpos);
                        }
                        if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                    }
                    if (asc_sls != string.Empty && mo_sls != string.Empty) break;
                }
                Prashna pras = new Prashna();
                pras.znum = znum;
                pras.ascSSSL = asc_sls;
                pras.moSSSL = mo_sls;
                pras.dayL = rlord;
                pras.praSSSL = zssl;
                pras.interr = "";
                bool basc = false;
                bool bmo = false;
                foreach (var asc in asc_sls.Split(','))
                {
                    if (zssl.Split('|')[2] == asc.Split('-')[2])
                    {
                        pras.answer = "YES";
                        basc = true;
                    }
                }
                foreach (var mo in mo_sls.Split(','))
                {
                    if (zssl.Split('|')[2] == mo.Split('-')[2])
                    {
                        pras.answer = "YES";
                        basc = true;
                    }
                }
                if (bmo && basc)
                    pras.remarks = string.Format("The sublord {0} of your selected number {1} has matched with Ruling Planet Moon's sublord, hence your question in context will be fulfilled", zssl.Split('|')[2], znum);
                else if (basc)
                    pras.remarks = string.Format("The sublord {0} of your selected number {1} has matched with Ruling Planet Ascendant sublord, hence your question in context will be fulfilled", zssl.Split('|')[2], znum);
                else if (bmo)
                    pras.remarks = string.Format("The sublord {0} of your selected number {1} has matched with Ruling Planet Moon's sublord, hence your question in context will be fulfilled", zssl.Split('|')[2], znum);
                else
                {
                    pras.remarks = string.Format("The sublord {0} of your selected number {1} did not match with Ruling Planet sublord, hence your question in context may not be not fulfilled at the moment", zssl.Split('|')[2], znum);
                    pras.answer = "NO";
                }

                return new JsonResult(pras);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Prashna pras = new Prashna();
                pras.interr = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                return new JsonResult(pras);

            }
        }
        public string getSSSL(int znum)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                using (StreamReader r3 = new StreamReader(string.Format(@"{0}\sublords.json", astClient)))
                {
                    string json = r3.ReadToEnd();
                    List<SubLord> sublords = JsonConvert.DeserializeObject<List<SubLord>>(json);
                    string subz = string.Empty;
                    int n = 0;
                    foreach (var item in sublords)
                    {
                        n++;
                        if (n == znum) return item.sign + '|' + item.star + '|' + item.sub;
                    }
                    return "ERROR";
                }
            }
            catch (Exception eX)
            {
                return eX.Message;
            }
        }
        [HttpGet("GetTransPreds")]
        public ActionResult GetTransPreds(string dob, string tob)
        {
            try
            {
                string latlng = "17.23|78.29";
                string timezone = "India Standard Time";
                //DBLog(string.Format("GetTransPreds-{0}", dob));
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                //string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                //string ayan = string.Empty;
                //using (StreamReader r = new StreamReader(sF))
                //{
                //    string jsa = r.ReadToEnd();
                //    dynamic ayans = JsonConvert.DeserializeObject(jsa);
                //    ayan = ayans[dob.Split('|')[2]].ToString();
                //}
                // JsonResult json1 = (JsonResult)BirthstarEx(dob, tob, ayan);
                JsonResult json1 = (JsonResult)GetBirthstar(dob, tob, latlng, "Asia/Calcutta", 4);
                BirthStar cStar = (BirthStar)json1.Value;
                Dictionary<string, string> dctPreds = new Dictionary<string, string>();
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                DateTime today = DateTime.Now;
                DateTime eday = DateTime.Now.AddDays(30);
                string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                while (today <= eday)
                {
                    //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    //calcStar(tday);
                    //JsonResult json1 = (JsonResult)Birthstar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute));
                    Horoscope mHoro = new Horoscope();
                    uint u1 = Convert.ToUInt32(today.Day);
                    uint u2 = Convert.ToUInt32(today.Month);
                    int i3 = Convert.ToInt32(today.Year);
                    uint u4 = 5;//Convert.ToUInt32(today.Hour);
                    uint u5 = 0;//Convert.ToUInt32(today.Minute);
                    uint u6 = 0;
					double u7 = Convert.ToDouble(latlng.Split('|')[0]);
					double u8 = Convert.ToDouble(latlng.Split('|')[1]);
					mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, timezone, false, string.Empty);
                    mHoro.calc_planets_pos(true, astClient);
                    int rpos = 0;
                    foreach (string sign in signs)
                    {
                        rpos++;
                        if (mHoro.planetsPos.ContainsKey(sign))
                        {
                            var pls = mHoro.planetsPos[sign].Split('|');
                            //var ePls = '';
                            //var mnode = '';
                            for (var k = 0; k < pls.Length; k++)
                            {
                                if (pls[k].Split(' ')[1] == "MEAN_NODE")
                                {
                                    var kpos = rpos + 6;
                                    if (kpos > 12) kpos = (kpos - 12);
                                    //var mn = i + 11;
                                    //if (mn > 15) mn -= 15;
                                    if (mHoro.planetsPos.ContainsKey(signs[kpos - 1]))
                                    {
                                        var eP = mHoro.planetsPos[signs[kpos - 1]];
                                        mHoro.planetsPos[signs[kpos - 1]] = eP + '|' + pls[k].Split(' ')[0] + ' ' + "Ke";
                                    }
                                    else
                                    {
                                        mHoro.planetsPos[signs[kpos - 1]] = pls[k].Split(' ')[0] + ' ' + "Ke";
                                    }
                                    // plPos[sign] = ePls;
                                    mHoro.planetsPos[sign] = mHoro.planetsPos[sign].Replace("MEAN_NODE", "Ra");
                                }

                            }
                            pls = mHoro.planetsPos[sign].Split('|');
                            for (var k = 0; k < pls.Length; k++)
                            {
                                string retro = "D";
                                string pl = pls[k].Split(' ')[1];
                                if (mHoro.retroPls.Contains(pl)) retro = "R";
                                if (dctPls.ContainsKey(pls[k].Split(' ')[1]))
                                {
                                    if (dctPls[pls[k].Split(' ')[1]].IndexOf(sign) < 0)
                                        dctPls[pls[k].Split(' ')[1]] += string.Format("|{0}-{1}-{2}", sign, String.Format(ci, "{0:MMM dd}", today), retro);
                                }
                                else
                                    dctPls[pls[k].Split(' ')[1]] = string.Format("{0}-{1}-{2}", sign, String.Format(ci, "{0:MMM dd}", today), retro);
                            }
                        }
                    }
                    today = today.AddDays(1);
                }
                string rJ = string.Format(@"{0}\signs_short.json", astClient);
                string rJ2 = string.Format(@"{0}\o_rashis.json", astClient);
                using (StreamReader r3 = new StreamReader(rJ2))
                using (StreamReader r4 = new StreamReader(rJ))
                {
                    string json3 = r3.ReadToEnd();
                    string json4 = r4.ReadToEnd();
                    dynamic rashis_short = JsonConvert.DeserializeObject(json4);
                    dynamic rashis = JsonConvert.DeserializeObject(json3);
                    foreach (var pred in dctPls)
                    {
                        string pras = pred.Value;
                        string spred = string.Empty;
                        int toks = 0;
                        foreach (var ras in pras.Split('|'))
                        {
                            int pos = GetPosFMon(rashis_short[cStar.birthSign.ToLower()].ToString(), ras.Split('-')[0]);
                            if (pred.Key == "Su" || pred.Key == "Mo" || pred.Key == "Ve" || pred.Key == "Ju" || pred.Key == "Sa" || pred.Key == "Me" || pred.Key == "Ma" || pred.Key == "Ra" || pred.Key == "Ke")
                            {
                                string pky = string.Format(@"{0}\{1}.json", astClient, (pred.Key == "Ra") ? "Ma" : (pred.Key == "Ke") ? "Sa" : pred.Key);
                                using (StreamReader r5 = new StreamReader(pky))
                                {
                                    string json5 = r5.ReadToEnd();
                                    dynamic plpreds = JsonConvert.DeserializeObject(json5);
                                    if (toks > 0)
                                    {
                                        if (ras.Split('-')[2] == "D")
                                            spred += string.Format("\nOn {0} {1} moves to {2} which is {3} house, during this time {4}", ras.Split('-')[1], dctPlNames[pred.Key], rashis[ras.Split('-')[0].ToLower()].ToString().Split('|')[1], pos, plpreds[pos.ToString()]);
                                        else
                                            spred += string.Format("\nOn {0} {1} turns retrograde & moves to {2} which is {3} house, during this time {4}", ras.Split('-')[1], dctPlNames[pred.Key], rashis[ras.Split('-')[0].ToLower()].ToString().Split('|')[1], pos, plpreds[pos.ToString()]);
                                    }
                                    else
                                    {
                                        spred = string.Format("{0} is transiting in {1} which is {2} house, during this time {3}", dctPlNames[pred.Key], rashis[ras.Split('-')[0].ToLower()].ToString().Split('|')[1], pos, plpreds[pos.ToString()]);
                                    }
                                }
                                toks++;
                            }
                        }
                        if (toks > 0)
                            dctPreds[dctPlNames[pred.Key]] = spred;
                    }
                }
                return new JsonResult(dctPreds);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetBirthstar")]
        public ActionResult GetBirthstar(string dob, string tob, string latlng, string timezone, int ayanid)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                //DBLog("GetBirthstar");
                //string latlng = "17.23|78.29";
                //string timezone = "India Standard Time";
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, astClient);
                BirthStar bStar = new BirthStar();
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            double moonDeg = 0.0;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = dmsToDec(Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]), Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]), Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]));
                                        bStar.birthSignDeg = pl.Split(' ')[0];
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = dmsToDec(Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]), Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]), Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]));
                                bStar.birthSignDeg = pls.Split(' ')[0];
                                bmon = true;
                            }
                            if (bmon)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    bStar.birthStar = nak.name;
                                                    bStar.birthSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            float sunDeg = 0;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bStar.sunDeg = pl.Split(' ')[0];
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bStar.sunDeg = pls.Split(' ')[0];
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    bStar.sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                }

                return new JsonResult(bStar);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        public int GetPosFMon(string bsign, string psign)
        {
            bsign = bsign.ToLower();
            psign = psign.ToLower();
            int pos = 0;
            string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
            for (int i = 0; i < 12; i++)
            {
                if (pos > 0) pos++;
                if (signs[i] == bsign)
                {
                    pos++;
                }
                if (pos > 0 && signs[i] == psign) break;
                if (i == 11) i = -1;
            }
            return pos;
        }
        [HttpGet("GetMoonPhase")]
        public ActionResult GetMoonPhase(string dob, string tob, string latlng, string timezone)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, TZConvert.IanaToWindows(timezone), false, string.Empty);
                mHoro.calc_planets_pos(false, astClient);
                mHoro.calc_houses();
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                horo.housePos = mHoro.housePos;
                horo.ascPos = mHoro.ascPos;
                float moonDeg = 0;
                string moonSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        //moonDeg = float.Parse(string.Format("{0}.{1}", sm.Split('.')[0], sm.Split('.')[1]));
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                moonSign = calcStar(moonDeg, sign.ToString());
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                    string tithi = calcTithi(moonSign, moonDeg, sunSign, sunDeg);
                    horo.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    horo.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                    horo.tithiRem = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[2] : "";
                }
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (horo.planetPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = horo.planetPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }
                        }
                    }
                }
                horo.planetPos = dctPls;
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetMoonPhaseEx")]
        public ActionResult GetMoonPhaseEx(string dob, string tob, string latlng, string timezone, int ayanid)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				//TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
				// TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
				//double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
				//string ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_houses();
                mHoro.calc_planets_pos(true, astClient);
                // mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, u9, u10, TZConvert.IanaToWindows(timezone), true, ayan);
                //mHoro.calc_planets_pos(false);
                //mHoro.calc_houses();
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                horo.housePos = mHoro.housePos;
                horo.ascPos = mHoro.ascPos;
                float moonDeg = 0;
                string moonSign = string.Empty;
                float sunDeg = 0;
                string sunSign = string.Empty;
                string sF = string.Format(@"{0}\o_short_signs.json", astClient);
                using (StreamReader r = new StreamReader(sF))
                {
                    string json = r.ReadToEnd();
                    dynamic signs = JsonConvert.DeserializeObject(json);
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bmon = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Mo")
                                    {
                                        moonDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        //moonDeg = float.Parse(string.Format("{0}.{1}", sm.Split('.')[0], sm.Split('.')[1]));
                                        bmon = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Mo")
                            {
                                moonDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bmon = true;
                            }
                            if (bmon)
                            {
                                moonSign = calcStar(moonDeg, sign.ToString());
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    int rashi_num = Convert.ToInt32(rashis[sign.ToString()].ToString().Split('|')[0]);
                                    string nJ = string.Format(@"{0}\nakshatras.json", astClient);
                                    using (StreamReader r2 = new StreamReader(nJ))
                                    {
                                        string json2 = r2.ReadToEnd();
                                        List<Star> nakshatras = JsonConvert.DeserializeObject<List<Star>>(json2);
                                        foreach (var nak in nakshatras)
                                        {
                                            string[] snak = nak.location.start.Split(',')[0].Split('.');
                                            double nakd1 = dmsToDec(Convert.ToInt32(snak[0]), Convert.ToInt32(snak[1]), 0);
                                            string[] enak = nak.location.end.Split(',')[0].Split('.');
                                            double nakd2 = dmsToDec(Convert.ToInt32(enak[0]), Convert.ToInt32(enak[1]), 0);
                                            if (nak.location.start.Split(',')[1] == sign.ToString() && nak.location.end.Split(',')[1] == sign.ToString())
                                            {
                                                if (moonDeg >= nakd1 && moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.start.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg >= nakd1)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                            else if (nak.location.end.Split(',')[1] == sign.ToString().ToLower())
                                            {
                                                if (moonDeg <= nakd2)
                                                {
                                                    horo.birthStar = nak.name;
                                                    moonSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    foreach (var sign in signs)
                    {
                        if (mHoro.planetsPos.ContainsKey(sign.ToString()))
                        {
                            bool bsun = false;
                            string pls = mHoro.planetsPos[sign.ToString()].ToString();
                            if (pls.Contains('|') == true)
                            {
                                foreach (string pl in pls.Split('|'))
                                {
                                    if (pl.Split(' ')[1] == "Su")
                                    {
                                        sunDeg = (Convert.ToInt32(pl.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pl.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pl.Split(' ')[0].Split('.')[2]) / 3600);
                                        bsun = true;
                                        break;
                                    }
                                }
                            }
                            else if (pls.Split(' ')[1] == "Su")
                            {
                                sunDeg = (Convert.ToInt32(pls.Split(' ')[0].Split('.')[0]) + Convert.ToInt32(pls.Split(' ')[0].Split('.')[1]) / 60 + Convert.ToInt32(pls.Split(' ')[0].Split('.')[2]) / 3600);
                                bsun = true;
                            }
                            if (bsun)
                            {
                                string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                                using (StreamReader r4 = new StreamReader(rJ))
                                {
                                    string json4 = r4.ReadToEnd();
                                    dynamic rashis = JsonConvert.DeserializeObject(json4);
                                    sunSign = rashis[sign.ToString()].ToString().Split('|')[1].ToString();
                                }
                                break;
                            }
                        }
                    }
                    string tithi = calcTithi(moonSign, moonDeg, sunSign, sunDeg);
                    horo.tithi = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[0] : tithi;
                    horo.moonPhase = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[1] : "";
                    horo.tithiRem = (tithi.IndexOf('|') > -1) ? tithi.Split('|')[2] : "";
                }
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (horo.planetPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = horo.planetPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctPls.ContainsKey(zod_nam[i]))
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }
                        }
                    }
                }
                horo.planetPos = dctPls;
                return new JsonResult(horo);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("AnalyzeDasamsa")]
        public ActionResult AnalyzeDasamsa(string dob, string tob, string latlng, string timezone, string lang, int ayanid)
        {
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            try
            {
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string[] ras1 = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                JsonResult jOb = (JsonResult)CalcDivChart(mHoro.planetsPos, 10);
                Dictionary<string, string> plPos = (Dictionary<string, string>)(jOb.Value);
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, PlanetHouse> dctDPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0].Trim();
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (mHoro.planetsPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                PlanetHouse pH = dctPlHou[hou10.Split('|')[2]];
                //pH = dctDPlHou[pH.houselord];
                PlanetStrength rLS = checkStrength(pH); //10th rashi lord
                rsn = string.Empty;
                asn = string.Empty;
                foreach (var ppos in plPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                ksn = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0].Trim();
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                rpos = calcHno(asn, rsn);
                kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (plPos.ContainsKey(ksn))
                {
                    var eP = plPos[ksn];
                    plPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    plPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                plPos[rsn] = plPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0; r2 = 0;
                asc = false;
                string houT12 = string.Empty;
                string houT4 = string.Empty, houT9 = string.Empty, houT10 = string.Empty, houT5 = string.Empty, houT6 = string.Empty, houT2 = string.Empty, houT7 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                asc_h = ras1[r1];
                            }
                        }
                    }
                    if (r2 == 2) houT2 = ras1[r1];
                    else if (r2 == 4) houT4 = ras1[r1];
                    else if (r2 == 5) houT5 = ras1[r1];
                    else if (r2 == 6) houT6 = ras1[r1];
                    else if (r2 == 7) houT7 = ras1[r1];
                    else if (r2 == 9) houT9 = ras[r1];
                    else if (r2 == 10) houT10 = ras1[r1];
                    else if (r2 == 12)
                    {
                        houT12 = ras1[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                mon_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                sun_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras1)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (plPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in plPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctDPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                string cdas = string.Empty;
                if (plPos.ContainsKey(houT10.Split('|')[0]))
                {
                    foreach (string pl in plPos[houT10.Split('|')[0]].Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {  //consider only true  
                            cdas += dctPlNames[pl.Split(' ')[1]] + ",";
                        }
                    }
                }

                string desc = string.Format("You can expect good career prospects during");
                if (cdas.Length > 0)
                {
                    desc += string.Format(" {0} dasha which occupied 10th house and during {1} dasha who is 10th house lord in Dasamsa chart", cdas.TrimEnd(','), dctPlNames[houT10.Split('|')[2]]);
                }
                else
                {
                    desc += string.Format(" dasha of 10th house lord {0}", dctPlNames[houT10.Split('|')[2]]);
                }
                dctYogs["10LDAS"] = desc;
                cdas = string.Empty;
                foreach (var ph in dctDPlHou)
                {
                    if (dctDPlHou[ph.Key].lordship == "KEN" || dctDPlHou[ph.Key].lordship == "BOTH")
                    {
                        cdas += dctDPlHou[ph.Key].name + ",";
                    }
                }
                if (cdas != string.Empty)
                {
                    desc = string.Format("In your D-10 chart {0} in angular house, A significant career milestone can be seen during its dasha/bhukthi indicated below provided if TRANSIT of this planet also agrees", cdas.TrimEnd(','));
                    string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                    string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                    double mpos = dctPlHou["Mo"].pos;
                    BirthStar bS = calcBirthStar(mpos, dctPlHou["Mo"].sign);
                    using (StreamReader r4 = new StreamReader(rJ))
                    {
                        string json4 = r4.ReadToEnd();
                        dynamic rashis = JsonConvert.DeserializeObject(json4);
                        int msi = Convert.ToInt32(rashis[dctPlHou["Mo"].sign].ToString().Split('|')[0]);
                        int nsi = Convert.ToInt32(rashis[bS.startSign].ToString().Split('|')[0]);
                        JsonResult oV = (JsonResult)CalcVim(string.Format("{0}-{1}-{2}T{3}", dob.Split('|')[2], dob.Split('|')[1], dob.Split('|')[0], tob.Replace('|', ':')), bS.ruler, mpos, Convert.ToDouble(bS.startDeg), msi, nsi, lang);
                        Dictionary<string, Dasha> vDas = (Dictionary<string, Dasha>)oV.Value;
                        string dasl = string.Empty, bhul = string.Empty;
                        foreach (var vim in vDas)
                        {
                            Dasha das = vim.Value;
                            int n = 0;
                            if (das.lord.Split('-').Count() > 2) continue;
                            bhul = string.Empty;
                            foreach (var lrd in das.lord.Split('-'))
                            {
                                n++;
                                if (n < 3)
                                {
                                    if (das.lord.Split('-').Count() == 1)
                                    {
                                        if (cdas.IndexOf(lrd) != -1)
                                        {
                                            //if (dasl.IndexOf(lrd) == -1)
                                            // {
                                            desc += string.Format("<span style=\"font-weight:bold\">{0} {1}</span><br/>", das.lord, das.per);
                                            dasl += lrd + "|";
                                            //}
                                        }
                                    }
                                    else if (das.lord.Split('-').Count() == 2 && n == 2)
                                    {
                                        if (cdas.IndexOf(lrd) != -1)
                                        {
                                            if (bhul.IndexOf(lrd) == -1)
                                            {
                                                desc += string.Format("<span style=\"font-weight:bold\">{0} {1}</span><br/>", das.lord, das.per);
                                                bhul += lrd + "|";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    dctYogs["CMS"] = desc;
                }
                pH = dctDPlHou[houT10.Split('|')[2]];
                // pH = dctDPlHou[pH.houselord];
                PlanetStrength dLS = checkStrength(pH); //10th D-10 lord
                string stren = string.Empty;
                bool strong = false;
                switch (dLS)
                {
                    case PlanetStrength.EXALTED:
                        stren = "Exalted";
                        strong = true;
                        break;
                    case PlanetStrength.MOOLTRIKONA:
                        stren = "Mooltrikona";
                        strong = true;
                        break;
                    case PlanetStrength.OWN:
                        strong = true;
                        stren = "Own";
                        break;
                    case PlanetStrength.FRIEND:
                        strong = true;
                        stren = "Friendly";
                        break;
                    case PlanetStrength.DEBILIATED:
                        stren = "Debiliated";
                        strong = false;
                        break;
                }
                bool wellplaced = false;
                string place = string.Empty;
                if (pH.lordship == "KEN" || pH.lordship == "TRI" || pH.lordship == "BOTH")
                {
                    wellplaced = true;
                    if (pH.lordship == "TRI") { place = "Quadrant"; } else { place = "Angular"; }
                }
                if (strong && wellplaced)
                {
                    dctYogs["10LSWP"] = string.Format("In your D-10 chart 10th lord {0} is in {1} sign & wellplaced in {2} which ensures good career.", pH.name, stren, place);
                }
                pH = dctDPlHou[asc_h.Split('|')[2]];
                // pH = dctDPlHou[pH.houselord];
                Horo horo = new Horo();
                horo.planetPos = plPos;
                JsonResult jO = (JsonResult)GetAspects(horo, asc_h.Split('|')[0]);
                Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isben = false;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (isBEN(lrd))
                        {
                            isben = true;
                        }

                        //int ben = isBenefic(asc_h.Split('|')[0], lrd);
                        //switch (ben)
                        //{
                        //    case 1:
                        //        isben = true;
                        //        break;
                        //    case 2:
                        //        break;
                        //    case 0:
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }
                }
                if (isben)
                    dctYogs["ASCHS"] = string.Format("In your D-10 chart Ascendant house is aspected by benefic planet which is good.");
                jO = (JsonResult)GetAspects(horo, houT10.Split('|')[0]);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isTben = false;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (isBEN(lrd))
                        {
                            isTben = true;
                        }

                        //int ben = isBenefic(asc_h.Split('|')[0], lrd);
                        //switch (ben)
                        //{
                        //    case 1:
                        //        isTben = true;
                        //        break;
                        //    case 2:
                        //        break;
                        //    case 0:
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }
                }
                if (isTben)
                    dctYogs["10HS"] = string.Format("In your D-10 chart 10th house is aspected by benefic planet which is good.");
                PlanetStrength pS = checkStrength(dctDPlHou["Su"]);
                if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.FRIEND)
                {
                    if (dctDPlHou["Su"].mhno == 3 || dctDPlHou["Su"].mhno == 6 || dctDPlHou["Su"].mhno == 10 || dctDPlHou["Su"].mhno == 11)
                    {
                        //powerful sun in upachaya house
                        dctYogs["PSUPA"] = string.Format("In your D-10 chart powerful sun in upachaya house which is good");
                        JsonResult jR = (JsonResult)GetAspects(horo, dctPlHou["Su"].sign);
                        Dictionary<int, string> dAs = (Dictionary<int, string>)jR.Value;
                        bool bJ = false;
                        foreach (var asp in dAs)
                        {
                            foreach (var lrd in asp.Value.Split('|'))
                            {
                                if (lrd == "Ju") { bJ = true; break; }
                            }
                            if (bJ) break;
                        }
                        if (bJ)
                        {
                            dctYogs["PSUPAJUP"] = string.Format("In your D-10 chart powerful sun in upachaya and aspected by jupiter, native is likely to get recognition & eminence in career.");
                        }
                    }
                    if (dctDPlHou["Su"].mhno == 1 || dctDPlHou["Su"].mhno == 4 || dctDPlHou["Su"].mhno == 7 || dctDPlHou["Su"].mhno == 10)
                    {
                        //powerful sun in angle house
                        dctYogs["PSANG"] = string.Format("In your D-10 chart powerful sun in Angular house which gives high status, high income, administrative power, far sihtedness.");
                    }
                }
                //PlanetHouse lagH = dctPlHou[hou10.Split('|')[2]];
                PlanetHouse lagTH = dctDPlHou[hou10.Split('|')[2]];
                pS = checkStrength(lagTH);
                if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.FRIEND)
                {
                    //10th lord of D1 is strong in D10
                    dctYogs["10D1SD10"] = string.Format("The 10th Lord {0} in your Rashi Chart is strong in D-10 chart, which ensures status and smooth career with many sources of income.", lagTH.name);
                }
                PlanetHouse sH = dctDPlHou["Sa"];
                pS = checkStrength(sH);
                if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.FRIEND)
                {
                    if (sH.lordship == "KEN" || sH.lordship == "TRI" || sH.lordship == "BOTH")
                    {
                        //strong and well placed saturn
                        dctYogs["SSWP"] = string.Format("In your D-10 chart saturn is strong & well placed which gives support from subordinates, workers, political career, gains from underground mining, iron & steel etc.");
                    }
                }
                PlanetHouse mH = dctDPlHou["Mo"];
                pS = checkStrength(mH);
                if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.FRIEND)
                {
                    //strong moon
                    dctYogs["SM"] = string.Format("In your D-10 chart Moon is strong which gives zeal to work.");
                }


            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("ERROR", string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
            return new JsonResult(dctYogs);
        }
        [HttpGet("CalcDivChart")]
        public ActionResult CalcDivChart(Dictionary<string, string> plpos, int ndivs)
        {
            Dictionary<string, string> navPls = new Dictionary<string, string>();
            try
            {
                string[] sgns = { "ar|M|Ma|1|O", "ta|F|Ve|2|E", "ge|D|Me|3|O", "cn|M|Mo|4|E", "le|F|Su|5|O", "vi|D|Me|6|E", "li|M|Ve|7|O", "sc|F|Ma|8|E", "sa|D|Ju|9|O", "cp|M|Sa|10|E", "aq|F|Sa|11|O", "pi|D|Ju|12|E" };
                if (ndivs == 4)
                {
                    foreach (var sign in sgns)
                    {
                        if (plpos.ContainsKey(sign.Split('|')[0]))
                        {
                            foreach (string pl in plpos[sign.Split('|')[0]].Split('|'))
                            {
                                int spos = Convert.ToInt32(sign.Split('|')[3]);
                                string[] degs = pl.Split(' ')[0].Split('.');

                                double po = Convert.ToDouble(degs[0] + '.' + degs[1]);
                                if (po >= 0 && po < 7.30)
                                {
                                    //no change
                                }
                                else if (po >= 7.30 && po < 15)
                                {
                                    spos += 3;
                                }
                                else if (po >= 15 && po < 22)
                                {
                                    spos += 6;
                                }
                                else if (po >= 22 && po < 30)
                                {
                                    spos += 9;
                                }
                                if (spos > 12) spos -= 12;
                                switch (spos)
                                {
                                    case 1:
                                        if (!navPls.ContainsKey("ar"))
                                            navPls["ar"] = pl;
                                        else
                                            navPls["ar"] += "|" + pl;
                                        break;
                                    case 2:
                                        if (!navPls.ContainsKey("ta"))
                                            navPls["ta"] = pl;
                                        else
                                            navPls["ta"] += "|" + pl;
                                        break;
                                    case 3:
                                        if (!navPls.ContainsKey("ge"))
                                            navPls["ge"] = pl;
                                        else
                                            navPls["ge"] += "|" + pl;
                                        break;
                                    case 4:
                                        if (!navPls.ContainsKey("cn"))
                                            navPls["cn"] = pl;
                                        else
                                            navPls["cn"] += "|" + pl;
                                        break;
                                    case 5:
                                        if (!navPls.ContainsKey("le"))
                                            navPls["le"] = pl;
                                        else
                                            navPls["le"] += "|" + pl;
                                        break;
                                    case 6:
                                        if (!navPls.ContainsKey("vi"))
                                            navPls["vi"] = pl;
                                        else
                                            navPls["vi"] += "|" + pl;
                                        break;
                                    case 7:
                                        if (!navPls.ContainsKey("li"))
                                            navPls["li"] = pl;
                                        else
                                            navPls["li"] += "|" + pl;
                                        break;
                                    case 8:
                                        if (!navPls.ContainsKey("sc"))
                                            navPls["sc"] = pl;
                                        else
                                            navPls["sc"] += "|" + pl;
                                        break;
                                    case 9:
                                        if (!navPls.ContainsKey("sa"))
                                            navPls["sa"] = pl;
                                        else
                                            navPls["sa"] += "|" + pl;
                                        break;
                                    case 10:
                                        if (!navPls.ContainsKey("cp"))
                                            navPls["cp"] = pl;
                                        else
                                            navPls["cp"] += "|" + pl;
                                        break;
                                    case 11:
                                        if (!navPls.ContainsKey("aq"))
                                            navPls["aq"] = pl;
                                        else
                                            navPls["aq"] += "|" + pl;
                                        break;
                                    case 12:
                                        if (!navPls.ContainsKey("pi"))
                                            navPls["pi"] = pl;
                                        else
                                            navPls["pi"] += "|" + pl;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (ndivs == 9)
                {
                    double sec = (double)(30) / (double)ndivs;
                    double secp = 0;
                    double n = 1;
                    ArrayList divs = new ArrayList();
                    while ((secp = sec * n) <= 30)
                    {
                        divs.Add(secp);
                        n++;
                    }
                    int spos = 0;
                    int ns = 0;
                    foreach (var sign in sgns)
                    {
                        if (sign.Split('|')[1] == "M")
                            spos = Convert.ToInt32(sign.Split('|')[3]);
                        else if (sign.Split('|')[1] == "F")
                            spos = Convert.ToInt32(sign.Split('|')[3]) + 8;
                        else if (sign.Split('|')[1] == "D")
                            spos = Convert.ToInt32(sign.Split('|')[3]) + 4;
                        //spos = Convert.ToInt32(sign.Split('|')[3]);
                        if (plpos.ContainsKey(sign.Split('|')[0]))
                        {
                            foreach (string pl in plpos[sign.Split('|')[0]].Split('|'))
                            {
                                int ppos = spos;
                                string[] degs = pl.Split(' ')[0].Split('.');

                                double po = Convert.ToDouble(degs[0] + '.' + degs[1]);
                                n = 0.0;
                                for (int dp = 0; dp < divs.Count; dp++)
                                {
                                    if (po >= n && po <= (double)divs[dp]) { break; }
                                    n = (double)divs[dp];
                                    ppos++;
                                }
                                int sord = 0;
                                int rpos = ppos;
                                while (rpos > 12) rpos -= 12;
                                //int r1 = 0;
                                //int r2 = 0;
                                //for (r1 = 0; r1 < 12; r1++,r2++)
                                //{
                                //    if (r2 == rpos) break;
                                //    if (r1 == 11) r1 = 0;
                                //}

                                int navp = rpos;
                                if (navp < 1) navp = 12 - navp;
                                switch (navp)
                                {
                                    case 1:
                                        if (!navPls.ContainsKey("ar"))
                                            navPls["ar"] = pl;
                                        else
                                            navPls["ar"] += "|" + pl;
                                        break;
                                    case 2:
                                        if (!navPls.ContainsKey("ta"))
                                            navPls["ta"] = pl;
                                        else
                                            navPls["ta"] += "|" + pl;
                                        break;
                                    case 3:
                                        if (!navPls.ContainsKey("ge"))
                                            navPls["ge"] = pl;
                                        else
                                            navPls["ge"] += "|" + pl;
                                        break;
                                    case 4:
                                        if (!navPls.ContainsKey("cn"))
                                            navPls["cn"] = pl;
                                        else
                                            navPls["cn"] += "|" + pl;
                                        break;
                                    case 5:
                                        if (!navPls.ContainsKey("le"))
                                            navPls["le"] = pl;
                                        else
                                            navPls["le"] += "|" + pl;
                                        break;
                                    case 6:
                                        if (!navPls.ContainsKey("vi"))
                                            navPls["vi"] = pl;
                                        else
                                            navPls["vi"] += "|" + pl;
                                        break;
                                    case 7:
                                        if (!navPls.ContainsKey("li"))
                                            navPls["li"] = pl;
                                        else
                                            navPls["li"] += "|" + pl;
                                        break;
                                    case 8:
                                        if (!navPls.ContainsKey("sc"))
                                            navPls["sc"] = pl;
                                        else
                                            navPls["sc"] += "|" + pl;
                                        break;
                                    case 9:
                                        if (!navPls.ContainsKey("sa"))
                                            navPls["sa"] = pl;
                                        else
                                            navPls["sa"] += "|" + pl;
                                        break;
                                    case 10:
                                        if (!navPls.ContainsKey("cp"))
                                            navPls["cp"] = pl;
                                        else
                                            navPls["cp"] += "|" + pl;
                                        break;
                                    case 11:
                                        if (!navPls.ContainsKey("aq"))
                                            navPls["aq"] = pl;
                                        else
                                            navPls["aq"] += "|" + pl;
                                        break;
                                    case 12:
                                        if (!navPls.ContainsKey("pi"))
                                            navPls["pi"] = pl;
                                        else
                                            navPls["pi"] += "|" + pl;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    double sec = (double)(30) / (double)ndivs;
                    double secp = 0;
                    double n = 1;
                    ArrayList divs = new ArrayList();
                    while ((secp = sec * n) <= 30)
                    {
                        divs.Add(secp);
                        n++;
                    }
                    int spos = 0;
                    int ns = 0;
                    foreach (var sign in sgns)
                    {
                        if (sign.Split('|')[4] == "O")
                            spos = Convert.ToInt32(sign.Split('|')[3]);
                        else
                            spos = Convert.ToInt32(sign.Split('|')[3]) + 8;
                        //spos = Convert.ToInt32(sign.Split('|')[3]);
                        if (plpos.ContainsKey(sign.Split('|')[0]))
                        {
                            foreach (string pl in plpos[sign.Split('|')[0]].Split('|'))
                            {
                                int ppos = spos;
                                string[] degs = pl.Split(' ')[0].Split('.');

                                double po = Convert.ToDouble(degs[0] + '.' + degs[1]);
                                n = 0.0;
                                for (int dp = 0; dp < divs.Count; dp++)
                                {
                                    if (po >= n && po <= (double)divs[dp]) { break; }
                                    n = (double)divs[dp];
                                    ppos++;
                                }
                                int sord = 0;
                                int rpos = ppos;
                                while (rpos > 12) rpos -= 12;
                                //int r1 = 0;
                                //int r2 = 0;
                                //for (r1 = 0; r1 < 12; r1++,r2++)
                                //{
                                //    if (r2 == rpos) break;
                                //    if (r1 == 11) r1 = 0;
                                //}

                                int navp = rpos;
                                if (navp < 1) navp = 12 - navp;
                                switch (navp)
                                {
                                    case 1:
                                        if (!navPls.ContainsKey("ar"))
                                            navPls["ar"] = pl;
                                        else
                                            navPls["ar"] += "|" + pl;
                                        break;
                                    case 2:
                                        if (!navPls.ContainsKey("ta"))
                                            navPls["ta"] = pl;
                                        else
                                            navPls["ta"] += "|" + pl;
                                        break;
                                    case 3:
                                        if (!navPls.ContainsKey("ge"))
                                            navPls["ge"] = pl;
                                        else
                                            navPls["ge"] += "|" + pl;
                                        break;
                                    case 4:
                                        if (!navPls.ContainsKey("cn"))
                                            navPls["cn"] = pl;
                                        else
                                            navPls["cn"] += "|" + pl;
                                        break;
                                    case 5:
                                        if (!navPls.ContainsKey("le"))
                                            navPls["le"] = pl;
                                        else
                                            navPls["le"] += "|" + pl;
                                        break;
                                    case 6:
                                        if (!navPls.ContainsKey("vi"))
                                            navPls["vi"] = pl;
                                        else
                                            navPls["vi"] += "|" + pl;
                                        break;
                                    case 7:
                                        if (!navPls.ContainsKey("li"))
                                            navPls["li"] = pl;
                                        else
                                            navPls["li"] += "|" + pl;
                                        break;
                                    case 8:
                                        if (!navPls.ContainsKey("sc"))
                                            navPls["sc"] = pl;
                                        else
                                            navPls["sc"] += "|" + pl;
                                        break;
                                    case 9:
                                        if (!navPls.ContainsKey("sa"))
                                            navPls["sa"] = pl;
                                        else
                                            navPls["sa"] += "|" + pl;
                                        break;
                                    case 10:
                                        if (!navPls.ContainsKey("cp"))
                                            navPls["cp"] = pl;
                                        else
                                            navPls["cp"] += "|" + pl;
                                        break;
                                    case 11:
                                        if (!navPls.ContainsKey("aq"))
                                            navPls["aq"] = pl;
                                        else
                                            navPls["aq"] += "|" + pl;
                                        break;
                                    case 12:
                                        if (!navPls.ContainsKey("pi"))
                                            navPls["pi"] = pl;
                                        else
                                            navPls["pi"] += "|" + pl;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                navPls.Add("eX.Message", line.ToString());
            }
            return new JsonResult(navPls);
        }
        [HttpGet("GetAspects")]
        public ActionResult GetAspects(Horo horo, string sign)
        {
            Dictionary<int, string> dctAsp = new Dictionary<int, string>();
            try
            {
                //Dictionary<string, string> dctPlNames = new Dictionary<string,string>();
                //dctPlNames.Add("Su", "Sun");
                //dctPlNames.Add("Mo", "Moon");
                //dctPlNames.Add("Ju", "Jupiter");
                //dctPlNames.Add("Me", "Mercury");
                //dctPlNames.Add("Ve", "Venus");
                //dctPlNames.Add("Ma", "Mars");
                //dctPlNames.Add("Sa", "Saturn");
                //dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                //dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                Dictionary<string, string> aspects = new Dictionary<string, string>();
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string af = string.Format(@"{0}\aspects.json", astClient);
                string asps = string.Empty;
                using (StreamReader rdr = new StreamReader(af, Encoding.UTF8))
                {
                    asps = rdr.ReadToEnd();
                }
                aspects = JsonConvert.DeserializeObject<Dictionary<string, string>>(asps);
                var seven_asp = "";
                var sign_7 = aspects[sign + "-7"];
                if (horo.planetPos.ContainsKey(sign_7))
                {
                    var pls = horo.planetPos[sign_7].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        //if (pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1].ToLower() != "ra" && pls[k].Split(' ')[1].ToLower() != "ke" && pls[k].Split(' ')[1] != "AC")
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            //seven_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
                            if (dctAsp.ContainsKey(7))
                            {
                                dctAsp[7] = string.Format("{0}|{1}", dctAsp[7], pls[k].Split(' ')[1]);
                            }
                            else
                            {
                                dctAsp.Add(7, pls[k].Split(' ')[1]);
                            }
                        }
                    }
                }
                var five_asp = "";
                var sign_5 = aspects[sign + "-5"].Split('|')[1];
                if (horo.planetPos.ContainsKey(sign_5))
                {
                    var pls = horo.planetPos[sign_5].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            if (pls[k].Split(' ')[1].ToLower() == "ju")
                            {
                                //five_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
                                if (dctAsp.ContainsKey(5))
                                {
                                    dctAsp[5] = string.Format("{0}|{1}", dctAsp[5], pls[k].Split(' ')[1]);
                                }
                                else
                                {
                                    dctAsp.Add(5, pls[k].Split(' ')[1]);
                                }
                            }
                        }
                    }
                }
                var nine_asp = "";
                var sign_9 = aspects[sign + "-9"].Split('|')[1];
                if (horo.planetPos.ContainsKey(sign_9))
                {
                    var pls = horo.planetPos[sign_9].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            if (pls[k].Split(' ')[1].ToLower() == "ju")
                            {
                                //nine_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
                                if (dctAsp.ContainsKey(9))
                                {
                                    dctAsp[9] = string.Format("{0}|{1}", dctAsp[9], pls[k].Split(' ')[1]);
                                }
                                else
                                {
                                    dctAsp.Add(9, pls[k].Split(' ')[1]);
                                }
                            }
                        }
                    }
                }
                var ten_asp = "";
                var sign_10 = aspects[sign + "-10"].Split('|')[1];
                if (horo.planetPos.ContainsKey(sign_10))
                {
                    var pls = horo.planetPos[sign_10].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            if (pls[k].Split(' ')[1].ToLower() == "sa")
                            {
                                //ten_asp += dctPlNames[pls[k].split(' ')[1].ToLower()] + ' ';
                                if (dctAsp.ContainsKey(10))
                                {
                                    dctAsp[10] = string.Format("{0}|{1}", dctAsp[10], pls[k].Split(' ')[1]);
                                }
                                else
                                {
                                    dctAsp.Add(10, pls[k].Split(' ')[1]);
                                }
                            }
                        }
                    }
                }
                var three_asp = "";
                var sign_3 = aspects[sign + "-3"].Split('|')[1];
                if (horo.planetPos.ContainsKey(sign_3))
                {
                    var pls = horo.planetPos[sign_3].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            if (pls[k].Split(' ')[1].ToLower() == "sa")
                            {
                                // three_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
                                if (dctAsp.ContainsKey(3))
                                {
                                    dctAsp[3] = string.Format("{0}|{1}", dctAsp[3], pls[k].Split(' ')[1]);
                                }
                                else
                                {
                                    dctAsp.Add(3, pls[k].Split(' ')[1]);
                                }
                            }
                        }
                    }
                }
                if (horo.planetPos.ContainsKey(sign))
                {
                    var pls = horo.planetPos[sign].Split('|');
                    for (var k = 0; k < pls.Length; k++)
                    {
                        if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
                        {
                            if (dctAsp.ContainsKey(1))
                            {
                                dctAsp[1] = string.Format("{0}|{1}", dctAsp[1], pls[k].Split(' ')[1]);
                            }
                            else
                            {
                                dctAsp.Add(1, pls[k].Split(' ')[1]);
                            }
                        }
                    }
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                dctAsp.Add(-1, string.Format("ERROR: {0},LINE:{1}", eX.Message, line));
            }
            return new JsonResult (dctAsp);
        }
		public ActionResult GetAspectsEx(Horo horo, string sign)
		{
			Dictionary<int, string> dctAsp = new Dictionary<int, string>();
			try
			{
				Dictionary<string, string> aspects = new Dictionary<string, string>();
				string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
				string af = string.Format(@"{0}\aspects.json", astClient);
				string asps = string.Empty;
				using (StreamReader rdr = new StreamReader(af, Encoding.UTF8))
				{
					asps = rdr.ReadToEnd();
				}
				aspects = JsonConvert.DeserializeObject<Dictionary<string, string>>(asps);
				var seven_asp = "";
				var sign_7 = aspects[sign + "-7"];
				if (horo.planetPos.ContainsKey(sign_7))
				{
					var pls = horo.planetPos[sign_7].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						//if (pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1].ToLower() != "ra" && pls[k].Split(' ')[1].ToLower() != "ke" && pls[k].Split(' ')[1] != "AC")
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							//seven_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
							if (dctAsp.ContainsKey(7))
							{
								dctAsp[7] = string.Format("{0}|{1}&{2}", dctAsp[7], pls[k], sign_7);
							}
							else
							{
								dctAsp.Add(7, string.Format("{0}&{1}", pls[k], sign_7));
							}
						}
					}
				}
				var five_asp = "";
				var sign_5 = aspects[sign + "-5"].Split('|')[1];
				if (horo.planetPos.ContainsKey(sign_5))
				{
					var pls = horo.planetPos[sign_5].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							if (pls[k].Split(' ')[1].ToLower() == "ju")
							{
								//five_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
								if (dctAsp.ContainsKey(5))
								{
									dctAsp[5] = string.Format("{0}|{1}&{2}", dctAsp[5], pls[k], sign_5);
								}
								else
								{
									dctAsp.Add(5, string.Format("{0}&{1}", pls[k], sign_5));
								}
							}
						}
					}
				}
				var nine_asp = "";
				var sign_9 = aspects[sign + "-9"].Split('|')[1];
				if (horo.planetPos.ContainsKey(sign_9))
				{
					var pls = horo.planetPos[sign_9].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							if (pls[k].Split(' ')[1].ToLower() == "ju")
							{
								//nine_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
								if (dctAsp.ContainsKey(9))
								{
									dctAsp[9] = string.Format("{0}|{1}&{2}", dctAsp[9], pls[k], sign_9);
								}
								else
								{
									dctAsp.Add(9, string.Format("{0}&{1}", pls[k], sign_9));
								}
							}
						}
					}
				}
				var ten_asp = "";
				var sign_10 = aspects[sign + "-10"].Split('|')[1];
				if (horo.planetPos.ContainsKey(sign_10))
				{
					var pls = horo.planetPos[sign_10].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							if (pls[k].Split(' ')[1].ToLower() == "sa")
							{
								//ten_asp += dctPlNames[pls[k].split(' ')[1].ToLower()] + ' ';
								if (dctAsp.ContainsKey(10))
								{
									dctAsp[10] = string.Format("{0}|{1}&{2}", dctAsp[10], pls[k], sign_10);
								}
								else
								{
									dctAsp.Add(10, string.Format("{0}&{1}", pls[k], sign_10));
								}
							}
						}
					}
				}
				var three_asp = "";
				var sign_3 = aspects[sign + "-3"].Split('|')[1];
				if (horo.planetPos.ContainsKey(sign_3))
				{
					var pls = horo.planetPos[sign_3].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							if (pls[k].Split(' ')[1].ToLower() == "sa")
							{
								// three_asp += dctPlNames[pls[k].Split(' ')[1].ToLower()] + ' ';
								if (dctAsp.ContainsKey(3))
								{
									dctAsp[3] = string.Format("{0}|{1}&{2}", dctAsp[3], pls[k], sign_3);
								}
								else
								{
									dctAsp.Add(3, string.Format("{0}&{1}", pls[k], sign_3));
								}
							}
						}
					}
				}
				if (horo.planetPos.ContainsKey(sign))
				{
					var pls = horo.planetPos[sign].Split('|');
					for (var k = 0; k < pls.Length; k++)
					{
						if (pls[k].Split(' ')[1] != "Ur" && pls[k].Split(' ')[1] != "Pl" && pls[k].Split(' ')[1] != "me" && pls[k].Split(' ')[1] != "os" && pls[k].Split(' ')[1] != "Ne" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ra" && pls[k].Split(' ')[1] != "AC" && pls[k].Split(' ')[1] != "Ke")
						{
							if (dctAsp.ContainsKey(1))
							{
								dctAsp[1] = string.Format("{0}|{1}&{2}", dctAsp[1], pls[k], sign);
							}
							else
							{
								dctAsp.Add(1, string.Format("{0}&{1}", pls[k], sign));
							}
						}
					}
				}

			}
			catch (Exception eX)
			{
				var st = new StackTrace(eX, true);
				// Get the top stack frame
				var frame = st.GetFrame(st.FrameCount - 1);
				// Get the line number from the stack frame
				var line = frame.GetFileLineNumber();
				dctAsp.Add(-1, string.Format("ERROR: {0},LINE:{1}", eX.Message, line));
			}
			return new JsonResult(dctAsp);
		}
		public bool isBEN(string lord)
        {
            if (lord == "Ju" || lord == "Mo" || lord == "Ve" || lord == "Me") return true;
            return false;
        }
        [HttpGet("AnalyzeDasamsaDasha")]
        public ActionResult AnalyzeDasamsaDasha(string mdas, string dob, string tob, string latlng, string timezone, string lang, int ayanid)
        {
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                JsonResult jOb = (JsonResult)CalcDivChart(mHoro.planetsPos, 10);
                Dictionary<string, string> plPos = (Dictionary<string, string>)(jOb.Value);
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string[] ras1 = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, PlanetHouse> dctDPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                            else
                            {
                                if (pl.Split(' ')[1] != "Mo" && pl.Split(' ')[1] != "MEAN_NODE" && pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                                {  //consider only true  
                                }
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (mHoro.planetsPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                PlanetHouse pH = dctPlHou[hou10.Split('|')[2]];
                //pH = dctDPlHou[pH.houselord];
                PlanetStrength rLS = checkStrength(pH); //10th rashi lord
                rsn = string.Empty;
                ksn = string.Empty;
                asn = string.Empty;
                foreach (var ppos in plPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                rpos = calcHno(asn, rsn);
                kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (plPos.ContainsKey(ksn))
                {
                    var eP = plPos[ksn];
                    plPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    plPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                plPos[rsn] = plPos[rsn].Replace("MEAN_NODE", "Ra");

                r1 = 0; r2 = 0;
                asc = false;
                string houT12 = string.Empty;
                string houT4 = string.Empty, houT9 = string.Empty, houT10 = string.Empty, houT5 = string.Empty, houT6 = string.Empty, houT2 = string.Empty, houT7 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                asc_h = ras1[r1];
                            }
                        }
                    }
                    if (r2 == 2) houT2 = ras1[r1];
                    else if (r2 == 4) houT4 = ras1[r1];
                    else if (r2 == 5) houT5 = ras1[r1];
                    else if (r2 == 6) houT6 = ras1[r1];
                    else if (r2 == 7) houT7 = ras1[r1];
                    else if (r2 == 9) houT9 = ras[r1];
                    else if (r2 == 10) houT10 = ras1[r1];
                    else if (r2 == 12)
                    {
                        houT12 = ras1[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                mon_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                sun_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras1)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (plPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in plPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctDPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                pH = dctDPlHou[mdas];
                // pH = dctDPlHou[pH.houselord];
                PlanetStrength dLS = checkStrength(pH); //10th D-10 lord
                string stren = string.Empty;
                bool strong = false;
                switch (dLS)
                {
                    case PlanetStrength.EXALTED:
                        stren = "Exalted";
                        strong = true;
                        break;
                    case PlanetStrength.MOOLTRIKONA:
                        stren = "Mooltrikona";
                        strong = true;
                        break;
                    case PlanetStrength.OWN:
                        strong = true;
                        stren = "Own";
                        break;
                    case PlanetStrength.FRIEND:
                        strong = true;
                        stren = "Friendly";
                        break;
                    case PlanetStrength.DEBILIATED:
                        stren = "Debilitated";
                        strong = false;
                        break;
                }
                bool wellplaced = false;
                string place = string.Empty;
                if (pH.lordship == "KEN" || pH.lordship == "TRI" || pH.lordship == "BOTH")
                {
                    wellplaced = true;
                    if (pH.lordship == "TRI") { place = "Quadrant"; } else { place = "Angular"; }
                }
                if (strong && wellplaced)
                {
                    //dctYogs["10LSWP"] = string.Format("In your D-10 chart 10th lord {0} is in {1} sign & wellplaced in {2} house which ensures good career.", pH.name, stren, place);
                }
                pH = dctPlHou[mdas];
                // pH = dctDPlHou[pH.houselord];
                PlanetStrength dLS2 = checkStrength(pH); //10th D-10 lord
                string stren2 = string.Empty;
                bool strong2 = false;
                switch (dLS)
                {
                    case PlanetStrength.EXALTED:
                        stren2 = "Exalted";
                        strong2 = true;
                        break;
                    case PlanetStrength.MOOLTRIKONA:
                        stren2 = "Mooltrikona";
                        strong2 = true;
                        break;
                    case PlanetStrength.OWN:
                        strong2 = true;
                        stren2 = "Own";
                        break;
                    case PlanetStrength.FRIEND:
                        strong2 = true;
                        stren2 = "Friendly";
                        break;
                    case PlanetStrength.DEBILIATED:
                        stren2 = "Debilitated";
                        strong2 = false;
                        break;
                }
                bool wellplaced2 = false;
                string place2 = string.Empty;
                if (pH.lordship == "KEN" || pH.lordship == "TRI" || pH.lordship == "BOTH")
                {
                    wellplaced2 = true;
                    if (pH.lordship == "TRI") { place = "Quadrant"; } else { place = "Angular"; }
                }
                string desc = string.Empty, head = string.Empty;
                bool bdeb = false;
                head = string.Format("You are currently in {0} dasha ", pH.name);
                if ((strong && wellplaced) && (strong2 && wellplaced2))
                {
                    desc += string.Format(" which is strong & wellplaced in both D-1 & D-10 charts, you can expect very good career prospect in the current period");
                }
                else if ((strong) && (strong2))
                {
                    desc += string.Format(" which is strong in both D-1 & D-10 charts, the career prospects during this current period will be very good");
                }
                else if ((strong) && (!strong2))
                {
                    if (stren2 == "Debilitated")
                        desc += string.Format(" which is strong in D-1 chart but is Debilitated in D-10, may create uncertainities under critical sub periods ", pH.name);
                }
                else if ((!strong) || (!strong2))
                {
                    if (stren2 == "Debilitated")
                    {
                        desc += string.Format(" which is debilitated in D-10, such a placement can caause uncertainities  ", pH.name);
                        bdeb = true;
                    }
                }
                else if (strong2 && !strong)
                {
                    desc += string.Format(" is week in lagna chart but whereas strong in D-10 chart. The career prospects in this period can be good.");
                }
                else if (strong2)
                {
                    desc += string.Format(" is strong in D-10 chart. The career prospets in this period will be good.");
                }
                //else
                //{
                Horo horo = new Horo();
                horo.planetPos = plPos;
                JsonResult jO = (JsonResult)GetAspects(horo, pH.sign);
                Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isben = false, ismel = false;
                string mel_pl = string.Empty, ben_pl = string.Empty;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (isBEN(lrd))
                        {
                            isben = true;
                            ben_pl += dctPlNames[lrd] + ",";
                        }
                        else
                        {
                            ismel = true;
                            mel_pl += dctPlNames[lrd] + ",";
                        }
                        //int ben = isBenefic(pH.sign, lrd);
                        //switch (ben)
                        //{
                        //    case 1:
                        //        isben = true;
                        //        ben_pl += dctPlNames[lrd] + ",";
                        //        break;
                        //    case 2:
                        //        ismel = true;
                        //        mel_pl += dctPlNames[lrd] + ",";
                        //        break;
                        //    case 0:
                        //        break;
                        //    default:
                        //        break;
                        //}
                    }
                }
                if (isben)
                {
                    desc += string.Format(" {0} is having benefic aspect from {1} which is good.", pH.name, ben_pl);
                }
                if (ismel)
                {
                    if (bdeb)
                        desc += string.Format(" {0} also is having malefic aspect from {1} which may cause uncertainities during its critical sub periods may even temporarily hold anticipated results. ", pH.name, mel_pl.TrimEnd(','));
                    else
                    {
                        if (desc != string.Empty)
                        {
                            desc += string.Format(" but {0} is also having malefic aspect from {1} such aspect may cause uncertainities during its critical sub periods may even temporarily hold anticipated results. ", pH.name, mel_pl.TrimEnd(','));
                        }
                        else
                        {
                            desc += string.Format(" {0} is having malefic aspect from {1} such aspect may cause uncertainities during its critical sub periods may even temporarily hold anticipated results. ", pH.name, mel_pl.TrimEnd(','));
                        }
                    }
                }
                if (desc != string.Empty)
                {
                    dctYogs["CDASLS"] = head + desc;
                }
                else
                {
                    desc += string.Format("{0} is neither strong nor is week in D-10 chart, would ensure steady progress.", pH.name);
                    dctYogs["CDASLS"] = head + desc;
                }
                //}
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("ERROR", string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
            return new JsonResult (dctYogs);
        }
        [HttpGet("GetAllHobbyAsts")]
        public ActionResult GetAllHobbyAsts()
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                try
                {
                    var dbAst = db.GetCollection<Astrologer>("HobbyAstro");
                    var qAstUser =
                                    (from e in dbAst.AsQueryable<Astrologer>()
                                     select e).ToList();
                    if (qAstUser != null)
                    {
                        return new JsonResult(qAstUser);
                    }
                    else
                    {
                        var ast = new Astrologer
                        {
                            status = "X"
                        };
                        return new JsonResult (ast);
                    }
                }
                catch (Exception eX)
                {
                    var ast = new Astrologer
                    {
                        tagline = eX.Message,
                        status = "E"
                    };
                    return new JsonResult(ast);
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var ast = new Astrologer
                {
                    tagline = eX.Message,
                    status = "E"
                };
                return new JsonResult(ast);
            }
        }
        [HttpGet("GetOffer")]
        public ActionResult GetOffer(string uuid)
        {
            var offer = new Offer
            {
                uuid = uuid,
                oid = "com.mypubz.eportal.offer499",
                title = "Subscribe For Just Rs. 499",
                desc = "OFFER ENDS TODAY!!! SUBSCRIBE NOW FOR JUST 499 & ENJOY ALL BENEFITS.",
                price = 499,
                avail = false,
                impr = 0
            };
            return new JsonResult(offer);
            //try
            //{
            //    var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
            //    MongoClient client = new MongoClient(connectionString); // connect to localhost
            //    Console.WriteLine("Getting DB...");
            //    var db = client.GetDatabase("myypub");
            //    var dbOfr = db.GetCollection<Offer>("Offer");
            //    var ofr =
            //            (from e in dbOfr.AsQueryable<Offer>()
            //             where e.uuid == uuid select e).ToList().FirstOrDefault();
            //    if (ofr != null)
            //    {
            //        var update = Builders<Offer>.Update.Set("impr", ofr.impr + 1);
            //        var filter2 = Builders<Offer>.Filter.Eq("uuid", ofr.uuid);
            //        var tick = dbOfr.FindOneAndUpdate<Offer>(filter2, update);
            //        return Json(ofr, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        var offer = new Offer
            //        {
            //            uuid = uuid,
            //            oid = "com.mypubz.eportal.offer499",
            //            title = "Subscribe For Just Rs. 499",
            //            desc = "OFFER ENDS TODAY!!! SUBSCRIBE NOW FOR JUST 499 & ENJOY ALL BENEFITS.",
            //            price = 499,
            //            avail = true,
            //            impr = 0
            //        };
            //        return Json(offer, JsonRequestBehavior.AllowGet);
            //    }
            //}
            //catch (Exception eX)
            //{
            //    var offer = new Offer
            //    {
            //        uuid = uuid,
            //        oid = "",
            //        title = "",
            //        desc = string.Format("An exception occured: {0}", eX.Message),
            //        price = 0,
            //        avail = false,
            //        impr = 0
            //    };
            //    return Json(offer, JsonRequestBehavior.AllowGet);
            //}
        }
        [HttpGet("GetTransPredsEx")]
        public ActionResult GetTransPredsEx(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
        {
            try
            {
                string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                string astPls = "SuMoMaMeJuVeSaRaKe";
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mNHoro = new Horoscope();
                uint un1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint un2 = Convert.ToUInt32(dob.Split('|')[1]);
                int in3 = Convert.ToInt32(dob.Split('|')[2]);
                uint un4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint un5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint un6 = Convert.ToUInt32(tob.Split('|')[2]);
				double un7 = Convert.ToDouble(latlng.Split('|')[0]);
				double un8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)un1, (int)un2, in3, tzofset, (AYANMSAS)ayanid);
                }
                mNHoro.init_data_ex2(un1, un2, in3, un4, un5, un6, un7, un8, tz, ayan, (uint)ayanid);
                mNHoro.calc_planets_pos(true, astClient);
                // checkYogs(mHoro);
                //return PartialView("Birthchart", mHoro);
                string[] zod_nam = new string[] { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                Dictionary<string, string> dctNPls = new Dictionary<string, string>();
                for (int i = 0; i < zod_nam.Count(); i++)
                {
                    if (mNHoro.planetsPos.ContainsKey(zod_nam[i]))
                    {
                        var ppos = mNHoro.planetsPos[zod_nam[i]];
                        foreach (var pl in ppos.Split('|'))
                        {
                            string[] pld = pl.Split(' ')[0].Split('.');
                            if (dctNPls.ContainsKey(zod_nam[i]))
                            {
                                dctNPls[zod_nam[i]] = string.Format("{0}|{1}.{2} {3}", dctNPls[zod_nam[i]], pld[0], pld[1], pl.Split(' ')[1]);
                            }
                            else
                            {
                                dctNPls[zod_nam[i]] = string.Format("{0}.{1} {2}", pld[0], pld[1], pl.Split(' ')[1]);
                            }

                            if (astPls.Contains(pl.Split(' ')[1]))
                            {
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = 0,
                                    mhno = 0,
                                    shno = 0,
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = zod_nam[i],
                                    signtype = "",
                                    lordship = "",
                                    houselord = ""
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }

                //DBLog(string.Format("GetTransPreds-{0}", dob));
                CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
                DateTimeFormatInfo dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedDayNames = new String[] { "SUN", "MON", "TUE", "WED",
                                                "THU", "FRI", "SAT" };
                //                string sF = string.Format(@"{0}\ayan-kp.json", astClient);
                ////                string ayan = string.Empty;
                //                using (StreamReader r = new StreamReader(sF))
                //                {
                //                    string jsa = r.ReadToEnd();
                //                    dynamic ayans = JsonConvert.DeserializeObject(jsa);
                //                    ayan = ayans[dob.Split('|')[2]].ToString();
                //                }
                //JsonResult json1 = (JsonResult)GetBirthstar(dob, tob, latlng, timezone, 4);
                //BirthStar cStar = (BirthStar)json1.Data;
                BirthStar cStar = calcBirthStar(dctPlHou["Mo"].pos, dctPlHou["Mo"].sign);
                Dictionary<string, string> dctPreds = new Dictionary<string, string>();
                Dictionary<string, string> dctPls = new Dictionary<string, string>();
                DateTime today = DateTime.Now;
                DateTime eday = DateTime.Now.AddDays(30);
                string[] signs = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
                while (today <= eday)
                {
                    //string tday = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    //calcStar(tday);
                    //JsonResult json1 = (JsonResult)Birthstar(string.Format("{0}|{1}|{2}", today.Day, today.Month, today.Year), string.Format("{0}|{1}|0", today.Hour, today.Minute));
                    // string latlng = "17.23|78.29";
                    //string timezone = "India Standard Time";
                    Horoscope mHoro = new Horoscope();
                    uint u1 = Convert.ToUInt32(today.Day);
                    uint u2 = Convert.ToUInt32(today.Month);
                    int i3 = Convert.ToInt32(today.Year);
                    uint u4 = 5;//Convert.ToUInt32(today.Hour);
                    uint u5 = 0;//Convert.ToUInt32(today.Minute);
                    uint u6 = 0;
					double u7 = Convert.ToDouble(latlng.Split('|')[0]);
					double u8 = Convert.ToDouble(latlng.Split('|')[1]);
					mHoro.init_data(u1, u2, i3, u4, u5, u6, u7, u8, tz, false, string.Empty);
                    mHoro.calc_planets_pos(true, astClient);
                    int rpos = 0;
                    foreach (string sign in signs)
                    {
                        rpos++;
                        if (mHoro.planetsPos.ContainsKey(sign))
                        {
                            var pls = mHoro.planetsPos[sign].Split('|');
                            //var ePls = '';
                            //var mnode = '';
                            for (var k = 0; k < pls.Length; k++)
                            {
                                if (pls[k].Split(' ')[1] == "MEAN_NODE")
                                {
                                    var kpos = rpos + 6;
                                    if (kpos > 12) kpos = (kpos - 12);
                                    //var mn = i + 11;
                                    //if (mn > 15) mn -= 15;
                                    if (mHoro.planetsPos.ContainsKey(signs[kpos - 1]))
                                    {
                                        var eP = mHoro.planetsPos[signs[kpos - 1]];
                                        mHoro.planetsPos[signs[kpos - 1]] = eP + '|' + pls[k].Split(' ')[0] + ' ' + "Ke";
                                    }
                                    else
                                    {
                                        mHoro.planetsPos[signs[kpos - 1]] = pls[k].Split(' ')[0] + ' ' + "Ke";
                                    }
                                    // plPos[sign] = ePls;
                                    mHoro.planetsPos[sign] = mHoro.planetsPos[sign].Replace("MEAN_NODE", "Ra");
                                }

                            }
                            pls = mHoro.planetsPos[sign].Split('|');
                            for (var k = 0; k < pls.Length; k++)
                            {

                                if (dctPls.ContainsKey(pls[k].Split(' ')[1]))
                                {
                                    if (dctPls[pls[k].Split(' ')[1]].IndexOf(sign) < 0)
                                        dctPls[pls[k].Split(' ')[1]] += string.Format("|{0}-{1}", sign, String.Format(ci, "{0:MMM dd}", today));
                                }
                                else
                                    dctPls[pls[k].Split(' ')[1]] = string.Format("{0}-{1}", sign, String.Format(ci, "{0:MMM dd}", today));
                            }
                        }
                    }
                    today = today.AddDays(1);
                }
                string rJ = string.Format(@"{0}\signs_short.json", astClient);
                string rJ2 = string.Format(@"{0}\o_rashis.json", astClient);
                using (StreamReader r3 = new StreamReader(rJ2))
                using (StreamReader r4 = new StreamReader(rJ))
                {
                    string json3 = r3.ReadToEnd();
                    string json4 = r4.ReadToEnd();
                    dynamic rashis_short = JsonConvert.DeserializeObject(json4);
                    dynamic rashis = JsonConvert.DeserializeObject(json3);
                    foreach (var pred in dctPls)
                    {
                        string pras = pred.Value;
                        string spred = string.Empty;
                        int toks = 0;
                        foreach (var ras in pras.Split('|'))
                        {
                            int pos = GetPosFMon(rashis_short[cStar.birthSign.ToLower()].ToString(), ras.Split('-')[0]);
                            if (pred.Key == "Su" || pred.Key == "Mo" || pred.Key == "Ve" || pred.Key == "Ju" || pred.Key == "Sa" || pred.Key == "Me" || pred.Key == "Ma" || pred.Key == "Ra" || pred.Key == "Ke")
                            {
                                string pky = string.Format(@"{0}\{1}.json", astClient, pred.Key);
                                using (StreamReader r5 = new StreamReader(pky))
                                {
                                    string json5 = r5.ReadToEnd();
                                    dynamic plpreds = JsonConvert.DeserializeObject(json5);
                                    if (toks > 0)
                                    {
                                        spred += string.Format("\nOn {0} {1} moves to {2} which is {3} house, during this time {4}", ras.Split('-')[1], dctPlNames[pred.Key], rashis[ras.Split('-')[0].ToLower()].ToString().Split('|')[1], pos, plpreds[pos.ToString()]);
                                    }
                                    else
                                    {
                                        spred = string.Format("{0} is transiting in {1} which is {2} house, during this time {3}", dctPlNames[pred.Key], rashis[ras.Split('-')[0].ToLower()].ToString().Split('|')[1], pos, plpreds[pos.ToString()]);
                                    }
                                    spred += GetNatalAspects(dctPlHou, plpreds, dctPlNames[pred.Key], ras.Split('-')[0]);
                                }
                                toks++;
                            }
                        }
                        if (toks > 0)
                            dctPreds[dctPlNames[pred.Key]] = spred;
                    }
                }
                return new JsonResult(dctPreds);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("GetNatalAspects")]
        string GetNatalAspects(Dictionary<string, PlanetHouse> dctPlHou, dynamic plPreds, string tpl, string tras)
        {
            string natp = string.Format(" & also during this time the transit {0} gets ", tpl);
            int np = 0;
            foreach (var pl in dctPlHou)
            {
                PlanetHouse pH = pl.Value;
                if (pH.code == "Mo") continue;
                int pos = GetPosFMon(tras, pH.sign);
                if (pos == 1 || pos == 2 || pos == 3 || pos == 5 || pos == 7 || pos == 9 || pos == 11 || pos == 12)
                {
                    natp += string.Format("{0} aspect from natal {1} which may cause {2}, ", pos, pH.name, plPreds[pH.code]);
                    np++;
                }
            }
            if (np > 0) return natp;
            return string.Empty;
        }
        [HttpGet("AnalyzeMoney")]
        public ActionResult AnalyzeMoney(string das, string dob, string tob, string latlng, string timezone, string lang, int ayanid)
        {
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            try
            {
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string[] ras1 = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                //   JsonResult jOb = (JsonResult)CalcDivChart(mHoro, 10);
                Dictionary<string, string> plPos = mHoro.planetsPos;//(Dictionary<string, string>)(jOb.Data);
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou3 = string.Empty, hou4 = string.Empty, hou8 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou11 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, PlanetHouse> dctDPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
				dctPlNames.Add("me", "MEAN NODE");
				dctPlNames.Add("AC", "Ascendant");
                Dictionary<string, int> dctIndu = new Dictionary<string, int>();
                dctIndu.Add("Su", 30);
                dctIndu.Add("Mo", 16);
                dctIndu.Add("Ma", 6);
                dctIndu.Add("Me", 8);
                dctIndu.Add("Ju", 10);
                dctIndu.Add("Ve", 12);
                dctIndu.Add("Sa", 1);


                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0].Trim();
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 3) hou3 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 8) hou8 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 11) hou11 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                string mhou9 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 9) mhou9 = ras[r1];
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (mHoro.planetsPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                string desc = string.Empty;
                string md = dctPlNames[das.Split('|')[0]];
                string ad = dctPlNames[das.Split('|')[1]];
                string pd = dctPlNames[das.Split('|')[2]];
                string msig = string.Empty;
                if (das.Split('|')[0] == "Ju" || das.Split('|')[0] == "Mo" || das.Split('|')[0] == "Ve" || das.Split('|')[0] == "Me")
                {
                    msig += md;
                }
                if (das.Split('|')[1] == "Ju" || das.Split('|')[1] == "Mo" || das.Split('|')[1] == "Ve" || das.Split('|')[1] == "Me")
                {
                    msig += (msig != string.Empty) ? "& " + ad : ad;
                }
                if (das.Split('|')[2] == "Ju" || das.Split('|')[2] == "Mo" || das.Split('|')[2] == "Ve" || das.Split('|')[2] == "Me")
                {
                    msig += (msig != string.Empty) ? "& " + pd : pd;
                }
                desc = string.Format("<span style=\"font-weight:bold\">You are currently in {0} Maha Dasha, {1} Antar Dasha & {2} Pratyantar Dasha</span><br>", md, ad, pd);
                if (msig != string.Empty)
                    desc += string.Format("<span>Your Dasha Lord(s) <span style=\"font-weight:bold;color:blue\">{0}</span> are Money significators. Hence in the current Dasha will help you gain money which depends on strength of these Dasha Lords.</span><br>", msig);
                desc += "<span style=\"font-weight:bold\">Moon and Jupiter is Main significator of Wealth in any Horoscope. Venus bestows Luxuries and comforts in Life. Without the Blessing of Venus we can not enjoy anything. So a well placed Venus Jupiter and Moon makes one Rich.</style><br>";
                //PlanetStrength
                desc += "<span>Lets examine the strength of each of these planets in your Rashi Chart.</span><br>";
                //PlanetStrength pSJ =  PlanetStrength.NORMAL;
                string ds = descStrength(dctPlHou["Ju"]);
                // dctYogs.Add("SIG-Ju", ds);
                desc += string.Format("<span style=\"font-weight:bold;color:red\">{0}</span><br>", ds);
                //desc += "<br>";
                //PlanetStrength pSM = PlanetStrength.NORMAL;
                ds = descStrength(dctPlHou["Mo"]);
                //dctYogs.Add("SIG-Mo", ds);
                desc += string.Format("<span style=\"font-weight:bold;color:blue\">{0}</span><br>", ds);
                //PlanetStrength pSV = PlanetStrength.NORMAL;
                ds = descStrength(dctPlHou["Ve"]);
                //dctYogs.Add("SIG-Ve", ds);
                desc += string.Format("<span style=\"font-weight:bold;color:green\">{0}</span><br>", ds);
                //PlanetStrength pSME = PlanetStrength.NORMAL;
                ds = descStrength(dctPlHou["Me"]);
                //dctYogs.Add("SIG-Me", ds);
                desc += string.Format("<span style=\"font-weight:bold;color:brown\">{0}</span><br>", ds);
                PlanetHouse pH2L = dctPlHou[hou2.Split('|')[2]];
                PlanetHouse pH11L = dctPlHou[hou11.Split('|')[2]];
                if (pH2L.sign == asc_h.Split('|')[0] && pH11L.sign == asc_h.Split('|')[1])
                {
                    desc += string.Format("<span>The second house lord <span style=\"font-weight:bold;color:blue\">{0}</span> & 11th house lord <span style=\"font-weight:bold;color:red\">{1}</span> are conjunct in Ascendant house which forms a very auspicious wealth yoga in horoscope.", dctPlNames[hou2.Split('|')[2]], dctPlNames[hou11.Split('|')[2]]);
                }
                PlanetStrength pS1L = checkStrength(dctPlHou[asc_h.Split('|')[2]]);
                PlanetStrength pS2L = checkStrength(dctPlHou[hou2.Split('|')[2]]);
                PlanetStrength pS11L = checkStrength(dctPlHou[hou11.Split('|')[2]]);
                bool o1 = false, o2 = false, o3 = false;
                if (pS1L == PlanetStrength.OWN || pS1L == PlanetStrength.MOOLTRIKONA)
                {
                    o1 = true;
                }
                if (pS2L == PlanetStrength.OWN || pS2L == PlanetStrength.MOOLTRIKONA)
                {
                    o2 = true;
                }
                if (pS11L == PlanetStrength.OWN || pS11L == PlanetStrength.MOOLTRIKONA)
                {
                    o3 = true;
                }
                if (o1 && o2 && o3)
                {
                    desc += string.Format("<span>The first house lord <span style=\"font-weight:bold;color:blue\">{0}</span>, second house lord <span style=\"font-weight:bold;color:red\">{1}</span> & 11th house lord <span style=\"font-weight:bold;color:green\">{3}</span> are in their respective houses which makes the native extremely rich.", dctPlNames[asc_h.Split('|')[2]], dctPlNames[hou2.Split('|')[2]], dctPlNames[hou11.Split('|')[2]]);
                }
                PlanetStrength pS7L = checkStrength(dctPlHou[hou11.Split('|')[2]]);
                r1 = 0;
                r2 = 0;
                Horo horo = new Horo();
                horo.planetPos = plPos;
                JsonResult jO = (JsonResult)GetAspects(horo, hou7.Split('|')[0]);
                Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isben = false, ismel = false;
                string mel_pl = string.Empty, ben_pl = string.Empty;
                bool ownasp1 = false;
                bool ownasp2 = false;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        bool ben = isBEN(lrd);
                        if (ben) { isben = true; ben_pl += dctPlNames[lrd] + ","; }
                        else { ismel = true; mel_pl += dctPlNames[lrd] + ","; }

                        if (lrd == hou7.Split('|')[2]) { ownasp1 = true; }
                    }
                }
                //if (isben)
                //{
                //  desc += string.Format(" The 7th house is having benefic aspect from {0} which is good.", ben_pl);
                //}
                jO = (JsonResult)GetAspects(horo, hou8.Split('|')[0]);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isben2 = false;
                ismel = false;
                mel_pl = string.Empty;
                ben_pl = string.Empty;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        bool ben = isBEN(lrd);
                        if (ben) { isben2 = true; ben_pl += dctPlNames[lrd] + ","; }
                        else { ismel = true; mel_pl += dctPlNames[lrd] + ","; }
                        if (lrd == hou8.Split('|')[2]) { ownasp2 = true; }
                    }
                }
                //if (isben2)
                //{
                //desc += string.Format(" The 8th house is having benefic aspect from {0} which is good.", ben_pl);
                //}
                if (isben && isben2)
                {
                    //7th & 8th house having benefic aspect
                    desc += string.Format("<span style=\"font-weight:bold\">Both 7th & 8th house are aspected by benefic planet(s). The natives financial condition improves after marriage.</span><br>");

                }
                if (ownasp1)
                {
                    desc += string.Format("<span style=\"font-weight:bold\">7th Lord aspecting its own house which is auspicious</span><br>");
                }
                if (ownasp2)
                {
                    desc += string.Format("<span style=\"font-weight:bold\">8th Lord aspecting its own house which is auspicious</span><br>");
                }
                bool eql = dctPlHou["Ju"].hno == dctPlHou["Me"].hno && dctPlHou["Me"].hno == dctPlHou[hou2.Split('|')[2]].hno;
                if (eql)
                {
                    //Jupiter conjunct with Mercury and 2nd Lord
                    desc += string.Format("<span style=\"font-weight:bold\">Jupiter, Mercury & the 2nd Lord {0} are in same house. Which is a very good combination for Financial gains.</span><br>", dctPlNames[hou2.Split('|')[2]]);
                }
                if ((dctPlHou[hou2.Split('|')[2]].houselord != dctPlHou[hou5.Split('|')[2]].houselord) && (dctPlHou[hou2.Split('|')[2]].houselord == hou5.Split('|')[2] && dctPlHou[hou5.Split('|')[2]].houselord == hou2.Split('|')[2]))
                {
                    //exchage between 2nd lord and 5th lord
                    desc += string.Format("<span style=\"font-weight:bold\">2nd Lord {0} & 5th Lord {1} exchanged their houses which is very auspicious for Financial Growth. as per Bhavat Bhavam principle, it is the secondary House of Fortune. Moreover it is one of the Trikona House. Trikona Houses are known as Lakshmisthana or Houses of wealth</span><br>", dctPlNames[dctPlHou[hou2.Split('|')[2]].houselord], dctPlNames[dctPlHou[hou2.Split('|')[5]].houselord]);
                }
                jO = (JsonResult)GetAspects(horo, dctPlHou["Mo"].sign);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    if (asp.Value.Contains("Su"))
                    {
                        //sun aspects moon
                        desc += string.Format("<span style=\"font-weight:bold\">In your horoscope Sun is aspecting Moon which is very favorable for financial prosperity .</span><br>");
                    }
                    if (asp.Value.Contains("Ju"))
                    {
                        //jupiter aspects moon
                        desc += string.Format("<span style=\"font-weight:bold\">In your horoscope Jupiter is aspecting Moon which is very favorable for financial prosperity .</span><br>");
                    }
                }
                jO = (JsonResult)GetAspects(horo, dctPlHou[hou2.Split('|')[2]].sign);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    if (asp.Value.Contains("Ju"))
                    {
                        //jupiter aspects 2nd lord
                        desc += string.Format("<span style=\"font-weight:bold\">In your horoscope Jupiter is aspecting 2nd Lord which is very favorable for financial prosperity .</span><br>");
                    }
                }
                jO = (JsonResult)GetAspects(horo, hou2.Split('|')[0]);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    if (asp.Value.Contains("Ju"))
                    {
                        //jupiter aspects 2nd house
                        desc += string.Format("<span style=\"font-weight:bold\">In your horoscope Jupiter is aspecting 2nd house which is very favorable for financial prosperity .</span><br>");

                    }
                }
                bool h8b = isBEN(hou8.Split('|')[2]);
                if (h8b)
                {
                    //benefics in 8th house
                    desc += string.Format("<span style=\"font-weight:bold\">In your horoscope there is a benefic planet in 8th house which makes one rich by inheritance or Lottery or Share market .</span><br>");
                }
                if (dctPlHou["Ve"].hno == 12)
                {
                    //venus in 12 house
                    desc += string.Format("<span style=\"font-weight:bold\">In your horoscope Venus is in 12 house which is very beneficial for financial gains.</span><br>");
                }
                //ARUDHA LAGNA
                PlanetHouse pHL = dctPlHou[asc_h.Split('|')[2]];
                int arh = (pHL.hno * 2) - 1;
                if (arh > 12) arh -= 12;
                //var k = (arh == 1) ? asc_h : string.Format("hou{0}", arh);

                string arHou = string.Empty;
                switch (arh)
                {
                    case 1:
                        arHou = asc_h;
                        break;
                    case 2:
                        arHou = hou2;
                        break;
                    case 3:
                        arHou = hou3;
                        break;
                    case 4:
                        arHou = hou4;
                        break;
                    case 5:
                        arHou = hou5;
                        break;
                    case 6:
                        arHou = hou6;
                        break;
                    case 7:
                        arHou = hou7;
                        break;
                    case 8:
                        arHou = hou8;
                        break;
                    case 9:
                        arHou = hou9;
                        break;
                    case 10:
                        arHou = hou10;
                        break;
                    case 11:
                        arHou = hou11;
                        break;
                    case 12:
                        arHou = hou12;
                        break;
                    default:
                        break;
                }

                desc += string.Format("<h2>Arudh Lagna</h2>");
                desc += string.Format("<p>Arudha Lagna is the Image of your Lagna on others. Its like reflection. It gives us a clue about how a person is perceived by others. For example if some one comes out of a BMW car, we instantly draw a conclusion that the person is extremely rich which may be or may not be true. It may happen that he does not own the car. This is the role of Arudha Lagna. So it is very important regarding Materialistic gain.</p><br>");
                desc += string.Format("<p>Your Arudh Lagna is {0} & its Lord is {1}</p><br>", arHou.Split('|')[0], dctPlNames[arHou.Split('|')[2]]);
                //11th from ARUDH LAGNA
                r1 = 0;
                r2 = 0;
                bool arl = false;
                string ar11 = string.Empty, ar2 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (arl) r2++;
                    if (ras[r1].Split('|')[0].Trim() == arHou.Split('|')[0])
                    {
                        arl = true;
                        r2++;
                    }
                    if (r2 == 11)
                    {
                        ar11 = ras[r1];
                    }
                    if (r2 == 2)
                    {
                        ar2 = ras[r1];
                    }
                    if (r2 == 12) break;
                    if (r1 == 11) r1 = -1;
                }
                jO = (JsonResult)GetAspects(horo, ar11.Split('|')[0]);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                string arPls = string.Empty;
                foreach (var asp in dctAsp)
                {
                    arPls += asp.Value + ",";
                }
                if (arPls != string.Empty)
                {
                    desc += string.Format("<p>{0} aspects 11th house from Arudh Lagna which is very auspicious, as per Jaimini such aspect could make the native wealthy.</p><br>", arPls);
                }
                if (plPos.ContainsKey(ar11.Split('|')[0]))
                {
                    //11h hs planet
                    var pls = plPos[ar11.Split('|')[0]];
                    string plc = string.Empty;
                    bool ben = false, mel = false;
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            plc += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                            bool b = isBEN(pl.Split(' ')[1]);
                            if (b)
                            {
                                ben = true;
                            }
                            else
                            {
                                mel = true;
                            }
                        }
                    }
                    if (ben && mel)
                    {
                        desc += string.Format("<p>since there is both benefic & malefic planet in the 11th house from Arudh Lagna, as per Jaimini such placement can make the native  wealthy through both righteous and unfair means</p><br>");
                    }
                    else if (ben)
                    {
                        desc += string.Format("<p>since there is a benefic planet positioned in the 11th house from Arudh Lagna, as per Jaimini such placement can make the native  wealthy through fair & righteous means</p><br>");
                    }
                    else if (mel)
                    {
                        desc += string.Format("<p>since there is a malefic planet positioned in the 11th house from Arudh Lagna, as per Jaimini such placement cause the native to make money through unfair means.</p><br>");
                    }
                }
                if (plPos.ContainsKey(ar2.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[ar2.Split('|')[0]];
                    string plc = string.Empty;
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "Ju" || pl.Split(' ')[1] == "Mo" || pl.Split(' ')[1] == "Ve")
                            plc += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                    }
                    if (plc != string.Empty)
                    {
                        desc += string.Format("<p>In your horoscope {0} in 2nd house from Arudh Lagna, as per Jaimini such a placement causes native wealthy & prosperous.</p><br>", plc);
                    }
                }
                if (arh == 11)
                {
                    desc += string.Format("<p>In your horoscope Arudh Lagna falls in 11th house from Ascendant, as per Jaimini such placement causes the native to be wealthy.</p><br>");
                }
                else if (arh == 1 || arh == 4 || arh == 5 || arh == 7 || arh == 9 || arh == 10)
                {
                    desc += string.Format("<p>In your horoscope Arudh Lagna is falling in {0} house which is Angular or Trine house from Ascendant, as per Jaimini such placement causes the native to be wealthy.</p><br>", arh);
                }
                else if (arh == 6 || arh == 8 || arh == 12)
                {
                    desc += string.Format("<p>In your horoscope Arudh Lagna falls in {0} house from Ascendant, as per Jaimini such placement is not good may need astrological remedies. Please talk to one of our expert astrologers.</p><br>", arh);
                }
                desc += string.Format("<h2>Indu Lagna</h2>");
                desc += string.Format("<p>Indu lagna is also known as the ascendant of wealth. It has special significance in Ashtakavarga. It is also known by the name of Moon Yoga in the Brihat Parashara Hora. Indu lagna or Moon ascendant is analyzed to determine the financial situation of a native. It is used to determine the wealth and prosperity of the native. It also determines important incidents of a native’s life.</p><br>");
                int ind = dctIndu[hou9.Split('|')[2]] + dctIndu[mhou9.Split('|')[2]];
                int inr = ind % 12;
                r1 = 0;
                r2 = 0;
                mon = false;
                string inl = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                            }
                        }
                    }
                    if (r2 == inr) inl = ras[r1];
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }

                r1 = 0;
                r2 = 0;
                bool inu = false;
                string in1 = string.Empty, in2 = string.Empty, in4 = string.Empty, in6 = string.Empty, in7 = string.Empty, in8 = string.Empty, in10 = string.Empty, in11 = string.Empty, in12 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (inu) r2++;
                    if (ras[r1].Split('|')[0].Trim() == inl.Split('|')[0].Trim())
                    {
                        inu = true;
                        r2++;
                    }
                    if (r2 == 1)
                    {
                        in1 = ras[r1];
                    }
                    else if (r2 == 2)
                    {
                        in2 = ras[r1];
                    }
                    else if (r2 == 4) in4 = ras[r1];
                    else if (r2 == 6) in6 = ras[r1];
                    else if (r2 == 7) in7 = ras[r1];
                    else if (r2 == 8) in8 = ras[r1];
                    else if (r2 == 10) in10 = ras[r1];
                    else if (r2 == 11) in11 = ras[r1];
                    else if (r2 == 12) in12 = ras[r1];
                    if (r2 == 12) break;
                    if (r1 == 11) r1 = -1;
                }
				desc += string.Format("<p>In your horoscope Indu Lagna falls in {0} house, lord of this house is {1}.</p><br>", inl.Split('|')[0], dctPlNames[inl.Split('|')[2]]);
                string indPls = string.Empty;
                if (plPos.ContainsKey(in1.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in1.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        //if (pl.Split(' ')[1] == "Ju" || pl.Split(' ')[1] == "Mo" || pl.Split(' ')[1] == "Ve")
                        indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                    }
                }
                if (plPos.ContainsKey(in4.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in4.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        //if (pl.Split(' ')[1] == "Ju" || pl.Split(' ')[1] == "Mo" || pl.Split(' ')[1] == "Ve")
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (plPos.ContainsKey(in7.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in7.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        //if (pl.Split(' ')[1] == "Ju" || pl.Split(' ')[1] == "Mo" || pl.Split(' ')[1] == "Ve")
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (plPos.ContainsKey(in10.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in10.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (plPos.ContainsKey(in11.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in11.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                desc += string.Format("<p>As per sacred texts, the planets positioned in 1st, 4th, 7th, 10th & 11th houses from Indu Lagna are suppose to be wealth givers, the native is expeted to earn during dashas of these planets.</p><br>");
                if (indPls != string.Empty)
                {
                    desc += string.Format("<p>In your horoscope, the planets {0} are wealth givers, you are expeted earn good wealth during dashas of these planets.</p><br>", indPls);
                }
                desc += string.Format("<p>As per sacred texts, the planets positioned in 6th, 8th, 12th houses from Indu Lagna are considered inauspicious for wealth.</p><br>");
                indPls = string.Empty;
                if (plPos.ContainsKey(in6.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in6.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (plPos.ContainsKey(in8.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in8.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (plPos.ContainsKey(in12.Split('|')[0]))
                {
                    //2nd hs planet
                    var pls = plPos[in12.Split('|')[0]];
                    foreach (var pl in pls.Split('|'))
                    {
                        if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                        {
                            indPls += string.Format("{0},", dctPlNames[pl.Split(' ')[1]]);
                        }
                    }
                }
                if (indPls != string.Empty)
                {
                    desc += string.Format("<p>In your horoscope, the planets {0} are considered inauspicious for weath, you may expect some challenges during dashas of these planets.</p><br>", indPls);
                }
                dctYogs.Add("MONEY", desc);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("ERROR", string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
            return new JsonResult(dctYogs);
        }
        [HttpGet("AnalyzeD4")]
        public ActionResult AnalyzeD4(string dob, string tob, string latlng, string timezone, string lang, int ayanid)
        {
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                JsonResult jOb = (JsonResult)CalcDivChart(mHoro.planetsPos, 4);
                Dictionary<string, string> plPos = (Dictionary<string, string>)(jOb.Value);
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string[] ras1 = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou8 = string.Empty, hou11 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, PlanetHouse> dctDPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                dctPlNames.Add("su", "Sun");
                dctPlNames.Add("mo", "Moon");
                dctPlNames.Add("ju", "Jupiter");
                dctPlNames.Add("me", "Mercury");
                dctPlNames.Add("ve", "Venus");
                dctPlNames.Add("ma", "Mars");
                dctPlNames.Add("sa", "Saturn");
                dctPlNames.Add("ra", "Rahu");
                dctPlNames.Add("ke", "Ketu");
                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 8) hou8 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 11) hou11 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                            else
                            {
                                if (pl.Split(' ')[1] != "Mo" && pl.Split(' ')[1] != "MEAN_NODE" && pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                                {  //consider only true  
                                }
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (mHoro.planetsPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }

                PlanetHouse pH = dctPlHou[hou4.Split('|')[2]];
                //pH = dctDPlHou[pH.houselord];
                PlanetStrength rLS = checkStrength(pH); //10th rashi lord
                rsn = string.Empty;
                ksn = string.Empty;
                asn = string.Empty;
                foreach (var ppos in plPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                rpos = calcHno(asn, rsn);
                kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (plPos.ContainsKey(ksn))
                {
                    var eP = plPos[ksn];
                    plPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    plPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                plPos[rsn] = plPos[rsn].Replace("MEAN_NODE", "Ra");

                r1 = 0; r2 = 0;
                asc = false;
                string houT12 = string.Empty, asc_D = string.Empty;
                string houT4 = string.Empty, houT9 = string.Empty, houT10 = string.Empty, houT5 = string.Empty, houT6 = string.Empty, houT2 = string.Empty, houT7 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                asc_D = ras1[r1];
                            }
                        }
                    }
                    if (r2 == 2) houT2 = ras1[r1];
                    else if (r2 == 4) houT4 = ras1[r1];
                    else if (r2 == 5) houT5 = ras1[r1];
                    else if (r2 == 6) houT6 = ras1[r1];
                    else if (r2 == 7) houT7 = ras1[r1];
                    else if (r2 == 9) houT9 = ras[r1];
                    else if (r2 == 10) houT10 = ras1[r1];
                    else if (r2 == 12)
                    {
                        houT12 = ras1[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                mon_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                sun_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras1)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (plPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in plPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctDPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                string desc = string.Empty;
                //analyze the strength of Lagna Lord, 4th Lord & 4th Karak in Rashi Chart
                desc += string.Format("<span style=\"font-weight:bold\"> According to Vedic texts, in order to analyze ones properties such as house, lands, real estate, vehicles etc., from the horoscope, we should first see in the rashi chart the strength of Ascendant/Lagna House & its lord, 4th House & its lord(properties) & karak Mars(land) & Saturn(construction) & Venus(lexury)</span>");
                string ds = descStrength(dctPlHou[asc_h.Split('|')[2]]);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("<p>In your rashi chart ascendant lord {0}</p>", ds);
                ds = descStrength(dctPlHou[hou4.Split('|')[2]]);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("<p>In your rashi chart fourth house lord {0}</p>", ds);
                //analyze the strength of 4th house in Rashi Chart
                PlanetHouse p4H = dctPlHou[hou4.Split('|')[2]];
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                JsonResult jO = (JsonResult)GetAspects(horo, hou4.Split('|')[0]);
                Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
                bool isben = false, ismel = false;
                string mel_pl = string.Empty, ben_pl = string.Empty;
                string lrds_asp_4 = string.Empty;
                int cn = 0;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        lrds_asp_4 += dctPlNames[lrd].ToLower() + ",";
                        if (isBEN(lrd))
                        {
                            isben = true;
                            ben_pl += dctPlNames[lrd] + ",";
                        }
                        else
                        {
                            ismel = true;
                            mel_pl += dctPlNames[lrd] + ",";
                        }
                        //int ben = isBenefic(hou4.Split('|')[0], lrd);
                        //switch (ben)
                        //{
                        //    case 1:
                        //        isben = true;
                        //        ben_pl += dctPlNames[lrd] + ",";
                        //        break;
                        //    case 2:
                        //        ismel = true;
                        //        mel_pl += dctPlNames[lrd] + ",";
                        //        break;
                        //    case 0:
                        //        break;
                        //    default:
                        //        break;
                        //}
                        if (lrd.ToLower() == hou8.Split('|')[2].ToLower())
                        {
                            //4th lord is aspected by 8th
                            desc += string.Format(" <p>4th house(propety) lord {0} is aspected by 8th house(inhertance) lord {1} which indicates property through inheritance.</p>", p4H.name, dctPlNames[hou8.Split('|')[2]]);
                            cn++;
                        }
                        else if (lrd.ToLower() == hou11.Split('|')[2].ToLower())
                        {
                            //4th lord is aspected by 11th
                            desc += string.Format("<p>4th house(property) lord {0} is aspected by 11th house(gains) lord {1} which indicates gains through property.</p>", p4H.name, dctPlNames[hou8.Split('|')[2]]);
                            cn++;
                        }
                    }
                }
                if (isben)
                {
                    desc += string.Format("<p>In your horoscope 4th house is having benefic aspect from {0} which is good.</p>", ben_pl);
                }
                //check if there is connection between 4th, 8th & 11th lords in Rashi Chart
                //Horo horo = new Horo();
                //horo.planetPos = mHoro.planetsPos;
                PlanetHouse p8H = dctPlHou[hou8.Split('|')[2]];
                jO = (JsonResult)GetAspects(horo, p8H.sign);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (lrd == hou4.Split('|')[2])
                        {
                            //8th lord is aspected by 4th
                            desc += string.Format("<p>8th house(inheritence) lord {0} is aspected by 4th house(property) lord {1} which indicates property through inheritance.</p>", p8H.name, p4H.name);
                            cn++;
                        }
                        else if (lrd == hou11.Split('|')[2])
                        {
                            //8th lord is aspected by 11th
                            desc += string.Format("<p>8th house(inheritance) lord {0} is aspected by 11th house(gains) lord {1} which indicates gains through inheritance.</p>", p8H.name, dctPlNames[hou11.Split('|')[2]]);
                            cn++;
                        }
                    }
                }
                PlanetHouse p11H = dctPlHou[hou11.Split('|')[2]];
                jO = (JsonResult)GetAspects(horo, p11H.sign);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (lrd == hou4.Split('|')[2])
                        {
                            //11th lord is aspected by 4th
                            desc += string.Format("<p>11th house(gains) lord {0} is aspected by 4th house(property) lord {1} which indicates gains through property.</p>", p11H.name, p4H.name);
                            cn++;
                        }
                        else if (lrd == hou8.Split('|')[2])
                        {
                            //11th lord is aspected by 8th
                            desc += string.Format("<p>11th house(gains) lord {0} is aspected by 8th house(inheritance) lord {1} which indicates gains through inheritance.</p>", p11H.name, p8H.name);
                            cn++;
                        }
                    }
                }
                desc += string.Format("<p>As per our ancient texts, if there is a connection between 4th Lord, 8th Lord & 11th Lord in Rashi Chart one will accumulate huge properties depending on strength of connection.</p>", p11H.name, p8H.name);

                if (cn == 6)
                {
                    //connection between 4-8-11 is very strong
                    desc += string.Format("<p> In your rashi chart 4th(property) lord, 8th(inheritance) lord & 11th(gains) lord are aspecting each other, such a combination will help you make huge property gains during the dasha/bhukthi of 4th lord({0}), 8th lord({1}), 11th lord({2})</p>", p4H.name, p8H.name, p11H.name);
                }
                else
                {
                    if ((p4H.hno == 8 || p4H.hno == 11) && (p8H.hno == 4 || p8H.hno == 11) && (p11H.hno == 4 || p11H.hno == 8))
                    {
                        desc += string.Format("<p> In your rashi chart 4th(property) lord in {0}th house, 8th(inheritance) lord  in {1}th house & 11th(gains) lord in {2}th house, such a combination will help you make huge property gains during the dasha/bhukthi of 4th lord({0}), 8th lord({1}), 11th lord({2})</p>", p4H.name, p8H.name, p11H.name);
                    }
                    else
                    {
                        if (p4H.hno == 8 || p4H.hno == 11)
                        {
                            if (p4H.hno == 8)
                                desc += string.Format("<p>In your rashi chart 4th house(property) lord in 8th house(inheritance) which indicate you gain property through inheritence.</p>");
                            else
                                desc += string.Format("<p>In your rashi chart 4th house(Property) lord in 11th house(gains) which indicates you will gain through properties.</p>");
                        }
                        if (p8H.hno == 4 || p8H.hno == 11)
                        {
                            if (p8H.hno == 4)
                                desc += string.Format("<p>In your rashi chart 8th house(inteheritance) lord in 4th house(property) which indicate you gain property through inheritence.</p>");
                            else
                                desc += string.Format("<p>In your rashi chart 8th house(inheritance) lord in 11th house(gains) which indicates you will gain through inheritance.</p>");
                        }
                        else if (p11H.hno == 4 || p11H.hno == 8)
                        {
                            if (p11H.hno == 3)
                                desc += string.Format("<p>In your rashi chart 11th house(gains) lord in 4th house(property) which indicates you gain through property.</p>");
                            else
                                desc += string.Format("<p>In your rashi chart 11th house(gains) lord in 8th house(inheritance) which indicates you will gain through inheritance.</p>");
                        }
                    }
                }
                //check how karaks(Ma,Sa) are placed
                PlanetHouse pMa = dctPlHou["Ma"];
                ds = descStrength(pMa);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your rashi chart 4th Karak(lord of lands) {0}", ds);
                PlanetHouse pSa = dctPlHou["Sa"];
                ds = descStrength(pSa);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your rashi chart lord of construction {0}", ds);
                PlanetHouse pVe = dctPlHou["Ve"];
                ds = descStrength(pVe);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your rashi chart lord of lexury {0}", ds);
                if (dctPlHou[asc_h.Split('|')[2]].shno == 6)
                    desc += string.Format("<p>In your rashi chart ascendant lord {0} is placed in 6th house(disputes) which indicates some challenges related to properties.</p>", dctPlHou[asc_h.Split('|')[2]].name);
                if (dctPlHou[hou4.Split('|')[2]].shno == 6)
                    desc += string.Format("<p>In your rashi chart 4th lord {0} is placed in 6th house(disputes) which indicates some challenges related to properties.</p>", dctPlHou[hou4.Split('|')[2]].name);
                //analyze the strength of Lagna Lord, 4th Lord & 4th Karak in D4 Chart
                desc += string.Format("<h2>D4 Chart Analysis</h2>");
                desc += string.Format("<p>As per ancient text, the Varga charts reveal how promising will be the results analyzed in D1 or Rashi Chart. Below is the analysis of D4 or Chaturthamsa Chart </p>");
                ds = descStrength(dctDPlHou[asc_D.Split('|')[2]]);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your D4 chart, the ascendant lord {0}", ds);
                ds = descStrength(dctDPlHou[houT4.Split('|')[2]]);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your D4 chart, the 4th house lord {0}", ds);
                //check how karaks(Ma,Sa) are placed
                pMa = dctDPlHou["Ma"];
                ds = descStrength(pMa);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your D4 chart 4th House Karak, {0}", ds);
                pSa = dctDPlHou["Sa"];
                ds = descStrength(pSa);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your D4 chart, {0}", ds);
                pVe = dctDPlHou["Ve"];
                ds = descStrength(pVe);
                if (ds.Trim() != string.Empty)
                    desc += string.Format("In your D4 chart, {0}", ds);
                //check vargottama
                if (dctPlHou[asc_h.Split('|')[2]].sign == dctDPlHou[asc_D.Split('|')[2]].sign)
                    desc += string.Format("<p>ASC lord {0} is positioned in same house in D1 & D4 chart becomes vargottama which is excellent.</p>", dctPlHou[asc_h.Split('|')[2]].name);
                if (dctPlHou[hou4.Split('|')[2]].sign == dctDPlHou[houT4.Split('|')[2]].sign)
                    desc += string.Format("<p>4rh lord {0} is positioned in same house in D1 & D4 chart becomes vargottama which is excellent.</p>", dctPlHou[hou4.Split('|')[2]].name);
                if (dctPlHou["Ma"].sign == dctDPlHou["Ma"].sign)
                    desc += string.Format("<p>Mars(4th Karak) is positioned in same house in D1 & D4 chart becomes vargottama which is excellent.</p>");
                if (dctPlHou["Ve"].sign == dctDPlHou["Ve"].sign)
                    desc += string.Format("<p>Venus is positioned in same house in D1 & D4 chart becomes vargottama which is excellent.</p>");
                if (dctPlHou["Sa"].sign == dctDPlHou["Sa"].sign)
                    desc += string.Format("<p>Saturn is positioned in same house in D1 & D4 chart becomes vargottama which is excellent.</p>");
                if (dctDPlHou[asc_D.Split('|')[2]].hno == 6)
                    desc += string.Format("<p>In your D4 chart ascendant lord {0} is placed in 6th house(disputes) which indicates some challenges related to properties.</p>", dctDPlHou[asc_D.Split('|')[2]].name);
                if (dctDPlHou[houT4.Split('|')[2]].hno == 6)
                    desc += string.Format("<p>In your D4 chart 4th lord {0} is placed in 6th house(disputes) which indicates some challenges related to properties.</p>", dctDPlHou[houT4.Split('|')[2]].name);
                horo.planetPos = plPos;
                jO = (JsonResult)GetAspects(horo, houT4.Split('|')[0]);
                dctAsp = (Dictionary<int, string>)(jO.Value);
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        if (lrds_asp_4.IndexOf(dctPlNames[lrd].ToLower()) == -1)
                            lrds_asp_4 += dctPlNames[lrd].ToLower() + ",";
                    }
                }
                desc += string.Format("<h2>Timing of Event</h2>");
                desc += string.Format("<p>You will buy properties during dasha/bhukthi of Lords which aspect 4th house in D1 & D4 charts provided if TRANSIT also agrees. Please note this is just one of combinations & there are many other combinations linked to 4th house& its lord will have similar results, please talk to one of our expert astrologers for detailed analysis. </p>");
                if (lrds_asp_4.Trim() != string.Empty)
                {
                    desc += string.Format("<p>In your D1 & D4 charts {0} aspecting 4th house, you are expected to purchase properties during dasha & bhukthi of {1}</p>", lrds_asp_4, lrds_asp_4);
                    string astClient = Path.Combine(_env.ContentRootPath, @"Content\astroclient");
                    string rJ = string.Format(@"{0}\o_rashis.json", astClient);
                    double mpos = dctPlHou["Mo"].pos;
                    BirthStar bS = calcBirthStar(mpos, dctPlHou["Mo"].sign);
                    using (StreamReader r4 = new StreamReader(rJ))
                    {
                        string json4 = r4.ReadToEnd();
                        dynamic rashis = JsonConvert.DeserializeObject(json4);
                        int msi = Convert.ToInt32(rashis[dctPlHou["Mo"].sign].ToString().Split('|')[0]);
                        int nsi = Convert.ToInt32(rashis[bS.startSign].ToString().Split('|')[0]);
                        JsonResult oV = (JsonResult)CalcVim(string.Format("{0}-{1}-{2}T{3}", dob.Split('|')[2], dob.Split('|')[1], dob.Split('|')[0], tob.Replace('|', ':')), bS.ruler, mpos, Convert.ToDouble(bS.startDeg), msi, nsi, lang);
                        Dictionary<string, Dasha> vDas = (Dictionary<string, Dasha>)oV.Value;
                        string dasl = string.Empty, bhul = string.Empty;
                        foreach (var vim in vDas)
                        {
                            Dasha das = vim.Value;
                            int n = 0;
                            if (das.lord.Split('-').Count() > 2) continue;
                            bhul = string.Empty;
                            foreach (var lrd in das.lord.Split('-'))
                            {
                                n++;
                                if (n < 3)
                                {
                                    if (das.lord.Split('-').Count() == 1)
                                    {
                                        if (lrds_asp_4.IndexOf(lrd.ToLower()) != -1)
                                        {
                                            //if (dasl.IndexOf(lrd) == -1)
                                            // {
                                            desc += string.Format("<span style=\"font-weight:bold\">{0} {1}</span><br/>", das.lord, das.per);
                                            dasl += lrd + "|";
                                            //}
                                        }
                                    }
                                    else if (das.lord.Split('-').Count() == 2 && n == 2)
                                    {
                                        if (lrds_asp_4.IndexOf(lrd.ToLower()) != -1)
                                        {
                                            if (bhul.IndexOf(lrd) == -1)
                                            {
                                                desc += string.Format("<span style=\"font-weight:bold\">{0} {1}</span><br/>", das.lord, das.per);
                                                bhul += lrd + "|";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                dctYogs.Add("D4A", desc);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("ERROR", string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
            return new JsonResult(dctYogs);
        }
        [HttpGet("AnalyzeD9")]
        public ActionResult AnalyzeD9(string dob, string tob, string latlng, string timezone, string lang, int ayanid)
        {
            Dictionary<string, string> dctYogs = new Dictionary<string, string>();
            try
            {
                string tz = TZConvert.IanaToWindows(timezone);
                Horoscope mHoro = new Horoscope();
                uint u1 = Convert.ToUInt32(dob.Split('|')[0]);
                uint u2 = Convert.ToUInt32(dob.Split('|')[1]);
                int i3 = Convert.ToInt32(dob.Split('|')[2]);
                uint u4 = Convert.ToUInt32(tob.Split('|')[0]);
                uint u5 = Convert.ToUInt32(tob.Split('|')[1]);
                uint u6 = Convert.ToUInt32(tob.Split('|')[2]);
				double u7 = Convert.ToDouble(latlng.Split('|')[0]);
				double u8 = Convert.ToDouble(latlng.Split('|')[1]);
				string ayan = string.Empty;
                if (((AYANMSAS)ayanid != AYANMSAS.FAGAN) && ((AYANMSAS)ayanid != AYANMSAS.LAHIRI))
                {
                    TimeZoneInfo tzInf = TimeZoneInfo.FindSystemTimeZoneById(tz);
                    TimeSpan tzO = tzInf.GetUtcOffset(DateTime.Now);
                    double tzofset = Convert.ToDouble(string.Format("{0}.{1}", tzO.Hours, tzO.Minutes));
                    ayan = Ayanmsa.CalcEx((int)u1, (int)u2, i3, tzofset, (AYANMSAS)ayanid);
                }
                mHoro.init_data_ex2(u1, u2, i3, u4, u5, u6, u7, u8, tz, ayan, (uint)ayanid);
                mHoro.calc_planets_pos(true, Path.Combine(_env.ContentRootPath, @"Content\astroclient"));
                JsonResult jOb = (JsonResult)CalcDivChart(mHoro.planetsPos, 9);
                Dictionary<string, string> plPos = (Dictionary<string, string>)(jOb.Value);
                string[] ras = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                string[] ras1 = { "ar|M|Ma", "ta|F|Ve", "ge|D|Me", "cn|M|Mo", "le|F|Su", "vi|D|Me", "li|M|Ve", "sc|F|Ma", "sa|D|Ju", "cp|M|Sa", "aq|F|Sa", "pi|D|Ju" };
                int r1 = 0, r2 = 0;
                bool asc = false;
                string asc_h = string.Empty;
                string mon_h = string.Empty;
                string sun_h = string.Empty;
                string hou12 = string.Empty;
                string hou4 = string.Empty, hou9 = string.Empty, hou10 = string.Empty, hou8 = string.Empty, hou11 = string.Empty, hou5 = string.Empty, hou6 = string.Empty, hou2 = string.Empty, hou7 = string.Empty;
                string fsgn_pls = string.Empty;
                string msgn_pls = string.Empty;
                string dsgn_pls = string.Empty;
                Dictionary<string, PlanetHouse> dctPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, PlanetHouse> dctDPlHou = new Dictionary<string, PlanetHouse>();
                Dictionary<string, string> dctPlNames = new Dictionary<string, string>();
                dctPlNames.Add("Su", "Sun");
                dctPlNames.Add("Mo", "Moon");
                dctPlNames.Add("Ju", "Jupiter");
                dctPlNames.Add("Me", "Mercury");
                dctPlNames.Add("Ve", "Venus");
                dctPlNames.Add("Ma", "Mars");
                dctPlNames.Add("Sa", "Saturn");
                dctPlNames.Add("MEAN_NODE", "MEAN NODE");
                dctPlNames.Add("TRUE_NODE", "TRUE NODE");
                dctPlNames.Add("Ra", "Rahu");
                dctPlNames.Add("Ke", "Ketu");
                dctPlNames.Add("su", "Sun");
                dctPlNames.Add("mo", "Moon");
                dctPlNames.Add("ju", "Jupiter");
                dctPlNames.Add("me", "Mercury");
                dctPlNames.Add("ve", "Venus");
                dctPlNames.Add("ma", "Mars");
                dctPlNames.Add("sa", "Saturn");
                dctPlNames.Add("ra", "Rahu");
                dctPlNames.Add("ke", "Ketu");
                Dictionary<string, string> dctFriends = new Dictionary<string, string>();
                dctFriends.Add("Su", "Mo|Ma|Ju");
                dctFriends.Add("Mo", "Su|Me");
                dctFriends.Add("Ma", "Su|Mo|Ju");
                dctFriends.Add("Me", "Su|Ve");
                dctFriends.Add("Ju", "Su|Mo|Ma");
                dctFriends.Add("Ve", "Sa|Me");
                dctFriends.Add("Sa", "Me|Ve");
                dctFriends.Add("Ra", "Ve|Sa");
                dctFriends.Add("Ke", "Ve|Sa");

                string rsn = string.Empty, asn = string.Empty, ksn = string.Empty, rdeg = string.Empty, asc_pos = string.Empty;
                foreach (var ppos in mHoro.planetsPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC")
                        {
                            asn = ppos.Key;
                            asc_pos = pl.Split(' ')[0];
                        }
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                int rpos = calcHno(asn, rsn);
                int kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (mHoro.planetsPos.ContainsKey(ksn))
                {
                    var eP = mHoro.planetsPos[ksn];
                    mHoro.planetsPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    mHoro.planetsPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                mHoro.planetsPos[rsn] = mHoro.planetsPos[rsn].Replace("MEAN_NODE", "Ra");
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                asc_h = ras[r1];
                            }
                        }
                    }
                    if (r2 == 2) hou2 = ras[r1];
                    else if (r2 == 4) hou4 = ras[r1];
                    else if (r2 == 5) hou5 = ras[r1];
                    else if (r2 == 6) hou6 = ras[r1];
                    else if (r2 == 7) hou7 = ras[r1];
                    else if (r2 == 8) hou8 = ras[r1];
                    else if (r2 == 9) hou9 = ras[r1];
                    else if (r2 == 10) hou10 = ras[r1];
                    else if (r2 == 11) hou11 = ras[r1];
                    else if (r2 == 12)
                    {
                        hou12 = ras[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                bool mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                mon_h = ras[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                bool sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras[r1] += "|" + (r2).ToString();
                    }
                    if (mHoro.planetsPos.ContainsKey(ras[r1].Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ras[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras[r1] += "|" + (r2).ToString();
                                sun_h = ras[r1];
                            }
                            else
                            {
                                if (pl.Split(' ')[1] != "Mo" && pl.Split(' ')[1] != "MEAN_NODE" && pl.Split(' ')[1] != "Ke" && pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                                {  //consider only true  
                                }
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                PlanetHouse pHAK = null;
                double amk = 0.0;
                foreach (string ra in ras)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (mHoro.planetsPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in mHoro.planetsPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctPlHou[pl.Split(' ')[1]] = pHou;
                                if (pHou.pos > amk)
                                {
                                    amk = pHou.pos;
                                    pHAK = pHou;
                                }
                            }
                        }
                    }
                }

                PlanetHouse pH = dctPlHou[hou4.Split('|')[2]];
                //pH = dctDPlHou[pH.houselord];
                PlanetStrength rLS = checkStrength(pH); //10th rashi lord
                rsn = string.Empty;
                ksn = string.Empty;
                asn = string.Empty;
                foreach (var ppos in plPos)
                {
                    foreach (var pl in ppos.Value.Split('|'))
                    {
                        if (pl.Split(' ')[1] == "AC") asn = ppos.Key;
                        else if (pl.Split(' ')[1] == "MEAN_NODE")
                        {
                            rsn = ppos.Key;
                            rdeg = pl.Split(' ')[0];
                        }
                        if (rsn != string.Empty && asn != string.Empty) break;
                    }
                    if (rsn != string.Empty && asn != string.Empty) break;
                }
                r1 = 0;
                r2 = 0;
                asc = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc) r2++;
                    if (ras[r1].Split('|')[0].Trim() == rsn)
                    {
                        asc = true;
                        r2++;
                    }
                    if (r2 == 7)
                    {
                        ksn = ras[r1].Split('|')[0];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }

                rpos = calcHno(asn, rsn);
                kpos = calcHno(rsn, ksn);
                //var mn = i + 11;
                //if (mn > 15) mn -= 15;
                if (plPos.ContainsKey(ksn))
                {
                    var eP = plPos[ksn];
                    plPos[ksn] = string.Format("{0}|{1} Ke", eP, rdeg);
                }
                else
                {
                    plPos[ksn] = string.Format("{0} Ke", rdeg);
                }
                // plPos[sign] = ePls;
                plPos[rsn] = plPos[rsn].Replace("MEAN_NODE", "Ra");

                r1 = 0; r2 = 0;
                asc = false;
                string houT12 = string.Empty, asc_D = string.Empty;
                string houT4 = string.Empty, houT9 = string.Empty, houT10 = string.Empty, houT5 = string.Empty, houT8 = string.Empty, houT6 = string.Empty, houT2 = string.Empty, houT7 = string.Empty;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (asc)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "AC")
                            {
                                asc = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                asc_D = ras1[r1];
                            }
                        }
                    }
                    if (r2 == 2) houT2 = ras1[r1];
                    else if (r2 == 4) houT4 = ras1[r1];
                    else if (r2 == 5) houT5 = ras1[r1];
                    else if (r2 == 6) houT6 = ras1[r1];
                    else if (r2 == 7) houT7 = ras1[r1];
                    else if (r2 == 8) houT8 = ras1[r1];
                    else if (r2 == 9) houT9 = ras[r1];
                    else if (r2 == 10) houT10 = ras1[r1];
                    else if (r2 == 12)
                    {
                        houT12 = ras1[r1];
                        break;
                    }
                    if (r1 == 11) r1 = -1;
                }
                r1 = 0;
                r2 = 0;
                mon = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (mon)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Mo")
                            {
                                mon = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                mon_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                    if (r2 == 12) break;
                }
                r1 = 0;
                r2 = 0;
                sun = false;
                for (r1 = 0; r1 < 12; r1++)
                {
                    if (r2 == 12) break;
                    if (sun)
                    {
                        r2++;
                        ras1[r1] += "|" + (r2).ToString();
                    }
                    if (plPos.ContainsKey(ras1[r1].Split('|')[0]))
                    {
                        foreach (string pl in plPos[ras1[r1].Split('|')[0]].Split('|'))
                        {
                            if (pl.Split(' ')[1] == "Su")
                            {
                                sun = true;
                                r2++;
                                ras1[r1] += "|" + (r2).ToString();
                                sun_h = ras1[r1];
                            }
                        }
                    }
                    if (r1 == 11) r1 = -1;
                }
                foreach (string ra in ras1)
                {
                    bool bpl = false;
                    string pkey = string.Empty;
                    if (plPos.ContainsKey(ra.Split('|')[0]))
                    {
                        foreach (string pl in plPos[ra.Split('|')[0]].Split('|'))
                        {
                            int hno = Convert.ToInt32(ra.Split('|')[3]);
                            string lordship = string.Empty;
                            switch (hno)
                            {
                                case 1:
                                    lordship = "BOTH";
                                    break;
                                case 5:
                                case 9:
                                    lordship = "TRI";
                                    break;
                                case 4:
                                case 7:
                                case 10:
                                    lordship = "KEN";
                                    break;
                                default:
                                    break;
                            }
                            if (ra.Split('|')[1] == "F")
                            {
                                fsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else if (ra.Split('|')[1] == "M")
                            {
                                msgn_pls = pl.Split(' ')[1] + "|";
                            }
                            else
                            {
                                dsgn_pls = pl.Split(' ')[1] + "|";
                            }
                            if (pl.Split(' ')[1] != "Ur" && pl.Split(' ')[1] != "Pl" && pl.Split(' ')[1] != "me" && pl.Split(' ')[1] != "os" && pl.Split(' ')[1] != "Ne" && pl.Split(' ')[1] != "AC" && pl.Split(' ')[1] != "TRUE_NODE")
                            {  //consider only true  
                                string[] pld = pl.Split(' ')[0].Split('.');
                                PlanetHouse pHou = new PlanetHouse
                                {
                                    code = pl.Split(' ')[1],
                                    name = dctPlNames[pl.Split(' ')[1]],
                                    hno = Convert.ToInt32(ra.Split('|')[3]),
                                    mhno = Convert.ToInt32(ra.Split('|')[4]),
                                    shno = Convert.ToInt32(ra.Split('|')[5]),
                                    pos = Convert.ToDouble(string.Format("{0}.{1}", pld[0], pld[1])),
                                    sign = ra.Split('|')[0],
                                    signtype = ra.Split('|')[1],
                                    lordship = lordship,
                                    houselord = ra.Split('|')[2]
                                };
                                dctDPlHou[pl.Split(' ')[1]] = pHou;
                            }
                        }
                    }
                }
                string desc = string.Format("<h2>Vargottama Planets</h2>");
                desc += string.Format("<p>When a planet is in the same sign in the birth chart and navamsa,then that planet is called Vargottama planet. It is coined from 2 words, Varga and Uttama. This is the best planet in the entire divisional chart.</p>");
                //analyze vargottamas
                string vgms = string.Empty;
                string vga = string.Empty;
                foreach (var plh in dctPlHou)
                {
                    if (dctPlHou[plh.Key].sign == dctDPlHou[plh.Key].sign) //vargottama
                    {
                        vgms += plh.Value.name;
                        if (plh.Value.code == pHAK.code) //atmakaraka is vargottama
                        {
                            vgms += " is also atmakaraka";
                        }
                        vgms += ",";
                        switch (plh.Value.name.ToUpper())
                        {
                            case "SUN":
                                vga += string.Format("<p>The Sun indicates the soul, authority, will power, ego and self-esteem. Sun as Vargottama in your D9 chart, you will have a lot of will power. There will be a lot of intimations from your side. </p>");
                                if (plh.Value.sign == "li")
                                    vga += string.Format("<span>Due to the Sun is in its debilitation sign Libra. The results can vary, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "MOON":
                                vga += string.Format("<p>The Moon indicates the emotions, pleasure, nourishment, motherly love and happiness. Moon is the Vargottama planet in your D9 Chart, the qualities indicated by the Moon will be more evident in you.</p>");
                                if (plh.Value.sign == "sc")
                                    vga += string.Format("<span>Due to the Moon is in its debilitation sign Scorpio. The results can vary, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "MERCURY":
                                vga += string.Format("<p>TThe Mercury indicates communication, technology, media and intelligence. Mercury is Vargottama planet in your D9 Chart, you will display a sharp intellect.</p>");
                                if (plh.Value.sign == "pi")
                                    vga += string.Format("<span>Due to the Mercury is in its debilitation sign Pisces. The results can vary, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "VENUS":
                                vga += string.Format("<p>The Venus indicates love, luxury, comfort, money, strength and relationships, knowledge of astrology. Venus is Vargottama planet in your D9 Chart. The Venusian qualities will be bright.</p>");
                                if (plh.Value.sign == "vi")
                                    vga += string.Format("<span>Due to the Venus is in its debilitation sign Vergo. The results can vary, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "MARS":
                                vga += string.Format("<p>The Mars indicates resistance, fighting spirit, valor, and vigor. Mars is Vargottama planet in your D9 Chart.  Martian qualities are good.</p>");
                                if (plh.Value.sign == "cn")
                                    vga += string.Format("<span>Due to the Mars is in its debilitation sign Cancer. The results can vary, will need a lot of training to be under control. The wrong use of any skill can damage your social relations, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "JUPITER":
                                vga += string.Format("<p>The Jupiter indicates wisdom, higher knowledge, higher studies, spirituality.  Jupiter is Vargottama planet in your D9 Chart. Jupiterian qualities will be very much evident in your life.</p>");
                                if (plh.Value.sign == "cp")
                                    vga += string.Format("<span>Due to the Jupiter is in its debilitation sign Capricorn. The results can vary, please talk to one of our expert astrologers for much detailed prediction & remedies</span>");
                                break;
                            case "SATURN":
                                vga += string.Format("<p>The Saturn indicates hindrance in physical development, longevity, careful, leads life by controlling desires, selfishness, Irresponsible, carelessness and exercises restraints</p>");
                                break;
                            case "RAHU":
                                vga += string.Format("<p>The Rahu is Vargottama in your D9 Chart which indicates rebelliousness, aggression, passion  and mental disposition.</p>");
                                break;
                            case "KETU":
                                vga += string.Format("<p>The Ketu is Vargottama in your D9 Chart which indicates isolation, detachment, spirituality.</p>");
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (vgms != string.Empty)
                {
                    desc += string.Format(" </p>In your D9 chart {0} are vargottama, As per ancient texts if the vargottama planet is aspected by benefic planets a powerful raja yoga is formed.</p>", vgms);
                    desc += vga;
                }
                desc += string.Format("<h2>Pushkar Navamsha</h2>");
                desc += string.Format("<p>It is a particular navamsa in a sign where planets behaves in auspicious manner.</p>");
                Dictionary<string, PlanetHouse> dctPushk = new Dictionary<string, PlanetHouse>();
                foreach (var plh in dctPlHou)
                {
                    switch (plh.Value.sign)
                    {
                        case "ar":
                            if (plh.Value.pos >= (double)20.0 && plh.Value.pos <= (double)23.20)
                            { //aries pushkar
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)26.40 && plh.Value.pos <= (double)30.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "ta":
                            if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)13.20 && plh.Value.pos <= (double)16.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "ge":
                            if (plh.Value.pos >= (double)16.40 && plh.Value.pos <= (double)20.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)23.20 && plh.Value.pos <= (double)26.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "cn":
                            if (plh.Value.pos >= (double)0.0 && plh.Value.pos <= (double)3.20)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "le":
                            if (plh.Value.pos >= (double)20.0 && plh.Value.pos <= (double)23.20)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)26.40 && plh.Value.pos <= (double)30.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "vi":
                            if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)13.20 && plh.Value.pos <= (double)16.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "li":
                            if (plh.Value.pos >= (double)16.40 && plh.Value.pos <= (double)20.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)23.20 && plh.Value.pos <= (double)26.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "sc":
                            if (plh.Value.pos >= (double)0.0 && plh.Value.pos <= (double)3.20)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "sa":
                            if (plh.Value.pos >= (double)20.0 && plh.Value.pos <= (double)23.20)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)26.40 && plh.Value.pos <= (double)30.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "cp":
                            if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)13.20 && plh.Value.pos <= (double)16.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "aq":
                            if (plh.Value.pos >= (double)16.40 && plh.Value.pos <= (double)20.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)23.20 && plh.Value.pos <= (double)26.40)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        case "pi":
                            if (plh.Value.pos >= (double)0.0 && plh.Value.pos <= (double)3.20)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            else if (plh.Value.pos >= (double)6.40 && plh.Value.pos <= (double)10.0)
                            {
                                dctPushk.Add(plh.Key, plh.Value);
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (dctDPlHou["Ju"].hno == dctDPlHou["Ma"].hno)
                {
                    if (dctPushk.ContainsKey("Mo"))
                    {
                        desc += string.Format("<p>In your D9 Chart Jupiter & Mars conjunct in {0} and Moon is in Pushkar Navamsha which is a powerful combination, according to Jataka Parijata chapter 7 verse 25 a person with such palcememt in horoscope will rule over masses.</p>", dctDPlHou["Ju"].sign);
                    }
                    else if (vgms.Contains("Mo"))
                    {
                        desc += string.Format("<p>In your D9 Chart Jupiter & Mars conjunct in {0} and Moon is Vargottama which is a powerful combination, according to Jataka Parijata chapter 7 verse 25 a person with such palcememt in horoscope will rule over masses.</p>", dctDPlHou["Ju"].sign);
                    }
                }
                foreach (var psh in dctPushk)
                {
                    if (psh.Value.hno == 1)
                    {
                        desc += string.Format("<p>In your D9 Chart Vargottama {0} is in Ascendant, such placement will give you high success in life</p>", psh.Value.name);
                    }
                }
                foreach (var dpl in dctDPlHou)
                {
                    if (dpl.Value.hno == 5 || dpl.Value.hno == 9 || dpl.Value.hno == 10)
                    {
                        if (dctPushk.ContainsKey(dpl.Value.houselord))
                        {
                            desc += string.Format("<p>In your D9 Chart, {0}th house lord {1} is in Pushkara Navamsha, such a placement will give you good succss in its dasha/bhukthi</p>", dpl.Value.hno, dctPushk[dpl.Value.houselord].name);
                        }
                    }
                    if (dctPushk.ContainsKey("Ju"))
                    {
                        if ((dpl.Key == "Ju") && (dpl.Value.hno == 1 || dpl.Value.hno == 4 || dpl.Value.hno == 7 || dpl.Value.hno == 10))
                        {
                            desc += string.Format("<p>In your D9 Chart, Jupiter in Pushkara Navamsha and in Angular house in {0} such a placement makes the native very wealthy.</p>", dpl.Value.sign);
                        }
                        else if ((dpl.Key == "Ju") && (dpl.Value.hno == 5 || dpl.Value.hno == 9))
                        {
                            desc += string.Format("<p>In your D9 Chart, Jupiter in Pushkara Navamsha and in Trine house in {0} such a placement makes the native very wealthy.</p>", dpl.Value.sign);
                        }
                    }
                    if (dctPushk.ContainsKey(dpl.Key))
                    {
                        if (dpl.Value.hno == 6 || dpl.Value.hno == 8 || dpl.Value.hno == 12)
                        {
                            desc += string.Format("<p>In your D9 Chart, the Pushkara Navamsha Planet {0} placed in dustana {1} house, such a placement may cause in addition the native being wealthy would experience some health issues, please talk to one of our expert astrologers for remedies.</p>", dpl.Value.name, dpl.Value.hno);
                        }
                    }
                }
                double asc_dp = Convert.ToDouble(string.Format("{0}.{1}", asc_pos.Split('.')[0], asc_pos.Split('.')[1]));
                bool asc_pus = false;
                switch (asc_h.Split('|')[0])
                {
                    case "ar":
                        if (asc_dp >= (double)20.0 && asc_dp <= (double)23.20)
                        { //aries pushkar
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)26.40 && asc_dp <= (double)30.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "ta":
                        if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)13.20 && asc_dp <= (double)16.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "ge":
                        if (asc_dp >= (double)16.40 && asc_dp <= (double)20.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)23.20 && asc_dp <= (double)26.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "cn":
                        if (asc_dp >= (double)0.0 && asc_dp <= (double)3.20)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "le":
                        if (asc_dp >= (double)20.0 && asc_dp <= (double)23.20)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)26.40 && asc_dp <= (double)30.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "vi":
                        if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)13.20 && asc_dp <= (double)16.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "li":
                        if (asc_dp >= (double)16.40 && asc_dp <= (double)20.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)23.20 && asc_dp <= (double)26.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "sc":
                        if (asc_dp >= (double)0.0 && asc_dp <= (double)3.20)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "sa":
                        if (asc_dp >= (double)20.0 && asc_dp <= (double)23.20)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)26.40 && asc_dp <= (double)30.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "cp":
                        if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)13.20 && asc_dp <= (double)16.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "aq":
                        if (asc_dp >= (double)16.40 && asc_dp <= (double)20.0)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)23.20 && asc_dp <= (double)26.40)
                        {
                            asc_pus = true;
                        }
                        break;
                    case "pi":
                        if (asc_dp >= (double)0.0 && asc_dp <= (double)3.20)
                        {
                            asc_pus = true;
                        }
                        else if (asc_dp >= (double)6.40 && asc_dp <= (double)10.0)
                        {
                            asc_pus = true;
                        }
                        break;
                    default:
                        break;
                }
                if (dctPushk.ContainsKey(asc_h.Split('|')[2]) && asc_pus && dctPushk.ContainsKey(hou10.Split('|')[2]))
                {//lagna, lagna lord & 10th lord in puskara
                    desc += string.Format("<p>The Ascendant Lord {0} & 10th Lord {1} of your Rashi Chart are in Pushkara Navamsa in D9 Chart. Such a placement will make the native very fortunate.</p>", dctPlHou[asc_h.Split('|')[2]].name, dctPlHou[hou10.Split('|')[2]].name);
                }
                else if (dctPushk.ContainsKey(asc_h.Split('|')[2]))
                {
                    desc += string.Format("<p>The Ascendant Lord {0} of your Rashi Chart in Pushkara Navamsa, such a placement produces very good results during its Dasha & Bhukti.</p>", dctPlHou[asc_h.Split('|')[2]].name);
                }
                int npk = dctPushk.Count();
                string spk = string.Empty;
                foreach (var puk in dctPushk)
                {
                    spk += puk.Value.name + ",";
                }
                if (spk != string.Empty)
                {
                    desc += string.Format("<p>In your D9 chart, {0} are in Pushkara Navamsa. You can expect good times during dasha/bhukti of these planet(s).</p>", spk);
                }
                desc += string.Format("<h2>Maritial Happiness</h2>");
                desc += string.Format("<p>By virtue of inherent nature Mars, the planet of vigour and passion, and Venus, the planet of love and conjugal relationship create mutual attraction between the opposite sex. The Moon representing mind (Chandrama manso jatah) shows the inclination, Jupiter blesses the two with the bond of marriage, and Saturn, as lord of 'time; solemnises marriage at an appropriate time. As a restrictive planet, Saturn also inculcates faithfulness, controls amorous digressions, and ensures lasting bond of marriage. The astrological principles discussed in this article apply to both the males and females.</p>");
                desc += string.Format("<p>A person enters Grahastha Ashram, or becomes a house holder, on getting married. It is an important stage of life and every eligible bachelor has high expectations from his marriage and spouse. Although it is said that the marriages are settled in heaven, yet the Vedic science of Astrology unfolds a clear picture about the married life of an individual, whether his expectations will be fulfilled or not.</p>");
                desc += string.Format("<p>For ascertaining marriage prospects of an individual the factors to be examined are primarily the 7th house (marriage), and also 2nd house (family, and longevity of spouse being 8th from 7th house), the 8th house (mangalya, or marital happiness), 4th house( general happiness), the 5th house (love and progeny), 12th house (bed comforts) together with Moon (mind) and Venus, significator for marriage and conjugal bliss (Kalatra Karaka). In the case of girls, Jupiter, significator for husband, is given prime importance. The benefic association or aspect of Jupiter on the 7th house and the 7th lord indicates a virtuous and loyal spouse.</p>");
                Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                JsonResult jO = (JsonResult)GetAspects(horo, dctPlHou[hou7.Split('|')[2]].sign);
                Dictionary<int, string> dctAsp = (Dictionary<int, string>)(jO.Value);
                bool is7ben = false, is7mel = false;
                string mel_pl = string.Empty, ben_pl = string.Empty;
                PlanetStrength pS7L = checkStrength(dctDPlHou[houT7.Split('|')[2]]);
                PlanetStrength pSKVe = checkStrength(dctDPlHou["Ve"]);
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        int ben = isBenefic(dctPlHou[hou7.Split('|')[2]].sign, lrd);
                        switch (ben)
                        {
                            case 1:
                                is7ben = true;
                                ben_pl += dctPlNames[lrd] + ",";
                                break;
                            case 2:
                                is7mel = true;
                                mel_pl += dctPlNames[lrd] + ",";
                                break;
                            case 0:
                                break;
                            default:
                                break;
                        }
                    }
                }
                //Horo horo = new Horo();
                horo.planetPos = mHoro.planetsPos;
                JsonResult jO8 = (JsonResult)GetAspects(horo, dctPlHou[hou8.Split('|')[2]].sign);
                dctAsp = (Dictionary<int, string>)(jO8.Value);
                bool is8ben = false, is8mel = false;
                bool b7s = false, b8s = false;
                bool b7Le = false, kVe = false;
                mel_pl = string.Empty;
                ben_pl = string.Empty;
                foreach (var asp in dctAsp)
                {
                    foreach (var lrd in asp.Value.Split('|'))
                    {
                        int ben = isBenefic(dctPlHou[hou8.Split('|')[2]].sign, lrd);
                        switch (ben)
                        {
                            case 1:
                                is8ben = true;
                                ben_pl += dctPlNames[lrd] + ",";
                                break;
                            case 2:
                                is8mel = true;
                                mel_pl += dctPlNames[lrd] + ",";
                                break;
                            case 0:
                                break;
                            default:
                                break;
                        }
                    }
                }
                PlanetStrength pS7 = checkStrength(dctPlHou[hou7.Split('|')[2]]);
                PlanetStrength pS8 = checkStrength(dctPlHou[hou8.Split('|')[2]]);
                if (pS7L == PlanetStrength.DEBILIATED || pS7L == PlanetStrength.ENEMY) b7Le = true;
                if (pSKVe == PlanetStrength.DEBILIATED || pSKVe == PlanetStrength.ENEMY) kVe = true;
                if (pS7 == PlanetStrength.EXALTED || pS7 == PlanetStrength.MOOLTRIKONA || pS7 == PlanetStrength.OWN || pS7 == PlanetStrength.FRIEND)
                {
                    b7s = true;
                }
                if (pS8 == PlanetStrength.EXALTED || pS8 == PlanetStrength.MOOLTRIKONA || pS8 == PlanetStrength.OWN || pS8 == PlanetStrength.FRIEND)
                {
                    b8s = true;
                }
                PlanetStrength pSVe = checkStrength(dctPlHou["Ve"]);
                PlanetStrength pSMo = checkStrength(dctPlHou["Mo"]);
                bool bVs = false, bMs = false;
                if (pSVe == PlanetStrength.EXALTED || pSVe == PlanetStrength.MOOLTRIKONA || pSVe == PlanetStrength.OWN || pSVe == PlanetStrength.FRIEND)
                {
                    bVs = true;
                }
                if (pSMo == PlanetStrength.EXALTED || pSMo == PlanetStrength.MOOLTRIKONA || pSMo == PlanetStrength.OWN || pSMo == PlanetStrength.FRIEND)
                {
                    bMs = true;
                }
                if (is7ben && is8ben && b7s && b8s && bVs && bMs)
                {
                    desc += string.Format("<p>In your Rashi Chart 7th lord {0} & 8th lord {1} & the Karaks {2} {3}. And Whereas 7th & 8th house has benefic aspects. Such a placement in horoscope causes the Married life to be very happy & harmonic.</p>", descStrength(dctPlHou[hou7.Split('|')[2]]), descStrength(dctPlHou[hou8.Split('|')[2]]), descStrength(dctPlHou["Ve"]), descStrength(dctPlHou["Mo"]));
                }
                else if (!is7ben && is7mel)
                {
                    desc += string.Format("<p>In your Rashi Chart 7th house has malefic aspect, may cause some issues please consult our expert astrologers for remedies</p>");
                }
                if (pSVe == PlanetStrength.DEBILIATED || pSVe == PlanetStrength.ENEMY)
                {
                    desc += string.Format("<p>In your Rashi Chart the Kalatra Karaka(Significator for Marriage & conjugal bliss) {0}, may cause some issues in maritial bliss please consult our expert astrologers for remedies</p>", descStrength(dctPlHou["Ve"]));
                }
                if (pSMo == PlanetStrength.DEBILIATED || pSMo == PlanetStrength.ENEMY)
                {
                    desc += string.Format("<p>In your Rashi Chart the Karak of Mind {0}, may cause some issues please consult our expert astrologers for remedies</p>", descStrength(dctPlHou["Mo"]));
                }
                desc += string.Format("<h3>Are you Manglik?<h3>");
                desc += string.Format("<p>As per ancient texts, if Mars is positioned in 1st,2nd,4th,7th,8th or 12th in Rashi Chart from both Ascendant & Moon, the native is considered to have Manglik Dosh. A native with Manglik Dosh is said to experince difficulties in the married life. However there are many cancellation rules which an expert astrologer should be able to judge by assessing various aspects from the horoscope. Please consult an expert astrologer for detailed analysis. </p>");
                if (dctPlHou["Ma"].hno == 1 || dctPlHou["Ma"].hno == 2 || dctPlHou["Ma"].hno == 4 || dctPlHou["Ma"].hno == 7 || dctPlHou["Ma"].hno == 8 || dctPlHou["Ma"].hno == 12)
                {
                    if (dctPlHou["Ma"].mhno == 1 || dctPlHou["Ma"].mhno == 2 || dctPlHou["Ma"].mhno == 4 || dctPlHou["Ma"].mhno == 7 || dctPlHou["Ma"].mhno == 8 || dctPlHou["Ma"].mhno == 12)
                    {
                        desc += string.Format("<p>In your horoscope Mars is positioned in {0} house from Lagna/Ascendant & in {1} house from Moon such a placement in your horoscope is considered as Manglik Dosha. Please note there may be cancellation rules which nullifies Manglik Dosha, please talk to our expert astrologers to know more. Our Astrologers should be able to provide more appropriate remedies for Manglik Dosha.</p>", dctPlHou["Ma"].hno, dctPlHou["Ma"].mhno);
                    }
                    else
                    {
                        desc += string.Format("<p>In your horoscope Mars is positioned in {0} house from Lagna/Ascendant & whereas its position from Moon is away from Manglik effect, hence only a partial Manglik Dosha present in your horoscope. Please talk to our expert astrologers to know more. Our Astrologers should be able to provide more appropriate remedies for Manglik Dosha.</p>", dctPlHou["Ma"].hno);
                    }
                }
                else
                {
                    desc += string.Format("<p>In your Rashi Chart Mars is positioned in {0} house which is not among the houses considered to be Manglik. Hence you are free from Manglik Dosh</p>", dctPlHou["Ma"].hno);
                }
                desc += string.Format("<h3>Knowing Your Spouse</h3>");
                if (dctPlHou["Su"].hno == 7)
                {
                    desc += string.Format("<p>In your rashi chart Sun is in 7th house which indicates your spouse is from a higher status family, is proud and dominating. Please consult our expert astrologers for effective remedies.</p>");
                }
                if (dctPlHou["Ma"].hno == 7)
                {
                    desc += string.Format("<p>In your rashi chart Mars is in 7th house which indicates harsh & cruel spouse. Please consult our expert astrologers for effective remedies.</p>");
                }
                if (dctPlHou["Sa"].hno == 7)
                {
                    desc += string.Format("<p>In your rashi chart Saturn is in 7th house such placement would cause the spouse to be morose & undersexed. Marriage is delayed or with a widower or with one not good looking. Please consult our expert astrologers for effective remedies.</p>");
                }
                if (dctPlHou["Ra"].hno == 7)
                {
                    desc += string.Format("<p>In your rashi chart Rahu is in 7th house which indicates late marriage, the wife comes from a low family, is sickly and the native has relationship with low caste women. Please consult our expert astrologers for effective remedies.</p>");
                }
                if (dctPlHou["Ke"].hno == 7 && dctPlHou["Ke"].sign != "sc")
                {
                    desc += string.Format("<p>In your rashi chart Ketu is in 7th house which indicates unhappy married life. Please consult our expert astrologers for effective remedies.</p>");
                }
                if (dctPlHou["Me"].hno == 7 || dctPlHou["Mo"].hno == 7)
                {
                    bool stongMe = false, stongMo = false;
                    if (dctPlHou["Me"].hno == 7)
                    {
                        PlanetStrength pSM = checkStrength(dctPlHou["Me"]);
                        if (pSM == PlanetStrength.EXALTED || pSM == PlanetStrength.MOOLTRIKONA || pSM == PlanetStrength.OWN || pSM == PlanetStrength.FRIEND)
                        {
                            stongMe = true;
                        }
                    }
                    if (dctPlHou["Mo"].hno == 7)
                    {
                        PlanetStrength pSM = checkStrength(dctPlHou["Mo"]);
                        if (pSM == PlanetStrength.EXALTED || pSM == PlanetStrength.MOOLTRIKONA || pSM == PlanetStrength.OWN || pSM == PlanetStrength.FRIEND)
                        {
                            stongMo = true;
                        }
                    }
                    if (is7ben || stongMe || stongMo)
                    {
                        string csts = string.Empty;
                        //if(is7ben) {
                        if (dctPlHou["Me"].hno == 7 && stongMe)
                            csts += descStrength(dctPlHou["Me"]);
                        if (dctPlHou["Mo"].hno == 7 && stongMo)
                            csts += descStrength(dctPlHou["Mo"]);
                        if (is7ben)
                            csts += " & 7th house has benefic aspect";
                        //}
                        desc += string.Format("<p>In your rashi chart {0}, such a placement indicates young beautiful & intelligent spouse.</p>", csts);
                    }
                    if (dctPlHou["Ve"].hno == 7)
                    {
                        desc += string.Format("<p>In your rashi chart Venus is in 7th house which indicates handsome but overindulgent spouse.</p>");
                    }
                    if (dctPlHou["Ju"].hno == 7)
                    {
                        desc += string.Format("<p>In your rashi chart Jupier is in 7th house which indicates religious & capable spouse.</p>");
                    }

                    if (b7Le && kVe)
                    {
                        horo.planetPos = plPos;
                        JsonResult jO7 = (JsonResult)GetAspects(horo, dctDPlHou[houT7.Split('|')[2]].sign);
                        dctAsp = (Dictionary<int, string>)(jO7.Value);
                        bool ben7 = false;
                        foreach (var asp in dctAsp)
                        {
                            foreach (var lrd in asp.Value.Split('|'))
                            {
                                if (isBEN(lrd)) ben7 = true;
                            }
                        }
                        if (dctDPlHou[houT7.Split('|')[2]].sign != dctDPlHou["Ve"].sign)
                        {
                            jO7 = (JsonResult)GetAspects(horo, dctDPlHou["Ve"].sign);
                            dctAsp = (Dictionary<int, string>)(jO7.Value);
                            foreach (var asp in dctAsp)
                            {
                                foreach (var lrd in asp.Value.Split('|'))
                                {
                                    if (isBEN(lrd)) ben7 = true;
                                }
                            }
                        }
                        if (!ben7)
                            desc += string.Format("<p>In your D9 Chart, the 7th Lord {0} & Karak {1} and is devoid of any benefic association, such a placement in D9 Chart indicates problems in marriage, please consult our expert astrlogers for more detailed analyis and remedies.</p>", descStrength(dctPlHou[houT7.Split('|')[2]]), descStrength(dctDPlHou["Ve"]));
                    }
                }

                desc += string.Format("<p>As per Ancient Texts, the placement of 7th lord in D9 Chart gives a clue regarding spouse characteristics</p>");
                PlanetHouse pH7L = dctDPlHou[houT7.Split('|')[2]];
                if (pH7L.hno == 1 && isBEN(pH7L.code) && dctFriends[pH7L.code].Contains(asc_D.Split('|')[2]))
                {
                    desc += string.Format("<p>In your D9 Chart, the benefic 7th Lord {0} in 1st House & is friendly to Lagna Lord {1}, such a combinaton indicates the wife comes from known family, is helpful to the native & they are happy</p>", pH7L.name, dctPlNames[asc_D.Split('|')[2]]);
                }
                if (pH7L.hno == 1 && dctDPlHou[asc_D.Split('|')[2]].hno == 1)
                {
                    desc += string.Format("<p>In your D9 Chart both 7th Lord {0} && Lagna Lord {1} are in Lagna/Ascendant which indicates early marriage.</p>", pH7L.name, dctPlNames[asc_D.Split('|')[2]]);
                }
                int bn = isBenefic(pH7L.sign, pH7L.code);
                if (pH7L.hno == 7 && bn == 2 && b7Le)
                {
                    desc += string.Format("<p>In your D9 Chart, the 7th Lord {0} & is placed in Lagna/Ascendant which indicates danger to first wife, second wife survives.</p>", descStrength(pH7L));
                }
                if (pH7L.hno == 2)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th lord {0} in 2nd house which indicate wealthy spouse.</p>", pH7L.name);
                    if (b7Le)
                    {
                        desc += string.Format("<p>But also in your D9 Chart, 7th Lord {0}, which also indicates finacial crunch after marriage, second marriage.</p>", descStrength(pH7L));
                    }
                }
                if (pH7L.hno == 3)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 3rd house. Which indicates spouse is a souce of strength to the native.</p>", pH7L.name);
                    if (b7Le)
                        desc += string.Format("<p>But also 7th lord {0}, which may indicate the wife is attacted towards native's younger brother.", descStrength(pH7L));
                }
                if (pH7L.hno == 4)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 4th house, which indicates the wife is good natured and helpful to the native who becomes prosperous.</p>", pH7L.name);
                    if (b7Le)
                        desc += string.Format("<p>But also 7th Lord {0}, which may indicate unhappyness to the native.</p>", descStrength(pH7L));
                }
                if (pH7L.hno == 5)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 5th house, which indicates the wife is cultured and religious and begets him many children.</p>", pH7L.name);
                    if (b7Le)
                        desc += string.Format("<p>But also 7th Lord {0}, which may indicate wife will be cruel & obstinate.</p>", descStrength(pH7L));
                }
                if (pH7L.hno == 6)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 6th house, which indicates the wife is sickly. The native has to spend a lot on his wife and incurs debt. He may also fall sick due to excessive indulgence</p>", pH7L.name);
                }
                if (pH7L.hno == 7)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 7th house, which indicates the native marries early and leads a happy life with a capable wife.</p>", pH7L.name);
                    if (b7Le && (bn == 2))
                        desc += string.Format("<p>But also 7th Lord {0} and is malefic, which indicates there maybe affairs outside marriage.</p>", descStrength(pH7L));
                }
                if (pH7L.hno == 8)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 8th house, which indicates marriage is delayed, spouse is sickly, married life is unhappy.</p>", pH7L.name);
                    bool b8H = isBEN(dctDPlHou[houT8.Split('|')[2]].name);
                    bool b8L = isBEN(dctDPlHou[houT8.Split('|')[2]].houselord);
                    if (b8H && b8L)
                        desc += string.Format("<p>But since 8th house & 8th lord has benefic aspect, the malefic effect is nullified.</p>");
                }
                if (pH7L.hno == 9)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 9th house, which indicates the wife brings luck to the native whose fortune raises after marriage.</p>", pH7L.name);
                }
                if (pH7L.hno == 10)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 10th house, which indicates the wife comes from good background and proves helpful in the natives profession/enterprise or maybe a working lady and of independent nature.</p>", pH7L.name);
                }
                if (pH7L.hno == 11)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 11th house, which indicates the married life will be happy, gains through spouse.</p>", pH7L.name);
                }
                if (pH7L.hno == 12)
                {
                    desc += string.Format("<p>In your D9 Chart, 7th Lord {0} is in 12th house, which indicates marriage is delayed, there is a huge expenditure on spouse.</p>", pH7L.name);
                    if (b7Le)
                        desc += string.Format("<p>But also 7th Lord {0}, which may indicate death of spouse or seperation.</p>", descStrength(pH7L));
                }

                dctYogs.Add("D9A", desc);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                // return Json(string.Format("ERROR: {0} LINE {1}", eX.Message, line), JsonRequestBehavior.AllowGet);

                dctYogs.Add("ERROR", string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
            return new JsonResult(dctYogs);
        }
        string descStrength(PlanetHouse pl)
        {
            string desc = string.Empty;
            //bool strong_asc = false;
            PlanetStrength pS = PlanetStrength.NORMAL;
            switch (pl.code)
            {
                case "Su":
                    if (pl.sign == "ar")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "le")
                    {
                        if (pl.pos >= 4 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                        else
                        {
                            pS = PlanetStrength.OWN;
                        }
                    }
                    else if (pl.sign == "le")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "cn" || pl.sign == "ar" || pl.sign == "sa" || pl.sign == "sc")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "cp" || pl.sign == "aq") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "li") pS = PlanetStrength.DEBILIATED;
                    break;
                case "Mo":
                    if (pl.sign == "ta")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                        desc += "Moon is positioned in its exalted sign which is excellent!";
                    }
                    else if (pl.sign == "ta")
                    {
                        if (pl.pos >= 4 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                            desc += "Moon is positioned in its mooltrikona sign which is excellent!";
                        }
                        else
                        {
                            desc += "Moon is positioned in its own sign which is excellent!";
                            pS = PlanetStrength.OWN;
                        }
                    }
                    else if (pl.sign == "cn")
                    {//own
                        pS = PlanetStrength.OWN;
                        desc += "Moon is positioned in its own sign which is very good!";
                    }
                    else if (pl.sign == "le" || pl.sign == "ar" || pl.sign == "sc" || pl.sign == "sa" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                        desc += "Moon is positioned in its friendly sign which is good!";
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "ge" || pl.sign == "vi" || pl.sign == "sa" || pl.sign == "aq")
                    {//enemy house
                        pS = PlanetStrength.ENEMY;
                        desc += "Moon is positioned in its emeny sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    else if (pl.sign == "sc")
                    {
                        pS = PlanetStrength.DEBILIATED;
                        desc += "Moon is positioned in its debiliated sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    break;
                case "Ju":
                    if (pl.sign == "cn")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                        desc += "Jupiter is positioned in its exalted sign which is excellent!";
                    }
                    else if (pl.sign == "sa")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 10)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                            desc += "Jupiter is positioned in its mooltrikona sign which is excellent!";
                        }
                        else
                        {
                            desc += "Jupiter is positioned in its own sign which is excellent!";
                            pS = PlanetStrength.OWN;
                        }
                    }
                    else if (pl.sign == "pi")
                    {//own
                        pS = PlanetStrength.OWN;
                        desc += "Jupiter is positioned in its own sign which is very good!";
                    }
                    else if (pl.sign == "le" || pl.sign == "sc" || pl.sign == "ar")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                        desc += "Jupiter is positioned in its friendly sign which is good!";
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "ta" || pl.sign == "li")
                    {
                        pS = PlanetStrength.ENEMY;
                        desc += "Jupiter is positioned in its emeny sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    else if (pl.sign == "cp")
                    {
                        pS = PlanetStrength.DEBILIATED;
                        desc += "Jupiter is positioned in its debiliated sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    if (pS == PlanetStrength.EXALTED || pS == PlanetStrength.MOOLTRIKONA || pS == PlanetStrength.OWN)
                    {
                        desc += " Native is very lucky.";
                    }
                    break;
                case "Ve":
                    if (pl.sign == "pi")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                        desc += "Venus is positioned in its exalted sign which is excellent!";
                    }
                    else if (pl.sign == "li")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 15)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                            desc += "Venus is positioned in its mooltrikona sign which is excellent!";
                        }
                        else
                        {
                            pS = PlanetStrength.OWN;
                            desc += "Venus is positioned in its own sign which is excellent!";
                        }
                    }
                    else if (pl.sign == "ta")
                    {//own
                        pS = PlanetStrength.OWN;
                        desc += "Venus is positioned in its own sign which is very good!";
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "cp" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                        desc += "Venus is positioned in its friendly sign which is good.";
                    }
                    else if (pl.sign == "le" || pl.sign == "ta" || pl.sign == "li")
                    {
                        pS = PlanetStrength.ENEMY;
                        desc += "Venus is positioned in its enemy sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    else if (pl.sign == "sc")
                    {
                        pS = PlanetStrength.DEBILIATED;
                        desc += "Venus is positioned in its debiliated sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    break;
                case "Ma":
                    if (pl.sign == "cp")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "ar")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 12)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                        else
                        {
                            pS = PlanetStrength.OWN;
                        }
                    }
                    else if (pl.sign == "sc")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "cp" || pl.sign == "aq") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "le" || pl.sign == "cn" || pl.sign == "sa" || pl.sign == "pi")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    break;
                case "Me":
                    if (pl.sign == "vi")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                        desc += "Mercury is positioned in its exalted sign which is excellent!";
                    }
                    else if (pl.sign == "vi")
                    {//own
                        if (pl.pos >= 16 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                            desc += "Mercury is positioned in its mooltrikona sign which is excellent!";
                        }
                        else
                        {
                            pS = PlanetStrength.OWN;
                            desc += "Mercury is positioned in its own sign which is excellent!";
                        }
                    }
                    else if (pl.sign == "ge")
                    {//own
                        pS = PlanetStrength.OWN;
                        desc += "Mercury is positioned in its own sign which is excellent!";
                    }
                    else if (pl.sign == "ta" || pl.sign == "li" || pl.sign == "cp" || pl.sign == "aq")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "sa" || pl.sign == "pi" || pl.sign == "ar" || pl.sign == "sc")
                    {//enemy house
                        pS = PlanetStrength.ENEMY;
                        desc += "Mercury is positioned in its enemy sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    else if (pl.sign == "pi")
                    {
                        pS = PlanetStrength.DEBILIATED;
                        desc += "Mercury is positioned in its debiliated sign, may require astrological measure, please check with one of our expert astrologers";
                    }
                    break;
                case "Sa":
                    if (pl.sign == "li")
                    {//exalted
                        pS = PlanetStrength.EXALTED;
                    }
                    else if (pl.sign == "aq")
                    {//own
                        if (pl.pos >= 0 && pl.pos <= 20)
                        {//mooltrikona
                            pS = PlanetStrength.MOOLTRIKONA;
                        }
                        else
                        {
                            pS = PlanetStrength.OWN;
                        }
                    }
                    else if (pl.sign == "cp")
                    {//own
                        pS = PlanetStrength.OWN;
                    }
                    else if (pl.sign == "ge" || pl.sign == "vi" || pl.sign == "ta")
                    {//friend house
                        pS = PlanetStrength.FRIEND;
                    }
                    else if (pl.sign == "le") pS = PlanetStrength.ENEMY;
                    else if (pl.sign == "ar") pS = PlanetStrength.DEBILIATED;
                    break;
                default:
                    break;
            }
            return desc;
        }
        public int isBenefic(string sign, string lord)
        {
            switch (sign)
            {
                case "ar":
                    if (lord == "Su" || lord == "Ju" || lord == "Mo") return 1;
                    else if (lord == "Me") return 2;
                    else if (lord == "Sa" || lord == "Ve" || lord == "Ma") return 0;
                    break;
                case "ta":
                    if (lord == "Sa" || lord == "Ve" || lord == "Me") return 1;
                    else if (lord == "Ju" || lord == "Mo" || lord == "Ma") return 2;
                    else if (lord == "Su") return 0;
                    break;
                case "ge":
                    if (lord == "Mo" || lord == "Me" || lord == "Su") return 1;
                    else if (lord == "Ju" || lord == "Ma") return 2;
                    else if (lord == "Mo" || lord == "Me" || lord == "Su") return 0;
                    break;
                case "cn":
                    if (lord == "Mo" || lord == "" || lord == "Mo") return 1;
                    else if (lord == "Me" || lord == "Sa") return 2;
                    else if (lord == "Su" || lord == "Ve" || lord == "Ju") return 0;
                    break;
                case "le":
                    if (lord == "Su" || lord == "Ma") return 1;
                    else if (lord == "Me" || lord == "Ve" || lord == "Mo" || lord == "Sa") return 2;
                    else if (lord == "Sa" || lord == "Ve" || lord == "Me") return 1;
                    break;
                case "ve":
                    if (lord == "Ve" || lord == "Me") return 1;
                    else if (lord == "Ju" || lord == "Mo" || lord == "Ma") return 2;
                    else if (lord == "Su") return 0;
                    break;
                case "li":
                    if (lord == "Sa" || lord == "Me") return 1;
                    else if (lord == "Ma" || lord == "Su" || lord == "Ju" || lord == "Ma") return 2;
                    else if (lord == "Mo" || lord == "Ve") return 0;
                    break;
                case "sc":
                    if (lord == "Mo" || lord == "Su" || lord == "Ju") return 1;
                    else if (lord == "Me" || lord == "Ve") return 2;
                    else if (lord == "Sa" || lord == "Ma") return 0;
                    break;
                case "sa":
                    if (lord == "Ma" || lord == "Su" || lord == "Ju") return 1;
                    else if (lord == "Me" || lord == "Ve" || lord == "Mo") return 2;
                    break;
                case "cp":
                    if (lord == "Ve" || lord == "Me" || lord == "Sa") return 1;
                    else if (lord == "Ma" || lord == "Mo" || lord == "Su") return 2;
                    else if (lord == "Ju") return 1;
                    break;
                case "aq":
                    if (lord == "Ve" || lord == "Sa") return 1;
                    else if (lord == "Ma" || lord == "Mo" || lord == "Ju") return 2;
                    else if (lord == "Su" || lord == "Me") return 0;
                    break;
                case "pi":
                    if (lord == "Mo" || lord == "Ju" || lord == "Ma") return 1;
                    else if (lord == "Me" || lord == "Su" || lord == "Ve") return 2;
                    else if (lord == "Sa") return 0;
                    break;
                default:
                    return -1;
                    break;
            }
            return -1;
        }
        [HttpGet("GetHouseGroup")]
        public ActionResult GetHouseGroup(string uuid)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var astUsers = db.GetCollection<KPHouseGroup>("KPHouseGroup");
                var filter = Builders<KPHouseGroup>.Filter.Eq("uuid", uuid);
                try
                {
                    long cnt = astUsers.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var hgrp = astUsers.Find<KPHouseGroup>(filter).FirstOrDefault();
                        return new JsonResult(hgrp);

                    }
                    else
                    {
                        var hgrp = new KPHouseGroup
                        {
                            uuid = uuid,
                            hgp = "not found"
                        };
                        return new JsonResult(hgrp);
                    }
                }
                catch (Exception eX)
                {
                    var hgrp = new KPHouseGroup
                    {
                        uuid = uuid,
                        hgp = eX.Message
                    };
                    return new JsonResult(hgrp);
                }

            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                string err = string.Format("ERROR: {0} LINE {1}", eX.Message, line);
                var hgrp = new KPHouseGroup
                {
                    uuid = uuid,
                    hgp = err
                };
                return new JsonResult(hgrp);
            }
        }
        [HttpPost("AddHouseGroup")]
        public ActionResult AddHouseGroup([FromBody]KPHouseGroup kphg)
        {
            try
            {
                var connectionString = "mongodb://mypub:vedichoroo@18.138.194.20/myypub";
                MongoClient client = new MongoClient(connectionString); // connect to localhost
                Console.WriteLine("Getting DB...");
                var db = client.GetDatabase("myypub");
                var dbHG = db.GetCollection<KPHouseGroup>("KPHouseGroup");
                var filter = Builders<KPHouseGroup>.Filter.Eq("uuid", kphg.uuid);
                try
                {
                    long cnt = dbHG.CountDocuments(filter);
                    if (cnt > 0L)
                    {
                        var hgrp = dbHG.Find<KPHouseGroup>(filter).FirstOrDefault();
                        var update = Builders<KPHouseGroup>.Update.Set("hgp", kphg.hgp);
                        hgrp = dbHG.FindOneAndUpdate<KPHouseGroup>(filter, update);
                        hgrp.hgp = "Success";
                        return new JsonResult(hgrp);
                    }
                    else
                    {
                        //var hgrp = new KPHouseGroup
                        //{
                          //  uuid = kphg.uuid,
                            //hgp = kphg.hgp
                        //};
                        dbHG.InsertOne(kphg);
                        kphg.hgp = "Success";
                        return new JsonResult(kphg);
                    }
                }
                catch (Exception eX)
                {
                    var hgrp = new KPHouseGroup
                    {
                        uuid = kphg.uuid,
                        hgp = eX.Message
                    };
                    return new JsonResult (hgrp);
                }
            }
            catch (Exception eX)
            {
                var hgrp = new KPHouseGroup
                {
                    uuid = kphg.uuid,
                    hgp = eX.Message
                };
                return new JsonResult(hgrp);
            }
        }
		[HttpGet("Astakvarga")]
		public async Task<IActionResult> Astakvarga(string dob, string tob, string latlng, string timezone, double tzofset, int ayanid)
		{
			Astakavarga akv = new Astakavarga();
			akv.akPts = new Dictionary<string, int>();
			akv.houSgn = new Dictionary<string, string>();
			try
			{
				string asg = string.Empty, ssg = string.Empty, msg = string.Empty, jsg = string.Empty, mesg = string.Empty, masg = string.Empty, vsg = string.Empty, sasg = string.Empty;
				Horo horo = null;
				Task tf = new Task(() =>
				{

					JsonResult jRes = (JsonResult)GetcuspsEx2(dob, tob, latlng, timezone, tzofset, ayanid);
					horo = (Horo)jRes.Value;

					//SUN
					string astClient = Path.Combine(_env.ContentRootPath, @"Content/astroclient");

					foreach (var pls in horo.planetPos)
					{
						foreach (var pl in pls.Value.Split('|'))
						{
							if (pl.Split(' ')[1] == "AC")
							{
								asg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Su")
							{
								ssg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Mo")
							{
								msg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Ju")
							{
								jsg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Me")
							{
								mesg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Ma")
							{
								masg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Ve")
							{
								vsg = pls.Key;
							}
							else if (pl.Split(' ')[1] == "Sa")
							{
								sasg = pls.Key;
							}
						}
					}
				});
				tf.RunSynchronously();
				Task.WaitAll(tf);
				//SUN
				Dictionary<string, int> t1 = await calcAkPts("Su", asg, calcHno(asg, ssg), horo);
				//MOON
				Dictionary<string, int> t2 = await calcAkPts("Mo", asg, calcHno(asg, msg), horo);
				
				//JUPITER
				Dictionary<string, int> t3 = await calcAkPts("Ju", asg, calcHno(asg, jsg), horo);
					
				//VENUS
				Dictionary<string, int> t4 = await calcAkPts("Ve", asg, calcHno(asg, vsg), horo);
					
				//MERCURY
				Dictionary<string, int> t5 = await calcAkPts("Me", asg, calcHno(asg, mesg), horo);
					
				//MARS
				Dictionary<string, int> t6 = await calcAkPts("Ma", asg, calcHno(asg, masg), horo);
					
				//SATURN
				Dictionary<string, int> t7 = await calcAkPts("Sa", asg, calcHno(asg, sasg), horo);
					
				//await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);

				t1.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t2.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t3.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t4.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t5.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t6.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				t7.ToList().ForEach(x => akv.akPts.Add(x.Key, x.Value));
				int r1 = 0, r2 = 0;
				bool asc = false;
				string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };
				for (r1 = 0; r1 < 12; r1++)
				{
					if (asc)
					{
						r2++;
					}
					if (horo.planetPos.ContainsKey(ras[r1]))
					{
						foreach (string pl in horo.planetPos[ras[r1]].Split('|'))
						{
							if (pl.Split(' ')[1] == "AC")
							{
								asc = true;
								r2++;
								akv.houSgn["1"] = ras[r1];
							}
						}
					}
					if (r2 == 2) akv.houSgn["2"] = ras[r1];
					else if (r2 == 3) akv.houSgn["3"] = ras[r1];
					else if (r2 == 4) akv.houSgn["4"] = ras[r1];
					else if (r2 == 5) akv.houSgn["5"] = ras[r1];
					else if (r2 == 6) akv.houSgn["6"] = ras[r1];
					else if (r2 == 7) akv.houSgn["7"] = ras[r1];
					else if (r2 == 8) akv.houSgn["8"] = ras[r1];
					else if (r2 == 9) akv.houSgn["9"] = ras[r1];
					else if (r2 == 10) akv.houSgn["10"] = ras[r1];
					else if (r2 == 11) akv.houSgn["11"] = ras[r1];
					else if (r2 == 12)
					{
						akv.houSgn["12"] = ras[r1];
						break;
					}
					if (r1 == 11) r1 = -1;
				}

				return new JsonResult(akv);
			}
			catch (Exception eX)
			{
				akv.akPts[eX.Message] = -1;
				return new JsonResult(akv);
			}

		}

		async Task<Dictionary<string, int>> calcAkPts(string lord, string asg, int asn, Horo horo)
		{
			try
			{
				//await Task.Run(() =>
				//{
					Dictionary<string, int> dctAk = new Dictionary<string, int>();
					dctAk[lord + "-1"] = 0;
					dctAk[lord + "-2"] = 0;
					dctAk[lord + "-3"] = 0;
					dctAk[lord + "-4"] = 0;
					dctAk[lord + "-5"] = 0;
					dctAk[lord + "-6"] = 0;
					dctAk[lord + "-7"] = 0;
					dctAk[lord + "-8"] = 0;
					dctAk[lord + "-9"] = 0;
					dctAk[lord + "-10"] = 0;
					dctAk[lord + "-11"] = 0;
					dctAk[lord + "-12"] = 0;
					string[] ras = { "ar", "ta", "ge", "cn", "le", "vi", "li", "sc", "sa", "cp", "aq", "pi" };

					string astClient = Path.Combine(_env.ContentRootPath, @"Content/astroclient");
					using (StreamReader rdr = new StreamReader(string.Format(@"{0}\{1}-bds.json", astClient, lord), Encoding.UTF8))
					{
						string dasp = rdr.ReadToEnd();
						Dictionary<string, string> dctLord = JsonConvert.DeserializeObject<Dictionary<string, string>>(dasp);
					
						string fsg = string.Empty;
						foreach (var pls in horo.planetPos)
						{
							foreach (var pl in pls.Value.Split('|'))
							{
								if (pl.Split(' ')[1] == lord)
								{
									fsg = pls.Key;
									break;
								}
							}
							if (fsg != string.Empty) break;
						}
						foreach (var itm in dctLord)
						{
							string[] hno = itm.Value.Split(',');
							foreach (var h in hno)
							{
								if (h == "1")
								{
									string ky = string.Format("{0}-{1}", lord, asn);
									if (dctAk.ContainsKey(ky))
										dctAk[ky] += 1;
									else
										dctAk[ky] = 1;
								}
								else
								{
									var sn = calcSHno(fsg, Convert.ToInt32(h));
									int hn = calcHno(asg, sn.Split('|')[0]);
									string ky = string.Format("{0}-{1}", lord, hn);
									if (dctAk.ContainsKey(ky))
										dctAk[ky] += 1;
									else
										dctAk[ky] = 1;
								}
							}
						}
					}
					return dctAk;
				//});
			}
			catch(Exception eX)
			{
				Dictionary<string, int> dE = new Dictionary<string, int>();
				dE[eX.Message] = -1;
				return dE;
			}
		}
		[HttpGet("AstroStories")]
        public ActionResult AstroStories()
        {
            try
            {
                //var posts = Enumerable.Empty<FeedItem>();
                var rssFeed = XDocument.Load(@"http://www.126news.com/Publish/astrology.xml");
                if (rssFeed != null)
                {
                    var posts = from item in rssFeed.Descendants("item")
                                select new FeedItem
                                {
                                    title = item.Element("title").Value,
                                    image = item.Element("enclosure").Attribute("url").Value,
                                    pubDate = item.Element("pubDate").Value,
                                    content = item.Element("description").Value
                                };
                    return new JsonResult(posts.ToList());
                }
                else
                {
                    return new JsonResult("ERROR: NOT FOUND");
                }
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }
        [HttpGet("LatestNews")]
        public ActionResult LatestNews(string category)
        {
            int np = 0;
            string sp = string.Empty;
            try
            {
                //var posts = Enumerable.Empty<FeedItem>();
                var rssFeed = XDocument.Load(string.Format(@"http://www.126news.com/Publish/{0}.xml", category));

                var posts = from item in rssFeed.Descendants("item")
                            select new FeedItem
                            {
                                title = item.Element("title").Value,
                                image = item.Element("enclosure").Attribute("url").Value,
                                pubDate = item.Element("pubDate").Value,
                                content = item.Element("description").Value
                            };
                foreach (var post in posts)
                {
                    np++;
                    sp = post.content;
                    Console.WriteLine(post.content);
                }
                return new JsonResult(posts);
            }
            catch (Exception eX)
            {
                var st = new StackTrace(eX, true);
                // Get the top stack frame
                var frame = st.GetFrame(st.FrameCount - 1);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                return new JsonResult(string.Format("ERROR: {0} LINE {1}", eX.Message, line));
            }
        }


    }
}