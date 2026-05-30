namespace DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;

[SaveDefinitionType(SaveDefinitionEnum.Index)]
public sealed class IndexSaveDefinition : DefaultSaveDefinition
{
    public override string FileName => "index";
    public override bool SupportsCompression => false;

    /// <summary>
    /// Derives an XOR mask by XORing the known pattern with bytes at known offset in the provided data.
    /// </summary>
    /// <param name="data">The byte data containing the encoded pattern.</param>
    /// <returns>A byte array representing the derived XOR mask.</returns>
    /// <exception cref="ArgumentException">The data length is less than the required offset plus pattern length.</exception>
    public static byte[] GuessXorMask(ReadOnlySpan<byte> data)
    {
        const byte patternOffset = 0x10;
        var pattern = "meHeader"u8.ToArray();
        if (data.Length < patternOffset + pattern.Length)
            throw new ArgumentException("Data is too short to contain the pattern.");
        var slice = data.Slice(patternOffset, pattern.Length);
        for (var i = 0; i < slice.Length; i++)
            pattern[i] ^= slice[i];
        return pattern;
    }
}
