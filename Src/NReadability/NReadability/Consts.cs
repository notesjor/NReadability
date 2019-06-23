using System.Reflection;

namespace NReadability
{
  public static class Consts
  {
    #region Constructor(s)

    static Consts()
    {
      NReadabilityFullName = $"NReadability {Assembly.GetExecutingAssembly().GetName().Version}";
    }

    #endregion

    #region Properties

    public static string NReadabilityFullName { get; }

    #endregion
  }
}