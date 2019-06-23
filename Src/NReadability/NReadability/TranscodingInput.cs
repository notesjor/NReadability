using System;

namespace NReadability
{
  public class TranscodingInput
  {
    private DomSerializationParams _domSerializationParams;

    public TranscodingInput(string htmlContent)
    {
      if (string.IsNullOrEmpty(htmlContent))
        throw new ArgumentException("Argument can't be null nor empty.", nameof(htmlContent));

      HtmlContent = htmlContent;
    }

    public string HtmlContent { get; }

    public string Url { get; set; }

    public DomSerializationParams DomSerializationParams
    {
      get => _domSerializationParams ?? (_domSerializationParams = DomSerializationParams.CreateDefault());
      set => _domSerializationParams = value;
    }
  }
}