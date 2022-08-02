namespace StreamingDemo.Api.Infrastructure;

/// <summary>
/// Implementation of IComparer{T} based on another one;
/// this simply reverses the original comparison.
/// from https://stackoverflow.com/questions/931891/reverse-sorted-dictionary-in-net/931941#931941
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ReverseComparer<T> : IComparer<T>
{
    private readonly IComparer<T> _originalComparer;

    /// <summary>
    /// Creates a new reversing comparer.
    /// </summary>
    /// <param name="original">The original comparer to 
    /// use for comparisons.</param>
    public ReverseComparer(IComparer<T> original)
    {
        _originalComparer = original ?? throw new ArgumentNullException(nameof(original)); ;
    }

    /// <summary>
    /// Returns the result of comparing the specified
    /// values using the original
    /// comparer, but reversing the order of comparison.
    /// </summary>
    public int Compare(T? x, T? y) => _originalComparer.Compare(y, x);
}