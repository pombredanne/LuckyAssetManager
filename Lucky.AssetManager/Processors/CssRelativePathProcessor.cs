﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Lucky.AssetManager.Assets;
using Lucky.AssetManager.Assets.AssetReaders;

namespace Lucky.AssetManager.Processors {
    internal class CssRelativePathProcessor : IProcessor {

        public IEnumerable<IAsset> Process(IEnumerable<IAsset> assets) {
            var results = assets.Where(a => a.CurrentPathIsExternal || a is CssAsset == false || a.IgnoreProcessing).ToList();
            foreach (IAsset asset in assets.Where(a => a.CurrentPathIsExternal == false && a is CssAsset && a.IgnoreProcessing == false)) {
                var newContent = Process(asset, asset.Reader.Content);
                asset.Reader = new MemoryAssetReader(asset.Reader.AssociatedFilePaths, newContent );
                results.Add(asset);
            }
            return results;
        }

        private const string Regex = @"url\((?<quote>'?""?)(?<url>.*)""?'?\)";

        private static string Process(IAsset asset, string assetContent) {
            var urlRegex = new Regex(Regex, RegexOptions.IgnoreCase);
            return urlRegex.Replace(assetContent, m => {
                                                      var result = m.Groups[0].Value;

                                                      if (m.Groups["url"].Success) {
                                                          var urlValue = m.Groups["url"].Value.TrimStart('/');
                                                          result = "url(";
                                                          if (m.Groups["quote"].Success) {
                                                              result += m.Groups["quote"].Value;
                                                          }
                                                          result += ToAbsoluteUrl(asset.CurrentPath, urlValue) + ")";
                                                      }

                                                      return result;
                                                  });
        }

        private static string ToAbsoluteUrl(string path, string relativeUrl) {
            var fromUri = new Uri(HttpContext.Current.Server.MapPath("~/"));
            var toUri = new Uri(new FileInfo(HttpContext.Current.Server.MapPath(path)).DirectoryName);

            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("~/"))
                relativeUrl = VirtualPathUtility.ToAbsolute(relativeUrl);

            var url = HttpContext.Current.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

            return string.Format("{0}://{1}{2}/{3}/{4}", url.Scheme, url.Host, port, fromUri.MakeRelativeUri(toUri), relativeUrl);
        }

    }
}