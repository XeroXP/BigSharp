BigSharp
============

![](https://img.shields.io/nuget/v/BigSharp)
![](https://img.shields.io/nuget/dt/BigSharp?color=laim)
![](https://img.shields.io/appveyor/build/XeroXP/bigsharp/master)
![](https://img.shields.io/appveyor/tests/XeroXP/bigsharp/master)

Big in C#. Port of [big.js](https://github.com/MikeMcl/big.js/). Public domain.

A small, fast C# library for arbitrary-precision decimal arithmetic.


Documentation
=============

* [Overview](#overview)
* [Installation](#installation)
* [Use](#use)
* [System requirements](#system-requirements)
* [Development and testing](#development-and-testing)
* [Contributors](#contributors)


Overview
--------

The primary goal of this project is to produce a translation of big.js to
C# which is as close as possible to the original implementation.

### Features

- Simple API
- No dependencies
- Stores values in an accessible decimal floating point format
- Comprehensive [documentation](../../wiki/) and test set


Installation
------------

You can install BigSharp via [NuGet](https://www.nuget.org/):

package manager:

    $ PM> Install-Package BigSharp

NET CLI:

	$ dotnet add package BigSharp

or [download source code](../../releases).


Use
-----

*In the code examples below, semicolons and `ToString` calls are not shown.*

The library exports a constructor function, `Big`.

A Big number is created from a primitive number, string, or other Big number.

```csharp
x = new Big(123.4567)
y = new Big("123456.7e-3")
z = new Big(x)
x.Eq(y) && x.Eq(z) && y.Eq(z)          // true
```

In Big strict mode, creating a Big number from a primitive number is disallowed.

```csharp
var bigFactory = new BigFactory(new BigConfig()
{
	STRICT = true
});
x = bigFactory.Big(1)                  // TypeError: [BigSharp] Invalid number
y = bigFactory.Big("1.0000000000000001")
y.ToNumber()                           // Error: [BigSharp] Imprecise conversion
```

A Big number is immutable in the sense that it is not changed by its methods.

```csharp
0.3 - 0.1                              // 0.19999999999999998
x = new Big(0.3)
x.Minus(0.1)                           // "0.2"
x                                      // "0.3"
```

The methods that return a Big number can be chained.

```csharp
x.Div(y).Plus(z).Times(9).Minus("1.234567801234567e+8").Plus(976.54321).Div("2598.11772")
x.Sqrt().Div(y).Pow(3).Gt(y.Mod(z))    // true
```

There are `ToExponential`, `ToFixed` and `ToPrecision` methods.

```csharp
x = new Big(255.5)
x.ToExponential(5)                     // "2.55500e+2"
x.ToFixed(5)                           // "255.50000"
x.ToPrecision(5)                       // "255.50"
```

The arithmetic methods always return the exact result except `Div`, `Sqrt` and `Pow`
(with negative exponent), as these methods involve division.

The maximum number of decimal places and the rounding mode used to round the results of these methods is determined by the value of the `DP` and `RM` properties of the `Big` number factory.

```csharp
var bigFactory = new BigFactory(new BigConfig()
{
	DP = 10,
	RM = RoundingMode.ROUND_HALF_UP
});

x = bigFactory.Big(2);
y = bigFactory.Big(3);
z = x.Div(y)                           // "0.6666666667"
z.Sqrt()                               // "0.8164965809"
z.Pow(-3)                              // "3.3749999995"
z.Times(z)                             // "0.44444444448888888889"
z.Times(z).Round(10)                   // "0.4444444445"
```

The value of a Big number is stored in a decimal floating point format in terms of a coefficient, exponent and sign.

```csharp
x = new Big(-123.456);
x.c                                    // [1,2,3,4,5,6]    coefficient (i.e. significand)
x.e                                    // 2                exponent
x.s                                    // -1               sign
```

For advanced usage, multiple Big number factories can be created, each with an independent configuration.

For further information see the [API](../../wiki/) reference documentation.


System requirements
-------------------

BNSharp supports:

* Net 6


Development and testing
------------------------

Make sure to rebuild projects every time you change code for testing.

### Testing

To run tests:

    $ dotnet test


Contributors
------------

[XeroXP](../../../).
