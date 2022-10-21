namespace libmtrack.Tracker;

public class Track
{
    public Sample[] Samples;
    public Pattern[] Patterns;
    public byte[] Orders;

    public int InitialTempo;
    public int InitialSpeed;

    public byte InitialVolume;

    public Track(Sample[] samples, Pattern[] patterns, byte[] orders, int initialTempo, int initialSpeed, byte initialVolume)
    {
        Samples = samples;
        Patterns = patterns;
        Orders = orders;

        InitialTempo = initialTempo;
        InitialSpeed = initialSpeed;
        InitialVolume = initialVolume;
    }
}