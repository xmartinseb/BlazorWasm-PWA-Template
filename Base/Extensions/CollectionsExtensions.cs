namespace Base.Extensions;

public static class CollectionsExtensions
{
    public static bool In<T>(this T value, params T[] candidates)
        => In(value, (IReadOnlyCollection<T>)candidates);

    public static bool NotIn<T>(this T value, params T[] candidates)
        => NotIn(value, (IReadOnlyCollection<T>)candidates);

    public static bool NotIn<T>(this T value, IReadOnlyCollection<T> candidates) 
        => !value.In(candidates);

    public static bool In<T>(this T value, IReadOnlyCollection<T> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        foreach (var candidate in candidates)
        {
            if (EqualityComparer<T>.Default.Equals(value, candidate))
                return true;
        }

        return false;
    }


    public static void ClearAddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        collection.Clear();
        foreach (var item in items)
            collection.Add(item);
    }
}