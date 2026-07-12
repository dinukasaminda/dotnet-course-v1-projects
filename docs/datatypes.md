### C# Datatypes


| Alias | .NET Type | Category | Size | Default Value | Range / Notes |
|---------|------------|------------|--------|---------------|---------------|
| `byte` | System.Byte | Integer | 8-bit unsigned | `0` | 0 to 255 |
| `sbyte` | System.SByte | Integer | 8-bit signed | `0` | -128 to 127 |
| `short` | System.Int16 | Integer | 16-bit signed | `0` | -32,768 to 32,767 |
| `ushort` | System.UInt16 | Integer | 16-bit unsigned | `0` | 0 to 65,535 |
| `int` | System.Int32 | Integer | 32-bit signed | `0` | -2,147,483,648 to 2,147,483,647 |
| `uint` | System.UInt32 | Integer | 32-bit unsigned | `0` | 0 to 4,294,967,295 |
| `long` | System.Int64 | Integer | 64-bit signed | `0L` | -9.2 × 10¹⁸ to 9.2 × 10¹⁸ |
| `ulong` | System.UInt64 | Integer | 64-bit unsigned | `0UL` | 0 to 1.8 × 10¹⁹ |
| `nint` | System.IntPtr | Integer | Platform (32/64-bit) | `0` | Depends on platform |
| `nuint` | System.UIntPtr | Integer | Platform (32/64-bit) | `0` | Depends on platform |
| `float` | System.Single | Floating-point | 32-bit | `0.0f` | ±1.5×10⁻⁴⁵ to ±3.4×10³⁸ (~7 digits) |
| `double` | System.Double | Floating-point | 64-bit | `0.0d` | ±5.0×10⁻³²⁴ to ±1.7×10³⁰⁸ (~15–17 digits) |
| `decimal` | System.Decimal | Floating-point | 128-bit | `0.0m` | ±1.0×10⁻²⁸ to ±7.9×10²⁸ (~28–29 digits) |
| `char` | System.Char | Text | 16-bit Unicode | `'\0'` | U+0000 to U+FFFF |
| `string` | System.String | Text | Varies | `null` | Sequence of `char`; reference type |
| `bool` | System.Boolean | Boolean | 1 bit (stored as 1 byte) | `false` | `true` or `false` |
| `object` | System.Object | Other | Varies | `null` | Base type of all types; reference type |
| `dynamic` | System.Object | Other | Varies | `null` | Resolved at runtime; reference type |



| Feature                 |          Can change? | Set when?    | Example                  |
| ----------------------- | -------------------: | ------------ | ------------------------ |
| `var` / normal variable |                  Yes | Runtime      | user input               |
| `const`                 |                   No | Compile time | fixed mathematical value |
| `readonly`              | No after constructor | Runtime      | config value             |


## Parsing Datatypes

- Explicit parsing (int)value
- Convert.toInt 
- double.TryParse 