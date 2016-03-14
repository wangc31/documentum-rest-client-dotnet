ReSTMc is the main project that can be used for model and controller. The DroidXamarinTest project uses Xamarin to
show how the ReSTMc dll can be used on mobile platforms and the Tester project is a console application to show 
how end to end functions work (the source code here is a great example of how to handle use cases). It has been 
tested on Windows and Linux and should work on Mac as well.

To run the Tester program, edit the properties in the App.config file. Specifically, the 
location to get random files from. If you have a directory of hundreds of random files, the 
tester will choose the number of documents you specify (at runtime) at random from the directory.

You can ignore the random emails directory, this test will not be run unless you have the ReST 
extensions I have developed for importing email (splitting off attachments like WDK does). If you 
have TCS installed, the loader can also detect and report duplicate file imports and give you
options to deal with them. Actually, most of this is at the controller project level. 

The Tester project is meant to be an example of how one might you the ReSTModelAndController to
do programming work against it. I hope to follow this up by getting a java Model and Controler
project up to comparable functional level. I am hoping to find a way to maintain multiple language 
code bases and am open to suggestions if you have any.

I did a first round of work exposing the Model and Controller dlls as COM (for use in Office VBA, Python, 
or any other COM aware language). The whole project has been tested under Windows and under Linux (using Mono).
The project compatability reports it is also compatible with .NetCore so it should work on Mobile as well. This
would include iOS, Droid, and Windows phones.

I have provided a sample project for using the ReSTMc project with Xamarin on Droid. It is definitely nothing
fancy, it just lists cabinets and allows you to navigate folders. I am new to Xamarin and mobile so perhaps
someone can quickly turn it into a nice mobile app.

