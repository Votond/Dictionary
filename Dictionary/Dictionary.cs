namespace Dictionary
{
    public class Dictionary(string type)
    {
        public static List<Dictionary> Dictionaries = new();
        public string Type { get; private set; } = type;
        private readonly Dictionary<string, List<string>> _dictionary = new();

        public static Dictionary? GetDictionaryByType(string type) => Dictionaries.Find(dictionary => dictionary.Type == type);

        public List<string>? GetTranslation(string word)
        {
            try
            {
                _dictionary.First(pair => pair.Key == word);
                return _dictionary.First(pair => pair.Key == word).Value;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void AddWord(string word, string translation) => _dictionary.Add(word, [translation]);

        public void ChangeWord(string wordToReplace, string newWord)
        {
            var translations = _dictionary[wordToReplace];
            _dictionary.Remove(wordToReplace);

            _dictionary.Add(newWord, translations);
        }

        public void RemoveWord(string word) => _dictionary.Remove(word);

        public void AddTranslation(string translation, string word)
        {
            var translations = _dictionary[word];
            translations.Add(translation);

            _dictionary[word] = translations;
        }

        public void ChangeTranslation(string translationToChange, string word, string newTranslation)
        {
            var translations = _dictionary[word];
            translations[translations.FindIndex(el => el == translationToChange)] = newTranslation;
        }

        public bool RemoveTranslation(string translation, string word)
        {
            var translations = new List<string>(_dictionary[word]);
            translations.Remove(translation);

            if (translations.Count == 0)
            {
                return false;
            }
            else
            {
                _dictionary[word] = translations;
                return true;
            }
        }
    }
}