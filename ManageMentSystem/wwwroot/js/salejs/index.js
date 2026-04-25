// Sales Index Page JavaScript
let selectedProducts = [];
let allProducts = [];
let editSelectedProducts = [];
let searchTimeout;

// Load all products on page load
$(document).ready(function () {
    loadAllProducts();


    $('#modalCustomerSelect').on('change', function () {
        if ($(this).val() === '0') {
            $('#modalNewCustomerFields').show();
            $('#modalNewCustomerFields input[name="NewCustomerName"]').prop('required', true);
            $('#modalNewCustomerFields input[name="NewCustomerPhone"]').prop('required', true);
            $('#modalNewCustomerFields input[name="NewCustomerAddress"]').prop('required', true);
        } else {
            $('#modalNewCustomerFields').hide();
            $('#modalNewCustomerFields input[name="NewCustomerName"]').prop('required', false);
            $('#modalNewCustomerFields input[name="NewCustomerPhone"]').prop('required', false);
            $('#modalNewCustomerFields input[name="NewCustomerAddress"]').prop('required', false);
        }
    });

    // Handle customer selection change in edit modal
    $('#editSaleCustomerId').change(function () {
        var selectedValue = $(this).val();
        if (selectedValue === '0') {
            $('#editModalNewCustomerFields').show();
            $('#editModalNewCustomerName').prop('required', true);
            $('#editModalNewCustomerPhone').prop('required', true);
            $('#editModalNewCustomerAddress').prop('required', false);
        } else {
            $('#editModalNewCustomerFields').hide();
            $('#editModalNewCustomerName').prop('required', false);
            $('#editModalNewCustomerPhone').prop('required', false);
            $('#editModalNewCustomerAddress').prop('required', false);
        }
    });
});

// Load all products
function loadAllProducts() {
    $.get('/Sales/GetProducts', function (data) {
        allProducts = data;
    });
}