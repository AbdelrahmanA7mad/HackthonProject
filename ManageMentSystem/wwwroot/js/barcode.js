// XPRINTER XP-360B Optimized Functions - Modernized
// Paper width: 20-82mm, Resolution: 203DPI
// Designed for thermal label printers with preview and accessibility helpers

// ==================== CONFIGURATION ====================
const LABEL_CONFIG = {
    labelWidth: 80,          // عرض اللاصقة بالمليمتر
    labelHeight: 25,         // ارتفاع اللاصقة بالمليمتر
    padding: 1,              // المسافة الداخلية
    productNameFont: 12,     // حجم خط اسم المنتج
    priceFont: 9,            // حجم خط السعر
    barcodeWidth: 9,       // عرض خطوط الباركود
    barcodeHeight: 150,       // ارتفاع الباركود
    maxNameLength: 18,       // أقصى عدد أحرف لاسم المنتج
    currency: (window.APP_CURRENCY_SYMBOL || 'ج.م'),         // العملة
    counterFont: 5,          // حجم خط العداد
    barcodeTextFont: 12      // حجم خط رقم الباركود
};

// إعدادات الباركود العامة
const BARCODE_SETTINGS = {
    format: "CODE128",
    displayValue: false,
    fontSize: 30,
    textMargin: 0,
    background: "#ffffff",
    lineColor: "#000000",
    margin: 0,
    quiet: 2
};

// ==================== CORE / UTILITIES ====================

// Generate unique barcode
function generateBarcode(inputSelector) {
    var timestamp = Date.now().toString().slice(-6);
    var random = Math.floor(100 + Math.random() * 900);
    var barcode = timestamp + random;
    $(inputSelector).val(barcode);
}

// Validate barcode
function validateBarcode(barcode) {
    return barcode && barcode.trim() !== '' && barcode !== '-' && barcode.length >= 3;
}

// Handle print errors
function handlePrintError(error, context) {
    console.error('Print error in ' + context + ':', error);
    alert('حدث خطأ أثناء الطباعة في ' + context + '. يرجى المحاولة مرة أخرى.');
}

// Format product name
function formatProductName(name) {
    return name.length > LABEL_CONFIG.maxNameLength ?
        name.substring(0, LABEL_CONFIG.maxNameLength) + '..' : name;
}

// Format price
function formatPrice(price) {
    return price ? parseFloat(price).toFixed(2) + ' ' + LABEL_CONFIG.currency : 'غير محدد';
}

// Convert an inline SVG node to a PNG Data URL via Canvas (for downloads)
async function svgToPngDataUrl(svgElement, scale = 2) {
    return new Promise((resolve, reject) => {
        try {
            const svgString = new XMLSerializer().serializeToString(svgElement);
            const svgBlob = new Blob([svgString], { type: 'image/svg+xml;charset=utf-8' });
            const url = URL.createObjectURL(svgBlob);
            const image = new Image();
            image.onload = function () {
                try {
                    const canvas = document.createElement('canvas');
                    const width = svgElement.viewBox && svgElement.viewBox.baseVal && svgElement.viewBox.baseVal.width
                        ? svgElement.viewBox.baseVal.width
                        : svgElement.getBoundingClientRect().width || 300;
                    const height = svgElement.viewBox && svgElement.viewBox.baseVal && svgElement.viewBox.baseVal.height
                        ? svgElement.viewBox.baseVal.height
                        : svgElement.getBoundingClientRect().height || 120;
                    canvas.width = Math.max(1, Math.floor(width * scale));
                    canvas.height = Math.max(1, Math.floor(height * scale));
                    const ctx = canvas.getContext('2d');
                    ctx.fillStyle = '#ffffff';
                    ctx.fillRect(0, 0, canvas.width, canvas.height);
                    ctx.drawImage(image, 0, 0, canvas.width, canvas.height);
                    URL.revokeObjectURL(url);
                    resolve(canvas.toDataURL('image/png'));
                } catch (err) {
                    URL.revokeObjectURL(url);
                    reject(err);
                }
            };
            image.onerror = function (e) {
                URL.revokeObjectURL(url);
                reject(e);
            };
            image.src = url;
        } catch (e) {
            reject(e);
        }
    });
}

// ==================== CSS GENERATOR FUNCTION ====================
function generateLabelCSS(isMultiple = false) {
    return `
        * { 
            box-sizing: border-box; 
            margin: 0; 
            padding: 0; 
        }
        body { 
            font-family: "Cairo", "Tajawal", "Arial", "Tahoma", sans-serif; 
            background: white; 
            -webkit-print-color-adjust: exact; 
            color-adjust: exact; 
            margin: 0; 
            padding: 0; 
        }
        .label-container { 
            width: ${LABEL_CONFIG.labelWidth}mm; 
            height: ${LABEL_CONFIG.labelHeight}mm; 
            border: none; 
            padding: ${LABEL_CONFIG.padding}mm; 
            background: white; 
            position: relative; 
            display: flex; 
            flex-direction: column; 
            justify-content: space-between; 
            box-sizing: border-box; 
            page-break-inside: avoid;
            ${isMultiple ? 'page-break-after: always;' : ''}
        }
        ${isMultiple ? '.label-container:last-child { page-break-after: avoid; }' : ''}
        .header-section { 
            text-align: center; 
            flex-shrink: 0; 
        }
        .product-name { 
            font-size: ${LABEL_CONFIG.productNameFont}pt; 
            font-weight: bold; 
            color: #000; 
            line-height: 1.1; 
            margin-bottom: 0.3mm; 
            height: 5mm; 
            overflow: hidden; 
            display: flex; 
            align-items: center; 
            justify-content: center; 
        }
        .price-section { 
            text-align: center; 
            margin-bottom: 0.3mm; 
        }
        .product-price { 
            font-size: ${LABEL_CONFIG.priceFont}pt; 
            font-weight: bold; 
            color: #000; 
            background: #f0f0f0; 
            padding: 0.5mm 2mm; 
            border: none; 
            border-radius: 2mm; 
            display: inline-block; 
        }
        .barcode-section { 
            display: flex; 
            flex-direction: column;
            align-items: center; 
            justify-content: center; 
            text-align: center; 
            margin: 0; 
            height: ${LABEL_CONFIG.labelHeight - 2}mm; 
            overflow: visible; 
            page-break-inside: avoid;
            flex: 1;
        }
        .barcode { 
            max-width: ${LABEL_CONFIG.labelWidth - 2}mm !important; 
            max-height: ${LABEL_CONFIG.barcodeHeight}px !important; 
            width: auto !important; 
            height: auto !important; 
        }
        .barcode-text { 
            font-size: ${LABEL_CONFIG.barcodeTextFont + 2}pt; 
            color: #000; 
            font-family: "Courier New", monospace; 
            font-weight: bold; 
            text-align: center; 
            margin-top: 1mm; 
            line-height: 1; 
            flex-shrink: 0; 
        }
        .footer-info { 
            display: none;
        }
        ${isMultiple ? `
        .counter { 
            position: absolute; 
            top: 1mm; 
            left: 1mm; 
            font-size: ${LABEL_CONFIG.counterFont}pt; 
            background: #000; 
            color: white; 
            padding: 0.5mm 1mm; 
            border-radius: 1mm; 
        }` : ''}
        @media print {
            body { margin: 0; padding: 0; }
            .label-container { border: none; }
            .product-price { border: none; }
            ${isMultiple ? '.counter { background: transparent !important; color: #000 !important; }' : ''}
            @page { 
                margin: 0; 
                size: ${LABEL_CONFIG.labelWidth}mm ${LABEL_CONFIG.labelHeight}mm; 
            }
            * { page-break-inside: avoid !important; }
            .label-container { page-break-inside: avoid !important; }
        }
    `;
}

// ==================== HTML GENERATOR FUNCTION ====================
function generateLabelHTML(product, index = null, total = null) {
    const barcodeId = index !== null ? `barcode${index}` : 'barcode';

    return `
        <div class="label-container">
            ${index !== null ? `<div class="counter">${index + 1}/${total}</div>` : ''}
            <div class="barcode-section">
                <svg class="barcode" id="${barcodeId}"></svg>
                <div class="barcode-text">${product.barcode}</div>
            </div>
        </div>
    `;
}

// ==================== BARCODE SCRIPT BUILDER ====================
function generateBarcodeScript(products, isMultiple = false) {
    let script = 'setTimeout(function() {';

    if (isMultiple) {
        script += 'var errors = [];';

        products.forEach((product, index) => {
            script += `
                try {
                    JsBarcode("#barcode${index}", "${product.barcode}", {
                        format: "${BARCODE_SETTINGS.format}",
                        width: ${LABEL_CONFIG.barcodeWidth},
                        height: ${LABEL_CONFIG.barcodeHeight},
                        displayValue: ${BARCODE_SETTINGS.displayValue},
                        fontSize: ${BARCODE_SETTINGS.fontSize},
                        textMargin: ${BARCODE_SETTINGS.textMargin},
                        background: "${BARCODE_SETTINGS.background}",
                        lineColor: "${BARCODE_SETTINGS.lineColor}",
                        margin: ${BARCODE_SETTINGS.margin},
                        quiet: ${BARCODE_SETTINGS.quiet}
                    });
                } catch(e) {
                    errors.push("${product.name}");
                }
            `;
        });

        script += `
            if(errors.length > 0) {
                alert("خطأ في إنشاء باركود للمنتجات: " + errors.join(", "));
            }
            setTimeout(function() { 
                window.print(); 
                setTimeout(function() { window.close(); }, 2000); 
            }, 500);
        `;
    } else {
        const product = products[0];
        script += `
            try {
                JsBarcode("#barcode", "${product.barcode}", {
                    format: "${BARCODE_SETTINGS.format}",
                    width: ${LABEL_CONFIG.barcodeWidth},
                    height: ${LABEL_CONFIG.barcodeHeight},
                    displayValue: ${BARCODE_SETTINGS.displayValue},
                    fontSize: ${BARCODE_SETTINGS.fontSize},
                    textMargin: ${BARCODE_SETTINGS.textMargin},
                    background: "${BARCODE_SETTINGS.background}",
                    lineColor: "${BARCODE_SETTINGS.lineColor}",
                    margin: ${BARCODE_SETTINGS.margin},
                    quiet: ${BARCODE_SETTINGS.quiet}
                });
                setTimeout(function() { 
                    window.print(); 
                    setTimeout(function() { window.close(); }, 1000); 
                }, 300);
            } catch(e) {
                console.error("Barcode generation failed:", e);
                alert("خطأ في إنشاء الباركود");
            }
        `;
    }

    script += `}, ${isMultiple ? '1000' : '800'});`;
    return script;
}

// ==================== PRINT FLOW ====================
function printBarcodeLabels(products, isMultiple = false) {
    // Validate all products
    for (let product of products) {
        if (!validateBarcode(product.barcode)) {
            alert(`الباركود غير صالح للمنتج: ${product.name}`);
            return;
        }
    }

    const printWindow = window.open('', '_blank');

    let printContent = `
        <!DOCTYPE html>
        <html dir="rtl">
        <head>
            <title>طباعة باركود - XPRINTER</title>
            <meta charset="UTF-8">
            <script src="/lib/jsbarcode/JsBarcode.all.min.js"></script>
            <style>${generateLabelCSS(isMultiple)}</style>
        </head>
        <body>
    `;

    // Generate HTML for all products
    if (isMultiple) {
        products.forEach((product, index) => {
            printContent += generateLabelHTML(product, index, products.length);
        });
    } else {
        printContent += generateLabelHTML(products[0]);
    }

    printContent += `
            <script>${generateBarcodeScript(products, isMultiple)}</script>
        </body>
        </html>
    `;

    printWindow.document.write(printContent);
    printWindow.document.close();
}
// ==================== PUBLIC API (Backward-compatible) ====================

// Print single barcode
function printBarcodeXP360B(barcode, productName, salePrice) {
    const products = [{
        name: productName,
        barcode: barcode,
        salePrice: salePrice
    }];
    printBarcodeLabels(products, false);
}

// Print all barcodes from products table
function printAllBarcodesXP360B() {
    const products = [];

    $('#productsTable tbody tr').each(function () {
        const $row = $(this);
        const barcode = $row.find('td:eq(5) .badge').text().trim();
        const name = $row.find('td:eq(0)').text().trim();
        const salePriceText = $row.find('td:eq(4)').text().trim();
        const salePrice = parseFloat(salePriceText.replace(/[^0-9.]/g, '')) || 0;

        if (barcode && barcode !== '-') {
            products.push({
                name: name,
                barcode: barcode,
                salePrice: salePrice.toString()
            });
        }
    });

    if (products.length === 0) {
        alert('لا توجد منتجات بباركود للطباعة');
        return;
    }

    printBarcodeLabels(products, true);
}

// Open preview from products table (modern UX)
function previewAllBarcodesXP360B() { /* removed */ }

// Open preview for a single product
function previewBarcodeXP360B(barcode, productName, salePrice) { /* removed */ }

// Expose modern API on window for ease of wiring in views
window.BarcodeUI = { printLabels: printBarcodeLabels };

// ==================== USAGE EXAMPLES ====================
/*
// للتحكم في حجم اللاصقة، غير هذه القيم في الأعلى في تعريف LABEL_CONFIG:
// LABEL_CONFIG.labelWidth = 85;        // عرض اللاصقة
// LABEL_CONFIG.labelHeight = 30;       // ارتفاع اللاصقة
// LABEL_CONFIG.barcodeWidth = 3.5;     // عرض خطوط الباركود
// LABEL_CONFIG.barcodeHeight = 100;    // ارتفاع الباركود
// LABEL_CONFIG.productNameFont = 14;   // حجم خط اسم المنتج
// ملاحظة: بما أن LABEL_CONFIG معرف كـ const، يجب تعديل القيم في تعريفه الأصلي (السطر 6)

// استخدام الدوال
printBarcodeXP360B('123456789', 'اسم المنتج', 25.50);
printAllBarcodesXP360B();
*/