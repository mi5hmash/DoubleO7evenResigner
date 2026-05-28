using System.Runtime.InteropServices;

namespace DoubleO7evenResignerCore.Helpers;

public class DoubleO7igner
{
    public static void UnSign(Span<byte> dataSpan, ulong signature)
    {
        var dataSpanAsUlong = MemoryMarshal.Cast<byte, ulong>(dataSpan);
        var tailLength = dataSpan.Length % sizeof(ulong);
        var dataTail = dataSpan[^tailLength..];

        var signatureBytes = BitConverter.GetBytes(signature);

        // XOR the main portion of the data span (interpreted as ulong) with the signature
        for (var i = 0; i < dataSpanAsUlong.Length; i++)
            dataSpanAsUlong[i] ^= signature;
        // XOR the remaining bytes in the tail with the corresponding bytes from the signature
        for (var i = 0; i < tailLength; i++)
            dataTail[i] ^= signatureBytes[i];
    }

    public static void ReSign(Span<byte> dataSpan, ulong signatureInput, ulong signatureOutput)
    {
        var dataSpanAsUlong = MemoryMarshal.Cast<byte, ulong>(dataSpan);
        var tailLength = dataSpan.Length % sizeof(ulong);
        var dataTail = dataSpan[^tailLength..];

        // UnSign the data with the input signature
        var signatureInputBytes = BitConverter.GetBytes(signatureInput);
        
        for (var i = 0; i < dataSpanAsUlong.Length; i++)
            dataSpanAsUlong[i] ^= signatureInput;

        for (var i = 0; i < tailLength; i++)
            dataTail[i] ^= signatureInputBytes[i];

        // Sign the data with the output signature
        var signatureOutputBytes = BitConverter.GetBytes(signatureOutput);

        for (var i = 0; i < dataSpanAsUlong.Length; i++)
            dataSpanAsUlong[i] ^= signatureOutput;

        for (var i = 0; i < tailLength; i++)
            dataTail[i] ^= signatureOutputBytes[i];
    }
}