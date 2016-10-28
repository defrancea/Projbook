# Set up
You can install Projbook to any project but we recommand creating a dedicated documentation project. It will allow you to centralize all the documentation without contaminating the code projects.

First of all, install [Projbook](https://www.nuget.org/packages/Projbook) using nuget in the project you want the documentation generated from. Alternatively, you can install [Projbook.Core](https://www.nuget.org/packages/Projbook.Core) if you don't have any PDF needs.

It will create a default configuration `projbook.json` from where you can configure your documentation project, define your templates and your page content:
```fs[../Projbook.Example] projbook.json
```
```json[../Projbook.Example/projbook.json]
```

You can also find some sample files under `Page`. These are going to be the source of the documentation, it's where you'll spend most of your time writing your documentation referencing your actual code source. The syntax of these is markdown and we'll detail later how to reference and extract your source code.
```fs[../Projbook.Example] *.md
```

Installing a visual studio [Markdown Editor](https://visualstudiogallery.msdn.microsoft.com/eaab33c3-437b-4918-8354-872dfe5d1bfe) will make the markdown writing easier.

Default templates can edited in order to customize your rendering.
```fs[../Projbook.Example] index-template.html|template*.html
```

> Notice that the default template contains some commented code providing a disqus integration. Follow the instructions in the template to enable the same disqus integration as this document.

To generate the documentation you simply need to build the project and find your documentation in your target directory:
```fs[../Projbook.Documentation] index.html|projbook.html
```
> It is possible to skip pdf generation using compilation symbols by using `PROJBOOK_NOPDF` in order to speed up Debug build while keeping it for Release builds.
Since `Projbook.Core` does not including pdf generation dependencies, this symbol will be forced if you choose it instead of `Projbook`.

# Snippet extraction
Projbook extends the markdown syntax in order to define snippet reference. By default, you can specify code block but you need to manually type the content this way:
~~~md
```txt
Some code exmaple
```
~~~

The first syntax extension allows you to leave the content empty but reference a file. During the document project's build it will find the file content and inject it inside the code block:
~~~md
```txt[Path/To/File.txt]
```
~~~

Opionally you can specify a pattern used to extract some part of the referenced. This pattern highly depending on the type of content you extract. We'll detail supported syntax and format later.
~~~md
```txt[Path/To/File.txt] <pattern>
```
~~~

The syntax you define will define two things:
* The syntax highlighting that is going to be applied using [prism.js](http://prismjs.com/)
* The pattern you can apply (will be detailed below)

Ultimately you can extract any text-based content associated with any syntax hightlighting but using syntax-specific extraction pattern will make the snippet extraction really powerful.
As an example, this code block:
~~~md
```csharp[Code/SampleClass.cs] Method(int)
```
~~~

The whole file content being:
```csharp[Code/SampleClass.cs]
```

Will be rendered as extracting the referenced method:
```csharp[Code/SampleClass.cs] Method(int)
```