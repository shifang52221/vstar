using System.Security.Cryptography;
using System.Text;

namespace VStartNext.Infrastructure.Security;

public sealed class DpapiSecretProtector : ISecretProtector
{
    public string Protect(string plaintext)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = ProtectedData.Protect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(cipher);
    }

    public string Unprotect(string ciphertext)
    {
        var cipher = Convert.FromBase64String(ciphertext);
        var plain = ProtectedData.Unprotect(cipher, optionalEntropy: null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plain);
    }
}
