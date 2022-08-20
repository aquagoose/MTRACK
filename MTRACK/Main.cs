using System;
using Cubic.Audio;
using Cubic.Scenes;
using MTRACK.Audio;
using MTRACK.Tracker;
using Note = MTRACK.Tracker.Note;
using Octave = MTRACK.Tracker.Octave;
using Pattern = MTRACK.Tracker.Pattern;
using PianoKey = MTRACK.Tracker.PianoKey;
using Track = MTRACK.Tracker.Track;

namespace MTRACK;

public class Main : Scene
{
    protected override void Initialize()
    {
        base.Initialize();

        Sample sample = Sample.LoadFromWav("/home/ollie/Music/Samples/piano_middlec.wav");

        Pattern pattern = new Pattern(3, 16);
        pattern.SetNote(0, 0, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern.SetNote(1, 0, new Note(PianoKey.E, Octave.Octave4, 0, 1));
        pattern.SetNote(2, 0, new Note(PianoKey.G, Octave.Octave4, 0, 1));
        
        pattern.SetNote(0, 4, new Note(PianoKey.G, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 4, new Note(PianoKey.B, Octave.Octave3, 0, 1));
        pattern.SetNote(2, 4, new Note(PianoKey.D, Octave.Octave4, 0, 1));
        
        pattern.SetNote(0, 8, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 8, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern.SetNote(2, 8, new Note(PianoKey.E, Octave.Octave4, 0, 1));
        
        pattern.SetNote(0, 12, new Note(PianoKey.F, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 12, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern.SetNote(2, 12, new Note(PianoKey.C, Octave.Octave4, 0, 1));

        Track track = new Track(new[] { sample }, new[] { pattern }, new[] { 0u }, 125, 6);
        TrackPlayer player = new TrackPlayer(Game.AudioDevice, track);
        player.Play();
    }
}