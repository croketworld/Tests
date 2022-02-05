using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using AsyncTasks;
using GNETClientAsyncMethods;
using GNETClientAsyncTasks;
using System.Diagnostics;

namespace WindowsFormsApp1AsyncTest35
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Iniciar();
        }

        private readonly Stopwatch duracion = new Stopwatch();
        private void Iniciar() {
            GNETThreadPool2.OnTareaIniciada += Tarea_Iniciada;
            GNETThreadPool2.OnTareaFinalizada += Tarea_Finalizada;
        }
        private void Tarea_Iniciada(object sender, EjecutarTareaEventArgs e) { 
        
        }

        public delegate void TextboxInvokeDelegate();
        private void Tarea_Finalizada(object sender, EjecutarTareaResultEventArgs e)
        {
            textBox1.BeginInvoke(new TextboxInvokeDelegate(() => {
                textBox1.Text += "-------------------------------------------------" + Environment.NewLine +
                    $"{e.Resultado}{Environment.NewLine} Tarea({e.Tarea.Id}) {e.Tarea.Tarea.Method.Name} Ejecutada en: {duracion.Elapsed.TotalSeconds} segundos. {Environment.NewLine}";
            }));
            
            if (GNETThreadPool2.HilosEnCola <= 1) {
                duracion.Stop();
                this.toolStripStatusLabel1.Text = $"Todas las tareas asíncronas finalizadas en {duracion.Elapsed.TotalSeconds} segundos";
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Stopwatch duracion = Stopwatch.StartNew();
            duracion.Start();
            CallPrintSync("Ejecutando sincronicamente");
            int vueltas = 100;
            for (int i = 0; i < vueltas; i++)
            {
                string txt = $"paso {i}/{vueltas}";
                CallPrintSync(txt);
                Debug.WriteLine(txt);
                System.Threading.Thread.Sleep(100);
            }
            duracion.Stop();
            CallPrintSync(duracion.Elapsed.TotalSeconds.ToString());
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            CallPrintAsync2("Ejecutando asincronamente");
            int vueltas = 100;
            for (int i = 0; i < vueltas; i++)
            {
                string txt = $"paso {i}/{vueltas}";
                CallPrintAsync2(txt);//esto se escribe asincronamente
                Debug.WriteLine(txt);//esto se escribe al momento, incluso antes de que acabe la línea anterior
            }
        }


        private void Button3_Click(object sender, EventArgs e)
        {
            CallPrintAsync("Ejecutando asincronamente");
            int vueltas = 100;
            for (int i = 0; i < vueltas; i++)
            {
                string txt = $"paso {i}/{vueltas}";
                CallPrintAsync(txt);
                Debug.WriteLine(txt);
            }
        }

        #region "sync"

        private void CallPrintSync(string text) {
            PrintInfoMethods.PrintInfo(this.toolStripStatusLabel1, text);
        }

        private void CallPrintAsync(string text) {
            GNETThreadPool.EjecutarTareaAsincronicamente(
                PrintInfoMethods.PrintInfo,
                new PrintInfoParams() { ControlReceiver = this.toolStripStatusLabel1, Text = text });
        }

        private void CallPrintAsync2(string text, bool modoConsola = false)
        {
            
            var argumentos = modoConsola ?
                new PrintInfoParams() { ControlReceiver = this.textBox1,Text = text }
                : new PrintInfoParams(){ControlReceiver =this.toolStripStatusLabel1,Text = text};

            var argumentos2 = new PrintInfoParams() { ControlReceiver = this.button2, Text = text };
            var result = new PrintInfoResult();
            duracion.Start();
            GNETThreadPool2.EjecutarTareaAsincronicamente(
              () => PrintInfoMethods.PrintInfo(argumentos,out result,modoConsola),
              argumentos,
              (o) => {
                  var k = o.AsyncState.GetType().Name;                 
                  //ésta llamada fallaría, ya que se hace desde el subhilo
                 //ControlPropertiesMethods.PrintInfo(argumentos2);
              }
            );

        }





        #endregion

    }
}
