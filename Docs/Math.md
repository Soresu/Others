Math Operations
===================

Operators are very useful thing in computing value. While writing a program, you need various types of operators to calculate value.
Most of these you had to learn in school, so it shouldn't be so hard

----------

### Arithmetic Operators:



|    Operator  |         Description        |        Example       |
|:------------:|:--------------------------:|:--------------------:|
|      +       | Add numbers                |        6+2=8         |
|      -       |      Subtract numbers      |        6-2=4         |
|      *       |      Multiply numbers      |        6*2=12        |
|      /       |      Divide numbers        |        6/2=3         |
|      %       |      Divide two numbers and returns reminder      |  22%10=2  |


It's important to remember that you always have to check the type of the variable:

```
int a = 7;
int b = 2;

int result1 = a / b;            // result1 = 3;
float result2 = a / b;          // result2 = 3;
float result3 = (float) a / b;  // result3 = 3.5;
```
### Increment and Decrement Operators
You can increase or decrease the variable's number by one with ++ or --

All of these command do the same thing:
```
a = a + 1;
a++;
++a;
```
The operators can be placed before the variable to be adjusted (prefix) or after it (postfix):
```
int a;
int b;
 
// Prefix.  a is incremented before its value is assigned to b
a = 10;
b = ++a;        //a = 11, b = 11;
 
// Postfix.  a is incremented after its value is assigned to b
a = 10;
b = a++;        //a = 11, b = 10;
```

### Math Class

Math is a class in the System namespace. The .NET Framework provides many built-in mathematical methods.
[More information](https://msdn.microsoft.com/en-us/library/system.math%28v=vs.110%29.aspx)

#### Some useful method:

```
// Abs - The Abs method computes absolute values.
var result1 = Math.Abs(10);         // result1 = 10;
var result2 = Math.Abs(-10);        // result1 = 10;

// Max, min - Return the highest or lowest number.
var result3 = Math.Max(5.12);       // result3 = 12;
var result4 = Math.Min(5.12);       // result4 = 5;

// Ceiling, Floor - They compute the next highest or lowest integer around the nuber.
var result5 = Math.Ceiling(12.8);   // result5 = 13;
var result6 = Math.Ceiling(12.2);   // result6 = 13;
var result7 = Math.Floor(12.8);     // result7 = 12;
var result8 = Math.Floor(12.2);     // result8 = 12;

// Sign - It will determine that the number is positive or negative  
// It returns one of three values: -1, 0 or 1.
var result9 = Math.Max(5.5);          // result9  = 1;
var result10 = Math.Min(0);         // result10 = 0;
var result11 = Math.Min(-5.5);        // result11 = -1;
```
You can also find some method what can be familiar from school. Like **Sin**, **Cos**, **Tan**, **Sqrt**, **Pow**, or **PI**
