using System;

namespace MTRACK.Audio;

public struct Channel
{
    public readonly int ChunkSamples;
    
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
        get => (int) _samplePos + Chunk * ChunkSamples;
        set => SamplePosF = value;
    }

    public int NextSamplePos
    {
        get
        {
            if (Speed < 0)
                return Speed > -1 ? SamplePos - 1 : SamplePos;
            else
                return Speed < 1 ? SamplePos + 1 : SamplePos;
        }
    }

    public float SamplePosF
    {
        get => _samplePos + Chunk * ChunkSamples;
        set
        {
            Chunk = (int) (value / ChunkSamples);
            _samplePos = value - (ChunkSamples * Chunk);
        }
    }

    public float NormalizedVolume;

    public bool PlayingBackward;

    public uint LoopStart;
    public uint LoopEnd;
    public bool Loop;

    public Channel(Resampler resampler)
    {
        _resamplerSampleRate = resampler.SampleRate;
        ChunkSamples = (int) resampler.SampleRate;
        _sampleRate = 0;
        Speed = 0;
        Sample = 0;
        NormalizedVolume = 0;
        _samplePos = 0;
        Chunk = 0;
        PlayingBackward = false;
        Loop = false;
        LoopStart = 0;
        LoopEnd = 0;
    }

    public void Advance()
    {
        if (_sampleRate == 0)
            return;
        _samplePos += Speed;
        if (_samplePos >= ChunkSamples)
        {
            Chunk++;
            _samplePos = _samplePos - ChunkSamples;
        }

        if (_samplePos <= -ChunkSamples)
        {
            Chunk--;
            _samplePos = _samplePos + ChunkSamples;
        }

        if (SamplePosF >= LoopEnd)
        {
            if (Loop)
                SamplePosF = LoopStart + (SamplePosF - LoopEnd);
            else
            {
                SampleRate = 0;
                SamplePos = 0;
            }
        }

        if (SamplePosF <= LoopStart)
        {
            if (Loop)
                SamplePosF = LoopEnd - (SamplePosF + LoopStart);
            else
            {
                SampleRate = 0;
                SamplePos = 0;
            }
        }
    }
}