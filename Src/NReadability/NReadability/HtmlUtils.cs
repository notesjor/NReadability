using System;

namespace NReadability
{
  public static class HtmlUtils
  {
    public static string RemoveScriptTags(string htmlContent)
    {
      if (htmlContent == null) throw new ArgumentNullException(nameof(htmlContent));

      if (htmlContent.Length == 0) return "";

      var indexOfScriptTagStart = htmlContent.IndexOf("<script", StringComparison.OrdinalIgnoreCase);

      if (indexOfScriptTagStart == -1) return htmlContent;

      var indexOfScriptTagEnd =
        htmlContent.IndexOf("</script>", indexOfScriptTagStart, StringComparison.OrdinalIgnoreCase);

      if (indexOfScriptTagEnd == -1) return htmlContent.Substring(0, indexOfScriptTagStart);

      var strippedHtmlContent =
        htmlContent.Substring(0, indexOfScriptTagStart) +
        htmlContent.Substring(indexOfScriptTagEnd + "</script>".Length);

      return RemoveScriptTags(strippedHtmlContent);
    }
  }
}