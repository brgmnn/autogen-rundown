namespace AutogenRundown;

public class Collections
{
    public static List<T> Flatten<T>(params ICollection<T>[] collections)
        => collections.SelectMany(c => c).ToList();
}
