using System;
using Cubic.Audio;
using MTRACK.Audio;

namespace MTRACK.Tracker;

public class TrackPlayer : IDisposable
{
    private AudioBuffer[] _audioBuffers;
    private AudioDevice _audioDevice;
    
    private Resampler _resampler;
    private Track _track;

    private int _samplesPerTick;
    private int _currentSamples;
    private int _tick;
    private int _pattern;
    private int _row;

    public TrackPlayer(AudioDevice device, Track track)
    {
        _resampler = new Resampler(32, 48000, true, track.Samples);
        _audioDevice = device;
        
        _track = track;
        _audioBuffers = new AudioBuffer[2];
        for (int i = 0; i < _audioBuffers.Length; i++)
            _audioBuffers[i] = device.CreateBuffer();
    }

    public void Play()
    {
        
    }

    private void Advance()
    {
        if (_tick == 0)
        {
            ref Pattern pattern = ref _track.Patterns[_pattern];
            for (int c = 0; c < pattern.Channels; c++)
            {
                ref Note note = ref pattern.Notes[c, _row];
                if (note.Key != PianoKey.None)
                {
                    
                }
            }
        }
        
        _resampler.Advance();
    }

    private int CalculateSamplesPerTick(int tempo)
    {
        return (int) ((2.5f / tempo) * _resampler.SampleRate);
    }

    public void Dispose()
    {
        
    }
}