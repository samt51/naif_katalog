// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

let cart = JSON.parse(sessionStorage.getItem('naif_catalog_cart')) || [];
let pdfCanceled = false;

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
            updateCartUI();
            this.textContent = '✓ Eklendi';
            this.classList.remove('btn-dark');
            this.classList.add('btn-success');
            setTimeout(() => {
                this.textContent = 'Kataloğa Ekle';
                this.classList.remove('btn-success');
                this.classList.add('btn-dark');
            }, 1500);
        } else {
            this.textContent = 'Zaten Eklendi';
            setTimeout(() => { this.textContent = 'Kataloğa Ekle'; }, 1500);
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

        // Group cart items by category
        let grouped = {};
        cart.forEach(item => {
            let cat = item.category || 'DİĞER';
            if(!grouped[cat]) grouped[cat] = [];
            grouped[cat].push(item);
        });

        // --- Image loader helper ---
        function loadImage(url) {
            return new Promise(resolve => {
                let img = new Image();
                img.crossOrigin = 'Anonymous';
                img.onload = () => resolve(img);
                img.onerror = () => resolve(null);
                img.src = url;
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
            const COLS = 4, ROWS = 5;
            const ITEMS_PER_PAGE = COLS * ROWS;      // 15

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
            const TEXT_H = 0.25;  // space for product code below image

            let now = new Date();
            let dateStr = now.toLocaleDateString('tr-TR') + " " + now.toLocaleTimeString('tr-TR', {hour:'2-digit', minute:'2-digit'});

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
                pdf.text("KATALOG", ML + LOGO_W + 0.08, LOGO_Y + LOGO_H - 0.08);

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

                drawUnicodeText(pdf, 'Firma',    bx + col3 * 0.5, by + 0.14, 7, { align: 'center', color: '#999999' });
                drawUnicodeText(pdf, 'Ad Soyad', bx + col3 * 1.5, by + 0.14, 7, { align: 'center', color: '#999999' });
                drawUnicodeText(pdf, 'Telefon',  bx + col3 * 2.5, by + 0.14, 7, { align: 'center', color: '#999999' });

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
                pdf.text("Urunler ve modeller Naif Jewellery adina tescilli olup izinsiz kullanilamaz.", ML, FOOTER_TEXT_Y);
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

                // Image
                let dataUrl = imageCache[item.image];
                if (dataUrl) {
                    let imgH = cellH - TEXT_H - 0.15;  // available height for image
                    let imgW = cellW - 0.2;             // available width for image
                    // We don't know actual aspect ratio from dataUrl, so draw stretched to fit
                    // jsPDF will handle it
                    let ix = x + 0.1;
                    let iy = y + 0.08;
                    try {
                        pdf.addImage(dataUrl, 'JPEG', ix, iy, imgW, imgH);
                    } catch(e) {}
                }

                // Product code
                let code = item.code.length > 20 ? item.code.substring(0, 20) + '...' : item.code;
                drawUnicodeText(pdf, code, x + cellW / 2, y + cellH - 0.06, 8, { align: 'center', bold: true });
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
        cart.splice(globalIndex, 1);
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
            
            if(specs.length > 0) {
                detailsHtml += `<div class="text-muted" style="font-size: 11px;">${specs.join(' | ')}</div>`;
            }
            
            if(item.price) {
                detailsHtml += `<div class="text-success fw-bold" style="font-size: 12px;">${item.price}</div>`;
            }

            container.innerHTML += `
                <div class="d-flex align-items-center mb-2 border rounded p-2 bg-light">
                    <img src="${item.image}" width="50" height="50" style="object-fit:contain; background-color:#fff;" class="me-2 rounded border" />
                    <div class="flex-grow-1 small">
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
