(() => {
    'use strict';
    const track = document.getElementById('new-products');
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (track) {
        const previous = document.querySelector('.home-slider-prev');
        const next = document.querySelector('.home-slider-next');
        const position = document.getElementById('slider-position');
        const metrics = () => {
            const gap = parseFloat(getComputedStyle(track).gap) || 0;
            const step = track.firstElementChild.getBoundingClientRect().width + gap;
            const visible = Math.max(1, Math.round((track.clientWidth + gap) / step));
            return { step, visible };
        };
        const update = () => {
            previous.disabled = track.scrollLeft <= 2;
            next.disabled = track.scrollLeft + track.clientWidth >= track.scrollWidth - 2;
            const { step, visible } = metrics();
            const first = Math.round(track.scrollLeft / step) + 1;
            position.textContent = `${first}–${Math.min(track.children.length, first + visible - 1)} / ${track.children.length}`;
        };
        const move = direction => {
            const { step, visible } = metrics();
            const current = Math.round(track.scrollLeft / step);
            const target = Math.max(0, Math.min(track.children.length - visible, current + direction * visible));
            track.scrollTo({ left: target * step, behavior: reducedMotion ? 'instant' : 'smooth' });
        };
        previous.addEventListener('click', () => move(-1));
        next.addEventListener('click', () => move(1));
        track.addEventListener('scroll', update, { passive: true });
        track.addEventListener('keydown', event => {
            if (event.target !== track || !['ArrowRight', 'ArrowLeft'].includes(event.key)) return;
            event.preventDefault();
            move(event.key === 'ArrowRight' ? 1 : -1);
        });
        new ResizeObserver(update).observe(track);
        update();
    }
    const form = document.getElementById('newsletter-form');
    form?.addEventListener('submit', async event => {
        event.preventDefault();
        if (!form.reportValidity()) return;
        const button = form.querySelector('button[type="submit"]');
        const message = document.getElementById('newsletter-message');
        button.disabled = true;
        button.textContent = 'KAYDEDİLİYOR…';
        message.textContent = '';
        message.classList.remove('error');
        try {
            const response = await fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'Accept': 'application/json' } });
            if (response.redirected) throw new Error('Oturumunuz sona erdi. Lütfen tekrar giriş yapın.');
            const result = await response.json();
            if (!response.ok) throw new Error(result.message || 'Abonelik kaydedilemedi. Lütfen tekrar deneyin.');
            message.textContent = result.message;
        } catch (error) {
            message.classList.add('error');
            message.textContent = error instanceof SyntaxError || error instanceof TypeError ? 'Bağlantı kurulamadı. Lütfen tekrar deneyin.' : error.message;
        } finally {
            button.disabled = false;
            button.textContent = 'ABONE OL';
        }
    });

    const arrivals = document.querySelector('.home-arrivals');
    if (arrivals && !reducedMotion && !CSS.supports('animation-timeline', 'view()')) {
        let frame = 0;
        const updateArrive = () => {
            frame = 0;
            const rect = arrivals.getBoundingClientRect();
            const viewHeight = window.innerHeight;
            const start = viewHeight;
            const end = viewHeight * 0.42;
            const progress = Math.min(1, Math.max(0, (start - rect.top) / Math.max(1, start - end)));
            arrivals.style.opacity = String(0.1 + progress * 0.9);
        };
        window.addEventListener('scroll', () => {
            if (frame) return;
            frame = requestAnimationFrame(updateArrive);
        }, { passive: true });
        updateArrive();
    } else if (arrivals && reducedMotion) {
        arrivals.style.opacity = '1';
    }
})();
