namespace Dictionary
{
    public class Dictionary(string type)
    {
        public static List<Dictionary> Dictionaries = new();
        public string Type { get; private set; } = type;
        public readonly Dictionary<string, List<string>> WordMap = new();

        public static Dictionary? GetDictionaryByType(string type) => Dictionaries.Find(dictionary => dictionary.Type == type);

        public List<string>? GetTranslation(string word)
        {
            try
            {
                WordMap.First(pair => pair.Key == word);
                return WordMap.First(pair => pair.Key == word).Value;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void AddWord(string word, string translation) => WordMap.Add(word, [translation]);

        public void ChangeWord(string wordToReplace, string newWord)
        {
            var translations = WordMap[wordToReplace];
            WordMap.Remove(wordToReplace);

            WordMap.Add(newWord, translations);
        }

        public void RemoveWord(string word) => WordMap.Remove(word);

        public void AddTranslation(string translation, string word)
        {
            var translations = WordMap[word];
            translations.Add(translation);

            WordMap[word] = translations;
        }

        public void ChangeTranslation(string translationToChange, string word, string newTranslation)
        {
            var translations = WordMap[word];
            translations[translations.FindIndex(el => el == translationToChange)] = newTranslation;
        }

        public bool RemoveTranslation(string translation, string word)
        {
            var translations = new List<string>(WordMap[word]);
            translations.Remove(translation);

            if (translations.Count == 0)
            {
                return false;
            }
            else
            {
                WordMap[word] = translations;
                return true;
            }
        }
    }
}