// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

let cart = JSON.parse(sessionStorage.getItem('naif_catalog_cart')) || [];
let pdfCanceled = false;

function catalogAddButtonHtml() {
    const isEnglish = document.getElementById('currentLangText')?.textContent?.trim() === 'EN';
    return '<i class="fas fa-cart-plus"></i> <span class="notranslate" translate="no" data-cart-button-text>' + (isEnglish ? 'Add to cart' : 'Sepete Ekle') + '</span>';
}

window.logCatalogAction = function(actionType, item) {
    if (!item) return;
    let specs = [item.category, item.ayar, item.renk, item.gram, item.price, `Adet: ${item.quantity || 1}`, item.note ? `Not: ${item.note}` : ''].filter(Boolean).join(' | ');
    fetch('/Home/LogCatalogAction', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({
            actionType: actionType,
            productId: item.productId || null,
            productCode: item.code || '',
            details: specs,
            quantity: item.quantity || 1,
            note: item.note || null
        })
    }).catch(function() { /* Log hatası müşterinin sepet işlemini engellememeli. */ });
};

function decodeCatalogText(value) {
    let textarea = document.createElement('textarea');
    textarea.innerHTML = String(value || '');
    return textarea.value;
}

function normalizeCatalogImageUrl(value) {
    if (!value) return '';
    try {
        let url = new URL(decodeCatalogText(value), window.location.href);
        // Yerel katalog resmini mevcut site adresinden yükleyerek canvas erişimini koru.
        if (url.pathname.toLowerCase().includes('/images/katalog/')) {
            return url.pathname + url.search;
        }
        return url.href;
    } catch (_) {
        return decodeCatalogText(value);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    updateCartUI();

    // ===== SEARCH BAR LOGIC =====
    const searchInput = document.getElementById('searchInput');
    const searchClearBtn = document.getElementById('searchClearBtn');
    const searchForm = document.getElementById('searchForm');

    if (searchInput) {
        // Show/hide clear button based on input value
        function toggleClearBtn() {
            if (searchInput.value.trim().length > 0) {
                searchClearBtn.classList.add('visible');
            } else {
                searchClearBtn.classList.remove('visible');
            }
        }
        toggleClearBtn(); // Initial state

        searchInput.addEventListener('input', function() {
            toggleClearBtn();
        });

        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                searchForm.submit();
            }
            if (e.key === 'Escape') {
                searchInput.value = '';
                toggleClearBtn();
                searchInput.blur();
            }
        });

        searchClearBtn.addEventListener('click', function() {
            searchInput.value = '';
            toggleClearBtn();
            searchInput.focus();
            // If there was an active search, clear it by navigating
            if (window.location.search.includes('search=')) {
                window.location.href = searchForm.action;
            }
        });
    }

    // Zoom Logic
    document.querySelectorAll('.img-zoom-container').forEach(container => {
        let img = container.querySelector('.zoom-img');
        
        container.addEventListener('mousemove', function(e) {
            let rect = container.getBoundingClientRect();
            let x = e.clientX - rect.left;
            let y = e.clientY - rect.top;
            
            let xPercent = Math.max(0, Math.min(100, (x / rect.width) * 100));
            let yPercent = Math.max(0, Math.min(100, (y / rect.height) * 100));
            
            img.style.transformOrigin = `${xPercent}% ${yPercent}%`;
            img.style.transform = "scale(3)";
        });
        
        container.addEventListener('mouseleave', function() {
            img.style.transform = "scale(1)";
            img.style.transformOrigin = "center center";
        });

        // Click to open lightbox
        container.addEventListener('click', function() {
            img.style.transform = "scale(1)";
            img.style.transformOrigin = "center center";

            // Collect all images from this product's carousel
            let card = container.closest('.card') || container.closest('.col-md-4');
            let allImgs = card ? Array.from(card.querySelectorAll('.zoom-img')).map(i => i.src) : [img.src];
            lightboxImages = allImgs;
            lightboxIndex = allImgs.indexOf(img.src);
            if (lightboxIndex < 0) lightboxIndex = 0;

            let lightboxImg = document.getElementById('lightboxImage');
            lightboxImg.src = lightboxImages[lightboxIndex];
            updateLightboxArrows();

            // Product data for add-cart button
            let addBtn = card ? card.querySelector('.btn-add-cart') : null;
            let lbAddBtn = document.getElementById('lightboxAddCart');
            if (addBtn) {
                lbAddBtn.setAttribute('data-code', addBtn.getAttribute('data-code'));
                lbAddBtn.setAttribute('data-image', addBtn.getAttribute('data-image'));
                lbAddBtn.setAttribute('data-category', addBtn.getAttribute('data-category') || '');
                lbAddBtn.style.display = '';
            } else {
                lbAddBtn.style.display = 'none';
            }

            let modal = new bootstrap.Modal(document.getElementById('imageLightbox'));
            modal.show();
        });
    });

    // Lightbox image gallery state
    let lightboxImages = [];
    let lightboxIndex = 0;

    function updateLightboxArrows() {
        document.getElementById('lightboxPrev').style.display = lightboxImages.length > 1 ? '' : 'none';
        document.getElementById('lightboxNext').style.display = lightboxImages.length > 1 ? '' : 'none';
        let counter = document.getElementById('lightboxCounter');
        if (lightboxImages.length > 1) {
            counter.textContent = (lightboxIndex + 1) + ' / ' + lightboxImages.length;
            counter.style.display = '';
        } else {
            counter.style.display = 'none';
        }
    }

    document.getElementById('lightboxPrev').addEventListener('click', function() {
        if (lightboxImages.length <= 1) return;
        lightboxIndex = (lightboxIndex - 1 + lightboxImages.length) % lightboxImages.length;
        document.getElementById('lightboxImage').src = lightboxImages[lightboxIndex];
        updateLightboxArrows();
    });

    document.getElementById('lightboxNext').addEventListener('click', function() {
        if (lightboxImages.length <= 1) return;
        lightboxIndex = (lightboxIndex + 1) % lightboxImages.length;
        document.getElementById('lightboxImage').src = lightboxImages[lightboxIndex];
        updateLightboxArrows();
    });
    document.getElementById('lightboxAddCart').addEventListener('click', function() {
        let code = this.getAttribute('data-code');
        let image = this.getAttribute('data-image');
        let category = this.getAttribute('data-category') || 'DİĞER';
        if (!code) return;
        if (!cart.find(c => c.code === code)) {
            cart.push({ code, image, category });
            logCatalogAction('AddProduct', cart[cart.length - 1]);
            updateCartUI();
            this.textContent = '✓ Eklendi';
            this.classList.remove('btn-dark');
            this.classList.add('btn-success');
            setTimeout(() => {
                this.innerHTML = catalogAddButtonHtml();
                this.classList.remove('btn-success');
                this.classList.add('btn-dark');
            }, 1500);
        } else {
            this.textContent = 'Zaten Eklendi';
            setTimeout(() => { this.innerHTML = catalogAddButtonHtml(); }, 1500);
        }
    });

    document.querySelectorAll('.btn-add-cart').forEach(btn => {
        btn.addEventListener('click', function () {
            let code = this.getAttribute('data-code');
            let image = this.getAttribute('data-image');
            let category = this.getAttribute('data-category') || 'DİĞER';
            
            let self = this;
            let originalHtml = self.innerHTML;
            let originalClass = self.className;

            if(!image) {
                // Hata durumu (resim yok)
                self.className = 'btn btn-sm btn-danger btn-add-cart';
                self.innerHTML = '<i class="fas fa-times"></i> Hata';
                setTimeout(() => {
                    self.className = originalClass;
                    self.innerHTML = originalHtml;
                }, 1500);
                return;
            }

            if(!cart.find(c => c.code === code)) {
                cart.push({code: code, image: image, category: category});
                logCatalogAction('AddProduct', cart[cart.length - 1]);
                updateCartUI();
                
                // Başarılı durumu
                self.className = 'btn btn-sm btn-success btn-add-cart';
                self.innerHTML = '<i class="fas fa-check"></i> Eklendi';
                setTimeout(() => {
                    self.className = originalClass;
                    self.innerHTML = originalHtml;
                }, 1500);
            } else {
                // Zaten ekli durumu
                self.className = 'btn btn-sm btn-warning btn-add-cart text-dark';
                self.innerHTML = '<i class="fas fa-exclamation"></i> Zaten Ekli';
                setTimeout(() => {
                    self.className = originalClass;
                    self.innerHTML = originalHtml;
                }, 1500);
            }
        });
    });

    let confirmClearModalInstance = null;
    
    document.getElementById('clearCatalogBtn').addEventListener('click', function () {
        if(cart.length === 0) {
            let originalHtml = this.innerHTML;
            this.classList.add('btn-danger');
            this.innerHTML = 'Zaten Boş';
            setTimeout(() => {
                this.classList.remove('btn-danger');
                this.innerHTML = originalHtml;
            }, 1500);
            return;
        }
        
        let modalEl = document.getElementById('confirmClearModal');
        if(modalEl) {
            confirmClearModalInstance = new bootstrap.Modal(modalEl);
            confirmClearModalInstance.show();
        }
    });

    let confirmClearActionBtn = document.getElementById('confirmClearCatalogActionBtn');
    if(confirmClearActionBtn) {
        confirmClearActionBtn.addEventListener('click', function() {
            cart.forEach(item => logCatalogAction('RemoveProduct', item));
            cart = [];
            updateCartUI();
            if(confirmClearModalInstance) {
                confirmClearModalInstance.hide();
            }
        });
    }

    document.getElementById('cancelPdfBtn').addEventListener('click', function() {
        pdfCanceled = true;
        document.getElementById('loadingOverlay').style.display = 'none';
    });

    document.getElementById('completePdfBtn').addEventListener('click', async function () {
        if(cart.length === 0) {
            document.getElementById('globalErrorMessage').innerText = 'Kataloğunuz boş.';
            new bootstrap.Modal(document.getElementById('globalErrorModal')).show();
            return;
        }

        pdfCanceled = false;
        document.getElementById('loadingOverlay').style.display = 'flex';

        let companyName = document.getElementById('companyName').value;
        let firstName = document.getElementById('firstName').value;
        let lastName = document.getElementById('lastName').value;
        let phoneNumber = document.getElementById('phoneNumber').value;

        const pdfLanguage = (document.getElementById('currentLangText')?.textContent || 'EN').trim().toLowerCase();
        const pdfIsEnglish = pdfLanguage === 'en';
        const pdfText = pdfIsEnglish ? {
            catalog: 'CATALOG', company: 'Company', fullName: 'Full Name', phone: 'Phone',
            purity: 'Purity', color: 'Color', weight: 'Weight', stoneDetails: 'Stone Details',
            noStoneDetails: 'No stone details available', clarity: 'Clarity', pieces: 'pcs',
            quantity: 'Quantity', note: 'Note', moreStoneDetails: 'more stone details', footer: 'Products and models are registered to Naif Jewellery and may not be used without permission.',
            other: 'OTHER'
        } : {
            catalog: 'KATALOG', company: 'Firma', fullName: 'Ad Soyad', phone: 'Telefon',
            purity: 'Ayar', color: 'Renk', weight: 'Ağırlık', stoneDetails: 'Taş Detayları',
            noStoneDetails: 'Taş detayı bulunmuyor', clarity: 'Berraklık', pieces: 'adet',
            quantity: 'Adet', note: 'Not', moreStoneDetails: 'taş detayı daha', footer: 'Ürünler ve modeller Naif Jewellery adına tescilli olup izinsiz kullanılamaz.',
            other: 'DİĞER'
        };

        const englishDefinitionNames = {
            'YÜZÜK':'RING', 'YUZUK':'RING', 'KÜPE':'EARRINGS', 'KUPE':'EARRINGS',
            'KOLYE':'NECKLACE', 'BİLEKLİK':'BRACELET', 'BILEKLIK':'BRACELET',
            'ALYANS':'WEDDING RING', 'KOLYE UCU':'PENDANT', 'PIRLANTA':'DIAMOND',
            'ELMAS':'DIAMOND', 'ZÜMRÜT':'EMERALD', 'ZUMRUT':'EMERALD',
            'SAFİR':'SAPPHIRE', 'SAFIR':'SAPPHIRE', 'YAKUT':'RUBY',
            'BEYAZ':'WHITE', 'SARI':'YELLOW', 'KIRMIZI':'RED', 'ROSE':'ROSE'
        };

        function translatePdfDefinition(value) {
            const decoded = decodeCatalogText(value || '-');
            if (!pdfIsEnglish) return decoded;
            return englishDefinitionNames[decoded.trim().toLocaleUpperCase('tr-TR')] || decoded;
        }

        const definitionTranslationCache = new Map();
        async function getPdfDefinitionTranslation(entityType, entityId, fallback) {
            if (!pdfIsEnglish || !entityId || !window.NAIF_API_BASE_URL) return translatePdfDefinition(fallback);
            const cacheKey = `${entityType}:${entityId}:en`;
            if (definitionTranslationCache.has(cacheKey)) return definitionTranslationCache.get(cacheKey);
            try {
                const url = `${window.NAIF_API_BASE_URL}/api/definition-translations?entityType=${encodeURIComponent(entityType)}&entityId=${encodeURIComponent(entityId)}&languageCode=en`;
                const response = await fetch(url);
                const result = await response.json();
                const translated = result && result.data ? decodeCatalogText(result.data) : translatePdfDefinition(fallback);
                definitionTranslationCache.set(cacheKey, translated);
                return translated;
            } catch (_) {
                return translatePdfDefinition(fallback);
            }
        }

        const pdfItems = await Promise.all(cart.map(async item => {
            const translatedItem = { ...item };
            translatedItem.category = await getPdfDefinitionTranslation('Category', item.categoryId, item.category || pdfText.other);
            translatedItem.ayar = await getPdfDefinitionTranslation('MetalPurity', item.ayarId, item.ayar || '-');
            translatedItem.renk = await getPdfDefinitionTranslation('MetalType', item.renkId, item.renk || '-');
            translatedItem.stones = await Promise.all((item.stones || []).map(async stone => ({
                ...stone,
                type: await getPdfDefinitionTranslation('StoneType', stone.typeId, stone.type || '-'),
                clarity: await getPdfDefinitionTranslation('StoneClarity', stone.clarityId, stone.clarity || '-'),
                color: await getPdfDefinitionTranslation('Color', stone.colorId, stone.color || '-')
            })));
            return translatedItem;
        }));

        // Group cart items by category
        let grouped = {};
        pdfItems.forEach(item => {
            let cat = translatePdfDefinition(item.category || pdfText.other);
            if(!grouped[cat]) grouped[cat] = [];
            grouped[cat].push(item);
        });

        // --- Image loader helper ---
        function loadImage(url) {
            return new Promise(resolve => {
                let img = new Image();
                img.onload = () => resolve(img);
                img.onerror = () => resolve(null);
                let normalizedUrl = normalizeCatalogImageUrl(url);
                if (/^https?:\/\//i.test(normalizedUrl)) img.crossOrigin = 'anonymous';
                img.src = normalizedUrl;
            });
        }

        // Convert image to data URL (safe for jsPDF)
        function imgToDataUrl(img, asPng) {
            let canvas = document.createElement('canvas');
            canvas.width = img.naturalWidth;
            canvas.height = img.naturalHeight;
            let ctx = canvas.getContext('2d');
            // Fill white background to prevent transparency becoming black
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0);
            return asPng ? canvas.toDataURL('image/png') : canvas.toDataURL('image/jpeg', 0.92);
        }

        // Render Unicode text (Turkish chars) via Canvas and add as image to PDF
        function drawUnicodeText(pdf, text, x, y, fontSize, options = {}) {
            if (!text || text.trim() === '') return;
            let scale = 4; // render at 4x for crisp text
            let canvas = document.createElement('canvas');
            let ctx = canvas.getContext('2d');
            let weight = options.bold ? 'bold' : 'normal';
            let font = `${weight} ${fontSize * scale}px Arial, Helvetica, sans-serif`;
            ctx.font = font;
            let metrics = ctx.measureText(text);
            canvas.width = Math.ceil(metrics.width) + 8;
            canvas.height = Math.ceil(fontSize * scale * 1.4) + 8;
            // Re-set font after resize
            ctx.font = font;
            ctx.fillStyle = options.color || '#333333';
            ctx.textBaseline = 'top';
            ctx.fillText(text, 4, 4);
            let dataUrl = canvas.toDataURL('image/png');
            let wInch = canvas.width / (96 * scale);
            let hInch = canvas.height / (96 * scale);
            let drawX = x;
            if (options.align === 'center') drawX = x - wInch / 2;
            if (options.align === 'right') drawX = x - wInch;
            let drawY = y - hInch * 0.6;
            try { pdf.addImage(dataUrl, 'PNG', drawX, drawY, wInch, hInch); } catch(e) {}
        }

        // Pre-load logo
        let logoImg = await loadImage('https://static.wixstatic.com/media/3afb87_01283146a20c43eda2fa6ba343107de9~mv2.png/v1/fill/w_299,h_95,al_c,q_85,usm_0.66_1.00_0.01,enc_avif,quality_auto/1730592442899-7c4af1dc-40ee-4adf-80ba-dd9cd7379dee_1-Photoroom.png');
        let logoDataUrl = logoImg ? imgToDataUrl(logoImg, true) : null;

        // Pre-load all product images
        let imageCache = {};
        for (let item of cart) {
            if (item.image && !imageCache[item.image]) {
                let img = await loadImage(item.image);
                imageCache[item.image] = img ? imgToDataUrl(img) : null;
            }
            if (pdfCanceled) {
                document.getElementById('loadingOverlay').style.display = 'none';
                return;
            }
        }

        // Close modals
        let modalEl = document.getElementById('customerModal');
        let modal = bootstrap.Modal.getInstance(modalEl);
        if(modal) modal.hide();
        let offcanvasEl = document.getElementById('catalogOffcanvas');
        let offcanvas = bootstrap.Offcanvas.getInstance(offcanvasEl);
        if(offcanvas) offcanvas.hide();

        try {
            // ===== LAYOUT CONSTANTS (inches, A4: 8.27 x 11.69) =====
            const ML = 0.5;                          // left margin
            const CW = 7.27;                         // content width (8.27 - 0.5 - 0.5)
            const MR_X = ML + CW;                    // right edge x
            const COLS = 2, ROWS = 3;
            const ITEMS_PER_PAGE = COLS * ROWS;      // detailed cards: 6 products per page

            // Header
            const LOGO_Y = 0.35, LOGO_W = 1.25, LOGO_H = 0.4;
            const HEADER_LINE_Y = 0.85;

            // Customer box (page 1 only)
            const CUST_Y = 0.88, CUST_H = 0.45;

            // Footer
            const FOOTER_LINE_Y = 10.9, FOOTER_TEXT_Y = 11.1;

            // Content area
            const CONTENT_TOP_P1 = CUST_Y + CUST_H + 0.08;  // page 1: after customer box
            const CONTENT_TOP = HEADER_LINE_Y + 0.12;         // other pages: after header line
            const CONTENT_BOTTOM = FOOTER_LINE_Y - 0.1;

            // Group title
            const TITLE_H = 0.32;

            // Grid spacing
            const COL_GAP = 0.15;
            const ROW_GAP = 0.1;
            const COL_W = (CW - (COLS - 1) * COL_GAP) / COLS;
            const IMAGE_H = 1.15;

            let now = new Date();
            let dateLocale = pdfIsEnglish ? 'en-GB' : 'tr-TR';
            let dateStr = now.toLocaleDateString(dateLocale) + " " + now.toLocaleTimeString(dateLocale, {hour:'2-digit', minute:'2-digit'});

            // ===== CREATE PDF =====
            let JsPDF = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
            let pdf = new JsPDF({ unit: 'in', format: 'a4', orientation: 'portrait' });

            let pageNum = 0;
            let isFirstPageGlobal = true;

            // --- Draw header on current page ---
            function drawHeader() {
                if (logoDataUrl) {
                    try { pdf.addImage(logoDataUrl, 'JPEG', ML, LOGO_Y, LOGO_W, LOGO_H); } catch(e) {}
                }
                pdf.setFontSize(14);
                pdf.setTextColor(50, 50, 50);
                pdf.text(pdfText.catalog, ML + LOGO_W + 0.08, LOGO_Y + LOGO_H - 0.08);

                pdf.setFontSize(10);
                pdf.setTextColor(100, 100, 100);
                pdf.text(dateStr, MR_X, LOGO_Y + LOGO_H - 0.05, null, null, 'right');

                pdf.setDrawColor(200, 200, 200);
                pdf.setLineWidth(0.01);
                pdf.line(ML, HEADER_LINE_Y, MR_X, HEADER_LINE_Y);
            }

            // --- Draw customer info box (page 1 only) ---
            function drawCustomerBox() {
                let bx = ML, by = CUST_Y, bw = CW, bh = CUST_H;
                let col3 = bw / 3;

                pdf.setDrawColor(200, 200, 200);
                pdf.setLineWidth(0.008);
                pdf.rect(bx, by, bw, bh);
                pdf.line(bx + col3, by, bx + col3, by + bh);
                pdf.line(bx + col3 * 2, by, bx + col3 * 2, by + bh);

                drawUnicodeText(pdf, pdfText.company,  bx + col3 * 0.5, by + 0.14, 7, { align: 'center', color: '#999999' });
                drawUnicodeText(pdf, pdfText.fullName, bx + col3 * 1.5, by + 0.14, 7, { align: 'center', color: '#999999' });
                drawUnicodeText(pdf, pdfText.phone,    bx + col3 * 2.5, by + 0.14, 7, { align: 'center', color: '#999999' });

                let maxC = 24;
                let cn = (companyName || '-');
                cn = cn.length > maxC ? cn.substring(0, maxC) + '...' : cn;
                let fn = ((firstName + ' ' + lastName).trim() || '-');
                fn = fn.length > maxC ? fn.substring(0, maxC) + '...' : fn;
                let ph = (phoneNumber || '-');
                ph = ph.length > maxC ? ph.substring(0, maxC) + '...' : ph;

                drawUnicodeText(pdf, cn, bx + col3 * 0.5, by + 0.32, 10, { align: 'center', bold: true });
                drawUnicodeText(pdf, fn, bx + col3 * 1.5, by + 0.32, 10, { align: 'center', bold: true });
                drawUnicodeText(pdf, ph, bx + col3 * 2.5, by + 0.32, 10, { align: 'center', bold: true });
            }

            // --- Draw footer (page number placeholder — will update at end) ---
            function drawFooter() {
                pdf.setDrawColor(200, 200, 200);
                pdf.setLineWidth(0.01);
                pdf.line(ML, FOOTER_LINE_Y, MR_X, FOOTER_LINE_Y);

                pdf.setFontSize(8);
                pdf.setTextColor(150, 150, 150);
                drawUnicodeText(pdf, pdfText.footer, ML, FOOTER_TEXT_Y, 8, { color: '#969696' });
            }

            // --- Draw group title ---
            function drawGroupTitle(title, y) {
                drawUnicodeText(pdf, title, ML, y + 0.18, 13, { color: '#555555' });
                pdf.setDrawColor(200, 200, 200);
                pdf.setLineWidth(0.008);
                pdf.line(ML, y + 0.25, MR_X, y + 0.25);
            }

            // --- Draw a product cell ---
            function drawProduct(item, x, y, cellW, cellH) {
                // Cell background + border
                pdf.setDrawColor(220, 220, 220);
                pdf.setLineWidth(0.005);
                pdf.setFillColor(250, 250, 250);
                pdf.roundedRect(x, y, cellW, cellH, 0.06, 0.06, 'FD');

                // Product image area
                let dataUrl = imageCache[item.image];
                if (dataUrl) {
                    let imgH = IMAGE_H;
                    let imgW = cellW - 0.24;
                    let ix = x + 0.12;
                    let iy = y + 0.08;
                    try {
                        pdf.addImage(dataUrl, 'JPEG', ix, iy, imgW, imgH);
                    } catch(e) {}
                }

                // Product identity and selected specifications
                const infoY = y + IMAGE_H + 0.16;
                pdf.setFillColor(242, 246, 250);
                pdf.roundedRect(x + 0.09, infoY, cellW - 0.18, 0.55, 0.04, 0.04, 'F');

                let code = (item.code || '-').length > 24 ? item.code.substring(0, 24) + '...' : (item.code || '-');
                drawUnicodeText(pdf, code, x + 0.16, infoY + 0.14, 10, { bold: true, color: '#172033' });
                if (item.price) {
                    drawUnicodeText(pdf, item.price, x + cellW - 0.16, infoY + 0.14, 9, { align: 'right', bold: true, color: '#168b52' });
                }

                const specs = [
                    [pdfText.purity, item.ayar || '-'],
                    [pdfText.color, translatePdfDefinition(item.renk || '-')],
                    [pdfText.weight, item.gram || '-'],
                    [pdfText.quantity, String(item.quantity || 1)]
                ];
                const specW = (cellW - 0.32) / specs.length;
                specs.forEach((spec, index) => {
                    const sx = x + 0.16 + index * specW;
                    drawUnicodeText(pdf, spec[0], sx, infoY + 0.32, 6, { color: '#8a94a5' });
                    drawUnicodeText(pdf, spec[1], sx, infoY + 0.47, 8, { bold: true, color: '#273247' });
                });

                // Stone details preserve the values selected on the detail page (VVS, VS, etc.).
                const stones = Array.isArray(item.stones) ? item.stones : [];
                const stoneY = infoY + 0.66;
                drawUnicodeText(pdf, pdfText.stoneDetails, x + 0.13, stoneY + 0.08, 8, { bold: true, color: '#273247' });

                if (stones.length === 0) {
                    drawUnicodeText(pdf, pdfText.noStoneDetails, x + 0.13, stoneY + 0.28, 7, { color: '#9aa2af' });
                } else {
                    const visibleStones = stones.slice(0, 3);
                    visibleStones.forEach((stone, index) => {
                        const sy = stoneY + 0.18 + index * 0.29;
                        pdf.setFillColor(index % 2 === 0 ? 248 : 242, index % 2 === 0 ? 249 : 246, index % 2 === 0 ? 251 : 250);
                        pdf.roundedRect(x + 0.1, sy, cellW - 0.2, 0.25, 0.025, 0.025, 'F');

                        drawUnicodeText(pdf, translatePdfDefinition(stone.type || '-'), x + 0.16, sy + 0.09, 7, { bold: true, color: '#273247' });
                        drawUnicodeText(pdf, pdfText.clarity + ': ' + (stone.clarity || '-'), x + 1.02, sy + 0.09, 7, { color: '#3d6b99' });
                        drawUnicodeText(pdf, stone.totalCarat || '-', x + 2.08, sy + 0.09, 7, { bold: true, color: '#168b52' });
                        drawUnicodeText(pdf, (stone.quantity || '-') + ' ' + pdfText.pieces, x + cellW - 0.16, sy + 0.09, 7, { align: 'right', color: '#586174' });
                    });

                    if (stones.length > visibleStones.length) {
                        drawUnicodeText(pdf, '+' + (stones.length - visibleStones.length) + ' ' + pdfText.moreStoneDetails, x + 0.13, stoneY + 1.08, 6, { color: '#8a94a5' });
                    }
                }

                if (item.note) {
                    const noteY = stoneY + (stones.length === 0 ? 0.48 : Math.min(stones.length, 3) * 0.29 + 0.32);
                    const shortNote = item.note.length > 70 ? item.note.substring(0, 67) + '...' : item.note;
                    drawUnicodeText(pdf, pdfText.note + ': ' + shortNote, x + 0.13, noteY, 7, { color: '#7c3aed' });
                }
            }

            // ===== BUILD PAGES =====
            // Split each category into chunks of ITEMS_PER_PAGE
            let pageData = [];
            for (let cat in grouped) {
                let items = grouped[cat];
                for (let i = 0; i < items.length; i += ITEMS_PER_PAGE) {
                    pageData.push({
                        category: cat,
                        items: items.slice(i, i + ITEMS_PER_PAGE)
                    });
                }
            }

            // Render each page
            for (let p = 0; p < pageData.length; p++) {
                if (p > 0) pdf.addPage();
                pageNum++;

                let page = pageData[p];
                let contentTop = isFirstPageGlobal ? CONTENT_TOP_P1 : CONTENT_TOP;

                // Draw header
                drawHeader();

                // Draw customer box on first page
                if (isFirstPageGlobal) {
                    drawCustomerBox();
                    isFirstPageGlobal = false;
                }

                // Draw footer
                drawFooter();

                // Draw group title
                drawGroupTitle(page.category, contentTop);
                let gridTop = contentTop + TITLE_H;

                // Calculate row height for this page
                let availH = CONTENT_BOTTOM - gridTop;
                let rowH = (availH - (ROWS - 1) * ROW_GAP) / ROWS;

                // Draw products
                page.items.forEach((item, idx) => {
                    let col = idx % COLS;
                    let row = Math.floor(idx / COLS);

                    let x = ML + col * (COL_W + COL_GAP);
                    let y = gridTop + row * (rowH + ROW_GAP);

                    drawProduct(item, x, y, COL_W, rowH);
                });
            }

            // Update page numbers on all pages
            let totalPages = pageNum;
            for (let i = 1; i <= totalPages; i++) {
                pdf.setPage(i);
                pdf.setFontSize(8);
                pdf.setTextColor(150, 150, 150);
                pdf.text(i + " / " + totalPages, MR_X - 0.1, FOOTER_TEXT_Y, null, null, 'right');
            }

            // Save
            pdf.save('Katalog.pdf');

            document.getElementById('loadingOverlay').style.display = 'none';
            if (!pdfCanceled) {
                cart.forEach(item => logCatalogAction('RemoveProduct', item));
                cart = [];
                updateCartUI();
            }
        } catch(err) {
            document.getElementById('loadingOverlay').style.display = 'none';
            document.getElementById('globalErrorMessage').innerText = 'PDF oluşturulurken bir hata oluştu.';
            new bootstrap.Modal(document.getElementById('globalErrorModal')).show();
            console.error(err);
        }
    });
});

window.removeFromCart = function(idOrCode) {
    let globalIndex = cart.findIndex(c => (c.id || c.code) === idOrCode);
    if(globalIndex !== -1) {
        let removedItem = cart[globalIndex];
        cart.splice(globalIndex, 1);
        logCatalogAction('RemoveProduct', removedItem);
        updateCartUI();
    }
}

function updateCartUI() {
    sessionStorage.setItem('naif_catalog_cart', JSON.stringify(cart));
    document.getElementById('catalogCount').innerText = cart.length;
    
    let container = document.getElementById('catalogItemsOffcanvas');
    container.innerHTML = '';
    
    // Group items for display in offcanvas
    let grouped = {};
    cart.forEach((item) => {
        let cat = item.category || 'DİĞER';
        if(!grouped[cat]) grouped[cat] = [];
        grouped[cat].push(item);
    });

    for(let cat in grouped) {
        container.innerHTML += `<h6 class="mt-4 mb-2 fw-bold text-secondary border-bottom pb-1">${cat}</h6>`;
        grouped[cat].forEach(item => {
            let itemId = item.id || item.code;
            let detailsHtml = '';
            
            let specs = [];
            if(item.ayar) specs.push(item.ayar);
            if(item.renk) specs.push(item.renk);
            if(item.gram) specs.push(item.gram);
            specs.push(`${item.quantity || 1} adet`);
            
            if(specs.length > 0) {
                detailsHtml += `<div class="text-muted" style="font-size: 11px;">${specs.join(' | ')}</div>`;
            }
            
            if(item.price) {
                detailsHtml += `<div class="text-success fw-bold" style="font-size: 12px;">${item.price}</div>`;
            }


            if(item.note) {
                detailsHtml += `<div class="text-dark" style="font-size:11px;margin-top:4px;"><strong>Not:</strong> ${escapeCatalogHtml(item.note)}</div>`;
            }

            if (Array.isArray(item.stones) && item.stones.length > 0) {
                detailsHtml += `<div style="margin-top:7px;padding-top:6px;border-top:1px solid #e2e8f0;">`;
                detailsHtml += `<div style="font-size:10px;font-weight:700;color:#64748b;text-transform:uppercase;letter-spacing:.5px;margin-bottom:4px;">Taş Detayları</div>`;
                item.stones.forEach(stone => {
                    detailsHtml += `<div style="display:grid;grid-template-columns:1fr auto;gap:3px 8px;background:#fff;border:1px solid #edf2f7;border-radius:7px;padding:5px 7px;margin-bottom:4px;font-size:10px;line-height:1.25;">` +
                        `<strong style="color:#334155;">${escapeCatalogHtml(stone.type)} · ${escapeCatalogHtml(stone.clarity)}</strong>` +
                        `<strong style="color:#2563eb;">${escapeCatalogHtml(stone.totalCarat)}</strong>` +
                        `<span style="color:#64748b;">Renk: ${escapeCatalogHtml(stone.color)}</span>` +
                        `<span style="color:#64748b;">Adet: ${escapeCatalogHtml(stone.quantity)}</span></div>`;
                });
                detailsHtml += `</div>`;
            }

            container.innerHTML += `
                <div class="d-flex align-items-start mb-2 border rounded p-2 bg-light shadow-sm">
                    <img src="${item.image}" width="58" height="58" style="object-fit:contain; background-color:#fff;" class="me-2 rounded border" />
                    <div class="flex-grow-1 small" style="min-width:0;">
                        <div class="fw-bold">${item.code}</div>
                        ${detailsHtml}
                    </div>
                    <button class="btn btn-sm btn-outline-danger py-0 px-2" onclick="removeFromCart('${itemId}')">
                        Sil
                    </button>
                </div>
            `;
        });
    }
}

function escapeCatalogHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, ch => ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', '"':'&quot;', "'":'&#39;' })[ch]);
}
