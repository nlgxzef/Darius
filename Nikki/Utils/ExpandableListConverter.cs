using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Nikki.Utils
{
    public class ExpandableListConverter<T> : ExpandableObjectConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var list = value as IList<T>;
            var props = new List<PropertyDescriptor>();

            //foreach (var item in base.GetProperties(context, value, attributes))
            //{
            //    props.Add((PropertyDescriptor)item);
            //}

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    props.Add(new ListItemPropertyDescriptor((IList)list, i));
                }
            }

            return new PropertyDescriptorCollection(props.ToArray());
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                return $"Collection ({((IList<T>)value)?.Count ?? 0} items)";
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class ListItemPropertyDescriptor : PropertyDescriptor
    {
        private readonly IList _list;
        private readonly int _index;

        public ListItemPropertyDescriptor(IList list, int index)
            : base($"[{index}]", null)
        {
            _list = list;
            _index = index;
        }

        public override Type ComponentType => _list.GetType();
        public override bool IsReadOnly => false;
        public override Type PropertyType => _list[_index]?.GetType() ?? typeof(object);

        public override bool CanResetValue(object component) => false;
        public override object GetValue(object component) => _list[_index];
        public override void ResetValue(object component) { }
        public override void SetValue(object component, object value) => _list[_index] = value;
        public override bool ShouldSerializeValue(object component) => true;
    }

}