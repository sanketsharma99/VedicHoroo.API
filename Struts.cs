using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;

namespace MyyPub.VedicHorro
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
    public class BirthStar
    {
        public string birthStar { get; set; }
        public string birthSign { get; set; }
        public string birthSignDeg { get; set; }
        public string partnerBirthStar { get; set; }
        public string partnerBirthSign { get; set; }
        public string partnerBirthSignDeg { get; set; }
        public string sunDeg { get; set; }
        public string sunSign { get; set; }
        public string manglik { get; set; }
        public string partnerManglik { get; set; }
        public string ruler { get; set; }
        public string startSign { get; set; }
        public string endSign { get; set; }
        public string startDeg { get; set; }
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
        public string moon_phase { get; set; }
    }
    enum PlanetStrength
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
        public double pos { get; set; }
        public string sign { get; set; }
        public string signtype { get; set; }
        public string lordship { get; set; }
        public string houselord { get; set; }
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
    public class Astrologer
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

    public class FeedItem
    {
        public string pubDate { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string image { get; set; }
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

}