using System;
using System.Collections.Generic;
using System.Text;

namespace MyyPub.Models
{
	public class PlanetPosition
	{
		public string dms_lat { set; get; }
		public string dms_lng { set; get; }
		public double dist { get; set; }
		public double lat_speed { get; set; }
		public double lng_speed { get; set; }
		public double dist_speed { get; set; }
	}
}
