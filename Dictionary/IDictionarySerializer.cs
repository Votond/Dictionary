namespace Dictionary
{
    public interface IDictionarySerializer
    {
        public void Serialize(List<Dictionary> dictionaries);
        public List<Dictionary> Deserialize();
    }
}
