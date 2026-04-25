// ============================================
// نسخة محسنة من site.js للاستخدام المحلي فقط
// ============================================

document.addEventListener('DOMContentLoaded', function () {
    initializeBasicFeatures();
    initializeLocalOptimizations();
});

// ===== الميزات الأساسية =====
function initializeBasicFeatures() {
    setupSidebar();
    setupAlerts();
    setupFormValidation();
    setupTableEnhancements();
    setupConfirmDialogs();
}

// إعداد القائمة الجانبية
function setupSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        // إغلاق القائمة عند النقر خارجها (للموبايل فقط)
        document.addEventListener('click', function (event) {
            if (window.innerWidth <= 768) {
                if (!sidebar.contains(event.target) && !sidebarToggle.contains(event.target)) {
                    sidebar.classList.remove('show');
                }
            }
        });
    }
}

// إعداد التنبيهات
function setupAlerts() {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        // إخفاء التنبيهات تلقائياً بعد 5 ثوانِ
        setTimeout(() => {
            alert.style.transition = 'opacity 0.3s ease';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });
}

// إعداد التحقق من النماذج
function setupFormValidation() {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!validateForm(this)) {
                event.preventDefault();
                showValidationMessage('يرجى ملء جميع الحقول المطلوبة');
            }
        });
    });
}

// التحقق من النموذج
function validateForm(form) {
    const requiredFields = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    requiredFields.forEach(field => {
        const value = field.value.trim();

        if (!value) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');

            // التحقق من البريد الإلكتروني
            if (field.type === 'email') {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(value)) {
                    field.classList.add('is-invalid');
                    isValid = false;
                }
            }

            // التحقق من رقم الهاتف
            if (field.type === 'tel') {
                const phoneRegex = /^[0-9+\-\s()]+$/;
                if (!phoneRegex.test(value)) {
                    field.classList.add('is-invalid');
                    isValid = false;
                }
            }
        }
    });

    return isValid;
}

// عرض رسائل التحقق
function showValidationMessage(message) {
    // استخدام helper موحد إذا كان متاحاً
    if (typeof showWarning !== 'undefined') {
        showWarning('تنبيه', message);
    } else if (typeof Swal !== 'undefined') {
        Swal.fire({
            icon: 'warning',
            title: 'تنبيه',
            text: message,
            confirmButtonText: 'حسناً',
            confirmButtonColor: '#ffc107'
        });
    } else {
        // استخدام التنبيه العادي كبديل
        const alertDiv = document.createElement('div');
        alertDiv.className = 'alert alert-danger alert-dismissible fade show';
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        // إضافة التنبيه في أعلى الصفحة
        const container = document.querySelector('.container, .container-fluid, main');
        if (container) {
            container.insertBefore(alertDiv, container.firstChild);

            // إزالة التنبيه بعد 3 ثوانِ
            setTimeout(() => {
                alertDiv.style.opacity = '0';
                setTimeout(() => alertDiv.remove(), 300);
            }, 3000);
        }
    }
}

// تحسين الجداول
function setupTableEnhancements() {
    const tables = document.querySelectorAll('.table');
    tables.forEach(table => {
        // إضافة hover effect للصفوف
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach(row => {
            row.addEventListener('mouseenter', function () {
                this.style.backgroundColor = '#f8f9fa';
            });
            row.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '';
            });
        });

        // إضافة فرز بسيط للأعمدة
        setupTableSorting(table);
    });
}

// فرز الجداول
function setupTableSorting(table) {
    const headers = table.querySelectorAll('th[data-sortable]');
    headers.forEach((header, index) => {
        header.style.cursor = 'pointer';
        header.innerHTML += ' <i class="fas fa-sort text-muted"></i>';

        header.addEventListener('click', function () {
            sortTable(table, index);
        });
    });
}

function sortTable(table, columnIndex) {
    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr'));

    // تحديد اتجاه الفرز
    const isAscending = table.dataset.sortDirection !== 'asc';
    table.dataset.sortDirection = isAscending ? 'asc' : 'desc';

    // فرز الصفوف
    rows.sort((a, b) => {
        const aValue = a.cells[columnIndex].textContent.trim();
        const bValue = b.cells[columnIndex].textContent.trim();

        // فرز رقمي إذا كان القيم أرقام
        if (!isNaN(aValue) && !isNaN(bValue)) {
            return isAscending ?
                parseFloat(aValue) - parseFloat(bValue) :
                parseFloat(bValue) - parseFloat(aValue);
        }

        // فرز نصي مع دعم العربية
        return isAscending ?
            aValue.localeCompare(bValue, 'ar') :
            bValue.localeCompare(aValue, 'ar');
    });

    // إعادة ترتيب الصفوف
    rows.forEach(row => tbody.appendChild(row));

    // تحديث أيقونات الفرز
    const allHeaders = table.querySelectorAll('th[data-sortable] i');
    allHeaders.forEach(icon => {
        icon.className = 'fas fa-sort text-muted';
    });

    const currentIcon = table.querySelectorAll('th[data-sortable]')[columnIndex].querySelector('i');
    currentIcon.className = isAscending ?
        'fas fa-sort-up text-primary' :
        'fas fa-sort-down text-primary';
}

// إعداد رسائل التأكيد
function setupConfirmDialogs() {
    const deleteButtons = document.querySelectorAll('[data-action="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            const message = this.dataset.message || 'هل أنت متأكد من حذف هذا العنصر؟';
            const title = this.dataset.title || 'تأكيد الحذف';

            confirmDelete(message, title).then(confirmed => {
                if (confirmed) {
                    // تنفيذ الحذف
                    if (this.form) {
                        this.form.submit();
                    } else if (this.href) {
                        window.location.href = this.href;
                    }
                }
            });
        });
    });
}

// ===== التحسينات للاستخدام المحلي =====
function initializeLocalOptimizations() {
    setupLocalSearch();
    setupPrintOptimization();
    setupMobileOptimization();
    setupKeyboardShortcuts();
    setupLocalStorage();
}

// البحث المحلي
function setupLocalSearch() {
    const searchInputs = document.querySelectorAll('input[type="search"], .search-input');
    searchInputs.forEach(input => {
        input.addEventListener('input', debounce(function () {
            performLocalSearch(this.value, this.dataset.target);
        }, 300));
    });
}

function performLocalSearch(searchTerm, targetSelector = 'tbody tr') {
    const term = searchTerm.toLowerCase().trim();
    const targets = document.querySelectorAll(targetSelector);

    targets.forEach(target => {
        const text = target.textContent.toLowerCase();
        const shouldShow = !term || text.includes(term);
        target.style.display = shouldShow ? '' : 'none';
    });

    // عرض رسالة إذا لم توجد نتائج
    showSearchResults(targets, term);
}

function showSearchResults(targets, searchTerm) {
    const visibleCount = Array.from(targets).filter(t => t.style.display !== 'none').length;

    // إزالة رسائل البحث السابقة
    document.querySelectorAll('.search-result-message').forEach(msg => msg.remove());

    if (searchTerm && visibleCount === 0) {
        const message = document.createElement('div');
        message.className = 'alert alert-info search-result-message';
        message.textContent = 'لم يتم العثور على نتائج تطابق البحث';

        const container = targets[0]?.closest('table')?.parentElement || document.querySelector('main');
        if (container) {
            container.insertBefore(message, container.firstChild);
        }
    }
}

// تحسين الطباعة
function setupPrintOptimization() {
    // إضافة أنماط الطباعة
    const printStyles = document.createElement('style');
    printStyles.textContent = `
        @media print {
            .no-print, .btn, .sidebar, .alert { display: none !important; }
            .main-content { margin: 0 !important; padding: 0 !important; }
            .table { font-size: 10px !important; }
            .card { box-shadow: none !important; border: 1px solid #000 !important; }
            body { font-size: 12px !important; }
            h1, h2, h3 { font-size: 14px !important; margin: 5px 0 !important; }
        }
    `;
    document.head.appendChild(printStyles);

    // إضافة أزرار الطباعة
    const printButtons = document.querySelectorAll('[data-action="print"]');
    printButtons.forEach(button => {
        button.addEventListener('click', function () {
            const target = document.querySelector(this.dataset.target || 'body');
            printElement(target);
        });
    });
}

function printElement(element) {
    const printWindow = window.open('', '_blank');
    const styles = Array.from(document.querySelectorAll('style, link[rel="stylesheet"]'))
        .map(s => s.outerHTML || `<link rel="stylesheet" href="${s.href}">`)
        .join('');

    printWindow.document.write(`
        <!DOCTYPE html>
        <html dir="rtl" lang="ar">
        <head>
            <meta charset="utf-8">
            <title>طباعة</title>
            ${styles}
        </head>
        <body class="print-body">
            ${element.outerHTML}
        </body>
        </html>
    `);

    printWindow.document.close();
    printWindow.focus();

    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 1000);
}

// تحسين الموبايل
function setupMobileOptimization() {
    if (window.innerWidth <= 768) {
        // تحسين اللمس
        const interactiveElements = document.querySelectorAll('button, a, input, select');
        interactiveElements.forEach(element => {
            element.style.touchAction = 'manipulation';
            element.style.minHeight = '44px'; // حد أدنى لسهولة اللمس
        });

        // تحسين الجداول للموبايل
        const tables = document.querySelectorAll('.table');
        tables.forEach(table => {
            if (!table.closest('.table-responsive')) {
                const wrapper = document.createElement('div');
                wrapper.className = 'table-responsive';
                table.parentNode.insertBefore(wrapper, table);
                wrapper.appendChild(table);
            }
        });
    }
}

// اختصارات لوحة المفاتيح
function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function (event) {
        // Ctrl/Cmd + S للحفظ
        if ((event.ctrlKey || event.metaKey) && event.key === 's') {
            event.preventDefault();
            const saveButton = document.querySelector('[type="submit"], .btn-save');
            if (saveButton) {
                saveButton.click();
            }
        }

        // Escape لإغلاق المودال أو الإلغاء
        if (event.key === 'Escape') {
            const modal = document.querySelector('.modal.show');
            if (modal) {
                const closeButton = modal.querySelector('[data-bs-dismiss="modal"]');
                if (closeButton) closeButton.click();
            }
        }

        // F3 للبحث
        if (event.key === 'F3') {
            event.preventDefault();
            const searchInput = document.querySelector('input[type="search"], .search-input');
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }
        }
    });
}

// التخزين المحلي
function setupLocalStorage() {
    // حفظ حالة القائمة الجانبية
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        const sidebarState = localStorage.getItem('sidebarCollapsed');
        if (sidebarState === 'true') {
            sidebar.classList.add('collapsed');
        }

        // حفظ الحالة عند التغيير
        const toggleButton = document.getElementById('sidebarToggle');
        if (toggleButton) {
            toggleButton.addEventListener('click', function () {
                const isCollapsed = sidebar.classList.contains('collapsed');
                localStorage.setItem('sidebarCollapsed', isCollapsed);
            });
        }
    }

    // حفظ تفضيلات الجدول
    saveTablePreferences();
}

function saveTablePreferences() {
    const tables = document.querySelectorAll('.table[data-save-state]');
    tables.forEach(table => {
        const tableId = table.id || table.dataset.saveState;
        if (!tableId) return;

        // استعادة الفرز المحفوظ
        const savedSort = localStorage.getItem(`table_sort_${tableId}`);
        if (savedSort) {
            const { column, direction } = JSON.parse(savedSort);
            table.dataset.sortDirection = direction === 'asc' ? 'desc' : 'asc';
            sortTable(table, column);
        }

        // حفظ الفرز عند التغيير
        table.addEventListener('click', function (event) {
            const header = event.target.closest('th[data-sortable]');
            if (header) {
                const columnIndex = Array.from(header.parentElement.children).indexOf(header);
                const direction = table.dataset.sortDirection;
                localStorage.setItem(`table_sort_${tableId}`, JSON.stringify({
                    column: columnIndex,
                    direction: direction
                }));
            }
        });
    });
}

// ===== وظائف مساعدة =====

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func.apply(this, args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// إظهار مؤشر التحميل
function showLoadingIndicator(message = 'جاري التحميل...') {
    // إزالة المؤشرات السابقة
    document.querySelectorAll('.loading-overlay').forEach(overlay => overlay.remove());

    const loadingDiv = document.createElement('div');
    loadingDiv.className = 'loading-overlay';
    loadingDiv.innerHTML = `
        <div class="d-flex flex-column align-items-center justify-content-center h-100">
            <div class="spinner-border text-primary mb-3" role="status">
                <span class="visually-hidden">${message}</span>
            </div>
            <p class="text-center">${message}</p>
        </div>
    `;

    loadingDiv.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(255, 255, 255, 0.9);
        z-index: 9999;
        display: flex;
    `;

    document.body.appendChild(loadingDiv);

    return {
        hide: function () {
            loadingDiv.remove();
        }
    };
}

// تأكيد الحذف المحسن
function confirmDelete(message = 'هل أنت متأكد من حذف هذا العنصر؟', title = 'تأكيد الحذف') {
    return new Promise((resolve) => {
        // استخدام helper موحد إذا كان متاحاً
        if (typeof showDangerConfirm !== 'undefined') {
            showDangerConfirm(title, message, 'حذف', 'إلغاء').then((result) => {
                resolve(result.isConfirmed);
            });
        } else if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: title,
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'حذف',
                cancelButtonText: 'إلغاء'
            }).then((result) => {
                resolve(result.isConfirmed);
            });
        } else {
            // استخدام المودال العادي كبديل
            const modal = document.createElement('div');
            modal.className = 'modal fade';
            modal.innerHTML = `
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p>${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إلغاء</button>
                            <button type="button" class="btn btn-danger" data-confirm="true">حذف</button>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);

            const bootstrapModal = new bootstrap.Modal(modal);
            bootstrapModal.show();

            modal.addEventListener('click', function (event) {
                if (event.target.dataset.confirm) {
                    resolve(true);
                    bootstrapModal.hide();
                } else if (event.target.closest('[data-bs-dismiss="modal"]')) {
                    resolve(false);
                }
            });

            modal.addEventListener('hidden.bs.modal', function () {
                modal.remove();
            });
        }
    });
}

// وظائف عامة متاحة للاستخدام العالمي
window.showLoadingIndicator = showLoadingIndicator;
window.confirmDelete = confirmDelete;
window.validateForm = validateForm;
window.performLocalSearch = performLocalSearch;

// ===== PWA (تثبيت التطبيق) =====
let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
    // منع المتصفح من إظهار التنبيه التلقائي
    e.preventDefault();
    // تخزين الحدث ليتم استدعاؤه لاحقاً
    deferredPrompt = e;

    // إظهار أزرار التثبيت في الواجهة إذا كانت موجودة
    const installBtns = document.querySelectorAll('.pwa-install-btn');
    installBtns.forEach(btn => btn.classList.remove('hidden'));
});

async function installPWA() {
    if (!deferredPrompt) {
        // إذا لم يكن الحدث متاحاً، قد يكون التطبيق مثبتاً بالفعل أو المتصفح لا يدعمه
        Swal.fire({
            title: 'تنبيه',
            text: 'التطبيق مثبت بالفعل أو أن متصفحك لا يدعم التثبيت التلقائي. يمكنك إضافته يدوياً من إعدادات المتصفح.',
            icon: 'info',
            confirmButtonText: 'حسناً'
        });
        return;
    }

    // إظهار نافذة التثبيت الخاصة بالمتصفح
    deferredPrompt.prompt();

    // الانتظار لمعرفة قرار المستخدم
    const { outcome } = await deferredPrompt.userChoice;
    console.log(`User response to the install prompt: ${outcome}`);

    // مسح الحدث المخزن
    deferredPrompt = null;
}

window.installPWA = installPWA;
window.downloadShortcut = installPWA; // إعادة توجيه الوظيفة القديمة للجديدة لضمان التوافق
