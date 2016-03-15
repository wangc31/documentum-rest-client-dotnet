ReSTM
	The main project that can be used for model and controller. 

DroidXamarinTest 
	This project uses Xamarin to show how the ReSTMc dll can 
	be used on mobile platforms. To use this project, you must use Xamarin Studio or
	have Xamarin for Visual Studion installed. It is definitely nothing fancy, just a basic
	concept of list cabinets, navigate folders, nothing more.

Tester 
	This project is a console application to show how end to end functions work (the source 
	code here is a great example of how to handle use cases). It has been tested on Windows 
	and Linux and should work on Mac as well.
	
	QuickStart:
		Open the App.Config file
		1. Find: <section name="restconfig" .../> element. If you are using Mono, it should read:
		<section name="restconfig" type="System.Configuration.NameValueSectionHandler,System"/>
		If you are using Visual Studio, it should read:
		<section name="restconfig" type="System.Configuration.NameValueSectionHandler"/>
		Basically, one has a type with a ,System in it and the other doesn't. It is a workaround
		for Mono for now until they fix it, but having this ,System in the config for Visual 
		Studio causes an exception when loading the configuration file.
		2. Go to the <restconfig> section and become familiar with the descriptions and what 
		the parameters do. To get started, all you need do here is set the "randomfilesdirectory" 
		value to a (Windows) [driveletter]:\Path\To\Files directory or (Unix) /Path/To/Files
		The path you choose should have a number of files directly under this directory. The Tester
		will choose a number of files from here at random as samples to upload.
		3. You can set the "randomemailsdirectory" to the same value as "randomfilesdirectory", this
		is not currently used with base Rest services, it is only available with extensions enabled.
		If you need an extension for Rest that imports emails and splits out the attachment files,
		have your account rep contact michael.mccollough@emc.com.
		4. You can update the defaultReSTHomeUri, defaultUsername, defaultRepositoryName, 
		defaultPassword values to the values for your environment.
		5. If you set the "useDefault" parameter to true, it will not prompt you initially to enter the 
		above information. If you set it to false, when you initially launch the Tester it will prompt you 
		but allow you to hit enter to accept the default values you set in this configuration file.
	Kerberos:
		If you Rest service is setup to use Kerberos, you can set the defaultUserName and defaultPassword to "" (blank)
		and the Tester will use your current Windows credentials to login to the repository.
		
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


