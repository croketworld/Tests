using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;



namespace AsyncTasks
{

    /// <summary>
    /// Ejecuta la tarea mediante ThreadPool.QueueUserWorkItem
    /// </summary>
    /// <remarks>
    /// PROS: Velocidad 
    /// CONTRAS: No se tiene control de las tareas ni de cuando finalizan
    /// </remarks>
    public static class GNETThreadPool
    {
        public static int MaximoNumeroHilosEnCola = 10;
        public static int IdUltimoHiloAñadido = 0;
        public static int HilosEnCola
        {
            get { return (TareasActuales != null) ? TareasActuales.Count : 0; }
        }

        public static List<EjecutarTareaRequest<WaitCallback>> TareasActuales { get; set; }

        private static int GenerarId() {
            int result = IdUltimoHiloAñadido + 1;
            if (result == int.MaxValue) { result = 1; }
            return result;
        }

        public static int EjecutarTareaAsincronicamente(WaitCallback tarea, object arguments)
        {
            if (TareasActuales == null) {
                TareasActuales = new List<EjecutarTareaRequest<WaitCallback>>();
                IdUltimoHiloAñadido = 0;
            }
            EjecutarTareaRequest<WaitCallback> tareaRequest = new EjecutarTareaRequest<WaitCallback>() { Arguments = arguments, Tarea = tarea, Id = GenerarId() };
            System.Threading.ThreadPool.QueueUserWorkItem(tarea, arguments);
            IdUltimoHiloAñadido = tareaRequest.Id;
            TareasActuales.Add(tareaRequest);
            return tareaRequest.Id;
        }


    }


    /// <summary>
    /// Ejecuta la tarea mediante Action.BeginInvoke
    /// </summary>
    /// <remarks>
    /// PROS: control total de tareas en ejecución, eventos (con posibilidad de añadir cancelación).
    /// También permite hacer invocaciones en cadena 
    /// (de momento sólo un único callback de finalización que permite usar los objetos resultantes de la 1ª tarea)
    /// Se puede ampliar y escalar
    /// CONTRAS: 
    /// </remarks>
    /// <![CDATA[
    /// TODO:
    /// 
    /// *cancelación de tareas
    /// -   Añadir propiedad de cancelación en evento OnTareaIniciada
    /// -   Añadir método cancelar(tareaid), cancelar(tarea), cancelar(todo)
    /// 
    /// *limitación de simultaneidad
    /// -   Aplicar límite de simultaneidad HilosEnCola >= MaximoNumeroHilosEnCola
    /// -   Añadir lista de pooleo para poner en cola cuando HilosEnCola >= MaximoNumeroHilosEnCola
    /// -
    /// 
    /// ]]>
    public static class GNETThreadPool2 {

        public static event EventHandler<EjecutarTareaEventArgs> OnTareaIniciada;
        public static event EventHandler<EjecutarTareaResultEventArgs> OnTareaFinalizada;


        public static int MaximoNumeroHilosEnCola = 10;
        public static int IdUltimoHiloAñadido = 0;
        public static int HilosEnCola
        {
            get { return (TareasActuales != null) ? TareasActuales.Count : 0; }
        }

        public static List<EjecutarTareaRequest<Action>> TareasActuales { get;set;}
        private static void AddTarea(EjecutarTareaRequest<Action> tareaRequest)
        {
            TareasActuales.Add(tareaRequest);
            OnTareaIniciada?.Invoke(null, new EjecutarTareaEventArgs() { Tarea = tareaRequest });
        }
        private static void RemoveTarea(EjecutarTareaRequest<Action> tareaRequest, object estado)
        {
            TareasActuales.Remove(tareaRequest);
            OnTareaFinalizada?.Invoke(null, new EjecutarTareaResultEventArgs() { Tarea = tareaRequest, Resultado = estado });
        }

        private static int GenerarId()
        {
            int result = ((TareasActuales != null) ?  TareasActuales.Count : 0) + 1;
            if (result == int.MaxValue) { result = 1; }
            return result;
        }

        public static IAsyncResult EjecutarTareaAsincronicamente(Action tarea, object arguments, AsyncCallback OnCompleted)
        {
            if (tarea == null) { throw new ArgumentNullException("tarea"); }
            Action<Action,object,AsyncCallback> EstaTarea = (Action _tarea, object _arguments, AsyncCallback _OnCompleted) => {
                if (TareasActuales == null)
                {
                    TareasActuales = new List<EjecutarTareaRequest<Action>>();
                    IdUltimoHiloAñadido = 0;
                }
                Stopwatch duracion = Stopwatch.StartNew();
                duracion.Start();
                EjecutarTareaRequest<Action> tareaRequest = new EjecutarTareaRequest<Action>() { Arguments = _arguments, Tarea = _tarea, Id = GenerarId() };
                AddTarea(tareaRequest);
                IAsyncResult resultado = _tarea.BeginInvoke(null, _arguments);
                do
                {
                    Thread.Sleep(100);
                }
                while (resultado.IsCompleted == false);
                RemoveTarea(tareaRequest, resultado.AsyncState);
              
                Interlocked.Exchange(ref IdUltimoHiloAñadido, tareaRequest.Id);//IdUltimoHiloAñadido = tareaRequest.Id;

                #region "log"
                duracion.Stop();
                System.Text.StringBuilder sb = new StringBuilder();

                sb.Append(
                    "######################################################################" +
                    $"{Environment.NewLine}Tarea({tareaRequest.Id}) {_tarea.Method.Name} Ejecutada en: {duracion.Elapsed.TotalSeconds} segundos. {Environment.NewLine}");
                DataContractSerializer dd = new DataContractSerializer(_arguments.GetType());
                try
                {
                    var ms = new System.IO.MemoryStream();
                    dd.WriteObject(ms, _arguments);
                    sb.Append(
                    $"Parámetros: {System.Text.Encoding.UTF8.GetString(ms.ToArray())}{Environment.NewLine}"  
                    + "######################################################################"
                    + Environment.NewLine
                    );
                    ms.Close();
                }
                catch { }
                Debug.Write(sb.ToString() + Environment.NewLine);

                #endregion

                _OnCompleted?.BeginInvoke(resultado, null, tareaRequest);
                GC.Collect();
            };

            return EstaTarea.BeginInvoke(tarea, arguments, OnCompleted, OnCompleted, arguments);
            
        }

        public static TIAsyncResult EjecutarTareaAsincronicamente<TIAsyncResult>(Action tarea, object arguments, AsyncCallback OnCompleted) where TIAsyncResult : IAsyncResult
        {
            if (tarea == null) { throw new ArgumentNullException("tarea"); }
            Action<Action, object, AsyncCallback> EstaTarea = (Action _tarea, object _arguments, AsyncCallback _OnCompleted) => {
                if (TareasActuales == null)
                {
                    TareasActuales = new List<EjecutarTareaRequest<Action>>();
                    IdUltimoHiloAñadido = 0;
                }
                Stopwatch duracion = Stopwatch.StartNew();
                duracion.Start();
                EjecutarTareaRequest<Action> tareaRequest = new EjecutarTareaRequest<Action>() { Arguments = _arguments, Tarea = _tarea, Id = GenerarId() };
                AddTarea(tareaRequest);
                TIAsyncResult resultado = (TIAsyncResult)_tarea.BeginInvoke(null, _arguments);
                do
                {
                    Thread.Sleep(100);
                }
                while (resultado.IsCompleted == false);
                RemoveTarea(tareaRequest, resultado.AsyncState);

                Interlocked.Exchange(ref IdUltimoHiloAñadido, tareaRequest.Id);//IdUltimoHiloAñadido = tareaRequest.Id;

                #region "log"
                duracion.Stop();
                System.Text.StringBuilder sb = new StringBuilder();

                sb.Append(
                    "######################################################################" +
                    $"{Environment.NewLine}Tarea({tareaRequest.Id}) {_tarea.Method.Name} Ejecutada en: {duracion.Elapsed.TotalSeconds} segundos. {Environment.NewLine}");
                DataContractSerializer dd = new DataContractSerializer(_arguments.GetType());
                try
                {
                    var ms = new System.IO.MemoryStream();
                    dd.WriteObject(ms, _arguments);
                    sb.Append(
                    $"Parámetros: {System.Text.Encoding.UTF8.GetString(ms.ToArray())}{Environment.NewLine}"
                    + "######################################################################"
                    + Environment.NewLine
                    );
                    ms.Close();
                }
                catch { }
                Debug.Write(sb.ToString() + Environment.NewLine);

                #endregion

                _OnCompleted?.BeginInvoke(resultado, null, tareaRequest);
                GC.Collect();
            };

            return (TIAsyncResult)EstaTarea.BeginInvoke(tarea, arguments, OnCompleted, OnCompleted, arguments);

        }


    }


}
