using System;
using System.Windows.Forms;

namespace CADCore
{
    public class CreateLine : UserOperation
    {
        private Line _line;

        public override OperationReturnValue Execute()
        {
            /*
            _line = (Line) OwnerDocument.CreateCADObject(ElementTypes.Line, this);
            _line.Start = GetUserInput(event.MouseUp).Position;
            _line.End = GetUserInput(event.MouseUp).Position;
            return OperationReturnValue.EndOperation;
             */

            _line = (Line) OwnerDocument.CreateCADObject(ElementTypes.Line, this);
            EventArgs m;
            do
            {
                if (!WaitInput(UserInputEventType.MouseMove | UserInputEventType.MouseClick, out m))
                    return OperationReturnValue.CancelOperation;
            } while (((MouseEventArgs) m).Button != MouseButtons.Left);

            SkipInput(5);

            _line.Start = new Point(MousePoint.Vertex);
            do
            {
                if (!WaitInput(UserInputEventType.MouseMove | UserInputEventType.MouseClick, out m))
                    return OperationReturnValue.CancelOperation;
                _line.End = new Point(MousePoint.Vertex);
            } while (((MouseEventArgs) m).Button != MouseButtons.Left);
            _line.End = new Point(MousePoint.Vertex);
            return OperationReturnValue.EndOperation;
        }
    }

    public class CreateCircle : UserOperation
    {
        private Circle _circle;

        /*public override void OnMouseUp(MouseEventArgs mouseEventArgs)
        {
            base.OnMouseUp(mouseEventArgs);

            if (mouseEventArgs.Button == MouseButtons.Left)
            {
                if (_circle == null)
                {
                    _circle = (Circle)OwnerDocument.CreateCADObject(ElementTypes.Circle);
                    _circle.Center = MousePoint;
                }
                else
                {
                    _circle.Radius = (_circle.Center - (Vec3D)MousePoint).Length();
                    OperationStatus = OperationReturnValue.EndOperation;
                }
            }
        }

        public override void OnTick()
        {
            base.OnTick();

            OperationStatus = OperationReturnValue.Continue;

            if (_circle != null)
            {
                _circle.Radius = (_circle.Center - (Vec3D)MousePoint).Length();
            }
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyData == Keys.Escape)
                OperationStatus = OperationReturnValue.CancelOperation;
        }*/
    }
}
