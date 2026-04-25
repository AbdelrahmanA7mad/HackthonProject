// CategoryStatistics.js - JavaScript functions for CategoryStatistics.cshtml
// This file contains all JavaScript functionality for the category statistics page

// Utility function to handle button states
function setButtonLoading(button, isLoading, loadingText, originalText) {
    if (isLoading) {
        button.prop('disabled', true).text(loadingText);
    } else {
        button.prop('disabled', false).text(originalText);
    }
}

// Delete Product function
function deleteProduct(productId) {
    $('#deleteProductForm').attr('action', '/Products/Delete/' + productId);
    var modal = new bootstrap.Modal(document.getElementById('deleteProductModal'));
    modal.show();
}

// Delete Sale function
function deleteSale(saleId) {
    $('#deleteSaleForm').attr('action', '/Sales/Delete/' + saleId);
    var modal = new bootstrap.Modal(document.getElementById('deleteSaleModal'));
    modal.show();
}

// Initialize Category Statistics page
function initializeCategoryStatisticsPage() {
    // Page size change handler
    $('#pageSizeSelect').on('change', function() {
        var newPageSize = $(this).val();
        var currentPage = window.currentPage || 1;
        var categoryId = window.currentCategoryId || 0;
        window.location.href = `/Products/ByCategory/${categoryId}?page=${currentPage}&pageSize=${newPageSize}`;
    });
}

// Document Ready
$(document).ready(function () {
    initializeCategoryStatisticsPage();
});
