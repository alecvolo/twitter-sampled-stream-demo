using StreamingDemo.Api.RankedHashtags.Projectors;

namespace StreamingDemo.Api.Infrastructure;

public class TagCountComparer : IEqualityComparer<TagCount>
{
    public bool Equals(TagCount? x, TagCount? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Count == y.Count;
    }

    public int GetHashCode(TagCount obj) => obj.Count.GetHashCode();
}