namespace libmtrack.Tracker;

public struct Note
{
    public readonly bool Initialized;
    
    public PianoKey Key;
    public Octave Octave;
    public uint Sample;
    public int Volume;
    public float NormalizedVolume => Volume / 64f;
    public ITEffect Effect;
    public byte EffectParam;

    public Note(PianoKey key, Octave octave, uint sample, int volume, ITEffect effect, byte effectParam)
    {
        Key = key;
        Octave = octave;
        Sample = sample;
        Volume = volume;
        Effect = effect;
        EffectParam = effectParam;
        Initialized = true;
    }
}