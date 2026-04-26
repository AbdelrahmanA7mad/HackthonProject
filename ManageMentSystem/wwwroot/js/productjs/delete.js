// Delete.js - JavaScript functions for Delete.cshtml
// This file contains all JavaScript functionality for the product delete confirmation page

// Confirm delete function
function confirmDelete() {
    return confirm('هل أنت متأكد تماماً من حذف هذا المنتج؟\n\nسيتم حذف المنتج من قائمة المنتجات المتوفرة.\nالعمليات السابقة (مبيعات، أقساط، ديون مؤجلة) ستبقى محفوظة.\n\nلا يمكن التراجع عن هذا الإجراء.');
}

// Initialize Delete page
function initializeDeletePage() {
    // Prevent accidental form submission
    $('form').on('submit', function(e) {
        if (!confirmDelete()) {
            e.preventDefault();
            return false;
        }
    });

    // Add visual feedback
    $('.btn-danger').on('click', function() {
        $(this).prop('disabled', true);
        $(this).html('<i data-lucide="loader-2" class="animate-spin me-2 inline-block w-4 h-4"></i>جاري الحذف...');
        lucide.createIcons();
    });
}

// Document Ready
$(document).ready(function() {
    initializeDeletePage();
});
