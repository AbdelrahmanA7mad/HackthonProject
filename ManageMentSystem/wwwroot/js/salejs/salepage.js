// quicksale.js - Tailwind CSS Edition (Red & 3 Rows Updated)
let qsAllProducts = [];
let qsCart = [];
let qsSearchTimeout;
let qsPage = 1;
let qsPageSize = 16;
let qsFiltered = [];
let qsCategories = [];

$(document).ready(function () {

    // --- إضافة ستايل خاص للزر لضمان عمله مع الجافاسكريبت ---
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

    // 1. Initial Load
    $.get('/Sales/GetProducts', function (data) {
        // قراءة البيانات وتوحيد الأسماء (Fix mapping)
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

        // Extract categories
        const catMap = {};
        qsAllProducts.forEach(p => {
            const id = p.categoryId || 0;
            const name = (p.categoryName || '').trim();
            if (id && name && !catMap[id]) catMap[id] = name;
        });
        qsCategories = Object.entries(catMap).map(([id, name]) => ({ id: parseInt(id), name }));
        renderCategoryOptions();

        qsFiltered = applyFilters();
        qsPage = 1;
        renderProductsGrid(paginate(qsFiltered));
        updatePager();
        updateCount();
    });

    // 2. Search & Filter Listeners
    $('#qsSearch').on('input', function () {
        clearTimeout(qsSearchTimeout);
        const term = ($(this).val() || '').trim().toLowerCase();
        qsSearchTimeout = setTimeout(function () {
            qsFiltered = applyFilters();
            qsPage = 1;
            renderProductsGrid(paginate(qsFiltered));
            updatePager();
            updateCount();
        }, 200);
    });

    $('#qsCategory').on('change', function () {
        qsFiltered = applyFilters();
        qsPage = 1;
        renderProductsGrid(paginate(qsFiltered));
        updatePager();
        updateCount();
    });

    // 3. Pager
    $('#qsPrev').on('click', function () {
        if (qsPage > 1) { qsPage--; renderProductsGrid(paginate(qsFiltered)); updatePager(); }
    });
    $('#qsNext').on('click', function () {
        const total = qsFiltered.length;
        const maxPage = Math.max(1, Math.ceil(total / qsPageSize));
        if (qsPage < maxPage) { qsPage++; renderProductsGrid(paginate(qsFiltered)); updatePager(); }
    });

    // 4. Customer Search & Validation
    if (typeof initializePhoneValidation === 'function') {
        initializePhoneValidation('#modalNewCustomerPhone', '#quickSaleForm', '#modalCustomerSelect', '#modalNewCustomerFields', true);
    }

    // 5. Payment Visibility
    $('#qsPaymentType').on('change', updatePaymentVisibility);

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
    $('#qsBarcode').on('input', function () {
        const code = ($(this).val() || '').trim();
        if (code.length === 0) return;
        const product = qsAllProducts.find(p => (p.barcode || '').toLowerCase() === code.toLowerCase());
        if (product) {
            qsAddToCart(product.id, product.name || '', product.price || 0, product.purchasePrice || 0, product.quantity || 0, product.barcode || '');
            $(this).val('');
        } else {
            const alt = qsAllProducts.find(p => (p.barcode || '').toLowerCase().includes(code.toLowerCase()));
            if (alt) {
                qsAddToCart(alt.id, alt.name || '', alt.price || 0, alt.purchasePrice || 0, alt.quantity || 0, alt.barcode || '');
                $(this).val('');
            }
        }
    });
    $('#qsBarcodeClear').on('click', function () { $('#qsBarcode').val('').focus(); });

    // 9. Clear Cart
    $('#qsClearCart').on('click', function () {
        if (!qsCart.length) return;
        Swal.fire({
            title: 'تفريغ السلة',
            text: 'هل أنت متأكد؟',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'نعم، تفريغ',
            cancelButtonText: 'إلغاء'
        }).then((result) => {
            if (result.isConfirmed) {
                qsCart.forEach(item => {
                    const prod = getProductById(item.productId);
                    if (prod) prod.quantity = (prod.quantity || 0) + item.quantity;
                    updateProductCardStock(item.productId);
                });
                qsCart = [];
                renderCart();
                updateTotals();
            }
        });
    });

    // 10. Submit Form (Save Only)
    $('#quickSaleForm').on('submit', function (e) {
        e.preventDefault();
        const btn = $('#qsSaveBtn');

        const customerVal = $('#modalCustomerSelect').val();
        const paymentType = $('#qsPaymentType').val() || '1';

        // العميل مطلوب فقط للبيع الآجل (3) أو الجزئي (2)
        if (paymentType !== '1') {
            if (!customerVal) {
                showErrorMessage('العميل', 'يجب اختيار عميل للبيع الآجل أو الجزئي');
                return;
            }
            if (customerVal === '0' && (!$('#modalNewCustomerName').val().trim() || !$('#modalNewCustomerPhone').val().trim())) {
                showErrorMessage('العميل', 'يرجى إدخال بيانات العميل الجديد');
                return;
            }
        }
        if (qsCart.length === 0) {
            showErrorMessage('السلة', 'يرجى إضافة منتج');
            return;
        }


        const total = parseFloat($('#qsTotalInput').val() || '0');
        const paidVal = parseFloat($('#qsPaidAmount').val() || '0');

        if (paymentType === '1') $('#qsPaidAmount').val(total.toFixed(2));
        else if (paymentType === '3') $('#qsPaidAmount').val('0');
        else if (paymentType === '2' && (paidVal <= 0 || paidVal > total)) {
            showErrorMessage('خطأ', 'المبلغ المدفوع غير صحيح');
            return;
        }

        btn.prop('disabled', true);
        Swal.fire({
            title: 'جاري حفظ البيع...',
            html: 'يرجى الانتظار',
            allowOutsideClick: false,
            didOpen: () => { Swal.showLoading(); }
        });

        $(this).find('input[name^="SaleItems"]').remove();
        qsCart.forEach((p, i) => {
            $(this).append(`<input type="hidden" name="SaleItems[${i}].ProductId" value="${p.productId}">`);
            $(this).append(`<input type="hidden" name="SaleItems[${i}].Quantity" value="${p.quantity}">`);
            $(this).append(`<input type="hidden" name="SaleItems[${i}].UnitPrice" value="${p.customSalePrice}">`);
            $(this).append(`<input type="hidden" name="SaleItems[${i}].CustomSalePrice" value="${p.customSalePrice}">`);
            $(this).append(`<input type="hidden" name="SaleItems[${i}].PurchasePrice" value="${p.purchasePrice}">`);
        });

        $.ajax({
            url: '/Sales/Create',
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    Swal.fire({ icon: 'success', title: 'تم الحفظ', timer: 1500, showConfirmButton: false });
                    resetScreen();
                } else {
                    showErrorMessage('خطأ', res.message);
                }
                btn.prop('disabled', false);
            },
            error: function () {
                showErrorMessage('خطأ', 'فشل الاتصال بالخادم');
                btn.prop('disabled', false);
            }
        });
    });

    // 11. Add to Cart via Click (Delegation)
    $(document).on('click', '.qs-product-card', function (e) {
        if ($(e.target).closest('button').length) return;
        const id = $(this).data('product-id');
        const p = getProductById(id);
        if (p && (p.quantity || 0) > 0) {
            qsAddToCart(p.id, p.name, p.price, p.purchasePrice, p.quantity, p.barcode);
        }
    });

    // 11. Save and Print Handler
    $('#saveAndPrintBtn').on('click', function (e) {
        e.preventDefault();
        const btn = $(this);

        const customerVal = $('#modalCustomerSelect').val();
        const paymentType = $('#qsPaymentType').val() || '1';

        // العميل مطلوب فقط للبيع الآجل (3) أو الجزئي (2)
        if (paymentType !== '1') {
            if (!customerVal) {
                showErrorMessage('العميل', 'يجب اختيار عميل للبيع الآجل أو الجزئي');
                return;
            }
            if (customerVal === '0' && (!$('#modalNewCustomerName').val().trim() || !$('#modalNewCustomerPhone').val().trim())) {
                showErrorMessage('العميل', 'يرجى إدخال بيانات العميل الجديد');
                return;
            }
        }
        if (qsCart.length === 0) {
            showErrorMessage('السلة', 'يرجى إضافة منتج');
            return;
        }


        const total = parseFloat($('#qsTotalInput').val() || '0');
        const paidVal = parseFloat($('#qsPaidAmount').val() || '0');

        if (paymentType === '1') $('#qsPaidAmount').val(total.toFixed(2));
        else if (paymentType === '3') $('#qsPaidAmount').val('0');
        else if (paymentType === '2' && (paidVal <= 0 || paidVal > total)) {
            showErrorMessage('خطأ', 'المبلغ المدفوع غير صحيح');
            return;
        }

        btn.prop('disabled', true);
        Swal.fire({
            title: 'جاري الحفظ والطباعة...',
            html: 'يرجى الانتظار',
            allowOutsideClick: false,
            didOpen: () => { Swal.showLoading(); }
        });

        const form = $('#quickSaleForm');
        form.find('input[name^="SaleItems"]').remove();
        qsCart.forEach((p, i) => {
            form.append(`<input type="hidden" name="SaleItems[${i}].ProductId" value="${p.productId}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].Quantity" value="${p.quantity}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].UnitPrice" value="${p.customSalePrice}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].CustomSalePrice" value="${p.customSalePrice}">`);
            form.append(`<input type="hidden" name="SaleItems[${i}].PurchasePrice" value="${p.purchasePrice}">`);
        });

        $.ajax({
            url: '/Sales/Create',
            type: 'POST',
            data: form.serialize(),
            success: function (res) {
                if (res.success) {
                    if (res.saleId) {
                        const url = '/Sales/ReceiptInvoice/' + res.saleId;
                        window.open(url, '_blank');
                    }
                    Swal.fire({ icon: 'success', title: 'تم الحفظ والطباعة', timer: 1500, showConfirmButton: false });
                    resetScreen();
                } else {
                    showErrorMessage('خطأ', res.message);
                }
                btn.prop('disabled', false);
            },
            error: function () {
                showErrorMessage('خطأ', 'فشل الاتصال بالخادم');
                btn.prop('disabled', false);
            }
        });
    });

    // Fullscreen Toggle Logic
    $('#qsFullscreenToggle').on('click', function () {
        $('body').toggleClass('qs-fullscreen-mode');
        const isFullscreen = $('body').hasClass('qs-fullscreen-mode');
        const icon = $(this).find('i');

        if (isFullscreen) {
            icon.removeClass('fa-expand').addClass('fa-compress');
            $(this).attr('title', 'إنهاء ملء الشاشة')
                .removeClass('text-indigo-800 border-indigo-500')
                .addClass('text-red-700 border-red-500 hover:bg-red-50');
            if (document.documentElement.requestFullscreen) {
                document.documentElement.requestFullscreen().catch(e => console.log(e));
            }
        } else {
            icon.removeClass('fa-compress').addClass('fa-expand');
            $(this).attr('title', 'ملء الشاشة')
                .removeClass('text-red-700 border-red-500 hover:bg-red-50')
                .addClass('text-indigo-800 border-indigo-500 hover:bg-indigo-50');
            if (document.exitFullscreen && document.fullscreenElement) {
                document.exitFullscreen();
            }
        }
        setTimeout(() => $(window).trigger('resize'), 300);
    });
});

// --- Helper Functions ---

function getProductById(id) { return qsAllProducts.find(p => p.id === id); }

function applyFilters() {
    const term = ($('#qsSearch').val() || '').trim().toLowerCase();
    const cat = ($('#qsCategory').val() || '').trim();
    let list = [...qsAllProducts];
    if (cat) list = list.filter(p => (p.categoryId || 0) == cat);
    if (term) list = list.filter(p => (p.name || '').toLowerCase().includes(term) || (p.barcode || '').toLowerCase().includes(term));
    list.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
    return list;
}

function renderCategoryOptions() {
    const sel = $('#qsCategory');
    const current = sel.val();
    sel.find('option:not([value=""])').remove();
    qsCategories.sort((a, b) => a.name.localeCompare(b.name));
    qsCategories.forEach(c => sel.append(`<option value="${c.id}">${c.name}</option>`));
    if (current) sel.val(current);
}

function paginate(list) {
    const start = (qsPage - 1) * qsPageSize;
    return list.slice(start, start + qsPageSize);
}

function updatePager() {
    const total = qsFiltered.length;
    if (total === 0) { $('#qsPager').addClass('hidden'); return; }
    $('#qsPager').removeClass('hidden');
    const start = (qsPage - 1) * qsPageSize + 1;
    const end = Math.min(qsPage * qsPageSize, total);
    $('#qsRange').text(`${start}-${end} من ${total}`);
}

function updateCount() { $('#qsCount').text(qsFiltered.length); }

function renderProductsGrid(products) {
    const grid = $('#qsProductsGrid');
    grid.empty();

    // 1. حساب الأعمدة
    let columns = 2;
    const windowWidth = $(window).width();
    if (windowWidth >= 1280) columns = 4;
    else if (windowWidth >= 768) columns = 3;

    // --- التعديل هنا: 3 صفوف بدلاً من 2 ---
    const itemsPerPage = columns * 3; // ضربنا في 3 لتظهر 3 صفوف
    // ------------------------------------

    if (qsPageSize !== itemsPerPage) {
        qsPageSize = itemsPerPage;
        const currentStart = (qsPage - 1) * qsPageSize;
        products = qsFiltered.slice(currentStart, currentStart + qsPageSize);
        updatePager();
    }

    if (!products || !products.length) {
        $('#qsNoResults').removeClass('hidden').addClass('flex');
        return;
    }
    $('#qsNoResults').addClass('hidden').removeClass('flex');

    // 2. رسم الكروت
    products.forEach(p => {
        const hasStock = (p.quantity || 0) > 0;

        const stockBadge = hasStock
            ? `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-bold bg-indigo-50 text-indigo-900">${p.quantity}</span>`
            : `<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-medium bg-red-100 text-red-700">0</span>`;

        // --- تعديل الستايل للأحمر ---
        // إذا كان هناك مخزون: خلفية بيضاء. إذا نفذ: خلفية حمراء باهتة وحدود حمراء
        const bgClass = hasStock ? 'bg-white border-gray-200' : 'bg-red-100 border-red-300';

        const stateClasses = hasStock
            ? 'opacity-100 hover:-translate-y-1 hover:shadow-md cursor-pointer'
            : 'cursor-not-allowed';
        // -------------------------

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

function updateProductCardStock(productId) {
    const prod = getProductById(productId);
    const card = $(`.qs-product-card[data-product-id="${productId}"]`);
    if (!card.length || !prod) return;

    const badgeContainer = card.find('.js-stock-badge');
    if (prod.quantity > 0) {
        badgeContainer.html(`<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-bold bg-indigo-50 text-indigo-900">${prod.quantity}</span>`);

        // --- تحديث الستايل عند توفر المنتج ---
        card.removeClass('bg-red-100 border-red-300 cursor-not-allowed grayscale opacity-75')
            .addClass('bg-white border-gray-200 opacity-100 hover:-translate-y-1 hover:shadow-md cursor-pointer');

        // Re-add hover overlay
        if (card.find('.qs-hover-overlay').length === 0) {
            card.prepend(`
            <div class="qs-hover-overlay">
                <button type="button" onclick="qsAddToCart(${prod.id}, '${prod.name.replace(/'/g, "\\'")}', ${prod.price}, ${prod.purchasePrice}, ${prod.quantity}, '${prod.barcode || ''}')" 
                    class="qs-hover-btn">
                    <i class="fas fa-plus fa-2x"></i>
                </button>
            </div>
           `);
        }

    } else {
        badgeContainer.html(`<span class="inline-flex items-center px-2 py-0.5 rounded-full text-md font-medium bg-red-50 text-red-700">0</span>`);

        // --- تحديث الستايل عند نفاذ المنتج ---
        card.removeClass('bg-white border-gray-200 opacity-100 hover:-translate-y-1 hover:shadow-md cursor-pointer')
            .addClass('bg-red-50 border-red-300 cursor-not-allowed'); // تحويل للخلفية الحمراء

        card.find('.qs-hover-overlay').remove();
    }
}

// تعريف صوت "الرنة" عند الإضافة - يمكنك تغيير الرابط بأي ملف mp3 لديك
const qsAddSound = new Audio('https://actions.google.com/sounds/v1/cartoon/wood_plank_flicks.ogg');
function qsAddToCart(productId, productName, price, purchasePrice, availableStock, barcode) {
    const prod = getProductById(productId);
    const remaining = prod ? (prod.quantity || 0) : 0;

    if (remaining <= 0) {
        showToast('نفذت الكمية لهذا المنتج', 'error');
        return;
    }

    qsAddSound.currentTime = 0; // لإعادة الصوت للبداية إذا ضغطت بسرعة خلف بعض
    qsAddSound.play();
    const existing = qsCart.find(i => i.productId === productId);
    if (existing) {
        existing.quantity += 1;
    } else {
        qsCart.push({ productId, productName, price, purchasePrice, customSalePrice: price, quantity: 1, availableStock, barcode });
    }

    if (prod) prod.quantity = remaining - 1;
    updateProductCardStock(productId);
    renderCart();
    updateTotals();
    showToast(`تم إضافة ${productName}`, 'success');
}

function renderCart() {
    const tbody = $('#qsCartTable tbody');
    tbody.empty();
    qsCart.forEach(item => {
        const row = `
        <tr class="hover:bg-gray-50 transition-colors">
            <td class="px-2 whitespace-nowrap text-sm font-medium text-gray-900 overflow-hidden text-ellipsis max-w-[120px]" title="${item.productName}">
                ${item.productName}
            </td>
            <td class="px-2 py-2 whitespace-nowrap text-center">
                <input type="number" min="1" value="${item.quantity}" onchange="qsUpdateQty(${item.productId}, this.value)" class="w-10 text-center border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 text-sm py-1 px-1 border">
            </td>
            <td class="px-2 py-2 whitespace-nowrap text-center">
                <input type="number" step="1" min="0" value="${(item.customSalePrice || 0).toFixed(2)}" onchange="qsUpdatePrice(${item.productId}, this.value)" class="w-20 text-center border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 text-sm py-1 px-1 border">
            </td>
            <td class="px-2 py-2 whitespace-nowrap text-center text-sm font-bold text-gray-700">
                ${(item.quantity * item.customSalePrice).toFixed(2)}
            </td>
            <td class="px-2 py-2 whitespace-nowrap text-right">
                <button type="button" onclick="qsRemove(${item.productId})" class="text-red-500 hover:text-red-700 transition-colors">
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
    const desired = Math.max(1, parseInt(qty) || 1);
    const prod = getProductById(productId);
    const remainingGrid = prod ? (prod.quantity || 0) : 0;

    // Current total held by this item + what's remaining in grid
    const maxAvailable = item.quantity + remainingGrid;

    if (desired > maxAvailable) {
        showToast('الكمية المطلوبة غير متوفرة', 'warning');
        item.quantity = maxAvailable;
        if (prod) prod.quantity = 0;
    } else {
        const diff = desired - item.quantity;
        item.quantity = desired;
        if (prod) prod.quantity -= diff;
    }
    renderCart();
    updateTotals();
    updateProductCardStock(productId);
}

function qsUpdatePrice(productId, price) {
    const item = qsCart.find(i => i.productId === productId);
    if (!item) return;
    item.customSalePrice = parseFloat(price) || 0;
    renderCart();
    updateTotals();
}

function qsRemove(productId) {
    const item = qsCart.find(i => i.productId === productId);
    if (item) {
        const prod = getProductById(productId);
        if (prod) prod.quantity += item.quantity;
    }
    qsCart = qsCart.filter(i => i.productId !== productId);
    renderCart();
    updateTotals();
    updateProductCardStock(productId);
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
    if (type === '2') {
        $('#qsPaidWrapper').removeClass('hidden');
        $('#qsPaymentMethodWrapper').removeClass('hidden');
    } else if (type === '1') {
        $('#qsPaidWrapper').addClass('hidden');
        $('#qsPaymentMethodWrapper').removeClass('hidden');
        if (total > 0) $('#qsPaidAmount').val(total.toFixed(2));
    } else {
        $('#qsPaidWrapper').addClass('hidden');
        $('#qsPaymentMethodWrapper').addClass('hidden');
        $('#qsPaidAmount').val('0');
    }
}

function resetScreen() {
    qsCart = [];
    renderCart();
    $('#qsDiscountPercentage').val('0');
    $('#qsDiscountAmount').val('0');
    updateTotals();
    $('#modalCustomerSelect').val('');
    $('#customerSearch').val('');
    $('#customerSearchResults').hide();
    $('#modalNewCustomerFields').hide();
    $('#modalNewCustomerName, #modalNewCustomerPhone, #modalNewCustomerAddress').val('');
}

function showErrorMessage(title, msg) {
    Swal.fire({ icon: 'error', title: title, text: msg });
}
function showToast(msg, type = 'info') {
    const Toast = Swal.mixin({
        toast: true, position: 'top-end', showConfirmButton: false, timer: 3000, timerProgressBar: true
    });
    Toast.fire({ icon: type, title: msg });
}

// Handle barcode scan success for QuickSale
function handleBarcodeScanSuccess(productName) {
    if (typeof showBarcodeScanSuccess === 'function') {
        showBarcodeScanSuccess(productName);
    } else {
        showToast(`تم العثور على: ${productName}`, 'success');
    }
}

// Handle barcode scan error for QuickSale
function handleBarcodeScanError(message) {
    if (typeof showBarcodeScanError === 'function') {
        showBarcodeScanError(message);
    } else {
        showToast(message, 'error');
    }
}