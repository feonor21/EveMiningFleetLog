using System;

namespace EveMiningFleet.Logic.Tools
{
    public static class ClassLog
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ex"></param>
        /// <param name="_folderlog"></param>
        public static void writeException(Exception _ex, string _folderlog = "log/")
        {
            string error = "";
            while (_ex != null)
            {
                error += _ex.GetType().FullName + "\n";
                error += "Message : " + _ex.Message + "\n";
                error += "StackTrace : " + _ex.StackTrace + "\n";
                _ex = _ex.InnerException;
            }

            writeLog(error, _folderlog);
        }


        static readonly object AppendAllTextLock = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public static void writeLog(string _text, string _folderlog = "log/")
        {
            Console.WriteLine(DateTime.Now.ToUniversalTime().ToString("HH mm ss ff") + "\t" + _text);
        }

    }
}