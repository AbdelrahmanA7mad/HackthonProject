// SweetAlert2 functions for Sales page
// This file now uses the unified SweetAlert configuration from swal-config.js

// Success message function
function showSuccessMessage(title, text) {
    showSuccess(title, text);
}

// Error message function
function showErrorMessage(title, text) {
    showError(title, text);
}

// Warning message function
function showWarningMessage(title, text) {
    showWarning(title, text);
}

// Info message function
function showInfoMessage(title, text) {
    showInfo(title, text);
}

// Confirmation dialog for delete operations
function showDeleteConfirmation(title, text, confirmCallback) {
    showDangerConfirm(title, text).then((result) => {
        if (result.isConfirmed) {
            confirmCallback();
        }
    });
}

// hideLoading is already defined as closeSwal() in swal-config.js
function hideLoading() {
    closeSwal();
}

// Form validation with SweetAlert
function showValidationError(errors) {
    let errorText = '';
    if (Array.isArray(errors)) {
        errorText = errors.join('<br>');
    } else {
        errorText = errors;
    }

    showError('خطأ في التحقق من البيانات', errorText);
}

// Success with auto close
function showSuccessAutoClose(title, text, timer = 2000) {
    showSuccess(title, text, timer);
}

// Custom alert with custom buttons
function showCustomAlert(title, text, icon = 'info', confirmText = 'حسناً', cancelText = 'إلغاء', showCancel = false) {
    if (showCancel) {
        return showConfirm(title, text, confirmText, cancelText);
    } else {
        // Choose based on icon
        switch (icon) {
            case 'success': return showSuccess(title, text);
            case 'error': return showError(title, text);
            case 'warning': return showWarning(title, text);
            default: return showInfo(title, text);
        }
    }
}

// Sale deletion confirmation with detailed warning
function showSaleDeleteConfirmation(saleId, customerName, totalAmount) {
    return showDangerConfirm(
        'تأكيد حذف البيع',
        `<div class="text-start">
            <p><strong>العميل:</strong> ${customerName}</p>
            <p><strong>المبلغ الإجمالي:</strong> ${totalAmount} جنيه</p>
            <div class="alert alert-warning mt-3">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>تحذير:</strong> سيتم حذف البيع بالكامل وإرجاع المنتجات للمخزون وحذف جميع العمليات المرتبطة.
            </div>
            <p class="text-danger"><strong>لا يمكن التراجع عن هذا الإجراء.</strong></p>
        </div>`
    );
}

// Product validation error
function showProductValidationError(message) {
    showError('خطأ في المنتج', message);
}

// Customer validation error
function showCustomerValidationError(message) {
    showError('خطأ في بيانات العميل', message);
}

// Payment validation error
function showPaymentValidationError(message) {
    showError('خطأ في بيانات الدفع', message);
}

// Sale saved successfully
// Sale saved successfully
function showSaleSavedSuccess(saleId, customerName, totalAmount) {
    SwalConfig.fire({
        icon: 'success',
        title: 'تم حفظ البيع بنجاح',
        html: `<div class="text-start">
            <p><strong>رقم البيع:</strong> ${saleId}</p>
            <p><strong>العميل:</strong> ${customerName}</p>
            <p><strong>المبلغ الإجمالي:</strong> ${totalAmount} جنيه</p>
        </div>`,
        timer: 4000,
        timerProgressBar: true,
        confirmButtonText: 'حسناً',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Sale updated successfully
function showSaleUpdatedSuccess(saleId, customerName, totalAmount) {
    SwalConfig.fire({
        icon: 'success',
        title: 'تم تحديث البيع بنجاح',
        html: `<div class="text-start">
            <p><strong>رقم البيع:</strong> ${saleId}</p>
            <p><strong>العميل:</strong> ${customerName}</p>
            <p><strong>المبلغ الإجمالي:</strong> ${totalAmount} جنيه</p>
        </div>`,
        timer: 4000,
        timerProgressBar: true,
        confirmButtonText: 'حسناً',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Sale deleted successfully
function showSaleDeletedSuccess() {
    showSuccess('تم حذف البيع بنجاح', 'تم حذف البيع وإرجاع المنتجات للمخزون');
}

// WhatsApp sending confirmation
function showWhatsAppConfirmation(customerName, invoiceNumber) {
    return SwalConfig.fire({
        title: 'تأكيد إرسال الفاتورة',
        html: `
            <div class="text-start">
                <p><strong>العميل:</strong> ${customerName}</p>
                <p><strong>رقم الفاتورة:</strong> ${invoiceNumber}</p>
                <p>سيتم إرسال الفاتورة عبر WhatsApp</p>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'إرسال',
        cancelButtonText: 'إلغاء',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-[#25D366] hover:bg-[#128C7E]',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}

// WhatsApp sent successfully
function showWhatsAppSentSuccess() {
    showToast('تم إرسال الفاتورة عبر WhatsApp', 'success');
}

// WhatsApp send failed
function showWhatsAppSendFailed(error) {
    showError('فشل في الإرسال', error || 'حدث خطأ أثناء إرسال الفاتورة');
}

// Barcode scan success
function showBarcodeScanSuccess(productName) {
    showToast(`تم العثور على: ${productName}`, 'success');
}

// Barcode scan error
function showBarcodeScanError(message) {
    showToast(message, 'error');
}

// Product out of stock warning
function showOutOfStockWarning(productName, availableStock) {
    SwalConfig.fire({
        icon: 'warning',
        title: 'المخزون غير كافي',
        html: `<div class="text-start">
            <p><strong>المنتج:</strong> ${productName}</p>
            <p><strong>المخزون المتاح:</strong> ${availableStock}</p>
            <p>يرجى تقليل الكمية المطلوبة أو إضافة المزيد من المخزون</p>
        </div>`,
        confirmButtonText: 'حسناً',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-amber-500 hover:bg-amber-600'
        }
    });
}

// Confirm add product to sale
function showAddProductConfirmation(productName, quantity, unitPrice, totalPrice) {
    return SwalConfig.fire({
        icon: 'question',
        title: 'تأكيد إضافة المنتج',
        html: `<div class="text-start">
            <p><strong>المنتج:</strong> ${productName}</p>
            <p><strong>الكمية:</strong> ${quantity}</p>
            <p><strong>سعر الوحدة:</strong> ${unitPrice} جنيه</p>
            <p><strong>الإجمالي:</strong> ${totalPrice} جنيه</p>
        </div>`,
        showCancelButton: true,
        confirmButtonText: 'إضافة',
        cancelButtonText: 'إلغاء',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-indigo-600 hover:bg-indigo-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}
