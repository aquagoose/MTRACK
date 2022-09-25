namespace libmtrack.Tracker;

public class Track
{
    public Sample[] Samples;
    public Pattern[] Patterns;
    public uint[] Orders;

    public int InitialTempo;
    public int InitialSpeed;

    public byte InitialVolume;

    public Track(Sample[] samples, Pattern[] patterns, uint[] orders, int initialTempo, int initialSpeed, byte initialVolume)
    {
        Samples = samples;
        Patterns = patterns;
        Orders = orders;

        InitialTempo = initialTempo;
        InitialSpeed = initialSpeed;
        InitialVolume = initialVolume;
    }
}