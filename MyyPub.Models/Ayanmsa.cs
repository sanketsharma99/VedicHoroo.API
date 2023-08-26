using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyyPub.Models
{
    public enum AYANMSAS {
        BVRAMAN = 1,
        KPOLD,
        KPNEW,
        LAHIRI,
        KHULLAR,
        FAGAN
    };
    public class Ayanmsa
    {
        public static string CalcEx(int d, int m, int y, double tz, AYANMSAS ay)
        {
            double ayd = 0.0;
            switch (ay)
            {
                case AYANMSAS.BVRAMAN:
                    ayd = ramanAyanEx(d, m, y, tz);
                    break;
                case AYANMSAS.KPNEW:
                    ayd = kpayannew(d, m, y);
                    break;
                case AYANMSAS.KPOLD:
                    ayd = kpayanold(d, m, y);
                    break;
                case AYANMSAS.LAHIRI:
                    ayd = lahiriAyanEx(d, m, y, tz);
                    break;
                case AYANMSAS.KHULLAR:
                    ayd = kpayankhu(d, m, y);
                    break;
                default:
                    break;
            }
            return dms(ayd);
        }
        public static string Calc(int d, int m, int y, AYANMSAS ay)
        {
            double ayd = 0.0;
            switch (ay)
            {
                case AYANMSAS.BVRAMAN:
                    ayd = ramanAyan(d, m, y);
                    break;
                case AYANMSAS.KPNEW:
                    ayd = kpayannew(d, m, y);
                    break;
                case AYANMSAS.KPOLD:
                    ayd = kpayanold(d, m, y);
                    break;
                case AYANMSAS.LAHIRI:
                    ayd = lahiriAyan(d, m, y);
                    break;
                case AYANMSAS.KHULLAR:
                    ayd = kpayankhu(d, m, y);
                    break;
                default:
                    break;
            }
            return dms(ayd);
        }
        static double jd(int d, int m, int y) { 
            double a;
            double j; 
            double l; 
            double b; 
            if (m < 3) { 
                m += 12; 
                y--;
            }
             a = y / 100; 
            b = (30.6) *(m + 1); 
            l = (int)(b); 
            j = 365 * y + y / 4 + l + 2 - a + a / 4 + d; 
            return j;
        }
        static double calculateB6(int d, int m, int y) { 
            double h, mt, s, h6, b6, timeZone; 
            h = 12; 
            mt = 0; 
            s = 0; 
            timeZone = 5.5; 
            h6 = (h + mt / 60 + s / 3600 - (12 + timeZone)) / 24; 
            b6 = (jd(d, m, y) - 694025 + h6) / 36525; 
            return b6;
        }
        static double calculateB6Ex(int d, int m, int y, double tz)
        {
            double h, mt, s, h6, b6;
            h = 12;
            mt = 0;
            s = 0;
            //timeZone = 5.5;
            h6 = (h + mt / 60 + s / 3600 - (12 + tz)) / 24;
            b6 = (jd(d, m, y) - 694025 + h6) / 36525;
            return b6;
        }
        static double ramanAyanEx(int dd, int mm, int yy, double tz)
        {
            return 21.013972 + 1.398191 * calculateB6(dd, mm, yy);
        }
        static double lahiriAyanEx(int dd, int mm, int yy, double tz)
        {
            return 22.460148 + 1.396042 * calculateB6Ex(dd, mm, yy, tz) + 3.08E-4 * calculateB6Ex(dd, mm, yy, tz) * calculateB6Ex(dd, mm, yy, tz);
        }
        static double ramanAyan(int dd, int mm, int yy) { 
            return 21.013972 + 1.398191 * calculateB6(dd, mm, yy);
        }
        static double lahiriAyan(int dd, int mm, int yy) { 
            return 22.460148 + 1.396042 * calculateB6(dd, mm, yy) + 3.08E-4 * calculateB6(dd, mm, yy) * calculateB6(dd, mm, yy);
        }
        static double kpayanold(int dd, int mm, int yy) { 
            return (yy + (mm * 30 + dd) / 365 - 297.3204723) * 50.2388475 / 3600;
        }
        static double kpayannew(int dd, int mm, int yy) { 
            double newAya, kpayaOn1stJan, daysAfter1stJan, correctionForDays; 
            kpayaOn1stJan = 22 + (1335 + (yy - 1900) * 50.2388475) / 3600 + (yy - 1900) * (yy - 1900) * 1.11E-4 / 3600; 
            daysAfter1stJan = ((mm - 1) * 30 + (dd - 1)) / 3600; 
            correctionForDays = daysAfter1stJan / 365 * (50.2388475 + 1.11E-4 * 20); 
            newAya = kpayaOn1stJan + correctionForDays; 
            return newAya;
        }
        static double kpayankhu(int dd, int mm, int yy) { 
            double dayAya, newAya, totalday; 
            dayAya = 50.2388475 / 365.25; 
            totalday = (yy - 291) * 365.25; 
            totalday += mm * 30 + dd - 114; 
            newAya = dayAya * totalday; 
            newAya /= 3600; 
            return newAya; 
        }
        static string dms(double x) { 
            var parts = "";
            double temp;
            bool negative = false;
            if (x < 0) {
                negative = true;
                x = x * (-1);
            }
            int deg, min, sec; 
            deg = (int)(x); 
            parts = parts + deg + "."; 
            temp = x - deg; 
            min = (int)(temp * 60); 
            parts = parts + min + "."; 
            temp = temp * 60; 
            temp = temp - (int)(temp); 
            sec = (int)(temp * 60 + 0.5);
            parts = parts + sec;
            if (negative == true)
                parts = "-" + parts;
            return parts;
        }

    }
}
