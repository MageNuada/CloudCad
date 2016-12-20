using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using CADCore.Serialization;
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

    public interface IOperationAccessor
    {
        UserOperation LockOperation { get; set; }
    }

    public class VertexList : ICollection
    {
        private readonly List<int> _indices = new List<int>();
        private readonly CadDocument _cadDocument;
        private int _version;

        public VertexList()
        {
            _indices = new List<int>();
        }

        public VertexList(List<int> indices, CadDocument cadDocument)
        {
            _indices = indices;
            _cadDocument = cadDocument;
        }

        public Point this[int index]
        {
            get
            {
                while (_indices.Count <= index)
                    Add(new Point());
                return _cadDocument.AllVertices[_indices[index]];
            }
            set
            {
                while (_indices.Count <= index)
                    Add(value);
                _cadDocument.AllVertices[_indices[index]] = value;
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
            if (!_cadDocument.AllVertices.Contains(vertex))
                _cadDocument.AllVertices.Add(vertex);
            _indices.Add(_cadDocument.AllVertices.IndexOf(vertex));
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
                    _current = _parent._cadDocument.AllVertices[_list[_index]];
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

    public class CadObject : ISerializable, IOperationAccessor
    {
        [FieldSerialize] private int uid;
        [FieldSerialize] private Vec3D transformPosition;
        [FieldSerialize] private QuatD transformRotation;
        [FieldSerialize] private int elementType;
        [FieldSerialize] private readonly List<int> _vertexIndices = new List<int>();
        
        [DefaultValue(null)]
        public CadDocument Parent { get; internal set; }

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


        public void Save(TextBlock block)
        {
            if (block == null)
            {
                Log.Error("Empty save block!");
                return;
            }

            var fields = EntitySerialization.GetFields(GetType());
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo info = fields[i];
                EntitySerialization.SaveField(info, this, block);
            }
        }

        public void Load(TextBlock block)
        {
            if (block == null) return;

            EntitySerialization.LoadBlock(this, block);
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
