// Review sort algorithm required
function sortTable(n, num) {
    var rows, switching, i, x, y, shouldSwitch, dir, switchCount = 0;
    var table = document.getElementById("resultTable" + num);
    switching = true;
   
    dir = "asc";
  
    while (switching) {
       
        switching = false;
        rows = table.rows;
        
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            x = rows[i].getElementsByTagName("td")[n];
            y = rows[i + 1].getElementsByTagName("td")[n];
            var compareX = isNaN(parseInt(x.innerHTML)) ? x.innerHTML.toLowerCase() : parseInt(x.innerHTML);
            var compareY = isNaN(parseInt(y.innerHTML)) ? y.innerHTML.toLowerCase() : parseInt(y.innerHTML);
            compareX = (compareX == '-') ? 0 : compareX;
            compareY = (compareY == '-') ? 0 : compareY;
            if (dir == "asc") {
                if (compareX > compareY) {
                    shouldSwitch = true;
                    break;
                }
            } else if (dir == "desc") {
                if (compareX < compareY) {
                    shouldSwitch = true;
                    break;
                }
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchCount++;
        } else {
            if (switchCount == 0 && dir == "asc") {
                dir = "desc";
                switching = true;
            }
        }
    }
}

function populateResult(textContent, urlContent, option1, option2, option3) {
    var analyzeButton = document.getElementById('analyze');
    var notification = document.getElementById('notif');
    disableButton(analyzeButton);

    if (!checkValidUrl(urlContent) || !textContent) {
        alert("Please enter some input text with a valid URL");
        enableButton(analyzeButton);
        return;
    }
    if (!option1 && !option2 && !option3) {
        enableButton(analyzeButton);
        notification.hidden = false;
        return;
    }
    $.ajax({
        url: 'Word/Analyze',
        dataType: "json",
        data: { 'text': textContent, 'url': urlContent, 'opt1': option1, 'opt2': option2, 'opt3': option3 },
        method: 'post',
        success: function (words) {

            // Clear all table
            var resultTableHeader1 = $('#resultTable1 thead');
            resultTableHeader1.empty();
            var analyzeResultTable1 = $('#resultTable1 tbody');
            analyzeResultTable1.empty();
            var resultTableHeader2 = $('#resultTable2 thead');
            resultTableHeader2.empty();
            var analyzeResultTable2 = $('#resultTable2 tbody');
            analyzeResultTable2.empty();
            var resultTableHeader3 = $('#resultTable3 thead');
            resultTableHeader3.empty();
            var analyzeResultTable3 = $('#resultTable3 tbody');
            analyzeResultTable3.empty();

            if (option1) {
                resultTableHeader1.append(
                    '<tr><th onclick="sortTable(0,1)">Name</th><th onclick="sortTable(1,1)">Occurrences in Page Content</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable1.append('<tr><td>' + words[i].name + '</td><td>' + words[i].count + '</td></tr>');
                }
            }

            if (option2) {
                resultTableHeader2.append(
                    '<tr><th onclick="sortTable(0,2)">Name</th><th onclick="sortTable(1,2)">Occurrences in Page Meta Tags</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable2.append('<tr><td>' + words[i].name + '</td><td>' + words[i].metaCount + '</td></tr>');
                }
            }

            if (option3) {
                resultTableHeader3.append(
                    '<tr><th onclick="sortTable(0,3)">Name</th><th onclick="sortTable(1,3)">Number of external links</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable3.append('<tr><td>' + words[i].name + '</td><td>' + words[i].extLinkCount + '</td></tr>');
                }
            }
            notification.hidden = true;
            enableButton(analyzeButton);
        },
        error: function (err) {
            alert(err);
            notification.hidden = true;
            enableButton(analyzeButton);
        }
    });
}

function checkValidUrl(url) {
    var pattern = new RegExp(
        '^(https?:\\/\\/)?' + // protocol
        '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
        '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
        '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
        '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
        '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator

    return pattern.test(url);
}

function disableButton(analyzeButton) {
    analyzeButton.disabled = 'disabled';
    analyzeButton.value = "Processing...";
}

function enableButton(analyzeButton) {
    analyzeButton.disabled = false;
    analyzeButton.value = "Analyze";
}
