// Forms and Submission Functions for Sales

// Add Sale Form Submission (for modal)
$('#addSaleForm').on('submit', function (e) {
    // Check if this is the NewSale page (not modal)
    if (window.location.pathname.includes('/NewSale')) {
        return; // Let newsale.js handle it
    }

    e.preventDefault();
    var saveBtn = $('#saveBtn');
    saveBtn.prop('disabled', true);
    saveBtn.text('جارٍ الحفظ...');

    // Check if there's a phone exists alert
    if (!checkPhoneAlertBeforeSubmit('#addSaleModal')) {
        saveBtn.prop('disabled', false);
        saveBtn.text('حفظ البيع');
        return false;
    }

    // Check if creating new customer
    var selectedCustomer = $('#modalCustomerSelect').val();
    if (selectedCustomer === '0') {
        var customerName = $('#modalNewCustomerName').val().trim();
        var customerPhone = $('#modalNewCustomerPhone').val().trim();
        var customerAddress = $('#modalNewCustomerAddress').val().trim();

        if (!customerName || !customerPhone) {
            showCustomerValidationError('يرجى إدخال اسم العميل ورقم الهاتف');
            saveBtn.prop('disabled', false);
            saveBtn.text('حفظ البيع');
            return false;
        }
    }

    // Handle payment type / paid amount mapping
    var paymentType = $('#salePaymentType').val() || '1';
    var totalAfterDiscount = parseFloat($('#totalAmountInput').val() || '0');
    var paidAmountInput = $('#salePaidAmount');
    if (paymentType === '1') { // Cash
        paidAmountInput.val(totalAfterDiscount.toFixed(2));
    } else if (paymentType === '3') { // Credit
        paidAmountInput.val('0');
    } else if (paymentType === '2') { // Partial
        var val = parseFloat(paidAmountInput.val() || '0');
        if (val <= 0 || val > totalAfterDiscount) {
            showPaymentValidationError('قيمة المبلغ المدفوع الجزئي يجب أن تكون أكبر من صفر وأقل من أو تساوي الإجمالي.');
            saveBtn.prop('disabled', false);
            saveBtn.text('حفظ البيع');
            return false;
        }
    }

    // Ensure PaymentType is present in form
    $(this).find('input[name="PaymentType"]').remove();
    $(this).append('<input type="hidden" name="PaymentType" value="' + paymentType + '">');

    // Clear any existing hidden inputs for SaleItems
    $(this).find('input[name^="SaleItems"]').remove();

    // Add selected products to form data
    selectedProducts.forEach((product, index) => {
        $(this).append(`<input type="hidden" name="SaleItems[${index}].ProductId" value="${product.productId}">`);
        $(this).append(`<input type="hidden" name="SaleItems[${index}].Quantity" value="${product.quantity}">`);
        $(this).append(`<input type="hidden" name="SaleItems[${index}].UnitPrice" value="${product.customSalePrice}">`);
        $(this).append(`<input type="hidden" name="SaleItems[${index}].CustomSalePrice" value="${product.customSalePrice}">`);
        $(this).append(`<input type="hidden" name="SaleItems[${index}].PurchasePrice" value="${product.purchasePrice}">`);
    });

    $.ajax({
        url: '/Sales/Create',
        type: 'POST',
        data: $(this).serialize(),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (result) {
            if (result.success) {
                $('#addSaleModal').modal('hide');
                $('#addSaleForm')[0].reset();
                selectedProducts = [];
                $('#selectedProductsTable tbody').empty();
                updateTotalAmount();
                $('#barcodeScanner').val('');
                $('#productSearch').val('');
                $('#productSearchResults').hide();

                // Show success message
                showSaleSavedSuccess(
                    result.saleId || 'غير محدد',
                    result.customerName || 'غير محدد',
                    result.totalAmount || '0'
                );

                // Reload after delay
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                showErrorMessage('خطأ في إضافة البيع', result.message || 'حدث خطأ أثناء إضافة البيع');
                saveBtn.prop('disabled', false);
                saveBtn.text('حفظ البيع');
            }
        },
        error: function () {
            showErrorMessage('خطأ في إضافة البيع', 'حدث خطأ أثناء إضافة البيع');
            saveBtn.prop('disabled', false);
            saveBtn.text('حفظ البيع');
        }
    });
});

// Edit Sale Form Submission
$('#editSaleForm').on('submit', function (e) {
    e.preventDefault();
    var form = $(this); // حفظ مرجع النموذج
    var id = $('#editSaleId').val();

    console.log('Edit form submitted for sale ID:', id);
    console.log('Edit selected products:', editSelectedProducts);

    // Handle payment type / paid amount mapping (Edit)
    var editPaymentType = $('#editPaymentType').val() || '1';
    var editTotalAfterDiscount = parseFloat($('#editSaleTotalAmount').val() || '0');
    var editPaidAmount = $('#editPaidAmount');
    if (editPaymentType === '1') { // Cash
        editPaidAmount.val(editTotalAfterDiscount.toFixed(2));
    } else if (editPaymentType === '3') { // Credit
        editPaidAmount.val('0');
    } else if (editPaymentType === '2') { // Partial
        var eVal = parseFloat(editPaidAmount.val() || '0');
        if (eVal <= 0 || eVal > editTotalAfterDiscount) {
            showPaymentValidationError('قيمة المبلغ المدفوع الجزئي يجب أن تكون أكبر من صفر وأقل من أو تساوي الإجمالي.');
            return false;
        }
    }
    // Ensure PaymentType is in form data for edit
    form.find('input[name="PaymentType"]').remove();
    $('<input>').attr({ type: 'hidden', name: 'PaymentType', value: editPaymentType }).appendTo(form);

    // Check if creating new customer
    var selectedCustomer = $('#editSaleCustomerId').val();
    if (selectedCustomer === '0') {
        var customerName = $('#editModalNewCustomerName').val().trim();
        var customerPhone = $('#editModalNewCustomerPhone').val().trim();

        if (!customerName || !customerPhone) {
            showCustomerValidationError('يرجى إدخال اسم العميل ورقم الهاتف');
            return false;
        }
    }

    // التحقق من وجود منتجات مختارة
    if (editSelectedProducts.length === 0) {
        showProductValidationError('يرجى إضافة منتج واحد على الأقل!');
        return false;
    }

    // إضافة بيانات المنتجات المختارة للنموذج
    editSelectedProducts.forEach((product, index) => {
        console.log('Adding product to form:', product);

        const prefix = `SaleItems[${index}].`;
        $('<input>').attr({
            type: 'hidden',
            name: prefix + 'ProductId',
            value: product.productId
        }).appendTo(form);

        $('<input>').attr({
            type: 'hidden',
            name: prefix + 'Quantity',
            value: product.quantity
        }).appendTo(form);

        $('<input>').attr({
            type: 'hidden',
            name: prefix + 'UnitPrice',
            value: product.customSalePrice
        }).appendTo(form);

        $('<input>').attr({
            type: 'hidden',
            name: prefix + 'CustomSalePrice',
            value: product.customSalePrice
        }).appendTo(form);

        $('<input>').attr({
            type: 'hidden',
            name: prefix + 'PurchasePrice',
            value: product.purchasePrice
        }).appendTo(form);
    });

    console.log('Form data before submission:', form.serialize());

    $.ajax({
        url: '/Sales/Edit/' + id,
        type: 'POST',
        data: form.serialize(),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function (result) {
            console.log('Success response:', result);
            if (result.success) {
                // Hide the modal (Tailwind)
                const modal = document.getElementById('editSaleModal');
                if (modal) {
                    modal.classList.add('hidden');
                }


                // Show success message
                showSaleUpdatedSuccess(
                    result.saleId || id,
                    result.customerName || 'غير محدد',
                    result.totalAmount || '0'
                );

                // Reload after delay
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                showErrorMessage('خطأ في تحديث البيع', result.message || 'حدث خطأ أثناء تحديث البيع');
            }
        },
        error: function (xhr, status, error) {
            console.log('Error response:', xhr.responseText);
            console.log('Status:', status);
            console.log('Error:', error);
            showErrorMessage('خطأ في تحديث البيع', 'حدث خطأ أثناء تحديث البيع: ' + error);
        }
    });
});

// Note: Delete functionality is now handled in Utilities.js using SweetAlert