using System;
using MTRACK.Audio;
using Pie.Audio;

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
    private int _order;
    private int _row;
    private int _speed;

    private int _currentBuffer;
    private int _channel;

    private short[] _buffer;
    private int _bufferPos;

    public TrackPlayer(AudioDevice device, Track track)
    {
        _resampler = new Resampler(32, 48000, true, track.Samples);
        //_resampler.SampleVolume = 48 / 255f;
        _audioDevice = device;
        
        _track = track;
        _audioBuffers = new AudioBuffer[2];
        for (int i = 0; i < _audioBuffers.Length; i++)
            _audioBuffers[i] = device.CreateBuffer();

        _speed = track.InitialSpeed;
        _samplesPerTick = CalculateSamplesPerTick(track.InitialTempo);

        _buffer = new short[_resampler.SampleRate];
    }

    public void Play()
    {
        for (int i = 0; i < _audioBuffers.Length; i++)
        {
            AdvanceBuffer();
            //_audioDevice.UpdateBuffer(_audioBuffers[i], AudioFormat.Stereo16, _resampler.SongBuffer,
            //    _resampler.SampleRate);
            _audioDevice.UpdateBuffer(_audioBuffers[i], AudioFormat.Stereo16, _buffer, _resampler.SampleRate);
        }

        _channel = _audioDevice.FindChannel();
        _audioDevice.Play(_channel, _audioBuffers[0]);
        _audioDevice.Queue(_channel, _audioBuffers[1]);
        _audioDevice.BufferFinished += QueueBuffers;
    }

    private void AdvanceBuffer()
    {
        //for (int i = 0; i < _resampler.BufferLengthInSamples; i++)
        //    Advance();
        for (int i = 0; i < 24000; i++)
            Advance();

        _bufferPos = 0;
        //_resampler.BufferPos = 0;
    }

    private void QueueBuffers(AudioDevice device, uint channel)
    {
        if (channel != _channel)
            return;
        
        AdvanceBuffer();

        //_audioDevice.UpdateBuffer(_audioBuffers[_currentBuffer], AudioFormat.Stereo16, _resampler.SongBuffer,
        //    _resampler.SampleRate);
        _audioDevice.UpdateBuffer(_audioBuffers[_currentBuffer], AudioFormat.Stereo16, _buffer, _resampler.SampleRate);
        device.Queue((int) channel, _audioBuffers[_currentBuffer]);
        _currentBuffer++;
        if (_currentBuffer >= _audioBuffers.Length)
            _currentBuffer = 0;
    }

    private void Advance()
    {
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
        _buffer[_bufferPos++] = advance[0];
        _buffer[_bufferPos++] = advance[1];

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
                        _order = 0;
                }
            }
        }
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

    public void Dispose()
    {
        for (int i = 0; i < _audioBuffers.Length; i++)
            _audioBuffers[i].Dispose();
    }
}