namespace AutogenRundown.DataBlockTypes
{
    internal interface IDataBlockType
    {
        string GetShortName();

        string GetFullName();

        void DoSaveToDisk(string fullPath);

        void AddJsonBlock(string json);

        void OnChanged();

        void RegisterOnChangeEvent(Action onChanged);
    }
}
