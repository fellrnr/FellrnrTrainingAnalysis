using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Datum
    {
        public Datum(string name, bool recorded)
        {
            Name = name;
            Recorded = recorded;
        }
        public string Name { get; set; }

        public bool Recorded { get; set; }
    }

    [Serializable]
    public class TypedDatum<T> : Datum
    {
        public TypedDatum(string name, bool recorded, T data) : base(name, recorded)
        {
            Data = data;
        }
        /// Override ToString() for debugging
        public override string ToString()
        {
            return Data == null ? "null" : Data.ToString()!;
        }

        public T Data { get; set; }
    }

}