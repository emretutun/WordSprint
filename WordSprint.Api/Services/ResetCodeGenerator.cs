using System.Security.Cryptography;
using System.Text;

namespace WordSprint.Api.Services;

public static class ResetCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789abcdefghjkmnpqrstuvwxyz";

    public static string Generate(int length = 6)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (int i = 0; i < length; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];

        return new string(chars);
    }

    public static string Hash(string code, string userId, string pepper)
    {
        var input = $"{userId}:{code}:{pepper}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
