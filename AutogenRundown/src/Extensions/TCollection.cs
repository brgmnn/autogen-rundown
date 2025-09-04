namespace AutogenRundown.Extensions;

public static class TCollection
{
    /// <summary>
    /// Picks a random element from an enumerable/collection
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? PickRandom<T>(this IEnumerable<T> items) => Generator.Pick(items);

    /// <summary>
    /// Shuffles the collection of items
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ICollection<T> Shuffle<T>(this ICollection<T> items) => Generator.Shuffle(items);
}
