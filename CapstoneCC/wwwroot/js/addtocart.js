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

    console.log('Sending data to server:', requestBody); // Debugging: Log the data to be sent

    fetch('/AddToCart?handler=UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestBody)
    })
    .then(response => response.json()) // Expect a JSON response
    .then(data => {
        if (data.success) {
            // Optionally, update the cart total based on the server response
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

// Remove Product Row
document.querySelectorAll('.remove-btn').forEach(button => {
    button.addEventListener('click', function () {
        const row = this.closest('tr');
        const productId = row.getAttribute('data-product-id');

        // Send removal request to backend
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

// Open the payment modal
function openPaymentModal() {
    document.getElementById("paymentModal").style.display = "block";
}

// Close the payment modal
function closeModal() {
    // Close the modal and reset the payment method selection
    document.getElementById("paymentModal").style.display = "none";
    document.getElementById("paymentMethod").value = "";
    document.querySelector(".paypal-container").style.display = "none";
    document.getElementById("submitOrder").style.display = "none";
}

function selectPaymentMethod(method) {
    // Set the selected payment method in the hidden input
    document.getElementById("paymentMethod").value = method;

    // Show or hide elements based on the selected method
    if (method === "paypal") {
        document.querySelector(".paypal-container").style.display = "block";
        document.getElementById("submitOrder").style.display = "none"; // Hide submit button for PayPal
    } else if (method === "cod") {
        document.querySelector(".paypal-container").style.display = "none"; // Hide PayPal button
        document.getElementById("submitOrder").style.display = "block"; // Show submit button
    }
}

// PayPal button initialization
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
            console.log("PayPal Details:", details); // Log the details for debugging
            
            // Extract the PayPal order ID
            const paypalOrderId = details.id; 
    
            // Call a server-side handler to process the PayPal payment
            fetch('/AddToCart?handler=SubmitOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    paymentMethod: 'paypal',
                    paypalOrderId: paypalOrderId, // Pass the PayPal order ID to the server
                    // You can pass other details from the 'details' object if needed
                })
            })
            .then(response => {
                if (response.ok) {
                    // Redirect to the Orders page after successful server-side processing
                    window.location.href = '/Orders'; 
                } else {
                    // Handle errors if the server-side processing failed
                    response.text().then(errorMessage => {
                        console.error('Server-side error:', errorMessage);
                        alert('An error occurred while processing your order. Please try again.');
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

// Trigger modal open when checkout button is clicked
document.getElementById('checkoutButton').addEventListener('click', function (event) {
    event.preventDefault(); // Prevent default form submission
    openModal();
});
