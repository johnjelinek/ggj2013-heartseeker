using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Heartseeker.Services
{
    public class HeartSeekerWebServices
    {
        private IFeedParser feedParser;

        public HeartSeekerWebServices(IFeedParser parser)
        {
            feedParser = parser;
        }

        public void Fetch(Uri feed)
        {

            WebClient client = new WebClient();
            client.Headers["User-Agent"] =
                    "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
                    "(compatible; MSIE 6.0; Windows NT 5.1; " +
                    ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            client.Headers["Accept"] = "application/xml";

            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadStringAsync(feed);

        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    feedParser.Parse(e.Result); // parser will update the FeedList which is observable by the UI list view for the feedtype
                }
            }
            catch (Exception ex)
            {
                // thePage.NoNetwork(); // get this back to the UI for display!!
            }
        }
    }
}
