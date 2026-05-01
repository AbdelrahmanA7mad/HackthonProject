// Product Search Functions for Sales

// Real-time search with debouncing
$(document).on('input', '#productSearch', function () {
    clearTimeout(searchTimeout);
    const searchTerm = $(this).val().trim();

    if (searchTerm.length === 0) {
        $('#productSearchResults').addClass('hidden').hide();
        return;
    }

    // Debounce the search to avoid too many calls
    searchTimeout = setTimeout(function () {
        searchProducts(searchTerm);
    }, 300);
});

// Search products
function searchProducts(searchTerm) {
    const filteredProducts = allProducts.filter(product =>
        product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (product.description && product.description.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (product.barcode && product.barcode.toLowerCase().includes(searchTerm.toLowerCase()))
    );

    displaySearchResults(filteredProducts);
}

// Display search results
function displaySearchResults(products) {
    const tbody = $('#productSearchTable tbody');
    tbody.empty();

    if (products.length === 0) {
        const colspan = 7;
        tbody.append(`<tr><td colspan="${colspan}" class="text-center text-muted">لا توجد نتائج</td></tr>`);
    } else {
        // Limit to first 10 results for better performance
        const limitedProducts = products.slice(0, 10);

        limitedProducts.forEach(product => {
            const stockStatus = product.quantity > 0 ?
                `<span class="badge bg-success">متوفر: ${product.quantity}</span>` :
                `<span class="badge bg-danger">غير متوفر</span>`;

            const barcodeDisplay = product.barcode ?
                `<span class="badge bg-light text-dark" title="باركود المنتج">${product.barcode}</span>` :
                '<span class="text-muted">-</span>';

            const addButton = product.quantity > 0 ?
                `<button type="button" class="btn btn-sm btn-outline-success" 
                        onclick="addProductToSale(${product.id}, '${product.name.replace(/'/g, "\\'")}', ${product.price}, ${product.purchasePrice}, ${product.quantity}, '${product.barcode || ''}')">
                    <i data-lucide="plus" class="w-4 h-4 inline-block"></i> إضافة
                </button>` :
                `<button type="button" class="btn btn-sm btn-outline-secondary" disabled>
                    <i data-lucide="x" class="w-4 h-4 inline-block"></i> غير متوفر
                </button>`;

            // التحقق من الصلاحيات
            const showPurchasePrice = typeof canViewPurchasePrice !== 'undefined' ? canViewPurchasePrice : true;
            const showSalePrice = typeof canViewSalePrice !== 'undefined' ? canViewSalePrice : true;

            let row = `
                <tr>
                    <td><strong>${product.name}</strong></td>
                    <td>${product.description || '-'}</td>
                    <td>${barcodeDisplay}</td>`;

            // عرض سعر الشراء (إذا كان لديه الصلاحية)
            if (showPurchasePrice) {
                row += `<td><span class="badge bg-info">${product.purchasePrice.toFixed(2)}</span></td>`;
            }

            // عرض سعر البيع (إذا كان لديه الصلاحية)
            if (showSalePrice) {
                row += `<td><span class="badge bg-success">${product.price.toFixed(2)}</span></td>`;
            }

            row += `
                    <td>${stockStatus}</td>
                    <td>${addButton}</td>
                </tr>
            `;
            tbody.append(row);
        });
        lucide.createIcons();

        if (products.length > 10) {
            const colspan = 7;
            tbody.append(`<tr><td colspan="${colspan}" class="text-center text-muted">عرض أول 10 نتائج من ${products.length} منتج</td></tr>`);
        }
    }

    $('#productSearchResults').removeClass('hidden').show();
}

// Edit product search functionality
$(document).on('input', '#editProductSearch', function () {
    clearTimeout(searchTimeout);
    const searchTerm = $(this).val().trim();

    if (searchTerm.length === 0) {
        $('#editProductSearchResults').addClass('hidden').hide();
        return;
    }

    searchTimeout = setTimeout(function () {
        searchEditProducts(searchTerm);
    }, 300);
});

// Search edit products
function searchEditProducts(searchTerm) {
    const filteredProducts = allProducts.filter(product =>
        product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (product.description && product.description.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (product.barcode && product.barcode.toLowerCase().includes(searchTerm.toLowerCase()))
    );

    displayEditSearchResults(filteredProducts);
}

// Display edit search results
function displayEditSearchResults(products) {
    const tbody = $('#editProductSearchTable tbody');
    tbody.empty();

    if (products.length === 0) {
        const colspan = 7;
        tbody.append(`<tr><td colspan="${colspan}" class="px-6 py-8 text-center text-gray-500 font-bold">لا توجد نتائج مطابقة للبحث</td></tr>`);
    } else {
        const limitedProducts = products.slice(0, 10);

        limitedProducts.forEach(product => {
            const stockStatus = product.quantity > 0 ?
                `<span class="bg-emerald-100 text-emerald-800 text-xs font-bold px-2.5 py-0.5 rounded-lg border border-emerald-200">متوفر: ${product.quantity}</span>` :
                `<span class="bg-red-100 text-red-800 text-xs font-bold px-2.5 py-0.5 rounded-lg border border-red-200">غير متوفر</span>`;

            const barcodeDisplay = product.barcode ?
                `<span class="bg-gray-100 text-gray-700 text-xs font-mono font-bold px-2 py-1 rounded border border-gray-200" title="باركود المنتج">${product.barcode}</span>` :
                '<span class="text-gray-400 text-xs font-bold">-</span>';

            const addButton = product.quantity > 0 ?
                `<button type="button" class="group flex items-center justify-center gap-1 w-full bg-white hover:bg-emerald-50 text-emerald-700 border border-emerald-200 hover:border-emerald-300 px-3 py-1.5 rounded-lg transition-all shadow-sm active:scale-95 text-xs font-bold" 
                        onclick="addEditProductToSale(${product.id}, '${product.name.replace(/'/g, "\\'")}', ${product.price}, ${product.purchasePrice}, ${product.quantity}, '${product.barcode || ''}')">
                    <i data-lucide="plus" class="w-4 h-4 group-hover:scale-110 transition-transform"></i> إضافة
                </button>` :
                `<button type="button" class="w-full bg-gray-50 text-gray-400 border border-gray-200 px-3 py-1.5 rounded-lg cursor-not-allowed text-xs font-bold" disabled>
                    <i data-lucide="x" class="w-4 h-4"></i> نفذت
                </button>`;

            let row = `
                <tr class="hover:bg-gray-50 border-b border-gray-100 last:border-0 transition-colors">
                    <td class="px-4 py-3 text-right">
                        <div class="font-bold text-gray-800">${product.name}</div>
                        <div class="text-xs text-gray-500 mt-0.5 truncate max-w-[200px]">${product.description || ''}</div>
                    </td>
                    <td class="px-4 py-3 text-right whitespace-nowrap">${barcodeDisplay}</td>
                    <td class="px-4 py-3 text-right whitespace-nowrap"><span class="bg-gray-50 text-primary px-2 py-1 rounded text-xs font-bold border border-gray-100">${product.purchasePrice.toFixed(2)}</span></td>
                    <td class="px-4 py-3 text-right whitespace-nowrap"><span class="bg-bgSubtle text-primary px-2 py-1 rounded text-xs font-bold border border-gray-200">${product.price.toFixed(2)}</span></td>
                    <td class="px-4 py-3 text-center whitespace-nowrap">${stockStatus}</td>
                    <td class="px-4 py-3 text-center whitespace-nowrap w-24">${addButton}</td>
                </tr>
            `;
            tbody.append(row);
        });
        lucide.createIcons();

        if (products.length > 10) {
            const colspan = 7;
            tbody.append(`<tr><td colspan="${colspan}" class="px-6 py-3 text-center text-xs font-bold text-gray-500 bg-gray-50">عرض أول 10 نتائج من ${products.length} منتج</td></tr>`);
        }
    }

    $('#editProductSearchResults').removeClass('hidden').show();
}

// Hide search results when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('#productSearch, #productSearchResults').length) {
        $('#productSearchResults').addClass('hidden').hide();
    }
    if (!$(e.target).closest('#editProductSearch, #editProductSearchResults').length) {
        $('#editProductSearchResults').addClass('hidden').hide();
    }
});