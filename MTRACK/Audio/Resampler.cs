using Cubic.Utilities;
using MTRACK.Tracker;

namespace MTRACK.Audio;

public class Resampler
{
    private readonly Sample[] _samples;
    private Channel[] _channels;
    private bool _stereo;
    
    public readonly uint SampleRate;
    public uint BufferPos;
    public readonly short[] SongBuffer;
    public readonly uint BufferLengthInSamples;

    public Resampler(int numChannels, uint sampleRate, bool stereo, Sample[] samples)
    {
        SampleRate = sampleRate;
        
        _channels = new Channel[numChannels];
        for (int i = 0; i < numChannels; i++)
            _channels[i] = new Channel(this);
        
        _stereo = stereo;
        _samples = samples;
        // Create a song buffer that is always half a second in length.
        BufferLengthInSamples = sampleRate / 2;
        SongBuffer = new short[stereo ? BufferLengthInSamples * 2 : BufferLengthInSamples];
        BufferPos = 0;
    }

    public void Advance()
    {
        SongBuffer[BufferPos * 2] = 0;
        SongBuffer[BufferPos * 2 + 1] = 0;
        for (int c = 0; c < _channels.Length; c++)
        {
            ref Channel channel = ref _channels[c];
            ref Sample sample = ref _samples[channel.Sample];
            
            channel.Advance();
            if (sample.Stereo)
            {
                for (int a = 0; a < 2; a++)
                {
                    short value = GetSample((channel.SamplePos + channel.Chunk * Channel.ChunkSamples) * 2 + a, ref sample);
                    short valueNext = GetSample((channel.NextSamplePos + channel.Chunk * Channel.ChunkSamples) * 2 + a, ref sample);
                    value = (short) CubicMath.Lerp(value, valueNext, channel.SamplePosF - channel.SamplePos);
                    SongBuffer[BufferPos * 2 + a] = Mix(SongBuffer[BufferPos * 2 + a], value);
                }
            }
            else
            {
                short value = GetSample(channel.SamplePos + channel.Chunk * Channel.ChunkSamples, ref sample);
                short valueNext = GetSample(channel.NextSamplePos + channel.Chunk * Channel.ChunkSamples, ref sample);
                value = (short) CubicMath.Lerp(value, valueNext, channel.SamplePosF - channel.SamplePos);
                SongBuffer[BufferPos * 2] = Mix(SongBuffer[BufferPos * 2], value);
                SongBuffer[BufferPos * 2 + 1] = Mix(SongBuffer[BufferPos * 2 + 1], value);
            }
        }

        BufferPos++;
    }

    public void SetSampleRate(uint channel, float sampleRate, uint sample)
    {
        ref Channel chn = ref _channels[channel];
        chn.SampleRate = sampleRate;
        chn.Sample = sample;
        chn.SamplePos = 0;
        chn.Loop = true;
        chn.LoopEnd = _samples[sample].DataLengthInSamples;
    }

    public short GetSample(int pos, ref Sample sample)
    {
        short value;
        if (sample.SixteenBit)
        {
            pos *= 2;
            pos -= pos % 2;
            value = (short) (sample.Data[pos] | (sample.Data[pos + 1] << 8));
        }
        else
            value = (short) ((sample.Data[pos] << 8) - short.MaxValue);

        return value;
    }

    private short Mix(short value1, short value2)
    {
        return (short) CubicMath.Clamp(value1 + (value2), short.MinValue, short.MaxValue);
    }
}