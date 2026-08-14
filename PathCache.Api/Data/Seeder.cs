using System.Text.Json;

namespace PathCache.Api.Data;

public static class Seeder
{
    private static readonly string[] NodeNames =
    [
        "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel",
        "India", "Juliett", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa",
        "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "Xray",
        "Yankee", "Zulu", "Ares", "Boreas", "Ceres", "Dione", "Europa", "Freya",
        "Gaia", "Hyperion", "Io", "Janus", "Kronos", "Luna", "Maia", "Nyx",
    ];

    public static void Run(PathCacheDbContext db, int count)
    {
        if (db.Paths.Any())
            return;

        var random = new Random();
        var usedPairs = new HashSet<(string Source, string Target)>();
        var records = new List<PathRecord>();
        var maxPairs = NodeNames.Length * (NodeNames.Length - 1);

        while (records.Count < count && usedPairs.Count < maxPairs)
        {
            var source = NodeNames[random.Next(NodeNames.Length)];
            var target = NodeNames[random.Next(NodeNames.Length)];

            if (source == target)
                continue;

            if (!usedPairs.Add((source, target)))
                continue;

            var hops = random.Next(2, 7);
            var intermediates = NodeNames
                .Where(n => n != source && n != target)
                .OrderBy(_ => random.Next())
                .Take(hops - 1);

            var fullPath = new[] { source }.Concat(intermediates).Append(target);

            records.Add(new PathRecord
            {
                Source = source,
                Target = target,
                Hops = hops,
                PathJson = JsonSerializer.Serialize(fullPath),
                ComputedAt = DateTime.UtcNow,
            });
        }

        db.Paths.AddRange(records);
        db.SaveChanges();
    }
}
