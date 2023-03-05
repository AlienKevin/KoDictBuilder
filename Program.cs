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
        for (int i = 0; i < dict.Entries.Count; i++)
        {
            var entry = dict.Entries[i];
            var jamos = Hangeul.Hangeuls2Jamos(entry.Word);
            var wordHash = Farmhash.Sharp.Farmhash.Hash64(jamos);
            AddItem(d0Neighborhood, wordHash, i);
            // 1-deletion-neighborhood
            AddItem(d1Neighborhood, wordHash, i);
            for (int k = 0; k < jamos.Count(); k++)
            {
                var d1WordHash = Farmhash.Sharp.Farmhash.Hash64(jamos.Remove(k));
                AddItem(d1Neighborhood, d1WordHash, i);
            }
        }
        File.WriteAllBytes(path: "../KoDict/d0Neighborhood.msgpack", MessagePackSerializer.Serialize(d0Neighborhood));
        File.WriteAllBytes(path: "../KoDict/d1Neighborhood.msgpack", MessagePackSerializer.Serialize(d1Neighborhood));
    }
}
