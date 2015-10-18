param($installPath, $toolsPath, $package, $project)

# Retrieve folders
$contentFolder = $project.ProjectItems.Item("Content")
$scriptsFolder = $project.ProjectItems.Item("Scripts")

# Copy top level files
$project.ProjectItems.Item("projbook.json").Properties.Item("CopyToOutputDirectory").Value = 2
$project.ProjectItems.Item("template.html").Properties.Item("CopyToOutputDirectory").Value = 2
$project.ProjectItems.Item("template-pdf.html").Properties.Item("CopyToOutputDirectory").Value = 2

# Copy files in Content folder
$contentFolder.ProjectItems.Item("prism.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("projbook.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("bootstrap-theme.min.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("bootstrap.min.css").Properties.Item("CopyToOutputDirectory").Value = 2

# Copy files in Scripts folder
$scriptsFolder.ProjectItems.Item("prism.js").Properties.Item("CopyToOutputDirectory").Value = 2
$scriptsFolder.ProjectItems.Item("bootstrap.min.js").Properties.Item("CopyToOutputDirectory").Value = 2
$scriptsFolder.ProjectItems.Item("jquery-1.9.1.min.js").Properties.Item("CopyToOutputDirectory").Value = 2

# Delete files from wkhtmltopdf
$project.ProjectItems.Item("readme.txt").Delete()
$project.ProjectItems.Item("wkhtmltopdf.exe").Remove()