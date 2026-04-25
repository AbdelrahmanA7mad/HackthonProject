// Barcode Scanner Functions for Sales

let barcodeBuffer = '';
let barcodeTimeout = null;

// Initialize barcode scanner handlers when DOM is ready
$(document).ready(function () {
    // Barcode scanner functionality - Auto process on Enter key
    $('#barcodeScanner').on('keydown', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            e.preventDefault();
            const barcode = $(this).val().trim();
            if (barcode.length > 0) {
                processBarcode(barcode);
            }
        }
    });

    // Alternative: Auto-detect barcode scanner (scans fast and ends with Enter)
    $('#barcodeScanner').on('keypress', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            e.preventDefault();
            const barcode = $(this).val().trim();
            if (barcode.length > 0) {
                processBarcode(barcode);
            }
        }
    });
    
    // ركّز على خانة الباركود أول ما الصفحة تفتح
    $('#barcodeScanner').focus();
});

// Process scanned barcode
function processBarcode(barcode) {
    const pathname = window.location.pathname || '';
    const isQuickSale = pathname.indexOf('/QuickSale') !== -1;
    
    // Use the correct products array based on page
    const productsArray = isQuickSale ? (typeof qsAllProducts !== 'undefined' ? qsAllProducts : []) : (typeof allProducts !== 'undefined' ? allProducts : []);
    
    // Find product by barcode (case-insensitive)
    const product = productsArray.find(p => 
        (p.barcode || '').toLowerCase() === barcode.toLowerCase()
    );

    if (product) {
        const quantity = product.quantity ?? product.Quantity ?? 0;
        
        if (quantity > 0) {
            // Add product to sale automatically
            if (isQuickSale) {
                // Use QuickSale functions
                if (typeof qsAddToCart === 'function') {
                    qsAddToCart(
                        product.id || product.Id,
                        product.name || product.Name || '',
                        product.price || product.Price || 0,
                        product.purchasePrice || product.PurchasePrice || 0,
                        quantity,
                        product.barcode || product.Barcode || ''
                    );
                }
            } else {
                // Use regular sale functions
                if (typeof addProductToSale === 'function') {
                    addProductToSale(product.id, product.name, product.price, product.purchasePrice, product.quantity, product.barcode);
                }
            }

            // Show success message
            if (isQuickSale) {
                if (typeof handleBarcodeScanSuccess === 'function') {
                    handleBarcodeScanSuccess(product.name || product.Name);
                } else if (typeof showToast === 'function') {
                    showToast(`تم إضافة المنتج "${product.name || product.Name}"`, 'success');
                }
            } else {
                if (typeof showToast === 'function') {
                    showToast(`تم إضافة المنتج "${product.name}"`, 'success');
                }
            }

            // Play success sound (if supported)
            playSuccessSound();

            // Clear barcode scanner
            $('#barcodeScanner').val('').focus();
        } else {
            const productName = product.name || product.Name || 'المنتج';
            if (isQuickSale) {
                if (typeof handleBarcodeScanError === 'function') {
                    handleBarcodeScanError(`المنتج "${productName}" غير متوفر في المخزون`);
                } else if (typeof showToast === 'function') {
                    showToast(`المنتج "${productName}" غير متوفر في المخزون`, 'warning');
                }
            } else {
                if (typeof showToast === 'function') {
                    showToast(`المنتج "${productName}" غير متوفر في المخزون`, 'warning');
                }
            }

            // Play warning sound (if supported)
            playWarningSound();

            $('#barcodeScanner').val('').focus();
        }
    } else {
        if (isQuickSale) {
            if (typeof handleBarcodeScanError === 'function') {
                handleBarcodeScanError(`لم يتم العثور على منتج بالباركود: ${barcode}`);
            } else if (typeof showToast === 'function') {
                showToast(`لم يتم العثور على منتج بالباركود: ${barcode}`, 'error');
            }
        } else {
            if (typeof showToast === 'function') {
                showToast(`لم يتم العثور على منتج بالباركود: ${barcode}`, 'danger');
            }
        }

        // Play error sound (if supported)
        playWarningSound();

        $('#barcodeScanner').val('').focus();
    }
}

// Play success sound
function playSuccessSound() {
    try {
        // Create audio context for beep sound
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        oscillator.type = 'sine';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.1);
    } catch (e) {
        // Fallback: no sound if audio context not supported
        console.log('Audio not supported');
    }
}

// Play warning sound
function playWarningSound() {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.frequency.setValueAtTime(400, audioContext.currentTime);
        oscillator.type = 'sine';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.2);
    } catch (e) {
        console.log('Audio not supported');
    }
}

// Clear barcode scanner
function clearBarcodeScanner() {
    $('#barcodeScanner').val('').focus();
}

// Edit barcode scanner functionality - Auto process on Enter key
$(document).ready(function () {
    $('#editBarcodeScanner').on('keydown', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            e.preventDefault();
            const barcode = $(this).val().trim();
            if (barcode.length > 0) {
                processEditBarcode(barcode);
            }
        }
    });
});

// Process edit barcode
function processEditBarcode(barcode) {
    const pathname = window.location.pathname || '';
    const isQuickSale = pathname.indexOf('/QuickSale') !== -1;
    
    // Use the correct products array based on page
    const productsArray = isQuickSale ? (typeof qsAllProducts !== 'undefined' ? qsAllProducts : []) : (typeof allProducts !== 'undefined' ? allProducts : []);
    
    // Find product by barcode (case-insensitive)
    const product = productsArray.find(p => 
        (p.barcode || '').toLowerCase() === barcode.toLowerCase()
    );

    if (product) {
        const quantity = product.quantity ?? product.Quantity ?? 0;
        
        if (quantity > 0) {
            if (typeof addEditProductToSale === 'function') {
                addEditProductToSale(
                    product.id || product.Id,
                    product.name || product.Name || '',
                    product.price || product.Price || 0,
                    product.purchasePrice || product.PurchasePrice || 0,
                    quantity,
                    product.barcode || product.Barcode || ''
                );
            }
            
            const productName = product.name || product.Name || '';
            if (isQuickSale) {
                if (typeof handleBarcodeScanSuccess === 'function') {
                    handleBarcodeScanSuccess(productName);
                } else if (typeof showToast === 'function') {
                    showToast(`تم إضافة المنتج "${productName}"`, 'success');
                }
            } else {
                if (typeof showToast === 'function') {
                    showToast(`تم إضافة المنتج "${productName}"`, 'success');
                }
            }
            $('#editBarcodeScanner').val('').focus();
        } else {
            const productName = product.name || product.Name || 'المنتج';
            if (isQuickSale) {
                if (typeof handleBarcodeScanError === 'function') {
                    handleBarcodeScanError(`المنتج "${productName}" غير متوفر في المخزون`);
                } else if (typeof showToast === 'function') {
                    showToast(`المنتج "${productName}" غير متوفر في المخزون`, 'warning');
                }
            } else {
                if (typeof showToast === 'function') {
                    showToast(`المنتج "${productName}" غير متوفر في المخزون`, 'warning');
                }
            }
            $('#editBarcodeScanner').val('').focus();
        }
    } else {
        if (isQuickSale) {
            if (typeof handleBarcodeScanError === 'function') {
                handleBarcodeScanError(`لم يتم العثور على منتج بالباركود: ${barcode}`);
            } else if (typeof showToast === 'function') {
                showToast(`لم يتم العثور على منتج بالباركود: ${barcode}`, 'error');
            }
        } else {
            if (typeof showToast === 'function') {
                showToast(`لم يتم العثور على منتج بالباركود: ${barcode}`, 'danger');
            }
        }
        $('#editBarcodeScanner').val('').focus();
    }
}

// Clear edit barcode scanner
function clearEditBarcodeScanner() {
    $('#editBarcodeScanner').val('').focus();
}
