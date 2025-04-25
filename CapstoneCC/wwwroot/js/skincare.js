let currentPrice = 0;
let currentDiscount = 0;
function openProductModal(name, description, price, imagePath, discount, productId) {
    document.getElementById("modalTitle").innerText = name;
    document.getElementById("modalDescription").innerText = description;
    document.getElementById("modalPrice").innerText = `₱${price}`;
    document.getElementById("modalImage").src = imagePath;
    document.getElementById("ProductId").value = productId;

    if (discount) {
        document.getElementById("modalDiscount").innerText = `${discount}% OFF`;
    } else {
        document.getElementById("modalDiscount").innerText = "";
    }
    currentPrice = parseFloat(price);
    document.getElementById("quantity").value = 1;

    const discountedPrice = price * (1 - discount / 100);
    document.getElementById("DiscountedPrice").value = discountedPrice.toFixed(2);

    updateFinalPrice();

    const modal = document.getElementById("productModal");
    modal.style.display = "flex";;
}

function updateFinalPrice() {
    const quantity = parseInt(document.getElementById("quantity").value) || 1;
    const discount = parseFloat(document.getElementById("modalDiscount").innerText.replace("% OFF", "").trim()) || 0;

    // Calculate the discounted price
    const discountedPrice = currentPrice * (1 - discount / 100);
    const totalPrice = discountedPrice * quantity;

    // Update the total price displayed in the modal
    document.getElementById("finalPrice").innerText = `Total: ₱${totalPrice.toFixed(2)}`;
}

function closeModal() {
    document.getElementById("productModal").style.display = "none";
}

function addToCart() {
    const productId = document.getElementById("ProductId").value; // Get ProductId
    const quantity = parseInt(document.getElementById("quantity").value, 10); // Get Quantity
    const discountedPrice = currentPrice * (1 - currentDiscount / 100); // Calculate discounted price

    // Prepare data for backend
    const productData = {
        ProductId: productId,
        Quantity: quantity,
        DiscountedPrice: discountedPrice
    };

    // CSRF token retrieval (if necessary)
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // Send data to the backend using fetch
    fetch('/Skincare?handler=AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            ...(csrfToken && { 'RequestVerificationToken': csrfToken })
        },
        body: JSON.stringify(productData)
    })
        .then(response => {
            if (response.ok) {
                alert("Product added to cart successfully!");
                closeModal(); // Close the modal
            } else {
                alert("Failed to add product to cart.");
            }
        })
        .catch(error => {
            console.error("Error adding product to cart:", error);
            alert("An error occurred. Please try again.");
        });
}

document.getElementById("quantity").addEventListener("input", function () {
    updateFinalPrice();
});

function increaseQuantity() {
    const quantityInput = document.getElementById("quantity");
    quantityInput.value = (parseInt(quantityInput.value) || 1) + 1;

    // Update the total price dynamically
    updateFinalPrice();
}

function decreaseQuantity() {
    const quantityInput = document.getElementById("quantity");
    const currentValue = parseInt(quantityInput.value) || 1;
    if (currentValue > 1) {
        quantityInput.value = currentValue - 1;
    }

    // Update the total price dynamically
    updateFinalPrice();
}

window.onclick = function (event) {
    const modal = document.getElementById("productModal");
    if (event.target === modal) {
        closeModal();
    }
};

function redirectToLogin() {
    alert("You need to log in to add items to the cart.");
    window.location.href = '/Identity/Account/Login'; // Adjust the URL to match your login page
}