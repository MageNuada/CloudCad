using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Engine.MathEx;

namespace CADCore.Serialization
{
    public interface ISerializable
    {
        void Save(TextBlock block);
        void Load(TextBlock block);
    }

    internal static class EntitySerialization
    {
        internal static FieldInfo[] GetFields(Type objectType)
        {
            var fields = new List<FieldInfo>();

            var type = objectType;
            while (type != null)
            {
                fields.AddRange(
                    type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                   BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }
            return fields.ToArray();
        }

        internal static FieldInfo FindField(string fieldName, Type objectType)
        {
            var type = objectType;
            while (type != null)
            {
                var field = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                    return field;
                type = type.BaseType;
            }
            return null;
        }

        internal static void SaveField(FieldInfo info, object instance, TextBlock block)
        {
            if (info == null || block == null) return;
            object[] attr = info.GetCustomAttributes(typeof(FieldSerializeAttribute), false);
            if (attr.Length <= 0 ||
                (!info.FieldType.IsPrimitive && info.FieldType != typeof(decimal) && info.FieldType != typeof(string)))
                return;
            var value = info.GetValue(instance);
            if (value == null) return;

            if (info.FieldType.IsPrimitive)
                block.SetAttribute(info.Name, value.ToString());
            else if (value is ISerializable)
                (value as ISerializable).Save(block.AddChild(info.Name));
            else
            {
                var subblock = block.AddChild(info.Name);
                if (value is IEnumerable)
                {
                    var enumerator = (value as IEnumerable).GetEnumerator();
                    while (enumerator.MoveNext() && enumerator.Current != null)
                    {
                        TextBlock listelement = subblock.AddChild("element");
                        FieldInfo[] fields = GetFields(enumerator.Current.GetType());
                        for (var i = 0; i < fields.Length; i++)
                        {
                            SaveField(fields[i], enumerator.Current, listelement);
                        }
                    }
                }
                else if (value is Vec3D)
                {
                    subblock.SetAttribute("x", ((Vec3D)value).X.ToString());
                    subblock.SetAttribute("y", ((Vec3D)value).Y.ToString());
                    subblock.SetAttribute("z", ((Vec3D)value).Z.ToString());
                }
                else if (value is QuatD)
                {
                    subblock.SetAttribute("x", ((QuatD)value).X.ToString());
                    subblock.SetAttribute("y", ((QuatD)value).Y.ToString());
                    subblock.SetAttribute("z", ((QuatD)value).Z.ToString());
                    subblock.SetAttribute("w", ((QuatD)value).W.ToString());
                }
                else
                {
                    FieldInfo[] fields = GetFields(value.GetType());
                    for (var i = 0; i < fields.Length; i++)
                        SaveField(fields[i], value, subblock);
                }
            }
        }

        internal static void LoadBlock(object instance, TextBlock block)
        {
            foreach (TextBlock.Attribute a in block.Attributes)
            {
                var field = FindField(a.Name, instance.GetType());
                if (field == null) continue;
                object nv = Convert.ChangeType(a.Value, field.FieldType);
                field.SetValue(instance, nv);
            }

            foreach (var child in block.Children)
            {
                var field = FindField(child.Name, instance.GetType());
                if (field == null) continue;

                if (field.FieldType == typeof(Vec3D))
                {
                    field.SetValue(instance, new Vec3D
                    {
                        X = double.Parse(child.GetAttribute("x")),
                        Y = double.Parse(child.GetAttribute("y")),
                        Z = double.Parse(child.GetAttribute("z"))
                    });
                }
                else if (field.FieldType == typeof(QuatD))
                {
                    field.SetValue(instance, new QuatD
                    {
                        X = double.Parse(child.GetAttribute("x")),
                        Y = double.Parse(child.GetAttribute("y")),
                        Z = double.Parse(child.GetAttribute("z")),
                        W = double.Parse(child.GetAttribute("w"))
                    });
                }
                else if (field.FieldType.GetInterface(typeof(ISerializable).Name) != null)
                {
                    var o = (ISerializable)Activator.CreateInstance(field.FieldType);
                    o.Load(child);
                    field.SetValue(instance, o);
                }
                else if (field.FieldType.GetInterface(typeof(IList).Name) != null)
                {
                    var o = (IList)Activator.CreateInstance(field.FieldType);
                    var listtype = o.GetType().GetGenericArguments().Single();

                    foreach (var subChild in child.Children)
                    {
                        var subChildInstance = Activator.CreateInstance(listtype);
                        LoadBlock(subChildInstance, subChild);
                        o.Add(subChildInstance);
                    }
                    field.SetValue(instance, o);
                }
            }
        }

    }

}
