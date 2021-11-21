// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function sortTable(n) {
    var table, rows, switching, i, x, y, shouldSwitch, dir, switchcount = 0;
    table = document.getElementById("resultTable");
    switching = true;
   
    dir = "asc";
  
    while (switching) {
       
        switching = false;
        rows = table.rows;
        
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            x = rows[i].getElementsByTagName("TD")[n];
            y = rows[i + 1].getElementsByTagName("TD")[n];
            var cmpX = isNaN(parseInt(x.innerHTML)) ? x.innerHTML.toLowerCase() : parseInt(x.innerHTML);
            var cmpY = isNaN(parseInt(y.innerHTML)) ? y.innerHTML.toLowerCase() : parseInt(y.innerHTML);
            cmpX = (cmpX == '-') ? 0 : cmpX;
            cmpY = (cmpY == '-') ? 0 : cmpY;
            if (dir == "asc") {
                if (cmpX > cmpY) {
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (cmpX < cmpY) {
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchcount++;
        } else {
            if (switchcount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
}

function populateResult() {
    $.ajax({
        url: 'Word/Analyze',
        dataType: "json",
        method: 'GET',
        success: function (words) {

            var resultTableHeader = $('#resultTable thead');
            resultTableHeader.empty();
            resultTableHeader.append('<tr><th onclick="sortTable(0)">Name</th><th onclick="sortTable(1)">Occurrences</th></tr>');

            var analyzeResultTable = $('#resultTable tbody');
            analyzeResultTable.empty();

            for (let i = 0; i < words.length; i++) {
                analyzeResultTable.append('<tr><td>' + words[i].name + '</td><td>' + words[i].count + '</td></tr>');
            }
        },
        error: function (err) {
            alert(err);
        }
    });
} 
