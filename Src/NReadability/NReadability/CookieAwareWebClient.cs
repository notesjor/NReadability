using System;
using System.Net;

namespace NReadability
{
  public class CookieAwareWebClient : WebClient
  {
    private readonly CookieContainer _cookieContainer;

    #region Constructor(s)

    public CookieAwareWebClient()
    {
      _cookieContainer = new CookieContainer();
    }

    #endregion

    #region Overrides of WebClient

    protected override WebRequest GetWebRequest(Uri address)
    {
      var webRequest = base.GetWebRequest(address);

      if (webRequest is HttpWebRequest httpWebRequest) httpWebRequest.CookieContainer = _cookieContainer;

      return webRequest;
    }

    #endregion
  }
}