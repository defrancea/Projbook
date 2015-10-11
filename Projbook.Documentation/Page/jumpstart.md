## Set up
1. Create a documentation project
2. Add [Projbook](https://www.nuget.org/packages/Projbook) using nuget
3. Create your pages in markdown format in `Page` folder
4. Update `projbook.json` file in order to include and order your documentation pages:
```csharp[projbook.json]
```
5. Build the documentation project
6. The generated documentation will be available in your `TargetDir`
7. Add MSBuild event to trigger generation:
```xml
<Target Name="AfterBuild">
  <Projbook
  	ProjectPath="$(ProjectDir)Projbook.Documentation.csproj"
    TemplateFile="$(TargetDir)template.html"
    ConfigurationFile="$(TargetDir)projbook.json"
    OutputDirectory="$(TargetDir)" />
  <Exec
  	Command="$(SolutionDir)packages\wkhtmltopdf.msvc.64.exe.0.12.2.5\tools\wkhtmltopdf.exe $(TargetDir)template-pdf-generated.html $(TargetDir)template-pdf-generated.pdf"
    ContinueOnError="true" />
</Target>
```

## Extract file content
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

### Member selector
The second part of the extraction rule is the member name, you can either use the raw name or the full qualified name. All of following extraction rule will extract the same content:
* `csharp[Path/To/The/File.cs SampleClass.Method(string)]`
* `csharp[Path/To/The/File.cs Projbook.Documentation.Code.SampleClass.Method(string)]`
```csharp[Code/SampleClass.cs SampleClass.Method(string)]
```

### Aggregate members
Thanks to the partial member matching, in case of ambigous matching Projbook will extract all matching members and stack them up. Here `Method` having overloads, `csharp[Code/SampleClass.cs Method]` will extract all of them, note that the member name is not fully qualified but it could if needed or preferred:
```csharp[Code/SampleClass.cs Method]
```

## Extraction options
During the extraction process Projbook can process snippet content in extracting block structure or the code block. This is doable by adding an option prefix to the extraction rule.

### Block structure
The `=` char represents the top and the bottom of a code block. With `csharp[Code/SampleClass.cs =SampleClass]` Projbook will perform a member extraction and will replace the code content by `// ...`:
```csharp[Code/SampleClass.cs =SampleClass]
```

### Block content
The `-` char represents the content of a code block. With `csharp[Code/SampleClass.cs -Method(int)]` Projbook will perform a member extraction isolating the code content:
```csharp[Code/SampleClass.cs -Method(int)]
```

### Combine with member aggregation
You can combine rules for extracting and processing many member with options. The rule `csharp[Code/SampleClass.cs =Method]` will find any matching member with name `Method`, stack them up and remove the code content by `// ...`:
```csharp[Code/SampleClass.cs =Method]
```

## Page source
Have a look to this documentation page [source](https://github.com/defrancea/Projbook/blob/master/Projbook.Documentation/Page/jumpstart.md).