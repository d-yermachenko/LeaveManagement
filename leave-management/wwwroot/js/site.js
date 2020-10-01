// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function updateCompaniesList(sender) {
    $.ajax({
        type: "POST",
        url: sender.getAttribute("data-refreshMethod"),
        data: { number1: val1, number2: val2 },
        dataType: "text",
        success: function (msg) {
            console.log(msg);
        },
        error: function (req, status, error) {
            console.log(msg);
        }
    })
}