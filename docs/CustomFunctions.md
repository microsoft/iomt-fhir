- [Custom Functions](#custom-functions)
  - [Exception Handling](#exception-handling)
  - [Mathmatical Functions](#mathmatical-functions)
    - [add](#add)
    - [divide](#divide)
    - [multiply](#multiply)
    - [pow](#pow)
    - [subtract](#subtract)
  - [String Functions](#string-functions)
    - [insertString](#insertstring)
  - [Date Functions](#date-functions)
    - [fromUnixTimestamp](#fromunixtimestamp)
    - [fromUnixTimestampMs](#fromunixtimestampms)

# Custom Functions

A variety of functions are available for use when using **JmesPath** as the expression language. In addition to the functions available as part of the JmesPath [specification](https://jmespath.org/specification.html#built-in-functions), a number of custom functions may also be used. This article describes these functions. 

Each function has a signature that follows the JmesPath [specification](https://jmespath.org/specification.html#built-in-functions). This can be represented as:

```
return_type function_name(type $argname)
```

The signature indicates the the valid types for the arugments. If an invalid type is passed in for an argument an error will occur.

When math related functions are performed the end result _must_ be able to fit within a C# [long](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types#characteristics-of-the-integral-types) value. If this cannot happen then a mathmatical error will occur.

Please see the [specification](https://jmespath.org/specification.html#built-in-functions) for more details.

## Exception Handling

Exceptions may occur at various points within the event processing lifecyle. We detail the various points where they can occur below.

- Template Parsing
  - When
    - Each time a new batch of messages is received the device mapping template is loaded and parsed
  - Exceptions that may occur
    -  Failure to parse the template
    -  Failure to parse any expressions
 -  Outcome
    -  System will attempt to reload and parse the latest device mapping template until parsing succeeds. No new messages will be processed until parsing is successful

- Function Execution
  - When
    - Each time a function is executed against data within a message
  - Exceptions that may occur
    -  Input data does not match that of the function signature
    -  Any additional exceptions listed in the description of the function
 -  Outcome
    -  System stop processing that message. The message is not retried.

## Mathmatical Functions

### add

```
number add(number $left, number $right)
```

Returns the result of adding the left argument to the right.

Examples:
| Given                       | Expression       | Result |
|-----------------------------|------------------|--------|
| n/a                         | add(`10`, `10`)  | 20     |
| {"left" : 40, "right" : 50} | add(left, right) | 90     |
| {"left" : 0, "right" : 50}  | add(left, right) | 50     |

### divide

```
number divide(number $left, number $right)
```

Returns the result of dividing the left argument by the right.

Examples:
| Given                       | Expression          | Result                           |
|-----------------------------|---------------------|----------------------------------|
| n/a                         | divide(`10`, `10`)  | 1                                |
| {"left" : 40, "right" : 50} | divide(left, right) | 0.8                              |
| {"left" : 0, "right" : 50}  | divide(left, right) | 0                                |
| {"left" : 50, "right" : 0}  | divide(left, right) | mathmatic error : divide by zero |

### multiply

```
number multiply(number $left, number $right)
```

Returns the result of multiplying the left argument with the right.

Examples:
| Given                       | Expression            | Result |
|-----------------------------|-----------------------|--------|
| n/a                         | multiply(`10`, `10`)  | 100    |
| {"left" : 40, "right" : 50} | multiply(left, right) | 2000   |
| {"left" : 0, "right" : 50}  | multiply(left, right) | 0      |


### pow

```
number pow(number $left, number $right)
```

Returns the result of raising the left argument to the power of the right.

Examples:
| Given                         | Expression       | Result                     |
|-------------------------------|------------------|----------------------------|
| n/a                           | pow(`10`, `10`)  | 10000000000                |
| {"left" : 40, "right" : 50}   | pow(left, right) | mathmatic error : overflow |
| {"left" : 0, "right" : 50}    | pow(left, right) | 0                          |
| {"left" : 100, "right" : 0.5} | pow(left, right) | 10                         |

### subtract

```
number subtract(number $left, number $right)
```

Returns the result of subtracting the right argument from the left.

Examples:
| Given                       | Expression            | Result |
|-----------------------------|-----------------------|--------|
| n/a                         | subtract(`10`, `10`)  | 0      |
| {"left" : 40, "right" : 50} | subtract(left, right) | -10    |
| {"left" : 0, "right" : 50}  | subtract(left, right) | -50    |

## String Functions

### insertString

```
string insertString(string $original, string $toInsert, number pos)
```

Produces a new string by inserting the value of _toInsert_ into the string _original_. The string will be inserted at position _pos_ within the string _original_. 

The positional argument is zero based, the position of zero refers to the first character within the string. If the positional argument provided is out of range of the length of _original_ then an error will occur.

Examples:
| Given                                                     | Expression                                         | Result              |
|-----------------------------------------------------------|----------------------------------------------------|---------------------|
| {"original" : "mple", "toInsert" : "sa", "pos" : 0}       | insertString(original, toInsert, pos)              | "sample"            |
| {"original" : "suess", "toInsert" : "cc", "pos" : 2}      | insertString(original, toInsert, pos)              | "success"           |
| {"original" : "myString", "toInsert" : "!!", "pos" : 8}   | insertString(original, toInsert, pos)              | "myString!!"        |
| {"original" : "myString", "toInsert" : "!!"}              | insertString(original, toInsert, length(original)) | "myString!!"        |
| {"original" : "myString", "toInsert" : "!!", "pos" : 100} | insertString(original, toInsert, pos)              | error: out of range |
| {"original" : "myString", "toInsert" : "!!", "pos" : -1}  | insertString(original, toInsert, pos)              | error: out of range |

## Date Functions

### fromUnixTimestamp

```
string fromUnixTimestamp(number $unixTimestampInSeconds)
```

Produces an [ISO 8061](https://en.wikipedia.org/wiki/ISO_8601) compliant time stamp from the given Unix timestamp. The timestamp is represented as the number of seconds since the Epoch (January 1 1970).

Examples:
| Given                 | Expression              | Result                  |
|-----------------------|-------------------------|-------------------------|
| {"unix" : 1625677200} | fromUnixTimestamp(unix) | "2021-07-07T17:00:00+0" |
| {"unix" : 0}          | fromUnixTimestamp(unix) | "1970-01-01T00:00:00+0" |

### fromUnixTimestampMs

```
string fromUnixTimestampMs(number $unixTimestampInMs)
```

Produces an [ISO 8061](https://en.wikipedia.org/wiki/ISO_8601) compliant time stamp from the given Unix timestamp. The timestamp is represented as the number of milliseconds since the Epoch (January 1 1970).

Examples:
| Given                    | Expression                | Result                  |
|--------------------------|---------------------------|-------------------------|
| {"unix" : 1626799080000} | fromUnixTimestampMs(unix) | "2021-07-20T16:38:00+0" |
| {"unix" : 0}             | fromUnixTimestampMs(unix) | "1970-01-01T00:00:00+0" |
