namespace Dictionary;

public static class ListUtils
{
    public static List<T> AddAndReturn<T>(List<T> list, T el)
    {
        list.Add(el);
        return list;
    }
}