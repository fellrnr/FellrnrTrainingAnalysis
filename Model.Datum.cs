using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(TypedDatum<string>))]
    [MemoryPackUnion(1, typeof(TypedDatum<float>))]
    [MemoryPackUnion(2, typeof(TypedDatum<DateTime>))]
    public abstract partial class Datum
    {
        public Datum(string name, bool recorded)
        {
            Name = name;
            Recorded = recorded;
        }
        [MemoryPackInclude]
        public string Name { get; set; }

        [MemoryPackInclude]
        public bool Recorded { get; set; }
    }

    [MemoryPackable]
    [Serializable]
    public partial class TypedDatum<T> : Datum
    {
        public TypedDatum(string name, bool recorded, T data) : base(name, recorded)
        {
            Data = data;
        }
        /// Override ToString() for display
        public override string ToString()
        {
            return Data == null ? "null" : Data.ToString()!;
        }

        [MemoryPackInclude]
        public T Data { get; set; }
    }

}