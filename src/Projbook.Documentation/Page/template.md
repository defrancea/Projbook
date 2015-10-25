## HTML and PDF templates
There are two template file:
* `template.html`: Used as template for HTML generation
* `template-pdf.html`: used as template for PDF generation
Default templates using bootstrap can be directly used as it without any changes but can be modified or entirely rewritte if needed.

If `template-pdf.html` doesn't exist, `template.html` will be used for both.

## Syntax
Templates use html with [Razor syntax](http://www.asp.net/web-pages/overview/getting-started/introducing-razor-syntax-(c) having relevant information about documentation as model.

## Model
The model is usable using the `@Model` variable using razor and contains top level member:
* `Model.Title`: The documentation title from the configuration
* `Model.Pages`: An array of Page (mode details below)

### Pages
The `Page` class contains following members that can be used in templates:
* Id
* Title
* PreSectionContent
* Sections

### Sections
The `Section` class contains following members that can be used in templates:
* Id
* Title
* Content

## Source example
Have a look to this documentation template [source](https://github.com/defrancea/Projbook/blob/master/src/Projbook.Documentation/template.html)