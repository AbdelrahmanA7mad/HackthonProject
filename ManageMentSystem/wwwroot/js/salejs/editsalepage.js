// editsalepage.js - For Editing Sales (Based on QuickSale)
let qsAllProducts = [];
let qsCart = [];
let qsSearchTimeout;
let qsPage = 1;
let qsPageSize = 16;
let qsFiltered = [];

// تعريف الصوت
const qsAddSound = new Audio('https://actions.google.com/sounds/v1/cartoon/wood_plank_flicks.ogg');

$(document).ready(function () {

    // Autofocus on barcode scanner
    $('#barcodeScanner').focus();

    // Initialize cart from server data defined in the View
    if (typeof initialCartData !== 'undefined' && initialCartData.length > 0) {
        qsCart = initialCartData;
        renderCart();
        updateTotals(); // Initial total calculation
    }

    // --- Same styling for buttons ---
    $('head').append(`
        <style>
            .qs-hover-overlay {
                position: absolute; inset: 0; z-index: 20;
                display: flex; align-items: center; justify-content: center;
                background-color: rgb(67 ,45 ,215 ,.1);
                opacity: 0; transition: all 0.3s ease;
                border-radius: 0.5rem;
            }
            .qs-product-card:hover .qs-hover-overlay { opacity: 1; }
            .qs-hover-btn {
                width: 60px; height: 60px; border-radius: 50%;
                background-color: #372aac; color: white;
                border: none; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
                display: flex; align-items: center; justify-content: center;
                transform: scale(0.8); transition: transform 0.2s;
                cursor: pointer;
            }
        </style>
    `);

    // 1. Initial Load of Products
    $.get('/Sales/GetProducts', function (data) {
        // Fix mapping
        qsAllProducts = (data || []).map(p => {
            return {
                id: p.id || p.Id,
                name: p.name || p.Name || '',
                quantity: p.quantity ?? p.Quantity ?? 0,
                price: p.price ?? p.Price ?? 0,
                purchasePrice: p.purchasePrice ?? p.PurchasePrice ?? 0,
                barcode: p.barcode || p.Barcode || '',
                categoryId: p.categoryId || p.CategoryId || 0,
                categoryName: p.categoryName || p.CategoryName || '',
                description: p.description || p.Description || ''
            };
        });

        qsFiltered = applyFilters();
        qsPage = 1;
        renderProductsGrid(paginate(qsFiltered));
    });

    // 2. Search & Filter Listeners (Same as QuickSale)
    $('#qsSearch').on('input', function () {
        clearTimeout(qsSearchTimeout);
        const term = ($(this).val() || '').trim().toLowerCase();
        qsSearchTimeout = setTimeout(function () {
            qsFiltered = applyFilters();
            qsPage = 1;
            renderProductsGrid(paginate(qsFiltered));
        }, 200);
    });

    // 3. Pager
    $('#qsPrev').on('click', function () {
        if (qsPage > 1) { qsPage--; renderProductsGrid(paginate(qsFiltered)); }
    });
    $('#qsNext').on('click', function () {
        const total = qsFiltered.length;
        const maxPage = Math.max(1, Math.ceil(total / qsPageSize));
        if (qsPage < maxPage) { qsPage++; renderProductsGrid(paginate(qsFiltered)); }
    });

    // 5. Payment Visibility
    $('#qsPaymentType').on('change', updatePaymentVisibility);
    // Initialize visibility based on current value
    updatePaymentVisibility();

    // 6. Discounts
    $('#qsDiscountPercentage').on('input', function () {
        const percentage = parseFloat($(this).val() || '0');
        const subtotal = qsCart.reduce((sum, i) => sum + (i.customSalePrice * i.quantity), 0);
        const discountAmount = (subtotal * percentage) / 100;
        $('#qsDiscountAmount').val(discountAmount.toFixed(2));
        updateTotals();
    });
    $('#qsDiscountAmount').on('input', function () {
        const discountAmount = parseFloat($(this).val() || '0');
        const subtotal = qsCart.reduce((sum, i) => sum + (i.customSalePrice * i.quantity), 0);
        const percentage = subtotal > 0 ? (discountAmount / subtotal) * 100 : 0;
        $('#qsDiscountPercentage').val(percentage.toFixed(2));
        updateTotals();
    });

    // 7. Barcode Logic
    $('#barcodeScanner').on('input', function () {
        const code = ($(this).val() || '').trim();
        // Allow typing, but trigger on enter or sufficient length if using a scanner that types fast
        // Better to use keypress 'Enter' for barcode scanners typically
    });

    $('#barcodeScanner').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            const code = ($(this).val() || '').trim();
            if (!code) return;

            const product = qsAllProducts.find(p => (p.barcode || '').toLowerCase() === code.toLowerCase());
            if (product) {
                qsAddToCart(product.id, product.name || '', product.price || 0, product.purchasePrice || 0, product.quantity || 0, product.barcode || '');
                $(this).val('');
                showToast(`تم إضافة: ${product.name}`, 'success');
            } else {
                showToast('لم يتم العثور على المنتج', 'error');
                $(this).val('');
            }
        }
    });

    // 9. Clear Cart
    $('#qsClearCart').on('click', function () {
        if (!qsCart.length) return;
        Swal.fire({
            title: 'هل أنت متأكد؟',
            text: "سيتم تفريغ السلة!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'نعم، قم بالحذف!',
            cancelButtonText: 'إلغاء'
        }).then((result) => {
            if (result.isConfirmed) {
                qsCart = [];
                renderCart();
                updateTotals();
            }
        });
    });

    // 10. Submit Edit Form
    $('#editSaleFormPage').on('submit', function (e) {
        e.preventDefault();
        const btn = $('#qsSaveEditBtn');
        const form = $(this);
        const actionUrl = form.attr('action');

        // Validation
        const customerVal = $('#modalCustomerSelect').val();
        const paymentType = $('#qsPaymentType').val() || '1'; // 1=Cash, 2=Partial, 3=Credit

        // Customer Check
        if (paymentType !== '1') {
            if (!customerVal || (customerVal === '0' && (!$('#modalNewCustomerName').val() && !$('#modalNewCustomerPhone').val()))) {
                Swal.fire({ icon: 'error', title: 'خطأ', text: 'يجب اختيار عميل للبيع الآجل أو الجزئي' });
                return;
            }
        }

        if (qsCart.length === 0) {
            Swal.fire({ icon: 'error', title: 'السلة', text: 'يرجى إضافة منتج' });
            return;
        }

        const total = parseFloat($('#qsTotalInput').val() || '0');
        const paidVal = parseFloat($('#qsPaidAmount').val() || '0');

        if (paymentType === '1') $('#qsPaidAmount').val(total.toFixed(2));
        else if (paymentType === '3') $('#qsPaidAmount').val('0');
        else if (paymentType === '2' && (paidVal <= 0 || paidVal > total)) {
            Swal.fire({ icon: 'error', title: 'خطأ', text: 'المبلغ المدفوع غير صحيح' });
            return;
        }

        // Prepare Data
        btn.prop('disabled', true);
        Swal.fire({ title: 'جاري الحفظ...', didOpen: () => Swal.showLoading() });

        // Clean old hidden inputs
        form.find('input[name^="SaleItems"]').remove();

        // Add current cart items
        qsCart.forEach((p, i) => {
            form.append(`<input type="hidden" name="SaleItems[${i}].ProductId" value="${p.productId}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].Quantity" value="${p.quantity}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].UnitPrice" value="${p.customSalePrice}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].CustomSalePrice" value="${p.customSalePrice}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].PurchasePrice" value="${p.purchasePrice}">`);
        });

        // Also map Customer data if using the custom select
        // Ensure inputs are named correctly for binding

        $.ajax({
            url: actionUrl,
            type: 'POST',
            data: form.serialize(),
            success: function (res) {
                if (res.success) {
                    Swal.fire({ icon: 'success', title: 'تم التعديل بنجاح', timer: 1500, showConfirmButton: false })
                        .then(() => {
                            window.location.href = '/Sales/Index';
                        });
                } else {
                    Swal.fire({ icon: 'error', title: 'خطأ', text: res.message });
                    btn.prop('disabled', false);
                }
            },
            error: function () {
                Swal.fire({ icon: 'error', title: 'خطأ', text: 'فشل الاتصال بالخادم' });
                btn.prop('disabled', false);
            }
        });
    });

    // 11. Add to Cart via Click
    $(document).on('click', '.qs-product-card', function (e) {
        if ($(e.target).closest('button').length) return;
        const id = $(this).data('product-id');
        const p = getProductById(id);
        if (p) {
            qsAddToCart(p.id, p.name, p.price, p.purchasePrice, p.quantity, p.barcode);
        }
    });

    // Focus Barcode on any key press if not focused
    $(document).on('keydown', function (e) {
        if (!$(e.target).is('input, textarea')) {
            // If typing alphanumeric, focus barcode scanner
            if (e.key.length === 1 && !e.ctrlKey && !e.altKey) {
                $('#barcodeScanner').focus();
            }
        }
    });
});

// --- Helper Functions ---

function getProductById(id) { return qsAllProducts.find(p => p.id === id); }

function applyFilters() {
    const term = ($('#qsSearch').val() || '').trim().toLowerCase();
    let list = [...qsAllProducts];
    if (term) list = list.filter(p => (p.name || '').toLowerCase().includes(term));
    // Sort
    list.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
    return list;
}

function paginate(list) {
    const start = (qsPage - 1) * qsPageSize;
    return list.slice(start, start + qsPageSize);
}

function renderProductsGrid(products) {
    const grid = $('#qsProductsGrid');
    grid.empty();

    // Simple columns calc based on window width
    let columns = 2;
    const windowWidth = $(window).width();
    if (windowWidth >= 1280) columns = 4;
    else if (windowWidth >= 768) columns = 3;

    const itemsPerPage = columns * 3;

    // Update Page Size if needed
    if (qsPageSize !== itemsPerPage) {
        qsPageSize = itemsPerPage;
        const currentStart = (qsPage - 1) * qsPageSize;
        // Note: Assuming logic to handle slice is external or we trust simple logic here
    }

    if (!products || !products.length) {
        $('#qsNoResults').removeClass('hidden').addClass('flex');
        return;
    }
    $('#qsNoResults').addClass('hidden').removeClass('flex');

    products.forEach(p => {
        const hasStock = (p.quantity || 0) > 0;

        const stockBadge = hasStock
            ? `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-bold bg-indigo-50 text-indigo-900">${p.quantity}</span>`
            : `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-medium bg-red-100 text-red-700">0</span>`;

        const bgClass = hasStock ? 'bg-white border-gray-200' : 'bg-red-100 border-red-300';
        const stateClasses = hasStock
            ? 'opacity-100 hover:-translate-y-1 hover:shadow-md cursor-pointer'
            : 'cursor-not-allowed';

        const card = `
        <div class="qs-product-card group relative ${bgClass} border rounded-lg shadow-sm p-3 flex flex-col transition-all duration-200 ${stateClasses}" 
             data-product-id="${p.id}">
             
            ${hasStock ? `
            <div class="qs-hover-overlay">
                <button type="button" onclick="qsAddToCart(${p.id}, '${p.name.replace(/'/g, "\\'")}', ${p.price}, ${p.purchasePrice}, ${p.quantity}, '${p.barcode || ''}')" 
                    class="qs-hover-btn">
                    <i class="fas fa-plus fa-2x"></i>
                </button>
            </div>
            ` : ''}

             <div class="flex justify-between items-center mb-2 gap-2">
                <h4 class="text-sm font-bold text-gray-800 line-clamp-2 leading-tight h-10 w-full" title="${p.name}">${p.name}</h4>
                <div class="js-stock-badge shrink-0">
                    ${stockBadge}
                </div>
            </div>
            
            <div class="qs-desc mb-2 text-sm text-bold text-gray-500 line-clamp-2" title="${p.description || ''}">${p.description || ''}</div>

            <div class="mt-auto flex flex-col items-start border-t border-gray-50 pt-2">
                <span class="text-lg font-bold text-indigo-800 mb-1">${(p.price || 0).toFixed(2)}</span>
                
                ${p.barcode ? `
                <div class="flex items-center text-gray-500 bg-gray-50 px-1.5 py-0.5 rounded w-full">
                    <i class="fas fa-barcode text-[15px] ml-1"></i>
                    <span class="text-[.8rem] font-bold font-mono tracking-wider truncate">${p.barcode}</span>
                </div>
                ` : '<span class="h-6 block"></span>'}
            </div>
        </div>`;
        grid.append(card);
    });
}

function qsAddToCart(productId, productName, price, purchasePrice, availableStock, barcode) {
    const existing = qsCart.find(i => i.productId === productId);
    if (existing) {
        existing.quantity += 1;
    } else {
        qsCart.push({ productId, productName, price, purchasePrice, customSalePrice: price, quantity: 1, availableStock, barcode });
    }
    renderCart();
    updateTotals();

    // Play sound
    qsAddSound.currentTime = 0;
    qsAddSound.play().catch(e => console.log('Audio play failed:', e));
    showToast(`تم إضافة: ${productName}`, 'success');
}

// Helper to set customer name on load
$(document).ready(function () {
    const selectedOption = $('#modalCustomerSelect option:selected');
    if (selectedOption.val()) {
        $('#customerSearch').val(selectedOption.text().trim());
    }
});

function renderCart() {
    const tbody = $('#qsCartTable tbody');
    tbody.empty();
    qsCart.forEach(item => {
        const row = `
        <tr class="hover:bg-gray-50 transition-colors">
            <td class="px-2 text-sm text-gray-900">${item.productName}</td>
            <td class="px-2 py-2 text-center">
                <input type="number" min="1" value="${item.quantity}" onchange="qsUpdateQty(${item.productId}, this.value)" class="w-12 text-center border border-gray-300 rounded text-sm py-1">
            </td>
            <td class="px-2 py-2 text-center">
                <input type="number" step="1" min="0" value="${(item.customSalePrice || 0).toFixed(2)}" onchange="qsUpdatePrice(${item.productId}, this.value)" class="w-20 text-center border border-gray-300 rounded text-sm py-1">
            </td>
            <td class="px-2 py-2 text-center font-bold">
                ${(item.quantity * item.customSalePrice).toFixed(2)}
            </td>
            <td class="px-2 py-2 text-right">
                <button type="button" onclick="qsRemove(${item.productId})" class="text-red-500 hover:text-red-700">
                    <i class="fas fa-trash-alt"></i>
                </button>
            </td>
        </tr>`;
        tbody.append(row);
    });
}

function qsUpdateQty(productId, qty) {
    const item = qsCart.find(i => i.productId === productId);
    if (!item) return;
    item.quantity = Math.max(1, parseInt(qty) || 1);
    renderCart();
    updateTotals();
}

function qsUpdatePrice(productId, price) {
    const item = qsCart.find(i => i.productId === productId);
    if (!item) return;
    item.customSalePrice = parseFloat(price) || 0;
    renderCart();
    updateTotals();
}

function qsRemove(productId) {
    qsCart = qsCart.filter(i => i.productId !== productId);
    renderCart();
    updateTotals();
}

function updateTotals() {
    const subtotal = qsCart.reduce((s, i) => s + (i.customSalePrice * i.quantity), 0);
    const discount = parseFloat($('#qsDiscountAmount').val() || '0');
    const total = Math.max(0, subtotal - discount);
    $('#qsTotal').text(total.toFixed(2));
    $('#qsTotalInput').val(total.toFixed(2));
    updatePaymentVisibility();
}

function updatePaymentVisibility() {
    const type = $('#qsPaymentType').val() || '1';
    const total = parseFloat($('#qsTotalInput').val() || '0');
    if (type === '2') { // Partial
        $('#qsPaidWrapper').removeClass('hidden');
        $('#qsPaymentMethodWrapper').removeClass('hidden');
    } else if (type === '1') { // Cash
        $('#qsPaidWrapper').addClass('hidden');
        $('#qsPaymentMethodWrapper').removeClass('hidden');
        if (total > 0) $('#qsPaidAmount').val(total.toFixed(2));
    } else { // Credit
        $('#qsPaidWrapper').addClass('hidden');
        $('#qsPaymentMethodWrapper').addClass('hidden');
        $('#qsPaidAmount').val('0');
    }
}
