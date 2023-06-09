﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{

    //this represents the expandable tree of dates for activities
    [Serializable]
    public class CalendarNode : Extensible
    {
        public CalendarNode(Utils.DateTimeTree dateTimeTree) 
        {
//            DateFormat = dateFormat;
            DateTimeTree = dateTimeTree;
        }

        //public string DateFormat { get;  }

        //public const string FormatAsYear = "yyyy";
        //public const string FormatAsMonth = "MMM yyyy";
        //public const string FormatAsDay = "ddd dd MMM yyyy";

        public override Utils.DateTimeTree Id { get { return DateTimeTree; } } //Hack to see if tree works

        public Utils.DateTimeTree DateTimeTree { get; }

        //public string DisplayString { get { return string.Format(DateFormat, DateTime); } }

        private SortedDictionary<DateTime, Extensible> _children = new SortedDictionary<DateTime, Extensible>();

        public IReadOnlyDictionary<DateTime, Extensible> Children { get { return _children; } }

        public void AddChild(DateTime date, Extensible child)
        {
            _children.Add(date, child);
        }

        public bool HasChild(DateTime date)
        {
            return _children.ContainsKey(date);
        }


        public override void Recalculate(bool force)
        {

            base.Recalculate(force);

            Dictionary<string, float> floatAccumulators = new Dictionary<string, float>();
            Dictionary<string, string> stringAccumulators = new Dictionary<string, string>();
            //iterate bottom up, so we can summarise their summaries
            foreach (KeyValuePair<DateTime, Extensible> kvp in _children)
            {
                Extensible child = kvp.Value;
                child.Recalculate(force);
                foreach (string name in child.DataNames)
                {
                    Datum? datum = child.GetNamedDatum(name);
                    if (datum == null)
                        continue;
                    if (datum is TypedDatum<float>)  
                    {
                        TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                        if (!floatAccumulators.ContainsKey(name))
                            floatAccumulators.Add(name, 0);

                        floatAccumulators[name] += floatDatum.Data; //TODO: add averaging or addition, and by sport type
                    }
                    else if (datum is TypedDatum<string>)
                    {
                        TypedDatum<string> stringDatum = (TypedDatum<string>)datum;
                        if (!stringAccumulators.ContainsKey(name))
                            stringAccumulators.Add(name, stringDatum.Data);
                        else if (stringAccumulators[name] != stringDatum.Data)
                            stringAccumulators[name] = "mixed";
                    }
                    else if (datum is TypedDatum<DateTime>)
                    {
                        if (!stringAccumulators.ContainsKey(name))
                            stringAccumulators.Add(name, ""); //TODO: handle combining dates better in tree structure
                    }
                }
            }
            foreach (KeyValuePair<string, float> kvp in floatAccumulators)
            {
                string name = kvp.Key;
                float value = kvp.Value;
                TypedDatum<float> floatDatum = new TypedDatum<float>(name, false, value);
                AddOrReplaceDatum(floatDatum);
            }
            foreach (KeyValuePair<string, string> kvp in stringAccumulators)
            {
                string name = kvp.Key;
                string value = kvp.Value;
                TypedDatum<string> floatDatum = new TypedDatum<string>(name, false, value);
                AddOrReplaceDatum(floatDatum);
            }

        }


    }
}
