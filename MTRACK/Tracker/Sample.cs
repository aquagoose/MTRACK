using System;
using System.IO;
using Pie.Audio;

namespace MTRACK.Tracker;

public struct Sample
{
    public byte[] Data;
    public bool Stereo;
    public bool SixteenBit;
    public uint SampleRate;
    public uint DataLengthInSamples;

    public float Multiplier;

    public bool Loop;
    public uint LoopStart;
    public uint LoopEnd;

    public static Sample LoadFromWav(string path)
    {
        byte[] data = AudioHelper.LoadWav(File.ReadAllBytes(path), out uint sampleRate, out AudioFormat format);

        int channels = 0, bitsPerSample = 0;
        switch (format)
        {
            case AudioFormat.Mono8:
                channels = 1;
                bitsPerSample = 8;
                break;
            case AudioFormat.Mono16:
                channels = 1;
                bitsPerSample = 16;
                break;
            case AudioFormat.Stereo8:
                channels = 2;
                bitsPerSample = 8;
                break;
            case AudioFormat.Stereo16:
                channels = 2;
                bitsPerSample = 16;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        uint dataLength = (uint) (data.Length / channels / (bitsPerSample == 16 ? 2 : 1));
        return new Sample()
        {
            Data = data,
            Stereo = channels == 2,
            SixteenBit = bitsPerSample == 16,
            SampleRate = sampleRate,
            DataLengthInSamples = dataLength,
            Multiplier = sampleRate / TrackPlayer.CalculateSampleRate(PianoKey.C, Octave.Octave4, sampleRate, 1),
            LoopStart = 0,
            LoopEnd = dataLength
        };
    }
}