Variable
===================

A variable is a value that can change, depending on conditions or on information passed to the program.

----------

Example
-------------

Lets create a variable that will hold the text that we will display in chat when our assembly has loaded.

	string welcomeText = "Chevy Loaded!";
 
Now, lets break down piece by piece this statement to grasp a better understanding.

> string

This is our type, it is the type of variable we're creating. In this case, it's a string. A string represents a sequence of zero or more Unicode characters. (Find more types [here](https://msdn.microsoft.com/en-us/library/s1ax56ch.aspx). Look on the left side for all of the types, click on one for examples)

  >welcomeText

  This is called the "Identifier"(or the name, basically). We use this name to do other operations on the string.

  >"Chevy Loaded!";

This sets the variable equal to something, in this case "ChevyVadeâ„¢ Loaded!". Note that the quotes wont show up when we use this variable, it just wraps up the text. All strings must start with a quote, and end with one. In addition, **all statements must end with a semi-colon.**

**Good**:

    string train = "ChewChewww";

**Bad**:

    string train = ChewChewww;


We can use these variables for Methods, something we'll get into later, and change their values.

    string train = "ChewChewww";
    Console.WriteLine(train);
    train += " Train";
    Console.WriteLine(train);

First, we declare our variable, labeled train. `Console.WriteLine` writes a line of text to the console. (To see this in-game, enable Developer Console in the loader). Next, we use the `+=` operator, which appends a value **of the same type** to the variable. In this case, it's simply putting ` Train` at the end of the string.

When we run this, the console should display this:   

	ChewChewww
	ChewChewww Train

Some helpful variables:

| Name of Type |         Description        |        Example       |
|:------------:|:--------------------------:|:--------------------:|
|     float    | Any number, with decimals. |   float x = 0.03f;   |
|    boolean   |      True/False only.      | bool lagging = true; |
|      int     |      Any whole number      |     int cats = 0;    |