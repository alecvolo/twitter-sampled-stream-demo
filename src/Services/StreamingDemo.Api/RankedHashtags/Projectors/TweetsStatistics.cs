using System.Text;

namespace StreamingDemo.Api.RankedHashtags.Projectors;

public record TweetsStatistics
{
    public ulong TweetsCount { get; init; }
    public TagCount[] TopHashtags { get; init; } = Array.Empty<TagCount>();
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(nameof(TweetsStatistics));
        builder.Append(" { ").Append(nameof(TweetsCount)).Append(" = ").Append(TweetsCount)
            .Append(' ').Append(nameof(TopHashtags)).Append(" = ");
        foreach (var tagCount in TopHashtags)
        {
            builder.Append(tagCount.Tag).Append('=').Append(tagCount.Tag).Append(' ');
        }
        builder.Append('}');
        return builder.ToString();
    }
}
public record TagCount(string Tag, ulong Count);
