using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Utils
{
    [Serializable]
    public class DateTimeTree
    {
        public enum DateTreeType { Root, Year, Month, Day, Time };
        public DateTreeType Type { get; set;} = DateTreeType.Time;
        public DateTime DateTime { get; set;}
        
        public DateTimeTree(DateTime dateTime, DateTreeType type) 
        { 
            DateTime = dateTime;
            Type = type;
        }

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
            switch(Type)
            {
                case DateTreeType.Root:
                    return "Root";
                case DateTreeType.Year:
                    return DateTime.ToString(FormatAsYear);
                case DateTreeType.Month:
                    return DateTime.ToString(FormatAsMonth);
                case DateTreeType.Day:
                    return DateTime.ToString(FormatAsDay);
                case DateTreeType.Time:
                    return DateTime.ToString(FormatAsTime);
            }
            return DateTime.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (this.GetType() != obj.GetType())
            {
                return false;
            }

            DateTimeTree other = (DateTimeTree)obj;
            if(other.Type!= Type) 
            { 
                return false;  
            }
            
            //roots are always the same
            if(other.Type == DateTreeType.Root) 
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
