namespace MTRACK.Tracker;

public class Track
{
    public Sample[] Samples;
    public Pattern[] Patterns;
    public uint[] Orders;

    public Track(int numSamples, int numPatterns, int numOrders)
    {
        Samples = new Sample[numSamples];
        Patterns = new Pattern[numPatterns];
        Orders = new uint[numOrders];
    }
}