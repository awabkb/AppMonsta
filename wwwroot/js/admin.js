const treeDOM = document.querySelector('.tree.navigation');
const tree = new eds.Tree(treeDOM);
tree.init();

const layout = new eds.Layout(document.querySelector('body'));
layout.init();

// Layout & Page
const bodyDOM = document.querySelector('body');
const page = new eds.Page(bodyDOM);
page.init();

eds.NotificationLog.init();


function getData() {

    ////////////////////////// Active Users //////////////////////////
    $.ajax({
        url: "api/cms/users?active=true",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#a-users');
            tableDOM.innerHTML = '';
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'name',
                        title: 'Name',
                        sort: 'asc'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'asc'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'asc'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                        sort: 'asc'
                    },
                    {
                        key: 'phone',
                        title: 'Phone',
                    },
                    {
                        key: 'registeredAt',
                        title: 'Registered On',
                        sort: 'asc'
                    },
                ],
                actions: true,
                sortable: true,
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-cross"></i></button>`;

                    td.querySelector('button.activate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(0).text();
                        var email = $(tr).find('td').eq(3).text();
                        var result = confirm("Are you sure you want to deactivate user?");
                        if (result) {
                            $.ajax({
                                url: "api/cms/deactivate?email=" + email,
                                type: "PUT",
                                success: function (res) {
                                    const notification = new eds.Notification({
                                        title: "User Action",
                                        description: name + ' has been deactivated',
                                    });
                                    notification.init();
                                    getData();
                                }
                            });
                        }
                    });
                }
            });
            table.init();
            document.querySelector('#export-ausers').addEventListener('click', () => {
                const notification = new eds.Notification({
                  title: 'Export data',
                  description: 'Table data is exported to IMK_ActiveUsers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Name','Country', 'ASP', 'Email', 'Phone', 'Registered On']);
                table.data.forEach(row => {
                    rows.push([row["name"],row["country"], row["asp"], row["email"], row["phone"], row["registeredAt"]]);
                });
                exportToCsv("IMK_ActiveUsers.csv", rows)

            });
        }
    });


    ////////////////////////// Inactive //////////////////////////

    $.ajax({
        url: "api/cms/users?active=false",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#i-users');
            tableDOM.innerHTML = '';
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'name',
                        title: 'Name',
                        sort: 'asc'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'asc'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'asc'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                        sort: 'asc'
                    },
                    {
                        key: 'phone',
                        title: 'Phone',
                    },
                    {
                        key: 'registeredAt',
                        title: 'Registered On',
                        sort: 'asc'
                    },
                ],
                actions: true,
                sortable: true,
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-check"></i></button>`;

                    td.querySelector('button.activate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(0).text();
                        var email = $(tr).find('td').eq(3).text();
                        var result = confirm("Are you sure you want to activate user?");
                        if (result) {
                            $.ajax({
                                url: "api/cms/activate?email=" + email,
                                type: "PUT",
                                success: function (res) {
                                    const notification = new eds.Notification({
                                        title: "User Action",
                                        description: name + ' has been activated',
                                    });
                                    notification.init();
                                    getData();
                                }
                            });
                        }
                    });

                }
            });

            table.init();
            document.querySelector('#export-iusers').addEventListener('click', () => {
                const notification = new eds.Notification({
                  title: 'Export data',
                  description: 'Table data is exported to IMK_InactiveUsers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Name','Country', 'ASP', 'Email', 'Phone', 'Registered On']);
                table.data.forEach(row => {
                    rows.push([row["name"],row["country"], row["asp"], row["email"], row["phone"], row["registeredAt"]]);
                });
                exportToCsv("IMK_InactiveUsers.csv", rows)

            });
        }
    });



    ////////////////////////// Approvers //////////////////////////

    $.ajax({
        url: "api/cms/approvers",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#t-approvers');
            tableDOM.innerHTML = '';
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'asc'
                    },
                    {
                        key: 'name',
                        title: 'Name',
                        sort: 'asc'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                    },
                    {
                        key: 'role',
                        title: 'Role',
                        sort: 'asc'
                    },
                ],
                actions: true,
                sortable: true,
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon delete"><i class="icon icon-trashcan"></i></button>`;

                    td.querySelector('button.delete').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(1).text();
                        var email = $(tr).find('td').eq(2).text();
                        var result = confirm("Are you sure you want to delete approver?");
                        if (result) {
                            $.ajax({
                                url: "api/cms/approver?email=" + email,
                                type: "DELETE",
                                success: function (res) {
                                    const notification = new eds.Notification({
                                        title: "Approver Action",
                                        description: name + ' has been removed',
                                    });
                                    notification.init();
                                    getData()
                                }
                            });
                        }
                    });
                }
            });

            table.init();
            document.querySelector('#export-approvers').addEventListener('click', () => {
                const notification = new eds.Notification({
                  title: 'Export data',
                  description: 'Table data is exported to IMK_Approvers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Country', 'Name', 'Email', 'Role']);
                table.data.forEach(row => {
                    rows.push([row["country"],row["name"], row["email"], row["role"]]);
                });
                exportToCsv("IMK_Approvers.csv", rows)

            });
        }
    });



    ////////////////////////// Logs //////////////////////////

    $.ajax({
        url: "api/cms/logs",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#t-logs');
            const table = new eds.Table(tableDOM, {
                data: res["value"],
                columns: [
                    {
                        key: 'date',
                        title: 'Date',
                        sort: 'asc'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'asc'
                    },
                    {
                        key: 'site',
                        title: 'Site',
                        sort: 'asc'
                    },
                    {
                        key: 'longitude',
                        title: 'Longitude',
                    },
                    {
                        key: 'latitude',
                        title: 'Latitude',
                    },
                    {
                        key: 'rpi',
                        title: 'RPI',
                        sort: 'asc'
                    },
                    {
                        key: 'app',
                        title: 'APP',
                        sort: 'asc'
                    },
                    {
                        key: 'user',
                        title: 'User',
                        sort: 'asc'
                    },
                ],
                rowsPerPage: 50,
                sortable: true,
                expandable: true,
                onCreatedDetailsRow: (td, data) => {
                    td.innerHTML = `<b>Commands:</b> ${data['command']}<br><b>Results:</b> ${data['result']}`;
                }
            });

            table.init();
            document.querySelector('#export-logs').addEventListener('click', () => {
                const notification = new eds.Notification({
                  title: 'Export data',
                  description: 'Table data is exported to IMK_Logs.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Date', 'Country', 'Site', 'Longitude', 'Latitude', 'RPI Version', 'App Version', 'User', 'Commands', 'Results' ]);
                table.data.forEach(row => {
                    rows.push([row["date"],row["country"], row["site"], row["longitude"], row["latitude"], row["rpi"], row["app"], row["user"], row["command"],row["result"]]);
                });
                exportToCsv("IMK_Logs.csv", rows)

            });
        }
    });

}

/// ACTION ///
const selectDOM = document.querySelector('#country');
const select = new eds.Select(selectDOM);
select.init();



const dialogs = document.querySelectorAll('.dialog');
if (dialogs) {
    Array.from(dialogs).forEach((dialogDOM) => {
        const dialog = new eds.Dialog(dialogDOM);
        dialog.init();
    });
}

$('#menu li.item').on('click', function () {
    $('.element').hide();
    $("#" + $(this).attr('value') + "").show();
});

$('#submit-approver').on('click', function (e) {
    var values = "name=" + $("#member-name").val() + "&email=" + $("#member-email").val() + "&role=" + $('input[name="role"]:checked').val() + "&country=" + $('.item.country.active').text();
    $.ajax({
        url: "api/cms/approver?" + values,
        type: "POST",
        success: function (res) {
            console.log(res)
            const notification = new eds.Notification({
                title: "Approver Action",
                description: 'New approver has been added',
            });
            notification.init();
            getData();
        }
    });
});

function exportToCsv(filename, rows) {
    var processRow = function (row) {
        var finalVal = '';
        for (var j = 0; j < row.length; j++) {
            var innerValue = row[j] === null ? '' : row[j].toString();
            if (row[j] instanceof Date) {
                innerValue = row[j].toLocaleString();
            };
            var result = innerValue.replace(/"/g, '""');
            if (result.search(/("|,|\n)/g) >= 0)
                result = '"' + result + '"';
            if (j > 0)
                finalVal += ',';
            finalVal += result;
        }
        return finalVal + '\n';
    };

    var csvFile = '';
    for (var i = 0; i < rows.length; i++) {
        csvFile += processRow(rows[i]);
    }

    var blob = new Blob([csvFile], { type: 'text/csv;charset=utf-8;' });
    if (navigator.msSaveBlob) { // IE 10+
        navigator.msSaveBlob(blob, filename);
    } else {
        var link = document.createElement("a");
        if (link.download !== undefined) { // feature detection
            // Browsers that support HTML5 download attribute
            var url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", filename);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }
}


