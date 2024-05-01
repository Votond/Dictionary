using System.Text;

namespace Dictionary;

public static class ListUtils
{
    public static List<T> AddAndReturn<T>(List<T> list, T el)
    {
        list.Add(el);
        return list;
    }

    public static string FormatTranslations(List<string> translations)
    {
        StringBuilder builder = new StringBuilder();

        foreach (var translation in translations)
            builder.Append($"- {translation}\n");

        return builder.ToString();
    }
}