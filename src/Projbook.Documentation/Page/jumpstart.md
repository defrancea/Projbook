## Set up
1. Create a documentation project
2. Add [Projbook](https://www.nuget.org/packages/Projbook) using nuget
3. Update your pages in markdown format in `Page` folder
4. Update `projbook.json` to match your files:
```json[projbook.json]
```
5. Build the documentation project
6. The generated documentation will be available in your `TargetDir`

## Extract csharp file content
For the following examples we're going to use the following file as snippet source:
```csharp[Code/SampleClass.cs]
```
Extracting file content is doable by simply using a regular code block syntax and add the extraction rule between `[]`.
Extraction rule syntax include the target file, the member to extract and the extraction options.

### File selector
While using code block it's possible to reference a csharp file using `csharp[Path/To/The/File.cs]`.

If nothing else is defined, the whole file content will be extracted:
```csharp[Code/SampleClass.cs]
```
Using csharp as syntax allows to extract specific members (see below), however any syntax could be used for extracting a file content.
It's valid to extract the same file as txt using `txt[Code/SampleClass.cs]` but the syntax highlighting will be lost:
```text[Code/SampleClass.cs]
```

### Member selector
The second part of the extraction rule is the member name, you can either use the raw name or the full qualified name. All of following extraction rule will extract the same content:
* `csharp[Path/To/The/File.cs] SampleClass.Method(string)`
* `csharp[Path/To/The/File.cs] Projbook.Documentation.Code.SampleClass.Method(string)`
```csharp[Code/SampleClass.cs] SampleClass.Method(string)
```

### Aggregate members
Thanks to the partial member matching, in case of ambigous matching Projbook will extract all matching members and stack them up. Here `Method` having overloads, `csharp[Code/SampleClass.cs] Method` will extract all of them, note that the member name is not fully qualified but it could if needed or preferred:
```csharp[Code/SampleClass.cs] Method
```

## Extraction options
During the extraction process Projbook can process snippet content in extracting block structure or the code block. This is doable by adding an option prefix to the extraction rule.

### Block structure
The `=` char represents the top and the bottom of a code block. With `csharp[Code/SampleClass.cs] =SampleClass` Projbook will perform a member extraction and will replace the code content by `// ...`:
```csharp[Code/SampleClass.cs] =SampleClass
```

### Block content
The `-` char represents the content of a code block. With `csharp[Code/SampleClass.cs] -Method(int)` Projbook will perform a member extraction isolating the code content:
```csharp[Code/SampleClass.cs] -Method(int)
```

### Combine with member aggregation
You can combine rules for extracting and processing many member with options. The rule `csharp[Code/SampleClass.cs] =Method` will find any matching member with name `Method`, stack them up and remove the code content by `// ...`:
```csharp[Code/SampleClass.cs] =Method
```

## Extract Xml file content
It is also possible to extract xml content by using XPath as query language, for example, we can export all Import tag in the Projbook's documentation project by using `xml[Projbook.Documentation.csproj] //Import`:
```xml[Projbook.Documentation.csproj] //Import
```

## Source example
Have a look to this documentation pages as example [source](https://github.com/defrancea/Projbook/tree/master/src/Projbook.Documentation/Page)