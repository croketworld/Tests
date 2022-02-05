using System.Runtime.Serialization;

namespace GNETClientAsyncMethods
{
    [DataContract]
    public class PrintInfoParams
    {
        [IgnoreDataMember]
        public object ControlReceiver { get; set; }

        private string GetControlName() {
            string result = "";
            if (ControlReceiver != null) {
                try
                {
                    result = (string)ControlReceiver.GetType().GetProperty("Name").GetValue(ControlReceiver,null);
                }
                catch { }
            }
            return result;
        }
        [DataMember]
        public string ControlName {
            get { return GetControlName(); }
            set { }
        }
        [DataMember]
        public string Text { get; set; }
    }
}
