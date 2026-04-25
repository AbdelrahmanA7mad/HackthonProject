// Details.js - JavaScript functions for Details.cshtml
// This file contains all JavaScript functionality for the product details page

// Add product to sale
function addToSale(productId) {
    // Redirect to sales page with product pre-selected
    window.location.href = '/Sales/Create?productId=' + productId;
}

// Request maintenance for product
function requestMaintenance(productId) {
    // Redirect to maintenance page with product pre-selected
    window.location.href = '/Maintenance/Create?productId=' + productId;
}

// Create installment for product
function createInstallment(productId) {
    // Redirect to installments page with product pre-selected
    window.location.href = '/Installments/Create?productId=' + productId;
}

// Initialize Details page
function initializeDetailsPage() {
    // Add any specific initialization for details page
    console.log('Product Details page initialized');
}

// Document Ready
$(document).ready(function() {
    initializeDetailsPage();
});
