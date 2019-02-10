using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace SqlFileQueryLib.Tools
{
    public static class SqlDataTypeMapper
    {
        static Dictionary<Type, SqlDbType> typeMap { get; set; } = new Dictionary<Type, SqlDbType>();

        static SqlDataTypeMapper()
        {
            typeMap[typeof(string)] = SqlDbType.NVarChar;
            typeMap[typeof(char[])] = SqlDbType.NVarChar;
            typeMap[typeof(Int32)] = SqlDbType.Int;
            typeMap[typeof(Int16)] = SqlDbType.SmallInt;
            typeMap[typeof(Int64)] = SqlDbType.BigInt;
            typeMap[typeof(Byte[])] = SqlDbType.VarBinary;
            typeMap[typeof(Boolean)] = SqlDbType.Bit;
            typeMap[typeof(DateTime)] = SqlDbType.DateTime2;
            typeMap[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
            typeMap[typeof(Decimal)] = SqlDbType.Decimal;
            typeMap[typeof(Double)] = SqlDbType.Float;
            typeMap[typeof(Decimal)] = SqlDbType.Money;
            typeMap[typeof(Byte)] = SqlDbType.TinyInt;
            typeMap[typeof(TimeSpan)] = SqlDbType.Time;

            //Nulable
            typeMap[typeof(Int32?)] = SqlDbType.Int;
            typeMap[typeof(Int16?)] = SqlDbType.SmallInt;
            typeMap[typeof(Int64?)] = SqlDbType.BigInt;
            typeMap[typeof(Boolean?)] = SqlDbType.Bit;
            typeMap[typeof(DateTime?)] = SqlDbType.DateTime2;
            typeMap[typeof(DateTimeOffset?)] = SqlDbType.DateTimeOffset;
            typeMap[typeof(Decimal?)] = SqlDbType.Decimal;
            typeMap[typeof(Double?)] = SqlDbType.Float;
            typeMap[typeof(Decimal?)] = SqlDbType.Money;
            typeMap[typeof(Byte?)] = SqlDbType.TinyInt;
            typeMap[typeof(TimeSpan?)] = SqlDbType.Time;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SqlDbType GetDbType(Type inputType)
        {
            return typeMap[inputType];
        }
    }
}
