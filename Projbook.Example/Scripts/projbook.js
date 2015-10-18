// Update the url when changing tab
$('a[data-toggle="tab"]').click(function (e) {
    document.location = this.href;
});

// Reset the position in the document after page loading
var hash = document.location.hash.split('#');
if (2 <= hash.length)
{
    // Reset the page
    var pageId = hash[1].split('.');
    $('a[href="#' + pageId[0] + '"]').click();

    // Reset the anchor
    if (2 <= pageId.length)
    {
        $('a[href="#' + hash[1] + '"]')[0].click();
    }
}
