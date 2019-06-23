namespace NReadability
{
  public class WebTranscodingResult
  {
    public WebTranscodingResult(bool contentExtracted, bool titleExtracted)
    {
      ContentExtracted = contentExtracted;
      TitleExtracted = titleExtracted;
    }

    public bool ContentExtracted { get; }

    public bool TitleExtracted { get; }

    public string ExtractedContent { get; set; }

    public string ExtractedTitle { get; set; }
  }
}