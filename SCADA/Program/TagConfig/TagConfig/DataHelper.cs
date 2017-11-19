using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TagConfig
{
    public static class DataConvert //:MarshalByRefObject
    {
        public static DataTable ConvertTextToTable(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            DataTable mydt = new DataTable("");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mysr = new StreamReader(stream))
                {
                    string strline = mysr.ReadLine();
                    string[] aryline = strline.Split('\t');
                    for (int i = 0; i < aryline.Length; i++)
                    {
                        aryline[i] = aryline[i].Replace("\"", "");
                        mydt.Columns.Add(new DataColumn(aryline[i] + i));
                    }
                    int intColCount = aryline.Length;
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = strline.Split('\t');
                        DataRow mydr = mydt.NewRow();
                        for (int i = 0; i < intColCount; i++)
                        {
                            mydr[i] = aryline[i].Replace("\"", "");
                        }
                        mydt.Rows.Add(mydr);
                    }
                    return mydt;
                }
            }
        }

        public static DataTable ConvertCSVToTable(string str)
        {
            DataTable mydt = new DataTable("");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mysr = new StreamReader(stream))
                {
                    string strline = mysr.ReadLine();

                    Regex reg = new Regex(@",(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))");
                    string[] aryline = reg.Split(strline);
                    for (int i = 0; i < aryline.Length; i++)
                    {
                        aryline[i] = aryline[i].Replace("\"", "");
                        mydt.Columns.Add(new DataColumn(aryline[i]));
                    }
                    int intColCount = aryline.Length;
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = reg.Split(strline);

                        DataRow mydr = mydt.NewRow();
                        for (int i = 0; i < intColCount; i++)
                        {
                            mydr[i] = aryline[i].Replace("\"", "");
                        }
                        mydt.Rows.Add(mydr);
                    }
                    return mydt;
                }
            }
        }
    }
}
