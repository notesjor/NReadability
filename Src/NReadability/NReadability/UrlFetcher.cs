﻿/*
 * NReadability
 * http://code.google.com/p/nreadability/
 * 
 * Copyright 2010 Marek Stój
 * http://immortal.pl/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NReadability
{
  public class UrlFetcher : IUrlFetcher
  {
    private const int _bufferSize = 8192;

    private static readonly Encoding _DefaultFallbackEncoding = Encoding.UTF8;

    private static readonly Regex _MetaTagRegex = new Regex("<meta[^>]+content=\"[^\"]*charset=(?<charset>[^\"]+)\"",
                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Encoding _fallbackEncoding;
    private readonly CookieAwareWebClient _webClient;

    #region IUrlFetcher Members

    public string Fetch(string url)
    {
      return DownloadString(url);
    }

    #endregion

    #region Nested types

    private enum CompressionAlgorithm
    {
      GZip,
      Deflate
    }

    #endregion

    #region Constructor(s)

    public UrlFetcher(Encoding fallbackEncoding)
    {
      _fallbackEncoding = fallbackEncoding ?? _DefaultFallbackEncoding;

      _webClient = new CookieAwareWebClient();
      _webClient.Headers.Add("User-Agent",
                             "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
      _webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
      _webClient.Headers.Add("Accept-Encoding", "gzip,deflate");
    }

    public UrlFetcher()
      : this(null)
    {
    }

    #endregion

    #region Public methods

    public string DownloadString(string url)
    {
      return MakeRequest(url, () => _webClient.DownloadData(url));
    }

    public string UploadValues(string url, NameValueCollection keyValuePairs)
    {
      return MakeRequest(url, () => _webClient.UploadValues(url, keyValuePairs));
    }

    #endregion

    #region Private helper methods

    private static Encoding GetEncodingFromMetaTag(byte[] responseBytes)
    {
      var responseText = Encoding.ASCII.GetString(responseBytes);
      var match = _MetaTagRegex.Match(responseText);

      if (match.Success)
      {
        var charset = match.Groups["charset"].Value;

        try
        {
          return Encoding.GetEncoding(charset);
        }
        catch (ArgumentException)
        {
          return null;
        }
      }

      return null;
    }

    private static byte[] Decompress(byte[] responseBytes, CompressionAlgorithm compressionAlgorithm)
    {
      Stream decompressingStream;

      switch (compressionAlgorithm)
      {
        case CompressionAlgorithm.GZip:
          decompressingStream = new GZipStream(new MemoryStream(responseBytes), CompressionMode.Decompress);
          break;

        case CompressionAlgorithm.Deflate:
          decompressingStream = new DeflateStream(new MemoryStream(responseBytes), CompressionMode.Decompress);
          break;

        default:
          throw new NotSupportedException("Unsupported compression algorithm: " + compressionAlgorithm + ".");
      }

      try
      {
        var buffer = new byte[_bufferSize];

        using (var outputMemoryStream = new MemoryStream())
        {
          while (true)
          {
            var bytesRead = decompressingStream.Read(buffer, 0, buffer.Length);

            if (bytesRead <= 0) break;

            outputMemoryStream.Write(buffer, 0, bytesRead);
          }

          return outputMemoryStream.ToArray();
        }
      }
      finally
      {
        decompressingStream.Close();
      }
    }

    private string MakeRequest(string url, Func<byte[]> makeRequestFunc)
    {
      if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

      var responseBytes = makeRequestFunc();
      var contentEncoding = _webClient.ResponseHeaders["Content-Encoding"];

      if (!string.IsNullOrEmpty(contentEncoding))
      {
        contentEncoding = contentEncoding.ToLower();

        if (contentEncoding.Contains("gzip"))
          responseBytes = Decompress(responseBytes, CompressionAlgorithm.GZip);
        else if (contentEncoding.Contains("deflate"))
          responseBytes = Decompress(responseBytes, CompressionAlgorithm.Deflate);
        else
          throw new NotSupportedException("Unsupported content encoding: " + contentEncoding + ".");
      }

      var encoding = GuessEncoding(_webClient.ResponseHeaders, responseBytes);

      return encoding.GetString(responseBytes);
    }

    private Encoding GuessEncoding(WebHeaderCollection headers, byte[] responseBytes)
    {
      try
      {
        if (headers == null) return _fallbackEncoding;

        var contentType = headers[HttpResponseHeader.ContentType];

        if (string.IsNullOrEmpty(contentType)) return _fallbackEncoding;

        Encoding resultEncoding = null;
        var splittedContentType = contentType.ToLower(CultureInfo.InvariantCulture).Split(';', '=', ' ');
        var isCharset = false;

        foreach (var s in splittedContentType)
          if (s == "charset")
          {
            isCharset = true;
          }
          else if (isCharset)
          {
            try
            {
              resultEncoding = Encoding.GetEncoding(s);
            }
            catch (ArgumentException)
            {
              // encoding not supported - we'll fallback later
            }

            break;
          }

        return resultEncoding ?? (GetEncodingFromMetaTag(responseBytes) ?? _fallbackEncoding);
      }
      catch (Exception exception)
      {
        if (exception is ThreadAbortException || exception is StackOverflowException ||
            exception is OutOfMemoryException) throw;
      }

      return _fallbackEncoding;
    }

    #endregion
  }
}