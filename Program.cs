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
            BuildEnIndex();
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

    static void AddItem<K, V>(Dictionary<K, List<V>> dict, K key, V value)
    where K : notnull where V : notnull
    {
        List<V> items;
        if (dict.TryGetValue(key, out items))
        {
            items.Add(value);
        }
        else
        {
            dict.Add(key, new List<V> { value });
        }
    }

    static void BuildKoIndex()
    {
        var dict = new Dict();
        var d0Neighborhood = new Dictionary<ulong, List<int>>();
        var d1Neighborhood = new Dictionary<ulong, List<int>>();
        for (int entryIndex = 0; entryIndex < dict.Entries.Count; entryIndex++)
        {
            var entry = dict.Entries[entryIndex];
            var jamos = Hangeul.Hangeuls2Jamos(entry.Word);
            var wordHash = Farmhash.Sharp.Farmhash.Hash64(jamos);
            AddItem(d0Neighborhood, wordHash, entryIndex);
            // 1-deletion-neighborhood
            AddItem(d1Neighborhood, wordHash, entryIndex);
            for (int k = 0; k < jamos.Count(); k++)
            {
                var d1WordHash = Farmhash.Sharp.Farmhash.Hash64(jamos.Remove(k, 1));
                AddItem(d1Neighborhood, d1WordHash, entryIndex);
            }
        }
        File.WriteAllBytes(path: "../KoDict/d0Neighborhood.msgpack", MessagePackSerializer.Serialize(d0Neighborhood));
        File.WriteAllBytes(path: "../KoDict/d1Neighborhood.msgpack", MessagePackSerializer.Serialize(d1Neighborhood));
    }

    static void BuildEnIndex()
    {
        var dict = new Dict();
        var d0Neighborhood = new Dictionary<ulong, List<int>>();
        var d1Neighborhood = new Dictionary<ulong, List<int>>();
        for (int entryIndex = 0; entryIndex < dict.Entries.Count; entryIndex++)
        {
            var senses = dict.Entries[entryIndex].Senses.Select(sense => sense.EnglishLemma.ToLower());
            foreach (var (sense, senseIndex) in senses.Select((sense, senseIndex) => (sense, senseIndex)))
            {
                foreach (var word in sense.Split("; "))
                {
                    var index = Dict.PackEntryAndSenseIndex(entryIndex, senseIndex);
                    var wordHash = Farmhash.Sharp.Farmhash.Hash64(word);
                    AddItem(d0Neighborhood, wordHash, index);
                    // 1-deletion-neighborhood
                    AddItem(d1Neighborhood, wordHash, index);
                    if (word.Count() >= 3)
                    {
                        for (int k = 0; k < word.Count(); k++)
                        {
                            var d1WordHash = Farmhash.Sharp.Farmhash.Hash64(word.Remove(k, 1));
                            AddItem(d1Neighborhood, d1WordHash, index);
                        }
                    }
                }
            }
        }
        File.WriteAllBytes(path: "../KoDict/enD0Neighborhood.msgpack", MessagePackSerializer.Serialize(d0Neighborhood));
        File.WriteAllBytes(path: "../KoDict/enD1Neighborhood.msgpack", MessagePackSerializer.Serialize(d1Neighborhood));
    }
}
