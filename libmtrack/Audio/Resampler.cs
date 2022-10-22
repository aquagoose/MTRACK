using libmtrack.Tracker;

namespace libmtrack.Audio;

public class Resampler
{
    private readonly Sample[] _samples;
    private Channel[] _channels;
    private bool _stereo;
    
    public readonly uint SampleRate;
    //public uint BufferPos;
    //public readonly short[] SongBuffer;
    //public readonly uint BufferLengthInSamples;
    private readonly short[] _leftRight;

    public float SampleVolume;

    public Resampler(int numChannels, uint sampleRate, bool stereo, Sample[] samples)
    {
        SampleRate = sampleRate;
        
        _channels = new Channel[numChannels];
        for (int i = 0; i < numChannels; i++)
            _channels[i] = new Channel(this);
        
        _stereo = stereo;
        _samples = samples;
        // Create a song buffer that is always half a second in length.
        //BufferLengthInSamples = sampleRate / 2;
        //SongBuffer = new short[stereo ? BufferLengthInSamples * 2 : BufferLengthInSamples];
        //BufferPos = 0;
        _leftRight = new short[2];

        SampleVolume = 1;
    }

    public short[] Advance()
    {
        // Clear the song buffer at this position. If you remove this, it will try to mix with what's already there!
        _leftRight[0] = 0;
        _leftRight[1] = 0;
        for (int c = 0; c < _channels.Length; c++)
        {
            ref Channel channel = ref _channels[c];
            ref Sample sample = ref _samples[channel.Sample];
            
            channel.Advance();
            if (sample.Stereo)
            {
                for (int a = 0; a < 2; a++)
                {
                    short value, valueNext;
                    if (sample.UseAlternativeStereoMethod)
                    {
                        value = GetSample((int) (channel.SamplePos + (a == 1 ? sample.DataLengthInSamples : 0)), ref sample);
                        valueNext = GetSample((int) (channel.NextSamplePos + (a == 1 ? sample.DataLengthInSamples : 0)), ref sample);
                    }
                    else
                    {
                        value = GetSample(channel.SamplePos * 2 + a, ref sample);
                        valueNext = GetSample(channel.NextSamplePos * 2 + a, ref sample);
                    }

                    value = (short) (MathHelper.Lerp(value, valueNext, channel.SamplePosF - channel.SamplePos) * channel.NormalizedVolume);
                    _leftRight[a] = Mix(_leftRight[a], value);
                }
            }
            else
            {
                short value = GetSample(channel.SamplePos, ref sample);
                short valueNext = GetSample(channel.NextSamplePos, ref sample);
                value = (short) (MathHelper.Lerp(value, valueNext, channel.SamplePosF - channel.SamplePos) * channel.NormalizedVolume);
                _leftRight[0] = Mix(_leftRight[0], value);
                _leftRight[1] = Mix(_leftRight[1], value);
            }
        }

        //BufferPos++;
        return _leftRight;
    }

    public void SetSampleRate(uint channel, float sampleRate, uint sample, float volume)
    {
        ref Channel chn = ref _channels[channel];
        ref Sample smp = ref _samples[sample];
        chn.SampleRate = sampleRate;
        chn.Sample = sample;
        chn.SamplePos = 0;
        chn.Loop = smp.Loop;
        chn.LoopStart = smp.LoopStart;
        chn.LoopEnd = smp.Loop ? smp.LoopEnd - 1 : smp.DataLengthInSamples - 1;
        chn.NormalizedVolume = volume;
    }

    public void SetSampleRate(uint channel, float sampleRate)
    {
        ref Channel chn = ref _channels[channel];
        chn.SampleRate = sampleRate;
    }

    public void SetSampleOffset(uint channel, uint offset)
    {
        ref Channel chn = ref _channels[channel];
        chn.SamplePos = (int) offset;
    }

    public void SetChannelVolume(uint channel, float volume)
    {
        ref Channel chn = ref _channels[channel];
        chn.NormalizedVolume = volume;
    }

    private short GetSample(int pos, ref Sample sample)
    {
        short value;
        if (sample.SixteenBit)
        {
            pos *= 2;
            pos -= pos % 2;
            if (pos >= sample.Data.Length)
                return 0;
            value = (short) (sample.Data[pos] | (sample.Data[pos + 1] << 8));
        }
        else
            value = (short) ((sample.Data[pos] << 8) - (sample.Unsigned ? 0 : short.MaxValue));

        return value;
    }

    private short Mix(short value1, short value2)
    {
        return (short) MathHelper.Clamp(value1 + (value2 * SampleVolume), short.MinValue, short.MaxValue);
    }
}