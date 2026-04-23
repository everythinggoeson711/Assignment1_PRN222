namespace Montra.BLL.Services
{
    public static class PublicTrackingCodeGenerator
    {
        public static string Generate(string prefix)
        {
            var token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"{prefix}-{DateTime.UtcNow:yyMMdd}-{token}";
        }
    }
}