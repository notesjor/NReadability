namespace NReadability
{
  public class TranscodingResult
  {
    public TranscodingResult(bool contentExtracted, bool titleExtracted)
    {
      ContentExtracted = contentExtracted;
      TitleExtracted = titleExtracted;
    }

    public bool ContentExtracted { get; }

    public bool TitleExtracted { get; }

    public string ExtractedContent { get; set; }

    public string ExtractedTitle { get; set; }

    public string NextPageUrl { get; set; }
  }
}