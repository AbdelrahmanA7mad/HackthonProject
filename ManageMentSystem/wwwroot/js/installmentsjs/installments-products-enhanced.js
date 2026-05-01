// ===== Enhanced Product Management for Installments =====

let allProducts = [];
let selectedProducts = [];

// تحميل جميع المنتجات عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', function () {
    loadAllProducts();
});

// دالة تحميل جميع المنتجات
function loadAllProducts() {
    // سيتم تعيينها من الصفحة الرئيسية
}

// البحث عن المنتجات (مع دعم الباركود)
$(document).on('input', '#productSearch', function () {
    const searchTerm = $(this).val().toLowerCase().trim();

    if (searchTerm.length === 0) {
        $('#productSearchResults').addClass('hidden').hide();
        return;
    }

    const matchingProducts = allProducts.filter(product =>
        product.name.toLowerCase().includes(searchTerm) ||
        (product.description && product.description.toLowerCase().includes(searchTerm)) ||
        (product.barcode && product.barcode.toLowerCase().includes(searchTerm))
    );

    if (matchingProducts.length > 0) {
        let tbody = $('#productSearchTable tbody');
        tbody.empty();

        matchingProducts.slice(0, 10).forEach(product => {
            const stockClass = product.quantity > 10 ? 'text-success' : product.quantity > 0 ? 'text-warning' : 'text-danger';
            const stockIcon = product.quantity > 10 ? 'check-circle' : product.quantity > 0 ? 'alert-circle' : 'x-circle';

            // Escape single quotes in product name for onclick
            const escapedName = product.name.replace(/'/g, "\\'").replace(/"/g, '\\"');

            const row = `
                <tr class="hover:bg-gray-50 cursor-pointer" 
                    onclick="addProductToInstallmentAndHide(${product.id}, '${escapedName}', ${product.salePrice}, ${product.quantity}, '${product.barcode || ''}'); return false;">
                    <td class="px-4 py-3">
                        <div class="font-medium text-gray-900">${product.name}</div>
                        ${product.barcode ? `<div class="text-xs text-gray-500"><i data-lucide="barcode"></i> ${product.barcode}</div>` : ''}
                    </td>
                    <td class="px-4 py-3 text-center">
                        <span class="px-2 py-1 text-xs font-bold rounded-full bg-indigo-100 text-primary">${product.salePrice.toFixed(2)}</span>
                    </td>
                    <td class="px-4 py-3 text-center">
                        <span class="${stockClass}">
                            <i data-lucide="${stockIcon}"></i> ${product.quantity}
                        </span>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
        lucide.createIcons();

        if (matchingProducts.length > 10) {
            tbody.append(`<tr><td colspan="3" class="text-center text-gray-500 text-sm py-2">عرض أول 10 نتائج من ${matchingProducts.length} منتج</td></tr>`);
        }
    } else {
        $('#productSearchTable tbody').html('<tr><td colspan="3" class="text-center text-gray-500 py-4"><i data-lucide="search" class="text-2xl mb-2"></i><br>لا توجد منتجات مطابقة</td></tr>');
        lucide.createIcons();
    }

    $('#productSearchResults').removeClass('hidden').show();
});

// Wrapper function to add product and immediately hide dropdown
function addProductToInstallmentAndHide(productId, productName, price, availableStock, barcode) {
    // إخفاء القائمة فوراً
    $('#productSearchResults').addClass('hidden').hide();
    $('#productSearch').val('');

    // إضافة المنتج
    addProductToInstallment(productId, productName, price, availableStock, barcode);
}

// Handle Enter key for product search (barcode support)
$(document).on('keydown', '#productSearch', function (e) {
    if (e.key === 'Enter' || e.keyCode === 13) {
        e.preventDefault();
        const searchTerm = $(this).val().trim();

        // Check if it's a barcode (try exact match first)
        const product = allProducts.find(p =>
            (p.barcode || '').toLowerCase() === searchTerm.toLowerCase()
        );

        if (product && product.quantity > 0) {
            addProductToInstallmentAndHide(product.id, product.name, product.salePrice, product.quantity, product.barcode || '');
        }
    }
});

// إضافة منتج للتقسيط مع notification وصوت
function addProductToInstallment(productId, productName, price, availableStock, barcode = '') {
    // التحقق من وجود المنتج
    const existingProduct = selectedProducts.find(p => p.productId === productId);
    if (existingProduct) {
        const newTotalQuantity = existingProduct.quantity + 1;
        if (newTotalQuantity > availableStock) {
            showWarningNotification(`المخزون غير كافٍ`, `المتوفر من "${productName}": ${availableStock}`);
            playWarningSound();
            return;
        }
        existingProduct.quantity = newTotalQuantity;
        updateProductRow(existingProduct);
    } else {
        const newProduct = {
            productId: productId,
            productName: productName,
            price: price,
            quantity: 1,
            description: '',
            availableStock: availableStock,
            barcode: barcode
        };
        selectedProducts.push(newProduct);
        addProductRow(newProduct);
    }

    updateTotalAmount();

    // Show success notification and play sound
    showSuccessNotification('تم إضافة المنتج', productName);
    playSuccessSound();
}

// إضافة صف منتج للجدول
function addProductRow(product) {
    const index = selectedProducts.length - 1;
    const row = `
        <tr data-product-id="${product.productId}" class="hover:bg-gray-50 transition-colors">
            <td class="px-3 py-2">
                <div class="font-medium text-gray-900">${product.productName}</div>
                ${product.barcode ? `<div class="text-xs text-gray-500"><i data-lucide="barcode"></i> ${product.barcode}</div>` : ''}
                <input type="hidden" name="Items[${index}].ProductId" value="${product.productId}">
                <input type="hidden" name="Items[${index}].ProductName" value="${product.productName}">
            </td>
            <td class="px-3 py-2 text-center">
                <input type="number" class="w-20 text-center border-gray-300 rounded-md shadow-sm focus:ring-primary focus:border-primary text-sm py-1"
                       value="${product.quantity}" min="1" max="${product.availableStock}"
                       onchange="updateProductQuantity(${product.productId}, this.value)"
                       name="Items[${index}].Quantity">
            </td>
            <td class="px-3 py-2 text-center">
                <input type="number" class="w-24 text-center border-gray-300 rounded-md shadow-sm focus:ring-primary focus:border-primary text-sm py-1"
                       value="${product.price.toFixed(2)}" step="0.01" min="0"
                       onchange="updateProductPrice(${product.productId}, this.value)"
                       name="Items[${index}].UnitPrice">
            </td>
            <td class="px-3 py-2 text-center font-mono font-bold total-price">${(product.quantity * product.price).toFixed(2)}</td>
            <td class="px-3 py-2 text-center">
                <button type="button" class="text-red-600 hover:text-red-800 hover:bg-red-50 p-1.5 rounded transition-colors" 
                        onclick="removeProduct(${product.productId})" title="حذف">
                    <i data-lucide="trash-2"></i>
                </button>
            </td>
        </tr>
    `;
    $('#selectedProductsTable tbody').append(row);
    lucide.createIcons();
}

// تحديث صف المنتج
function updateProductRow(product) {
    const row = $(`#selectedProductsTable tbody tr[data-product-id="${product.productId}"]`);
    const quantityInput = row.find('input[name*=".Quantity"]');
    const priceInput = row.find('input[name*=".UnitPrice"]');
    const totalCell = row.find('.total-price');

    quantityInput.val(product.quantity);
    priceInput.val(product.price.toFixed(2));
    totalCell.text((product.quantity * product.price).toFixed(2));
}

// تحديث كمية المنتج
function updateProductQuantity(productId, quantity) {
    const product = selectedProducts.find(p => p.productId === productId);
    if (product) {
        const newQuantity = parseInt(quantity) || 1;
        if (newQuantity > product.availableStock) {
            showWarningNotification(`المخزون غير كافٍ`, `المتوفر من "${product.productName}": ${product.availableStock}`);
            $(`tr[data-product-id="${productId}"] input[name*=".Quantity"]`).val(product.quantity);
            playWarningSound();
            return;
        }
        product.quantity = newQuantity;
        updateProductRow(product);
        updateTotalAmount();
    }
}

// تحديث سعر المنتج
function updateProductPrice(productId, price) {
    const product = selectedProducts.find(p => p.productId === productId);
    if (product) {
        product.price = parseFloat(price) || 0;
        updateProductRow(product);
        updateTotalAmount();
    }
}

// إزالة منتج
function removeProduct(productId) {
    const product = selectedProducts.find(p => p.productId === productId);
    const index = selectedProducts.findIndex(p => p.productId === productId);
    if (index !== -1) {
        selectedProducts.splice(index, 1);
        $(`#selectedProductsTable tbody tr[data-product-id="${productId}"]`).remove();

        // إعادة ترقيم الحقول
        reindexSelectedProducts();
        updateTotalAmount();

        // Show removal notification
        if (product) {
            showInfoNotification('تم الحذف', `تم حذف "${product.productName}"`);
        }
    }
}

// إعادة ترقيم المنتجات المختارة
function reindexSelectedProducts() {
    $('#selectedProductsTable tbody tr').each(function (index) {
        $(this).find('input[name*="Items["]').each(function () {
            const currentName = $(this).attr('name');
            const fieldName = currentName.substring(currentName.lastIndexOf('.'));
            $(this).attr('name', `Items[${index}]${fieldName}`);
        });
    });
}

// تحديث المجموع الإجمالي
function updateTotalAmount() {
    let total = 0;
    selectedProducts.forEach(product => {
        total += product.quantity * product.price;
    });

    $('#totalAmount').val(total);

    // تحديث الحقول المحسوبة فوراً
    if (typeof calculateInstallmentSummary === 'function') {
        calculateInstallmentSummary();
    }
}

// ===== Notification Functions =====

function showSuccessNotification(title, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: 'success',
        title: title,
        text: message
    });
}

function showWarningNotification(title, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
    });

    Toast.fire({
        icon: 'warning',
        title: title,
        text: message
    });
}

function showInfoNotification(title, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 2000,
        timerProgressBar: true,
    });

    Toast.fire({
        icon: 'info',
        title: title,
        text: message
    });
}

// ===== Sound Functions =====

function playSuccessSound() {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        oscillator.type = 'sine';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.1);
    } catch (e) {
        console.log('Audio not supported');
    }
}

function playWarningSound() {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.setValueAtTime(400, audioContext.currentTime);
        oscillator.type = 'sine';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.2);
    } catch (e) {
        console.log('Audio not supported');
    }
}

// إخفاء نتائج البحث عند النقر خارجها
$(document).on('click', function (e) {
    if (!$(e.target).closest('#productSearch, #productSearchResults').length) {
        $('#productSearchResults').addClass('hidden').hide();
    }
});
