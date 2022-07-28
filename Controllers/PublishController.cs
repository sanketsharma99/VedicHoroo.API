using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MYPUBZ.Contracts;
using MYPUBZ.Theme.ServiceLocator;
using MYPUBZ.ThemeTempl;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.IO;
using Serilog;
namespace MyyPub.VedicHorro
{
    [Route("api")]
    [ApiController]
    public class PublishController : ControllerBase
    {
		private IWebHostEnvironment _env;
		private readonly ILogger<PublishController> _logger;
		public PublishController(IWebHostEnvironment env, ILogger<PublishController> logger)
		{
			_logger = logger;
			_env = env;
		}

		[HttpPost("PublishStories"), DisableRequestSizeLimit]
        public async Task<IActionResult> PublishStories([FromBody]  PublishStories pubStories)
        {
			try
			{
				ResponseData r = await PubStory(pubStories);
				if (r.code == 0)
				{
					return Ok(200);
				}
				else
				{
					Log.Information(string.Format("Call PubStory returned Error, {0}",r.message));
					return NotFound(r.message);
				}
			}
			catch (Exception eX)
			{
				var st = new StackTrace(eX, true);
				// Get the top stack frame
				var frame = st.GetFrame(st.FrameCount - 1);
				// Get the line number from the stack frame
				var line = frame.GetFileLineNumber();
				Log.Information(string.Format("publish failed {0}-Line:{1}", eX.Message, line));
				return NotFound(string.Format("publish failed {0}-Line:{1}", eX.Message, line));
			}
		}

		async private Task<ResponseData> PubStory(PublishStories pubStories)
		{
			MyyPubService svc = new MyyPubService();
			IThemeTempl templ = svc.GetService(pubStories.publicationId, _env.WebRootPath);
			var r = await Task.Run(() => {
				return templ.PublishStories(pubStories); 
			});
			return r;
		}

		[HttpPost("Upload"), DisableRequestSizeLimit]
		public async Task<IActionResult> Upload()
		{
			try
			{
				var files = Request.Form.Files;
				var tags = Request.Form["tagitems"];
				string tp = string.Empty;
				if (tags.Count > 0)
					tp = tags[0].TrimEnd(',').Trim().Replace(',', '-').Replace(' ', '-');
				else
					tp = "t";

				var path = Path.Combine(_env.WebRootPath, "images");
				List<string> lstf = new List<string>();
				if (files.Count() > 0)
				{
					int c = 1;
					var r = await Task.Run(() => {
						foreach (var file in files)
						{
							var fileName = string.Format("{0}-{1}{2}", Guid.NewGuid(),tp,Path.GetExtension(file.FileName)); //ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
							var fullPath = Path.Combine(path, fileName);
							using (var stream = new FileStream(fullPath, FileMode.Create))
							{
								file.CopyTo(stream);
							}
							lstf.Add(fileName);
						}
						return Ok(new { lstf });
					});
					return r;
				}
				else
				{
						return NotFound("failed: Could not recognize the file");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, string.Format("failed: Internal server error: {0}", ex.Message));
			}
		}
		[HttpPost("UploadProfile"), DisableRequestSizeLimit]
		public async Task<IActionResult> UploadProfile()
		{
			try
			{
				var img = Request.Form["imgdat"];
				if (string.IsNullOrEmpty(img))
					return StatusCode(500, "Invalid Data");

				//var t = base64image.Substring(22);  // remove data:image/png;base64,

				var r = await Task.Run(() => {
					var path = Path.Combine(_env.WebRootPath, "images");
					var fileName = string.Format("{0}.png", Guid.NewGuid()); 
					var fullPath = Path.Combine(path, fileName);
					System.IO.File.WriteAllBytes(fullPath, Convert.FromBase64String(img));
					return StatusCode(200, fileName);
				});
				return r;
			}
			catch (Exception ex)
			{
				return StatusCode(500, string.Format("failed: Internal server error: {0}", ex.Message));
			}
		}


		[HttpGet("GetPhotos")]
		public async Task<IActionResult> GetPhotos(string tag)
		{
			try
			{
				tag = tag.Trim().Replace(' ', '-');
				var path = Path.Combine(_env.WebRootPath, "images");
				var r = await Task.Run(() =>
				{
					string[] lstf = Directory.GetFiles(path, string.Format("*{0}*.*", tag)).Select(file => Path.GetFileName(file)).ToArray();
					return Ok(new { lstf });
				});
				return r;
			}
			catch (Exception ex)
			{
				return StatusCode(500, string.Format("failed: Internal server error: {0}", ex.Message));
			}
		}

	}
}
