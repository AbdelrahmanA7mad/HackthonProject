// SweetAlert2 functions for Customers page
// Refactored to use unified global configuration (swal-config.js)

// Success message function
function showSuccessMessage(title, text) {
    return showSuccess(title, text);
}

// Error message function
function showErrorMessage(title, text) {
    return showError(title, text);
}

// Warning message function
function showWarningMessage(title, text) {
    return showWarning(title, text);
}

// Info message function
function showInfoMessage(title, text) {
    return showInfo(title, text);
}

// Confirmation dialog for delete operations
function showDeleteConfirmation(title, text, confirmCallback) {
    showDangerConfirm(title, text, 'نعم، احذف!', 'إلغاء').then((result) => {
        if (result.isConfirmed) {
            confirmCallback();
        }
    });
}

// Loading and Hide Loading
// Removing generic functions as they collide with/shadow global helpers
// Legacy calls to showLoading(title, text) are compatible with global showLoading(title, text)
// hideLoading() is replaced by closeSwal() in global, but legacy code might call hideLoading().
// wrapper:
function hideLoading() {
    closeSwal();
}

// Form validation errors
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
    SwalConfig.fire({
        icon: 'success',
        title: title,
        text: text,
        timer: timer,
        timerProgressBar: true,
        showConfirmButton: false
    });
}

// Custom alert
function showCustomAlert(title, text, icon = 'info', confirmText = 'حسناً', cancelText = 'إلغاء', showCancel = false) {
    return SwalConfig.fire({
        title: title,
        text: text,
        icon: icon,
        showCancelButton: showCancel,
        confirmButtonText: confirmText,
        cancelButtonText: cancelText,
        reverseButtons: true
    });
}

// Toast notification
// showToast is global. Removing.


// Phone validation with SweetAlert
function showPhoneExistsAlert(customerName, customerPhone, customerAddress) {
    return SwalConfig.fire({
        title: 'العميل موجود بالفعل',
        html: `
            <div class="text-start">
                <p><strong>الاسم:</strong> ${customerName}</p>
                <p><strong>رقم الهاتف:</strong> ${customerPhone}</p>
                <p><strong>العنوان:</strong> ${customerAddress || 'غير محدد'}</p>
            </div>
        `,
        icon: 'info',
        showCancelButton: true,
        confirmButtonText: 'استخدام هذا العميل',
        cancelButtonText: 'إلغاء',
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#94a3b8'
    });
}

// WhatsApp sending confirmation
function showWhatsAppConfirmation(message, customerCount) {
    return SwalConfig.fire({
        title: 'تأكيد الإرسال',
        html: `
            <div class="text-start">
                <p><strong>الرسالة:</strong></p>
                <p class="border p-2 rounded bg-gray-50 text-sm">${message}</p>
                <p class="mt-2"><strong>عدد العملاء:</strong> ${customerCount}</p>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'إرسال',
        cancelButtonText: 'إلغاء',
        confirmButtonColor: '#25D366',
        cancelButtonColor: '#94a3b8'
    });
}

// Bulk WhatsApp results
function showBulkWhatsAppResults(results) {
    let successCount = results.filter(r => r.status === 'success').length;
    let failCount = results.filter(r => r.status === 'failed' || r.status === 'error').length;

    let resultsHtml = '<div class="text-start max-h-60 overflow-y-auto">';
    results.forEach(result => {
        let statusIcon = result.status === 'success' ? '✅' : '❌';
        let statusColor = result.status === 'success' ? 'text-emerald-600' : 'text-red-600';
        resultsHtml += `
            <div class="mb-2 p-2 border rounded-lg bg-gray-50">
                <div class="flex justify-between items-center">
                    <span>${statusIcon} <strong>${result.customerName}</strong></span>
                    <span class="${statusColor} font-bold text-xs">${result.status === 'success' ? 'نجح' : 'فشل'}</span>
                </div>
                <small class="text-gray-500 block">${result.phone}</small>
                ${result.status !== 'success' ? `<br><small class="text-red-500">${result.message}</small>` : ''}
            </div>
        `;
    });
    resultsHtml += '</div>';

    SwalConfig.fire({
        title: `نتائج الإرسال الجماعي`,
        html: `
            <div class="text-center mb-3 space-x-2">
                <span class="bg-emerald-100 text-emerald-800 px-2 py-1 rounded-full text-sm font-bold">نجح: ${successCount}</span>
                <span class="bg-red-100 text-red-800 px-2 py-1 rounded-full text-sm font-bold">فشل: ${failCount}</span>
            </div>
            ${resultsHtml}
        `,
        icon: successCount > failCount ? 'success' : 'warning',
        confirmButtonText: 'حسناً',
        confirmButtonColor: '#3b82f6',
        width: '600px'
    });
}
