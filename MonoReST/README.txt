ReSTMc is the main project that can be used for model and controller. 

The DroidXamarinTest project uses Xamarin to show how the ReSTMc dll can 
be used on mobile platforms. To use this project, you must use Xamarin Studio or
have Xamarin for Visual Studion installed. It is definitely nothing fancy, just a basic
concept of list cabinets, navigate folders, nothing more.

The Tester project is a console application to show 
how end to end functions work (the source code here is a great example of how to handle use cases). It has been 
tested on Windows and Linux and should work on Mac as well.

To run the Tester program, edit the properties in the App.config file. Specifically, the 
location to get random files from. If you have a directory of hundreds of random files, the 
tester will choose the number of documents you specify (at runtime) at random from the directory.

You can ignore the random emails directory, this test will not be run unless you have the ReST 
extensions I have developed for importing email (splitting off attachments like WDK does). If you 
have TCS installed, the loader can also detect and report duplicate file imports and give you
options to deal with them. In some cases, duplication is unavoidable; you need same file in 
different locations with different security. But having duplicate detection lets you decide
what to do, as a programmer, with the duplicate data.

The Tester project is meant to be an example of how one might use the ReSTMc library. The UseCaseTests.cs
class should be very good for mining use case code from for other projects.

I did a first round of work exposing the Model and Controller dlls (ReSTMc project) as COM (for use in Office VBA, Python, 
or any other COM aware language). The whole project has been tested under Windows and under Linux (using Mono).
The project compatability reports it is also compatible with .NetCore so it should work on Mobile as well. This
would include iOS, Droid, and Windows phones.


