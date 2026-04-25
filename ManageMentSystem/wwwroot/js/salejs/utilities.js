// Utility Functions for Sales

// Show toast message using SweetAlert
function showToast(message, type) {
    const iconType = type === 'success' ? 'success' : type === 'error' ? 'error' : 'info';
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    });

    Toast.fire({
        icon: iconType,
        title: message
    });
}

// Edit Sale
function editSale(id) {
    $.get('/Sales/Edit/' + id, function (data) {
        $('#editSaleId').val(data.id);
        $('#editSaleCustomerId').val(data.customerId);
        $('#editSaleDate').val(data.saleDate);
        $('#editSaleTotalAmount').val(data.totalAmount);
        // نوع الدفع والمبلغ المدفوع
        if (typeof data.paymentType !== 'undefined') {
            $('#editPaymentType').val(String(data.paymentType));
            $('#editPaymentType').trigger('change');
        }
        if (typeof data.paidAmount !== 'undefined') {
            $('#editPaidAmount').val(data.paidAmount);
        }

        // تحميل قيم الخصم
        $('#editDiscountPercentage').val(data.discountPercentage || 0);
        $('#editDiscountAmount').val(data.discountAmount || 0);
        updateEditAmountAfterDiscount();

        // تعيين اسم العميل في حقل البحث
        $('#editCustomerSearch').val(data.customerName);

        // تحميل المنتجات المختارة
        editSelectedProducts = [];
        if (data.saleItems && data.saleItems.length > 0) {
            data.saleItems.forEach(function (item) {
                editSelectedProducts.push({
                    productId: item.productId,
                    productName: item.productName,
                    quantity: item.quantity,
                    unitPrice: item.unitPrice,
                    customSalePrice: item.unitPrice,
                    purchasePrice: item.purchasePrice,
                    availableStock: 999, // سيتم تحديثه لاحقاً
                    barcode: item.barcode || ''
                });
            });
        }

        // عرض المنتجات المختارة
        displayEditSelectedProducts();
        updateEditTotalAmount();

        // Show the modal (Tailwind)
        const modal = document.getElementById('editSaleModal');
        if (modal) {
            modal.classList.remove('hidden');
        }
    });
}

// Delete Sale using SweetAlert
function deleteSale(id) {
    // Get sale details for confirmation
    $.get('/Sales/Details/' + id, function (data) {
        showSaleDeleteConfirmation(
            id,
            data.customerName || 'غير محدد',
            data.totalAmount || '0'
        ).then((result) => {
            if (result.isConfirmed) {
                // Show loading
                showLoading('جاري الحذف...', 'يرجى الانتظار');

                // Delete the sale
                $.ajax({
                    url: '/Sales/Delete/' + id,
                    type: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (result) {
                        hideLoading();
                        if (result.success) {
                            showSaleDeletedSuccess();
                            // Reload the page after a short delay
                            setTimeout(() => {
                                location.reload();
                            }, 1500);
                        } else {
                            showErrorMessage('خطأ في الحذف', result.message || 'فشل في حذف البيع');
                        }
                    },
                    error: function (xhr, status, error) {
                        hideLoading();
                        showErrorMessage('خطأ في الحذف', 'حدث خطأ أثناء حذف البيع: ' + error);
                    }
                });
            }
        });
    }).fail(function () {
        // If we can't get sale details, show simple confirmation
        showDeleteConfirmation(
            'تأكيد الحذف',
            'هل أنت متأكد من حذف هذا البيع؟ سيتم حذف البيع بالكامل وإرجاع المنتجات للمخزون.',
            function () {
                showLoading('جاري الحذف...', 'يرجى الانتظار');

                $.ajax({
                    url: '/Sales/Delete/' + id,
                    type: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (result) {
                        hideLoading();
                        if (result.success) {
                            showSaleDeletedSuccess();
                            setTimeout(() => {
                                location.reload();
                            }, 1500);
                        } else {
                            showErrorMessage('خطأ في الحذف', result.message || 'فشل في حذف البيع');
                        }
                    },
                    error: function (xhr, status, error) {
                        hideLoading();
                        showErrorMessage('خطأ في الحذف', 'حدث خطأ أثناء حذف البيع: ' + error);
                    }
                });
            }
        );
    });
}

// View Sale
function viewSale(id) {
    window.location.href = '/Sales/Details/' + id;
}

// Send Invoice WhatsApp using SweetAlert
function sendInvoiceWhatsapp(saleId, btn) {
    var $btn = $(btn);

    // Get sale details for confirmation
    $.get('/Sales/Details/' + saleId, function (data) {
        showWhatsAppConfirmation(
            data.customerName || 'غير محدد',
            data.id || saleId
        ).then((result) => {
            if (result.isConfirmed) {
                // Show loading
                showLoading('جاري الإرسال...', 'يرجى الانتظار');

                // Disable button
                $btn.prop('disabled', true);

                $.ajax({
                    url: '/Sales/SendWhatsapp/' + saleId,
                    type: 'GET',
                    success: function (result) {
                        hideLoading();
                        $btn.prop('disabled', false);

                        if (result.success) {
                            showWhatsAppSentSuccess();
                        } else {
                            showWhatsAppSendFailed(result.message);
                        }
                    },
                    error: function () {
                        hideLoading();
                        $btn.prop('disabled', false);
                        showWhatsAppSendFailed('حدث خطأ أثناء إرسال الفاتورة');
                    }
                });
            }
        });
    }).fail(function () {
        // If we can't get sale details, show simple confirmation
        showCustomAlert(
            'تأكيد الإرسال',
            'هل تريد إرسال الفاتورة عبر WhatsApp؟',
            'question',
            'إرسال',
            'إلغاء',
            true
        ).then((result) => {
            if (result.isConfirmed) {
                showLoading('جاري الإرسال...', 'يرجى الانتظار');
                $btn.prop('disabled', true);

                $.ajax({
                    url: '/Sales/SendWhatsapp/' + saleId,
                    type: 'GET',
                    success: function (result) {
                        hideLoading();
                        $btn.prop('disabled', false);

                        if (result.success) {
                            showWhatsAppSentSuccess();
                        } else {
                            showWhatsAppSendFailed(result.message);
                        }
                    },
                    error: function () {
                        hideLoading();
                        $btn.prop('disabled', false);
                        showWhatsAppSendFailed('حدث خطأ أثناء إرسال الفاتورة');
                    }
                });
            }
        });
    });
}

// Form submission handler for save button
document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form"); // أو حدد الـ form المعين لو فيه أكتر من واحد
    const saveBtn = document.getElementById("saveBtn");

    if (form && saveBtn) {
        form.addEventListener("submit", function () {
            saveBtn.disabled = true;
            saveBtn.innerText = "جارٍ الحفظ..."; // اختياري
        });
    }
});

// Close Edit Modal Function (for Tailwind)
function closeEditModal() {
    const modal = document.getElementById('editSaleModal');
    if (modal) {
        modal.classList.add('hidden');
    }
}
