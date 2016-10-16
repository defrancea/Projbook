Extraction pattern are implemented as [projbook plugins](plugins.html). Below is the documentation of the built in ones.

# C# pattern
C# patterns allow referencing any syntax block member like namepsaces, classes, fields, properties, events, indexers, methods. Some options allows wipe block content or extract block content only.
It mostly follow the [cref](https://msdn.microsoft.com/en-us/library/cc837134.aspx) syntax with some extra options and modifications:
* Constructors are matched by `<Constructor>`
* Finalizers are matched by `<Destructor>`
* Indexers are matched by their type like `[int]`
* You can reference events and properties sub blocks by using their name like `ThePropertyName.get` or `TheEventName.add`
* It's possible to match any methods by parameters like `(string, int)` matching any method having these parameters
* The `-` prefix to extract the block content only
* The `=` prefix to extract the block structure only

All example below are using this file content:
```csharp[Code/SampleClass.cs]
```

Member selection will simply apply a pattern matching on the full qualified member name such as you can use either name or a full qualified name to resolve ambiguity.
All of these pattern are equivalent in this context:
* `csharp[Path/To/The/File.cs] SampleClass.Method(string)`
* `csharp[Path/To/The/File.cs] Projbook.Documentation.Code.SampleClass.Method(string)`
* `csharp[Path/To/The/File.cs] (string)`

Will produce:
```csharp[Code/SampleClass.cs] Projbook.Documentation.Code.SampleClass.Method(string)
```

Constructors are matched by `<Constructor>` special name, `csharp[Code/SampleClass.cs] <Constructor>` would extract:
```csharp[Code/SampleClass.cs] <Constructor>
```

Thanks to pattern matching, in case of ambigous matching Projbook will extract all matching members and stack them up. Here `Method` having overloads, `csharp[Code/SampleClass.cs] Method` will extract all of them. Note that the member name is not fully qualified but it could if needed or preferred:
```csharp[Code/SampleClass.cs] Method
```
> This feature is not specific to method name but also apply to parameter extracting all methods having the same parameters and can even be combined with options (see below for more details).

During the extraction process Projbook can process snippet content in extracting block structure or the code block. This is doable by adding an option prefix to the extraction rule.

The `=` char represents the top and the bottom of a code block. With `csharp[Code/SampleClass.cs] =SampleClass` Projbook will perform a member extraction and will replace the code content by `// ...`:
```csharp[Code/SampleClass.cs] =SampleClass
```


The `-` char represents the content of a code block. With `csharp[Code/SampleClass.cs] -Method(int)` Projbook will perform a member extraction isolating the code content:
```csharp[Code/SampleClass.cs] -Method(int)
```

You can combine rules for extracting and processing many member with options. The rule `csharp[Code/SampleClass.cs] =Method` will find any matching member with name `Method`, stack them up and remove the code content by `// ...`:
```csharp[Code/SampleClass.cs] =Method
```

# Xml pattern
It is also possible to extract xml content by using [XPath](https://msdn.microsoft.com/en-us/library/ms256115) as query language, for example, we can export all Import tag in the Projbook's documentation project by using `xml[Projbook.Documentation.csproj] //Import`:
```xml[Projbook.Documentation.csproj] //Import
```

# File system pattern
A special syntax `fs` for **file system** is handled and rendered as a tree, simply uses Projbook references to target a path in the file system with this syntax and it will be rendered as a jstree:
~~~md
```fs[Page]
```
~~~

Being rendered as:
```fs[Page]
```


Patterns can be [file search pattern](https://msdn.microsoft.com/en-us/library/wz42302f(v=vs.110).aspx) such as `*.cs` or `my-file.txt` like this:
~~~md
```fs[.] *.md*
```
~~~

Being rendered as:
```fs[.] *.md*
```

It's also possible to combine patterns using `|` or `;` chars such as the following extraction rule:
~~~md
```fs[.] *.md|*template*.html
```
~~~

Produces:
```fs[.] *.md|*template*.html
```