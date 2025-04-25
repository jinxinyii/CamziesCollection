function validateMaxLength(input) {
    input.value = input.value.replace(/[^0-9]/g, '');
}

function openUpdateModal(productId) {
    const product = productData.find(p => p.id === productId);
    if (product) {
        document.getElementById('updateProductId').value = product.id;
        document.getElementById('updateProductName').value = product.name;
        document.getElementById('updateProductDiscount').value = product.discount;
        document.getElementById('updateProductPrice').value = product.price;
        document.getElementById('updateProductDescription').value = product.description;
        document.getElementById('updateProductBadge').value = product.badge;
        document.getElementById('updateProductStock').value = product.stock;
        document.getElementById('updateProductRetailPrice').value = product.retailPrice;
        document.getElementById('updateProductRetailQuantity').value = product.retailQuantity;
        if (product.barcode && (product.barcode.length === 12 || product.barcode.length === 13)) {
            document.getElementById("updateProductBarcode").value = product.barcode;
        } else {
            document.getElementById("updateProductBarcode").value = "";
        }
        document.getElementById('updateModal').style.display = 'block';
    }
}

function closeUpdateModal() {
    document.getElementById("updateModal").style.display = "none";
}

window.onclick = function (event) {
    const modal = document.getElementById("updateModal");
    if (event.target === modal) {
        closeUpdateModal();
    }
};