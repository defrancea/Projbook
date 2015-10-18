// Update the url when changing tab
$('a[data-toggle="tab"]').click(function (e) {
    document.location = this.href;
});