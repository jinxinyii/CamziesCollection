$(document).ready(function () {
    $('#orderDetailsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var orderId = button.data('order-id');

        // Fetch order details via AJAX
        $.ajax({
            url: '@Url.Page("OrderDetails")', // Razor Page to fetch order details
            type: 'GET',
            data: { id: orderId },
            success: function (data) {
                $('#orderDetailsContent').html(data);
            },
            error: function () {
                $('#orderDetailsContent').html('<p>Error loading order details.</p>');
            }
        });
    });
});

$('#confirmOrderButton').click(function () {
    var orderId = $('#orderDetailsModal').data('order-id');

    // Confirm order via AJAX
    $.ajax({
        url: '@Url.Page("ConfirmOrder")', // Razor Page to confirm order
        type: 'POST',
        data: {
            id: orderId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                alert('Order confirmed successfully.');
                $('#orderDetailsModal').modal('hide');
                // Optionally, refresh the orders list or update the UI
            } else {
                alert('Failed to confirm order.');
            }
        },
        error: function () {
            alert('Error confirming order.');
        }
    });
});

let currentOrderId = null; // To store the current OrderId
function confirmOrder(orderId) {
    if (confirm('Are you sure you want to approve this order?')) {
        updateOrderStatus(orderId, 'Approved');
    }
}

function declineOrder(orderId) {
    if (confirm('Are you sure you want to decline this order?')) {
        updateOrderStatus(orderId, 'Declined');
    }
}

function updateOrderStatus(orderId, status) {
    fetch(`/Orders?handler=UpdateOrderStatus`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ orderId: orderId, status: status })
    })
        .then(response => {
            if (response.ok) {
                location.reload();
            } else {
                alert('Failed to update order status.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('An error occurred while updating the order status.');
        });
}