$(document).ready(function () {
    // Initialize DataTable
    var table = $('#salesTable').DataTable({
        "order": [[1, "asc"]],
        "searching": true,
        "columnDefs": [
            { "type": "date", "targets": 1 }
        ]
    });

    // Initialize DateTime pickers
    var minDate, maxDate;
    minDate = new DateTime($('#min'), {
        format: 'YYYY-MM-DD'
    });
    maxDate = new DateTime($('#max'), {
        format: 'YYYY-MM-DD'
    });

    // Function to calculate total amount based on date range
    function calculateTotalAmount() {
        var min = minDate.val() ? new Date(minDate.val()) : null;
        var max = maxDate.val() ? new Date(maxDate.val()) : null;
        var total = 0;

        table.rows().every(function () {
            var data = this.data();
            var date = new Date(data[1]); // Assuming the date is in the second column
            var amount = parseFloat(data[4]) || 0; // Assuming the amount is in the fifth column

            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                total += amount;
            }
        });

        $('#totalAmount').text(total.toFixed(2));
    }

    // Automatically calculate total when start or end date changes
    $('#min, #max').on('change', function () {
        table.draw(); // Redraw table to apply the filter
        calculateTotalAmount(); // Calculate the total amount
    });

    // Custom filtering function to filter data based on date range
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = minDate.val();
            var max = maxDate.val();
            var date = new Date(data[1]); // Assuming the date is in the second column

            if (
                (min === null && max === null) ||
                (min === null && date <= new Date(max)) ||
                (new Date(min) <= date && max === null) ||
                (new Date(min) <= date && date <= new Date(max))
            ) {
                return true;
            }
            return false;
        }
    );

    // Event listener to redraw the table when the date range changes
    $('#min, #max').on('change', function () {
        table.draw();
    });
});
