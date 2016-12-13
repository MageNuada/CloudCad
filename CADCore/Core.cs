using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Engine;
using Engine.MathEx;

namespace CADCore
{
    public sealed class FieldSerializeAttribute : Attribute
    {
        private readonly string _propertyName;

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public FieldSerializeAttribute()
        {
        }

        public FieldSerializeAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }
    }

    public interface ISerializedObject
    {
        void Save(TextBlock block);
        void Load(TextBlock block);
    }

    public interface IOperationAccessor
    {
        UserOperation LockOperation { get; set; }
    }

    public class VertexList : ICollection
    {
        private readonly List<int> _indices = new List<int>();
        private readonly CADDocument _document;
        private int _version;

        public VertexList()
        {
            _indices = new List<int>();
        }

        public VertexList(List<int> indices, CADDocument document)
        {
            _indices = indices;
            _document = document;
        }

        public Point this[int index]
        {
            get
            {
                while (_indices.Count <= index)
                    Add(new Point());
                return _document.AllVertices[_indices[index]];
            }
            set
            {
                while (_indices.Count <= index)
                    Add(value);
                _document.AllVertices[_indices[index]] = value;
            }
        }

        public int Count
        {
            get { return _indices.Count; }
        }

        [DefaultValue(false)]
        public bool IsSynchronized { get; set; }

        [DefaultValue(null)]
        public object SyncRoot { get; set; }

        public void CopyTo(Array array, int index)
        {
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator();
        }

        public void Add(Point vertex)
        {
            if (!_document.AllVertices.Contains(vertex))
                _document.AllVertices.Add(vertex);
            _indices.Add(_document.AllVertices.IndexOf(vertex));
            _version++;
        }

        public struct Enumerator : IEnumerator<Vec3D>
        {
            private List<int> _list;
            private int _index;
            private readonly int _version;
            private Vec3D _current;
            private readonly VertexList _parent;

            public Vec3D Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Count + 1)
                    {
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return Current;
                }
            }

            internal Enumerator(List<int> list, VertexList parent)
            {
                _list = list;
                _index = 0;
                _version = 0;
                _version = parent._version;
                _current = default(Vec3D);
                _parent = parent;
            }

            public void Dispose()
            {
                _list = null;
            }

            public bool MoveNext()
            {
                if ( _version == _parent._version && _index < _list.Count)
                {
                    _current = _parent._document.AllVertices[_list[_index]];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _parent._version)
                {
                    return false;
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                _index = _list.Count + 1;
                _current = default(Vec3D);
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _parent._version)
                {
                    return;
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                _index = 0;
                _current = default(Vec3D);
            }
        }
    }

    public class CADObject : ISerializedObject, IOperationAccessor
    {
        [FieldSerialize] private int uid;
        [FieldSerialize] private Vec3D transformPosition;
        [FieldSerialize] private QuatD transformRotation;
        [FieldSerialize] private int elementType;
        [FieldSerialize] private readonly List<int> _vertexIndices = new List<int>();
        
        [DefaultValue(null)]
        public CADDocument Parent { get; internal set; }

        public int[] VerticesList { get; set; }

        [DefaultValue(null)]
        public UserOperation LockOperation { get; set; }

        public VertexList Vertices
        {
            get { return new VertexList(_vertexIndices, Parent); }
        }

        public bool PostCreated { get; internal set; }

        private BoundsD _bounds;

        [Browsable(true), Localizable(true), DefaultValue(0), DisplayName("ИД объекта"), Description("Внутренний ИД объекта.")]
        public int UID
        {
            get { return uid; }
            internal set { uid = value; }
        }

        public ElementTypes ElementType
        {
            get { return (ElementTypes) elementType; }
            internal set { elementType = (int) value; }
        }

        #region serialization

        private static FieldInfo[] GetFields(Type objectType)
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

        public static FieldInfo FindField(string fieldName, Type objectType)
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

        public static void SaveField(FieldInfo info, object instance, TextBlock block)
        {
            if (info == null || block == null) return;
            var attr = info.GetCustomAttributes(typeof (FieldSerializeAttribute), false);
            if (attr.Length <= 0 ||
                (!info.FieldType.IsPrimitive && info.FieldType != typeof (decimal) && info.FieldType != typeof (string)))
                return;
            var value = info.GetValue(instance);
            if (value == null) return;

            if (info.FieldType.IsPrimitive || info.FieldType == typeof (Decimal) ||
                info.FieldType == typeof (String))
                block.SetAttribute(info.Name, value.ToString());
            else if (value is ISerializedObject)
                (value as ISerializedObject).Save(block.AddChild(info.Name));
            else
            {
                var subblock = block.AddChild(info.Name);
                if (value is IEnumerable)
                {
                    var enumerator = (value as IEnumerable).GetEnumerator();
                    while (enumerator.MoveNext() && enumerator.Current != null)
                    {
                        var listelement = subblock.AddChild("element");
                        var fields = GetFields(enumerator.Current.GetType());
                        for (int i = 0; i < fields.Length; i++)
                        {
                            FieldInfo subinfo = fields[i];
                            SaveField(subinfo, enumerator.Current, listelement);
                        }
                    }
                }
                else if (value is Vec3D)
                {
                    subblock.SetAttribute("x", ((Vec3D) value).X.ToString());
                    subblock.SetAttribute("y", ((Vec3D) value).Y.ToString());
                    subblock.SetAttribute("z", ((Vec3D) value).Z.ToString());
                }
                else if (value is QuatD)
                {
                    subblock.SetAttribute("x", ((QuatD) value).X.ToString());
                    subblock.SetAttribute("y", ((QuatD) value).Y.ToString());
                    subblock.SetAttribute("z", ((QuatD) value).Z.ToString());
                    subblock.SetAttribute("w", ((QuatD) value).W.ToString());
                }
                else
                {
                    var fields = GetFields(value.GetType());
                    for (int i = 0; i < fields.Length; i++)
                    {
                        FieldInfo subinfo = fields[i];
                        SaveField(subinfo, value, subblock);
                    }
                }
            }
        }

        public void Save(TextBlock block)
        {
            if (block == null)
            {
                Log.Error("Empty save block!");
                return;
            }

            var fields = GetFields(GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo info = fields[i];
                SaveField(info, this, block);
            }
        }

        protected void LoadBlock(object instance, TextBlock block)
        {
            foreach (TextBlock.Attribute a in block.Attributes)
            {
                var field = FindField(a.Name, instance.GetType());
                if (field == null) continue;
                var nv = Convert.ChangeType(a.Value, field.FieldType);
                field.SetValue(instance, nv);
            }

            foreach (var child in block.Children)
            {
                var field = FindField(child.Name, instance.GetType());
                if (field == null) continue;

                if (field.FieldType == typeof (Vec3D))
                {
                    field.SetValue(instance, new Vec3D
                    {
                        X = double.Parse(child.GetAttribute("x")),
                        Y = double.Parse(child.GetAttribute("y")),
                        Z = double.Parse(child.GetAttribute("z"))
                    });
                }
                else if (field.FieldType == typeof (QuatD))
                {
                    field.SetValue(instance, new QuatD
                    {
                        X = double.Parse(child.GetAttribute("x")),
                        Y = double.Parse(child.GetAttribute("y")),
                        Z = double.Parse(child.GetAttribute("z")),
                        W = double.Parse(child.GetAttribute("w"))
                    });
                }
                else if (field.FieldType.GetInterface(typeof (ISerializedObject).Name) != null)
                {
                    var o = (ISerializedObject) Activator.CreateInstance(field.FieldType);
                    o.Load(child);
                    field.SetValue(instance, o);
                }
                else if (field.FieldType.GetInterface(typeof (IList).Name) != null)
                {
                    var o = (IList) Activator.CreateInstance(field.FieldType);
                    var listtype = o.GetType().GetGenericArguments().Single();

                    foreach (var subchild in child.Children)
                    {
                        var so = Activator.CreateInstance(listtype);
                        LoadBlock(so, subchild);
                        o.Add(so);
                    }
                    field.SetValue(instance, o);
                }
            }
        }

        public void Load(TextBlock block)
        {
            if (block == null) return;

            LoadBlock(this, block);
        }

        #endregion

        [Obsolete("For test only!")]
        public virtual void Draw(Graphics g)
        {
        }

        public virtual void Draw()
        {
        }

        public virtual void CalculateBounds()
        {
        }

        public void OnDataChange(VertexEvent vertexEvent)
        {
            
        }

        /// <summary>
        /// Создание точки на кривой объекта.
        /// </summary>
        /// <param name="distance">Расстояние от начала объекта.</param>
        /// <returns></returns>
        public virtual Point CreatePointAtObject(double distance)
        {
            return null;
        }

        public override string ToString()
        {
            return ElementType.ToString("G") + ", " + UID;
        }
    }
}
