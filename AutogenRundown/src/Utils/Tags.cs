namespace AutogenRundown.Utils;

public class Tags
{
    private HashSet<string> tags = new();

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="initialTags">Params array of tags to initialize with</param>
    public Tags(params string[] initialTags)
    {
        foreach (var tag in initialTags)
            tags.Add(tag);
    }

    public bool Add(string tag) => tags.Add(tag);

    public bool Contains(string tag) => tags.Contains(tag);

    public override string ToString()
    {
        return "{" + string.Join(", ", tags.ToArray()) + "}";
    }
}
