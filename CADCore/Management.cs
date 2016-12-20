using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CADCore.Serialization;
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
            TemporaryObjectsDictionary = new ConcurrentDictionary<int, CadObject>();
        }

        public ConcurrentDictionary<int, CadObject> TemporaryObjectsDictionary { get; private set; }

        public ConcurrentQueue<EventArgs> UserInputEvent { get; private set; }

        public CadDocument OwnerCadDocument { get; internal set; }

        //WORKPLANE

        [DefaultValue(0.0)]
        public double MouseX
        {
            private get { return _mouseX*CadManagementControl.Instance.CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Width; }
            set { _mouseX = value; }
        }

        [DefaultValue(0.0)]
        public double MouseY
        {
            private get { return _mouseY*CadManagementControl.Instance.CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Height; }
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

                KeyEventArgs args = e as KeyEventArgs;
                if (args != null && args.KeyData == Keys.Escape)
                    return false;

                if ((eventType & UserInputEventType.Keyboard) != 0 && e is KeyEventArgs)
                {
                    return true;
                }
                else if (e is MouseEventArgs)
                {
                    MouseEventArgs m = (MouseEventArgs) e;

                    MousePoint = (Point) new Vec3D(m.X, m.Y, 0);
                    if ((CadManagementControl.Instance.MouseLink & MouseLinkingType.Vertex) != 0)
                    {
                        for (int i = 0; i < OwnerCadDocument.AllVertices.Count - 1; i++)
                        {
                            Point p = OwnerCadDocument.AllVertices[i];
                            if ((p - new Vec3D(MouseX, MouseY, 0)).LengthSqr() >= LinkDistanceSquare) continue;
                            MousePoint = p;
                            break;
                        }
                    }
                    if ((CadManagementControl.Instance.MouseLink & MouseLinkingType.Intersection) != 0)
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
        event CadManagementControl.SystemTickDelegate TickEvent;
        void Start();
        void Stop();
    }

    public class CadManagementControl
    {
        private static CadManagementControl _instance;

        public static CadManagementControl Instance
        {
            get { return _instance; }
        }

        private CadManagementControl()
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

            timer.TickEvent += new CadManagementControl().ManagementTick;

            OperationsDictionary.Add("Create Line", typeof(CreateLine));
            OperationsDictionary.Add("Create Circle", typeof(CreateCircle));
        }

        private void OnBeforeSetActiveDocument(CadDocument newCadDocument)
        {
            if (CurrentCadDocument != null)
            {
                //EndOperation(CurrentCadDocument);

                CurrentCadDocument.ParentControl.RenderWorkPanel.MouseMove -= ManagementMouseMove;
                CurrentCadDocument.ParentControl.RenderWorkPanel.MouseDown -= ManagementMouseDown;
                CurrentCadDocument.ParentControl.RenderWorkPanel.MouseUp -= ManagementMouseUp;

                CurrentCadDocument.ParentControl.RenderWorkPanel.KeyDown -= ManagementKeyDown;
                CurrentCadDocument.ParentControl.RenderWorkPanel.KeyUp -= ManagementKeyUp;
                CurrentCadDocument.ParentControl.RenderWorkPanel.KeyPress -= ManagementKeyPress;

                CurrentCadDocument.ParentControl.RenderWorkPanel.Paint -= ManagementPaint;
            }

            if (newCadDocument != null)
            {
                newCadDocument.ParentControl.RenderWorkPanel.MouseMove += ManagementMouseMove;
                newCadDocument.ParentControl.RenderWorkPanel.MouseDown += ManagementMouseDown;
                newCadDocument.ParentControl.RenderWorkPanel.MouseUp += ManagementMouseUp;

                newCadDocument.ParentControl.RenderWorkPanel.KeyDown += ManagementKeyDown;
                newCadDocument.ParentControl.RenderWorkPanel.KeyUp += ManagementKeyUp;
                newCadDocument.ParentControl.RenderWorkPanel.KeyPress += ManagementKeyPress;

                newCadDocument.ParentControl.RenderWorkPanel.Paint += ManagementPaint;
            }
        }

        #region Operation Events

        private void ManagementPaint(object sender, PaintEventArgs e)
        {
            try
            {
                foreach (var o in CurrentCadDocument.ObjectsDictionary)
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
                //foreach (var o in CurrentCadDocument.TemporaryObjectsDictionary)
                //{
                //    o.Value.Draw(e.Graphics);
                //}

                if ((MouseLink & MouseLinkingType.Vertex) != 0)
                {
                    for (int i = 0; i < CurrentCadDocument.AllVertices.Count; i++)
                    {
                        Point p = CurrentCadDocument.AllVertices[i];
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
                operation.MouseX = e.X / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseUp(e);
                operation.UserInputEvent.Enqueue(e);
            }

            CurrentCadDocument.ParentControl.RenderWorkPanel.Focus();
        }

        private void ManagementMouseDown(object sender, MouseEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.MouseX = e.X / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseDown();
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementMouseMove(object sender, MouseEventArgs e)
        {
            foreach (var operation in ActiveOperations)
            {
                operation.MouseX = e.X / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Width;
                operation.MouseY = e.Y / (float)CurrentCadDocument.ParentControl.RenderWorkPanel.ClientSize.Height;
                operation.OnMouseMove();
                operation.UserInputEvent.Enqueue(e);
            }
        }

        private void ManagementTick(float tickdelta)
        {
            if (CurrentCadDocument != null)
                CurrentCadDocument.ParentControl.RenderWorkPanel.Refresh();

            /*return;
            for (int i = 0; i < ActiveOperations.Count; i++)
            {
                var operation = ActiveOperations[i];
                operation.OnTick();
                switch (operation.OperationStatus)
                {
                    case OperationReturnValue.EndOperation:
                        ActiveOperations.RemoveAt(i);
                        foreach (var o in new Dictionary<int, CadObject>(CurrentCadDocument.TemporaryObjectsDictionary))
                            CurrentCadDocument.PostCreateCADObject(o.Value);
                        EndOperation(operation);

                        break;
                    case OperationReturnValue.CancelOperation:
                        ActiveOperations.RemoveAt(i);
                        CurrentCadDocument.TemporaryObjectsDictionary.Clear();
                        break;
                }
            }*/
        }

        #endregion

        private readonly List<CadDocument> _documents = new List<CadDocument>();
        private readonly List<UserOperation> _activeOperations = new List<UserOperation>();
        private readonly List<CadObject> _oldObjects = new List<CadObject>();
        private CadDocument _currentCadDocument;
        private static readonly Dictionary<string, Type> operationsDictionary = new Dictionary<string, Type>();

        public static Dictionary<string, Type> OperationsDictionary
        {
            get { return operationsDictionary; }
        }

        public CadDocument CurrentCadDocument
        {
            get { return _currentCadDocument; }
            set
            {
                OnBeforeSetActiveDocument(value);
                _currentCadDocument = value;
            }
        }

        public List<UserOperation> ActiveOperations
        {
            get { return _activeOperations; }
        }

        public MouseLinkingType MouseLink { get; set; }
        public bool LinkVertices { get; set; }

        internal CadDocument CreateDocument(DocumentControl control)
        {
            CurrentCadDocument = new CadDocument {ParentControl = control};
            _documents.Add(CurrentCadDocument);
            return CurrentCadDocument;
        }

        public async void BeginOperation(string operationName)
        {
            if (CurrentCadDocument == null) return;

            _oldObjects.Clear();
            _oldObjects.AddRange(CurrentCadDocument.ObjectsDictionary.Values);
            CurrentCadDocument.ObjectsDictionary.StartAccessList();

            //call operation
            if (OperationsDictionary.ContainsKey(operationName))
            {
                var operation = (UserOperation) Activator.CreateInstance(OperationsDictionary[operationName]);
                operation.OwnerCadDocument = CurrentCadDocument;
                ActiveOperations.Add(operation);

                OperationReturnValue result = OperationReturnValue.CancelOperation;
                Task<OperationReturnValue> task = new Task<OperationReturnValue>(operation.Execute);

                try
                {
                    task.Start();
                    result = await task.ContinueWith(task1 => task1.Result, new CancellationToken());
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }

                if (result == OperationReturnValue.EndOperation)
                {
                    ActiveOperations.Remove(operation);
                    foreach (var o in new Dictionary<int, CadObject>(operation.TemporaryObjectsDictionary))
                    {
                        CurrentCadDocument.PostCreateCADObject(o.Value);
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

            var usedObjects = new List<CadObject>(operation.OwnerCadDocument.ObjectsDictionary.FinishAccessList());
            usedObjects.AddRange(operation.TemporaryObjectsDictionary.Values);
            TextBlock state = new TextBlock();
            foreach (var o in usedObjects)
            {
                var objBlock = state.AddChild(o.GetType().Name);
                o.Save(objBlock);
                bool existInNew = operation.OwnerCadDocument.ObjectsDictionary.ContainsValue(o);
                bool existInOld = _oldObjects.Contains(o);

                if (existInNew && existInOld)
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Existed.ToString("G"));
                else if (existInNew)
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Created.ToString("G"));
                else
                    objBlock.SetAttribute("Status", ElemenetLifeStatus.Deleted.ToString("G"));
            }

            operation.OwnerCadDocument.History.AddUndo(new HistoryControl.HistoryRecord(state, RecordType.OperationSuccededType));
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

        public HistoryControl(CadDocument ownerCad)
        {
            OwnerCadDocument = ownerCad;
        }
        
        [DefaultValue(0)]
        public int CurrentHistoryLevel { get; private set; }

        private readonly List<HistoryRecord> _records = new List<HistoryRecord>();

        public CadDocument OwnerCadDocument { get; private set; }

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
                        var o = OwnerCadDocument.CreateCADObject(
                            (ElementTypes) Enum.Parse(typeof (ElementTypes), child.GetAttribute("elementType")),
                            int.Parse(child.GetAttribute("uid")), null);
                        o.Load(child);
                        OwnerCadDocument.PostCreateCADObject(o);
                    }
                        break;
                    case ElemenetLifeStatus.Existed:
                        OwnerCadDocument.ObjectsDictionary[int.Parse(child.GetAttribute("uid"))].Load(
                            child);
                        break;
                    default:
                        OwnerCadDocument.DeleteCADObject(int.Parse(child.GetAttribute("uid")));
                        break;
                }
            }
            OwnerCadDocument.ParentControl.ElementProperties.Refresh();
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
                        var o = OwnerCadDocument.CreateCADObject(
                            (ElementTypes) Enum.Parse(typeof (ElementTypes), child.GetAttribute("elementType")),
                            int.Parse(child.GetAttribute("uid")), null);
                        o.Load(child);
                        OwnerCadDocument.PostCreateCADObject(o);
                    }
                        break;
                    case ElemenetLifeStatus.Existed:
                        OwnerCadDocument.ObjectsDictionary[int.Parse(child.GetAttribute("uid"))].Load(
                            child);
                        break;
                    default:
                        OwnerCadDocument.DeleteCADObject(int.Parse(child.GetAttribute("uid")));
                        break;
                }
            }

            CurrentHistoryLevel++;
            OwnerCadDocument.ParentControl.ElementProperties.Refresh();
        }

        internal void Destroy()
        {
            _records.Clear();
        }
    }
}
