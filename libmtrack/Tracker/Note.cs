namespace libmtrack.Tracker;

public struct Note
{
    public PianoKey Key;
    public Octave Octave;
    public uint Sample;
    public float NormalizedVolume;
    public ITEffect Effect;
    public byte EffectParam;

    public Note(PianoKey key, Octave octave, uint sample, float normalizedVolume, ITEffect effect, byte effectParam)
    {
        Key = key;
        Octave = octave;
        Sample = sample;
        NormalizedVolume = normalizedVolume;
        Effect = effect;
        EffectParam = effectParam;
    }
}