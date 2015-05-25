Language Integrated Query
===================

LINQ extends the language by the addition of query expressions, which are akin to SQL statements, and can be used to conveniently extract and process data from arrays, enumerable classes, XML documents, relational databases, and third-party data sources.
[More information](https://msdn.microsoft.com/en-us/library/bb397926.aspx)

----------

First impressions
-------------
Get list of integer whose value is higher than 6.

> With foreach

	List<int> numbers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10,11,12});
	List<int> numbersAboveSix = new List<int>();
	foreach (var number in numbers)
	{
		if (number>6)
		{
			numbersAboveSix.Add(number);
		}
	}
	// numbersAboveSix: 7,8,9,10,11,12
	
> With LINQ(Method syntax)

	List<int> numbers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10,11,12});
	var numbersAboveSix = numbers.Where(number => number > 6);
 	// numbersAboveSix: 7,8,9,10,11,12

> With LINQ(Query syntax)

	List<int> numbers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10,11,12});
	var numbersAboveSix = from number in numbers 
			              where number > 6 
			              select number;
 	// numbersAboveSix: 7,8,9,10,11,12

As you can see LINQ is much cleaner, easier to read. You can choose which method you want to use. The drawback is that LINQ is usually slower with small collections, but as the collection is grew, LINQ will come into the wiev.

----------

Basic LINQ Operations
-------------

### Getting data from source(**Select, First, FirstOrDefault**)

You can get the wanted data from the collection, for example the strings's length. You can use **First** to get the first element, or **FirstOrDefault** to avoid from errors if the collectiond doesn't contains this element. 

	List<string> numbers = new List<string>(new string[] { "One", "Two", "Three", "Four", "Five" });
	
	var numbersLenth = numbers.Select(number => number.Length);
	// numbersLenth : 3,3,5,4,4
	
	var example1 = numbers.Where(number => number.Length>3).First();
	// example1: Three
	
	var example2 = numbers.Where(number => number.Length>7).First();
	// Error: There is no string which length higher than seven 
	
	var example3 = numbers.Where(number => number.Length>7).FirstOrDefault();
	// example3: null
	
	var example4 = numbers.FirstOrDefault(number => number.Length>7);
	// you can simplify the query
	
### Filtering(**Where**)

Probably the most common query operation is to apply a filter in the form of a Boolean expression. You can use multiple conditions to get the data. For example list of integer whose value is higher than 6 OR the number is even.

	List<int> numbers = new List<int>(new int[]{1,2,3,4,5,6,7,8,9,10,11,12});
	var result = numbers.Where(number => number > 6 || number%2==0;
	// result : 2,4,7,8,9,10,11,12
    
### Ordering(**OrderBy, OrderByDescending**)

You can sort the data with any of its property. For example the lenght of any string.

	List<string> numbers = new List<string>(new string[] { "One", "Two", "Three", "Four","Five"});
	var result = numbers.OrderByDescending(number => number.Length);
	// result: "Three", "Four","Five" "One", "Two"
	
### Grouping(**OrderBy, OrderByDescending**)

The group clause enables you to group your results based on a key that you specify. In this example we will separate the number 1-12 to two groups depending it's size.

	// Initial array
	int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
	// Get the groups
	var groups = numbers.GroupBy(number => number > 6);
	// Loop over groups.
	foreach (var group in groups)
	{
		// Display key for group.
		Console.WriteLine("Higher than 6 = {0}:", group.Key);
		foreach (var value in group)
		{
			// Display values in group.
			Console.Write("{0} ", value);
		}
		Console.WriteLine();
	}
	Output:
		Higher than 6 = False:
		1 2 3 4 5 6
		Higher than 6 = True:
		7 8 9 10 11 12
		
### Count specified elements(**Count**)

Count specified elements in the collection, return as an integer.

	int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
	var count = numbers.Count(number => number > 6);
	//count:6
