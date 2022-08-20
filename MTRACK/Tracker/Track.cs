using MTRACK.Audio;

namespace MTRACK.Tracker;

public class Track
{
    public Sample[] Samples;
    public Pattern[] Patterns;
    public uint[] Orders;

    public int InitialTempo;
    public int InitialSpeed;

    public Track(Sample[] samples, Pattern[] patterns, uint[] orders, int initialTempo, int initialSpeed)
    {
        Samples = samples;
        Patterns = patterns;
        Orders = orders;

        InitialTempo = initialTempo;
        InitialSpeed = initialSpeed;
    }
}