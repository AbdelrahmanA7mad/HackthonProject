// Categories.js - JavaScript functions for Categories pages
// This file contains all JavaScript functionality for the categories management pages

// Initialize Categories page
function initializeCategoriesPage() {
    // Page size change handler
    $('#pageSizeSelect').on('change', function() {
        var newPageSize = $(this).val();
        var currentPage = window.currentPage || 1;
        window.location.href = `/Categories?page=${currentPage}&pageSize=${newPageSize}`;
    });
}

// Document Ready
$(document).ready(function() {
    initializeCategoriesPage();
});
