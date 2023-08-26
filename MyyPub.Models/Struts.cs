using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MyyPub.Models
{
    public class DomainData
    {
        public string Protocol { get; set; }
        public string HostName { get; set; }
        public string Fragment { get; set; }
    }
    class birth_details
    {
        public string name { get; set; }
        public string dob { get; set; }
        public string tob { get; set; }
        public string latlng { get; set; }
        public string timezone { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class AstroUser
    {
        public string devicetoken { get; set; }
        public string moonsign { get; set; }
        public float moondeg { get; set; }
    }
    public class location
    {
        public string start { get; set; }
        public string end { get; set; }
    }
    public class Star
    {
        public string name { get; set; }
        public int order { get; set; }
        public location location { get; set; }
        public string ruler { get; set; }
        public string symbol { get; set; }
        public string diety { get; set; }
        public string varna { get; set; }
        public string alphabets { get; set; }
        public string rulingAnimal { get; set; }
        public string unfriendlyAnimals { get; set; }
        public string neutralAnimals { get; set; }
        public string friendlyAnimals { get; set; }
        public string nadikuta { get; set; }
        public string ganakuta { get; set; }
        public string rajju { get; set; }
        public string characteristics { get; set; }
    }
    public class SubLord
    {
        public string deg { get; set; }
        public string sign { get; set; }
        public string star { get; set; }
        public string sub { get; set; }
    }
	public class SSLord
	{
		public string deg { get; set; }
		public string sign { get; set; }
		public string sub { get; set; }
	}
	public class BirthStar
	{
		public string birthStar { get; set; }
		public int pada { get; set; }
		public string birthSign { get; set; }
		public string birthSignDeg { get; set; }
		public string partnerBirthStar { get; set; }
		public int partnerPada { get; set; }
		public string partnerBirthSign { get; set; }
		public string partnerBirthSignDeg { get; set; }
		public string sunDeg { get; set; }
		public string sunSign { get; set; }
        public string ascSign { get; set; }
		public string manglik { get; set; }
		public string partnerManglik { get; set; }
		public string ruler { get; set; }
		public string startSign { get; set; }
		public string endSign { get; set; }
		public string startDeg { get; set; }
		public string papaSamyam { get; set; }
		public string kalatraDosh {get; set;}
	}
	public class RecfyBirthTime
    {
        public string curDOB { get; set; }
        public string recfyDOB { get; set; }
        public string rem { get; set; }
    }
    public class Prashna
    {
        public int znum { get; set; }
        public string dayL { get; set; }
        public string ascSSSL { get; set; }
        public string moSSSL { get; set; }
        public string praSSSL { get; set; }
        public string answer { get; set; }
        public string remarks { get; set; }
        public string interr { get; set; }
    }
    public class StarConst
    {
        public string date { get; set; }
        public string star { get; set; }
        public string starStrength { get; set; }
        public string lunarStrength { get; set; }
        public string tithi { get; set; }
		public string yoga { get; set; }
		public string karana { get; set; }
        public string moonPhase { get; set; }
        public string tithiRem { get; set; }
    }

    public class Transit
    {
        public string date { get; set; }
        public string signL { get; set; }
        public string starL { get; set; }
        public string subL { get; set; }
        public string star { get; set; }
        public string sign { get; set; }
    }
    public class Transit2
    {
        public string date { get; set; }
        public string sssl { get; set; }
		public string mdras { get; set; }
		public string mdnak { get; set; }
		public string mdsub { get; set; }
        public string rupll { get; set; }
        public string rupml { get; set; }
        public string rupdl { get; set; }
    }
    public class Moon
    {
        public float moonDeg;
        public string moonSign;
        public string birthStar;
        public string curStar;
        public string curSign;
        public int starWeight;
        public int moonWeight;
        public string starStrength;
        public string moonStrength;
        public string overallStrength;
    }
	public class Horo
	{
		public string name { get; set; }
		public string gender { get; set; }
		public string dob { get; set; }
		public string tob { get; set; }
		public string latlng { get; set; }
		public string timezone { get; set; }
		public string place { get; set; }
		public string ayanamsa { get; set; }
		public string sunrise { get; set; }
		public string sunset { get; set; }
		public Dictionary<string, string> planetPos { get; set; }
		public Dictionary<string, string> housePos { get; set; }
		public Dictionary<string, string> ascPos { get; set; }
		public Dictionary<string, int> astkPts { get; set; }
		public Dictionary<string, string> planetDecl { get; set; }
		public Dictionary<string, string> planetSped { get; set; }
		public string birthStar { get; set; }
		public string tithi { get; set; }
		public string moonPhase { get; set; }
		public string tithiRem { get; set; }
		public string yoga { get; set; }
		public string karana { get; set; }
		public string retroPls { get; set; }
		public string plStren { get; set; }

    }
    public class Birth
    {
        public string name { get; set; }
        public string dob { get; set; }
        public string lagna { get; set; }
        public string lagna_lord { get; set; }
        public string moon_sign { get; set; }
        public string sun_sign { get; set; }
        public string birth_star { get; set; }
        public string star_lord { get; set; }
        public string tithi { get; set; }
		public string yoga { get; set; }
		public string karana { get; set; }
        public string moon_phase { get; set; }
		public string moon_deg { get; set; }
    }
    public enum PlanetStrength
    {
        EXALTED = 1,
        MOOLTRIKONA,
        OWN,
        FRIEND,
        DEBILIATED,
        NORMAL,
        ENEMY
    };
    public class PlanetHouse
    {
        public string name { get; set; }
        public string code { get; set; }
        public int hno { get; set; }
        public int mhno { get; set; }
        public int shno { get; set; }
		public int phno { get; set; }
        public double pos { get; set; }
		public string dmspos { get; set; }
        public string sign { get; set; }
		public string rashi { get; set; }
        public string signtype { get; set; }
        public string lordship { get; set; }
        public string houselord { get; set; }
		public string aspts { set; get; }
		public string starl { get; set; }
		public string signl { get; set; }
		public string subl { get; set; }
		public Dictionary<string, string> lifevts { get; set; }
        public Dictionary<string, string> dsigs { get; set; }
		public int[] sigs;
        public string inds { get; set; }
        public string smsg { get; set; }
    }
	public class House
	{
		public int hno { get; set; }
		public double pos { get; set; }
		public string dmspos { get; set; }
		public string sign { get; set; }
		public string rashi { get; set; }
		public string aspts { set; get; }
		public string starl { get; set; }
		public string signl { get; set; }
		public string subl { get; set; }
	}


	public class VimDasha
    {
        public string mdas { get; set; }
        public string adas { get; set; }
        public string pdas { get; set; }
    }
    public class Dasha
    {
        public string lord { get; set; }
        public string per { get; set; }
        public string type { get; set; }
        public string style { get; set; }
        public bool subs { get; set; }
        public bool show { get; set; }
        public string icon { get; set; }
    }
	public class Astakavarga
	{
		public Dictionary<string, int> akPts { get; set; }
		public Dictionary<string, string> houSgn { get; set; }
	}
    public class ChartSettings
	{
        public string dob { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string timezone { get; set; }
        public int dstofset { get; set; }
        public int ayanid { get; set; }
        public bool use_truenode { get; set; }
	}
	public class Shadbala
	{
		public Dictionary<string, double> uchBala { get; set; }
		public Dictionary<string, double> sptvBala { get; set; }
		public Dictionary<string, double> ojayBala { get; set; }
		public Dictionary<string, double> kenBala { get; set; }
		public Dictionary<string, double> drekBala { get; set; }
		public Dictionary<string, double> dikBala { get; set; }
		public Dictionary<string, double> natoBala { get; set; }
		public Dictionary<string, double> triBala { get; set; }
		public Dictionary<string, double> pakBala { get; set; }
		public Dictionary<string, double> hvmaBala { get; set; }
		public Dictionary<string, double> ayanBala { get; set; }
		public Dictionary<string, double> chestBala { get; set; }
		public Dictionary<string, double> naiskBala { get; set; }
		public Dictionary<string, double> drgBala { get; set; }

	}
	public class Panchang
	{
		string date { get; set; }
		string paksha { get; set; }
		string maasa { get; set; }
		DateTime sunrise { get; set; }
		DateTime sunset { get; set; }

	}
	[BsonIgnoreExtraElements]
    public class Plan
    {
		public MongoDB.Bson.ObjectId _id { get; set; }
        public string uuid { get; set; }
        public string name { get; set; }
        public int credits { get; set; }
        public string dobs { get; set; }
        public int rating { get; set; }
        public string review { get; set; }
    }
	[BsonIgnoreExtraElements]
	public class Arch
	{
		public string uuid { get; set; }
		public string dob { get; set; }
	}
		public class People
	{
		public Object _id { get; set; }
		public string uuid { get; set; }
		public string name { get; set; }
		public int credits { get; set; }
		public string dobs { get; set; }
	}
	[BsonIgnoreExtraElements]
    public class Ticket
    {
        public string uuid { get; set; }
        public string guid { get; set; }
        public string cat { get; set; }
        public string sub { get; set; }
        public string msg { get; set; }
        public string status { get; set; }
    }
	[BsonIgnoreExtraElements]
	public class Report
	{
		public string uuid { get; set; }
		public string guid { get; set; }
		public string dob { get; set; }
		public string chtyp { get; set; }
		public string aynm { get; set; }
		public string lan {get; set;}
		public string eml { get; set; }
		public string mob { get; set; }
		public string status { get; set; }
		public string reqdt { get; set; }
		public string lnk { get; set; }
		public string svg { get; set; }
	}
	[BsonIgnoreExtraElements]
    public class TicketResp
    {
        public string uuid { get; set; }
        public string guid { get; set; }
        public string resp { get; set; }
        public string status { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Puja
    {
        public string name { get; set; }
        public string desc { get; set; }
        public string img { get; set; }
        public string fee { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Astrologer
    {
        public string uuid { get; set; }
        public string uid { get; set; }
        public string mob { get; set; }
        public string name { get; set; }
        public string tagline { get; set; }
        public string avatar { get; set; }
        public string status { get; set; }
        public double? rating { get; set; }
        public int tot_ratings { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class CallInfo
	{
        public string uid { get; set; }
        public string cid { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public string date { get; set; }
        public int duration { get; set; }
        public bool? settled { get; set; }
	}
    [BsonIgnoreExtraElements]
    public class Rating
	{
        public string uuid { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }
        public string pid { get; set; }
        public int rating { get; set; }
        public string review { get; set; }

	}

    [BsonIgnoreExtraElements]
	public class Profile
	{
		public string uuid { get; set; }
		public string avatar { get; set; }
		public string dob { get; set; }
		public string status { get; set; }
        public string token { get; set; }
        public string expires { get; set; }
	}
	[BsonIgnoreExtraElements]
	public class User
	{
		public string uuid { get; set; }
		public string mob { get; set; }
		public string name { get; set; }
		public string gen { get; set; }
		public string dob { get; set; }
		public string tagline { get; set; }
		public string avatar { get; set; }
		public string status { get; set; }

	}
	[BsonIgnoreExtraElements]
	public class Admin
	{
		public string uuid { get; set; }
		public string uid { get; set; }
		public string mob { get; set; }
		public string name { get; set; }
		public string tagline { get; set; }
		public string avatar { get; set; }
		public string status { get; set; }
	}
	[BsonIgnoreExtraElements]
	public class Agent
	{
		public string uuid { get; set; }
		public string uid { get; set; }
		public string mob { get; set; }
		public string name { get; set; }
		public string tagline { get; set; }
		public string avatar { get; set; }
		public string status { get; set; }
	}
	[BsonIgnoreExtraElements]
	public class AstroBio
	{
		public string uid { get; set; }
		public string banner { get; set; }
		public string bio { get; set; }
	}
    [BsonIgnoreExtraElements]
    public class Subscriber
    {
        public string uuid { get; set; }
        public string nam { get; set; }
        public string mob { get; set; }
        public string eml { get; set; }
    }
	[BsonIgnoreExtraElements]
	public class Blog
	{
		public string uuid { get; set; }
		public string title { get; set; }
		public string story { get; set; }
		public string img { get; set; }
		public string createdt { get; set; }
	}
	[BsonIgnoreExtraElements]
	public class Message
	{
		public string uuid { get; set; }
		public string tag { get; set; }
		public string msg { get; set; }
	}
	[BsonIgnoreExtraElements]
	public class Quota
	{
		public string uuid { get; set; }
		public int qta { get; set; }
	}
	[BsonIgnoreExtraElements]
    public class KPHouseGroup
    {
        public string uuid { get; set; }
        public string hgp { get; set; }
    }
    public class Notif
    {
        public string type;
        public string msg;
    }
    public class Token
	{
        public string token { get; set; }
        public DateTime expires { get; set; }
	}
    public class FeedItem
    {
        public string pubDate { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string image { get; set; }
		public string byline { get; set; }
		public string bylineUrl { get; set; }
    }
    public class AppVersion
    {
        public string appn { get; set; }
        public string pkgn { get; set; }
        public string ver { get; set; }
        public string verc { get; set; }
    }
    public class AppMessage
    {
        public string title { get; set; }
        public string msg { get; set; }
    }
    public class Gemstone
	{
        [JsonProperty("Gemstone")]
        public string name { get; set; }
        [JsonProperty("Mantra")]
        public string mantra { get; set; }
        [JsonProperty("Day")]
        public string day { get; set; }
        [JsonProperty("Nakshatra")]
        public string star { get; set; }
        [JsonProperty("Metal")]
        public string metal { get; set; }
        [JsonProperty("Finger")]
        public string finger { get; set; }
        [JsonProperty("Deitees")]
        public string deitees { get; set; }
        [JsonProperty("Instructions")]
        public string ins { get; set; }
	}
    [BsonIgnoreExtraElements]
    public class Blogger
    {
        public string uid { get; set; }
        public string nam { get; set; }
        public string mob { get; set; }
        public string eml { get; set; }
        public string dob { get; set; }
        public bool ismale { get; set; }
        public string avatar { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ErrorInfo
    {
        public string msg { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Trendz
    {
        public string uuid { get; set; }
        public int credits { get; set; }
        public string date { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class AstroTalk
    {
        public string uuid { get; set; }
        public string uid { get; set; }
        public string aid { get; set; }
        public string date { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Offer
    {
        public string uuid { get; set; }
        public string oid { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
        public int price { get; set; }
        public bool avail { get; set; }
        public int impr { get; set; }
    }
	[BsonIgnoreExtraElements]
	public class Comment
	{
		public MongoDB.Bson.ObjectId _id { get; set; }
		public string cid { get; set; }
		public string pid { get; set; }
		public string uuid { get; set; }
		public string title { get; set; }
		public string name { get; set; }
		public string avatar { get; set; }
		public string msg { get; set; }
		public int liks { get; set; }
		public int nliks { get; set; }
		public int nrep { get; set; }
	}
    [BsonIgnoreExtraElements]
    public class Prediction
    {
        public string zod { get; set; }
        public string desc { get; set; }
    }

    public class ReportParams
	{
		public string uuid { get; set; }
		public string name { get; set; }
		public string gender { get; set; }
		public string dob { get; set; }
		public string tob { get; set; }
		public string pob { get; set; }
		public string latlng { get; set; }
		public string timezone { get; set; }
        public double? tzofset { get; set; }
		public int? dstofset { get; set; }
		public int ayanid { get; set; }
		public string lang { get; set; }
		public string chtyp { get; set; }
        public string cimg { get; set; }
        public string cnme { get; set; }
        public string cnum { get; set; }
        public string ceml { get; set; }
	}
    public class Dosha
	{
        public string name { get; set; }
        public string desc { get; set; }
        public string horodesc { get; set; }
        public string remedies { get; set; }
	}
    public class DashaParams
	{
        public string dob { get; set; }
        public string tob { get; set; }
        public string latlng { get; set; }
        public string timezone { get; set; }
        public int dstofset { get; set; }
        public string mlrd { get; set; }
        public string alrd { get; set; }
        public Dictionary<string, string> planetPos { get; set; }
        public string lang { get; set; }

	}
    public class PlParams
	{
        public string name { get; set; }
        public string gen { get; set; }
        public Horoscope mHoro { get; set; } 
        public Dictionary<int, string> dctHou { get; set; } 
        public Dictionary<string, PlanetHouse> dctPlHou { get; set; } 
        public string lang { get; set; }
    }

    public class SMLParams
	{
        public string dobf { get; set; }
        public string dobr { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string timezone { get; set; }
        public int dstofset { get; set; }
        public int ayanid { get; set; }
	}

    public class StarParams
    {
        public string star { get; set; }
        public string sign { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string timezone { get; set; }
        public int dstofset { get; set; }
        public int ayanid { get; set; }
    }
    public class CompatibilityParams
	{
        public string dob { get; set; }
        public string partner_dob { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public double partner_lat { get; set; }
        public double partner_lng { get; set; }
        public string tz { get; set; }
        public string partner_tz { get; set; }
        public int dst { get; set; }
        public int partner_dst { get; set; }
        public int ayanid { get; set; }
    }
    public class LoginParams
	{
        public string name { get; set; }
        public string password { get; set; }
	}
    public class PujaBooking
	{
        public string name { get; set; }
        public string date { get; set; }
        public string slot { get; set; }

        public string familynames { get; set; }
        public string gotram { get; set; }
	}
    public class AstroBioParams
    {
        public string uuid { get; set; }
        public string banner { get; set; }
        public string bio { get; set; }
    }
}