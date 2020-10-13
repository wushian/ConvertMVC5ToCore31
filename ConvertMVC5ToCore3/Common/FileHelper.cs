using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertMVC5ToCore3.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 讀取文字檔案
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadTextFileToEnd(string fileName)
        {
            string result = string.Empty;

            FileStream f = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read);

            try
            {
                using (StreamReader sr = new StreamReader(f))
                {
                    result = sr.ReadToEnd();
                }
            }
            finally
            {
                f.Close();
            }
            return result;
        }
        /// <summary>
        /// 寫入至檔案
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="textContent"></param>
        public static void WriteTextToFile(string fileName, string textContent)
        {
            FileStream f = new FileStream(
                fileName,
                FileMode.Create,
                FileAccess.ReadWrite);

            try
            {
                using (StreamWriter sw = new StreamWriter(f))
                {
                    sw.Write(textContent);
                }
            }
            finally
            {
                f.Close();
            }
        }
    }
}
