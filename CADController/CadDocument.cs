using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CADCore.Serialization;
using Engine;

namespace CADCore
{
    /// <summary>
    /// Словарь с записью объектов, к которым было обращение.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AccessDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly List<TValue> _accessedObjects = new List<TValue>();
        private bool _recordAccess;
        private readonly object _lockingObject = new object();

        public void StartAccessList()
        {
            lock (_lockingObject)
            {
                _accessedObjects.Clear();
                _recordAccess = true;
            }
        }

        public IEnumerable<TValue> FinishAccessList()
        {
            IEnumerable<TValue> res;
            lock (_lockingObject)
            {
                _recordAccess = false;
                res = _accessedObjects.Distinct();
            }
            return res;
        }

        public new TValue this[TKey key]
        {
            get
            {
                lock (_lockingObject)
                {
                    if (_recordAccess)
                    {
                        _accessedObjects.Add(base[key]);
                    }
                }
                return base[key];
            }
            set
            {
                lock (_lockingObject)
                {
                    base[key] = value;
                    if (_recordAccess)
                    {
                        _accessedObjects.Add(base[key]);
                    }
                }
            }
        }

        public new bool TryGetValue(TKey key, out TValue value)
        {
            bool result;
            lock (_lockingObject)
            {
                result = base.TryGetValue(key, out value);
                if (_recordAccess && result)
                {
                    _accessedObjects.Add(base[key]);
                }
            }
            return result;
        }

        public new void Add(TKey key, TValue value)
        {
            lock (_lockingObject)
            {
                base.Add(key, value);
                if (_recordAccess)
                {
                    _accessedObjects.Add(base[key]);
                }
            }
        }

        public new bool Remove(TKey key)
        {
            bool result;
            lock (_lockingObject)
            {
                result = base.Remove(key);
                if (_recordAccess && result)
                {
                    _accessedObjects.Add(base[key]);
                }
            }
            return result;
        }
    }

    public class CadDocument
    {
        public CadDocument()
        {
            DocumentID = ++_docIdCounter;
            _history = new HistoryControl(this);
        }

        private static int _docIdCounter;
        private int _elemIdCounter;

        /// <summary>
        /// ИД документа
        /// </summary>
        public int DocumentID { get; private set; }
        /// <summary>
        /// Контрол, с которым связан данный документ(на котором происходит отрисовка и изменение свойств объектов
        /// </summary>
        public DocumentControl ParentControl { get; internal set; }

        private readonly HistoryControl _history;

        [FieldSerialize] private readonly SynchronizedCollection<Point> _allVertices = new SynchronizedCollection<Point>();

        public SynchronizedCollection<Point> AllVertices
        {
            get { return _allVertices; }
        }

        /// <summary>
        /// Менеджер истории undo-redo документа
        /// </summary>
        public HistoryControl History
        {
            get { return _history; }
        }

        private readonly AccessDictionary<int, CadObject> _objectsDictionary = new AccessDictionary<int, CadObject>();
        private readonly ConcurrentDictionary<int, CadObject> _temporaryObjectsDictionary = new ConcurrentDictionary<int, CadObject>();

        /// <summary>
        /// Словарь всех объектов документа. Не изменять коллекцию!
        /// </summary>
        public AccessDictionary<int, CadObject> ObjectsDictionary
        {
            get { return _objectsDictionary; }
        }

        /// <summary>
        /// Функция создания нового объекта в документе
        /// </summary>
        /// <param name="elemType">Тип элемента из перечисления <see cref="ElementTypes"/></param>
        /// <returns>Возвращает созданный объект или null в случае неудачи</returns>
        public CadObject CreateCADObject(ElementTypes elemType, UserOperation operation)
        {
            return CreateCADObject(elemType, -1, operation);
        }
        
        /// <summary>
        /// Функция создания нового объекта в документе
        /// </summary>
        /// <param name="elemType">Тип элемента из перечисления <see cref="ElementTypes"/></param>
        /// <param name="id">ИД объекта, если загружается ранее созданный объект. Для выделения нового ИД надо ставить значение -1.</param>
        /// <returns>Возвращает созданный объект или null в случае неудачи</returns>
        internal CadObject CreateCADObject(ElementTypes elemType, int id, UserOperation operation)
        {
            CadObject o = null;
            switch (elemType)
            {
                case ElementTypes.Line:
                    o = new Line();
                    break;
                case ElementTypes.Arc:
                    o = new Arc();
                    break;
                case ElementTypes.Circle:
                    o = new Circle();
                    break;
                    //case ElementTypes.Point:
                    //    o = new Point();
                    //    break;
                default:
                    Log.Error("No objects!");
                    break;
            }
            if (o != null)
            {
                o.LockOperation = operation;
                o.ElementType = elemType;
                if (id == -1)
                    o.UID = ++_elemIdCounter;
                else
                    o.UID = id;
                //else
                //    TemporaryObjectsDictionary[o.UID] = o;
                o.Parent = this;
                if (operation != null)
                    operation.TemporaryObjectsDictionary[o.UID] = o;
                else
                    PostCreateCADObject(o);
            }
            return o;
        }

        public void PostCreateCADObject(CadObject obj)
        {
            if (obj.LockOperation != null)
            {
                if (obj.LockOperation.TemporaryObjectsDictionary.ContainsKey(obj.UID))
                {
                    while (obj != null && !obj.LockOperation.TemporaryObjectsDictionary.TryRemove(obj.UID, out obj)) ;
                }
            }
            //else
            //{
            //    if (TemporaryObjectsDictionary.ContainsKey(obj.UID))
            //    {
            //        while (!TemporaryObjectsDictionary.TryRemove(obj.UID, out obj)) ;
            //    }
            //}
            if (!ObjectsDictionary.ContainsKey(obj.UID))
            {
                obj.PostCreated = true;
                ParentControl.DocumentTree.Nodes.Find("2D", true)[0].Nodes.Add(new TreeNode(obj.ToString())
                {
                    Tag = obj,
                    Name = obj.ToString()
                });
            }
            ObjectsDictionary[obj.UID] = obj;
        }

        public void DeleteCADObject(int uid)
        {
            if (!ObjectsDictionary.ContainsKey(uid)) return;
            var o = ObjectsDictionary[uid];
            ObjectsDictionary.Remove(uid);
            ParentControl.DocumentTree.Nodes.Find("2D", true)[0].Nodes.RemoveByKey(o.ToString());
            if (ParentControl.ElementProperties.SelectedObject == o)
                ParentControl.ElementProperties.SelectedObject = null;
        }

        public TextBlock SaveDocument()
        {
            TextBlock file = new TextBlock();
            EntitySerialization.SaveField(EntitySerialization.FindField("allVertices", GetType()), this, file);
            foreach (CadObject o in ObjectsDictionary.Values)
            {
                o.Save(file.AddChild(o.GetType().Name));
            }
            return file;
        }

        public void LoadDocument(string filename)
        {
            string error;
            TextBlock file = TextBlock.Parse(File.ReadAllText(filename), out error);
            if (file == null)
            {
                Log.Error(error);
                return;
            }
            //LoadField(vertices)
            foreach (var child in file.Children)
            {
                var o =
                    CreateCADObject(
                        (ElementTypes) Enum.Parse(typeof (ElementTypes), child.GetAttribute("elementType")),
                        int.Parse(child.GetAttribute("uid")), null);
                o.Load(child);
                PostCreateCADObject(o);
            }
        }

        void Destroy()
        {
            ObjectsDictionary.Clear();
            ParentControl = null;
            History.Destroy();
        }
    }
}
