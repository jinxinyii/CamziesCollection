function updateClock() {
    const now = new Date();
    const options = {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: 'numeric',
        minute: 'numeric',
        second: 'numeric',
        hour12: true
    };
    const formattedTime = now.toLocaleString('en-US', options);
    document.getElementById('clock').textContent = formattedTime;
}
setInterval(updateClock, 1000);
updateClock();

window.onload = function () {
    document.getElementById("barcodeInput").focus();
};

document.getElementById("barcodeInput").addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        addToCart();
    }
});
let debounceTimeout;
function debounceShowSuggestions() {
    clearTimeout(debounceTimeout);
    debounceTimeout = setTimeout(showSuggestions, 100);
}

document.getElementById("barcodeInput").addEventListener("input", debounceShowSuggestions);


const cart = [];

function addToCart() {  
    const searchInput = document.getElementById("barcodeInput").value.trim().toLowerCase();

    if (searchInput) {
        const product = productData.find(p =>
            p.barcode.toLowerCase() === searchInput || p.name.toLowerCase().includes(searchInput)
        );

        if (!product) {
            alert("Product not found!");
            return;
        }
        const existingProduct = cart.find(item => item.barcode === product.barcode);
        if (existingProduct) {
            existingProduct.quantity += 1;
        } else {
            cart.push({
                id: product.id,
                barcode: product.barcode,
                productName: product.name,
                price: product.price,
                discountPercentage: product.discount,
                retailPrice: product.retailPrice,
                retailQuantity: product.retailQuantity,
                quantity: 1
            });
        }

        updateCartDisplay(); 
        document.getElementById("barcodeInput").value = ""; 
    } else {
        alert("Please enter a barcode or product name.");
    }
}
function showSuggestions() {
    const input = document.getElementById("barcodeInput").value.trim().toLowerCase();
    const suggestionBox = document.getElementById("suggestionBox");

    suggestionBox.innerHTML = "";

    if (input) {
        fetch(`/Inventory?handler=SearchProducts&query=${encodeURIComponent(input)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! Status: ${response.status}`);
                }
                return response.json();
            })
            .then(products => {
                if (products.length > 0) {
                    products.forEach(product => {
                        const suggestion = document.createElement("div");
                        suggestion.textContent = `${product.name} (${product.barcode}) - ₱${product.price}`;
                        suggestion.onclick = () => {
                            document.getElementById("barcodeInput").value = product.barcode;
                            suggestionBox.innerHTML = "";
                            suggestionBox.style.display = "none";
                            addToCart();
                        };
                        suggestionBox.appendChild(suggestion);
                    });
                    suggestionBox.style.display = "block";
                } else {
                    suggestionBox.style.display = "none";
                }
            })
            .catch(error => {
                console.error("Error fetching suggestions:", error);
            });
    } else {
        suggestionBox.style.display = "none";
    }
}

document.addEventListener("click", function (event) {
    const suggestionBox = document.getElementById("suggestionBox");
    const barcodeInput = document.getElementById("barcodeInput");
    if (!suggestionBox.contains(event.target) && event.target !== barcodeInput) {
        suggestionBox.style.display = "none";
    }
});

function updateCartDisplay() {
    const cartTableBody = document.getElementById("cartTable").querySelector("tbody");
    cartTableBody.innerHTML = "";
    let grandTotal = 0;

    cart.forEach(item => {
        const appliedPrice = item.quantity >= item.retailQuantity ? item.retailPrice : item.price;
        const discountAmount = (appliedPrice * item.discountPercentage) / 100;
        const discountedPrice = appliedPrice - discountAmount;
        const total = discountedPrice * item.quantity;
        grandTotal += total;
        const row = document.createElement("tr");
        row.innerHTML = `
        <td>${item.productName}</td>
        <td>${item.barcode}</td>
        <td>
            <input type="number" class="quantity-input"
                   value="${item.quantity}" 
                   min="1" 
                   onchange="updateCartQuantity('${item.barcode}', this.value)">
        </td>
        <td>₱${appliedPrice.toFixed(2)}</td>
        <td>${item.discountPercentage}%</td>
        <td>₱${total.toFixed(2)}</td>
        <td><button onclick="removeFromCart('${item.barcode}')">Remove</button></td>
    `;
        cartTableBody.appendChild(row);
    });

    document.getElementById("grandTotal").textContent = grandTotal.toFixed(2);
}

function removeFromCart(barcode) {
    const index = cart.findIndex(item => item.barcode === barcode);
    if (index !== -1) {
        cart.splice(index, 1);
    }
    updateCartDisplay(); 
}
function updateCartQuantity(barcode, newQuantity) {
    const item = cart.find(item => item.barcode === barcode);
    if (item) {
        const discountAmount = (item.price * item.discountPercentage) / 100; 
        item.quantity = parseInt(newQuantity, 10) || 1; 
        item.total = (item.price - discountAmount) * item.quantity; 
    }
    updateCartDisplay(); 
}
function openCheckoutModal() {
    if (cart.length === 0) {
        alert("The cart is empty. Please add items before proceeding to checkout.");
        return;
    }

    generateReceiptContent();
    document.getElementById("checkoutModal").style.display = "block";
}

function closeCheckoutModal() {
    document.getElementById("checkoutModal").style.display = "none";
}

function generateReceiptContent() {
    const receiptContent = document.getElementById("receiptContent");
    const transactionId = generateTransactionId();
    const shopName = "CAMZIE'S COLLECTION";
    const shopAddress = "Tanza Public Market Branch 25 Jasmin St., De Roman Subd., Brgy. Daang Amaya 1, Tanza, Cavite";
    const shopPhone = "+63 997 761 6753";
    const shopEmail = "camziescollections@yahoo.com";
    const printTime = new Date().toLocaleString();

    let content = `
        <div style="text-align: center;">
            <h3>${shopName}</h3>
            <p>${shopAddress}</p>
            <p>Phone: ${shopPhone} | Email: ${shopEmail}</p>
            <hr />
            <p><strong>Time:</strong> ${printTime}</p>
            <hr />
        </div>
        <table style="width: 100%; border-collapse: collapse; text-align: left;">
            <thead>
                <tr>
                    <th style="border-bottom: 1px solid #000;">Product</th>
                    <th style="border-bottom: 1px solid #000;">Qty</th>
                    <th style="border-bottom: 1px solid #000;">Price</th>
                    <th style="border-bottom: 1px solid #000;">Total</th>
                </tr>
            </thead>
            <tbody>
    `;

    let grandTotal = 0;

    cart.forEach(item => {
        const appliedPrice = item.quantity >= item.retailQuantity ? item.retailPrice : item.price;
        const discountAmount = (appliedPrice * item.discountPercentage) / 100;
        const discountedPrice = appliedPrice - discountAmount;
        const total = discountedPrice * item.quantity;
        grandTotal += total;

        content += `
            <tr>
                <td>${item.productName}</td>
                <td>${item.quantity}</td>
                <td>₱${appliedPrice.toFixed(2)}</td>
                <td>₱${total.toFixed(2)}</td>
            </tr>
        `;
    });

    content += `
            </tbody>
        </table>
        <p><strong>Grand Total: ₱${grandTotal.toFixed(2)}</strong></p>
        <p><strong>Transaction ID:</strong> ${transactionId}</p>
        <p><strong>Cashier:</strong> ${cashierName}</p>
        <p style="text-align: center;">Thank you for your purchase!</p>
    `;

    receiptContent.innerHTML = content;
    sendTransactionIdToServer(transactionId, grandTotal);
    sendStockUpdateToServer(cart);
}
function sendTransactionIdToServer(transactionId, amount) {
    fetch('/Sales/SaveTransactionId', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            transactionId: transactionId,
            amount: amount
        })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Transaction saved successfully:', data);
        })
        .catch(error => {
            console.error('Error saving transaction:', error);
        });
}
function generateTransactionId() {
    return `${Date.now()}-${Math.floor(Math.random() * 1000)}`;
}
function sendStockUpdateToServer(cart) {
    const cartData = cart.map(item => ({
        productId: item.id,
        quantity: item.quantity,
        productName: item.productName
    }));

    console.log("Sending Cart Data to Server:", cartData);

    fetch('/Sales/DeductStock', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(cartData)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => { throw err; });
            }
            return response.json();
        })
        .then(data => {
            console.log("Success:", data.message);
        })
        .catch(error => {
            console.error("Error updating stock:", error);
            if (error.insufficientItems) {
                alert(`${error.message}\n${error.insufficientItems.join("\n")}`);
            } else {
                alert("Transaction failed. Please try again.");
            }
        });
}

async function printReceipt() {
    const printContents = document.getElementById("receiptContent").innerHTML;
    const originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;

    cart.length = 0;
    updateCartDisplay();
    incrementWalkInCount();
    closeCheckoutModal();
    alert("Transaction successful! Stock updated.");
    function generateReceiptContent() {
        transactionId = generateTransactionId();
    }

    setTimeout(function () {
        location.reload();
    }, 500);
}
function confirmCheckout() {

    if (cart.length === 0) {
        alert("Your cart is empty. Please add items before confirming checkout.");
        return;
    }

    const receiptContent = document.getElementById("receiptContent").innerHTML.trim();
    if (!receiptContent || receiptContent.length === 0) {
        alert("Please generate a receipt first before confirming checkout.");
        return;
    }

    if (confirm("Are you sure you want to confirm this checkout?")) {
        incrementWalkInCount();
        console.log("Checkout confirmed.");
        alert("Checkout successfully confirmed!");
        cart.length = 0;
        updateCartDisplay();
        document.getElementById("receiptContent").innerHTML = "";
        closeCheckoutModal();
    }
    setTimeout(function () {
        location.reload();
    }, 500);
}

function incrementWalkInCount() {
    // Call the server to increment the walk-in count
    fetch('/Analytics/IncrementWalkInCount', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ increment: 1 })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to increment walk-in count');
            }
            return response.json();
        })
        .then(data => {
            console.log('Walk-in count incremented successfully');
        })
        .catch(error => {
            console.error('Error incrementing walk-in count:', error);
        });
}