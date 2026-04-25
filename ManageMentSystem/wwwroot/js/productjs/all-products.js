// AllProducts.js - JavaScript functions for AllProducts.cshtml
// This file contains all JavaScript functionality for the main products listing page

// Utility function to handle button states
function setButtonLoading(button, isLoading, loadingText, originalText) {
    if (isLoading) {
        button.prop('disabled', true).text(loadingText);
    } else {
        button.prop('disabled', false).text(originalText);
    }
}

// Utility function to handle button states
function setButtonLoading(button, isLoading, loadingText, originalText) {
    if (isLoading) {
        button.prop('disabled', true).text(loadingText);
    } else {
        button.prop('disabled', false).text(originalText);
    }
}

// Export to Excel function - Server-side export for all products
function exportToExcel() {
    // Show loading with SweetAlert
    showLoading('جاري التصدير...', 'يرجى الانتظار أثناء تحضير الملف');

    // Use server-side export to get all products, not just current page
    exportToExcelServerSide();
}

// Alternative server-side export function
function exportToExcelServerSide() {
    // Create a temporary form for file download
    var form = $('<form>', {
        'method': 'POST',
        'action': '/Products/ExportToExcel',
        'style': 'display: none'
    });

    // Add anti-forgery token
    form.append($('<input>', {
        'type': 'hidden',
        'name': '__RequestVerificationToken',
        'value': $('input[name="__RequestVerificationToken"]').val()
    }));

    // Submit form
    $('body').append(form);
    form.submit();
    form.remove();

    // Hide loading and show success message after delay
    setTimeout(function () {
        closeSwal();
        showSuccess('تم التصدير بنجاح', 'تم تحميل ملف Excel بنجاح');
    }, 1000);
}


// Add Product Form Submission
function initializeAddProductForm() {
    $('#addProductForm').on('submit', function (e) {
        e.preventDefault();
        var submitBtn = $(this).find('button[type="submit"]');

        setButtonLoading(submitBtn, true, 'جارٍ الحفظ...', 'حفظ المنتج');

        $.ajax({
            url: '/Products/Create',
            type: 'POST',
            data: $(this).serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    $('#addProductModal').modal('hide');
                    $('#addProductForm')[0].reset();
                    // تعيين الوصف تلقائياً بعد مسح النموذج
                    $('#addProductDescription').val('-');
                    showSuccess('تم الإضافة', 'تم إضافة المنتج بنجاح');
                    setTimeout(function () { location.reload(); }, 1200);
                } else {
                    showError('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء إضافة المنتج';
                showError('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حفظ المنتج');
            }
        });
    });
}

// Edit Product
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

    }).fail(() => showError('خطأ', 'حدث خطأ أثناء تحميل بيانات المنتج'));
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
    }).fail(() => showError('خطأ', 'حدث خطأ أثناء تحميل بيانات المنتج'));
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
                    showSuccess('تم التحديث', 'تم تحديث المنتج بنجاح');
                    setTimeout(function () { location.reload(); }, 1200);
                } else {
                    showError('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء تحديث المنتج';
                showError('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حفظ التغييرات');
            }
        });
    });
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
                    showSuccess('تم التحديث', 'تم تحديث الكمية بنجاح');
                    setTimeout(function () { location.reload(); }, 1200);
                } else {
                    showError('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء تحديث الكمية';
                showError('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حفظ الكمية الجديدة');
            }
        });
    });
}

// Delete Product
function deleteProduct(id) {
    showDangerConfirm(
        'تأكيد الحذف',
        'هل أنت متأكد من حذف هذا المنتج؟ لا يمكن التراجع عن هذا الإجراء.',
        'حذف',
        'إلغاء'
    ).then((result) => {
        if (result.isConfirmed) {
            // Show loading
            showLoading('جاري الحذف...', 'يرجى الانتظار');

            // Submit delete form
            $('#deleteProductForm').attr('action', `/Products/Delete/${id}`);
            $('#deleteProductForm').submit();
        }
    });
}

// Delete Product Form Submission
function initializeDeleteProductForm() {
    $('#deleteProductForm').on('submit', function (e) {
        e.preventDefault();
        var submitBtn = $(this).find('button[type="submit"]');

        setButtonLoading(submitBtn, true, 'جارٍ الحذف...', 'حذف');

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: $(this).serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                $('#deleteProductModal').modal('hide');
                if (result.success) {
                    closeSwal();
                    showSuccess('تم الحذف', 'تم حذف المنتج بنجاح');
                    setTimeout(function () { location.reload(); }, 1200);
                } else {
                    showError('خطأ', result.message);
                }
            },
            error: function (xhr) {
                var errorMessage = xhr.responseJSON?.message || 'حدث خطأ أثناء حذف المنتج';
                showError('خطأ', errorMessage);
            },
            complete: function () {
                setButtonLoading(submitBtn, false, '', 'حذف');
            }
        });
    });
}

// View Product
function viewProduct(id) {
    window.location.href = `/Products/Details/${id}`;
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

// Function to search product by barcode
function searchByBarcode() {
    var barcode = $('#searchBarcodeInput').val().trim();
    if (!barcode) {
        showWarning('تنبيه', 'يرجى إدخال باركود للبحث');
        return;
    }

    $.get('/Products/SearchByBarcode', { barcode: barcode }, function (product) {
        if (product) {
            SwalConfig.fire({
                title: 'تم العثور على المنتج',
                html: `
                    <div class="text-right">
                        <p class="mb-1"><strong>الاسم:</strong> ${product.name}</p>
                        <p class="mb-1"><strong>الكمية:</strong> ${product.quantity}</p>
                        <p class="mb-1"><strong>سعر البيع:</strong> ${product.salePrice} د.ك</p>
                        <p class="mb-1"><strong>الفئة:</strong> ${product.categoryName || 'غير محدد'}</p>
                    </div>
                `,
                icon: 'info',
                showCancelButton: true,
                confirmButtonText: 'تعديل المنتج',
                cancelButtonText: 'إلغاء'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = `/Products/Edit/${product.id}`;
                }
            });
        } else {
            showWarning('تنبيه', 'لم يتم العثور على منتج بهذا الباركود');
        }
    }).fail(function () {
        showError('خطأ', 'حدث خطأ أثناء البحث');
    });
}

// Print all barcodes from current page only
function printAllBarcodesFromCurrentPage() {
    var products = [];
    $('#productsTable tbody tr').each(function () {
        var $row = $(this);
        var barcode = $row.find('td:eq(5) .badge').text().trim();
        var name = $row.find('td:eq(0)').text().trim();
        var priceText = $row.find('td:eq(4)').text().trim();
        var price = parseFloat(priceText.replace(/[^\d.-]/g, ''));

        if (barcode && name && !isNaN(price)) {
            products.push({
                barcode: barcode,
                name: name,
                salePrice: price
            });
        }
    });

    if (products.length > 0) {
        // Use the existing printAllBarcodesXP360B function
        printAllBarcodesXP360B();
    } else {
        showWarning('تنبيه', 'لا توجد منتجات بباركود في الصفحة الحالية');
    }
}

// Initialize all event handlers
function initializeAllProductsPage() {
    // Page size change handler
    $('#pageSizeSelect').on('change', function () {
        var newPageSize = $(this).val();
        var currentPage = window.currentPage || 1;
        window.location.href = `/Products?page=${currentPage}&pageSize=${newPageSize}`;
    });

    // تعيين الوصف تلقائياً عند فتح نافذة إضافة المنتج
    $('#addProductModal').on('show.bs.modal', function () {
        $('#addProductDescription').val('-');
    });

    // Generate barcode button click handlers
    $('#generateBarcodeBtn').on('click', function () {
        generateBarcode('#addProductBarcode');
    });

    $('#editGenerateBarcodeBtn').on('click', function () {
        generateBarcode('#editProductBarcode');
    });

    // Barcode validation for add product form
    $('#addProductBarcode').on('blur', function () {
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

    // Search by barcode functionality
    $('#searchBarcodeInput').on('keypress', function (e) {
        if (e.which === 13) { // Enter key
            searchByBarcode();
        }
    });

    $('#searchBarcodeBtn').on('click', function () {
        searchByBarcode();
    });

    // Initialize form handlers
    initializeAddProductForm();
    initializeEditProductForm();
    initializeEditQuantityForm();
    initializeDeleteProductForm();
}

// Document Ready
$(document).ready(function () {
    initializeAllProductsPage();
});
