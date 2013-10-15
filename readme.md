Samson - a simple object mapper for Ministry Platform
========================================

What is this?
--------
Samson is a simple object mapper for [Ministry Platform](http://www.ministryplatform.com) 
that helps abstract away the low-level stuff and gets you back to 
writing applications.

Why?
--------
Ministry Platform's current API forces you to write requests to it in a 
query string like fashion (i.e. "field1=value1&field2=value2") and returns DataSets. 
This tool helps you send requests as objects and returns records
as objects as well without having to write out requests as explicit strings.

Show Me the Code
------------

	var dataContext = new MinistryPlatformDataContext();

	//get a congregation
	var models = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetAllCongregations");  
	
	foreach(var item in models)
	{
		//do something here
	}

	//update a congregation
	//compare old and new objects OR use built-in change tracking
    var model = dataContext.ExecuteStoredProcedure<Congregation>("api_Example_GetSingleCongregation", new { CongregationID = id }).SingleOrDefault();
	model.Description = "This is a new description";
	//updates ONLY the description field
    dataContext.Update<Congregation>(model);
	
	//create a new congregation
	var newModel = new Congregation{		
				Congregation_Name = "New Congregation",
                Description = "Description",
                Start_Date = DateTime.Now,
                Accounting_Company_ID = 1
	};

	dataContext.Create<Congregation>(newModel);

Other Features
---------
Samson also has methods that map to the most commonly used methods of the Ministry
Platform API. It's essentially an advanced wrapper for the API and 
can replace it almost entirely. Most commonly used methods have been built out.

How it Works
---------------
Samson uses class attributes and reflection to determine which class properties are to be 
serialzed, which is the primary key, and what name to map to the database.
The only requirement is that a class inherits from the provided "BaseModel"
class.

You don't neccesarily need to hand write every class. To help get your project moving even
faster, Samson.Models has a T4 template that reads all the tables in your Ministry
Platform instance (using a database connection string you provide). In order to use
the change tracking update method, you will have to use the auto-generated classes,
but it's not a requirement.

How to Install
--------------
Download the Samson.Core and Samson.Model projects. Before you run the T4 template
and compile and run, make sure that any application that uses Samson has the following in it's 
app.config or web.config file

	<system.serviceModel>
		<bindings>
			<basicHttpBinding>
				<binding name="apiSoap" />
			</basicHttpBinding>
		</bindings>
		<client>
			<endpoint address="http://yourservername.com/ministryplatform/public/api.asmx" binding="basicHttpBinding" bindingConfiguration="apiSoap" contract="MinistryPlatform.apiSoap" name="apiSoap" />
		</client>
	</system.serviceModel>

If you want to provide application-wide configuration for Samson, use this:

    <add key="mpguid" value="your-api-key" />
    <add key="mppw" value="your-api-password" />
    <add key="mpserver" value="servername" />

There are provided pre-generated classes that *should* work with any default Ministry 
Platform database. But there's no gaurantees of course :)

###Thanks
MVC Sample provided in most by [twitter.bootstrap.mvc](https://github.com/erichexter/twitter.bootstrap.mvc)
and T4 template provided in most by [Dapper.SimpleCRUD.ModelGenerator](https://github.com/ericdc1/Dapper.SimpleCRUD/tree/master/Dapper.SimpleCRUD.ModelGenerator)