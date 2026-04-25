// Update total amount (بعد الخصم)
function updateTotalAmount() {
    const subtotal = selectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = parseFloat($('#discountAmount').val()) || 0;
    const totalAfterDiscount = subtotal - discountAmount;

    // التقريب لعلامتين عشريتين عند العرض وفي الحقول المخفية
    $('#totalAmount').text(totalAfterDiscount.toFixed(2));
    $('#totalAmountInput').val(totalAfterDiscount.toFixed(2));

    updateAmountAfterDiscount();
}

// دوال الخصم
function updateDiscountAmount() {
    const percentage = parseFloat($('#discountPercentage').val()) || 0;
    const subtotal = selectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = (subtotal * percentage) / 100;

    // تقريب قيمة الخصم الناتج عن النسبة
    $('#discountAmount').val(discountAmount.toFixed(2));

    updateAmountAfterDiscount();
    updateTotalAmount();
}

function updateDiscountPercentage() {
    const discountAmount = parseFloat($('#discountAmount').val()) || 0;
    const subtotal = selectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);

    if (subtotal > 0) {
        const percentage = (discountAmount / subtotal) * 100;
        // تقريب النسبة المئوية لعلامتين
        $('#discountPercentage').val(percentage.toFixed(2));
    }

    updateAmountAfterDiscount();
    updateTotalAmount();
}

function updateAmountAfterDiscount() {
    const subtotal = selectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = parseFloat($('#discountAmount').val()) || 0;
    const amountAfterDiscount = subtotal - discountAmount;

    $('#amountAfterDiscount').val(amountAfterDiscount.toFixed(2));
}

// --- دوال الخصم للتعديل (Edit Mode) ---

function updateEditDiscountAmount() {
    const percentage = parseFloat($('#editDiscountPercentage').val()) || 0;
    const subtotal = editSelectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = (subtotal * percentage) / 100;

    $('#editDiscountAmount').val(discountAmount.toFixed(2));
    updateEditTotalAmount();
}

function updateEditDiscountPercentage() {
    const discountAmount = parseFloat($('#editDiscountAmount').val()) || 0;
    const subtotal = editSelectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);

    if (subtotal > 0) {
        const percentage = (discountAmount / subtotal) * 100;
        $('#editDiscountPercentage').val(percentage.toFixed(2));
    }

    updateEditTotalAmount();
}

function updateEditAmountAfterDiscount() {
    const subtotal = editSelectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = parseFloat($('#editDiscountAmount').val()) || 0;
    const amountAfterDiscount = subtotal - discountAmount;

    $('#editAmountAfterDiscount').val(amountAfterDiscount.toFixed(2));
}

function updateEditTotalAmount() {
    const subtotal = editSelectedProducts.reduce((sum, product) => sum + (product.quantity * product.customSalePrice), 0);
    const discountAmount = parseFloat($('#editDiscountAmount').val()) || 0;
    const totalAfterDiscount = subtotal - discountAmount;

    $('#editTotalAmount').text(totalAfterDiscount.toFixed(2));
    $('#editSaleTotalAmount').val(totalAfterDiscount.toFixed(2));

    updateEditAmountAfterDiscount();
}