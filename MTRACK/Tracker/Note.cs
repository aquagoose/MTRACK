namespace MTRACK.Tracker;

public struct Note
{
    public PianoKey Key;
    public Octave Octave;
    public uint Sample;
    public float NormalizedVolume;

    public Note(PianoKey key, Octave octave, uint sample, float normalizedVolume)
    {
        Key = key;
        Octave = octave;
        Sample = sample;
        NormalizedVolume = normalizedVolume;
    }
}