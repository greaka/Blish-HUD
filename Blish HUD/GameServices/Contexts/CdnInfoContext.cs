using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace Blish_HUD.Contexts
{
    /// <summary>
    ///     Provides build information provided by the asset CDNs.
    /// </summary>
    public class CdnInfoContext : Context
    {
        private const int TOTAL_CDN_ENDPOINTS = 2;

        private const string GW2_ASSETCDN_URL = "http://assetcdn.101.arenanetworks.com/latest/101";
        private const string GW2_CN_ASSETCDN_URL = "http://assetcdn.111.cgw2.com/latest/111";

        private static readonly Logger Logger = Logger.GetLogger<CdnInfoContext>();
        private CdnInfo _chineseCdnInfo;

        private int _loadCount;

        private CdnInfo _standardCdnInfo;

        private ContextAvailability TryGetCdnInfo(ref CdnInfo cdnInfo, out ContextResult<CdnInfo> contextResult)
        {
            if (this.State != ContextState.Ready) return NotReady(out contextResult);

            if (cdnInfo.BuildId > 0)
            {
                contextResult = new ContextResult<CdnInfo>(cdnInfo);
                return ContextAvailability.Available;
            }

            if (cdnInfo.BuildId < 0)
            {
                contextResult = new ContextResult<CdnInfo>(cdnInfo, "Failed to determine build ID from CDN.");
                return ContextAvailability.Failed;
            }

            contextResult = new ContextResult<CdnInfo>(cdnInfo, "Build ID has not been requested from the CDN.");
            return ContextAvailability.Unavailable;
        }

        /// <summary>
        ///     If <see cref="ContextAvailability.Available" />, returns
        ///     <see cref="CdnInfo" /> provided by the standard asset CDN.
        /// </summary>
        public ContextAvailability TryGetStandardCdnInfo(out ContextResult<CdnInfo> contextResult)
        {
            return TryGetCdnInfo(ref this._standardCdnInfo, out contextResult);
        }

        /// <summary>
        ///     If <see cref="ContextAvailability.Available" />, returns
        ///     <see cref="CdnInfo" /> provided by the Chinese asset CDN.
        /// </summary>
        public ContextAvailability TryGetChineseCdnInfo(out ContextResult<CdnInfo> contextResult)
        {
            return TryGetCdnInfo(ref this._chineseCdnInfo, out contextResult);
        }

        /// <summary>
        ///     Structured information provided by one of the asset CDNs.
        /// </summary>
        public struct CdnInfo
        {
            public int BuildId { get; }

            public int ExeFileId { get; }

            public int ExeFileSize { get; }

            public int ManifestFileId { get; }

            public int ManifestFileSize { get; }

            public CdnInfo(int buildId, int exeFileId, int exeFileSize, int manifestFileId, int manifestFileSize)
            {
                this.BuildId = buildId;
                this.ExeFileId = exeFileId;
                this.ExeFileSize = exeFileSize;
                this.ManifestFileId = manifestFileId;
                this.ManifestFileSize = manifestFileSize;
            }

            public static CdnInfo Invalid => new CdnInfo(-1, -1, -1, -1, -1);
        }

        #region Context Management

        public CdnInfoContext()
        {
            GameService.GameIntegration.Gw2Started += GameIntegrationOnGw2Started;
        }

        /// <inheritdoc />
        protected override void Load()
        {
            GetCdnInfoFromCdnUrl(GW2_ASSETCDN_URL)
                .ContinueWith(cdnInfo => SetCdnInfo(ref this._standardCdnInfo, cdnInfo.Result));
            GetCdnInfoFromCdnUrl(GW2_CN_ASSETCDN_URL)
                .ContinueWith(cdnInfo => SetCdnInfo(ref this._chineseCdnInfo, cdnInfo.Result));
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            this._loadCount = 0;
        }

        private void GameIntegrationOnGw2Started(object sender, EventArgs e)
        {
            // Unload without DoUnload to avoid expiring the context
            Unload();

            DoLoad();
        }

        private CdnInfo ParseCdnInfo(string rawCdnResponse)
        {
            if (string.IsNullOrEmpty(rawCdnResponse))
            {
                Logger.Warn("Failed to parse null or empty CDN response.");
                return CdnInfo.Invalid;
            }

            var cdnVars = rawCdnResponse.Split(' ');

            if (cdnVars.Length == 5)
            {
                var parsedSuccessfully = true;

                parsedSuccessfully &= int.TryParse(cdnVars[0], out var buildId);
                parsedSuccessfully &= int.TryParse(cdnVars[1], out var exeFileId);
                parsedSuccessfully &= int.TryParse(cdnVars[2], out var exeFileSize);
                parsedSuccessfully &= int.TryParse(cdnVars[3], out var manifestFileId);
                parsedSuccessfully &= int.TryParse(cdnVars[4], out var manifestFileSize);

                if (parsedSuccessfully)
                {
                    return new CdnInfo(buildId, exeFileId, exeFileSize, manifestFileId, manifestFileSize);
                }

                Logger.Warn("Failed to parse CDN response {rawCdnResponse}.", rawCdnResponse);
                return CdnInfo.Invalid;
            }

            Logger.Warn("Unexpected number of values provided by CDN response {rawCdnResponse}.", rawCdnResponse);
            return CdnInfo.Invalid;
        }

        private void SetCdnInfo(ref CdnInfo cdnInfo, string result)
        {
            cdnInfo = ParseCdnInfo(result);

            if (++this._loadCount >= TOTAL_CDN_ENDPOINTS)
            {
                ConfirmReady();
            }
        }

        private async Task<string> GetCdnInfoFromCdnUrl(string cdnUrl)
        {
            try
            {
                return await cdnUrl.GetStringAsync();
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                {
                    Logger.Warn(ex,
                        "Failed to get CDN information from {cdnUrl}.  HTTP response status was ({httpStatusCode}) {statusReason}.",
                        cdnUrl, (int) ex.Call.Response.StatusCode, ex.Call.Response.ReasonPhrase);
                }
                else
                {
                    Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.  No response was received.", cdnUrl);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.  An unexpected exception occurred.",
                    cdnUrl);
            }

            return null;
        }

        #endregion
    }
}