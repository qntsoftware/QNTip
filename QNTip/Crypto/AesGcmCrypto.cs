using System.Security.Cryptography;

namespace Qntip.Crypto;

internal static class AesGcmCrypto
{
    public const int KeySize = 32;
    public const int NonceSize = 12;
    public const int TagSize = 16;

    public static byte[] GenerateKey() => RandomNumberGenerator.GetBytes(KeySize);

    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var output = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
        Buffer.BlockCopy(ciphertext, 0, output, NonceSize + TagSize, ciphertext.Length);
        return output;
    }

    public static byte[] Decrypt(byte[] input, byte[] key)
    {
        if (input.Length < NonceSize + TagSize)
            throw new ArgumentException("Ciphertext too short.");

        var nonce = input.AsSpan(0, NonceSize).ToArray();
        var tag = input.AsSpan(NonceSize, TagSize).ToArray();
        var ciphertext = input.AsSpan(NonceSize + TagSize).ToArray();
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}