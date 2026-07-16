const fs = require('fs');
let content = fs.readFileSync('Views/Home/Index.cshtml', 'utf8');
content = content.replace(
    /\} catch \(e\) \{\s*console\.log\("Canlı Has Altın fiyatı çekilirken hata oluştu: ", e\);\s*\}/g,
    `} catch (e) {
            console.log("Canlı Has Altın fiyatı çekilirken hata oluştu: ", e);
        } finally {
            document.querySelectorAll('.dynamic-price').forEach(function(el) {
                if (el.innerHTML.includes('fa-spinner')) {
                    const basePrice = parseFloat(el.getAttribute('data-base-price'));
                    el.innerText = basePrice.toLocaleString('tr-TR', { maximumFractionDigits: 0 }) + ' $';
                }
            });
        }`
);
fs.writeFileSync('Views/Home/Index.cshtml', content);
