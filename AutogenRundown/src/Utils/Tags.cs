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

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="previous"></param>
    public Tags(Tags previous)
    {
        tags = new HashSet<string>(previous.tags);
    }

    public bool Add(string tag) => tags.Add(tag);

    public bool Contains(string tag) => tags.Contains(tag);

    public Tags Extend(params string[] tags)
    {
        var newTags = new Tags(this);

        foreach (var tag in tags)
            newTags.Add(tag);

        return newTags;
    }

    public override string ToString()
    {
        return "{" + string.Join(", ", tags.ToArray()) + "}";
    }
}
