using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using Engine.MathEx;

namespace CADCore
{
    public abstract class UserOperation
    {
        protected Point MousePoint;
        private double _mouseY;
        private double _mouseX;
        private const double LinkDistance = 10;
        private const double LinkDistanceSquare = LinkDistance * LinkDistance;

        public UserOperation()
        {
            UserInputEvent = new ConcurrentQueue<EventArgs>();
            TemporaryObjectsDictionary = new ConcurrentDictionary<int, CADObject>();
        }

        public ConcurrentDictionary<int, CADObject> TemporaryObjectsDictionary { get; private set; }

        public ConcurrentQueue<EventArgs> UserInputEvent { get; private set; }

        public CADDocument OwnerDocument { get; internal set; }

        //WORKPLANE

        [DefaultValue(0.0)]
        public double MouseX
        {
            private get { return _mouseX*ManagementControl.Instance.CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Width; }
            set { _mouseX = value; }
        }

        [DefaultValue(0.0)]
        public double MouseY
        {
            private get { return _mouseY*ManagementControl.Instance.CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Height; }
            set { _mouseY = value; }
        }

        protected void SkipInput(int milliseconds = 1)
        {
            EventArgs e;
            while (milliseconds > 0)
            {
                WaitInput(UserInputEventType.None, out e);
                if (e != null)
                    milliseconds--;
            }
        }

        protected bool WaitInput(UserInputEventType eventType, out EventArgs e)
        {
            while (true)
            {
                Thread.Sleep(1);
                UserInputEvent.TryDequeue(out e);
                if (eventType == UserInputEventType.None) return true;

                if (e is KeyEventArgs && ((KeyEventArgs) e).KeyData == Keys.Escape)
                    return false;

                if ((eventType & UserInputEventType.Keyboard) != 0 && e is KeyEventArgs)
                {
                    return true;
                }
                else if (e is MouseEventArgs)
                {
                    MouseEventArgs m = (MouseEventArgs) e;

                    MousePoint = (Point) new Vec3D(m.X, m.Y, 0);
                    if ((ManagementControl.Instance.MouseLink & MouseLinkingType.Vertex) != 0)
                    {
                        for (int i = 0; i < OwnerDocument.AllVertices.Count - 1; i++)
                        {
                            Point p = OwnerDocument.AllVertices[i];
                            if ((p - new Vec3D(MouseX, MouseY, 0)).LengthSqr() >= LinkDistanceSquare) continue;
                            MousePoint = p;
                            break;
                        }
                    }
                    if ((ManagementControl.Instance.MouseLink & MouseLinkingType.Intersection) != 0)
                    {

                    }

                    if (m.Button == MouseButtons.None &&
                        (eventType & UserInputEventType.MouseMove) != 0)
                        return true;
                    else if (m.Button != MouseButtons.None &&
                             (eventType & UserInputEventType.MouseClick) != 0)
                        return true;
                }
            }
        }

        public virtual OperationReturnValue Execute()
        {
            return OperationReturnValue.CancelOperation;
        }

        public virtual void OnMouseMove()
        {
        }

        public virtual void OnMouseUp(MouseEventArgs mouseEventArgs)
        {
        }

        public virtual void OnMouseDown()
        {
        }

        public virtual void OnKeyPress(KeyPressEventArgs e)
        {
        }

        public virtual void OnKeyDown(KeyEventArgs e)
        {
        }

        public virtual void OnKeyUp(KeyEventArgs e)
        {
        }

        public virtual void OnTick()
        {
        }
    }

    public interface ITick
    {
        event ManagementControl.SystemTickDelegate TickEvent;
    }

    public class ManagementControl
    {
        private static ManagementControl _instance;

        public static ManagementControl Instance
        {
            get { return _instance; }
        }

        private ManagementControl()
        {
            _instance = this;
        }

        public void Destroy()
        {
            _instance = null;
        }

        public delegate void SystemTickDelegate(float tickdelta);

        /// <summary>
        /// Функция запуска ядра CAD-системы.
        /// </summary>
        public static void CreateCADManagement(ITick timer)
        {
            Log._Init(Thread.CurrentThread, "Log" + DateTime.Now.ToString().Replace('.', '_').Replace(':', '_') + ".txt");
            if (timer == null)
            {
                Log.Fatal("No timer!");
                return;
            }

            timer.TickEvent += new ManagementControl().ManagementTick;

            OperationsDictionary.Add("Create Line", typeof(CreateLine));
            OperationsDictionary.Add("Create Circle", typeof(CreateCircle));
        }

        private void OnBeforeSetActiveDocument(CADDocument newDocument)
        {
            if (CurrentDocument != null)
            {
                //EndOperation(CurrentDocument);

                CurrentDocument.ParentControl.RenderWorkPanel.MouseMove -= ManagementMouseMove;
                CurrentDocument.ParentControl.RenderWorkPanel.MouseDown -= ManagementMouseDown;
                CurrentDocument.ParentControl.RenderWorkPanel.MouseUp -= ManagementMouseUp;

                CurrentDocument.ParentControl.RenderWorkPanel.KeyDown -= ManagementKeyDown;
                CurrentDocument.ParentControl.RenderWorkPanel.KeyUp -= ManagementKeyUp;
                CurrentDocument.ParentControl.RenderWorkPanel.KeyPress -= ManagementKeyPress;

                CurrentDocument.ParentControl.RenderWorkPanel.Paint -= ManagementPaint;
            }

            if (newDocument != null)
            {
                newDocument.ParentControl.RenderWorkPanel.MouseMove += ManagementMouseMove;
                newDocument.ParentControl.RenderWorkPanel.MouseDown += ManagementMouseDown;
                newDocument.ParentControl.RenderWorkPanel.MouseUp += ManagementMouseUp;

                newDocument.ParentControl.RenderWorkPanel.KeyDown += ManagementKeyDown;
                newDocument.ParentControl.RenderWorkPanel.KeyUp += ManagementKeyUp;
                newDocument.ParentControl.RenderWorkPanel.KeyPress += ManagementKeyPress;

                newDocument.ParentControl.RenderWorkPanel.Paint += ManagementPaint;
            }
        }

        #region Operation Events

        private void ManagementPaint(object sender, PaintEventArgs e)
        {
            try
            {
                foreach (var o in CurrentDocument.ObjectsDictionary)
                {
                    o.Value.Draw(e.Graphics);
                }
                foreach (var ao in this.ActiveOperations)
                {
                    foreach (var o in ao.TemporaryObjectsDictionary)
                    {
                        o.Value.Draw(e.Graphics);
                    }
                }
                //foreach (var o in CurrentDocument.TemporaryObjectsDictionary)
                //{
                //    o.Value.Draw(e.Graphics);
                //}

                if ((MouseLink & MouseLinkingType.Vertex) != 0)
                {
                    for (int i = 0; i < CurrentDocument.AllVertices.Count; i++)
                    {
                        Point p = CurrentDocument.AllVertices[i];
                        e.Graphics.FillRectangle(Brushes.Red, (float) (p.X - 2), (float) (p.Y - 2), 4, 4);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);
            }
        }

        private void ManagementKeyUp(object sender, KeyEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.OnKeyUp(e);
                operation.UserInputEvent.Enqueue(e);
            }

            if (e.KeyCode == Keys.M)
            {
                if ((MouseLink & MouseLinkingType.Vertex) == 0)
                    MouseLink = MouseLink | MouseLinkingType.Vertex;
                else
                    MouseLink = MouseLink ^ MouseLinkingType.Vertex;
            }
        }

        private void ManagementKeyPress(object sender, KeyPressEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.OnKeyPress(e);
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.OnKeyDown(e);
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementMouseUp(object sender, MouseEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.MouseX = e.X / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseUp(e);
                operation.UserInputEvent.Enqueue(e);
            }

            CurrentDocument.ParentControl.RenderWorkPanel.Focus();
        }

        private void ManagementMouseDown(object sender, MouseEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.MouseX = e.X / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseDown();
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementMouseMove(object sender, MouseEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.MouseX = e.X / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseMove();
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementTick(float tickdelta)
        {
            if (CurrentDocument != null)
                CurrentDocument.ParentControl.RenderWorkPanel.Refresh();

            /*return;
            for (int i = 0; i < ActiveOperations.Count; i++)
            {
                var operation = ActiveOperations[i];
                operation.OnTick();
                switch (operation.OperationStatus)
                {
                    case OperationReturnValue.EndOperation:
                        ActiveOperations.RemoveAt(i);
                        foreach (var o in new Dictionary<int, CADObject>(CurrentDocument.TemporaryObjectsDictionary))
                            CurrentDocument.PostCreateCADObject(o.Value);
                        EndOperation(operation);

                        break;
                    case OperationReturnValue.CancelOperation:
                        ActiveOperations.RemoveAt(i);
                        CurrentDocument.TemporaryObjectsDictionary.Clear();
                        break;
                }
            }*/
        }

        #endregion

        private readonly List<CADDocument> _documents = new List<CADDocument>();
        private readonly List<UserOperation> _activeOperations = new List<UserOperation>();
        private readonly List<CADObject> _oldObjects = new List<CADObject>();
        private CADDocument _currentDocument;
        private static readonly Dictionary<string, Type> operationsDictionary = new Dictionary<string, Type>();

        public static Dictionary<string, Type> OperationsDictionary
        {
            get { return operationsDictionary; }
        }

        public CADDocument CurrentDocument
        {
            get { return _currentDocument; }
            set
            {
                OnBeforeSetActiveDocument(value);
                _currentDocument = value;
            }
        }

        public List<UserOperation> ActiveOperations
        {
            get { return _activeOperations; }
        }

        public MouseLinkingType MouseLink { get; set; }
        public bool LinkVertices { get; set; }

        internal CADDocument CreateDocument(DocumentControl control)
        {
            CurrentDocument = new CADDocument {ParentControl = control};
            _documents.Add(CurrentDocument);
            return CurrentDocument;
        }

        public async void BeginOperation(string operationName)
        {
            if (CurrentDocument == null) return;

            _oldObjects.Clear();
            _oldObjects.AddRange(CurrentDocument.ObjectsDictionary.Values);
            CurrentDocument.ObjectsDictionary.StartAccessList();

            //call operation
            if (OperationsDictionary.ContainsKey(operationName))
            {
                var operation = (UserOperation) Activator.CreateInstance(OperationsDictionary[operationName]);
                operation.OwnerDocument = CurrentDocument;
                ActiveOperations.Add(operation);

                OperationReturnValue result = OperationReturnValue.CancelOperation;
                Task<OperationReturnValue> task = new Task<OperationReturnValue>(operation.Execute);

                try
                {
                    task.Start();
                    result = await task;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                if (result == OperationReturnValue.EndOperation)
                {
                    ActiveOperations.Remove(operation);
                    foreach (var o in new Dictionary<int, CADObject>(operation.TemporaryObjectsDictionary))
                    {
                        CurrentDocument.PostCreateCADObject(o.Value);
                        o.Value.LockOperation = null;
                    }
                    EndOperation(operation);
                }
                else
                {
                    ActiveOperations.Remove(operation);
                    operation.TemporaryObjectsDictionary.Clear();
                }
            }
        }

        public void EndOperation(UserOperation operation)
        {
            //stop operation

            var usedObjects = new List<CADObject>(operation.OwnerDocument.ObjectsDictionary.FinishAccessList());
            usedObjects.AddRange(operation.TemporaryObjectsDictionary.Values);
            TextBlock state = new TextBlock();
            foreach (var o in usedObjects)
            {
                var objBlock = state.AddChild(o.GetType().Name);
                o.Save(objBlock);
                bool existInNew = operation.OwnerDocument.ObjectsDictionary.ContainsValue(o);
                bool existInOld = _oldObjects.Contains(o);

                if (existInNew && existInOld)
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Existed.ToString("G"));
                else if (existInNew)
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Created.ToString("G"));
                else
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Deleted.ToString("G"));
            }

            operation.OwnerDocument.History.AddUndo(new HistoryControl.HistoryRecord(state, RecordType.OperationSuccededType));
        }
    }

    public class HistoryControl
    {
        public class HistoryRecord
        {
            private HistoryRecord()
            {
            }

            public HistoryRecord(TextBlock currentstate, RecordType recType)
            {
                NewState = currentstate;
                RecType = recType;
            }

            public TextBlock NewState { get; private set; }
            public RecordType RecType { get; private set; }

            public override string ToString()
            {
                var result = RecType.ToString("G");
                if (NewState != null)
                {
                    result = NewState.Children.Aggregate(result,
                        (current, child) => current + (" " + child.Name + " " + child.GetAttribute("uid")));
                }
                return result;
            }
        }

        private HistoryControl()
        {
        }

        public HistoryControl(CADDocument owner)
        {
            OwnerDocument = owner;
        }
        
        [DefaultValue(0)]
        public int CurrentHistoryLevel { get; private set; }

        private readonly List<HistoryRecord> _records = new List<HistoryRecord>();

        public CADDocument OwnerDocument { get; private set; }

        /// <summary>
        /// Добавление записи в историю undo-операций.
        /// </summary>
        /// <param name="record"></param>
        public void AddUndo(HistoryRecord record)
        {
            if (CurrentHistoryLevel < _records.Count)
                _records.RemoveRange(CurrentHistoryLevel, _records.Count - CurrentHistoryLevel);
            _records.Add(record);
            CurrentHistoryLevel++;
        }

        /// <summary>
        /// Отмена последнего шага
        /// </summary>
        public void Undo()
        {
            if (CurrentHistoryLevel <= 0)
            {
                CurrentHistoryLevel = 0;
                return;
            }
            if (CurrentHistoryLevel > _records.Count)
                CurrentHistoryLevel = _records.Count;
            CurrentHistoryLevel--;

            foreach (var child in _records[CurrentHistoryLevel].NewState.Children)
            {
                ElemenetLifeStatus status = ElemenetLifeStatus.Existed;
                if (child.IsAttributeExist("Status"))
                    status = (ElemenetLifeStatus) Enum.Parse(typeof (ElemenetLifeStatus), child.GetAttribute("Status"));

                switch (status)
                {
                    case ElemenetLifeStatus.Deleted:
                    {
                        var o = OwnerDocument.CreateCADObject(
                            (ElementTypes) Enum.Parse(typeof (ElementTypes), child.GetAttribute("elementType")),
                            int.Parse(child.GetAttribute("uid")), null);
                        o.Load(child);
                        OwnerDocument.PostCreateCADObject(o);
                    }
                        break;
                    case ElemenetLifeStatus.Existed:
                        OwnerDocument.ObjectsDictionary[int.Parse(child.GetAttribute("uid"))].Load(
                            child);
                        break;
                    default:
                        OwnerDocument.DeleteCADObject(int.Parse(child.GetAttribute("uid")));
                        break;
                }
            }
            OwnerDocument.ParentControl.ElementProperties.Refresh();
        }

        /// <summary>
        /// Возвращение к последнему шагу
        /// </summary>
        public void Redo()
        {
            if (CurrentHistoryLevel < 0)
                CurrentHistoryLevel = 0;
            if (CurrentHistoryLevel >= _records.Count)
            {
                CurrentHistoryLevel = _records.Count - 1;
                return;
            }

            foreach (var child in _records[CurrentHistoryLevel].NewState.Children)
            {
                ElemenetLifeStatus status = ElemenetLifeStatus.Existed;
                if (child.IsAttributeExist("Status"))
                    status =
                        (ElemenetLifeStatus) Enum.Parse(typeof (ElemenetLifeStatus), child.GetAttribute("Status"));

                switch (status)
                {
                    case ElemenetLifeStatus.Created:
                    {
                        var o = OwnerDocument.CreateCADObject(
                            (ElementTypes) Enum.Parse(typeof (ElementTypes), child.GetAttribute("elementType")),
                            int.Parse(child.GetAttribute("uid")), null);
                        o.Load(child);
                        OwnerDocument.PostCreateCADObject(o);
                    }
                        break;
                    case ElemenetLifeStatus.Existed:
                        OwnerDocument.ObjectsDictionary[int.Parse(child.GetAttribute("uid"))].Load(
                            child);
                        break;
                    default:
                        OwnerDocument.DeleteCADObject(int.Parse(child.GetAttribute("uid")));
                        break;
                }
            }

            CurrentHistoryLevel++;
            OwnerDocument.ParentControl.ElementProperties.Refresh();
        }

        internal void Destroy()
        {
            _records.Clear();
        }
    }
}
