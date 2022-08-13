using System;
using Cubic.Audio;
using Cubic.Scenes;
using MTRACK.Audio;
using MTRACK.Tracker;

namespace MTRACK;

public class Main : Scene
{
    private AudioBuffer[] _buffers;
    private Resampler _resampler;
    private int _currentBuffer;

    protected override void Initialize()
    {
        base.Initialize();

        _resampler =
            new Resampler(1, 48000, true, new Sample[] {Sample.LoadFromWav("")});
        _resampler.SetSampleRate(0, 44100, 0);
        _buffers = new AudioBuffer[2];
        for (int i = 0; i < _buffers.Length; i++)
        {
            AdvanceBuffer();
            _buffers[i] = Game.AudioDevice.CreateBuffer();
            Game.AudioDevice.UpdateBuffer(_buffers[i], AudioFormat.Stereo16, _resampler.SongBuffer, (int) _resampler.SampleRate);
        }

        Game.AudioDevice.PlayBuffer(0, _buffers[0]);
        for (int i = 1; i < _buffers.Length; i++)
            Game.AudioDevice.QueueBuffer(0, _buffers[i]);
        
        Game.AudioDevice.BufferFinished += AudioDeviceOnBufferFinished;
    }

    private void AudioDeviceOnBufferFinished(int channel)
    {
        AdvanceBuffer();
        Game.AudioDevice.UpdateBuffer(_buffers[_currentBuffer], AudioFormat.Stereo16, _resampler.SongBuffer, (int) _resampler.SampleRate);
        Game.AudioDevice.QueueBuffer(0, _buffers[_currentBuffer]);
        _currentBuffer++;
        if (_currentBuffer >= _buffers.Length)
            _currentBuffer = 0;
    }

    private void AdvanceBuffer()
    {
        for (int i = 0; i < _resampler.BufferLengthInSamples; i++)
            _resampler.Advance();
        _resampler.BufferPos = 0;
    }
}