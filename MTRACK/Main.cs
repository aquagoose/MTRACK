using System;
using Easel;
using Easel.Scenes;
using MTRACK.Audio;
using MTRACK.Tracker;
using Note = MTRACK.Tracker.Note;
using Octave = MTRACK.Tracker.Octave;
using Pattern = MTRACK.Tracker.Pattern;
using PianoKey = MTRACK.Tracker.PianoKey;
using Track = MTRACK.Tracker.Track;

namespace MTRACK;

public class Main : EaselGame
{
    protected override void Initialize()
    {
        base.Initialize();

        /*Sample sample = Sample.LoadFromWav("/home/ollie/Music/Samples/piano_middlec.wav");
        sample.Loop = true;
        Sample sample1 = Sample.LoadFromWav("/home/ollie/Music/Samples/synthbase.wav");

        Pattern pattern = new Pattern(4, 16);
        pattern.SetNote(0, 0, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern.SetNote(1, 0, new Note(PianoKey.E, Octave.Octave4, 0, 1));
        pattern.SetNote(2, 0, new Note(PianoKey.G, Octave.Octave4, 0, 1));
        pattern.SetNote(3, 0, new Note(PianoKey.C, Octave.Octave4, 1, 1));
        
        pattern.SetNote(0, 4, new Note(PianoKey.G, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 4, new Note(PianoKey.B, Octave.Octave3, 0, 1));
        pattern.SetNote(2, 4, new Note(PianoKey.D, Octave.Octave4, 0, 1));
        pattern.SetNote(3, 4, new Note(PianoKey.G, Octave.Octave3, 1, 1));
        
        pattern.SetNote(0, 8, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 8, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern.SetNote(2, 8, new Note(PianoKey.E, Octave.Octave4, 0, 1));
        pattern.SetNote(3, 8, new Note(PianoKey.A, Octave.Octave3, 1, 1));
        
        pattern.SetNote(0, 12, new Note(PianoKey.F, Octave.Octave3, 0, 1));
        pattern.SetNote(1, 12, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern.SetNote(2, 12, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern.SetNote(3, 12, new Note(PianoKey.F, Octave.Octave3, 1, 1));

        Pattern pattern1 = new Pattern(3, 16);
        pattern1.SetNote(0, 0, new Note(PianoKey.G, Octave.Octave3, 0, 1));
        pattern1.SetNote(1, 0, new Note(PianoKey.B, Octave.Octave3, 0, 1));
        pattern1.SetNote(2, 0, new Note(PianoKey.D, Octave.Octave4, 0, 1));
        
        pattern1.SetNote(0, 4, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern1.SetNote(1, 4, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern1.SetNote(2, 4, new Note(PianoKey.E, Octave.Octave4, 0, 1));
        
        pattern1.SetNote(0, 8, new Note(PianoKey.F, Octave.Octave3, 0, 1));
        pattern1.SetNote(1, 8, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern1.SetNote(2, 8, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        
        pattern1.SetNote(0, 12, new Note(PianoKey.A, Octave.Octave3, 0, 1));
        pattern1.SetNote(1, 12, new Note(PianoKey.C, Octave.Octave4, 0, 1));
        pattern1.SetNote(2, 12, new Note(PianoKey.E, Octave.Octave4, 0, 1));

        Track track = new Track(new[] { sample, sample1 }, new[] { pattern, pattern1 }, new[] { 0u, 0u, 1u, 1u }, 125, 6);
        TrackPlayer player = new TrackPlayer(Audio, track);
        player.Play();*/

        Sample sample = Sample.LoadFromWav("/home/ollie/Music/Laxity - A question of luck.wav");
        sample.Loop = true;
        Pattern pattern = new Pattern(2, 1);
        pattern.SetNote(0, 0, new Note(PianoKey.D, Octave.Octave4, 0, 1));
        Track track = new Track(new[] { sample }, new[] { pattern }, new[] { 0u }, 1, 9999999);
        TrackPlayer player = new TrackPlayer(Audio, track);
        player.Play();
    }

    public Main(GameSettings settings, Scene scene) : base(settings, scene) { }
}