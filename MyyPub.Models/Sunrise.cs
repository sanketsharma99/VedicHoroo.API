using System;
using System.Collections.Generic;
using System.Text;
using System.Json;

namespace MyyPub.Models
{
	public class Sunrise
	{
		private JsonArray monthList;
		public Sunrise()
		{
			monthList = new JsonArray();
			try
			{
				JsonObject m1 = new JsonObject();
				m1.Add("name", "January");
				m1.Add("numdays", 31);
				m1.Add("abbr", "Jan");
				monthList.Add(m1);
				JsonObject m2 = new JsonObject();
				m2.Add("name", "February");
				m2.Add("numdays", 28);
				m2.Add("abbr", "Feb");
				monthList.Add(m2);
				JsonObject m3 = new JsonObject();
				m3.Add("name", "March");
				m3.Add("numdays", 31);
				m3.Add("abbr", "Mar");
				monthList.Add(m3);
				JsonObject m4 = new JsonObject();
				m4.Add("name", "April");
				m4.Add("numdays", 30);
				m4.Add("abbr", "Apr");
				monthList.Add(m4);
				JsonObject m5 = new JsonObject();
				m5.Add("name", "May");
				m5.Add("numdays", 31);
				m5.Add("abbr", "May");
				monthList.Add(m5);
				JsonObject m6 = new JsonObject();
				m6.Add("name", "June");
				m6.Add("numdays", 30);
				m6.Add("abbr", "Jun");
				monthList.Add(m6);
				JsonObject m7 = new JsonObject();
				m7.Add("name", "July");
				m7.Add("numdays", 31);
				m7.Add("abbr", "Jul");
				monthList.Add(m7);
				JsonObject m8 = new JsonObject();
				m8.Add("name", "August");
				m8.Add("numdays", 31);
				m8.Add("abbr", "Aug");
				monthList.Add(m8);
				JsonObject m9 = new JsonObject();
				m9.Add("name", "September");
				m9.Add("numdays", 30);
				m9.Add("abbr", "Sep");
				monthList.Add(m9);
				JsonObject m10 = new JsonObject();
				m10.Add("name", "October");
				m10.Add("numdays", 31);
				m10.Add("abbr", "Oct");
				monthList.Add(m10);
				JsonObject m11 = new JsonObject();
				m11.Add("name", "November");
				m11.Add("numdays", 30);
				m11.Add("abbr", "Nov");
				monthList.Add(m11);
				JsonObject m12 = new JsonObject();
				m12.Add("name", "December");
				m12.Add("numdays", 31);
				m12.Add("abbr", "Dec");
				monthList.Add(m12);
			}
			catch (Exception eX)
			{

			}
		}
		double calcSunDeclination(double t)
		{
			double e = this.calcObliquityCorrection(t);
			double lambda = this.calcSunApparentLong(t);

			double sint = Math.Sin(this.degToRad(e)) * Math.Sin(this.degToRad(lambda));
			double theta = this.radToDeg(Math.Asin(sint));
			return theta;       // in degrees
		}
		double calcSunApparentLong(double t)
		{
			double o = this.calcSunTrueLong(t);
			double omega = 125.04 - 1934.136 * t;
			double lambda = o - 0.00569 - 0.00478 * Math.Sin(this.degToRad(omega));
			return lambda;      // in degrees
		}
		double calcSunTrueLong(double t)
		{
			double l0 = this.calcGeomMeanLongSun(t);
			double c = this.calcSunEqOfCenter(t);
			double O = l0 + c;
			return O;       // in degrees
		}
		double calcSunEqOfCenter(double t)
		{
			double m = this.calcGeomMeanAnomalySun(t);
			double mrad = this.degToRad(m);
			double sinm = Math.Sin(mrad);
			double sin2m = Math.Sin(mrad + mrad);
			double sin3m = Math.Sin(mrad + mrad + mrad);
			double C = sinm * (1.914602 - t * (0.004817 + 0.000014 * t)) + sin2m * (0.019993 - 0.000101 * t) + sin3m * 0.000289;
			return C;       // in degrees
		}
		double calcSunriseSetUTC(bool rise, double JD, double latitude, double longitude)
		{
			double t = this.calcTimeJulianCent(JD);
			double eqTime = this.calcEquationOfTime(t);
			double solarDec = this.calcSunDeclination(t);
			double hourAngle = this.calcHourAngleSunrise(latitude, solarDec);
			//alert("HA = " + radToDeg(hourAngle));
			if (!rise) hourAngle = -hourAngle;
			double delta = longitude + this.radToDeg(hourAngle);
			double timeUTC = 720 - (4.0 * delta) - eqTime;  // in minutes
			return timeUTC;
		}
		double calcHourAngleSunrise(double lat, double solarDec)
		{
			double latRad = this.degToRad(lat);
			double sdRad = this.degToRad(solarDec);
			double HAarg = (Math.Cos(this.degToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad));
			double HA = Math.Acos(HAarg);
			return HA;      // in radians (for sunset, use -HA)
		}
		public String calcSunriseSet(bool rise, int JD, double latitude, double longitude, double timezone, bool dst)
		// rise = 1 for sunrise, 0 for sunset
		{
			//var id = ((rise) ? "risebox" : "setbox")
			double timeUTC = this.calcSunriseSetUTC(rise, JD, latitude, longitude);
			double newTimeUTC = this.calcSunriseSetUTC(rise, JD + timeUTC / 1440.0, latitude, longitude);
			String srise = "";
			if (this.isNumber(newTimeUTC))
			{
				double timeLocal = newTimeUTC + (timezone * 60.0);
				timeLocal += ((dst) ? 60.0 : 0.0);
				if ((timeLocal >= 0.0) && (timeLocal < 1440.0))
				{
					srise = this.timeString(timeLocal, 2);
				}
				else
				{
					//Log.d("VedicHoroo", "calcSunriseSet:JD " + String.ValueOf(JD));
					double jday = JD;
					int increment = ((timeLocal < 0) ? 1 : -1);
					while ((timeLocal < 0.0) || (timeLocal >= 1440.0))
					{
						timeLocal += increment * 1440.0;
						jday -= increment;
					}
					//Log.d("VedicHoroo", "calcSunriseSet:jday " + String.valueOf(jday));
					srise = this.timeDateString(jday, timeLocal);
				}
			}
			else
			{ // no sunrise/set found
				srise = "not found";
			}
			return srise;
		}
		String timeDateString(double JD, double minutes)
		{
			//Log.d("VedicHoroo", "timeDateString: JD " + String.valueOf(JD));
			String output = this.timeString(minutes, 2) + " " + this.dayString(JD, false, 2);
			return output;
		}

		String timeString(double minutes, int flag)
		// timeString returns a zero-padded string (HH:MM:SS) given time in minutes
		// flag=2 for HH:MM, 3 for HH:MM:SS
		{
			String output = "";
			if ((minutes >= 0) && (minutes < 1440))
			{
				double floatHour = minutes / 60.0;
				double hour = Math.Floor(floatHour);
				double floatMinute = 60.0 * (floatHour - Math.Floor(floatHour));
				double minute = Math.Floor(floatMinute);
				double floatSec = 60.0 * (floatMinute - Math.Floor(floatMinute));
				double second = Math.Floor(floatSec + 0.5);
				if (second > 59)
				{
					second = 0;
					minute += 1;
				}
				if ((flag == 2) && (second >= 30)) minute++;
				if (minute > 59)
				{
					minute = 0;
					hour += 1;
				}
				output = this.zeroPad((int)hour, 2) + ":" + this.zeroPad((int)minute, 2);
				if (flag > 2) output = output + ":" + this.zeroPad((int)second, 2);
			}
			else
			{
				output = "time error";
			}
			return output;
		}
		String zeroPad(int n, int digits)
		{
			String n1 = n.ToString();
			while (n1.Length < digits)
			{
				n1 = '0' + n1;
			}
			return n1;
		}
		bool isNumber(double inputVal)
		{
			bool oneDecimal = false;
			String inputStr = "" + inputVal;
			for (int i = 0; i < inputStr.Length; i++)
			{
				char oneChar = inputStr[i];
				if (i == 0 && (oneChar == '-' || oneChar == '+'))
				{
					continue;
				}
				if (oneChar == '.' && !oneDecimal)
				{
					oneDecimal = true;
					continue;
				}
				if (oneChar < '0' || oneChar > '9')
				{
					return false;
				}
			}
			return true;
		}
		double calcTimeJulianCent(double jd)
		{
			double T = (jd - 2451545.0) / 36525.0;
			return T;
		}
		double calcSunTrueAnomaly(double t)
		{
			double m = this.calcGeomMeanAnomalySun(t);
			double c = this.calcSunEqOfCenter(t);
			double v = m + c;
			return v;       // in degrees
		}
		double calcSunRadVector(double t)
		{
			double v = this.calcSunTrueAnomaly(t);
			double e = this.calcEccentricityEarthOrbit(t);
			double R = (1.000001018 * (1 - e * e)) / (1 + e * Math.Cos(this.degToRad(v)));
			return R;       // in AUs
		}

		double calcEquationOfTime(double t)
		{
			double epsilon = this.calcObliquityCorrection(t);
			double l0 = this.calcGeomMeanLongSun(t);
			double e = this.calcEccentricityEarthOrbit(t);
			double m = this.calcGeomMeanAnomalySun(t);

			double y = Math.Tan(this.degToRad(epsilon) / 2.0);
			y *= y;

			double sin2l0 = Math.Sin(2.0 * this.degToRad(l0));
			double sinm = Math.Sin(this.degToRad(m));
			double cos2l0 = Math.Cos(2.0 * this.degToRad(l0));
			double sin4l0 = Math.Sin(4.0 * this.degToRad(l0));
			double sin2m = Math.Sin(2.0 * this.degToRad(m));

			double Etime = y * sin2l0 - 2.0 * e * sinm + 4.0 * e * y * sinm * cos2l0 - 0.5 * y * y * sin4l0 - 1.25 * e * e * sin2m;
			return this.radToDeg(Etime) * 4.0;  // in minutes of time
		}
		double calcMeanObliquityOfEcliptic(double t)
		{
			double seconds = 21.448 - t * (46.8150 + t * (0.00059 - t * (0.001813)));
			double e0 = 23.0 + (26.0 + (seconds / 60.0)) / 60.0;
			return e0;      // in degrees
		}

		double calcObliquityCorrection(double t)
		{
			double e0 = this.calcMeanObliquityOfEcliptic(t);
			double omega = 125.04 - 1934.136 * t;
			double e = e0 + 0.00256 * Math.Cos(this.degToRad(omega));
			return e;       // in degrees
		}
		double calcGeomMeanLongSun(double t)
		{
			double L0 = 280.46646 + t * (36000.76983 + t * (0.0003032));
			while (L0 > 360.0)
			{
				L0 -= 360.0;
			}
			while (L0 < 0.0)
			{
				L0 += 360.0;
			}
			return L0;      // in degrees
		}
		double calcEccentricityEarthOrbit(double t)
		{
			double e = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);
			return e;       // unitless
		}
		double calcGeomMeanAnomalySun(double t)
		{
			double M = 357.52911 + t * (35999.05029 - 0.0001537 * t);
			return M;       // in degrees
		}
		double radToDeg(double angleRad)
		{
			return (180.0 * angleRad / Math.PI);
		}
		bool isLeapYear(int yr)
		{
			return ((yr % 4 == 0 && yr % 100 != 0) || yr % 400 == 0);
		}

		String dayString(double jd, bool next, int flag)
		{
			String output = "JD error";
			try
			{
				double A;
				// returns a string in the form DDMMMYYYY[ next] to display prev/next rise/set
				// flag=2 for DD MMM, 3 for DD MM YYYY, 4 for DDMMYYYY next/prev
				//Log.d("VedicHoroo JD", String.valueOf(jd));

				if ((jd < 900000) || (jd > 2817000))
				{
					output = "Julian Day cannot be < 900000 or > 2817000";
				}
				else
				{
					double z = Math.Floor(jd + 0.5);
					double f = (jd + 0.5) - z;
					if (z < 2299161)
					{
						A = z;
					}
					else
					{
						double alpha = Math.Floor((z - 1867216.25) / 36524.25);
						A = z + 1 + alpha - Math.Floor(alpha / 4);
					}
					double B = A + 1524;
					double C = Math.Floor((B - 122.1) / 365.25);
					double D = Math.Floor(365.25 * C);
					double E = Math.Floor((B - D) / 30.6001);
					double day = B - D - Math.Floor(30.6001 * E) + f;
					double month = (E < 14) ? E - 1 : E - 13;
					double year = ((month > 2) ? C - 4716 : C - 4715);
					if (flag == 2)
						output = this.zeroPad((int)day, 2) + " " + this.monthList[(int)Math.Round(month - 1)]["abbr"].ToString();
					if (flag == 3)
						output = this.zeroPad((int)day, 2) + this.monthList[(int)Math.Round(month - 1)]["abbr"].ToString() + year;
					if (flag == 4)
						output = this.zeroPad((int)day, 2) + this.monthList[(int)Math.Round(month - 1)]["abbr"].ToString() + year + ((next) ? " next" : " prev");
				}
			}
			catch (Exception eX)
			{

			}
			return output;
		}
		double degToRad(double angleDeg)
		{
			return (Math.PI * angleDeg / 180.0);
		}
		public int getJD(int day, int mon, int yer)
		{
			int JD = -1;
			int docmonth = mon;
			int docday = day;
			int docyear = yer;
			try
			{
				if ((this.isLeapYear(docyear)) && (docmonth == 2))
				{
					if (docday > 29)
					{
						docday = 29;
					}
				}
				else
				{
					//Log.d("VedicHoroo numdays", this.monthList[(int)Math.Round(docmonth - 1)]["numdays"].ToString());
					//Log.d("VedicHoroo name", this.monthList.getJsonObject((int)Math.round(docmonth - 1)).get("name").toString());
					//Log.d("VedicHoroo abbr", this.monthList.getJsonObject((int)Math.round(docmonth - 1)).get("abbr").toString());
					if (docday > Int32.Parse(this.monthList[docmonth - 1]["numdays"].ToString()))
					{
						docday = Int32.Parse(this.monthList[docmonth - 1]["numdays"].ToString());
					}
				}
				if (docmonth <= 2)
				{
					docyear -= 1;
					docmonth += 12;
				}
				double A = Math.Floor((double)(docyear / 100));
				double B = 2 - A + Math.Floor(A / 4);
				//Log.d("VedicHoroo docday", String.valueOf(docday));
				//Log.d("VedicHoroo docmon", String.valueOf(docmonth));
				//Log.d("VedicHoroo docyear", String.valueOf(docyear));

				double jdbl = Math.Floor(365.25 * (docyear + 4716)) + Math.Floor(30.6001 * (docmonth + 1)) + docday + B - 1524.5;
				//Log.d("VedicHoroo jd double", String.valueOf(jdbl));
				JD = (int)Math.Floor(jdbl);
				//Log.d("VedicHoroo jd int", String.valueOf(JD));
			}
			catch (Exception eX)
			{

			}
			return JD;
		}

	}

}
