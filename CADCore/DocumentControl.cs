using System.Windows.Forms;
using Engine;

namespace CADCore
{
    public partial class DocumentControl : UserControl
    {
        public DocumentControl()
        {
            InitializeComponent();

            Load += DocumentControl_Load;

            if (ManagementControl.Instance != null)
            {
                UsedDocument = ManagementControl.Instance.CreateDocument(this);
                DocumentTree.Text = "untitled" + UsedDocument.DocumentID;
                DocumentTree.ExpandAll();
            }
            else
            {
                Log.Fatal("CAD-система не инициализирована!");
            }
            ElementProperties.PropertyValueChanged += ElementProperties_PropertyValueChanged;
        }

        void DocumentControl_Load(object sender, System.EventArgs e)
        {
        }

        public CADDocument UsedDocument { get; private set; }

        void ElementProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            var element = ElementProperties.SelectedObject as CADObject;
            if (element != null)
            {
                TextBlock block = new TextBlock();
                element.Save(block.AddChild(element.GetType().Name));
                UsedDocument.History.AddUndo(new HistoryControl.HistoryRecord(block, RecordType.PropertyChangedType));
            }
        }

        private void DocumentTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null && e.Node.Parent.Name == "2D")
            {
                ElementProperties.SelectedObject = e.Node.Tag;
                tabControl1.SelectTab("PropertiesPage");
            }
        }
    }
}
