using MessagePack;

class Build
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine($"Require 1 argument, but got {args.Length}");
        }
        else if (args[0] == "dict")
        {
            BuildDict();
        }
        else if (args[0] == "index")
        {
            BuildKoIndex();
        }
        else
        {
            Console.WriteLine($"Invalid argument {args[0]}");
        }
    }

    static void BuildDict()
    {
        using (StreamReader r = new StreamReader("../KoDict/dict.json"))
        {
            string json = r.ReadToEnd();
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
            File.WriteAllBytes(path: "../KoDict/dict.msgpack.l4z", MessagePackSerializer.ConvertFromJson(json, lz4Options));
        }
    }

    static void BuildKoIndex()
    {
        var dict = new Dict();
        foreach (var entry in dict.Entries)
        {
            var jamos = Hangeul.Hangeuls2Jamos(entry.Word);
            ulong hash64 = Farmhash.Sharp.Farmhash.Hash64("Hello");
            // TODO
        }
    }
}
