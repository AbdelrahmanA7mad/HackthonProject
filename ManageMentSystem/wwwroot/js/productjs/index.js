// Index.js - JavaScript functions for Index.cshtml (Category Statistics)
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
    showDeleteConfirmation(
        'تأكيد الحذف',
        'هل أنت متأكد من حذف هذا المنتج؟ لا يمكن التراجع عن هذا الإجراء.',
        function () {
            // Show loading
            showLoading('جاري الحذف...', 'يرجى الانتظار');

            // Submit delete form
            $('#deleteProductForm').attr('action', '/Products/Delete/' + productId);
            $('#deleteProductForm').submit();
        }
    );
}

// Delete Sale function
function deleteSale(saleId) {
    showDeleteConfirmation(
        'تأكيد حذف البيع',
        'هل أنت متأكد من حذف عملية البيع هذه؟ لا يمكن التراجع عن هذا الإجراء.',
        function () {
            // Show loading
            showLoading('جاري الحذف...', 'يرجى الانتظار');

            // Submit delete form
            $('#deleteSaleForm').attr('action', '/Sales/Delete/' + saleId);
            $('#deleteSaleForm').submit();
        }
    );
}

// Auto-select category when modal opens
function initializeAddProductModal() {
    $('#addProductModal').on('show.bs.modal', function () {
        var currentCategoryId = window.currentCategoryId || 0;
        if (currentCategoryId > 0) {
            $('#CategoryId').val(currentCategoryId);
            // Also update the hidden field
            $('input[name="CategoryId"]').val(currentCategoryId);
        }

        // تعيين الوصف تلقائياً
        $('#Description').val('-');
    });
}

// Generate barcode button click handler
function initializeBarcodeGeneration() {
    $('#generateBarcodeBtn').on('click', function () {
        generateBarcode('#Barcode');
    });

    // Generate barcode button click handler for edit form
    $('#editGenerateBarcodeBtn').on('click', function () {
        generateBarcode('#editProductBarcode');
    });
}

// Barcode validation functions
function initializeBarcodeValidation() {
    // Barcode validation for add product form
    $('#Barcode').on('blur', function () {
        var barcode = $(this).val().trim();
        if (barcode) {
            checkBarcodeUnique(barcode, null, '#addBarcodeAlert', '#addBarcodeAlertText', '#addViewExistingProduct');
        } else {
            $('#addBarcodeAlert').hide();
        }
    });

    // Barcode validation for edit product form
    $('#editProductBarcode').on('blur', function () {
        var barcode = $(this).val().trim();
        var productId = $('#editProductId').val();
        if (barcode) {
            checkBarcodeUnique(barcode, productId, '#editBarcodeAlert', '#editBarcodeAlertText', '#editViewExistingProduct');
        } else {
            $('#editBarcodeAlert').hide();
        }
    });
}

// Edit Product function
function editProduct(id) {
    $.get(`/Products/Edit/${id}`, function (data) {
        $('#editProductId').val(data.id);
        $('#editProductName').val(data.name);
        $('#editProductCategoryId').val(data.categoryId);
        $('#editProductQuantity').val(data.quantity);
        $('#editProductDescription').val(data.description);
        $('#editProductPurchasePrice').val(data.purchasePrice);
        $('#editProductSalePrice').val(data.salePrice);
        $('#editProductBarcode').val(data.barcode);

        $('#editProductModal').modal('show');

    }).fail(() => showErrorMessage('خطأ', 'حدث خطأ أثناء تحميل بيانات المنتج'));
}

// Edit Product Form Submission
function initializeEditProductForm() {
    $('#editProductForm').on('submit', function (e) {
        e.preventDefault();
        var id = $('#editProductId').val();
        var submitBtn = $(this).find('button[type="submit"]');

        setButtonLoading(submitBtn, true, 'جارٍ الحفظ...', 'حفظ التغييرات');

        $.ajax({
            url: `/Products/Edit/${id}`,
            type: 'POST',
            data: $(this).serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    $('#editProductModal').modal('hide');
                    showSuccessAutoClose('تم بنجاح', 'تم تحديث المنتج بنجاح');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showErrorMessage('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء تحديث المنتج';
                showErrorMessage('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حفظ التغييرات');
            }
        });
    });
}

// Function to check if barcode is unique
function checkBarcodeUnique(barcode, excludeProductId, alertSelector, textSelector, linkSelector) {
    if (!barcode) return;

    $.get('/Products/CheckBarcodeUnique', {
        barcode: barcode,
        id: excludeProductId
    }, function (isUnique) {
        if (!isUnique) {
            // Barcode exists, show alert
            $(textSelector).text(`الباركود '${barcode}' مستخدم بالفعل في منتج آخر.`);
            $(alertSelector).show();

            // Get product details for the link
            $.get('/Products/SearchByBarcode', { barcode: barcode }, function (product) {
                if (product) {
                    $(linkSelector).attr('href', `/Products/Edit/${product.id}`);
                    $(linkSelector).text(`عرض المنتج: ${product.name}`);
                }
            });
        } else {
            $(alertSelector).hide();
        }
    });
}

// دالة طباعة جميع الأسعار من الصفحة الحالية فقط
function collectAndPrintAllPrices() {
    var products = [];
    $('#productsTable tbody tr').each(function () {
        var $row = $(this);
        var name = $row.find('td:eq(0)').text().trim();
        var $priceCell = $row.find('.sale-price-cell');
        var priceText = $priceCell.length > 0 ? $priceCell.text().trim() : $row.find('td:eq(3)').text().trim();
        var price = parseFloat(priceText.replace(/[^\d.-]/g, ''));

        if (name && !isNaN(price)) {
            products.push({
                name: name,
                salePrice: price
            });
        }
    });

    if (products.length > 0) {
        printAllPrices(products, window.currentCategoryName || '');
    } else {
        showWarningMessage('تنبيه', 'لا توجد منتجات لطباعة أسعارها في الصفحة الحالية');
    }
}

// Handle form submission for add product
function initializeAddProductForm() {
    $('#addProductForm').on('submit', function (e) {
        e.preventDefault();

        var formData = $(this).serialize();
        var submitBtn = $(this).find('button[type="submit"]');
        var originalText = submitBtn.text();

        submitBtn.prop('disabled', true).text('جارٍ الحفظ...');

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    // Close modal and refresh page
                    $('#addProductModal').modal('hide');
                    showSuccessAutoClose('تم بنجاح', 'تم إضافة المنتج بنجاح');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showErrorMessage('خطأ', response.message);
                }
            },
            error: function () {
                showErrorMessage('خطأ', 'حدث خطأ أثناء حفظ المنتج');
            },
            complete: function () {
                submitBtn.prop('disabled', false).text(originalText);
            }
        });
    });
}

// Edit Product Quantity Only
function editProductQuantity(id) {
    $.get(`/Products/Edit/${id}`, function (data) {
        $('#editQuantityProductId').val(data.id);
        $('#editQuantityProductName').val(data.name);
        $('#currentQuantity').val(data.quantity);
        $('#editQuantityNew').val(data.quantity);

        // Set hidden fields with current values
        $('#editQuantityName').val(data.name);
        $('#editQuantityCategoryId').val(data.categoryId);
        $('#editQuantityPurchasePrice').val(data.purchasePrice);
        $('#editQuantitySalePrice').val(data.salePrice);
        $('#editQuantityDescription').val(data.description);
        $('#editQuantityBarcode').val(data.barcode);

        $('#editQuantityModal').modal('show');
    }).fail(() => showErrorMessage('خطأ', 'حدث خطأ أثناء تحميل بيانات المنتج'));
}

// Edit Quantity Form Submission
function initializeEditQuantityForm() {
    $('#editQuantityForm').on('submit', function (e) {
        e.preventDefault();
        var id = $('#editQuantityProductId').val();
        var submitBtn = $(this).find('button[type="submit"]');

        setButtonLoading(submitBtn, true, 'جارٍ الحفظ...', 'حفظ الكمية الجديدة');

        $.ajax({
            url: `/Products/UpdateQuantity/${id}`,
            type: 'POST',
            data: $(this).serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    $('#editQuantityModal').modal('hide');
                    showSuccessAutoClose('تم بنجاح', 'تم تحديث الكمية بنجاح');
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showErrorMessage('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء تحديث الكمية';
                showErrorMessage('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حفظ الكمية الجديدة');
            }
        });
    });
}

// Initialize all event handlers for Index page
function initializeIndexPage() {
    // Page size change handler
    $('#pageSizeSelect').on('change', function () {
        var newPageSize = $(this).val();
        var currentPage = window.currentPage || 1;
        var categoryId = window.currentCategoryId || 0;
        window.location.href = `/Products/ByCategory/${categoryId}?page=${currentPage}&pageSize=${newPageSize}`;
    });

    // Initialize all form handlers
    initializeAddProductModal();
    initializeBarcodeGeneration();
    initializeBarcodeValidation();
    initializeEditProductForm();
    initializeAddProductForm();
    initializeEditQuantityForm();
}

// Document Ready
$(document).ready(function () {
    initializeIndexPage();
});
