// SweetAlert2 functions for Accounts page
// Refactored to use unified global configuration (swal-config.js)

// Account creation/deposit/withdrawal success
function showTransactionSuccess(type, amount) {
    const isIncome = type === 'Income';
    const title = isIncome ? 'تم الإيداع بنجاح' : 'تم السحب بنجاح';
    const icon = 'success';

    SwalConfig.fire({
        icon: icon,
        title: title,
        html: `
            <div class="text-start">
                <p class="mb-2"><strong>المبلغ:</strong> <span class="${isIncome ? 'text-emerald-700' : 'text-red-700'} font-bold">${amount} جنيه</span></p>
                <div class="bg-slate-50 border border-slate-200 text-slate-600 p-3 rounded-lg mt-2 text-sm">
                    <i data-lucide="check-circle" class="me-2 text-emerald-500 inline-block w-4 h-4"></i>
                    تم تسجيل العملية وتحديث الأرصدة.
                </div>
            </div>
        `,
        timer: 3000,
        timerProgressBar: true,
        showConfirmButton: false,
        didOpen: () => {
            lucide.createIcons();
        },
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600'
        }
    });
}

// Validation Error
function showAccountValidationError(message) {
    showError('خطأ في البيانات', message);
}

// Insufficient Funds Error
function showInsufficientFundsError(currentBalance, requestedAmount) {
    SwalConfig.fire({
        icon: 'error',
        title: 'الرصيد غير كافي',
        html: `
            <div class="text-start">
                <p class="mb-2">رصيد وسيلة الدفع غير كافٍ لإتمام العملية.</p>
                <p class="mb-1"><strong>الرصيد المتاح:</strong> ${currentBalance} جنيه</p>
                <p class="mb-2"><strong>المبلغ المطلوب:</strong> ${requestedAmount} جنيه</p>
            </div>
        `,
        confirmButtonText: 'حسناً',
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-red-600 hover:bg-red-700'
        }
    });
}

// Delete Confirmation
function showDeleteTransactionConfirmation(id) {
    return SwalConfig.fire({
        title: 'تأكيد حذف العملية',
        html: `
            <div class="text-start">
                <p class="mb-2">هل أنت متأكد من حذف هذه العملية المالية؟</p>
                <div class="bg-amber-50 border border-amber-200 text-amber-800 p-4 rounded-xl mb-2 text-sm">
                    <div class="flex items-center gap-2 mb-1 font-bold">
                        <i data-lucide="alert-triangle" class="w-4 h-4"></i>
                        تحذير:
                    </div>
                    سيتم عكس تأثير هذه العملية على الأرصدة وحذفها نهائياً.
                </div>
                <p class="text-red-600 font-bold text-sm mt-2">لا يمكن التراجع عن هذا الإجراء.</p>
            </div>
        `,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء',
        reverseButtons: true,
        didOpen: () => {
            lucide.createIcons();
        },
        customClass: {
            popup: 'rounded-2xl shadow-2xl border border-slate-100',
            title: 'text-xl font-bold text-slate-800',
            htmlContainer: 'text-slate-600',
            confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white bg-red-600 hover:bg-red-700',
            cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200'
        }
    });
}
