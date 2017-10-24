using Xunit;

namespace System.Data.Odbc.Tests
{
    public class ReaderTests : IntegrationTestBase
    {
        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void EmptyReader()
        {
            Console.WriteLine(nameof(EmptyReader));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeString NVARCHAR(100))";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeString
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                Assert.False(reader.HasRows);

                var exception = Record.Exception(() => reader.GetString(1));
                Assert.NotNull(exception);
                Assert.IsType<InvalidOperationException>(exception);
                Assert.Equal(
                    "No data exists for the row/column.",
                    exception.Message);

                var values = new object[1];
                exception = Record.Exception(() => reader.GetValues(values));
                Assert.NotNull(exception);
                Assert.IsType<InvalidOperationException>(exception);
                Assert.Equal(
                    "No data exists for the row/column.",
                    exception.Message);
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void GetValues()
        {
            Console.WriteLine(nameof(GetValues));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeInt32 INT,
                    SomeString NVARCHAR(100))";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeInt32,
                    SomeString)
                VALUES (
                    2147483647,
                    'SomeString')";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeInt32,
                    SomeString
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                Assert.Equal(2147483647, values[0]);
                Assert.Equal("SomeString", values[1]);
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void GetValueFailsWithBigIntWithBackwardsCompatibility()
        {
            Console.WriteLine(nameof(GetValueFailsWithBigIntWithBackwardsCompatibility));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeInt64 BIGINT)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeInt64)
                VALUES (
                    2147499983647)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeInt64
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                var values = new object[reader.FieldCount];
                var exception = Record.Exception(() => reader.GetValue(0));
                Assert.NotNull(exception);
                Assert.IsType<ArgumentException>(exception);
                Assert.Equal(
                    "Unknown SQL type - -25.",
                    exception.Message);

                Assert.Equal(2147499983647, reader.GetInt64(0));
                Assert.Equal(2147499983647, reader.GetValue(0));
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void GetDataTypeName()
        {
            Console.WriteLine(nameof(GetDataTypeName));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeInt64 BIGINT)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeInt64)
                VALUES (
                    2147499983647)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeInt64
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                Assert.Equal("BIGINT", reader.GetDataTypeName(0));
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void GetFieldTypeIsNotSupportedInSqlite()
        {
            Console.WriteLine(nameof(GetFieldTypeIsNotSupportedInSqlite));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeInt64 BIGINT)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeInt64)
                VALUES (
                    2147499983647)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeInt64
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                var exception = Record.Exception(() => reader.GetFieldType(0));
                Assert.NotNull(exception);
                Assert.IsType<ArgumentException>(exception);
                Assert.Equal(
                    "Unknown SQL type - -25.",
                    exception.Message);
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void IsDbNullIsNotSupportedInSqlite()
        {
            Console.WriteLine(nameof(IsDbNullIsNotSupportedInSqlite));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeInt64 BIGINT)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeInt64)
                VALUES (
                    2147499983647)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeInt64
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                var exception = Record.Exception(() => reader.IsDBNull(0));
                Assert.NotNull(exception);
                Assert.IsType<ArgumentException>(exception);
                Assert.Equal(
                    "Unknown SQL type - -25.",
                    exception.Message);
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void InvalidRowIndex()
        {
            Console.WriteLine(nameof(InvalidRowIndex));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeString NVARCHAR(100))";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeString)
                VALUES (
                    'SomeString')";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeString
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                Assert.True(reader.HasRows);
                var exception = Record.Exception(() => reader.GetString(2));
                Assert.NotNull(exception);
                Assert.IsType<IndexOutOfRangeException>(exception);
                Assert.Equal(
                    "Index was outside the bounds of the array.",
                    exception.Message);
            }
        }

        [ConditionalFact(Helpers.AllSqlite3DepsIsAvailable)]
        public void InvalidRowName()
        {
            Console.WriteLine(nameof(InvalidRowName));
            command.CommandText =
                @"CREATE TABLE SomeTable (
                    SomeString NVARCHAR(100))";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO SomeTable (
                    SomeString)
                VALUES (
                    'SomeString')";
            command.ExecuteNonQuery();

            command.CommandText =
                @"SELECT 
                    SomeString
                FROM SomeTable";
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                Assert.True(reader.HasRows);
                var exception = Record.Exception(() => reader["SomeOtherString"]);
                Assert.NotNull(exception);
                Assert.IsType<IndexOutOfRangeException>(exception);
                Assert.Equal(
                    "SomeOtherString",
                    exception.Message);
            }
        }
    }
}
