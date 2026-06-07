(function () {
    var contentFrame = null;
    var isLoading = false;
    var faviconUrl = '/favicon.png';

    function ensureFavicon() {
        var icon = document.getElementById('kml-favicon');
        if (!icon) {
            icon = document.createElement('link');
            icon.id = 'kml-favicon';
            icon.rel = 'icon';
            icon.type = 'image/png';
            document.head.appendChild(icon);
        }

        var absoluteUrl = window.location.origin + faviconUrl;
        if (icon.href !== absoluteUrl) {
            icon.href = faviconUrl;
        }
    }

    function initDashboardFilters() {
        $('.filter-pill').off('click.dashboard').on('click.dashboard', function () {
            var filter = $(this).data('filter');
            $('.filter-pill').removeClass('active');
            $(this).addClass('active');

            if (filter === 'todos') {
                $('.module-card').show();
            } else {
                $('.module-card').each(function () {
                    $(this).toggle($(this).data('category') === filter);
                });
            }
        });
    }

    function updateSidebarActive() {
        var path = window.location.pathname.toLowerCase();
        document.querySelectorAll('.sidebar-nav a').forEach(function (link) {
            link.classList.remove('active');
            var href = (link.getAttribute('href') || '').toLowerCase().split('?')[0];
            if (!href || href === '#') return;
            if (path === href || path.indexOf(href + '/') === 0) {
                link.classList.add('active');
            }
        });
    }

    function onDashboardReady() {
        ensureFavicon();
        updateSidebarActive();
        initDashboardFilters();
    }

    function isSharedAsset(hrefOrSrc) {
        if (!hrefOrSrc) return true;
        return hrefOrSrc.indexOf('jquery') !== -1 ||
            hrefOrSrc.indexOf('dataTables') !== -1 ||
            hrefOrSrc.indexOf('dashboard.js') !== -1 ||
            hrefOrSrc.indexOf('dashboard.css') !== -1 ||
            hrefOrSrc.indexOf('bootstrap-icons') !== -1;
    }

    function injectPageStyles(doc) {
        document.querySelectorAll('link[data-page-style]').forEach(function (el) {
            el.remove();
        });

        doc.querySelectorAll('head link[rel="stylesheet"]').forEach(function (link) {
            var href = link.getAttribute('href');
            if (!href || isSharedAsset(href)) return;

            var pageLink = document.createElement('link');
            pageLink.rel = 'stylesheet';
            pageLink.href = href;
            pageLink.setAttribute('data-page-style', 'true');
            document.head.appendChild(pageLink);
        });
    }

    function runPageScripts(doc, done) {
        document.querySelectorAll('script[data-page-script]').forEach(function (el) {
            el.remove();
        });

        var scripts = Array.from(doc.querySelectorAll('body script')).filter(function (script) {
            var src = script.getAttribute('src');
            return !(src && isSharedAsset(src));
        });

        function runNext(index) {
            if (index >= scripts.length) {
                if (typeof done === 'function') done();
                return;
            }

            var script = scripts[index];
            var src = script.getAttribute('src');
            var newScript = document.createElement('script');
            newScript.setAttribute('data-page-script', 'true');

            if (src) {
                newScript.src = src;
                newScript.onload = function () { runNext(index + 1); };
                newScript.onerror = function () { runNext(index + 1); };
                document.body.appendChild(newScript);
            } else {
                newScript.textContent = script.textContent;
                document.body.appendChild(newScript);
                runNext(index + 1);
            }
        }

        runNext(0);
    }

    function loadDashboardPage(url, pushState) {
        if (!contentFrame || isLoading) return;

        isLoading = true;
        fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            credentials: 'same-origin'
        })
            .then(function (response) {
                if (!response.ok) throw new Error('Error de carga');
                return response.text();
            })
            .then(function (html) {
                var parser = new DOMParser();
                var doc = parser.parseFromString(html, 'text/html');
                var frame = doc.getElementById('dashboard-content');

                if (!frame) {
                    window.location.href = url;
                    return;
                }

                injectPageStyles(doc);
                contentFrame.innerHTML = frame.innerHTML;

                if (pushState) {
                    history.pushState({ dashboard: true }, '', url);
                }

                runPageScripts(doc, function () {
                    document.dispatchEvent(new CustomEvent('dashboard:contentLoaded'));
                    onDashboardReady();
                });
            })
            .catch(function () {
                window.location.href = url;
            })
            .finally(function () {
                isLoading = false;
            });
    }

    function isDashboardNavLink(link) {
        if (!link || !link.href) return false;
        if (link.dataset.noAjax === 'true') return false;
        if (link.target === '_blank') return false;
        if (link.origin !== window.location.origin) return false;

        return link.closest('.dashboard-sidebar') !== null ||
            link.closest('#dashboard-content') !== null;
    }

    function setupAjaxNavigation() {
        document.addEventListener('click', function (event) {
            var link = event.target.closest('a[href]');
            if (!link || link.id === 'headerUserTrigger') return;
            if (!isDashboardNavLink(link)) return;

            event.preventDefault();

            document.querySelectorAll('.header-user-dropdown.is-open').forEach(function (el) {
                el.classList.remove('is-open');
                el.querySelector('#headerUserTrigger')?.setAttribute('aria-expanded', 'false');
            });

            loadDashboardPage(link.href, true);
        });

        document.addEventListener('click', function (event) {
            var trigger = event.target.closest('#headerUserTrigger');
            var dropdown = event.target.closest('.header-user-dropdown');

            if (trigger) {
                event.stopPropagation();
                var container = trigger.closest('.header-user-dropdown');
                var isOpen = container.classList.contains('is-open');
                document.querySelectorAll('.header-user-dropdown.is-open').forEach(function (el) {
                    el.classList.remove('is-open');
                    el.querySelector('#headerUserTrigger')?.setAttribute('aria-expanded', 'false');
                });
                if (!isOpen) {
                    container.classList.add('is-open');
                    trigger.setAttribute('aria-expanded', 'true');
                }
                return;
            }

            if (!dropdown) {
                document.querySelectorAll('.header-user-dropdown.is-open').forEach(function (el) {
                    el.classList.remove('is-open');
                    el.querySelector('#headerUserTrigger')?.setAttribute('aria-expanded', 'false');
                });
            }
        });

        window.addEventListener('popstate', function (event) {
            if (event.state && event.state.dashboard) {
                loadDashboardPage(window.location.href, false);
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        ensureFavicon();
        contentFrame = document.getElementById('dashboard-content');
        if (!contentFrame) return;

        setupAjaxNavigation();
        history.replaceState({ dashboard: true }, '', window.location.href);
        onDashboardReady();
    });
})();
