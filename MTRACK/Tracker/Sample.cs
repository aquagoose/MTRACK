using System.IO;
using Cubic.Audio;

namespace MTRACK.Tracker;

public struct Sample
{
    public byte[] Data;
    public bool Stereo;
    public bool SixteenBit;
    public uint SampleRate;
    public uint DataLengthInSamples;

    public float Multiplier;

    public static Sample LoadFromWav(string path)
    {
        byte[] data = Sound.LoadWav(File.ReadAllBytes(path), out int channels, out int sampleRate, out int bitsPerSample);
        return new Sample()
        {
            Data = data,
            Stereo = channels == 2,
            SixteenBit = bitsPerSample == 16,
            SampleRate = (uint) sampleRate,
            DataLengthInSamples = (uint) (data.Length / channels / (bitsPerSample == 16 ? 2 : 1)),
            Multiplier = sampleRate / TrackPlayer.CalculateSampleRate(PianoKey.C, Octave.Octave4, sampleRate, 1)
        };
    }
}