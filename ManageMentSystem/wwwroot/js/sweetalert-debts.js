// SweetAlert2 functions for General Debts page
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

// Loading dialog
// Using global showLoading directly is preferred, but keeping this for backward compatibility
// Note: Global showLoading accepts (title, text)
// The original function used Swal.fire options directly, now delegating.
// If overwrite is needed specific to debts, we use SwalConfig.
// BUT global showLoading is already assigned to window.showLoading, 
// so this local function definition shadows it if not careful.
// However, since this is a var/function in global scope, it might conflict.
// If this file is loaded, it overwrites window.showLoading if it was defined before.
// We should just remove this function if it's identical, OR relay it.
// To be safe and avoid recursion if names are identical, we should NOT redefine standard names if possible.
// But legacy code calls showLoading().
// I will assume showLoading from swal-config.js IS the one to use, 
// so I don't need to redefine it here unless I want to change behavior.
// I will comment it out or remove it to let the global one take over?
// No, javascript function hoisting/overwriting... 
// If I define function showLoading() here, it overwrites the one from swal-config.js.
// I should remove it so the global one is used.
// However, the signature in swal-config is showLoading(title, text).
// The signature here is showLoading(title, text). Matches.
// I will REMOVE showLoading and hideLoading declarations from here so it uses the global ones.

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
    SwalConfig.fire({
        icon: 'success',
        title: title,
        text: text,
        timer: timer,
        timerProgressBar: true,
        showConfirmButton: false,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600'
        }
    });
}

// Custom alert with custom buttons
function showCustomAlert(title, text, icon = 'info', confirmText = 'حسناً', cancelText = 'إلغاء', showCancel = false) {
    return SwalConfig.fire({
        title: title,
        text: text,
        icon: icon,
        showCancelButton: showCancel,
        confirmButtonText: confirmText,
        cancelButtonText: cancelText,
        reverseButtons: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-indigo-600 hover:bg-indigo-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}

// Debt deletion confirmation with detailed warning
function showDebtDeleteConfirmation(debtId, title, partyName, amount, debtType) {
    const debtTypeText = debtType === 'OwedToMe' ? 'دين ليا' : 'دين عليا';
    const debtTypeIcon = debtType === 'OwedToMe' ? 'arrow-down' : 'arrow-up';
    const debtTypeClass = debtType === 'OwedToMe' ? 'bg-emerald-100 text-emerald-800' : 'bg-red-100 text-red-800';

    return SwalConfig.fire({
        title: 'تأكيد حذف الدين',
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>العنوان:</strong> ${title}</p>
                <p class="mb-2"><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p class="mb-2"><strong>المبلغ:</strong> ${amount} جنيه</p>
                <p class="mb-4"><strong>النوع:</strong> 
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold ${debtTypeClass}">
                        <i class="fas fa-${debtTypeIcon} me-1"></i>${debtTypeText}
                    </span>
                </p>
                <div class="bg-amber-50 border border-amber-200 text-amber-800 p-4 rounded-xl mb-2 text-sm">
                    <div class="flex items-center gap-2 mb-1 font-bold">
                        <i class="fas fa-exclamation-triangle"></i>
                        تحذير:
                    </div>
                    سيتم حذف الدين بالكامل مع جميع المدفوعات المرتبطة به.
                </div>
                <p class="text-red-600 font-bold text-sm mt-2">لا يمكن التراجع عن هذا الإجراء.</p>
            </div>
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'نعم، احذف الدين',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-red-600 hover:bg-red-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}

// Payment confirmation
function showPaymentConfirmation(debtTitle, partyName, amount, paymentMethod) {
    return SwalConfig.fire({
        title: 'تأكيد إضافة دفعة',
        html: `
            <div class="text-start space-y-2">
                <p><strong>العنوان:</strong> ${debtTitle}</p>
                <p><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p><strong>مبلغ الدفعة:</strong> <span class="text-emerald-600 font-bold">${amount} جنيه</span></p>
                <p><strong>طريقة الدفع:</strong> ${paymentMethod || 'غير محدد'}</p>
            </div>
        `,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'تأكيد الدفعة',
        cancelButtonText: 'إلغاء',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-600 hover:bg-emerald-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}

// Payment added successfully
function showPaymentAddedSuccess(debtTitle, amount) {
    SwalConfig.fire({
        icon: 'success',
        title: 'تم إضافة الدفعة بنجاح',
        html: `
            <div class="text-start">
                <p><strong>العنوان:</strong> ${debtTitle}</p>
                <p><strong>مبلغ الدفعة:</strong> ${amount} جنيه</p>
            </div>
        `,
        confirmButtonText: 'حسناً',
        timer: 4000,
        timerProgressBar: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Debt created successfully
function showDebtCreatedSuccess(title, partyName, amount, debtType) {
    const debtTypeText = debtType === 'OwedToMe' ? 'دين ليا' : 'دين عليا';
    const debtTypeIcon = debtType === 'OwedToMe' ? 'arrow-down' : 'arrow-up';
    const debtTypeClass = debtType === 'OwedToMe' ? 'bg-emerald-100 text-emerald-800' : 'bg-red-100 text-red-800';

    SwalConfig.fire({
        icon: 'success',
        title: 'تم إنشاء الدين بنجاح',
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>العنوان:</strong> ${title}</p>
                <p class="mb-2"><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p class="mb-2"><strong>المبلغ:</strong> ${amount} جنيه</p>
                <p><strong>النوع:</strong> 
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold ${debtTypeClass}">
                        <i class="fas fa-${debtTypeIcon} me-1"></i>${debtTypeText}
                    </span>
                </p>
            </div>
        `,
        confirmButtonText: 'حسناً',
        timer: 4000,
        timerProgressBar: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Debt updated successfully
function showDebtUpdatedSuccess(title, partyName, amount, debtType) {
    const debtTypeText = debtType === 'OwedToMe' ? 'دين ليا' : 'دين عليا';
    const debtTypeIcon = debtType === 'OwedToMe' ? 'arrow-down' : 'arrow-up';
    const debtTypeClass = debtType === 'OwedToMe' ? 'bg-emerald-100 text-emerald-800' : 'bg-red-100 text-red-800';

    SwalConfig.fire({
        icon: 'success',
        title: 'تم تحديث الدين بنجاح',
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>العنوان:</strong> ${title}</p>
                <p class="mb-2"><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p class="mb-2"><strong>المبلغ:</strong> ${amount} جنيه</p>
                <p><strong>النوع:</strong> 
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold ${debtTypeClass}">
                        <i class="fas fa-${debtTypeIcon} me-1"></i>${debtTypeText}
                    </span>
                </p>
            </div>
        `,
        confirmButtonText: 'حسناً',
        timer: 4000,
        timerProgressBar: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Debt deleted successfully
function showDebtDeletedSuccess() {
    showSuccess('تم حذف الدين بنجاح', 'تم حذف الدين مع جميع المدفوعات المرتبطة به');
}

// Payment validation error
function showPaymentValidationError(message) {
    showError('خطأ في بيانات الدفعة', message);
}

// Debt validation error
function showDebtValidationError(message) {
    showError('خطأ في بيانات الدين', message);
}

// Amount validation error
function showAmountValidationError(message) {
    showError('خطأ في المبلغ', message);
}

// Payment method validation error
function showPaymentMethodValidationError(message) {
    showError('خطأ في طريقة الدفع', message);
}

// Debt completion notification
function showDebtCompletionNotification(title, partyName, totalAmount) {
    SwalConfig.fire({
        icon: 'success',
        title: 'تم إكمال الدين!',
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>العنوان:</strong> ${title}</p>
                <p class="mb-2"><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p class="mb-2"><strong>إجمالي المبلغ:</strong> ${totalAmount} جنيه</p>
                <div class="bg-emerald-50 border border-emerald-200 text-emerald-800 p-3 rounded-lg mt-3 text-sm">
                    <i class="fas fa-check-circle me-2"></i>
                    <strong>تهانينا!</strong> تم سداد الدين بالكامل.
                </div>
            </div>
        `,
        confirmButtonText: 'حسناً',
        timer: 5000,
        timerProgressBar: true,
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-500 hover:bg-emerald-600'
        }
    });
}

// Debt overdue warning
function showDebtOverdueWarning(title, partyName, dueDate, daysOverdue) {
    SwalConfig.fire({
        icon: 'warning',
        title: 'دين متأخر!',
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>العنوان:</strong> ${title}</p>
                <p class="mb-2"><strong>الطرف:</strong> ${partyName || 'غير محدد'}</p>
                <p class="mb-2"><strong>تاريخ الاستحقاق:</strong> ${dueDate}</p>
                <p class="mb-2"><strong>عدد الأيام المتأخرة:</strong> <span class="text-red-600 font-bold">${daysOverdue} يوم</span></p>
                <div class="bg-amber-50 border border-amber-200 text-amber-800 p-3 rounded-lg mt-3 text-sm">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>تنبيه:</strong> هذا الدين متأخر عن موعد استحقاقه.
                </div>
            </div>
        `,
        confirmButtonText: 'حسناً',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-amber-500 hover:bg-amber-600'
        }
    });
}

// Bulk operations confirmation
function showBulkOperationConfirmation(operation, count) {
    return SwalConfig.fire({
        title: `تأكيد العملية الجماعية`,
        text: `هل تريد ${operation} ${count} دين؟`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'نعم، تأكيد',
        cancelButtonText: 'إلغاء',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-blue-600 hover:bg-blue-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}

// Export confirmation
function showExportConfirmation(format) {
    return SwalConfig.fire({
        title: 'تأكيد التصدير',
        text: `هل تريد تصدير البيانات بصيغة ${format}؟`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'نعم، صدّر',
        cancelButtonText: 'إلغاء',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-emerald-600 hover:bg-emerald-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}
