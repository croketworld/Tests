namespace AsyncTasks
{
    public class EjecutarTareaRequest<T>
    {
        public T Tarea { get; set; }
        public object Arguments { get; set; }
        public int Id { get; set; }

        public EjecutarTareaRequest() { }
    }


}
