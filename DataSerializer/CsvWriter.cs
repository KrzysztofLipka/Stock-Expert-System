using System;
using System.Collections.Generic;
using System.Text;

namespace DataSerializer
{
    public interface IWritableAsRow {
        string ToCommaSeparatedRow();
    }
    public class CsvWriter
    {
        public static void AddRecord(string filepath, IWritableAsRow row) {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true)) {
                    file.WriteLine(row.ToCommaSeparatedRow());
                }
            }
            catch(Exception ex) {
                throw new ApplicationException("Error: ", ex);
            }
        }
    }
}
