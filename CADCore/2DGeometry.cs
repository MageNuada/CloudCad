using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Engine.MathEx;

namespace CADCore
{
    [TypeConverter(typeof(_GeneralTypeConverter<Point>))]
    public class Point: IOperationAccessor
    {
        [FieldSerialize] private Vec3D _vertex;
        [FieldSerialize] private readonly List<int> _parents = new List<int>();

        [DefaultValue(null)]
        public UserOperation LockOperation { get; set; }

        [Browsable(true)]
        public Vec3D Vertex
        {
            get { return _vertex; }
            set { _vertex = value; }
        }

        public List<int> Parents
        {
            get { return _parents; }
        }

        public Point()
        {

        }

        public Point(Vec3D v)
        {
            _vertex = v;
        }

        public static implicit operator Vec3D(Point p)
        {
            return p._vertex;
        }

        public static explicit operator Point(Vec3D v)
        {
            return new Point(v);
        }

        public double X
        {
            get { return _vertex.X; }
        }

        public double Y
        {
            get { return _vertex.Y; }
        }

        public double Z
        {
            get { return _vertex.Z; }
        }

        public void VertexChanged(VertexEvent vertexEvent)
        {
            foreach (var i in Parents)
            {
                ManagementControl.Instance.CurrentDocument.ObjectsDictionary[i].OnDataChange(vertexEvent);
            }
        }

        public static Point Parse(string text)
        {
            return new Point {Vertex = Vec3D.Parse(text)};
        }

        public override string ToString()
        {
            return Vertex.ToString();
        }
    }

    public class Line : CADObject
    {
        public Line()
        {
            VerticesList = new int[2];
        }

        //public Point Start1
        //{
        //    get { return Parent.AllVertices[VerticesList[0]]; }
        //    set { Parent.AllVertices[VerticesList[0]] = value; }
        //}

        //public Point End1
        //{
        //    get { return Parent.AllVertices[VerticesList[1]]; }
        //    set { Parent.AllVertices[VerticesList[1]] = value; }
        //}
        
        public Point Start
        {
            get { return Vertices[0]; }
            set {  Vertices[0] = value; }
        }

        public Point End
        {
            get { return Vertices[1]; }
            set { Vertices[1] = value; }
        }

        [Obsolete("For test only!")]
        public override void Draw(Graphics g)
        {
            base.Draw(g);

            Brush brush = new SolidBrush(Color.DarkGreen);

            g.DrawLine(new Pen(brush), (float)Start.X, (float)Start.Y, (float)End.X, (float)End.Y);
        }
    }

    public class Circle : CADObject
    {
        public Circle()
        {
            VerticesList = new int[1];
        }

        [FieldSerialize] private double _radius;

        [Browsable(true)]
        public Point Center
        {
            //get { return center; }
            //set { center = value; }
            get { return Vertices[0]; }
            set { Vertices[0] = value; }
        }

        [Browsable(true)]
        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        [Obsolete("For test only!")]
        public override void Draw(Graphics g)
        {
            base.Draw(g);

            Brush brush = new SolidBrush(Color.DarkGreen);

            g.DrawEllipse(new Pen(brush), (float) (Center.X - Radius), (float) (Center.Y - Radius),
                (float) (Radius*2), (float) (Radius*2));
        }
    }

    public class Arc : CADObject
    {
        [FieldSerialize] private Point _center;
        [FieldSerialize] private double _radius;
        [FieldSerialize] private double _angle1;
        [FieldSerialize] private double _angle2;

        [Browsable(true)]
        public Point Center
        {
            get { return _center; }
            set { _center = value; }
        }

        [Browsable(true)]
        public double Angle1
        {
            get { return _angle1; }
            set { _angle1 = value; }
        }

        [Browsable(true)]
        public double Angle2
        {
            get { return _angle2; }
            set { _angle2 = value; }
        }

        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        [Obsolete("For test only!")]
        public override void Draw(Graphics g)
        {
            base.Draw(g);

            g.DrawArc(new Pen(Brushes.DarkGreen), (float) (Center.X - Radius), (float) (Center.Y - Radius),
                (float) (Radius*2), (float) (Radius*2), (float) Angle1, (float) Angle2);
        }
    }
}
