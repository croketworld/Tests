using System;
using System.Runtime.Serialization;
using System.Threading;

namespace GNETClientAsyncMethods
{

    public class PrintInfoResult : PrintInfoResult<string, ApplicationException> { }

    [DataContract]
    public class PrintInfoResult<TResult,TException> : IAsyncResult
        where TException : Exception
    {
        [DataMember]
        public TResult Result { get; set; }
        [DataMember]
        public bool OK { get { return ok; } set {
                ok = value;
                completado = true;
            } }

        [DataMember]
        public string ExceptionMessage {
            get { return Ex == null ? "" : Ex.Message; }
            set { }
        }

        [IgnoreDataMember]
        public TException Ex { get; set; }
        
        private bool completado;
        private bool ok;
        public bool IsCompleted => completado;

        public WaitHandle AsyncWaitHandle => throw new NotImplementedException();

        public object AsyncState => Result;

        public bool CompletedSynchronously => throw new NotImplementedException();

        public PrintInfoResult() { 
        
        }
    }
}