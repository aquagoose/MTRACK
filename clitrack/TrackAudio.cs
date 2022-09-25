using System;
using libmtrack.Tracker;
using Pie.Audio;

namespace clitrack;

public class TrackAudio : IDisposable
{
    public const uint SampleRate = 48000;
    
    private AudioDevice _device;
    private AudioBuffer[] _buffers;
    private short[] _buffer;
    private uint _bufferPos;
    private int _currentBuffer;
    
    private TrackPlayer _player;

    public bool IsFinished => _player.IsFinished;

    public TrackAudio()
    {
        _device = new AudioDevice(1);
        _buffers = new AudioBuffer[2];

        for (int i = 0; i < _buffers.Length; i++)
        {
            _buffers[i] = _device.CreateBuffer();
        }

        _buffer = new short[SampleRate];
        
        _device.BufferFinished += DeviceOnBufferFinished;
    }

    public void Play(Track track)
    {
        _player = new TrackPlayer(track, false);
        
        for (int i = 0; i < _buffers.Length; i++)
            AdvanceBuffer(i);

        _currentBuffer = 0;
        
        _device.Play(0, _buffers[0]);
        for (int i = 1; i < _buffers.Length; i++)
            _device.Queue(0, _buffers[i]);
    }
    
    private void DeviceOnBufferFinished(AudioDevice device, uint channel)
    {
        AdvanceBuffer(_currentBuffer);
        _device.Queue(0, _buffers[_currentBuffer]);
        _currentBuffer++;
        if (_currentBuffer >= _buffers.Length)
            _currentBuffer = 0;
    }

    private void AdvanceBuffer(int index)
    {
        for (int i = 0; i < SampleRate / 2; i++)
        {
            short[] advance = _player.Advance();
            _buffer[_bufferPos++] = advance[0];
            _buffer[_bufferPos++] = advance[1];
        }

        _bufferPos = 0;
        
        _device.UpdateBuffer(_buffers[index], AudioFormat.Stereo16, _buffer, SampleRate);
    }

    public void Dispose()
    {
        foreach (AudioBuffer buffer in _buffers)
            buffer.Dispose();
        _device.Dispose();
    }
}