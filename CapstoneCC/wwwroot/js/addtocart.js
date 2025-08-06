const phpToUsdRate = 0.018;

document.querySelectorAll('.quantity').forEach(input => {
    input.addEventListener('input', function () {
        const row = this.closest('tr');
        const priceElement = row.querySelector('.price');
        const totalAmountElement = row.querySelector('.total-amount');

        if (!priceElement || !totalAmountElement) {
            console.error('Price or Total Amount element not found in the row.');
            return;
        }

        const price = parseFloat(priceElement.textContent.replace('₱', ''));
        const quantity = parseInt(this.value, 10);
        const total = price * quantity;

        totalAmountElement.textContent = `₱${total.toFixed(2)}`;
        updateCartTotal();
        const productId = row.getAttribute('data-product-id');
        updateQuantityInCart(productId, quantity);
    });
});

function updateQuantityInCart(productId, newQuantity) {
    const requestBody = {
        productId: productId,
        newQuantity: newQuantity
    };

    console.log('Sending data to server:', requestBody);

    fetch('/AddToCart?handler=UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
 
            updateCartTotal();
        } else {
            console.error("Failed to update quantity.");
        }
    })
    .catch(error => {
        console.error('Error updating quantity:', error);
    });
}

function updateCartTotal() {
    let totalAmount = 0;
    document.querySelectorAll('.total-amount').forEach(cell => {
        totalAmount += parseFloat(cell.textContent.replace('₱', '')) || 0;
    }); 
    document.getElementById('cart-total').textContent = `₱${totalAmount.toFixed(2)}`;
}

document.querySelectorAll('.remove-btn').forEach(button => {
    button.addEventListener('click', function () {
        const row = this.closest('tr');
        const productId = row.getAttribute('data-product-id');

        fetch('/AddToCart?handler=RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                productId: productId
            })
        }).then(response => {
            if (response.ok) {
                row.remove();
                updateCartTotal();
            }
        });
    });
});

function calculateTotalInUSD() {
    let totalAmountPhp = 0;
    document.querySelectorAll('.total-amount').forEach(cell => {
        totalAmountPhp += parseFloat(cell.textContent.replace('₱', ''));
    });
    return totalAmountPhp;
}

function openPaymentModal() {
    document.getElementById("paymentModal").style.display = "block";
}

function closeModal() {
    document.getElementById("paymentModal").style.display = "none";
    document.getElementById("paymentMethod").value = "";
    document.querySelector(".paypal-container").style.display = "none";
    document.getElementById("submitOrder").style.display = "none";
}

function selectPaymentMethod(method) {
    document.getElementById("paymentMethod").value = method;

    if (method === "paypal") {
        document.querySelector(".paypal-container").style.display = "block";
        document.getElementById("submitOrder").style.display = "none";
    } else if (method === "cod") {
        document.querySelector(".paypal-container").style.display = "none"; 
        document.getElementById("submitOrder").style.display = "block"; 
    }
}

paypal.Buttons({
    createOrder: function(data, actions) {
        const totalUsd = calculateTotalInUSD();
        return actions.order.create({
            purchase_units: [{
                amount: {
                    value: totalUsd
                }
            }]
        });
    } ,
    onApprove: function (data, actions) {
        return actions.order.capture().then(function (details) {
            console.log("PayPal Details:", details);
            
            const paypalOrderId = details.id; 
    
            fetch('/AddToCart?handler=SubmitOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    paymentMethod: 'paypal',
                    paypalOrderId: paypalOrderId,
                })
            })
            .then(response => {
                if (response.ok) {
                    window.location.href = '/Orders'; 
                } else {
                    response.text().then(successMessage => {
                        console.log('Server response:', successMessage);
                        alert('Payment successful! Thank you for your order.');
                        document.querySelectorAll('tr[data-product-id]').forEach(row => row.remove());
                        updateCartTotal();
                    });
                }
            })
            .catch(error => {
                console.error('Error during fetch:', error);
                alert('An error occurred while processing your payment. Please try again.');
            });
        });
    },
    onCancel: function () {
        alert('PayPal payment was cancelled.');
    },
    onError: function (err) {
        console.error(err);
        alert('An error occurred during PayPal transaction.');
    }
}).render('#paypal-button-container');

document.getElementById('checkoutButton').addEventListener('click', function (event) {
    event.preventDefault();
    openModal();
});
