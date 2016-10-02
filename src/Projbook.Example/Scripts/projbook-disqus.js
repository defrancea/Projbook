function loadDisqus(rootPath, id) {
    // Unload disqus
    unloadDisqus();

    // Reset all sections
    $('.hide-comments').css('display', 'none');
    $('.comment-panel').css('display', 'none');
    $('.show-comments').css('display', 'block');

    // Flip comments action links
    $('#show-comments-' + id).css('display', 'none');
    $('#hide-comments-' + id).css('display', 'block');

    // Create and inject disqus placeholder
    $('#comment-content-' + id).append($('<div>', { id: 'disqus_thread' }));

    // Hide content panel
    $('#comment-panel-' + id).css('display', 'block');

    // Reset disqus
    DISQUS.reset({
        reload: true,
        config: function () {
            this.page.identifier = id;
            this.page.url = rootPath + '#!' + id;
        }
    });
}

function unloadDisqus(id) {
    // Remove disqus thread content
    $('#disqus_thread').remove();

    // Flip comments action links
    $('#hide-comments-' + id).css('display', 'none');
    $('#show-comments-' + id).css('display', 'block');

    // Hide content panel
    $('#comment-panel-' + id).css('display', 'none');
}