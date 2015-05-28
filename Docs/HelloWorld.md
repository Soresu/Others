Your first program - Hello World
===================

Traditionally the first program you write in any programming language is called 
a “Hello World” program – a program that simply outputs Hello World to your terminal. 

----------

Creating the project
-------------

First, open Visual Studio and create a new console application.

1. On the **File** menu, point to **New**, and then click **Project**.

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/1.jpg "Step one")

2. Select the **Visual C#** tab, then the **Console Application**

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/2.jpg "Step two") 

Recommended to name the project without any special character, it can cause some problem.
There shouldn't be any special character in the location path also.

3. After clicking the **Ok** buton we will see the default template.
In our case we have:

```
// With double slash you can write comments

// The using keyword is used to include the other namespaces in the program and use their classes
using System; // For this project, we only need this, but you can keep the others
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Namespace declaration
namespace HelloWorld
{
    // Class
    class Program
    {
        // Main method
        static void Main(string[] args)
        {
            //Our code will be here
        }
    }
}
```
A C# program usually consists of the following parts:

* Namespace declaration
  *  A namespace is a collection of classes. The HelloWorld namespace contains the class Program.
* A class
  * The class Program contains the data and method definitions that your program uses. Classes generally contain multiple methods. Methods define the behavior of the class. However, the Program class has only one method Main.
* Class methods
* Class attributes
* A Main method
  * This is the entry point for all C# programs. The Main method states what the class does when executed.
* Statements and Expressions
* Comments

When you run the program it will start the Main method. 

4. To write "Hello world" to the console, write the following line in it.

```
  Console.WriteLine("Hello World");
```
The System namespace contains the Console class and the WriteLine method.

If you would run this program now, after the console shows up, it would close immediately after programe write the text. To avoid this problem add the fallowing ines too. It will wait until you press a key.
```
  Console.WriteLine("Hello World");
  Console.ReadKey();
```
You should see this.
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World");
            Console.ReadKey();
        }
    }
}
```
5. Next step, you have to build your project, it will show you if there is any error in the project.

Click on the **Bulid** tab, then **Build Solution**

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/3.jpg "Step five") 

If you did everything fine, it should build the project successfully. You can check it in the **Output window**.

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/3_5.jpg "Step five2")

Finally to run the project, go to the **Debug tab** then click on **Start Debugging** or **Press F5**.
You can run the .exe file also, the path is in the **Output window**

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/3_6.jpg "Step Omega")

And of course you can add this project to the LeagueSharp too. But don't forget to enable the debug console in the options.

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/LS.jpg "Step LeaguesSharp")

![alt text](https://raw.githubusercontent.com/Soresu/Others/master/Images/Docs/Hello_World/4.jpg "Step Console")

## Congratulations for your first program.

