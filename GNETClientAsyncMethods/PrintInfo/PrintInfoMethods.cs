using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GNETClientAsyncMethods
{


    public static class PrintInfoMethods
    {

        public const string errorMessage = "ñeee";


        public static PrintInfoResult PrintInfo(
       PrintInfoParams parametros,
       bool append = false)
        {
            if (parametros == null || parametros.ControlReceiver == null) { throw new ArgumentNullException("parametros"); }

            var tipo = parametros.ControlReceiver.GetType();
            var objProperty = tipo.GetProperty("Text");
            string text = parametros.Text;
            PrintInfoResult result;
            try
            {
                if (append)
                {
                    var valor = ((TextBox)parametros.ControlReceiver).Text;
                    text = valor + Environment.NewLine + parametros.Text;
                }
                objProperty.SetValue(parametros.ControlReceiver, text, null);
                result = new PrintInfoResult() { Result = text, OK = true };
            }
            catch (Exception ex)
            {
                result = new PrintInfoResult() { Result = text, OK = false, Ex = new ApplicationException(errorMessage, ex) };
            }
            return result;
        }


        public static void PrintInfo(
            PrintInfoParams parametros,
            out PrintInfoResult result,
            bool append = false)
        {
            if (parametros == null || parametros.ControlReceiver == null) { throw new ArgumentNullException("parametros"); }
            
            var tipo = parametros.ControlReceiver.GetType();
            var objProperty = tipo.GetProperty("Text");
            string text = parametros.Text;
            try
            {
                if (append)
                {
                    var valor = ((TextBox)parametros.ControlReceiver).Text;
                    text = valor + Environment.NewLine + parametros.Text;
                }
                objProperty.SetValue(parametros.ControlReceiver, text, null);
                result = new PrintInfoResult() { Result = text, OK = true };
            }
            catch(Exception ex) {
                result = new PrintInfoResult() { Result = text, OK = false, Ex = new ApplicationException(errorMessage,ex) };
            }
            
        }



        public static void PrintInfo(object parametros)
        {
            PrintInfo((PrintInfoParams)parametros);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlReceiber"></param>
        /// <param name="text"></param>
        /// <exception cref="System.ArgumentNullException">Thrown when controlReceiber is null</exception> 
        public static void PrintInfo(object controlReceiber, string text)
        {
            PrintInfo(new PrintInfoParams() { ControlReceiver = controlReceiber, Text = text });
        }

    }
}
