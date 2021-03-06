ExpressForms
============

Express Forms Web Development Framework
For Fast and Easy Creation of CRUD Pages

============

ExpressForms-0.8.7 is a minor bugfix release.  It fixes an error where having a GUID column in
the data populating the tables causes the filtering functionality to not work.

============

ExpressForms-0.8.5 adds a small one-line function to the ExpressForms global ef object to allow
for the table object to be retrieved.

This was needed for an application that needed to know how many rows were in the table, but I think
that in the future, I need to create some sort of event system that can be customized.

***

ExpressForms-0.8.4 adds several features.
First of all, the ExpressFormsController now implements two virtual methods, SetupIndex and SetupEditor
that when overridden, allow developers to specify configuration that is specifically for Index or Editor
without having to override the Index or EditNew methods.

It is now possible to refer to IndexButtons in constructor methods in classes that inherit ExpressFormsController.

The IgnoredPropertyNames object has been changed from an array of strings to a list of strings so that
developers can call List methods on it like "Add", "AddRange", and "Clear".

There is now functionality to display a warning message before removing a record.
It works as shown below:

DefaultRemoveButton.GetConfirmationMessageFromRecord = (record, id) => "Are you sure you want to remove " + xslt.Name + "?";

Also, there are two new object references: DefaultRemoveButton and DefaultEditButton that refer to the two default
buttons on the ExpressFormsController object.  These two buttons are still placed in the IndexButtons object,
but these two object references make it easy to refer to them.

***

ExpressForms-0.8.3 fixes a few bugs in the autocomplete for the text filter.

***

I haven't been updating this file regularly, so I missed a few minor revisions.
ExpressForms-0.8.1, released October 7, 2013, fixes an issue where the ExpressFormsDropDownList doesn't default
to the current value for a record.

ExpressForms-0.8.0 features an extensive rewrite of the code that displays the buttons on the
index page.  This fixed some of the issues where only a 32-bit integer could be used for ID
because it was hardcoded to use a 32-bit integer in this code.  I haven't extensively tested
with other ID types, but they should work soon!  The main feature here is a new abstract
function called InitializeWithRecord on the ExpressFormsButton object.  This function gets called
and passed each record on the index page, so the appearance and behavior of the button can
be customized.

***

ExpressForms-0.7.1, released September 25, 2013, refines the filtering capability a bit and adds
autocomplete to the text selections.  In the next release, I plan to add the functionality to
turn the autocomplete on and off, as desired, plus the ability to plug in customized filters.

***

ExpressForms-0.7.0, released September 15, 2013 adds in sophisticated filtering with AJAX data.
I had intended to expand the AJAX support to multiple extensions, but that's going to have to wait
for another time.  I need the filtering for a project I'm working on, so that's the priority.

"Filters" describe two things.  Given a data type, they describe a form that the user can fill out
to describe how to filter the data, and they also have functionality to describe that logic in code.
Unfortunately, given how the structure of your data is unknown to ExpressForms, the DynamicMethod
object is used to generate CIL code that reads the properties of your classes at runtime.  Reflection
is slow and incompatible with Entity Framework, which I use a lot.

In the next release, I plan to give "Filters" the same customizability that "Inputs" have.

***

ExpressForms-0.6.0, released August 16, 2013, adds in the first AJAX support to ExpressForms.
After all, ExpressForms would be only a simple toy without the ability to have an AJAX framework
page through large datasets.

Right now, I've written an extension that allows ExpressForms to work with jQuery DataTables
and provided some example pages.  What I find exciting about this release is that filtering,
sorting, and pagination are all automatically provided.  In a future version, I want to provide
the ability for a developer to override the filtering and sorting methods for each column in the
same way that ExpressForms provides the ability to specify custom column headers and data display.
Some of this code uses reflection, so that's one opportunity for improvement.

My plan is that the next release of ExpressForms should provide for easy switching between different
AJAX frameworks.

Unfortunately, the limitation that tables must have a 32-bit integer column called "Id" for the
primary key remains for now.

***

ExpressForms-0.5, released August 7, 2013, is the first publicly released version of Express Forms.
It is realeased under the MIT license.  You are encouraged to contribute to it if you like.

The basic idea of Express Forms is to automate common aspects of the development of data-driven web applications.
Most of these applications could be described as "CRUD" applications; having four basic operations:
Create, Read, Update, and Delete.  Express Forms makes it easy to stand up web pages that provide the user with 
access to these operations.  Using Express Forms instad of designing these pages by hand is meant to make
.net web development more DRY, and thus faster and less error-prone.

This may sound somewhat like ASP.net MVC scaffolding, but I want to provide a much richer set of features
so that it can be used in production systems.

The starting point for working with Express Forms is the creation of two classes.  One inherits "ExpressFormsController"
and the other implements IExpressFormsExchange.

The ExpressFormsController class you derive has two type parameters: a class type and an Id type.  The class type
is the class whose properties will be represented on the form.  The Id type is meant to be a primitive type
describing an identifier for the data, but in this early version, only 32-bit integers are supported; furthermore,
the name of this property *must* be Id; this is hard-coded in a number of places; to be fixed in a later version.

The ExpressForms.Entities project provides an example of how ExpressFormsController can be used with Entity Framework.
Note how this class is inherited in the ExpressForsmExample project.  This is a good example of how I envision this
class to be used.  Note that although you can use Entity Framework with Express Forms, the framework is not tightly
coupled with Entity Framework.

ExpressFormsController exposes three public methods that the web browser can use: Index, Editor, and Postback.
Index renders a form for the user to browse records.  In this early version, the records are all rendered into a single
table, but in a later version, this is expected to be fully customizable so that AJAX frameworks can be used
to provide a rich interface.

Editor is renders a form for the user to create and update records.  At this time, it is much more advanced than Index.
Editor may be customized to render the form in any way imaginable.  The primary feature that has not been implemented here
is the ability to work with more than one table at a time in Entity Framework.  I expect that to be the trickiest feature
of all to implement.

Postback provides a single point of entry for the browser to update the data.  It is meant to work with both AJAX and form
posts, but the AJAX functionality is much more mature.

Next is the IExpressFormsExchange.  It is an interface where the methods to update the data are defined.  The ExpressForms.Entities
project defines a generic Entity Framework implementation.  It works very well with classes that represent the contents of a
single table.  However, you'll find lots of commented out code from where I tried to make it work with multiple tables
at once.  The generic Entity Framework exchange class seems to work very well, but it uses reflection liberally, and once again,
it assumes that the identifier for the row is a 32-bit integer called Id.

I think that a future version should perhaps be able to generate Exchange implementations for entity classes with no reflection.

Express Forms provides a single set of Javascript files that read and write data from forms.  It is designed to be
extensible so that you can easily add your own functionality in a logical way.


Here is a partial list of what I see as being critical features to implement in the future:

1) User validation input.  Scaffolding provides this; we should, too!

2) Extensibility so that developers can specify their own event handlers for various things, such as confirmation dialogs

3) The Index view needs to be flexible and work with AJAX frameworks; not just write a plain table.

4) Express Forms should be able to quickly stand up pages that work with data from two tables.


One final thing: Express Forms is not about writing code; it's about *not* writing code.

Therefore, the less code in this project, the better!