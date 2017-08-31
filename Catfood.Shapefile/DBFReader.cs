using System;
using System.Collections.Generic;
using System.Collections.Specialized;
//using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// This class reads a dbf files
    /// </summary>
    public class DBFReader : IDisposable
    {
        private BinaryReader reader;
        private Encoding encoding;

        /// <summary>
        /// instantiate a Dbf reader based on a stream and encoding type
        /// </summary>
        /// <param name="stream">The record number in the Shapefile</param>
        /// <param name="encoding">Metadata about the shape</param>
        public DBFReader(Stream stream, Encoding encoding)
        {
            this.encoding = encoding;
            this.reader = new BinaryReader(stream, encoding);

            ReadHeader();
        }

        /// <summary>
        /// instantiate a Dbf reader based on a file and encoding type
        /// </summary>
        /// <param name="filename">The record number in the Shapefile</param>
        /// <param name="encoding">Metadata about the shape</param>
        public DBFReader(string filename, Encoding encoding)
        {
            if (File.Exists(filename) == false)
                throw new FileNotFoundException();

            this.encoding = encoding;
            var bs = new BufferedStream(File.OpenRead(filename));
            this.reader = new BinaryReader(bs, encoding);

            ReadHeader();
        }

        private void ReadHeader()
        {
            byte[] buffer = reader.ReadBytes(Marshal.SizeOf(typeof(DBFHeader)));

            // Marshall the header into a DBFHeader structure
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            this.header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
            handle.Free();

            fields = new List<DBFFieldDescriptor>();
            while (reader.PeekChar() != 13)
            {
                buffer = reader.ReadBytes(Marshal.SizeOf(typeof(DBFFieldDescriptor)));
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var fieldDescriptor = (DBFFieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFFieldDescriptor));
                if ((fieldDescriptor.Flags & DBFFieldFlags.System) != DBFFieldFlags.System )
                {
                    fields.Add(fieldDescriptor);
                }
                handle.Free();
            }

            byte headerTerminator = reader.ReadByte();
            byte[] backlink = reader.ReadBytes(263);
        }

        ///// <summary>
        ///// get the binary reader of DBFReader
        ///// </summary>
        //public BinaryReader Reader {
        //    get
        //    {
        //        return this.reader;
        //    }
        //}

        /// <summary>
        /// Reset the position of reader to the end of header
        /// </summary>
        public void ResetReaderToEndOfHeader()
        {
            reader.BaseStream.Seek(header.HeaderLenght, SeekOrigin.Begin);
        }

        /// <summary>
        /// instantiate a Dbf reader based on a file and encoding type
        /// </summary>
        public void ReadRecord(Dictionary<string, string> fieldRecord)
        {


            var row = reader.ReadBytes(header.RecordLenght);
            int fieldsLength = 0;
            int position = 0;
            //foreach (var field in fields)
            //{
            //    fieldsLength = fieldsLength + field.FieldLength;
            //}


            foreach (var field in fields)
            {
                byte[] buffer = new byte[field.FieldLength];
                //Array.Copy(row, field.Address, buffer, 0, field.FieldLength);
                Array.Copy(row, position+1, buffer, 0, field.FieldLength);
                position = position + field.FieldLength;
                string text = (encoding.GetString(buffer) ?? String.Empty).Trim();
                switch ((DBFFieldType)field.FieldType)
                {
                    case DBFFieldType.Character:
                        fieldRecord[field.FieldName] = text;
                        break;

                    case DBFFieldType.Currency:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "0.0m";
                            }
                        }
                        else
                        {
                            //fieldRecord[field.FieldName] = Convert.ToDecimal(text).ToString();
                            fieldRecord[field.FieldName] = text;
                        }
                        break;

                    case DBFFieldType.Numeric:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "0.0f";
                            }
                        }
                        else
                        {
                            //fieldRecord[field.FieldName] = Convert.ToSingle(text).ToString();
                            fieldRecord[field.FieldName] = text;
                        }
                        break;
                    case DBFFieldType.Float:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "0.0f";
                            }
                        }
                        else
                        {
                            //fieldRecord[field.FieldName] = Convert.ToSingle(text).ToString();
                            fieldRecord[field.FieldName] = text;
                        }
                        break;

                    case DBFFieldType.Date:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = DateTime.MinValue.ToString();
                            }
                        }
                        else
                        {
                            fieldRecord[field.FieldName] = DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture).ToString();
                        }
                        break;

                    case DBFFieldType.DateTime:
                        if (String.IsNullOrWhiteSpace(text) || BitConverter.ToInt64(buffer, 0) == 0)
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = DateTime.MinValue.ToString();
                            }
                        }
                        else
                        {
                            fieldRecord[field.FieldName] = JulianToDateTime(BitConverter.ToInt64(buffer, 0)).ToString();
                        }
                        break;

                    case DBFFieldType.Double:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "0.0";
                            }
                        }
                        else
                        {
                            //fieldRecord[field.FieldName] = Convert.ToDouble(text).ToString();
                            fieldRecord[field.FieldName] = text;
                        }
                        break;

                    case DBFFieldType.Integer:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "0";
                            }
                        }
                        else
                        {
                            fieldRecord[field.FieldName] = BitConverter.ToInt32(buffer, 0).ToString();
                        }
                        break;

                    case DBFFieldType.Logical:
                        if (String.IsNullOrWhiteSpace(text))
                        {
                            if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                            {
                                fieldRecord[field.FieldName] = null;
                            }
                            else
                            {
                                fieldRecord[field.FieldName] = "false";
                            }
                        }
                        else
                        {
                            fieldRecord[field.FieldName] = (buffer[0] == 'Y' || buffer[0] == 'T').ToString();
                        }
                        break;

                    case DBFFieldType.Memo:
                    case DBFFieldType.General:
                    case DBFFieldType.Picture:
                    default:
                        fieldRecord[field.FieldName] = buffer.ToString();
                        break;
                }
            }
        }

        private void ReadRecords()
        {
            records = new List<Dictionary<DBFFieldDescriptor, object>>();

            // Skip back to the end of the header. 
            reader.BaseStream.Seek(header.HeaderLenght, SeekOrigin.Begin);
            for (int i = 0; i < header.NumberOfRecords; i++)
            {
                if (reader.PeekChar() == '*') // DELETED
                {
                    continue;
                }

                var record = new Dictionary<DBFFieldDescriptor, object>();
                var row = reader.ReadBytes(header.RecordLenght);

                foreach (var field in fields)
                {
                    byte[] buffer = new byte[field.FieldLength];
                    Array.Copy(row, field.Address, buffer, 0, field.FieldLength);
                    string text = (encoding.GetString(buffer) ?? String.Empty).Trim();

                    switch ((DBFFieldType)field.FieldType)
                    {
                        case DBFFieldType.Character:
                            record[field] = text;
                            break;

                        case DBFFieldType.Currency:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0m;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToDecimal(text);
                            }
                            break;

                        case DBFFieldType.Numeric:
                        case DBFFieldType.Float:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0f;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToSingle(text);
                            }
                            break;

                        case DBFFieldType.Date:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                record[field] = DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture);
                            }
                            break;

                        case DBFFieldType.DateTime:
                            if (String.IsNullOrWhiteSpace(text) || BitConverter.ToInt64(buffer, 0) == 0)
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = DateTime.MinValue;
                                }
                            }
                            else
                            {
                                record[field] = JulianToDateTime(BitConverter.ToInt64(buffer, 0));
                            }
                            break;

                        case DBFFieldType.Double:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0.0;
                                }
                            }
                            else
                            {
                                record[field] = Convert.ToDouble(text);
                            }
                            break;

                        case DBFFieldType.Integer:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = 0;
                                }
                            }
                            else
                            {
                                record[field] = BitConverter.ToInt32(buffer, 0);    
                            }
                            break;

                        case DBFFieldType.Logical:
                            if (String.IsNullOrWhiteSpace(text))
                            {
                                if ((field.Flags & DBFFieldFlags.AllowNullValues) == DBFFieldFlags.AllowNullValues)
                                {
                                    record[field] = null;
                                }
                                else
                                {
                                    record[field] = false;
                                }
                            }
                            else
                            {
                                record[field] = (buffer[0] == 'Y' || buffer[0] == 'T');    
                            }
                            break;
                        
                        case DBFFieldType.Memo:
                        case DBFFieldType.General:
                        case DBFFieldType.Picture:
                        default:
                            record[field] = buffer;
                            break;
                    }
                }

                records.Add(record);
            }
        }

        ///// <summary>
        ///// Read records to a DataTable
        ///// </summary>
        //public DataTable ReadToDataTable()
        //{
        //    ReadRecords();

        //    var table = new DataTable();

        //    // Columns
        //    foreach (var field in fields)
        //    {
        //        var colType = ToDbType(field.FieldType);
        //        var column = new DataColumn(field.FieldName, colType ?? typeof(String));
        //        table.Columns.Add(column);
        //    }

        //    // Rows
        //    foreach (var record in records)
        //    {
        //        var row = table.NewRow();
        //        foreach (var column in record.Keys)
        //        {
        //            row[column.FieldName] = record[column] ?? DBNull.Value;
        //        }
        //        table.Rows.Add(row);
        //    }

        //    return table;
        //}

        /// <summary>
        /// Read To Dictionary
        /// </summary>
        public IEnumerable<Dictionary<string, object>> ReadToDictionary()
        {
            ReadRecords();
            return records.Select(record => record.ToDictionary(r => r.Key.FieldName, r => r.Value)).ToList();
        }

        /// <summary>
        /// Read to Object
        /// </summary>
        public IEnumerable<T> ReadToObject<T>()
            where T : new()
        {
            ReadRecords();

            var type = typeof(T);
            var list = new List<T>();

            foreach (var record in records)
            {
                T item = new T();
                foreach (var pair in record.Select(s => new { Key = s.Key.FieldName, Value = s.Value }))
                {
                    var property = type.GetProperty(pair.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        if (property.PropertyType == pair.Value.GetType())
                        {
                            property.SetValue(item, pair.Value, null);
                        }
                        else
                        {
                            //if (pair.Value != DBNull.Value)
                            //{
                                property.SetValue(item, System.Convert.ChangeType(pair.Value, property.PropertyType), null);
                            //}
                        }
                    }
                }
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Get the list of fields
        /// </summary>
        public List<string> getFields()
        {
            List<string> fieldNames = new List<string>();

            foreach(DBFFieldDescriptor field in fields)
            {
                fieldNames.Add(field.FieldName);
            }
            return fieldNames;
        }

        private DBFHeader header;
        private List<DBFFieldDescriptor> fields = new List<DBFFieldDescriptor>();

        private List<Dictionary<DBFFieldDescriptor, object>> records = new List<Dictionary<DBFFieldDescriptor,object>>();

        #region IDisposable

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// dispose the reader
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (disposing == false) return;
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
        }

        /// <summary>
        /// Dispose the DBFReader
        /// </summary>
        ~DBFReader()
        {
            Dispose(false);
        } 

        #endregion

         /// <summary>
        /// Convert a Julian Date as long to a .NET DateTime structure
        /// Implemented from pseudo code at http://en.wikipedia.org/wiki/Julian_day
        /// </summary>
        /// <param name="julianDateAsLong">Julian Date to convert (days since 01/01/4713 BC)</param>
        /// <returns>DateTime</returns>
        private static DateTime JulianToDateTime(long julianDateAsLong)
         {
             if (julianDateAsLong == 0) return DateTime.MinValue;
            double p = Convert.ToDouble(julianDateAsLong);
            double s1 = p + 68569;
            double n = Math.Floor(4 * s1 / 146097);
            double s2 = s1 - Math.Floor(((146097 * n) + 3) / 4);
            double i = Math.Floor(4000 * (s2 + 1) / 1461001);
            double s3 = s2 - Math.Floor(1461 * i / 4) + 31;
            double q = Math.Floor(80 * s3 / 2447);
            double d = s3 - Math.Floor(2447 * q / 80);
            double s4 = Math.Floor(q / 11);
            double m = q + 2 - (12 * s4);
            double j = (100 * (n - 49)) + i + s4;
            return new DateTime(Convert.ToInt32(j), Convert.ToInt32(m), Convert.ToInt32(d));
        }

        /// <summary>
        /// This is the file header for a DBF. We do this special layout with everything
        /// packed so we can read straight from disk into the structure to populate it
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFHeader
        {
            /// <summary>The version.</summary>
            public readonly DBFVersion Version;

            /// <summary>The update year.</summary>
            public readonly byte UpdateYear;

            /// <summary>The update month.</summary>
            public readonly byte UpdateMonth;

            /// <summary>The update day.</summary>
            public readonly byte UpdateDay;

            /// <summary>The number of records.</summary>
            public readonly int NumberOfRecords;

            /// <summary>The length of the header.</summary>
            public readonly short HeaderLenght;

            /// <summary>The length of the bytes records.</summary>
            public readonly short RecordLenght;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] Reserved;

            /// <summary>Table Flags</summary>
            public readonly DBFTableFlags TableFlags;

            /// <summary>Code Page Mark</summary>
            public readonly byte CodePage;

            /// <summary>Reserved, contains 0x00</summary>
            public readonly short EndOfHeader;
        }

        /// <summary>
        /// Dbf Version
        /// </summary>
        public enum DBFVersion : byte
        {
            /// <summary>
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// </summary>
            FoxBase = 0x02,
            /// <summary>
            /// </summary>
            FoxBaseDBase3NoMemo = 0x03,
            /// <summary>
            /// </summary>
            VisualFoxPro = 0x30,
            /// <summary>
            /// </summary>
            VisualFoxProWithAutoIncrement = 0x31,
            /// <summary>
            /// </summary>
            dBase4SQLTableNoMemo = 0x43,
            /// <summary>
            /// </summary>
            dBase4SQLSystemNoMemo = 0x63,
            /// <summary>
            /// </summary>
            FoxBaseDBase3WithMemo = 0x83,
            /// <summary>
            /// </summary>
            dBase4WithMemo = 0x8B,
            /// <summary>
            /// </summary>
            dBase4SQLTableWithMemo = 0xCB,
            /// <summary>
            /// </summary>
            FoxPro2WithMemo = 0xF5,
            /// <summary>
            /// FoxBASE
            /// </summary>
            FoxBASE = 0xFB
        }

        /// <summary>
        /// </summary>
        [Flags]
        public enum DBFTableFlags : byte
        {
            /// <summary>
            /// </summary>
            None = 0x00,
            /// <summary>
            /// </summary>
            HasStructuralCDX = 0x01,
            /// <summary>
            /// </summary>
            HasMemoField = 0x02,
            /// <summary>
            /// </summary>
            IsDBC = 0x04
        }

        /// <summary>
        /// This is the field descriptor structure. There will be one of these for each column in the table.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFFieldDescriptor
        {
            /// <summary>The field name.</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public readonly string FieldName;

            /// <summary>The field type.</summary>
            public readonly char FieldType;

            /// <summary>The field address.</summary>
            public readonly int Address;

            /// <summary>The field length in bytes.</summary>
            public readonly byte FieldLength;

            /// <summary>The field precision.</summary>
            public readonly byte DecimalCount;

            /// <summary>Field Flags</summary>
            public readonly DBFFieldFlags Flags;

            /// <summary>AutoIncrement next value</summary>
            public readonly int AutoIncrementNextValue;

            /// <summary>AutoIncrement step value</summary>
            public readonly byte AutoIncrementStepValue;

            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] Reserved;

            public override string ToString()
            {
                return String.Format("{0} {1}", FieldName, FieldType);
            }
        }

        /// <summary>
        /// </summary>
        [Flags]
        public enum DBFFieldFlags : byte
        {
            /// <summary>
            /// </summary>
            None = 0x00,
            /// <summary>
            /// </summary>
            System = 0x01,
            /// <summary>
            /// </summary>
            AllowNullValues = 0x02,
            /// <summary>
            /// </summary>
            Binary = 0x04,
            /// <summary>
            /// </summary>
            AutoIncrementing = 0x0C
        }

        /// <summary>
        /// Field Type
        /// </summary>
        public enum DBFFieldType : int
        {
            /// <summary>
            /// </summary>
            Character = 'C',
            /// <summary>
            /// </summary>
            Currency = 'Y',
            /// <summary>
            /// </summary>
            Numeric = 'N',
            /// <summary>
            /// </summary>
            Float = 'F',
            /// <summary>
            /// </summary>
            Date = 'D',
            /// <summary>
            /// </summary>
            DateTime = 'T',
            /// <summary>
            /// </summary>
            Double = 'B',
            /// <summary>
            /// </summary>
            Integer = 'I',
            /// <summary>
            /// </summary>
            Logical = 'L',
            /// <summary>
            /// </summary>
            Memo = 'M',
            /// <summary>
            /// </summary>
            General = 'G',
            /// <summary>
            /// </summary>
            Picture = 'P'
        }

        /// <summary>
        /// Dbase Type
        /// </summary>
        public static Type ToDbType(char type)
        {
            switch ((DBFFieldType)type)
            {
                case DBFFieldType.Float:
                    return typeof(float);

                case DBFFieldType.Integer:
                    return typeof(int);

                case DBFFieldType.Currency:
                    return typeof(decimal);

                case DBFFieldType.Character:
                case DBFFieldType.Memo:
                    return typeof(string);

                case DBFFieldType.Date:
                case DBFFieldType.DateTime:
                    return typeof(DateTime);

                case DBFFieldType.Logical:
                    return typeof(bool);

                case DBFFieldType.General:
                case DBFFieldType.Picture:
                    return typeof(byte[]);

                default:
                    return null;
            }
        }
    }
}