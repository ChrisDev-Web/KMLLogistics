(function () {
    'use strict';

    var navToggle = document.getElementById('landingNavToggle');
    var navPanel = document.getElementById('landingNavPanel');
    var navLinks = document.querySelectorAll('.landing-nav__link[data-section]');
    var sections = document.querySelectorAll('#inicio, #sobre-nosotros, #servicios');

    function closeMobileNav() {
        if (!navPanel || !navToggle) return;
        navPanel.classList.remove('is-open');
        navToggle.setAttribute('aria-expanded', 'false');
        navToggle.querySelector('i').className = 'bi bi-list';
    }

    function openMobileNav() {
        if (!navPanel || !navToggle) return;
        navPanel.classList.add('is-open');
        navToggle.setAttribute('aria-expanded', 'true');
        navToggle.querySelector('i').className = 'bi bi-x-lg';
    }

    if (navToggle && navPanel) {
        navToggle.addEventListener('click', function () {
            if (navPanel.classList.contains('is-open')) closeMobileNav();
            else openMobileNav();
        });
    }

    navLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            closeMobileNav();
            document.querySelectorAll('.landing-nav__link[data-section]').forEach(function (l) {
                l.classList.remove('is-active');
            });
            document.querySelectorAll('.landing-nav__link[data-section="' + link.getAttribute('data-section') + '"]').forEach(function (l) {
                l.classList.add('is-active');
            });
        });
    });

    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            var targetId = anchor.getAttribute('href');
            if (!targetId || targetId === '#') return;
            var target = document.querySelector(targetId);
            if (!target) return;
            e.preventDefault();
            var headerOffset = 80;
            var top = target.getBoundingClientRect().top + window.pageYOffset - headerOffset;
            window.scrollTo({ top: top, behavior: 'smooth' });
        });
    });

    if ('IntersectionObserver' in window && sections.length) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var id = entry.target.id;
                navLinks.forEach(function (link) {
                    link.classList.toggle('is-active', link.getAttribute('data-section') === id);
                });
                document.querySelectorAll('.landing-nav__link[data-section="' + id + '"]').forEach(function (link) {
                    link.classList.add('is-active');
                });
                document.querySelectorAll('.landing-nav__link[data-section]').forEach(function (link) {
                    if (link.getAttribute('data-section') !== id) link.classList.remove('is-active');
                });
            });
        }, { rootMargin: '-40% 0px -50% 0px', threshold: 0 });

        sections.forEach(function (section) { observer.observe(section); });
    }

    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) closeMobileNav();
    });

    var valueModal = document.getElementById('valueModal');
    var valueModalTitle = document.getElementById('valueModalTitle');
    var valueModalText = document.getElementById('valueModalText');
    var valueModalIcon = document.getElementById('valueModalIcon');

    function openValueModal(title, desc, iconHtml) {
        if (!valueModal || !valueModalTitle || !valueModalText) return;
        valueModalTitle.textContent = title;
        valueModalText.textContent = desc;
        if (valueModalIcon && iconHtml) valueModalIcon.innerHTML = iconHtml;
        valueModal.classList.add('is-open');
        valueModal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';
    }

    function closeValueModal() {
        if (!valueModal) return;
        valueModal.classList.remove('is-open');
        valueModal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';
    }

    document.querySelectorAll('.value-card').forEach(function (card) {
        card.addEventListener('click', function () {
            var iconEl = card.querySelector('.value-card__icon');
            openValueModal(
                card.getAttribute('data-value-title') || '',
                card.getAttribute('data-value-desc') || '',
                iconEl ? iconEl.innerHTML : ''
            );
        });
    });

    document.querySelectorAll('[data-dismiss="value-modal"]').forEach(function (el) {
        el.addEventListener('click', closeValueModal);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && valueModal && valueModal.classList.contains('is-open')) {
            closeValueModal();
        }
    });
})();
