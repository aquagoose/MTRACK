using libmtrack.Audio;

namespace libmtrack.Tracker;

public class TrackPlayer
{
    private Resampler _resampler;
    private Track _track;

    private int _samplesPerTick;
    private int _currentSamples;
    private int _tick;
    private int _order;
    private int _row;
    private int _speed;

    public uint SampleRate => _resampler.SampleRate;
    public bool IsFinished { get; private set; }
    public bool Loop;

    private short[] _empty;

    public TrackPlayer(Track track, bool loop = true)
    {
        _resampler = new Resampler(32, 48000, true, track.Samples);
        _resampler.SampleVolume = track.InitialVolume / (float) byte.MaxValue;

        _track = track;

        _speed = track.InitialSpeed;
        _samplesPerTick = CalculateSamplesPerTick(track.InitialTempo);

        Loop = loop;

        _empty = new short[2];
    }

    public short[] Advance()
    {
        if (IsFinished)
            return _empty;
        
        if (_currentSamples == 0 && _tick == 0)
        {
            ref Pattern pattern = ref _track.Patterns[_track.Orders[_order]];
            for (int c = 0; c < pattern.Channels; c++)
            {
                ref Note note = ref pattern.Notes[c, _row];
                ref Sample sample = ref _track.Samples[note.Sample];
                if (note.Key != PianoKey.None)
                {
                    float multiplier = CalculateSampleRate(note.Key, note.Octave, sample.SampleRate, sample.Multiplier);
                    _resampler.SetSampleRate((uint) c, multiplier, note.Sample);
                }
            }
        }
        
        short[] advance = _resampler.Advance();

        _currentSamples++;
        if (_currentSamples >= _samplesPerTick)
        {
            _currentSamples = 0;
            _tick++;
            if (_tick >= _speed)
            {
                _tick = 0;
                _row++;
                if (_row >= _track.Patterns[_track.Orders[_order]].Rows)
                {
                    _row = 0;
                    _order++;
                    if (_order >= _track.Orders.Length)
                    {
                        if (Loop)
                            _order = 0;
                        else
                            IsFinished = true;
                    }
                }
            }
        }

        return advance;
    }

    private int CalculateSamplesPerTick(int tempo)
    {
        return (int) ((2.5f / tempo) * _resampler.SampleRate);
    }

    public static float CalculateSampleRate(PianoKey key, Octave octave, float c5Rate, float multiplier)
    {
        int note = 40 + (int) (key - 3) + (int) (octave - 4) * 12;
        float powNote = MathF.Pow(2, (note - 49f) / 12f);
        return c5Rate * powNote * multiplier;
    }
}