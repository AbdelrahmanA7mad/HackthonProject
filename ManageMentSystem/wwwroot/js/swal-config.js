/**
 * ========================================
 * Unified SweetAlert2 Configuration
 * ========================================
 * مركز التنبيهات الموحد لجميع صفحات التطبيق
 */

// التكوين الأساسي الموحد للكلاسات
const swalClasses = {
    popup: 'rounded-2xl shadow-2xl border border-slate-100',
    title: 'text-xl font-bold text-slate-800',
    htmlContainer: 'text-slate-600',
    confirmButton: 'px-6 py-2.5 rounded-xl font-bold shadow-lg transition-all hover:-translate-y-0.5 mx-1 text-white',
    cancelButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-gray-700 bg-gray-100 border border-gray-200',
    denyButton: 'px-6 py-2.5 rounded-xl font-bold transition-all hover:-translate-y-0.5 mx-1 text-white bg-red-500',
};

// التكوين الأساسي لـ SweetAlert
const SwalConfig = Swal.mixin({
    buttonsStyling: false,
    showClass: {
        popup: 'animate__animated animate__zoomIn animate__faster'
    },
    hideClass: {
        popup: 'animate__animated animate__zoomOut animate__faster'
    },
    position: 'center',
    reverseButtons: true
});

/**
 * دالة النجاح
 */
window.showSuccess = function (title, text = '', timer = 3000) {
    return SwalConfig.fire({
        icon: 'success',
        title: title,
        text: text,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-emerald-700 hover:bg-emerald-600'
        },
        confirmButtonText: 'حسناً',
        timer: timer,
        timerProgressBar: true
    });
};

/**
 * دالة الخطأ
 */
window.showError = function (title, text = '') {
    return SwalConfig.fire({
        icon: 'error',
        title: title,
        text: text,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-red-700 hover:bg-red-800'
        },
        confirmButtonText: 'حسناً',
    });
};

/**
 * دالة التحذير
 */
window.showWarning = function (title, text = '') {
    return SwalConfig.fire({
        icon: 'warning',
        title: title,
        text: text,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-amber-700 hover:bg-amber-800'
        },
        confirmButtonText: 'حسناً',
    });
};

/**
 * دالة المعلومات
 */
window.showInfo = function (title, text = '') {
    return SwalConfig.fire({
        icon: 'info',
        title: title,
        text: text,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-indigo-800 hover:bg-indigo-900'
        },
        confirmButtonText: 'حسناً',
    });
};

/**
 * دالة التأكيد (نعم/لا)
 */
window.showConfirm = function (title, text = '', confirmText = 'نعم، تأكيد', cancelText = 'إلغاء') {
    return SwalConfig.fire({
        icon: 'question',
        title: title,
        text: text,
        showCancelButton: true,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-indigo-800 hover:bg-indigo-900'
        },
        confirmButtonText: confirmText,
        cancelButtonText: cancelText,
    });
};

/**
 * دالة التأكيد الخطر (للحذف مثلاً)
 */
window.showDangerConfirm = function (title, html = '', confirmText = 'نعم، احذف', cancelText = 'إلغاء') {
    return SwalConfig.fire({
        icon: 'warning',
        title: title,
        html: html,
        showCancelButton: true,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-red-800 hover:bg-red-900'
        },
        confirmButtonText: confirmText,
        cancelButtonText: cancelText,
    });
};

/**
 * دالة التحميل/الانتظار
 */
window.showLoading = function (title = 'جارٍ العمل...', text = 'يرجى الانتظار') {
    return SwalConfig.fire({
        title: title,
        text: text,
        customClass: swalClasses, // استخدام الكلاسات الأساسية بدون ألوان خاصة للأزرار (مخفية أصلاً)
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: false,
        showConfirmButton: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
};

/**
 * دالة إغلاق SweetAlert
 */
window.closeSwal = function () {
    Swal.close();
};

/**
 * دالة Toast (إشعار صغير)
 */
window.showToast = function (title, icon = 'success', position = 'top-end') {
    const Toast = Swal.mixin({
        toast: true,
        position: position,
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        customClass: {
            popup: 'rounded-xl shadow-xl border border-slate-100',
            title: 'text-sm font-bold m-0 p-0',
            timerProgressBar: icon === 'error' ? 'bg-red-500' : (icon === 'warning' ? 'bg-amber-700' : 'bg-emerald-700')
        },
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    return Toast.fire({
        icon: icon,
        title: title
    });
};

/**
 * دالة نموذج إدخال
 */
window.showInputDialog = function (title, inputType = 'text', inputPlaceholder = '', inputValue = '') {
    return SwalConfig.fire({
        title: title,
        input: inputType,
        inputPlaceholder: inputPlaceholder,
        inputValue: inputValue,
        showCancelButton: true,
        customClass: {
            ...swalClasses,
            confirmButton: swalClasses.confirmButton + ' bg-indigo-800 hover:bg-indigo-900'
        },
        confirmButtonText: 'تأكيد',
        cancelButtonText: 'إلغاء',
        inputValidator: (value) => {
            if (!value) {
                return 'يرجى إدخال قيمة';
            }
        }
    });
};

// تصدير التكوين للاستخدام المباشر إذا لزم الأمر
window.SwalConfig = SwalConfig;
