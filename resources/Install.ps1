param($installPath, $toolsPath, $package, $project)

# Retrieve folders
$contentFolder = $project.ProjectItems.Item("Content")
$fontsFolder = $project.ProjectItems.Item("fonts")
$scriptsFolder = $project.ProjectItems.Item("Scripts")

# Copy files in Content folder
$contentFolder.ProjectItems.Item("prism.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("projbook.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("bootstrap-theme.min.css").Properties.Item("CopyToOutputDirectory").Value = 2
$contentFolder.ProjectItems.Item("bootstrap.min.css").Properties.Item("CopyToOutputDirectory").Value = 2

# Copy files in fonts folder
$fontsFolder.ProjectItems.Item("glyphicons-halflings-regular.eot").Properties.Item("CopyToOutputDirectory").Value = 2
$fontsFolder.ProjectItems.Item("glyphicons-halflings-regular.svg").Properties.Item("CopyToOutputDirectory").Value = 2
$fontsFolder.ProjectItems.Item("glyphicons-halflings-regular.ttf").Properties.Item("CopyToOutputDirectory").Value = 2
$fontsFolder.ProjectItems.Item("glyphicons-halflings-regular.woff").Properties.Item("CopyToOutputDirectory").Value = 2

# Copy files in Scripts folder
$scriptsFolder.ProjectItems.Item("prism.js").Properties.Item("CopyToOutputDirectory").Value = 2
$scriptsFolder.ProjectItems.Item("projbook.js").Properties.Item("CopyToOutputDirectory").Value = 2
$scriptsFolder.ProjectItems.Item("bootstrap.min.js").Properties.Item("CopyToOutputDirectory").Value = 2
$scriptsFolder.ProjectItems.Item("jquery-1.9.0.min.js").Properties.Item("CopyToOutputDirectory").Value = 2