using System;

namespace AsyncTasks
{
    public class EjecutarTareaResultEventArgs<T> : EjecutarTareaEventArgs
    {
        public T Resultado { get; set; }
    }
    public class EjecutarTareaResultEventArgs : EjecutarTareaResultEventArgs<object>{}
    public class EjecutarTareaEventArgs : EventArgs
    {
        public EjecutarTareaRequest<Action> Tarea { get; set; }
    }
}