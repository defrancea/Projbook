# Extractor plugins
All extractors are actually plugins that are bound to a syntax. Projbook engine will discover, load and callback snippets while processing snippet extraction.

In order to write a plugin you need to install [Projbook.Extension](https://www.nuget.org/packages/Projbook.Extension/) from nuget after what you can implement the plugin interface:
```csharp[Projbook/Extension/Spi/ISnippetExtractor.cs] ISnippetExtractor
```

You can reuse the default extractor implementation that will take case of the content loading and let you focus on your plugin:
```csharp[Projbook/Extension/CSharp/CSharpSnippetExtractor.cs] =CSharpSnippetExtractor
```
> This plugin will be trigerred every time a code snippet is using the `csharp` syntax.

The `TargetType` will indicate Projbook what kind of validation needs to be applied on the snippet like file or folder existence and error reporting:
```csharp[Projbook/Extension/Spi/TargetType.cs]
```

While implementing an extractor plugin you return an implementation of:
```csharp[Projbook/Extension/Model/Snippet.cs] =Snippet
```

When extracting text-based snippet like source code, you need to use the `PlainTextSnippet` implementation wraping the snippet content it will be injected in the code block:
```csharp[Projbook/Extension/Model/PlainTextSnippet.cs] Text
```

When extracting tree-based snippets like file system, you need to use the `NodeSnippet` implementation wraping the tree-based structure and will be rendered using jstree:
```csharp[Projbook/Extension/Model/NodeSnippet.cs] Node
```

Plugins are loaded with [MEF](https://msdn.microsoft.com/en-us/library/dd460648(v=vs.110).aspx) from the plugins directory:
```fs[../packages/Projbook.1.1.0-cr1] *Extractor.dll*
```
> All plugins dependencies need to be packaged at the same place

Look at [CSharp](https://github.com/defrancea/Projbook/tree/master/src/Projbook.Extension.CSharpExtractor), [Xml](https://github.com/defrancea/Projbook/tree/master/src/Projbook.Extension.XmlExtractor) or [FileSystem](https://github.com/defrancea/Projbook/tree/master/src/Projbook.Extension.FileSystemExtractor) plugin source code for a full and detailed example.