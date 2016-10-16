Default templates are using bootstrap can be directly used as it without any changes but can be modified or entirely rewritte if needed.
Templates use html with [Razor syntax](https://www.asp.net/web-pages/overview/getting-started/introducing-razor-syntax-c) having relevant information about documentation as model.

# HTML and PDF templates
There are two template file per generation configuration that you can define in `projbook.json`:
* `template-html`: Used as template for HTML generation.
* `template-pdf`: Used as template for PDF generation.

By default the generated files are going to have the same name as the template one with the `-generated` sufix but you can define your own using:
* `output-html`: The output file for `template-html`
* `output-pdf`: The output file for `template-pdf`
> You can use html template, pdf or both but at least one must be definied.

The `@Model` variable contains two top level member:
* `Model.Title`: The documentation title from the configuration
* `Model.Pages`: An array of Page (mode details below)

Here is the `Page` class members exposed in the template templates:
```csharp[Projbook/Core/Model/Page.cs] Page.Id
```
```csharp[Projbook/Core/Model/Page.cs] Page.Title
```
```csharp[Projbook/Core/Model/Page.cs] Page.PreSectionContent
```
```csharp[Projbook/Core/Model/Page.cs] Page.Sections
```

Here is the `Section` class members exposed in the template templates:
```csharp[Projbook/Core/Model/Section.cs] Section.Id
```
```csharp[Projbook/Core/Model/Section.cs] Section.Level
```
```csharp[Projbook/Core/Model/Section.cs] Section.Title
```
```csharp[Projbook/Core/Model/Section.cs] Section.Content
```

# Index template
The home page is generated using its own template that you can change using the following properties:
* `template`: The index template with `index-template/html` as a default value.
* `output`: The output file name with `index.html` as a default value.

From this template you can use `@Model.IndexConfiguration` containing the following member:
```csharp[Projbook/Core/Model/Configuration/IndexConfiguration.cs] IndexConfiguration.Title
```
```csharp[Projbook/Core/Model/Configuration/IndexConfiguration.cs] IndexConfiguration.Description
```
```csharp[Projbook/Core/Model/Configuration/IndexConfiguration.cs] IndexConfiguration.Icon
```
```csharp[Projbook/Core/Model/Configuration/IndexConfiguration.cs] IndexConfiguration.Configurations
```

Each Configuration configurations contains:
```csharp[Projbook/Core/Model/Configuration/Configuration.cs] Configuration.Title
```
```csharp[Projbook/Core/Model/Configuration/Configuration.cs] Configuration.Description
```
```csharp[Projbook/Core/Model/Configuration/Configuration.cs] Configuration.Icon
```

> Projbook default templates use a ready to go preset bootstrap-based content but you're free to edit template to matches your needs or your project theme.