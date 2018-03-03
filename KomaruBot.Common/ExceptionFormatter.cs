using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomaruBot.Common
{
    public class ExceptionFormatter
    {
        public static string FormatException(Exception exc, string description)
        {
            return $"Description: {description}{System.Environment.NewLine} {FormatException(exc)}";
        }

        public static string FormatException(Exception exception)
        {
            string formattedException = null;
            Exception currentException = exception.InnerException;

            formattedException = String.Format("**EXCEPTION DETAILS:{0}{0}Exception Type: {5}{0}Message: {1}{0}Stack Trace:{0}{2}{0}Data:{0}{3}{0}Source: {4}{0}{0}", System.Environment.NewLine, exception.Message, exception.StackTrace, FormatExceptionData(exception.Data), exception.Source, exception.GetType().FullName);
            int ieCt = 1;
            while (currentException != null)
            {
                formattedException += String.Format("---INNER EXCEPTION #{5}---{0}{0}Exception Type: {6}{0}Message: {1}{0}Stack Trace:{0}{2}{0}Data:{0}{3}{0}Source: {4}{0}{0}", System.Environment.NewLine, currentException.Message, currentException.StackTrace, currentException.Data, currentException.Source, ieCt, exception.GetType().FullName);
                ieCt++;
                currentException = currentException.InnerException;
            }
            return formattedException;
        }

        private static string FormatExceptionData(System.Collections.IDictionary data)
        {
            if (data == null)
            {
                return System.Environment.NewLine;
            }

            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (var obj in data.Keys)
                {
                    sb.AppendFormat("[{0}]:{1}", obj, data[obj].ToString());
                    sb.Append(System.Environment.NewLine);
                }
            }
            catch
            {
                sb.AppendLine("--Exception caught while iterating exception data--");
            }

            return sb.ToString();
        }
    }
}
