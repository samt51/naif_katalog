
            document.addEventListener('DOMContentLoaded', function() {
                const claritySelect = document.getElementById('filterClarityId');
                const stoneSelect = document.getElementById('filterStoneId');
                const stoneTypeSelect = document.getElementById('filterStoneTypeId');

                function updateDependentDropdowns() {
                    const selectedClarity = claritySelect.value;
                    const validTypes = new Set();
                    
                    // Filter Parti Adı (stoneId)
                    Array.from(stoneSelect.options).forEach(opt => {
                        if (opt.value === "") return;
                        
                        const clarityMatch = selectedClarity === "" || opt.getAttribute('data-clarity') === selectedClarity;
                        opt.style.display = clarityMatch ? '' : 'none';
                        
                        if(clarityMatch) {
                            validTypes.add(opt.getAttribute('data-type'));
                        } else if (stoneSelect.value === opt.value) {
                            stoneSelect.value = "";
                        }
                    });

                    // Filter Taş Türü (stoneTypeId) based on available stones
                    Array.from(stoneTypeSelect.options).forEach(opt => {
                        if (opt.value === "") return;
                        
                        const typeMatch = selectedClarity === "" || validTypes.has(opt.value);
                        opt.style.display = typeMatch ? '' : 'none';
                        
                        if(!typeMatch && stoneTypeSelect.value === opt.value) {
                            stoneTypeSelect.value = "";
                        }
                    });
                }

                if(claritySelect) {
                    claritySelect.addEventListener('change', updateDependentDropdowns);
                    // Initial run to hide options if page loaded with a clarity filter
                    updateDependentDropdowns();
                }
            });
        