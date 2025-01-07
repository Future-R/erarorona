using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameData.Variable
{

	internal sealed class CharacterData : IDisposable
	{
		readonly Int64[] dataInteger;
		readonly string[] dataString;
		readonly Int64[][] dataIntegerArray;
		readonly string[][] dataStringArray;
		readonly Int64[][,] dataIntegerArray2D;
		readonly string[][,] dataStringArray2D;
		public Int64[] DataInteger { get { return dataInteger; } }
		public string[] DataString { get { return dataString; } }
		public Int64[][] DataIntegerArray { get { return dataIntegerArray; } }
		public string[][] DataStringArray { get { return dataStringArray; } }
		public Int64[][,] DataIntegerArray2D { get { return dataIntegerArray2D; } }
		public string[][,] DataStringArray2D { get { return dataStringArray2D; } }
		public CharacterData(ConstantData constant)
		{
			dataInteger = new Int64[(int)VariableCode.__COUNT_CHARACTER_INTEGER__];
			dataString = new string[(int)VariableCode.__COUNT_CHARACTER_STRING__];
			dataIntegerArray = new Int64[(int)VariableCode.__COUNT_CHARACTER_INTEGER_ARRAY__][];
			dataStringArray = new string[(int)VariableCode.__COUNT_CHARACTER_STRING_ARRAY__][];
			dataIntegerArray2D = new Int64[(int)VariableCode.__COUNT_CHARACTER_INTEGER_ARRAY_2D__][,];
			dataStringArray2D = new string[(int)VariableCode.__COUNT_CHARACTER_STRING_ARRAY_2D__][,];
			for (int i = 0; i < dataIntegerArray.Length; i++)
				dataIntegerArray[i] = new Int64[constant.CharacterIntArrayLength[i]];
			for (int i = 0; i < dataStringArray.Length; i++)
				dataStringArray[i] = new string[constant.CharacterStrArrayLength[i]];
			for (int i = 0; i < dataIntegerArray2D.Length; i++)
			{
				Int64 length64 = constant.CharacterIntArray2DLength[i];
				int length = (int)(length64 >> 32);
				int length2 = (int)(length64 & 0x7FFFFFFF);
				dataIntegerArray2D[i] = new Int64[length, length2];
			}
			for (int i = 0; i < dataStringArray2D.Length; i++)
			{
				Int64 length64 = constant.CharacterStrArray2DLength[i];
				int length = (int)(length64 >> 32);
				int length2 = (int)(length64 & 0x7FFFFFFF);
				dataStringArray2D[i] = new string[length, length2];
			}
		}
		public CharacterData(ConstantData constant, CharacterTemplate tmpl)
			: this(constant)
		{

			dataInteger[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.NO] = tmpl.No;
			dataString[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.NAME] = tmpl.Name;
			dataString[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CALLNAME] = tmpl.Callname;
			dataString[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.NICKNAME] = tmpl.Nickname;
			dataString[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MASTERNAME] = tmpl.Mastername;
			Int64[] array, array2;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MAXBASE];
			array2 = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.BASE];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Maxbase)
			{
				array[pair.Key] = pair.Value;
				array2[pair.Key] = pair.Value;
			}
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MARK];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Mark)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXP];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Exp)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ABL];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Abl)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TALENT];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Talent)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.RELATION];
			for (int i = 0; i < array.Length; i++)
				array[i] = Config.RelationDef;
			foreach (KeyValuePair<int, Int64> pair in tmpl.Relation)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CFLAG];
			foreach (KeyValuePair<int, Int64> pair in tmpl.CFlag)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EQUIP];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Equip)
				array[pair.Key] = pair.Value;
			array = dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.JUEL];
			foreach (KeyValuePair<int, Int64> pair in tmpl.Juel)
				array[pair.Key] = pair.Value;
			string[] arrays = dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CSTR];
			foreach (KeyValuePair<int, string> pair in tmpl.CStr)
				arrays[pair.Key] = pair.Value;
			/*
			//tmpl.Maxbase.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MAXBASE], 0);
            Buffer.BlockCopy(tmpl.Maxbase, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MAXBASE], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MAXBASE]);
            //tmpl.Maxbase.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.BASE], 0);
            Buffer.BlockCopy(tmpl.Maxbase, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.BASE], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.BASE]);

			//tmpl.Mark.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MARK], 0);
            Buffer.BlockCopy(tmpl.Mark, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MARK], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.MARK]);
			//tmpl.Exp.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXP], 0);
            Buffer.BlockCopy(tmpl.Exp, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXP], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EXP]);
            //tmpl.Abl.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ABL], 0);
            Buffer.BlockCopy(tmpl.Abl, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ABL], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.ABL]);
            //tmpl.Talent.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TALENT], 0);
            Buffer.BlockCopy(tmpl.Talent, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TALENT], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.TALENT]);
            //tmpl.Relation.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.RELATION], 0);
            Buffer.BlockCopy(tmpl.Relation, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.RELATION], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.RELATION]);
            //tmpl.CFlag.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CFLAG], 0);
            Buffer.BlockCopy(tmpl.CFlag, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CFLAG], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CFLAG]);
            //tmpl.Equip.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EQUIP], 0);
            Buffer.BlockCopy(tmpl.Equip, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EQUIP], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.EQUIP]);
            //tmpl.Juel.CopyTo(dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.JUEL], 0);
            Buffer.BlockCopy(tmpl.Juel, 0, dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.JUEL], 0, 8 * constant.CharacterIntArrayLength[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.JUEL]);

			tmpl.CStr.CopyTo(dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CSTR], 0);
			*/
		}

		public void CopyTo(CharacterData other)
		{
			for (int i = 0; i < dataInteger.Length; i++)
				other.dataInteger[i] = dataInteger[i];
			for (int i = 0; i < dataString.Length; i++)
				other.dataString[i] = dataString[i];

			for (int i = 0; i < dataIntegerArray.Length; i++)
				for (int j = 0; j < dataIntegerArray[i].Length; j++)
					other.dataIntegerArray[i][j] = dataIntegerArray[i][j];
			for (int i = 0; i < dataStringArray.Length; i++)
				for (int j = 0; j < dataStringArray[i].Length; j++)
					other.dataStringArray[i][j] = dataStringArray[i][j];

			for (int i = 0; i < dataIntegerArray2D.Length; i++)
			{
				int length1 = dataIntegerArray2D[i].GetLength(0);
				int length2 = dataIntegerArray2D[i].GetLength(1);
				for (int j = 0; j < length1; j++)
					for (int k = 0; k < length2; k++)
						other.dataIntegerArray2D[i][j, k] = dataIntegerArray2D[i][j, k];
			}
			for (int i = 0; i < dataStringArray2D.Length; i++)
			{
				int length1 = dataStringArray2D[i].GetLength(0);
				int length2 = dataStringArray2D[i].GetLength(1);
				for (int j = 0; j < length1; j++)
					for (int k = 0; k < length2; k++)
						other.dataStringArray2D[i][j, k] = dataStringArray2D[i][j, k];
			}

		}

		const int strCount = (int)VariableCode.__COUNT_SAVE_CHARACTER_STRING__;
		const int intCount = (int)VariableCode.__COUNT_SAVE_CHARACTER_INTEGER__;
		const int intArrayCount = (int)VariableCode.__COUNT_SAVE_CHARACTER_INTEGER_ARRAY__;
		const int strArrayCount = (int)VariableCode.__COUNT_SAVE_CHARACTER_STRING_ARRAY__;
		
		public void SaveToStream(EraDataWriter writer)
		{

			for (int i = 0; i < strCount; i++)
				writer.Write(dataString[i]);
			for (int i = 0; i < intCount; i++)
				writer.Write(dataInteger[i]);
			for (int i = 0; i < intArrayCount; i++)
				writer.Write(dataIntegerArray[i]);
			for (int i = 0; i < strArrayCount; i++)
				writer.Write(dataStringArray[i]);
		}

		public void LoadFromStream(EraDataReader reader)
		{

			for (int i = 0; i < strCount; i++)
				dataString[i] = reader.ReadString();
			for (int i = 0; i < intCount; i++)
				dataInteger[i] = reader.ReadInt64();
			for (int i = 0; i < intArrayCount; i++)
				reader.ReadInt64Array(dataIntegerArray[i]);
			for (int i = 0; i < strArrayCount; i++)
				reader.ReadStringArray(dataStringArray[i]);
		}
		public void SaveToStreamExtended(EraDataWriter writer)
		{
			List<VariableCode> codeList = null;

			//dataString
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataString[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//datainteger
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataInteger[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataStringArray
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataIntegerArray
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataStringArray2D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_2D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataStringArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();

			//dataIntegerArray2D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_2D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				writer.WriteExtended(code.ToString(), dataIntegerArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);
			writer.EmuSeparete();
		}

		public void LoadFromStreamExtended(EraDataReader reader)
		{
			Dictionary<string, string> strDic = reader.ReadStringExtended();
			Dictionary<string, Int64> intDic = reader.ReadInt64Extended();
			Dictionary<string, List<string>> strListDic = reader.ReadStringArrayExtended();
			Dictionary<string, List<Int64>> intListDic = reader.ReadInt64ArrayExtended();
			Dictionary<string, List<string[]>> str2DListDic = reader.ReadStringArray2DExtended();
			Dictionary<string, List<Int64[]>> int2DListDic = reader.ReadInt64Array2DExtended();

			List<VariableCode> codeList = null;

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strDic.ContainsKey(code.ToString()))
					dataString[(int)VariableCode.__LOWERCASE__ & (int)code] = strDic[code.ToString()];

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intDic.ContainsKey(code.ToString()))
					dataInteger[(int)VariableCode.__LOWERCASE__ & (int)code] = intDic[code.ToString()];


			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strListDic.ContainsKey(code.ToString()))
					copyListToArray(strListDic[code.ToString()], dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intListDic.ContainsKey(code.ToString()))
					copyListToArray(intListDic[code.ToString()], dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

			//dataStringArray2D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_2D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (int2DListDic.ContainsKey(code.ToString()))
					copyListToArray2D(str2DListDic[code.ToString()], dataStringArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);

			//dataIntegerArray2D
			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_2D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (int2DListDic.ContainsKey(code.ToString()))
					copyListToArray2D(int2DListDic[code.ToString()], dataIntegerArray2D[(int)VariableCode.__LOWERCASE__ & (int)code]);
		}

		public void LoadFromStreamExtended_Old1802(EraDataReader reader)
		{
			Dictionary<string, string> strDic = reader.ReadStringExtended();
			Dictionary<string, Int64> intDic = reader.ReadInt64Extended();
			Dictionary<string, List<string>> strListDic = reader.ReadStringArrayExtended();
			Dictionary<string, List<Int64>> intListDic = reader.ReadInt64ArrayExtended();

			List<VariableCode> codeList = null;

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strDic.ContainsKey(code.ToString()))
					dataString[(int)VariableCode.__LOWERCASE__ & (int)code] = strDic[code.ToString()];

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intDic.ContainsKey(code.ToString()))
					dataInteger[(int)VariableCode.__LOWERCASE__ & (int)code] = intDic[code.ToString()];


			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__STRING__);
			foreach (VariableCode code in codeList)
				if (strListDic.ContainsKey(code.ToString()))
					copyListToArray(strListDic[code.ToString()], dataStringArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

			codeList = VariableIdentifier.GetExtSaveList(VariableCode.__CHARACTER_DATA__ | VariableCode.__ARRAY_1D__ | VariableCode.__INTEGER__);
			foreach (VariableCode code in codeList)
				if (intListDic.ContainsKey(code.ToString()))
					copyListToArray(intListDic[code.ToString()], dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)code]);

		}



		public void SaveToStreamBinary(EraBinaryDataWriter writer,VariableData varData)
		{
			foreach (KeyValuePair<string, VariableToken> pair in varData.GetVarTokenDic())
			{
				VariableToken var = pair.Value;
				if (var.IsSavedata && var.IsCharacterData && !var.IsGlobal)
				{
					VariableCode code = var.Code;
					VariableCode flag = code & (VariableCode.__ARRAY_1D__ | VariableCode.__ARRAY_2D__ | VariableCode.__ARRAY_3D__ | VariableCode.__STRING__ | VariableCode.__INTEGER__);
					int CodeInt = var.CodeInt;
					switch (flag)
					{
						case VariableCode.__INTEGER__:
							writer.WriteWithKey(code.ToString(), dataInteger[CodeInt]);
							break;
						case VariableCode.__STRING__:
							writer.WriteWithKey(code.ToString(), dataString[CodeInt]);
							break;
						case VariableCode.__INTEGER__ | VariableCode.__ARRAY_1D__:
							writer.WriteWithKey(code.ToString(), dataIntegerArray[CodeInt]);
							break;
						case VariableCode.__STRING__ | VariableCode.__ARRAY_1D__:
							writer.WriteWithKey(code.ToString(), dataStringArray[CodeInt]);
							break;
						case VariableCode.__INTEGER__ | VariableCode.__ARRAY_2D__:
							writer.WriteWithKey(code.ToString(), dataIntegerArray2D[CodeInt]);
							break;
						case VariableCode.__STRING__ | VariableCode.__ARRAY_2D__:
							writer.WriteWithKey(code.ToString(), dataStringArray2D[CodeInt]);
							break;
						//case VariableCode.__INTEGER__ | VariableCode.__ARRAY_3D__:
						//    writer.Write(code.ToString(), dataIntegerArray3D[CodeInt]);
						//    break;
						//case VariableCode.__STRING__ | VariableCode.__ARRAY_3D__:
						//    writer.Write(code.ToString(), dataStringArray3D[CodeInt]);
						//    break;
					}
				}
			}
			writer.WriteEOC();
		}


		public void LoadFromStreamBinary(EraBinaryDataReader reader)
		{
			VariableCode code = VariableCode.__NULL__;
			int codeInt = 0;
			while (true)
			{
				KeyValuePair<string, EraSaveDataType> nameAndType = reader.ReadVariableCode();
				VariableToken vToken = null;
				if (nameAndType.Key != null)
				{
					vToken = GlobalStatic.IdentifierDictionary.GetVariableToken(nameAndType.Key, null, false);
					code = vToken.Code;
					codeInt = (int)VariableCode.__LOWERCASE__ & (int)code;
				}
				switch (nameAndType.Value)
				{
					case EraSaveDataType.EOF:
						goto whilebreak;
					case EraSaveDataType.EOC:
						goto whilebreak;
					case EraSaveDataType.Int:
						if (vToken == null || !vToken.IsInteger || vToken.Dimension != 0)
							reader.ReadInt();
						else
							dataInteger[codeInt] = reader.ReadInt();
						break;
					case EraSaveDataType.Str:
						if (vToken == null || !vToken.IsString || vToken.Dimension != 0)
							reader.ReadString();
						else
							dataString[codeInt] = reader.ReadString();
						break;
					case EraSaveDataType.IntArray:
						if (vToken == null || !vToken.IsInteger || vToken.Dimension != 1)
							reader.ReadIntArray(null, true);
						else
							reader.ReadIntArray(dataIntegerArray[codeInt], true);
						break;
					case EraSaveDataType.StrArray:
						if (vToken == null || !vToken.IsString || vToken.Dimension != 1)
							reader.ReadStrArray(null, true);
						else
							reader.ReadStrArray(dataStringArray[codeInt], true);
						break;
					case EraSaveDataType.IntArray2D:
						if (vToken == null || !vToken.IsInteger || vToken.Dimension != 2)
							reader.ReadIntArray2D(null, true);
						else
							reader.ReadIntArray2D(dataIntegerArray2D[codeInt], true);
						break;
					case EraSaveDataType.StrArray2D:
						if (vToken == null || !vToken.IsString || vToken.Dimension != 2)
							reader.ReadStrArray2D(null, true);
						else
							reader.ReadStrArray2D(dataStringArray2D[codeInt], true);
						break;
					//case EraSaveDataType.IntArray3D:
					//    if (vToken == null || !vToken.IsInteger || vToken.Dimension != 3)
					//        reader.ReadIntArray3D(null, true);
					//    else
					//        reader.ReadIntArray3D(dataIntegerArray3D[codeInt], true);
					//    break;
					//case EraSaveDataType.StrArray3D:
					//    if (vToken == null || !vToken.IsString || vToken.Dimension != 3)
					//        reader.ReadStrArray3D(null, true);
					//    else
					//        reader.ReadStrArray3D(dataStringArray3D[codeInt], true);
					//    break;
					default:
						throw new FileEE("データ異常");
				}
			}
		whilebreak:
			return;
		}


		private void copyListToArray<T>(List<T> srcList, T[] destArray)
		{
			int count = Math.Min(srcList.Count, destArray.Length);
			srcList.CopyTo(0, destArray, 0, count);
			//for (int i = 0; i < count; i++)
			//{
			//    destArray[i] = srcList[i];
			//}
		}
		private void copyListToArray2D<T>(List<T[]> srcList, T[,] destArray)
		{
			int countX = Math.Min(srcList.Count, destArray.GetLength(0));
			int dLength = destArray.GetLength(1);
			for (int x = 0; x < countX; x++)
			{
				T[] srcArray = srcList[x];
				int countY = Math.Min(srcArray.Length, dLength);
				for (int y = 0; y < countY; y++)
				{
					destArray[x, y] = srcArray[y];
				}
			}
		}

        public void setValueAll(int varInt, Int64 value)
        {
            dataInteger[varInt] = value;
        }

        public void setValueAll(int varInt, string value)
        {
            dataString[varInt] = value;
        }

        public void setValueAll1D(int varInt, Int64 value, int start, int end)
        {
            Int64[] array = dataIntegerArray[varInt];
            for (int i = start; i < end; i++)
                array[i] = value;
        }

        public void setValueAll1D(int varInt, string value, int start, int end)
        {
            string[] array = dataStringArray[varInt];
            for (int i = start; i < end; i++)
                array[i] = value;
        }

        public void setValueAll2D(int varInt, Int64 value)
        {
            Int64[,] array = dataIntegerArray2D[varInt];
            int a1 = array.GetLength(0);
            int a2 = array.GetLength(1);
            for (int i = 0; i < a1; i++)
                for (int j = 0; j < a2; j++)
                    array[i, j] = value;
        }

        public void setValueAll2D(int varInt, string value)
        {
            string[,] array = dataStringArray2D[varInt];
            int a1 = array.GetLength(0);
            int a2 = array.GetLength(1);
            for (int i = 0; i < a1; i++)
                for (int j = 0; j < a2; j++)
                    array[i, j] = value;
        }

		#region IDisposable メンバ

		public void Dispose()
		{
			for (int i = 0; i < dataIntegerArray.Length; i++)
				dataIntegerArray[i] = null;
			for (int i = 0; i < dataStringArray.Length; i++)
				dataStringArray[i] = null;
			for (int i = 0; i < dataIntegerArray2D.Length; i++)
				dataIntegerArray2D[i] = null;
		}

		#endregion
		public Int64[] CFlag
		{
			get { return dataIntegerArray[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.CFLAG]; }
		}
		public Int64 NO
		{
			get { return dataInteger[(int)VariableCode.__LOWERCASE__ & (int)VariableCode.NO]; }
		}

		#region sort
		public IComparable temp_SortKey;
		public int temp_CurrentOrder;
		//Comparison<CharacterData>
		public static int AscCharacterComparison(CharacterData x, CharacterData y)
		{
			int ret = x.temp_SortKey.CompareTo(y.temp_SortKey);
			if (ret != 0)
				return ret;
			return x.temp_CurrentOrder.CompareTo(y.temp_CurrentOrder);
		}
		public static int DescCharacterComparison(CharacterData x, CharacterData y)
		{
			int ret = x.temp_SortKey.CompareTo(y.temp_SortKey);
			if (ret != 0)
				return -ret;
			return x.temp_CurrentOrder.CompareTo(y.temp_CurrentOrder);
		}

		public void SetSortKey(VariableToken sortkey, Int64 elem64)
		{
			//チェック済み
			//if (!sortkey.IsCharacterData)
			//    throw new ExeEE("キャラクタ変数でない");
			if (sortkey.IsString)
			{
				if (sortkey.IsArray2D)
				{
					string[,] array = dataStringArray2D[sortkey.CodeInt];
					int elem1 = (int)(elem64 >> 32);
					int elem2 = (int)(elem64 & 0x7FFFFFFF);
					if (elem1 < 0 || elem1 >= array.GetLength(0) || elem2 < 0 || elem2 >= array.GetLength(1))
						throw new CodeEE("ソートキーが配列外を参照しています");
					temp_SortKey = array[elem1, elem2];
				}
				else if (sortkey.IsArray1D)
				{
					string[] array = dataStringArray[sortkey.CodeInt];
					if (elem64 < 0 || elem64 >= array.Length)
						throw new CodeEE("ソートキーが配列外を参照しています");
					if (array[(int)elem64] != null)
						temp_SortKey = array[(int)elem64];
					else
						temp_SortKey = "";
				}
				else
				{
					if (dataString[sortkey.CodeInt] != null)
						temp_SortKey = dataString[sortkey.CodeInt];
					else
						temp_SortKey = "";
				}
			}
			else
			{
				if (sortkey.IsArray2D)
				{
					Int64[,] array = dataIntegerArray2D[sortkey.CodeInt];
					int elem1 = (int)(elem64 >> 32);
					int elem2 = (int)(elem64 & 0x7FFFFFFF);
					if (elem1 < 0 || elem1 >= array.GetLength(0) || elem2 < 0 || elem2 >= array.GetLength(1))
						throw new CodeEE("ソートキーが配列外を参照しています");
					temp_SortKey = array[elem1, elem2];
				}
				else if (sortkey.IsArray1D)
				{
					Int64[] array = dataIntegerArray[sortkey.CodeInt];
					if (elem64 < 0 || elem64 >= array.Length)
						throw new CodeEE("ソートキーが配列外を参照しています");
					temp_SortKey = array[(int)elem64];
				}
				else
				{
					temp_SortKey = dataInteger[sortkey.CodeInt];
				}
			}
		}
		#endregion
	}
}
