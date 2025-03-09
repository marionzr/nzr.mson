# MSON Specification

## 1. Introduction

MSON is a compact message serialization protocol designed to minimize data transmission overhead while maintaining a clear, 
structured approach to data representation.

The idea behind MSON was originally conceived in 2005 as a alternative solution to XML to serialize objects and transmit them over very limited GPRS/2G connections, 
which were the most common data transmission methods available in Brazil at that time. The protocol was initially developed in C# .NET CF 3.5 to run on a Windows Mobile app, 
and in Java 1.4 for backend services.

Few years later, in another project, the algorithm was extended to support fragmentation, addressing a key limitation of satellite communication. 
At that point, many regions in Brazil still did not have access to reliable Internet connection, and messages sent via satellite were constrained 
by a 1200-character limit (ASCII only). Fragmentation allowed messages to be split into smaller parts, enabling reliable transmission even under these constraints.

While the original MSON protocol was highly compact, it had some limitations. It used reserved symbols (* ~ | ^) to separate objects, properties, and collections, 
and it was restricted to just one object type per message, with the rest being limited to primitive types or collections of primitive types.

Although MSON may not find a place in most modern projects, the core idea has remained fresh in my mind. Upon finding the original code stored in a ZIP file, 
I decided to "remaster" it, updating the protocol for modern use. This new version of MSON is more flexible than its predecessor. It still maintains the compact nature of the original format, 
but it now supports a wider range of data structures. 

Give a start to the project and let's see where it goes. I will soon implement the first version of the MSON parser in Java and maybe in other languages

## 2. Core Principles

### 2.1 Design Goals

- Minimize data transmission size
- Maintain strict schema-based parsing
- Support complex nested structures
- Enable versioning and message fragmentation
- Provide space-efficient representations of common data types

## 3. Message Structure

### 3.1 Basic Message Format

A complete MSON message consists of two primary components:

1. Header (optional)
2. Payload

#### 3.1.1 Header Format

`<version><fragment_info>`

- Version: Single character (0-9, a-z, A-Z)
- Fragment Info: `<current_fragment>/<total_fragments>`

#### Examples

Full message, first version, no fragmentation

```mson
a1/1{1,abc}
```

Fragmented message

```mson
x1/2{first part of...
x2/2...message}
```

## 4. Data Type Representations

### 4.1 Primitive Types

#### 4.1.1 Integers

- Represented directly without quotes
- Supports positive and negative values

JSON

```json
{"age": 30}
```

MSON

```mson
{30}
```

#### 4.1.2 Floating Point Numbers

- Supports standard decimal representation
- No quotes or type indicators

JSON

```json
{"temperature": 98.6}

MSON

```mson
{98.6}
```

#### 4.1.3 Booleans

- Represented as bits: 0 (false), 1 (true)

JSON

```json
{"is_active": true}

MSON

```mson
{1}
```

#### 4.1.4 Strings

- Automatically trimmed
- No surrounding quotes
- Whitespace removed from start and end

JSON

```json
{"username": "  johndoe  "}
```

MSON

```mson
{johndoe}
```

#### 4.1.5 Null/Empty Values

- Empty brackets `{}` represent null
- `{,}` represents an object with all null properties
- `{,1}` represents an object with first property null

JSON with null

```json
{
  "profile": null,
  "settings": {
    "username": null,
    "email": "user@example.com"
  }
}

MSON equivalent

```mson
{{},{user@example.com}}
```

### 4.2 Date and Time

#### 4.2.1 Compact DateTime Format

`YYYYMMDDHHMMSS+TIMEZONE`

Example: 2025-03-05 02:47:30 +02:00

```mson
20250305024730+0200
```

## 5. Nested Structures

### 5.1 Objects and Arrays

- Represented using nested brackets
- Order of elements strictly follows predefined schema
- Nested structures maintain hierarchical relationships

JSON User Profile

```json
{
  "user": {
    "id": 12345,
    "username": "ronaldo9",
    "profile": {
      "fullName": "Ronaldo Nazário",
      "contacts": {
        "email": "ronaldo@r9.com"
      }
    }
  }
}
```

MSON Equivalent

```mson
{{12345,ronaldo9,{Ronaldo Nazário,{ronaldo@r9.com}}}}
```

## 6. Versioning and Schema

### 6.1 Version Identifier

- Single character (0-9, a-z, A-Z)
- Enables protocol/schema versioning
- Supports 62 distinct versions

### 6.2 Schema Requirements

- Server and client must have identical, versioned schema
- Schema defines:
  - Field order
  - Data types
  - Nested object structures
  - Field names (not transmitted)

## 7. Fragmentation

### 7.1 Large Message Handling

- Messages can be split across multiple transmissions
- Header indicates current fragment and total fragments

```mson
x1/3{first part of message...
x2/3{second part of message...
x3/3{final part of message}
```

## 8. Parsing Considerations

### 8.1 Parser Implementation Guidelines

- Strict positional parsing
- Rely entirely on predefined schema
- Handle nested structures recursively
- Implement robust error checking
- Support version compatibility

## 9. Compression Techniques

### 9.1 Overhead Reduction

- No repeated property names
- Minimal syntax characters
- Compact primitive representations
- Automatic string trimming

## 10. Recommended Use Cases

- High-frequency message exchanges
- IoT device communication
- Real-time data streaming
- Low-bandwidth networks
- Microservices with consistent schemas

## 11. Limitations and Considerations

- Requires strict schema synchronization
- Less human-readable than JSON
- Increased complexity in initial implementation
- Potential performance overhead for schema management

## 12. Compression Comparison Analysis

The following section provides a comparative analysis of character count and compression ratios between JSON and MSON formats using a more complex data structure.

### 12.1 Size Comparison Analysis

JSON Representation

```json
{"StateId":"9c4483b1-a523-e7c0-0293-052111033373","Customer":{"EmailAddress":"Chet.Shanahan@hotmail.com","Id":1,"CreatedAt":"2017-05-03T01:02:03+00:00","LastUpdatedAt":"2024-09-13T10:40:06+00:00"},"Products":[{"Name":"Licensed Wooden Bacon","Tags":["Grocery","Games","Baby"],"Category":{"Name":"Jewelery","products":[],"Id":10101010,"CreatedAt":"2018-09-01T00:01:02+00:00","LastUpdatedAt":"2018-09-01T10:59:59+00:00"},"Status":0,"Price":374.90,"Description":"Description with special chars: \u0027 \u0022","ReleaseDate":"2018-08-30T02:00:00+02:00","Weight":null,"Id":123456789,"CreatedAt":"2018-09-02T10:20:30+00:00","LastUpdatedAt":"2018-09-02T11:12:13+00:00"},{"Name":"Incredible Granite Hat","Tags":["Home","Kids","Games"],"Category":{"Name":"Jewelery","products":[],"Id":10101010,"CreatedAt":"2018-09-01T00:01:02+00:00","LastUpdatedAt":"2018-09-01T10:59:59+00:00"},"Status":1,"Price":586.21,"Description":null,"ReleaseDate":"2018-08-30T02:00:00+02:00","Weight":645,"Id":987654321,"CreatedAt":"2018-09-02T10:20:30+00:00","LastUpdatedAt":"2018-09-02T11:12:13+00:00"},{"Name":"Refined Soft Bike","Tags":[],"Category":{"Name":"Home","products":[],"Id":20202020,"CreatedAt":"2018-09-01T00:01:02+00:00","LastUpdatedAt":"2018-09-01T10:59:59+00:00"},"Status":2,"Price":797.52,"Description":"Description with reserved chars: {} [] ,","ReleaseDate":null,"Weight":13,"Id":111111111,"CreatedAt":"2018-09-02T10:20:30+00:00","LastUpdatedAt":"2018-09-02T11:12:13+00:00"}],"Id":999999999,"CreatedAt":"2018-09-03T01:02:03+00:00","LastUpdatedAt":"2018-09-03T10:40:06+00:00"}
```

MSON Representation

```mson
11/1~{9c4483b1a523e7c00293052111033373,{Chet.Shanahan@hotmail.com,1,20170503010203000+0000,20240913104006000+0000},[{Licensed Wooden Bacon,[Grocery,Games,Baby],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},New,374.90,Description with special chars: ' ",20180830020000000+0200,,123456789,20180902102030000+0000,20180902111213000+0000},{Incredible Granite Hat,[Home,Kids,Games],{Jewelery,10101010,20180901000102000+0000,20180901105959000+0000},Active,586.21,,20180830020000000+0200,645,987654321,20180902102030000+0000,20180902111213000+0000},{Refined Soft Bike,[],{Home,20202020,20180901000102000+0000,20180901105959000+0000},Inactive,797.52,Description with reserved chars: \{\} \[\] \,,,13,111111111,20180902102030000+0000,20180902111213000+0000}],999999999,20180903010203000+0000,20180903104006000+0000}
```

| JSON            | MSON            | % of Reduction |
|-----------------|-----------------|----------------|
| 1464 characters | 982 characters  | 33.0%          |


### 12.2 GZIP Compression Comparison

Compressed JSON Representation

```json-gzip
H4sIAAAAAAAAA81UUW+bMBD+K8ivDdHZhgB+WtNOabdqmppMfejy4MA1WCMQgbOqivrfey6EVMm6btIqzQYZ4893993n85ZNrbZ4mTHFkjQIYrngvg6F9DFKwQeRSB9CwTkHKWUk2YCdbRpbrbBmass+rrQpTrOsxqYhC2c52uE016Wm90NeWbc8TKsVbXMuOO2ukfxlp5bgAnhE1n2QM+AKhAJ5AqAACH6lG/ttnb3AisCHxOdyxkEFhBp12McB+1pX2Sa1FMLtln3RK6QNVybFssHMu6mqDEtvrNOqJMMzvXQ4NqmrFOsH+jOhDQ2NY714YHMKkZwuK1pSvbFPeI9Fi17vfc07VtD2I3KxCxj4zMXp+P2GXI8lcmFCz56cE2hD7sDxJFJMySgYJjQ9xyatzdoaIqZezrx7Y3OvWWNqdOGlua4b5X3fAIioHQQFcU2MdIPnFEUfQuxLmDkhnPOT5w9C3qBZ5hRnuSmKjrKQQTiK4uQVzsLxEKAkvM2ZsJwMKi57zn3eL8u0xswsCvQmtS6NRe9C2xcqXtBJpOlnkzW9kv+vhLyXMIxHQ8EPJGzz+/e6jIKwpZHE0SgMpDius38ryzXemZJKa1rdWW9sfuBekV8nv5PpOPFU1s/9nRMv+sRHSTQMxZu1Qxca1j+JYlc820fvdu4NDsumVWynA5fdadq1d5Ghy1yya684+aM7tcce3qlPTTBa/BgGAAA=
```

Compressed MSON Representation

```mson-gzip
H4sIAAAAAAAAA61SwW7bMAz9FWGXHUZ4pGTZVk9rWiDtNuywHnawc1AUFhaaSIVtNCiM7tunxDWCrdghQElQIB4EPj49EX2m36NxeV6pNVktFZcOURqFWhIRKqVKBeNVy0N219pgU31p47Czfpu5uAMCiVSiRoWEMp2In1JhgmWOhhRhjljM8AvU43fvOPS8Eb9i3HAQC+tigHrZRcfdMyztjntY2PXzCsavvOftASWc8kBXoUkdHglPdBNMqI02J7ofvAdV5plBuObedf5x8DGIvR9a0T+y83YrXGu7/kJ8FB+mMVVScZiMxzGpAyCpcl2UlZmJ5Kz2b/7Do0lSJ/7xNriON369ZbHsbPADixs7QH0Tdwzf/KafBL+H1ks3+CcGXRWZJPifliLXYKqy0LmSdLacn3zvQ/LuLt4PYuEfGOq0+lFMcvyY5659G+y0eGnKTMu3PnXcc/eUSF+NasbmRTR1sxINJGsU0BxnylmBmWO+8vYbT/A/3/gPx0j+1jkDAAA=
```

When applying GZIP compression to both formats:

| JSON (gzip)     | MSON (gzip)     | % of Reduction |
|-----------------|-----------------|----------------|
| 626 bytes       | 488 bytes       | 22.1%          |

This demonstrates that even after standard compression techniques are applied, MSON still provides significant bandwidth savings. The compounded effect of MSON's inherent efficiency plus standard compression techniques yields the optimal transmission size.

### 12.3 Analysis of Savings

The primary sources of character reduction in MSON include:

1. **Elimination of Property Names**: ~482 characters saved (32.9%)
   - No repetition of keys like "productId", "name", "price", etc.

2. **Removal of JSON Syntax**: ~315 characters saved (21.5%)
   - No quotes around strings
   - No colons between keys and values
   - Minimal use of structural characters

3. **Compact Data Representation**: ~354 characters saved (24.2%)
   - Date format optimization (ISO 8601 → MSON compact format)
   - Boolean representation (true/false → 1/0)
   - String trimming
   - Null optimization

The compression efficiency becomes even more significant as:

- Data volume increases
- Message structure becomes more deeply nested
- Messages are repeatedly transmitted with the same structure

# MSON Schema Specification Extension

## 13. Schema Definition

### 13.1 Introduction to MSON Schemas

MSON schemas define the structure, data types, and field ordering that both senders and receivers must adhere to. A schema acts as the contract between communication endpoints, enabling MSON's compact representation while ensuring reliable data interpretation.

### 13.2 Schema Format

An MSON schema is defined using a structured format that specifies all aspects of valid messages.

#### 13.2.1 Schema Header

```
<version_identifier><fragment-id>/<total-fragments>~<content>
```

Where 
- `<version_identifier>` matches the single character version used in messages (0-9, a-z, A-Z).
- `<fragment-id>`and `<total-fragments>` is used to identify fragmented messages. In case of no fragmentation will be always 1/1
- `<content>`is the actual serialized object

#### 13.2.2 Core Schema Structure

```
{
  "version": "<version_identifier>",
  "name": "<schema_name>",
  "root": {
    <field_definitions>
  }
}
```

### 13.3 Field Definition Format

Each field in the schema is defined with the following properties:

```json
"fieldName": {
  "type": "<data_type>",
  "position": <integer>,
  "description": "<field_description>",
  "fields": {
    <nested_field_definitions>
  }
}
```

#### 13.3.1 Supported Data Types

| Type | Description | MSON Representation |
|------|-------------|---------------------|
| `integer` | Whole number | Directly as number |
| `float` | Decimal number | Directly as number with decimal point |
| `boolean` | True/false value | 1 (true), 0 (false) |
| `string` | Text value | Without quotes, auto-trimmed |
| `datetime` | Date and time | `YYYYMMDDHHMMSS+TIMEZONE` format |
| `object` | Nested structure | Nested brackets `{}` |
| `array` | Ordered collection | Nested brackets with commas `[]` |
| `null` | Empty/null value | Empty brackets `{}` |


---

## Contributing
Contributions are welcome! Feel free to submit issues or pull requests.

---

## License

Nzr.MSON and MSON Specification
Copyright (C) 2025 https://github.com/marionzr

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.


# Disclaimer

This project is provided "as-is" without any warranty or guarantee of its functionality. The author assumes no responsibility or liability for any issues, damages, or consequences arising from the use of this code, whether direct or indirect. By using this project, you agree that you are solely responsible for any risks associated with its use, and you will not hold the author accountable for any loss, injury, or legal ramifications that may occur.

Please ensure that you understand the code and test it thoroughly before using it in any production environment.
