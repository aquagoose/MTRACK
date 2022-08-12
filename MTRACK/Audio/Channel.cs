using System;

namespace MTRACK.Audio;

public struct Channel
{
    public const int ChunkSamples = 48000;
    
    private float _resamplerSampleRate;
    private float _sampleRate;
    private float _samplePos;
    public int Chunk;

    public float SampleRate
    {
        get => _sampleRate;
        set
        {
            _sampleRate = value;
            Speed = value / _resamplerSampleRate;
        }
    }
    
    public float Speed;

    public uint Sample;

    public int SamplePos
    {
        get => (int) _samplePos;
        set => _samplePos = value;
    }

    public int NextSamplePos => Speed < 1 ? SamplePos + 1 : SamplePos;

    public float SamplePosF => _samplePos;

    public float NormalizedVolume;

    public bool PlayingBackward;

    public uint LoopStart;
    public uint LoopEnd;
    public bool Loop;

    public Channel(Resampler resampler)
    {
        _resamplerSampleRate = resampler.SampleRate;
        _sampleRate = _resamplerSampleRate;
        Speed = 1;
        Sample = 0;
        NormalizedVolume = 1;
        _samplePos = 0;
        Chunk = 0;
        PlayingBackward = false;
        Loop = false;
        LoopStart = 0;
        LoopEnd = 0;
    }

    public void Advance()
    {
        _samplePos += Speed;
        if (_samplePos >= ChunkSamples)
        {
            Chunk++;
            _samplePos = _samplePos - ChunkSamples;
        }

        if (Loop && _samplePos + Chunk * ChunkSamples >= LoopEnd)
        {
            _samplePos = _samplePos - LoopStart;
            Chunk = (int) (_samplePos / ChunkSamples);
            _samplePos = _samplePos - Chunk * ChunkSamples;
        }
    }
}