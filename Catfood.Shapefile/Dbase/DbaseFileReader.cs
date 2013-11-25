using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ShapefileReader
{
	/// <summary>
	/// Class that allows records in a dbase file to be enumerated.
	/// </summary>
	public partial class DbaseFileReader : IEnumerable
	{
		/// <summary>
		/// 
		/// </summary>
		private partial class DbaseFileEnumerator : IEnumerator, IDisposable
		{
			DbaseFileReader _parent;
			//ArrayList _arrayList;
			DbaseRecord _dataRecord;
			int _iCurrentRecord = 0;
			private BinaryReader _dbfStream = null;
			private int _readPosition = 0;
			private DbaseFileHeader _header = null;
			protected string[] _fieldNames = null;


			#region Implementation of IEnumerator

			/// <summary>
			/// Sets the enumerator to its initial position, which is 
			/// before the first element in the collection.
			/// </summary>
			/// <exception cref="T:System.InvalidOperationException">
			/// The collection was modified after the enumerator was created. 
			/// </exception>
			public void Reset()
			{
				_dbfStream.BaseStream.Seek(_header.HeaderLength, SeekOrigin.Begin);
				_iCurrentRecord = 0;
				//throw new InvalidOperationException();
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The collection was modified after the enumerator was created.
			///  </exception>
			public bool MoveNext()
			{
				_iCurrentRecord++;
				if (_iCurrentRecord <= _header.NumRecords)
					_dataRecord = this.Read();
				bool more = true;
				if (_iCurrentRecord > _header.NumRecords)
				{
					//this._dbfStream.Close();			
					more = false;
				}
				return more;
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			/// <value></value>
			/// <returns>The current element in the collection.</returns>
			/// <exception cref="T:System.InvalidOperationException">
			/// The enumerator is positioned before the first element of the collection
			/// or after the last element.
			/// </exception>
			public object Current
			{
				get
				{
					return _dataRecord;
				}
			}


			/// <summary>
			/// 
			/// </summary>
			protected void ReadHeader()
			{
				_header = new DbaseFileHeader();
				// read the header
				_header.ReadHeader(_dbfStream);

				// how many records remain
				_readPosition = _header.HeaderLength;
			}

			/// <summary>
			/// Read a single dbase record
			/// </summary>
			/// <returns>
			/// The read shapefile record,
			///  or null if there are no more records.
			///  </returns>
			private DbaseRecord Read()
			{
				ArrayList attrs = null;

				bool foundRecord = false;
				while (!foundRecord)
				{
					// retrieve the record length
					int tempNumFields = _header.NumFields;

					// storage for the actual values
					attrs = new ArrayList(tempNumFields);

					// read the deleted flag
					char tempDeleted = (char)_dbfStream.ReadChar();

					// read the record length
					int tempRecordLength = 1; // for the deleted character just read.

					// read the Fields
					for (int j = 0; j < tempNumFields; j++)
					{
						// find the length of the field.
						int tempFieldLength = _header.Fields[j].Length;
						tempRecordLength = tempRecordLength + tempFieldLength;

						// find the field type
						char tempFieldType = _header.Fields[j].DbaseType;

						// read the data.
						object tempObject = null;
						switch (tempFieldType)
						{
							case 'L':   // logical data type, one character (T,t,F,f,Y,y,N,n)
								char tempChar = (char)_dbfStream.ReadByte();
								if ((tempChar == 'T') || (tempChar == 't') || (tempChar == 'Y') || (tempChar == 'y'))
									tempObject = true;
								else tempObject = false;
								break;

							case 'C':   // character record.
								char[] sbuffer = new char[tempFieldLength];
								sbuffer = _dbfStream.ReadChars(tempFieldLength);
								// use an encoding to ensure all 8 bits are loaded
								// tempObject = new string(sbuffer, "ISO-8859-1").Trim();								

								//HACK: this can be made more efficient
								tempObject = new string(sbuffer).Trim().Replace("\0", String.Empty);   //.ToCharArray();
								break;

							case 'D':   // date data type.
								char[] ebuffer = new char[8];
								ebuffer = _dbfStream.ReadChars(8);
								string tempString = new string(ebuffer, 0, 4);

								int year;
								if (!Int32.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture, out year))
									break;
								tempString = new string(ebuffer, 4, 2);

								int month;
								if (!Int32.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture, out month))
									break;
								tempString = new string(ebuffer, 6, 2);

								int day;
								if (!Int32.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture, out day))
									break;

								tempObject = new DateTime(year, month, day);
								break;

							case 'N': // number
							case 'F': // floating point number
								char[] fbuffer = new char[tempFieldLength];
								fbuffer = _dbfStream.ReadChars(tempFieldLength);
								tempString = new string(fbuffer).Replace("\0", string.Empty);
								if (string.IsNullOrEmpty(tempString))
									tempString = "0";
								try
								{
									tempObject = Double.Parse(tempString.Trim(), CultureInfo.InvariantCulture);
								}
								catch (FormatException)
								{
									// if we can't format the number, just save it as a string
									tempObject = tempString;
								}
								break;

							default:
								throw new NotSupportedException("Do not know how to parse Field type " + tempFieldType);
						}
						attrs.Add(tempObject);
					}

					// ensure that the full record has been read.
					if (tempRecordLength < _header.RecordLength)
					{
						byte[] tempbuff = new byte[_header.RecordLength - tempRecordLength];
						tempbuff = _dbfStream.ReadBytes(_header.RecordLength - tempRecordLength);
					}

					// add the row if it is not deleted.
					if (tempDeleted != '*')
					{
						foundRecord = true;
					}
				}
				return new DbaseRecord(_header, attrs);
			}

			#endregion


		}

		public class DbaseRecord : IDataRecord
		{
			public readonly ArrayList Metadata;
			private DbaseFileHeader _header;
			private Dictionary<string, int> _ordinals;

			public DbaseRecord(DbaseFileHeader header, ArrayList metadata)
			{
				_header = header;
				Metadata = metadata;
				_ordinals = new Dictionary<string,int>();
				for (int i = 0; i < _header.Fields.Length; i++)
					_ordinals.Add(_header.Fields[i].Name, i);
			}

			#region IDataRecord Membres

			public int FieldCount
			{
				get { return _header.NumFields; }
			}

			public bool GetBoolean(int i)
			{
				return (bool)Metadata[i];
			}

			public byte GetByte(int i)
			{
				return (byte)Metadata[i];
			}

			public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
			{
				throw new NotImplementedException();
			}

			public char GetChar(int i)
			{
				return (char)Metadata[i];
			}

			public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
			{
				throw new NotImplementedException();
			}

			public IDataReader GetData(int i)
			{
				throw new NotImplementedException();
			}

			public string GetDataTypeName(int i)
			{
				return _header.Fields[i].Type.Name;
			}

			public DateTime GetDateTime(int i)
			{
				return (DateTime)Metadata[i];
			}

			public decimal GetDecimal(int i)
			{
				return (decimal)Metadata[i];
			}

			public double GetDouble(int i)
			{
				return (double)Metadata[i];
			}

			public Type GetFieldType(int i)
			{
				return _header.Fields[i].Type;
			}

			public float GetFloat(int i)
			{
				return (float)Metadata[i];
			}

			public Guid GetGuid(int i)
			{
				return (Guid)Metadata[i];
			}

			public short GetInt16(int i)
			{
				return (short)Metadata[i];
			}

			public int GetInt32(int i)
			{
				return (int)Metadata[i];
			}

			public long GetInt64(int i)
			{
				return (long)Metadata[i];
			}

			public string GetName(int i)
			{
				return _header.Fields[i].Name;
			}

			public int GetOrdinal(string name)
			{
				return _ordinals[name];
			}

			public string GetString(int i)
			{
				return (string)Metadata[i];
			}

			public object GetValue(int i)
			{
				return Metadata[i];
			}

			public int GetValues(object[] values)
			{
				throw new NotImplementedException();
			}

			public bool IsDBNull(int i)
			{
				return false;
			}

			public object this[string name]
			{
				get { return GetValue(GetOrdinal(name)); }
			}

			public object this[int i]
			{
				get { return GetValue(i); }
			}

			#endregion
		}


		private DbaseFileHeader _header = null;
		private string _filename;

		#region Constructors



		#endregion

		#region Methods



		#endregion

		#region Implementation of IEnumerable

		/// <summary>
		/// Gets the object that allows iterating through the members of the collection.
		/// </summary>
		/// <returns>
		/// An object that implements the IEnumerator interface.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			return new DbaseFileEnumerator(this);
		}

		#endregion
	}
}
