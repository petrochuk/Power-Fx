﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Microsoft.OpenApi.Models;
using Microsoft.PowerFx.Connectors;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Tests;
using Microsoft.PowerFx.Functions;
using Microsoft.PowerFx.Types;
using Xunit;
using static Microsoft.PowerFx.Connectors.Tests.OpenApiHelperFunctions;

namespace Microsoft.PowerFx.Tests
{
    public class OpenApiJsonSerializerTests : PowerFxTest
    {
        [Fact]
        public void JsonSerializer_Empty()
        {
            var str = SerializeJson(null);
            Assert.Equal("{}", str);
        }

        [Fact]
        public void JsonSerializer_Blank()
        {
            string expected = "{}";

            // Test against blank value
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaNumber));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaInteger));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaString));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaBoolean));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaArrayInteger));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaArrayString));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaArrayObject));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaArrayDateTime));
            Assert.Equal(expected, SerializeSchemaAgainstBlankValue(SchemaDateTime));

            // Test against error value
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaNumber));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaInteger));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaString));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaBoolean));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaArrayInteger));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaArrayString));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaArrayObject));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaArrayDateTime));
            Assert.Equal(expected, SerializeSchemaAgainstErrorValue(SchemaDateTime));
        }    

        [Fact]
        public void JsonSerializer_SingleInteger()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaInteger, FormulaValue.New(1))
            });

            Assert.Equal(@"{""a"":1}", str);
        }

        [Fact]
        public void JsonSerializer_Number()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaNumber, FormulaValue.New(1.17e-4))
            });

            Assert.Equal(@"{""a"":0.000117}", str);
        }

        [Fact]
        public void JsonSerializer_EscapedKey()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a\\b\"c\""] = (SchemaInteger, FormulaValue.New(1))
            });

            Assert.Equal(@"{""a\\b\u0022c\u0022"":1}", str);
        }

        [Fact]
        public void JsonSerializer_SingleInteger_NoValue()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaInteger, null)
            }));

            Assert.Equal("Expected NumberValue (integer) and got <null> value, for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_SingleInteger_NoSchema()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (null, null)
            }));

            Assert.Equal("Missing schema for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Integer_String_Mismatch()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaInteger, FormulaValue.New("abc"))
            }));

            Assert.Equal("Expected NumberValue (integer) and got StringValue value, for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Number_String_Mismatch()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaNumber, FormulaValue.New("abc"))
            }));

            Assert.Equal("Expected NumberValue (number) and got StringValue value, for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_String_Integer_Mismatch()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaString, FormulaValue.New(11))
            }));

            Assert.Equal("Expected StringValue and got DecimalValue value, for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Bool_String_Mismatch()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaBoolean, FormulaValue.New("abc"))
            }));

            Assert.Equal("Expected BooleanValue and got StringValue value, for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_TwoIntegers()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaInteger, FormulaValue.New(1)),
                ["b"] = (SchemaInteger, FormulaValue.New(-2))
            });

            Assert.Equal(@"{""a"":1,""b"":-2}", str);
        }

        [Fact]
        public void JsonSerializer_String()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaString, FormulaValue.New("abc"))
            });

            Assert.Equal(@"{""a"":""abc""}", str);
        }

        [Fact]
        public void JsonSerializer_Bool()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaBoolean, FormulaValue.New(true)),
                ["b"] = (SchemaBoolean, FormulaValue.New(false))
            });

            Assert.Equal(@"{""a"":true,""b"":false}", str);
        }

        [Fact]
        public void JsonSerializer_InvalidSchema()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (new OpenApiSchema() { Type = "unknown" }, FormulaValue.New(1)),
            }));

            Assert.Equal("Not supported property type unknown for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Null()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (new OpenApiSchema() { Type = "null" }, FormulaValue.New(1)),
            }));

            Assert.Equal("null schema type not supported yet for property a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Array_Integer()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayInteger, GetArray(1, 2))
            });

            Assert.Equal(@"{""a"":[1,2]}", str);
        }

        [Fact]
        public void JsonSerializer_Array_Empty()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayInteger, GetArray(Array.Empty<int>()))
            });

            Assert.Equal(@"{""a"":[]}", str);
        }

        [Fact]
        public void JsonSerializer_Array_Blank()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayInteger, GetArray(GetRecord((TableValue.ValueName, FormulaValue.NewBlank()))))
            });

            Assert.Equal(@"{""a"":[null]}", str);
        }

        [Fact]
        public void JsonSerializer_Array_String()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["z"] = (SchemaArrayInteger, GetArray("a", "b"))
            });

            Assert.Equal(@"{""z"":[""a"",""b""]}", str);
        }

        [Fact]
        public void JsonSerializer_Array_Boolean()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayString, GetArray(true, false))
            });

            Assert.Equal(@"{""a"":[true,false]}", str);
        }

        [Fact]
        public void JsonSerializer_Array_DateTime()
        {
            var dt1 = new DateTime(2022, 6, 22, 17, 5, 11, 117);
            var dt2 = new DateTime(1961, 11, 4, 2, 35, 33, 981);

            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["A"] = (SchemaArrayDateTime, GetArray(dt1, dt2))
            });

            var obj = JsonSerializer.Deserialize<DateTimeArrayType>(str);
            Assert.Equal(new[] { dt1, dt2 }, obj.A);
        }

        [Fact]
        public void JsonSerializer_Array_Record_Invalid()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayObject, GetArray(GetRecord(("x", FormulaValue.New(1)))))
            }));

            Assert.Equal("Incompatible Table for supporting array, RecordValue doesn't have 'Value' column - propertyName a", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Array_Record_Invalid2()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayObject, GetArray(GetRecord((TableValue.ValueName, GetRecord(("z", FormulaValue.New(2)))))))
            }));

            Assert.Equal("Not supported type Microsoft.PowerFx.Types.InMemoryRecordValue for value", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Array_Invalid()
        {
            var ex = Assert.Throws<PowerFxConnectorException>(() => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaArrayInteger, GetTable(GetRecord(("a", FormulaValue.New(1)), ("b", FormulaValue.New("foo")))))
            }));

            Assert.Equal("Incompatible Table for supporting array, RecordValue has more than one column - propertyName a, number of fields 2", ex.Message);
        }

        [Fact]
        public void JsonSerializer_Object()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaObject(("x", SchemaInteger, false), ("y", SchemaString, false)), GetRecord(("x", FormulaValue.New(1)), ("y", FormulaValue.New("foo"))))
            });

            Assert.Equal(@"{""a"":{""x"":1,""y"":""foo""}}", str);
        }

        [Fact]
        public void JsonSerializer_ComplexObject()
        {
            var str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaObject(
                            ("x", SchemaInteger, false),
                            ("y", SchemaString, false),
                            ("z", SchemaObject(("a", SchemaInteger, false)), false)),
                         GetRecord(
                             ("x", FormulaValue.New(1)),
                             ("y", FormulaValue.New("foo")),
                             ("z", GetRecord(("a", FormulaValue.New(-1))))))
            });

            Assert.Equal(@"{""a"":{""x"":1,""y"":""foo"",""z"":{""a"":-1}}}", str);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void JsonSerializer_Object_MissingObjectProperty(bool required)
        {
            Func<string> lambda = () => SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (SchemaObject(("x", SchemaInteger, true), ("y", SchemaString, required)), GetRecord(("x", FormulaValue.New(1))))
            });

            if (required)
            {
                var ex = Assert.Throws<PowerFxConnectorException>(lambda);
                Assert.Equal("Missing property y, object is too complex or not supported", ex.Message);
            }
            else
            {
                Assert.False(string.IsNullOrEmpty(lambda()));
            }
        }

        [Theory]
        [InlineData("2022-06-21T14:36:59.9353993+02:00")]
        [InlineData("2022-06-21T14:36:59.9353993-08:00")]
        public void JsonSerializer_Date(string dateString)
        {
            DateTime date = DateTime.Parse(dateString);
            RuntimeConfig rtConfig = new RuntimeConfig();
            rtConfig.SetTimeZone(TimeZoneInfo.Local);
            string str = SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>() { ["A"] = (SchemaDateTime, FormulaValue.New(date)) }, new ConvertToUTC(TimeZoneInfo.Local));

            DateTimeType obj = JsonSerializer.Deserialize<DateTimeType>(str);
            date = TimeZoneInfo.ConvertTimeToUtc(date);
            date = date.AddTicks(-(date.Ticks % 10000));
            Assert.Equal(date, obj.A);
        }

        private class DateTimeType
        {
            public DateTime A { get; set; }
        }

        private class DateTimeArrayType
        {
            public DateTime[] A { get; set; }
        }

        private string SerializeSchemaAgainstBlankValue(OpenApiSchema schema)
        {
            return SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (schema, FormulaValue.NewBlank())
            });
        }

        private string SerializeSchemaAgainstErrorValue(OpenApiSchema schema)
        {
            return SerializeJson(new Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)>()
            {
                ["a"] = (schema, CommonErrors.DivByZeroError(IRContext.NotInSource(FormulaType.Decimal)))
            });
        }
    }
}
