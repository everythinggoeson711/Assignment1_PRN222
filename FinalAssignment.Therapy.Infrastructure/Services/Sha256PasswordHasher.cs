using System.Security.Cryptography;
using System.Text;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Infrastructure.Services;

public class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string value, string hash)
        => string.Equals(Hash(value), hash, StringComparison.OrdinalIgnoreCase);
}