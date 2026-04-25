// ===== دوال حساب التقسيط =====

// دالة حساب ملخص التقسيط المُحدثة
function calculateInstallmentSummary() {
    const totalAmount = parseFloat($('#totalAmount').val()) || 0;
    const downPayment = parseFloat($('#downPayment').val()) || 0;
    const numberOfMonths = parseInt($('#numberOfMonths').val()) || 0;
    const interestRate = parseFloat($('#interestRate').val()) || 0;

    // التحقق من صحة البيانات
    if (totalAmount <= 0 || numberOfMonths <= 0) {
        resetCalculationFields();
        return;
    }

    // حساب المبلغ المتبقي
    const remainingAmount = Math.max(0, totalAmount - downPayment);

    // حساب مبلغ الفائدة
    const interestAmount = remainingAmount * (interestRate / 100) * (numberOfMonths / 12);

    // حساب المجموع مع الفائدة
    const totalWithInterest = totalAmount + interestAmount;

    // حساب الدفعة الشهرية
    const monthlyPaymentWithInterest = numberOfMonths > 0 ? (remainingAmount + interestAmount) / numberOfMonths : 0;

    // تحديث العرض الارقام
    $('#remainingAmount').text(Math.round(remainingAmount).toLocaleString());
    $('#interestAmount').text(Math.round(interestAmount).toLocaleString());
    $('#totalWithInterest').text(Math.round(totalWithInterest).toLocaleString());
    $('#calculatedMonthlyPayment').text(Math.round(monthlyPaymentWithInterest).toLocaleString());

    // تحديث الحقول المخفية والقيم
    $('#monthlyPayment').val(Math.round(monthlyPaymentWithInterest));
}

// دالة حساب ملخص التقسيط للتعديل المُحدثة
function calculateEditInstallmentSummary() {
    const totalAmount = parseFloat($('#editInstallmentTotalAmount').val()) || 0;
    const downPayment = parseFloat($('#editInstallmentDownPayment').val()) || 0;
    const numberOfMonths = parseInt($('#editInstallmentNumberOfMonths').val()) || 0;
    const interestRate = parseFloat($('#editInstallmentInterestRate').val()) || 0;

    // التحقق من صحة البيانات
    if (totalAmount <= 0 || numberOfMonths <= 0) {
        resetEditCalculationFields();
        return;
    }

    // حساب المبلغ المتبقي
    const remainingAmount = Math.max(0, totalAmount - downPayment);

    // حساب مبلغ الفائدة (بدون تقريب في البداية للدقة)
    const interestAmount = remainingAmount * (interestRate / 100) * (numberOfMonths / 12);

    // حساب المجموع مع الفائدة
    const totalWithInterest = totalAmount + interestAmount;

    // حساب الدفعة الشهرية
    const monthlyPaymentWithInterest = numberOfMonths > 0 ? (remainingAmount + interestAmount) / numberOfMonths : 0;

    // تحديث العرض مع التقريب
    $('#editRemainingAmount').text(Math.round(remainingAmount).toLocaleString());
    $('#editInterestAmount').text(Math.round(interestAmount).toLocaleString());
    $('#editTotalWithInterest').text(Math.round(totalWithInterest).toLocaleString());
    $('#editCalculatedMonthlyPayment').text(Math.round(monthlyPaymentWithInterest).toLocaleString());

    // تحديث حقل الدفعة الشهرية
    $('#editInstallmentMonthlyPayment').val(Math.round(monthlyPaymentWithInterest));
}

// دالة تصفير حقول الحسابات
function resetCalculationFields() {
    $('#remainingAmount').text('0');
    $('#interestAmount').text('0');
    $('#totalWithInterest').text('0');
    $('#calculatedMonthlyPayment').text('0');
    $('#monthlyPayment').val(0);
}

// دالة تصفير حقول الحسابات للتعديل
function resetEditCalculationFields() {
    $('#editRemainingAmount').text('0');
    $('#editInterestAmount').text('0');
    $('#editTotalWithInterest').text('0');
    $('#editCalculatedMonthlyPayment').text('0');
    $('#editInstallmentMonthlyPayment').val(0);
}
