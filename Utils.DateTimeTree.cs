using MemoryPack;

namespace FellrnrTrainingAnalysis.Utils
{
    [MemoryPackable]
    [Serializable]

    //This is a class that represents a date as just the year, the year and month, or year + month + day. This allows for using instances in a tree structure representing the dates. 
    public partial class DateTimeTree
    {
        public enum DateTreeType { Root, Year, Month, Day, Time };

        [MemoryPackInclude]
        public DateTreeType Type { get; set; } = DateTreeType.Time;

        [MemoryPackInclude]
        public DateTime DateTime { get; set; }

        public DateTimeTree(DateTime dateTime, DateTreeType type)
        {
            DateTime = dateTime;
            Type = type;
        }

        [MemoryPackConstructor]
        public DateTimeTree()
        {
            DateTime = DateTime.Now;
            Type = DateTreeType.Root;
        }

        public const string FormatAsYear = "yyyy";
        public const string FormatAsMonth = "MMM yyyy";
        public const string FormatAsDay = "ddd dd MMM yyyy";
        public const string FormatAsTime = "ddd dd MMM yyyy H:mm";


        public override string ToString()
        {
            switch (Type)
            {
                case DateTreeType.Root:
                    return "DateTreeType.Root";
                case DateTreeType.Year:
                    return "DateTreeType.Year:" + DateTime.ToString(FormatAsYear);
                case DateTreeType.Month:
                    return "DateTreeType.Month:" + DateTime.ToString(FormatAsMonth);
                case DateTreeType.Day:
                    return "DateTreeType.Day:" + DateTime.ToString(FormatAsDay);
                case DateTreeType.Time:
                    return "DateTreeType.Time:" + DateTime.ToString(FormatAsTime);
            }
            return DateTime.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            DateTimeTree other = (DateTimeTree)obj;
            if (other.Type != Type)
            {
                return false;
            }

            //roots are always the same
            if (other.Type == DateTreeType.Root)
            {
                return true;
            }
            return this.DateTime == other.DateTime;
        }

        public override int GetHashCode()
        {
            if (Type == DateTreeType.Root)
                return this.Type.GetHashCode();

            return this.Type.GetHashCode() ^ this.DateTime.GetHashCode();
        }
    }
}
