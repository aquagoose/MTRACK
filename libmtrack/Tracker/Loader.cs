using System.Text;
using StbVorbisSharp;

namespace libmtrack.Tracker;

public static class Loader
{
    public static Track LoadFile(string path)
    {
        using Stream stream = File.OpenRead(path);
        using BinaryReader reader = new BinaryReader(stream, Encoding.ASCII);

        if (CheckWav(reader))
            return LoadFromWav(reader);
        if (CheckOggVorbis(reader))
            return LoadFromVorbis(reader);
        if (CheckIT(reader))
            return LoadFromIT(reader);

        throw new Exception("Given file cannot be loaded by MTRACK.");
    }

    public static Track LoadFromWav(BinaryReader reader)
    {
        if (new string(reader.ReadChars(4)) != "RIFF")
        {
            throw new Exception(
                "Given file is missing the \"RIFF\" file header. This means it is either not a wave file, or is the wrong wave type (XWMs are currently not supported.)");
        }

        reader.ReadInt32(); // File size

        if (new string(reader.ReadChars(4)) != "WAVE")
        {
            throw new Exception(
                "The \"WAVE\" identifier was not found at its expected location. Currently, files with \"JUNK\" headers are not supported.");
        }

        if (new string(reader.ReadChars(4)) != "fmt ")
            throw new Exception("\"fmt \" identifier was not found at its expected location.");

        reader.ReadInt32(); // format data length

        if (reader.ReadInt16() != 1)
            throw new Exception("Currently, only PCM formats are supported. This file may be compressed?");

        short channels = reader.ReadInt16();
        uint sampleRate = (uint) reader.ReadInt32();

        // In the header, these look useful. But for now I am not sure where to use them, so ignore for now.
        reader.ReadInt32();
        reader.ReadInt16();

        short bitsPerSample = reader.ReadInt16();

        if (new string(reader.ReadChars(4)) != "data")
            throw new Exception("An error has occurred while reading the format data.");

        int dataSize = reader.ReadInt32();
        byte[] data = reader.ReadBytes(dataSize);

        Sample sample = new Sample(data, channels == 2, bitsPerSample == 16, sampleRate, false);
        uint songLength = (sample.DataLengthInSamples / sampleRate) + 1;
        Pattern pattern = new Pattern(1, (int) songLength);
        pattern.SetNote(0, 0, new Note(PianoKey.C, Octave.Octave4, 0, 1, ITEffect.None, 0));
        // 128 bpm, speed 48 is exactly 1 row per second.
        return new Track(new[] { sample }, new[] { pattern }, new[] { (byte) 0 }, 120, 48, 255);
    }

    public static Track LoadFromVorbis(BinaryReader reader)
    {
        Console.Write("Please wait, Converting Vorbis to PCM... ");
        byte[] data = reader.ReadBytes((int) reader.BaseStream.Length);
        using Vorbis vorbis = Vorbis.FromMemory(data);
        int size = (int) (vorbis.LengthInSeconds * vorbis.SampleRate * vorbis.Channels);
        byte[] vorbisData = new byte[(size + size % 4) * 2];
        int pos = 0;
        vorbis.SubmitBuffer();
        while (vorbis.Decoded != 0)
        {
            for (int i = 0; i < vorbis.Decoded * vorbis.Channels; i++)
            {
                vorbisData[pos++] = (byte) (vorbis.SongBuffer[i] & 255);
                vorbisData[pos++] = (byte) (vorbis.SongBuffer[i] >> 8);
            }
            vorbis.SubmitBuffer();
        }

        Console.WriteLine("Done.");
        
        Sample sample = new Sample(vorbisData, vorbis.Channels == 2, true, (uint) vorbis.SampleRate, false);
        uint songLength = (sample.DataLengthInSamples / sample.SampleRate) + 1;
        Pattern pattern = new Pattern(1, (int) songLength);
        pattern.SetNote(0, 0, new Note(PianoKey.C, Octave.Octave4, 0, 1, ITEffect.None, 0));
        // 128 bpm, speed 48 is exactly 1 row per second.
        return new Track(new[] { sample }, new[] { pattern }, new[] { (byte) 0 }, 120, 48, 255);
    }

    public static Track LoadFromIT(BinaryReader reader)
    {
        Console.WriteLine("Loading .IT file...");
        if (new string(reader.ReadChars(4)) != "IMPM")
            throw new Exception("Given file is not an impluse tracker file.");

        string songName = new string(reader.ReadChars(26));
        Console.WriteLine($"Loading \"{songName}\"...");
        reader.ReadBytes(2); // PHiligt

        ushort numOrders = reader.ReadUInt16();
        Console.WriteLine($"Orders: {numOrders}");
        ushort numInstruments = reader.ReadUInt16();
        Console.WriteLine($"Instruments: {numInstruments}");
        ushort numSamples = reader.ReadUInt16();
        Console.WriteLine($"Samples: {numSamples}");
        ushort numPatterns = reader.ReadUInt16();
        Console.WriteLine($"Patterns: {numPatterns}");
        reader.ReadBytes(4); // Cwt/v & Cmwt
        ushort flags = reader.ReadUInt16();
        //if ((flags & 0b0) != 1)
        //    throw new Exception("Mono mode is not yet supported.");
        if ((flags & 0b001) == 0)
            throw new Exception("Instruments are not yet supported.");

        if ((flags & 0b0001) == 0)
            throw new Exception("Only linear slides are currently supported.");

        reader.ReadUInt16(); // Special

        byte globalVolume = reader.ReadByte();
        byte mixVolume = reader.ReadByte();
        byte initialSpeed = reader.ReadByte();
        Console.WriteLine("Initial speed: " + initialSpeed);
        byte initialTempo = reader.ReadByte();
        Console.WriteLine("Initial tempo:" + initialTempo);
        byte separation = reader.ReadByte();
        reader.ReadBytes(11); // Message stuff & reserved

        const int maxChannels = 64;
        byte[] channelPans = reader.ReadBytes(maxChannels);
        byte[] channelVols = reader.ReadBytes(maxChannels);

        reader.BaseStream.Position = 0xC0;
        byte[] orders = reader.ReadBytes(numOrders);

        reader.BaseStream.Position = 0xC0 + numOrders + numInstruments;

        Sample[] samples = new Sample[numSamples];
        
        for (int i = 0; i < numSamples; i++)
        {
            reader.FindNextString("IMPS");
            string sDosFileName = new string(reader.ReadChars(12));
            reader.ReadByte(); // 00h (??? this looks like a padding byte?)
            byte sGlobalVolume = reader.ReadByte();
            byte sFlags = reader.ReadByte();
            if ((sFlags & 8) == 8)
                throw new Exception("Compressed samples cannot be loaded.");
            bool sSixteenBit = (sFlags & 2) == 2;
            bool sStereo = (sFlags & 4) == 4;
            
            byte sVolume = reader.ReadByte();
            string sName = new string(reader.ReadChars(26));
            Console.Write($"Loading Sample \"{sName} ({sDosFileName})\"... Flags " + Convert.ToString(sFlags, 2) + "... ");
            byte sConvert = reader.ReadByte();
            // MTRACK wants signed samples by default, we need to tell it if we are using unsigned.
            bool sUnsigned = (sConvert & 0) != 1;
            byte sDefaultPan = reader.ReadByte();
            uint sLength = reader.ReadUInt32();
            Console.Write(sLength + " samples... ");
            bool sLoop = (sFlags & 16) == 16;
            uint sLoopBegin = reader.ReadUInt32();
            uint sLoopEnd = reader.ReadUInt32();
            uint sSampleRate = reader.ReadUInt32();
            Console.Write(sSampleRate + "Hz...");
            uint sSusLoopBegin = reader.ReadUInt32();
            uint sSusLoopEnd = reader.ReadUInt32();
            uint sPtr = reader.ReadUInt32();
            reader.ReadBytes(4); // TODO vibrato stuff
            long currentPos = reader.BaseStream.Position;
            reader.BaseStream.Position = sPtr;
            byte[] sData = reader.ReadBytes((int) sLength * (sSixteenBit ? 2 : 1) * (sStereo ? 2 : 1));
            reader.BaseStream.Position = currentPos;

            Console.Write(sStereo ? "Stereo... " : "Mono... ");
            Console.WriteLine(sSixteenBit ? "16-bit... " : "8-bit... ");

            samples[i] = new Sample(sData, sStereo, sSixteenBit, sSampleRate, sLoop, sLoopBegin, (int) sLoopEnd, sUnsigned, true);
        }

        Pattern[] patterns = new Pattern[numPatterns];
        //Console.WriteLine(numPatterns);
        
        (byte maskVariable, byte lastNote, byte lastInstrument, byte lastVolume, byte lastCommand, byte lastCommandValue)[] prevVars = new (byte maskVariable, byte lastNote, byte lastInstrument, byte lastVolume, byte lastCommand, byte lastCommandValue)[64];
        
        for (int i = 0; i < numPatterns; i++)
        {
            ushort length = reader.ReadUInt16();
            ushort rows = reader.ReadUInt16();
            //Console.WriteLine(reader.BaseStream.Position);
            //Console.WriteLine(rows);
            //Console.WriteLine(i);

            patterns[i] = new Pattern(64, rows);

            reader.ReadBytes(4); // Padding bytes

            ushort row = 0;
            while (row < rows)
            {
                byte channelVariable = reader.ReadByte();
                if (channelVariable == 0)
                {
                    row++;
                    continue;
                }

                byte channel = (byte) ((channelVariable - 1) & 63);
                byte maskVariable = (channelVariable & 128) == 128 ? reader.ReadByte() : prevVars[channel].maskVariable;
                prevVars[channel].maskVariable = maskVariable;

                byte note = 253;
                byte instrument = 0;
                byte volume = 65;
                byte command = 0;
                byte commandInfo = 0;
                
                if ((maskVariable & 1) == 1)
                {
                    note = reader.ReadByte();
                    prevVars[channel].lastNote = note;
                    volume = 64;
                }
                
                if ((maskVariable & 2) == 2)
                {
                    instrument = reader.ReadByte();
                    prevVars[channel].lastInstrument = instrument;
                }
                
                if ((maskVariable & 4) == 4)
                {
                    volume = reader.ReadByte();
                    prevVars[channel].lastVolume = volume;
                }
                
                if ((maskVariable & 8) == 8)
                {
                    command = reader.ReadByte();
                    commandInfo = reader.ReadByte();

                    prevVars[channel].lastCommand = command;
                    prevVars[channel].lastCommandValue = commandInfo;
                }

                if ((maskVariable & 16) == 16)
                {
                    note = prevVars[channel].lastNote;
                    volume = (byte) (volume == 65 ? 64 : volume);
                }

                if ((maskVariable & 32) == 32)
                    instrument = prevVars[channel].lastInstrument;

                if ((maskVariable & 64) == 64)
                    volume = prevVars[channel].lastVolume;

                if ((maskVariable & 128) == 128)
                {
                    command = prevVars[channel].lastCommand;
                    commandInfo = prevVars[channel].lastCommandValue;
                }

                PianoKey key = PianoKey.None;
                Octave octave = Octave.Octave0;
                
                switch (note)
                {
                    case 255:
                        key = PianoKey.NoteOff;
                        octave = Octave.Octave0;
                        break;
                    case 254:
                        key = PianoKey.NoteCut;
                        octave = Octave.Octave0;
                        break;
                    case 253:
                        break;
                    default:
                        key = (PianoKey) (note % 12 + 3);
                        octave = (Octave) (note / 12);
                        break;
                }

                Console.WriteLine($"Pattern: {i}, NOTE: CH {channel}, Row {row}, Key {key}, Octave {octave}, Instrument {instrument}, Volume: {volume}, Effect: {(ITEffect) command}, Param: {commandInfo}");
                patterns[i].SetNote(channel, row, new Note(key, octave, (byte) (instrument - 1), volume, (ITEffect) command, commandInfo));
            }
        }

        return new Track(samples, patterns, orders, initialTempo, initialSpeed, 48);
    }

    public static long FindNextString(this BinaryReader reader, string @string)
    {
        long pos = reader.BaseStream.Position;
        while (new string(reader.ReadChars(@string.Length)) != @string)
        {
            pos++;
            reader.BaseStream.Position = pos;
        }

        return reader.BaseStream.Position;
    }

    private static bool CheckWav(BinaryReader reader)
    {
        reader.BaseStream.Position = 0;
        if (new string(reader.ReadChars(4)) == "RIFF")
        {
            reader.BaseStream.Position = 0;
            return true;
        }

        reader.BaseStream.Position = 0;
        return false;
    }
    
    private static bool CheckOggVorbis(BinaryReader reader)
    {
        reader.BaseStream.Position = 0;
        if (new string(reader.ReadChars(4)) != "OggS")
        {
            reader.BaseStream.Position = 0;
            return false;
        }

        reader.ReadBytes(25);
        if (new string(reader.ReadChars(6)) != "vorbis")
        {
            reader.BaseStream.Position = 0;
            return false;
        }

        reader.BaseStream.Position = 0;
        return true;
    }

    private static bool CheckIT(BinaryReader reader)
    {
        reader.BaseStream.Position = 0;
        if (new string(reader.ReadChars(4)) == "IMPM")
        {
            reader.BaseStream.Position = 0;
            return true;
        }

        reader.BaseStream.Position = 0;
        return false;
    }
}