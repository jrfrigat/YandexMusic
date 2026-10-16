using System.Buffers.Binary;
using System.Security.Cryptography;

namespace YandexMusic.Http;

/// <summary>
/// Decrypts an <c>encraw</c> track file: AES-128 in CTR mode with a zero initial counter, the whole
/// 16-byte block treated as one big-endian counter. .NET has no built-in CTR mode, so the keystream
/// is produced by encrypting the counter blocks with AES-ECB and XOR-ing them into the data. The
/// transform keeps its position between calls, so a file can be decrypted chunk by chunk while it
/// streams instead of being buffered whole.
/// </summary>
internal sealed class AesCtrDecryptor : IDisposable
{
    private const int BlockSize = 16;

    // Keystream is generated in batches so the ECB call is amortized over many blocks.
    private const int BatchBlocks = 256;

    private readonly Aes _aes;
    private readonly byte[] _counters = new byte[BlockSize * BatchBlocks];
    private readonly byte[] _keystream = new byte[BlockSize * BatchBlocks];
    private UInt128 _counter;
    private int _keystreamOffset = BlockSize * BatchBlocks;

    /// <summary>Creates a decryptor positioned at the start of the file.</summary>
    /// <param name="hexKey">The key in hexadecimal, as delivered with the file.</param>
    /// <exception cref="ArgumentException">The key is not valid hexadecimal or has an unsupported length.</exception>
    public AesCtrDecryptor(string hexKey)
    {
        byte[] key;
        try
        {
            key = Convert.FromHexString(hexKey);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("The decryption key is not valid hexadecimal.", nameof(hexKey), ex);
        }

        if (key.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("The decryption key has an unsupported length.", nameof(hexKey));
        }

        _aes = Aes.Create();
        _aes.Key = key;
    }

    /// <summary>Decrypts the next part of the file in place.</summary>
    /// <param name="data">The bytes that follow the ones transformed so far.</param>
    public void Transform(Span<byte> data)
    {
        var index = 0;
        while (index < data.Length)
        {
            if (_keystreamOffset == _keystream.Length)
            {
                RefillKeystream();
            }

            var count = Math.Min(data.Length - index, _keystream.Length - _keystreamOffset);
            var target = data.Slice(index, count);
            var source = _keystream.AsSpan(_keystreamOffset, count);
            for (var i = 0; i < count; i++)
            {
                target[i] ^= source[i];
            }

            index += count;
            _keystreamOffset += count;
        }
    }

    /// <inheritdoc />
    public void Dispose() => _aes.Dispose();

    private void RefillKeystream()
    {
        for (var block = 0; block < BatchBlocks; block++)
        {
            BinaryPrimitives.WriteUInt128BigEndian(_counters.AsSpan(block * BlockSize, BlockSize), _counter);
            _counter++;
        }

        _aes.EncryptEcb(_counters, _keystream, PaddingMode.None);
        _keystreamOffset = 0;
    }
}
