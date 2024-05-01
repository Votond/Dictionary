using Newtonsoft.Json;

namespace Dictionary
{
    public class DictionarySerializerImp : IDictionarySerializer
    {
        public static DictionarySerializerImp Instance { get; private set; } = new DictionarySerializerImp();
        private DictionarySerializerImp() { }

        public void Serialize(List<Dictionary> dictionaries)
        {
            CreateFileIfNotExists();
            File.WriteAllText("dictionaries.json", JsonConvert.SerializeObject(dictionaries));
        }

        public List<Dictionary> Deserialize()
        {
            CreateFileIfNotExists();
            return JsonConvert.DeserializeObject<List<Dictionary>>(File.ReadAllText("dictionaries.json")) ?? [];
        }

        private void CreateFileIfNotExists()
        {
            if (!File.Exists("dictionaries.json"))
                File.Create("dictionaries.json");
        }
    }
}
