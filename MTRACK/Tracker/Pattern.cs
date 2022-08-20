namespace MTRACK.Tracker;

public struct Pattern
{
    public Note[,] Notes;
    public readonly int Channels;

    public Pattern(int channels, int rows)
    {
        Channels = channels;
        Notes = new Note[channels, rows];
    }

    public void SetNote(int channel, int row, Note note)
    {
        Notes[channel, row] = note;
    }
}