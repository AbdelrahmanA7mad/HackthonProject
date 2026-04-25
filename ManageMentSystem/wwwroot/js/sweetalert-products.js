// SweetAlert2 functions for Products page
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
