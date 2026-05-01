// Customer Management Functions for Sales

// Customer search functionality
$(document).on('input', '#customerSearch', function () {
    const searchTerm = $(this).val().toLowerCase().trim();
    const resultsDiv = $('#customerSearchResults');
    const select = $('#modalCustomerSelect');

    if (searchTerm.length === 0) {
        resultsDiv.addClass('hidden').hide();
        return;
    }

    const matchingOptions = select.find('option').filter(function () {
        const optionText = $(this).text().toLowerCase();
        const optionValue = $(this).val();
        // استخدام attr بدلاً من data لضمان قراءة القيمة بشكل صحيح
        const optionPhone = $(this).attr('data-phone');

        // تحويل رقم الهاتف إلى نص للبحث الآمن
        const phoneStr = (optionPhone !== null && optionPhone !== undefined && optionPhone !== '') ? String(optionPhone).toLowerCase() : '';

        // البحث في الاسم أو رقم الهاتف
        const matchesName = optionText.includes(searchTerm);
        const matchesPhone = phoneStr.includes(searchTerm);

        return (matchesName || matchesPhone) && optionValue !== '';
    });

    if (matchingOptions.length > 0) {
        let resultsHtml = '';
        matchingOptions.each(function () {
            const value = $(this).val();
            const text = $(this).text();
            // استخدام attr بدلاً من data لضمان قراءة القيمة بشكل صحيح
            const phone = $(this).attr('data-phone');
            const isNewCustomer = value === '0';
            const itemClass = isNewCustomer ? 'text-primary fw-bold' : '';

            // عرض الاسم ورقم الهاتف إذا كان متوفر
            const displayText = phone ? `${text} (${phone})` : text;

            if (value !== '0' && value !== 0) {
                resultsHtml += `<div class="customer-result p-2 border-bottom ${itemClass}" data-value="${value}" style="cursor: pointer;">${displayText}</div>`;
            }
        });

        // دائماً إظهار خيار إضافة عميل جديد في نهاية البحث
        const isNumber = /^\d+$/.test(searchTerm);
        const newCustomerText = isNumber ? `➕ عميل جديد (${searchTerm})` : '➕ عميل جديد';
        resultsHtml += `<div class="customer-result p-2 border-bottom text-primary fw-bold" data-value="0" style="cursor: pointer;">${newCustomerText}</div>`;

        resultsDiv.html(resultsHtml).removeClass('hidden').show();
    } else {
        // إظهار خيار "عميل جديد" عندما لا توجد نتائج
        const isNumber = /^\d+$/.test(searchTerm);
        const newCustomerText = isNumber ? `➕ عميل جديد (${searchTerm})` : '➕ عميل جديد';
        resultsDiv.html(`<div class="customer-result p-2 border-bottom text-primary fw-bold" data-value="0" style="cursor: pointer;">${newCustomerText}</div>`).removeClass('hidden').show();
    }
});

// Handle customer result selection
$(document).on('click', '.customer-result', function () {
    const value = $(this).data('value');
    const text = $(this).text();
    const searchTerm = $('#customerSearch').val().trim();

    // حفظ النص الأصلي للبحث قبل تغييره
    const originalSearchTerm = searchTerm;

    $('#modalCustomerSelect').val(value);
    $('#customerSearch').val(text);
    $('#customerSearchResults').addClass('hidden').hide();

    // إذا كان العميل الجديد وتم البحث برقم، ضع الرقم في حقل الهاتف
    if ((value === '0' || value === 0) && originalSearchTerm && /^\d+$/.test(originalSearchTerm)) {
        // إظهار حقول العميل الجديد أولاً
        $('#modalNewCustomerFields').removeClass('hidden').show();

        // إضافة required للحقول
        $('#modalNewCustomerName').prop('required', true);
        $('#modalNewCustomerPhone').prop('required', true);
        $('#modalNewCustomerAddress').prop('required', true);

        // التأكد من وجود حقل الهاتف وتعيين القيمة
        const phoneField = $('#modalNewCustomerPhone');

        if (phoneField.length > 0) {
            // تعيين القيمة مباشرة
            phoneField.val(originalSearchTerm);

            // طريقة بديلة لتعيين القيمة
            phoneField.attr('value', originalSearchTerm);
            phoneField.prop('value', originalSearchTerm);

            // طريقة باستخدام JavaScript النقي
            const phoneFieldElement = document.getElementById('modalNewCustomerPhone');
            if (phoneFieldElement) {
                phoneFieldElement.value = originalSearchTerm;
            }

            // تأكيد إضافي بعد تعيين القيمة
            setTimeout(function () {
                if (phoneField.val() !== originalSearchTerm) {
                    phoneField.val(originalSearchTerm);
                    phoneField.attr('value', originalSearchTerm);
                    phoneField.prop('value', originalSearchTerm);
                    if (phoneFieldElement) {
                        phoneFieldElement.value = originalSearchTerm;
                    }
                }
            }, 50);

            // تأكيد إضافي بعد إظهار الحقول
            setTimeout(function () {
                if (phoneField.val() !== originalSearchTerm) {
                    phoneField.val(originalSearchTerm);
                    phoneField.attr('value', originalSearchTerm);
                    phoneField.prop('value', originalSearchTerm);
                    if (phoneFieldElement) {
                        phoneFieldElement.value = originalSearchTerm;
                    }
                }
            }, 200);
        }
    } else if (value === '0' || value === 0) {
        // إذا كان عميل جديد ولكن ليس برقم، أظهر الحقول فقط
        $('#modalNewCustomerFields').removeClass('hidden').show();
        $('#modalNewCustomerName').prop('required', true);
        $('#modalNewCustomerPhone').prop('required', true);
        $('#modalNewCustomerAddress').prop('required', true);
    }

    // Trigger change event for customer selection logic
    $('#modalCustomerSelect').trigger('change');
});

// Handle customer select change
$(document).on('change', '#modalCustomerSelect', function () {
    const selectedValue = $(this).val();

    if (selectedValue === '0') {
        // إظهار حقول العميل الجديد
        $('#modalNewCustomerFields').removeClass('hidden').show();
        $('#modalNewCustomerName').prop('required', true);
        $('#modalNewCustomerPhone').prop('required', true);
        $('#modalNewCustomerAddress').prop('required', true);

        // إذا كان هناك رقم في حقل البحث، انقله إلى حقل الهاتف
        const searchTerm = $('#customerSearch').val().trim();
        if (searchTerm && /^\d+$/.test(searchTerm)) {
            $('#modalNewCustomerPhone').val(searchTerm);
        }
    } else if (selectedValue !== '') {
        // إخفاء حقول العميل الجديد
        $('#modalNewCustomerFields').addClass('hidden').hide();
        $('#modalNewCustomerName').prop('required', false);
        $('#modalNewCustomerPhone').prop('required', false);
        $('#modalNewCustomerAddress').prop('required', false);
    }
});

// Hide results when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('#customerSearch, #customerSearchResults').length) {
        $('#customerSearchResults').addClass('hidden').hide();
    }
});

// Handle customer search focus
$(document).on('focus', '#customerSearch', function () {
    if ($(this).val().trim().length > 0) {
        $(this).trigger('input');
    }
});

// Initialize phone validation for sales page
if (typeof initializePhoneValidation === 'function') {
    initializePhoneValidation('#modalNewCustomerPhone', '#addSaleModal', '#modalCustomerSelect', '#modalNewCustomerFields');
}

// Reset form when modal is opened
$('#addSaleModal').on('show.bs.modal', function () {
    $('#customerSearch').val('');
    $('#modalCustomerSelect').val('');
    $('#modalNewCustomerFields').addClass('hidden').hide();
    $('#modalNewCustomerName').val('').prop('required', false);
    $('#modalNewCustomerPhone').val('').prop('required', false);
    $('#modalNewCustomerAddress').val('').prop('required', false);
    $('#customerSearchResults').addClass('hidden').hide();
    $('#barcodeScanner').val('');
    $('#productSearch').val('');
    $('#productSearchResults').addClass('hidden').hide();
    selectedProducts = [];
    $('#selectedProductsTable tbody').empty();
    updateTotalAmount();
});

// Helper functions for customer management
function useExistingCustomerInSales(customerId, customerName) {
    useExistingCustomer('#addSaleModal', '#modalCustomerSelect', '#modalNewCustomerFields', customerId, customerName, '#addSaleForm');
}

function updateExistingCustomerInSales() {
    updateExistingCustomer('#addSaleModal', '#modalCustomerSelect', '#modalNewCustomerFields', '#addSaleForm');
}