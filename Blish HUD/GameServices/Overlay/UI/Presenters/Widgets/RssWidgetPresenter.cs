using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Overlay.UI.Views;
using Flurl.Http;

namespace Blish_HUD.Overlay.UI.Presenters.Widgets
{
    public class RssWidgetPresenter : Presenter<RssWidgetView, Uri>
    {
        private readonly string _feedTitle;

        private SyndicationFeed _feed;

        /// <inheritdoc />
        public RssWidgetPresenter(RssWidgetView view, string feedTitle, Uri feedUri) : base(view, feedUri)
        {
            this._feedTitle = feedTitle;
        }

        /// <inheritdoc />
        protected override async Task<bool> Load(IProgress<string> progress)
        {
            if (!this.Model.IsWellFormedOriginalString()) return false;

            progress.Report("Requesting feed...");

            using (var feedResponse = await this.Model.AbsoluteUri.GetAsync())
            {
                if (!feedResponse.IsSuccessStatusCode)
                {
                    progress.Report($"Request failed: {feedResponse.ReasonPhrase}");
                    return false;
                }

                progress.Report("Loading feed...");

                using (var stringReader = new StringReader(await feedResponse.Content.ReadAsStringAsync()))
                {
                    using (var xmlReader = XmlReader.Create(stringReader))
                    {
                        this._feed = SyndicationFeed.Load(xmlReader);
                    }
                }
            }

            if (this._feed == null)
            {
                progress.Report("Failed to read response.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void UpdateView()
        {
            this.View.Title = this._feedTitle ?? this._feed.Title.Text;

            this.View.SetFeedEntry1(this._feed.Items.ElementAt(0));
            this.View.SetFeedEntry2(this._feed.Items.ElementAt(1));
        }
    }
}