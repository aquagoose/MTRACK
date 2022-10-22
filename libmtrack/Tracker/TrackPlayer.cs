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

    public TrackPlayer(Track track, uint sampleRate, bool loop = true)
    {
        _resampler = new Resampler(32, sampleRate, true, track.Samples);
        _resampler.SampleVolume = track.InitialVolume / (float) byte.MaxValue;

        _track = track;

        _speed = track.InitialSpeed;
        _samplesPerTick = CalculateSamplesPerTick(track.InitialTempo);

        if (_track.Orders.Length == 0)
            IsFinished = true;

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
            for (uint c = 0; c < pattern.Channels; c++)
            {
                ref Note note = ref pattern.Notes[c, _row];
                if (!note.Initialized)
                    continue;
                switch (note.Key)
                {
                    case PianoKey.None:
                        if (note.Volume <= 64)
                            _resampler.SetChannelVolume(c, note.NormalizedVolume);
                        break;
                    case PianoKey.NoteCut:
                        _resampler.SetSampleRate(c, 0, 0, 0);
                        break;
                    case PianoKey.NoteOff:
                        _resampler.SetSampleRate(c, 0, 0, 0);
                        break;
                    default:
                        ref Sample sample = ref _track.Samples[note.Sample];
                        float multiplier = CalculateSampleRate(note.Key, note.Octave, sample.SampleRate, sample.Multiplier);
                        _resampler.SetSampleRate(c, multiplier, note.Sample, note.NormalizedVolume);
                        break;
                }

                switch (note.Effect)
                {
                    case ITEffect.None:
                        break;
                    case ITEffect.SetSpeed:
                        _speed = note.EffectParam;
                        break;
                    case ITEffect.PositionJump:
                        break;
                    case ITEffect.PatternBreak:
                        break;
                    case ITEffect.VolumeSlide:
                        break;
                    case ITEffect.PortamentoDown:
                        break;
                    case ITEffect.PortamentoUp:
                        break;
                    case ITEffect.TonePortamento:
                        break;
                    case ITEffect.Vibrato:
                        break;
                    case ITEffect.Tremor:
                        break;
                    case ITEffect.Arpeggio:
                        break;
                    case ITEffect.VolumeSlideVibrato:
                        break;
                    case ITEffect.VolumeSlideTonePortamento:
                        break;
                    case ITEffect.SetChannelVolume:
                        break;
                    case ITEffect.ChannelVolumeSlide:
                        break;
                    case ITEffect.SampleOffset:
                        if (note.Key != PianoKey.None)
                            _resampler.SetSampleOffset(c, (uint) (note.EffectParam * 256));
                        break;
                    case ITEffect.PanningSlide:
                        break;
                    case ITEffect.Retrigger:
                        break;
                    case ITEffect.Tremolo:
                        break;
                    case ITEffect.Special:
                        break;
                    case ITEffect.SetTemp:
                        break;
                    case ITEffect.FineVibrato:
                        break;
                    case ITEffect.GlobalVolumeSlide:
                        break;
                    case ITEffect.SetPanning:
                        break;
                    case ITEffect.Panbrello:
                        break;
                    case ITEffect.MidiMacro:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
                    if (_order >= _track.Orders.Length || _track.Orders[_order] == 255)
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
        if (key == PianoKey.NoteCut)
            return 0;

        int note = 40 + (int) (key - 3) + (int) (octave - 3) * 12;
        float powNote = MathF.Pow(2, (note - 49f) / 12f);
        return c5Rate * powNote * multiplier;
    }
}