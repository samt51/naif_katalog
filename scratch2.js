
    function switchTab(tabId) {
        document.querySelectorAll('.modal-tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
        
        const tabBtn = document.querySelector('.modal-tab[onclick*="' + tabId + '"]');
        if (tabBtn) tabBtn.classList.add('active');
        
        document.getElementById('tab-' + tabId).classList.add('active');
    }

    let tempStones = [];
    let tempImages = [];
    let isNameManuallyEdited = false;
    let polishingCosts = []; // To store Cila/Rodaj maliyetleri
    let selectedCategoryIds = [];
    const allCategories = []

    function getCategoryPath(catId) {
        let path = [];
        let currentId = catId;
        while(currentId > 0) {
            let cat = allCategories.find(c => c.Id === currentId || c.id === currentId);
            if(cat) {
                path.unshift(currentId);
                currentId = cat.ParentId !== undefined ? cat.ParentId : cat.parentId;
            } else {
                break;
            }
        }
        return path;
    }

    function handleCategoryChange(level, selectedValue) {
        const container = document.getElementById('categoryContainer');
        while(container.children.length > level + 1) {
            container.removeChild(container.lastChild);
        }

        if (selectedValue) {
            renderNextCategoryLevel(parseInt(selectedValue), level + 1);
        }
    }

    function renderNextCategoryLevel(parentId, level, selectedValue = "") {
        const children = allCategories.filter(c => (c.ParentId !== undefined ? c.ParentId : c.parentId) === parentId);
        if (children.length === 0) return;

        const container = document.getElementById('categoryContainer');
        const select = document.createElement('select');
        select.id = 'cat_select_' + level;
        select.className = 'form-select';
        select.style.flex = "1";
        select.style.minWidth = "150px";
        
        select.innerHTML = '<option value="">Seçiniz</option>';
        children.forEach(c => {
            const id = c.Id !== undefined ? c.Id : c.id;
            const name = c.Name !== undefined ? c.Name : c.name;
            const isSelected = id == selectedValue ? 'selected' : '';
            select.innerHTML += `<option value="${id}" ${isSelected}>${name}</option>`;
        });

        select.addEventListener('change', function() {
            handleCategoryChange(level, this.value);
            if (this.value) {
                addCategoryToList(true); // silent = true
            }
        });

        container.appendChild(select);
    }

    function buildCategorySelects(targetCategoryId) {
        document.getElementById('categoryContainer').innerHTML = '';

        if (!targetCategoryId) {
            renderNextCategoryLevel(0, 0);
            return;
        }

        const path = getCategoryPath(targetCategoryId);
        let currentParent = 0;
        for(let i = 0; i < path.length; i++) {
            renderNextCategoryLevel(currentParent, i, path[i]);
            currentParent = path[i];
        }
        renderNextCategoryLevel(currentParent, path.length);
    }

    function addCategoryToList(silent = false) {
        const selects = document.querySelectorAll('#categoryContainer select');
        let selectedId = null;
        for (let i = selects.length - 1; i >= 0; i--) {
            if (selects[i].value) {
                selectedId = parseInt(selects[i].value);
                break;
            }
        }
        if (!selectedId) {
            if(!silent) Swal.fire('Uyarı', 'Lütfen bir kategori seçiniz.', 'warning');
            return;
        }

        if (selectedCategoryIds.includes(selectedId)) {
            if(!silent) Swal.fire('Uyarı', 'Bu kategori zaten ekli.', 'warning');
            return;
        }

        selectedCategoryIds.push(selectedId);
        updateSelectedCategoriesUI();
        triggerPolishingCostUpdate();
    }

    function removeCategory(id) {
        selectedCategoryIds = selectedCategoryIds.filter(x => x !== id);
        updateSelectedCategoriesUI();
        triggerPolishingCostUpdate();
    }

    function updateSelectedCategoriesUI() {
        const container = document.getElementById('selectedCategories');
        container.innerHTML = '';
        selectedCategoryIds.forEach(id => {
            const cat = allCategories.find(c => (c.Id || c.id) === id);
            if (cat) {
                const name = cat.Name || cat.name;
                container.innerHTML += `
                    <span style="background:var(--primary); color:white; padding:6px 12px; border-radius:20px; font-size:13px; display:inline-flex; align-items:center; gap:8px;">
                        ${name}
                        <i class="fas fa-times" style="cursor:pointer;" onclick="removeCategory(${id})"></i>
                        <input type="hidden" name="CategoryIds" value="${id}" />
                    </span>
                `;
            }
        });
    }

    function triggerPolishingCostUpdate() {
        if (selectedCategoryIds.length > 0) {
            const catId = selectedCategoryIds[0];
            let pc = polishingCosts.find(x => x.categoryId === catId);
            
            if (!pc) {
                const cat = allCategories.find(c => c.Id === catId);
                if (cat && cat.ParentId > 0) {
                    pc = polishingCosts.find(x => x.categoryId === cat.ParentId);
                }
            }

            if (pc) {
                document.getElementById('PolishingCost').value = pc.costPrice;
            } else {
                document.getElementById('PolishingCost').value = 0;
            }
        } else {
            document.getElementById('PolishingCost').value = 0;
        }
        calculateLiveCost();
    }

    document.addEventListener('DOMContentLoaded', async function() {
        // Fetch Polishing Costs
        const API_BASE_URL = "http://localhost";
        try {
            const pcRes = await fetch(`${API_BASE_URL}/api/PolishingCost`);
            if (pcRes.ok) {
                const pcData = await pcRes.json();
                polishingCosts = pcData.polishingCosts || [];
            }
        } catch(e) {}

        // Kategori dinlemesi artık butonla yapıldığı için burayı kaldırdık

        document.getElementById('Code').addEventListener('input', function() {
            this.value = this.value.toLocaleUpperCase('tr-TR');
            if (!isNameManuallyEdited) {
                document.getElementById('Name').value = this.value;
            }
        });
        
        document.getElementById('Name').addEventListener('input', function() {
            isNameManuallyEdited = true;
        });
    });

    // Live Calculation Logic
    const allPurities = []
    let globalStonesCost = 0; 

    function calculateLiveCost() {
        let hasPrice = parseFloat(document.getElementById('LiveHasPrice').value.replace(',', '.')) || 0;
        
        const gram = parseFloat(document.getElementById('Gram').value) || 0;
        const laborMult = parseFloat(document.getElementById('LaborMultiplier').value) || 0;
        const polishCost = parseFloat(document.getElementById('PolishingCost').value) || 0;
        const karatVal = document.getElementById('Karat').value;
        const multiplier = parseFloat(document.getElementById('SalesMultiplier').value) || 1;

        let milyem = 0.585; 
        if (karatVal) {
            const purityObj = allPurities.find(p => p.Name === karatVal || p.name === karatVal);
            if (purityObj) {
                const milVal = purityObj.Milyem || purityObj.milyem;
                if (milVal) {
                    milyem = parseFloat(milVal.toString().replace(',', '.'));
                }
            }
        }

        const goldCost = gram * milyem * hasPrice;
        const laborCost = gram * laborMult * hasPrice;
        
        const totalCost = goldCost + laborCost + polishCost + globalStonesCost;
        const salesPrice = totalCost * multiplier;

        document.getElementById('summaryGoldCost').innerText = '$' + goldCost.toFixed(2);
        document.getElementById('summaryLaborCost').innerText = '$' + laborCost.toFixed(2);
        document.getElementById('summaryStoneCost').innerText = '$' + globalStonesCost.toFixed(2);
        document.getElementById('summaryPolishingCost').innerText = '$' + polishCost.toFixed(2);
        document.getElementById('summaryTotalCost').innerText = '$' + totalCost.toFixed(2);
        document.getElementById('summarySalesPrice').innerText = '$' + salesPrice.toFixed(2);
    }

    document.getElementById('Gram').addEventListener('input', calculateLiveCost);
    document.getElementById('LaborMultiplier').addEventListener('input', calculateLiveCost);
    document.getElementById('PolishingCost').addEventListener('input', calculateLiveCost);
    document.getElementById('Karat').addEventListener('change', calculateLiveCost);

    // Taş karat oto-hesaplama
    function updateStoneTotalCarat() {
        const qty = parseFloat(document.getElementById('StoneQuantity').value) || 0;
        const car = parseFloat(document.getElementById('StoneCarat').value) || 0;
        document.getElementById('StoneTotalCarat').value = (qty * car).toFixed(3);
    }
    document.getElementById('StoneQuantity').addEventListener('input', updateStoneTotalCarat);
    document.getElementById('StoneCarat').addEventListener('input', updateStoneTotalCarat);

    document.getElementById('StoneTotalCarat').addEventListener('input', function() {
        const qty = parseFloat(document.getElementById('StoneQuantity').value) || 0;
        const tcar = parseFloat(this.value) || 0;
        if (qty > 0) {
            document.getElementById('StoneCarat').value = (tcar / qty).toFixed(3);
        }
    });

    function closeModal() {
        document.getElementById('productModal').classList.remove('active');
    }

    async function openModal(id = 0, preserveTab = false) {
        document.getElementById('productId').value = id;
        document.getElementById('modalTitle').innerText = id === 0 ? "Yeni Ürün Ekle" : "Ürünü Düzenle";
        
        tempStones = [];
        tempImages = [];
        document.getElementById('imageInput').value = '';

        if (!preserveTab) switchTab('genel');

        const API_BASE_URL = "http://localhost";
        fetch(`${API_BASE_URL}/api/ExchangeRate/HasAltin`)
            .then(r => r.json())
            .then(data => {
                if(data && data.price) {
                    const priceEl = document.getElementById('LiveHasPrice');
                    if (priceEl && !priceEl.value) {
                        priceEl.value = data.price.toFixed(2);
                    }
                }
            })
            .catch(e => console.log(e));

        if (id === 0) {
            document.getElementById('Code').readOnly = false;
            document.getElementById('Code').style.backgroundColor = '#fff';

            document.getElementById('productForm').reset();
            document.getElementById('MetalTypeId').value = "";
            document.getElementById('productId').value = "0";
            selectedCategoryIds = [];
            updateSelectedCategoriesUI();
            buildCategorySelects(0);
            document.getElementById('imageGallery').innerHTML = "";
            document.getElementById('Karat').value = "14K";
            document.getElementById('LaborMultiplier').value = "0.200";
            renderStones();
            
            calculateLiveCost();
        } else {
            document.getElementById('modalTitle').innerText = "Veriler Yükleniyor...";
            document.getElementById('productForm').style.opacity = '0.5';
            document.getElementById('productForm').style.pointerEvents = 'none';

            fetch('/Product/Details/' + id)
                .then(r => r.json())
                .then(res => {
                    document.getElementById('modalTitle').innerText = "Ürünü Düzenle";
                    document.getElementById('productForm').style.opacity = '1';
                    document.getElementById('productForm').style.pointerEvents = 'auto';

                    if (res.isSuccess && res.data) {
                        const p = res.data;
                        document.getElementById('Code').value = p.code;
                        document.getElementById('Code').readOnly = true;
                        document.getElementById('Code').style.backgroundColor = '#e9ecef';
                        document.getElementById('Name').value = p.name || '';
                        document.getElementById('Description').value = p.description || '';
                        if (p.categoryIds && p.categoryIds.length > 0) {
                            selectedCategoryIds = [...p.categoryIds];
                        } else {
                            selectedCategoryIds = [];
                        }
                        updateSelectedCategoriesUI();
                        buildCategorySelects(0);
                        document.getElementById('ColorId').value = p.colorId || '';
                        document.getElementById('Gram').value = p.gram || 0;

                        if (p.productMetals && p.productMetals.length > 0) {
                            document.getElementById('MetalTypeId').value = p.productMetals[0].metalTypeId || '';
                        } else {
                            document.getElementById('MetalTypeId').value = '';
                        }

                        document.getElementById('Karat').value = p.metalPurityName || '';
                        document.getElementById('LaborMultiplier').value = p.laborMultiplier || 0;
                        document.getElementById('PolishingCost').value = p.polishingCost || 0;
                        if(p.liveGoldPrice > 0) {
                            document.getElementById('LiveHasPrice').value = p.liveGoldPrice;
                        }

                        const imageGallery = document.getElementById('imageGallery');
                        imageGallery.innerHTML = '';
                        if(p.images) {
                            p.images.forEach(img => {
                                imageGallery.innerHTML += `<div style="width:100px; height:100px; border-radius:8px; background:url('${img}') center/cover; position:relative; box-shadow:0 2px 4px rgba(0,0,0,0.1);"></div>`;
                            });
                        }

                        tempStones = p.productStones ? p.productStones.map(s => {
                            let car = s.carat || 0;
                            const tcar = s.totalCarat || 0;
                            const qty = s.quantity || 0;
                            if (car === 0 && tcar > 0 && qty > 0) {
                                car = parseFloat((tcar / qty).toFixed(4));
                            }
                            return {
                                Id: s.id, ClarityId: s.clarityId, ClarityName: s.clarityName, StoneId: s.stoneId, StoneName: s.stoneName, ColorId: s.colorId, ColorName: s.colorName, Quantity: s.quantity, Carat: car, TotalCarat: s.totalCarat
                            };
                        }) : [];
                        
                        renderStones();
                        
                        calculateLiveCost();
                    }
                })
                .catch(err => {
                    console.error("openModal error:", err);
                    document.getElementById('modalTitle').innerText = "Hata Oluştu!";
                    document.getElementById('productForm').style.opacity = '1';
                    document.getElementById('productForm').style.pointerEvents = 'auto';
                    Swal.fire('Hata', 'Ürün detayları yüklenemedi. (' + err.message + ')', 'error');
                });
        }

        document.getElementById('productModal').classList.add('active');
    }

    
    let allStoneOptions = [];
    document.addEventListener('DOMContentLoaded', function() {
        const stoneSelect = document.getElementById('StoneId');
        for (let i = 0; i < stoneSelect.options.length; i++) {
            if (stoneSelect.options[i].value !== "") {
                allStoneOptions.push({
                    value: stoneSelect.options[i].value,
                    text: stoneSelect.options[i].text,
                    clarity: stoneSelect.options[i].getAttribute('data-clarity'),
                    price: stoneSelect.options[i].getAttribute('data-price')
                });
            }
        }
    });

    function filterStonesByClarity() {
        const selectedClarity = document.getElementById('ClarityId').value;
        const stoneSelect = document.getElementById('StoneId');
        
        stoneSelect.innerHTML = '<option value="">Seçiniz</option>'; // Temizle
        
        if (!selectedClarity) {
            stoneSelect.setAttribute('disabled', 'disabled');
        } else {
            stoneSelect.removeAttribute('disabled');
            
            allStoneOptions.forEach(opt => {
                if (opt.clarity === selectedClarity) {
                    const newOption = document.createElement('option');
                    newOption.value = opt.value;
                    newOption.text = opt.text;
                    newOption.setAttribute('data-clarity', opt.clarity);
                    if(opt.price) newOption.setAttribute('data-price', opt.price);
                    stoneSelect.appendChild(newOption);
                }
            });
        }
    }

    function addStone() {
        const clId = document.getElementById('ClarityId').value;
        const selCl = document.getElementById('ClarityId');
        const clName = clId ? selCl.options[selCl.selectedIndex].text : "-";

        const cId = document.getElementById('ColorId').value;
        const selColor = document.getElementById('ColorId');
        const cName = cId ? selColor.options[selColor.selectedIndex].text : "-";
        const sId = document.getElementById('StoneId').value;
        const selObj = document.getElementById('StoneId');
        const sName = selObj.options[selObj.selectedIndex].text;
        const qtyVal = document.getElementById('StoneQuantity').value || "0";
        const carVal = document.getElementById('StoneCarat').value || "0";
        const tcarVal = document.getElementById('StoneTotalCarat').value || "0";
        
        const qty = parseFloat(qtyVal.replace(',', '.')) || 0;
        let car = parseFloat(carVal.replace(',', '.')) || 0;
        const tcar = parseFloat(tcarVal.replace(',', '.')) || 0;
        
        if (car === 0 && tcar > 0 && qty > 0) {
            car = parseFloat((tcar / qty).toFixed(4));
        }

        if(!sId) { 
            Swal.fire({ icon: 'warning', title: 'Uyarı', text: 'Lütfen taş seçiniz' });
            return; 
        }

        tempStones.push({
            Id: 0, 
            StoneId: parseInt(sId),
            ClarityId: clId ? parseInt(clId) : null,
            ClarityName: clName,
            StoneName: sName,
            ColorId: cId ? parseInt(cId) : null,
            ColorName: cName,
            Quantity: qty,
            Carat: car,
            TotalCarat: tcar
        });
        
        renderStones();
        const Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, timer: 3000, timerProgressBar: true });
        Toast.fire({ icon: 'success', title: 'Taş eklendi' });
    }

    function deleteStoneLocal(index) {
        tempStones.splice(index, 1);
        renderStones();
    }

    

    

    function renderStones() {
        globalStonesCost = 0;
        const stoneBody = document.getElementById('stoneTableBody');
        if(!tempStones || tempStones.length === 0) {
            stoneBody.innerHTML = `<tr><td colspan="9" class="text-center text-muted">Henüz taş eklenmemiş</td></tr>`;
            calculateLiveCost();
            return;
        }

        const stonesArray = []
        const stoneTypesArray = []

        let html = '';
        tempStones.forEach((s, index) => {
            let stoneTypeName = '-';
            let price = 0;
            const st = stonesArray.find(x => (x.Id !== undefined ? x.Id : x.id) === s.StoneId);
            if(st) {
                const typeObj = stoneTypesArray.find(t => (t.Id !== undefined ? t.Id : t.id) === (st.StoneTypeId !== undefined ? st.StoneTypeId : st.stoneTypeId));
                if (typeObj) {
                    stoneTypeName = typeObj.Name !== undefined ? typeObj.Name : typeObj.name;
                }
                price = st.CostPrice !== undefined ? st.CostPrice : (st.costPrice || 0);
                
                let settingType = '';
                let settingPrice = 0;
                
                if (st.StoneSetting || st.stoneSetting) {
                    const settingObj = st.StoneSetting || st.stoneSetting;
                    settingType = settingObj.SettingType || settingObj.settingType || '';
                    settingPrice = settingObj.SettingPrice !== undefined ? settingObj.SettingPrice : (settingObj.settingPrice || 0);
                }

                globalStonesCost += (s.TotalCarat * price);
                
                if(settingType && (settingType.toLowerCase().includes('adet') || settingType.toLowerCase() === 'piece')) {
                    globalStonesCost += (s.Quantity * settingPrice);
                } else if(settingType && (settingType.toLowerCase().includes('karat') || settingType.toLowerCase() === 'carat')) {
                    globalStonesCost += (s.TotalCarat * settingPrice);
                }
            }

            html += `<tr>
                <td>${s.ClarityName || '-'}</td>
                <td>${stoneTypeName}</td>
                <td>${s.StoneName}</td>
                <td>${s.ColorName || '-'}</td>
                <td>${s.Quantity}</td>
                <td>${s.Carat}</td>
                <td>${s.TotalCarat}</td>
                <td>$${price.toFixed(2)}</td>
                <td><button type="button" class="btn btn-sm btn-danger" onclick="deleteStoneLocal(${index})"><i class="fas fa-trash"></i></button></td>
            </tr>`;
        });
        stoneBody.innerHTML = html;
        calculateLiveCost();
    }

    

    document.getElementById('uploadArea').addEventListener('click', () => {
        document.getElementById('imageInput').click();
    });

    document.getElementById('imageInput').addEventListener('change', (e) => {
        const files = e.target.files;
        if(files.length === 0) return;
        
        for(let i=0; i<files.length; i++) {
            tempImages.push(files[i]);
            const objectUrl = URL.createObjectURL(files[i]);
            const imageGallery = document.getElementById('imageGallery');
            imageGallery.innerHTML += `<div style="width:100px; height:100px; border-radius:8px; background:url('${objectUrl}') center/cover; position:relative; box-shadow:0 2px 4px rgba(0,0,0,0.1);">
                <span style="position:absolute; top:2px; right:2px; background:rgba(255,255,255,0.8); border-radius:4px; font-size:10px; padding:2px;">Yeni</span>
            </div>`;
        }
    });

    document.getElementById('productForm').addEventListener('submit', async (e) => {
        e.preventDefault();
        const btn = document.getElementById('btnSave');
        btn.disabled = true;
        btn.innerText = "Kaydediliyor...";

        const formData = new FormData(e.target);
        const id = parseInt(formData.get('Id'));
        
        if (selectedCategoryIds.length === 0) {
            Swal.fire('Uyarı', 'Lütfen en az bir kategori ekleyin.', 'warning');
            btn.disabled = false;
            btn.innerText = "Kaydet";
            return;
        }
        
        const livePriceInput = document.getElementById('LiveHasPrice').value;
        if(livePriceInput) {
            formData.set('LiveGoldPrice', livePriceInput.toString().replace(',', '.'));
        }

        ['Gram', 'LaborMultiplier', 'PolishingCost', 'DiamondCarat'].forEach(field => {
            const val = formData.get(field);
            if (val) {
                formData.set(field, val.toString().replace(',', '.'));
            }
        });

        formData.append('stonesJson', JSON.stringify(tempStones));
                
        const karatSel = document.getElementById('Karat');
        if (karatSel && karatSel.selectedIndex >= 0) {
            const karatId = karatSel.options[karatSel.selectedIndex].getAttribute('data-id');
            if (karatId) formData.append('MetalPurityId', karatId);
        }

        const metalTypeId = document.getElementById('MetalTypeId').value;
        const gram = parseFloat(document.getElementById('Gram').value) || 0;
        let finalMetals = [];
        if (metalTypeId) {
            finalMetals.push({ MetalTypeId: parseInt(metalTypeId), Weight: gram });
        }
        formData.append('metalsJson', JSON.stringify(finalMetals));

        tempImages.forEach(file => {
            formData.append('imageFiles', file);
        });

        const url = id > 0 ? '/Admin/UpdateProduct' : '/Admin/CreateProduct';

        try {
            const res = await fetch(url, { method: 'POST', body: formData });
            const result = await res.json();
            if (result.isSuccess) {
                Swal.fire({ icon: 'success', title: 'Başarılı!', text: 'Ürün başarıyla kaydedildi!' }).then(() => {
                    location.reload();
                });
            } else {
                Swal.fire({ icon: 'error', title: 'Hata!', text: (result.errors ? result.errors.join(', ') : 'İşlem başarısız') });
            }
        } catch (err) {
            Swal.fire({ icon: 'error', title: 'Hata!', text: err.message });
        } finally {
            btn.disabled = false;
            btn.innerText = "Kaydet";
        }
    });

    function deleteProduct(id) {
        Swal.fire({
            title: 'Emin misiniz?',
            text: 'Bu ürünü silmek istediğinize emin misiniz? (Ürün kalıcı olarak değil, sistemde gizlenecek şekilde işaretlenecektir)',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#ef4444',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'Evet, Sil!',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch(`http://localhost/api/Products/${id}`, {
                    method: 'DELETE'
                })
                .then(res => res.json())
                .then(async data => {
                    if(data.isSuccess || data.success) {
                        await fetch('/Product/ClearCache', { method: 'POST' });
                        Swal.fire(
                            'Silindi!',
                            'Ürün başarıyla silindi.',
                            'success'
                        ).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire('Hata!', data.message || 'Ürün silinirken bir hata oluştu.', 'error');
                    }
                })
                .catch(err => {
                    console.error(err);
                    Swal.fire('Hata!', 'Sunucuya bağlanılamadı.', 'error');
                });
            }
        });
    }

    $(document).ready(function() {
        var table = $('#productsTable').DataTable({
            pageLength: 50,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tümü"]],
            language: {
                "sDecimal":        ",",
                "sEmptyTable":     "Tabloda herhangi bir veri mevcut değil",
                "sInfo":           "_TOTAL_ kayıttan _START_ - _END_ arasındaki kayıtlar gösteriliyor",
                "sInfoEmpty":      "Kayıt yok",
                "sInfoFiltered":   "(_MAX_ kayıt içerisinden bulunan)",
                "sInfoPostFix":    "",
                "sInfoThousands":  ".",
                "sLengthMenu":     "Sayfada _MENU_ kayıt göster",
                "sLoadingRecords": "Yükleniyor...",
                "sProcessing":     "İşleniyor...",
                "sSearch":         "Ara:",
                "sZeroRecords":    "Eşleşen kayıt bulunamadı",
                "oPaginate": {
                    "sFirst":    "İlk",
                    "sLast":     "Son",
                    "sNext":     "Sonraki",
                    "sPrevious": "Önceki"
                },
                "oAria": {
                    "sSortAscending":  ": artan sütun sıralamasını aktifleştir",
                    "sSortDescending": ": azalan sütun sıralamasını aktifleştir"
                }
            }
        });

        $('#codeFilter').on('keyup input', function() {
            table.column(1).search(this.value).draw();
        });

        $('#categoryFilter').on('change', function() {
            table.column(2).search(this.value).draw();
        });

        $('#btnToggleFilters').on('click', function() {
            $('#filterCollapse').slideToggle(200);
        });
    });
