// Sorting table with bubble sort
function sortTable(col, num) {
    var rows, direction, switching, i, current, next, shouldSwitch,  switchCount = 0;
    var table = document.getElementById("resultTable" + num);
    switching = true;

    // default direction
    direction = "asc";
  
    while (switching) {
       
        switching = false;
        rows = table.rows;

        // loop each row
        for (i = 1; i < (rows.length - 1); i++) {
            shouldSwitch = false;
            current = rows[i].getElementsByTagName("td")[col];
            next = rows[i + 1].getElementsByTagName("td")[col];

            // parse as number if its count, and to lower case if its word
            var compareX = isNaN(parseInt(current.innerHTML)) ? current.innerHTML.toLowerCase() : parseInt(current.innerHTML);
            var compareY = isNaN(parseInt(next.innerHTML)) ? next.innerHTML.toLowerCase() : parseInt(next.innerHTML);

            // when sort ascending if present is greater and next value, then switch
            if (direction === "asc") {
                if (compareX > compareY) {
                    shouldSwitch = true;
                    break;
                }
            }
            // when sort descending if present is lesser and next value, then switch
            else if (direction === "desc") {
                if (compareX < compareY) {
                    shouldSwitch = true;
                    break;
                }
            }
        }

        // switch current with next, then continue
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
            switchCount++;
        }

        // if no switch, then change direction and re-sort
        else {
            if (switchCount === 0 && direction === "asc") {
                direction = "desc";
                switching = true;
            }
        }
    }
}

// Populate result into table if success
function populateResult(textContent, urlContent, option1, option2, option3) {

    // when start processing disable analyze button to prevent sending new request before complete
    var analyzeButton = document.getElementById('analyze');
    var notification = document.getElementById('notif');
    disableButton(analyzeButton);

    // input text and url must have value and validated
    if (!checkValidUrl(urlContent) || !textContent) {
        alert("Please enter some input text with a valid URL");
        enableButton(analyzeButton);
        return;
    }

    // if all options unchecked, show notification
    if (!option1 && !option2 && !option3) {
        enableButton(analyzeButton);
        notification.hidden = false;
        return;
    }

    // Sanitize white space
    var cleanTextContent = textContent.replace(/\s+/g, " ").trim();

    // send request from form to backend
    $.ajax({
        url: 'Word/Analyze',
        dataType: "json",
        data: { 'text': cleanTextContent, 'url': urlContent, 'opt1': option1, 'opt2': option2, 'opt3': option3 },
        method: 'post',
        success: function (words) {

            // Clear all table before populating new one
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
                // populate table for option 1 result
                resultTableHeader1.append(
                    '<tr><th onclick="sortTable(0,1)">Name</th><th onclick="sortTable(1,1)">Occurrences in Page Content</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable1.append('<tr><td>' + words[i].name + '</td><td>' + words[i].count + '</td></tr>');
                }
            }

            if (option2) {
                // populate table for option 2 result
                resultTableHeader2.append(
                    '<tr><th onclick="sortTable(0,2)">Name</th><th onclick="sortTable(1,2)">Occurrences in Page Meta Tags</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable2.append('<tr><td>' + words[i].name + '</td><td>' + words[i].metaCount + '</td></tr>');
                }
            }

            if (option3) {
                // populate table for option 3 result
                resultTableHeader3.append(
                    '<tr><th onclick="sortTable(0,3)">Name</th><th onclick="sortTable(1,3)">Number of external links</th></tr>');

                for (let i = 0; i < words.length; i++) {
                    analyzeResultTable3.append('<tr><td>' + words[i].name + '</td><td>' + words[i].extLinkCount + '</td></tr>');
                }
            }

            // once finished clear notification and enable analyze button
            notification.hidden = true;
            enableButton(analyzeButton);
        },
        error: function (err) {
            // when error show alert, clear notification and enable analyze button
            alert(err);
            notification.hidden = true;
            enableButton(analyzeButton);
        }
    });
}

// Expected url pattern to process
function checkValidUrl(url) {
    var pattern = new RegExp(
        '^(https?:\\/\\/)?' + // protocol http or https
        '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name alphabet with dot (.)
        '((\\d{1,3}\\.){3}\\d{1,3}))' + // ip v4 address
        '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port number and path
        '(\\?[;&a-z\\d%_.~+=-]*)?' + // query strings starts with (?)
        '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator

    return pattern.test(url);
}

// Disable analyze button
function disableButton(analyzeButton) {
    analyzeButton.disabled = 'disabled';
    analyzeButton.value = "Processing...";
}

// Enable analyze button
function enableButton(analyzeButton) {
    analyzeButton.disabled = false;
    analyzeButton.value = "Analyze";
}
