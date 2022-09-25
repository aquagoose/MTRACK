namespace libmtrack.Tracker;

public struct Pattern
{
    public Note[,] Notes;
    public readonly int Channels;
    public readonly int Rows;

    public Pattern(int channels, int rows)
    {
        Channels = channels;
        Rows = rows;
        Notes = new Note[channels, rows];
    }

    public void SetNote(int channel, int row, Note note)
    {
        Notes[channel, row] = note;
    }
}