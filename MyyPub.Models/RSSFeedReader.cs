using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace MyyPub.Models
{
    public class RSSFeedReader
    {
        public IEnumerable<Post> ReadFeed(string url)
        {
            var rssFeed = XDocument.Load(url);

            var posts = from item in rssFeed.Descendants("item")
                        select new Post
                        {
                            Title = item.Element("title").Value,
                            Description = item.Element("description").Value,
                            PublishedDate = item.Element("pubDate").Value
                        };

            return posts;
        }
    }

    public class Post
    {
        public string PublishedDate;
        public string Description;
        public string Title;
        public string Link;
        public string StoryImage;
        public string Content;
    }
}
