// ===== دوال البحث عن العملاء للأقساط =====

// البحث عن العملاء في الأقساط
$(document).on('input', '#installmentCustomerSearch', function () {
    const searchTerm = $(this).val().toLowerCase().trim();
    const resultsDiv = $('#installmentCustomerSearchResults');
    const select = $('#modalCustomerSelect');

    if (searchTerm.length === 0) {
        resultsDiv.addClass('hidden').hide();
        return;
    }

    const matchingOptions = select.find('option').filter(function () {
        const optionText = $(this).text().toLowerCase();
        const optionValue = $(this).val();
        const optionPhone = $(this).data('phone');

        // تحويل رقم الهاتف إلى نص للبحث الآمن
        const phoneStr = (optionPhone !== null && optionPhone !== undefined) ? String(optionPhone) : '';

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
            const phone = $(this).data('phone');
            const isNewCustomer = value === '0';
            const itemClass = isNewCustomer ? 'text-primary fw-bold' : '';

            // عرض الاسم ورقم الهاتف إذا كان متوفر
            const displayText = phone ? `${text} (${phone})` : text;

            if (value !== '0' && value !== 0) {
                resultsHtml += `<div class="installment-customer-result p-2 border-bottom ${itemClass}" data-value="${value}" style="cursor: pointer;">${displayText}</div>`;
            }
        });

        // دائماً إظهار خيار إضافة عميل جديد في نهاية البحث
        const isNumber = /^\d+$/.test(searchTerm);
        const newCustomerText = isNumber ? `➕ عميل جديد (${searchTerm})` : '➕ عميل جديد';
        resultsHtml += `<div class="installment-customer-result p-2 border-bottom text-primary fw-bold" data-value="0" style="cursor: pointer;">${newCustomerText}</div>`;

        resultsDiv.html(resultsHtml).removeClass('hidden').show();
    } else {
        // إظهار خيار "عميل جديد" عندما لا توجد نتائج
        const isNumber = /^\d+$/.test(searchTerm);
        const newCustomerText = isNumber ? `➕ عميل جديد (${searchTerm})` : '➕ عميل جديد';
        resultsDiv.html(`<div class="installment-customer-result p-2 border-bottom text-primary fw-bold" data-value="0" style="cursor: pointer;">${newCustomerText}</div>`).removeClass('hidden').show();
    }
});

// معالجة اختيار نتيجة البحث عن العميل
$(document).on('click', '.installment-customer-result', function () {
    const value = $(this).data('value');

    // التعديل هنا: نأخذ النص الأصلي ثم ننظفه من رقم الهاتف
    let text = $(this).text();

    // إذا كان النص يحتوي على قوس مفتوح (وهو ليس عميل جديد)، نفصل الاسم عن الرقم
    if (text.includes('(') && value != '0') {
        text = text.split('(')[0].trim();
    }
    // إذا كان عميل جديد، نكتب كلمة "عميل جديد" فقط لتجنب ظهور "عميل جديد (الرقم)"
    else if (value == '0') {
        text = "عميل جديد";
    }

    const searchTerm = $('#installmentCustomerSearch').val().trim();

    // حفظ النص الأصلي للبحث قبل تغييره (للاستخدام في حالة العميل الجديد برقم هاتف)
    const originalSearchTerm = searchTerm;

    $('#modalCustomerSelect').val(value);
    $('#installmentCustomerSearch').val(text); // الآن سيتم وضع الاسم النظيف فقط
    $('#installmentCustomerSearchResults').addClass('hidden').hide();

    // --- بقية الكود كما هو تماماً ---

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
            phoneField.val(originalSearchTerm);

            // محاولات تأكيد القيمة
            setTimeout(function () {
                if (phoneField.val() !== originalSearchTerm) {
                    phoneField.val(originalSearchTerm);
                }
            }, 50);
        }
    } else if (value === '0' || value === 0) {
        // إذا كان عميل جديد ولكن ليس برقم، أظهر الحقول فقط
        $('#modalNewCustomerFields').removeClass('hidden').show();
        $('#modalNewCustomerName').prop('required', true);
        $('#modalNewCustomerPhone').prop('required', true);
        $('#modalNewCustomerAddress').prop('required', true);
    } else {
        // هام: إذا تم اختيار عميل موجود، قم بإخفاء حقول العميل الجديد
        $('#modalNewCustomerFields').addClass('hidden').hide();
        $('#modalNewCustomerName').prop('required', false);
        $('#modalNewCustomerPhone').prop('required', false);
        $('#modalNewCustomerAddress').prop('required', false);
    }

    // Trigger change event for customer selection logic
    $('#modalCustomerSelect').trigger('change');
});

// إخفاء نتائج البحث عن العملاء عند النقر خارجها
$(document).on('click', function (e) {
    if (!$(e.target).closest('#installmentCustomerSearch, #installmentCustomerSearchResults').length) {
        $('#installmentCustomerSearchResults').addClass('hidden').hide();
    }
});

// معالجة التركيز على البحث عن العملاء
$(document).on('focus', '#installmentCustomerSearch', function () {
    if ($(this).val().trim().length > 0) {
        $(this).trigger('input');
    }
});