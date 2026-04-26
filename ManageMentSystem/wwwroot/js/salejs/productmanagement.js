// Product Management Functions for Sales

// Add product to sale
function addProductToSale(productId, productName, price, purchasePrice, availableStock, barcode) {
    // Check if product already exists
    const existingProduct = selectedProducts.find(p => p.productId === productId);
    if (existingProduct) {
        const newTotalQuantity = existingProduct.quantity + 1;
        if (newTotalQuantity > availableStock) {
            showOutOfStockWarning(productName, availableStock);
            return;
        }
        existingProduct.quantity = newTotalQuantity;
        updateProductRow(existingProduct);
    } else {
        const newProduct = {
            productId: productId,
            productName: productName,
            price: price,
            purchasePrice: purchasePrice,
            customSalePrice: price, // يبدأ بسعر البيع الحالي
            quantity: 1,
            availableStock: availableStock,
            barcode: barcode || ''
        };
        selectedProducts.push(newProduct);
        addProductRow(newProduct);
    }

    updateTotalAmount();
    $('#productSearchResults').hide();
    $('#productSearch').val('');

    // Show success message for NewSale page
    if (window.location.pathname.includes('/NewSale')) {
        handleProductAddedSuccess(productName);
    }
}

// Add product row to table
function addProductRow(product) {
    const profit = (product.customSalePrice - product.purchasePrice) * product.quantity;
    const profitStatus = profit > 0 ? 'ربح' : profit < 0 ? 'خسارة' : 'تعادل';
    const barcodeDisplay = product.barcode ?
        `<span class="badge bg-light text-dark" title="باركود المنتج">${product.barcode}</span>` :
        '<span class="text-muted">-</span>';

    // التحقق من الصلاحيات (إذا كانت متاحة)
    const showPurchasePrice = typeof canViewPurchasePrice !== 'undefined' ? canViewPurchasePrice : true;
    const showSalePrice = typeof canViewSalePrice !== 'undefined' ? canViewSalePrice : true;
    const showProfitMargin = typeof canViewProfitMargin !== 'undefined' ? canViewProfitMargin : true;

    let row = `
        <tr data-product-id="${product.productId}">
            <td>${product.productName}</td>
            <td>${barcodeDisplay}</td>
            <td>
                <input type="number" class="form-control form-control-sm" 
                       value="${product.quantity}" min="1" 
                       onchange="updateProductQuantity(${product.productId}, this.value)">
            </td>`;

    // سعر الشراء (إذا كان لديه الصلاحية)
    if (showPurchasePrice) {
        row += `<td><span class="badge bg-info">${product.purchasePrice.toFixed(2)}</span></td>`;
    }

    // سعر البيع وسعر البيع المخصص (إذا كان لديه الصلاحية)
    if (showSalePrice) {
        row += `
            <td><span class="badge bg-secondary">${product.price.toFixed(2)}</span></td>
            <td>
                <input type="number" class="form-control form-control-sm" 
                       value="${product.customSalePrice.toFixed(2)}" step="0.01" min="0"
                       onchange="updateCustomSalePrice(${product.productId}, this.value)">
            </td>`;
    }

    // الربح/الخسارة (إذا كان لديه الصلاحية)
    if (showProfitMargin) {
        row += `
            <td>
                <span class="badge ${profit > 0 ? 'bg-success' : profit < 0 ? 'bg-danger' : 'bg-secondary'}">
                    ${profitStatus}: ${profit.toFixed(2)}
                </span>
            </td>`;
    }

    row += `
            <td>${(product.quantity * product.customSalePrice).toFixed(2)}</td>
            <td>
                <button type="button" class="btn btn-sm btn-outline-danger" 
                        onclick="removeProduct(${product.productId})">
                    <i data-lucide="trash-2" class="w-4 h-4"></i>
                </button>
            </td>
        </tr>
    `;
    $('#selectedProductsTable tbody').append(row);
    lucide.createIcons();
}

// Update product row
function updateProductRow(product) {
    const row = $(`#selectedProductsTable tbody tr[data-product-id="${product.productId}"]`);

    // تحديث الكمية (العمود 2 دائماً)
    row.find('td:eq(2) input').val(product.quantity);

    // تحديث سعر البيع المخصص (العمود 5 بعد عمود سعر الشراء)
    const customPriceIndex = 5;
    row.find(`td:eq(${customPriceIndex}) input`).val(product.customSalePrice.toFixed(2));

    const profit = (product.customSalePrice - product.purchasePrice) * product.quantity;
    const profitStatus = profit > 0 ? 'ربح' : profit < 0 ? 'خسارة' : 'تعادل';

    // تحديث الربح/الخسارة (العمود 6)
    const profitCell = row.find('td:eq(6) span');
    if (profitCell.length) {
        profitCell
            .removeClass('bg-success bg-danger bg-secondary')
            .addClass(profit > 0 ? 'bg-success' : profit < 0 ? 'bg-danger' : 'bg-secondary')
            .text(`${profitStatus}: ${profit.toFixed(2)}`);
    }

    // تحديث المجموع
    updateTotalAmount();
}

// Update product quantity
function updateProductQuantity(productId, quantity) {
    const product = selectedProducts.find(p => p.productId === productId);
    if (product) {
        const newQuantity = parseInt(quantity);
        if (newQuantity > product.availableStock) {
            alert(`لا يمكن طلب كمية أكبر من المتوفر للمنتج "${product.productName}". المتوفر: ${product.availableStock}`);
            // Reset to previous value
            const row = $(`#selectedProductsTable tbody tr[data-product-id="${productId}"]`);
            row.find('td:eq(2) input').val(product.quantity);
            return;
        }
        product.quantity = newQuantity;
        updateProductRow(product);
        updateTotalAmount();
    }
}

// Update custom sale price
function updateCustomSalePrice(productId, price) {
    const product = selectedProducts.find(p => p.productId === productId);
    if (product) {
        const newPrice = parseFloat(price);
        if (newPrice < 0) {
            alert('السعر يجب أن يكون أكبر من أو يساوي صفر');
            // Reset to previous value
            const row = $(`#selectedProductsTable tbody tr[data-product-id="${productId}"]`);
            const customPriceIndex = 5;
            row.find(`td:eq(${customPriceIndex}) input`).val(product.customSalePrice.toFixed(2));
            return;
        }
        product.customSalePrice = newPrice;
        updateProductRow(product);
        updateTotalAmount();
    }
}

// Remove product
function removeProduct(productId) {
    const product = selectedProducts.find(p => p.productId === productId);
    selectedProducts = selectedProducts.filter(p => p.productId !== productId);
    $(`#selectedProductsTable tbody tr[data-product-id="${productId}"]`).remove();
    updateTotalAmount();

    // Show removal message for NewSale page
    if (window.location.pathname.includes('/NewSale') && product) {
        handleProductRemoved(product.productName);
    }
}

// Edit product management functions
function addEditProductToSale(productId, productName, price, purchasePrice, availableStock, barcode) {
    const existingProduct = editSelectedProducts.find(p => p.productId === productId);
    if (existingProduct) {
        const newTotalQuantity = existingProduct.quantity + 1;
        if (newTotalQuantity > availableStock) {
            showOutOfStockWarning(productName, availableStock);
            return;
        }
        existingProduct.quantity = newTotalQuantity;
        updateEditProductRow(existingProduct);
    } else {
        const newProduct = {
            productId: productId,
            productName: productName,
            price: price,
            purchasePrice: purchasePrice,
            customSalePrice: price,
            quantity: 1,
            availableStock: availableStock,
            barcode: barcode || ''
        };
        editSelectedProducts.push(newProduct);
        addEditProductRow(newProduct);
    }

    updateEditTotalAmount();
    $('#editProductSearchResults').hide();
    $('#editProductSearch').val('');
}

// Add edit product row
function addEditProductRow(product) {
    const profit = (product.customSalePrice - product.purchasePrice) * product.quantity;
    const profitStatus = profit > 0 ? 'ربح' : profit < 0 ? 'خسارة' : 'تعادل';

    const barcodeDisplay = product.barcode ?
        `<span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-mono font-bold border border-gray-200" title="باركود المنتج">${product.barcode}</span>` :
        '<span class="text-gray-400 text-xs font-bold">-</span>';

    const profitBadge = profit > 0
        ? `<span class="bg-emerald-100 text-emerald-700 px-2 py-1 rounded-full text-xs font-bold border border-emerald-200">${profitStatus}: ${profit.toFixed(2)}</span>`
        : profit < 0
            ? `<span class="bg-red-100 text-red-700 px-2 py-1 rounded-full text-xs font-bold border border-red-200">${profitStatus}: ${profit.toFixed(2)}</span>`
            : `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded-full text-xs font-bold border border-gray-200">${profitStatus}: ${profit.toFixed(2)}</span>`;

    const row = `
        <tr data-product-id="${product.productId}" class="hover:bg-gray-50">
            <td class="px-4 py-2 text-gray-800 font-medium whitespace-nowrap">${product.productName}</td>
            <td class="px-4 py-2 whitespace-nowrap">${barcodeDisplay}</td>
            <td class="px-4 py-2 whitespace-nowrap">
                <input type="number" class="w-20 px-2 py-1 border border-gray-300 rounded-lg focus:ring-1 focus:ring-primary focus:border-primary text-sm" 
                       value="${product.quantity}" min="1" 
                       onchange="updateEditProductQuantity(${product.productId}, this.value)">
            </td>
            <td class="px-4 py-2 whitespace-nowrap"><span class="bg-gray-50 text-primary px-2 py-1 rounded-full text-xs font-bold">${product.purchasePrice.toFixed(2)}</span></td>
            <td class="px-4 py-2 whitespace-nowrap"><span class="bg-gray-100 text-gray-700 px-2 py-1 rounded-full text-xs font-bold">${product.price.toFixed(2)}</span></td>
            <td class="px-4 py-2 whitespace-nowrap">
                <input type="number" class="w-24 px-2 py-1 border border-gray-300 rounded-lg focus:ring-1 focus:ring-primary focus:border-primary text-sm" 
                       value="${product.customSalePrice.toFixed(2)}" step="0.01" min="0"
                       onchange="updateEditCustomSalePrice(${product.productId}, this.value)">
            </td>
            <td class="px-4 py-2 whitespace-nowrap">${profitBadge}</td>
            <td class="px-4 py-2 font-bold text-gray-800 whitespace-nowrap">${(product.quantity * product.customSalePrice).toFixed(2)}</td>
            <td class="px-4 py-2 whitespace-nowrap">
                <button type="button" class="w-8 h-8 flex items-center justify-center rounded-lg text-red-600 hover:bg-red-50 transition-colors" 
                        onclick="removeEditProduct(${product.productId})">
                    <i data-lucide="trash-2" class="w-4 h-4"></i>
                </button>
            </td>
        </tr>
    `;
    $('#editSelectedProductsTable tbody').append(row);
    lucide.createIcons();
}


// Update edit product row
function updateEditProductRow(product) {
    const row = $(`#editSelectedProductsTable tbody tr[data-product-id="${product.productId}"]`);
    row.find('td:eq(2) input').val(product.quantity);
    row.find('td:eq(5) input').val(product.customSalePrice.toFixed(2));

    const profit = (product.customSalePrice - product.purchasePrice) * product.quantity;
    const profitStatus = profit > 0 ? 'ربح' : profit < 0 ? 'خسارة' : 'تعادل';

    const profitBadge = profit > 0
        ? `<span class="bg-emerald-100 text-emerald-700 px-2 py-1 rounded-full text-xs font-bold border border-emerald-200">${profitStatus}: ${profit.toFixed(2)}</span>`
        : profit < 0
            ? `<span class="bg-red-100 text-red-700 px-2 py-1 rounded-full text-xs font-bold border border-red-200">${profitStatus}: ${profit.toFixed(2)}</span>`
            : `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded-full text-xs font-bold border border-gray-200">${profitStatus}: ${profit.toFixed(2)}</span>`;

    row.find('td:eq(6)').html(profitBadge);
    row.find('td:eq(7)').text((product.quantity * product.customSalePrice).toFixed(2));
}


// Update edit product quantity
function updateEditProductQuantity(productId, quantity) {
    const product = editSelectedProducts.find(p => p.productId === productId);
    if (product) {
        product.quantity = parseInt(quantity);
        updateEditProductRow(product);
        updateEditTotalAmount();
    }
}

// Update edit custom sale price
function updateEditCustomSalePrice(productId, price) {
    const product = editSelectedProducts.find(p => p.productId === productId);
    if (product) {
        product.customSalePrice = parseFloat(price);
        updateEditProductRow(product);
        updateEditTotalAmount();
    }
}

// Remove edit product
function removeEditProduct(productId) {
    editSelectedProducts = editSelectedProducts.filter(p => p.productId !== productId);
    displayEditSelectedProducts();
    updateEditTotalAmount();
}

// Display edit selected products
function displayEditSelectedProducts() {
    const tbody = $('#editSelectedProductsTable tbody');
    tbody.empty();

    editSelectedProducts.forEach((product, index) => {
        const profit = (product.customSalePrice - product.purchasePrice) * product.quantity;
        const profitStatus = profit > 0 ? 'ربح' : profit < 0 ? 'خسارة' : 'تعادل';

        const barcodeDisplay = product.barcode ?
            `<span class="bg-gray-100 text-gray-700 px-2 py-0.5 rounded text-xs font-mono font-bold border border-gray-200" title="باركود المنتج">${product.barcode}</span>` :
            '<span class="text-gray-400 text-xs font-bold">-</span>';

        const profitBadge = profit > 0
            ? `<span class="bg-emerald-100 text-emerald-700 px-2 py-1 rounded-full text-xs font-bold border border-emerald-200">${profitStatus}: ${profit.toFixed(2)}</span>`
            : profit < 0
                ? `<span class="bg-red-100 text-red-700 px-2 py-1 rounded-full text-xs font-bold border border-red-200">${profitStatus}: ${profit.toFixed(2)}</span>`
                : `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded-full text-xs font-bold border border-gray-200">${profitStatus}: ${profit.toFixed(2)}</span>`;

        const row = `
            <tr data-product-id="${product.productId}" class="hover:bg-gray-50">
                <td class="px-4 py-2 text-gray-800 font-medium whitespace-nowrap">${product.productName}</td>
                <td class="px-4 py-2 whitespace-nowrap">${barcodeDisplay}</td>
                <td class="px-4 py-2 whitespace-nowrap">
                    <input type="number" class="w-20 px-2 py-1 border border-gray-300 rounded-lg focus:ring-1 focus:ring-primary focus:border-primary text-sm" 
                           value="${product.quantity}" min="1" 
                           onchange="updateEditProductQuantity(${product.productId}, this.value)">
                </td>
                <td class="px-4 py-2 whitespace-nowrap"><span class="bg-gray-50 text-primary px-2 py-1 rounded-full text-xs font-bold">${product.purchasePrice.toFixed(2)}</span></td>
                <td class="px-4 py-2 whitespace-nowrap"><span class="bg-gray-100 text-gray-700 px-2 py-1 rounded-full text-xs font-bold">${product.unitPrice.toFixed(2)}</span></td>
                <td class="px-4 py-2 whitespace-nowrap">
                    <input type="number" class="w-24 px-2 py-1 border border-gray-300 rounded-lg focus:ring-1 focus:ring-primary focus:border-primary text-sm" 
                           value="${product.customSalePrice.toFixed(2)}" step="0.01" min="0"
                       onchange="updateEditCustomSalePrice(${product.productId}, this.value)">
                </td>
                <td class="px-4 py-2 whitespace-nowrap">${profitBadge}</td>
                <td class="px-4 py-2 font-bold text-gray-800 whitespace-nowrap">${(product.quantity * product.customSalePrice).toFixed(2)}</td>
                <td class="px-4 py-2 whitespace-nowrap">
                    <button type="button" class="w-8 h-8 flex items-center justify-center rounded-lg text-red-600 hover:bg-red-50 transition-colors" 
                            onclick="removeEditProduct(${product.productId})">
                        <i data-lucide="trash-2" class="w-4 h-4"></i>
                    </button>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
    lucide.createIcons();
}


// Expose functions to global scope for inline onclick handlers
window.addProductToSale = window.addProductToSale || addProductToSale;
window.updateProductQuantity = window.updateProductQuantity || updateProductQuantity;
window.updateCustomSalePrice = window.updateCustomSalePrice || updateCustomSalePrice;
window.removeProduct = window.removeProduct || removeProduct;
window.addEditProductToSale = window.addEditProductToSale || addEditProductToSale;
window.updateEditProductQuantity = window.updateEditProductQuantity || updateEditProductQuantity;
window.updateEditCustomSalePrice = window.updateEditCustomSalePrice || updateEditCustomSalePrice;
window.removeEditProduct = window.removeEditProduct || removeEditProduct;