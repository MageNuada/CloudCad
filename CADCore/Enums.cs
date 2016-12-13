using System;

namespace CADCore
{
    /// <summary>
    /// Типы CAD-объектов
    /// </summary>
    public enum ElementTypes
    {
        [Obsolete("надо придумать замену.")]
        Point = 0,
        Line,
        Arc,
        Circle,
        Rectangle,

        Count,
    }

    internal enum ElemenetLifeStatus
    {
        Existed = 0,
        Created,
        Deleted,

        Count,
    }

    public enum RecordType
    {
        PropertyChangedType,
        OperationSuccededType,
    }

    /// <summary>
    /// Перечисление возвращаемого каждый тик состояния операции
    /// </summary>
    public enum OperationReturnValue
    {
        Continue = 0,
        EndOperation,
        CancelOperation,
    }

    public enum VertexEvent
    {
        Deleting = 0,
        Moving,
    }

    /// <summary>
    /// Типы вершин для притяжки курсора мыши
    /// </summary>
    [Flags]
    public enum MouseLinkingType
    {
        Vertex = 1,
        Intersection = 2,
        Edge = 4,
        Face = 8,
    }

    [Flags]
    public enum UserInputEventType
    {
        None = 0,

        Keyboard = 1,
        MouseMove = 2,
        MouseClick = 4,

    }
}
