using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reindeer_Hunter.ValueConverters
{
    class RoundToCheckboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<CheckBox> boxes = new List<CheckBox>();

            if (value != null)
            {
                List<Tuple<long, bool>> rounds = (List<Tuple<long, bool>>)value;

                for (int i = 0; i < rounds.Count; i++)
                {
                    CheckBox box = new CheckBox()
                    {
                        Content = rounds[i].Item1.ToString(),
                        IsChecked = rounds[i].Item2
                    };
                    boxes.Add(box);
                }
            }
            return boxes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<CheckBox> checkboxes = (List<CheckBox>)value;
            List<Tuple<long, bool>> rounds = new List<Tuple<long, bool>>();

            foreach (CheckBox box in checkboxes)
            {
                rounds.Add(new Tuple<long, bool>(long.Parse(box.Content.ToString()), (bool)box.IsChecked));
            }

            return rounds;
        }
    }
}
