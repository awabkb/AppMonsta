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
                        sort: 'none'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'none'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'none'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                        sort: 'none'
                    },
                    {
                        key: 'phone',
                        title: 'Phone',
                    },
                    {
                        key: 'lastActive',
                        title: 'Last Active On',
                        sort: 'none',
                        onCreatedCell: (td, cellData) => {
                            if(cellData === "0001-01-01T00:00:00")
                                td.innerHTML = "Not active yet";
                        },
                    },
                    {
                        key: 'registeredAt',
                        title: 'Registered On',
                        sort: 'none'
                    },
                ],
                actions: true,
                sortable: true,
                selectable: 'multi',
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-cross"></i></button>`;

                    td.querySelector('button.activate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(1).text();
                        var email = $(tr).find('td').eq(4).text();
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
            const toggleActivateBtn = () => {
                (document.querySelector('#deactivate-users')).style.display =
                    (table.selected.length === 0) ? 'none' : '';
            };
            document.querySelector('#deactivate-users').addEventListener('click', () => {
                var result = confirm("Are you sure you want to deactivate users?");
                if (result) {
                    table.selected.forEach((d) => {
                        var email = d["email"];
                        $.ajax({
                            url: "api/cms/deactivate?email=" + email,
                            type: "PUT",
                            success: function (res) {
                                getData();
                            }
                        });
                    });
                }
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been deactivated',
                });
                notification.init();
                toggleActivateBtn();
            });

            tableDOM.addEventListener('toggleSelectRow', toggleActivateBtn);
            toggleActivateBtn()

            document.querySelector('#export-ausers').addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Table data is exported to IMK_ActiveUsers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
                table.data.forEach(row => {
                    rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["registeredAt"]]);
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
                        sort: 'none'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'none'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'none'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                        sort: 'none'
                    },
                    {
                        key: 'phone',
                        title: 'Phone',
                    },
                    {
                        key: 'lastActive',
                        title: 'Last Active On',
                        sort: 'none',
                        onCreatedCell: (td, cellData) => {
                            if(cellData === "0001-01-01T00:00:00")
                                td.innerHTML = "Not active yet";
                        },
                    },
                    {
                        key: 'registeredAt',
                        title: 'Registered On',
                        sort: 'none'
                    },
                ],
                actions: true,
                sortable: true,
                selectable: 'multi',
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-check"></i></button>`;

                    td.querySelector('button.activate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(1).text();
                        var email = $(tr).find('td').eq(4).text();
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

            const toggleActivateBtn = () => {
                (document.querySelector('#activate-users')).style.display =
                    (table.selected.length === 0) ? 'none' : '';
            };
            document.querySelector('#activate-users').addEventListener('click', () => {
                var result = confirm("Are you sure you want to activate users?");
                if (result) {
                    table.selected.forEach((d) => {
                        var email = d["email"];
                        $.ajax({
                            url: "api/cms/activate?email=" + email,
                            type: "PUT",
                            success: function (res) {
                                getData();
                            }
                        });
                    });
                }
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been activated',
                });
                notification.init();
                toggleActivateBtn();
            });

            tableDOM.addEventListener('toggleSelectRow', toggleActivateBtn);
            toggleActivateBtn()

            document.querySelector('#export-iusers').addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Table data is exported to IMK_InactiveUsers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
                table.data.forEach(row => {
                    rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["registeredAt"]]);
                });
                exportToCsv("IMK_InactiveUsers.csv", rows)

            });
        }
    });





    

    ////////////////////////// Deactived Users //////////////////////////

    $.ajax({
        url: "api/cms/deactivated",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#d-users');
            tableDOM.innerHTML = '';
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'name',
                        title: 'Name',
                        sort: 'none'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'none'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'none'
                    },
                    {
                        key: 'email',
                        title: 'Email',
                        sort: 'none'
                    },
                    {
                        key: 'phone',
                        title: 'Phone',
                    },
                    {
                        key: 'lastActive',
                        title: 'Last Active On',
                        sort: 'none',
                        onCreatedCell: (td, cellData) => {
                            if(cellData === "0001-01-01T00:00:00")
                                td.innerHTML = "Not active yet";
                        },
                    },
                    {
                        key: 'registeredAt',
                        title: 'Registered On',
                        sort: 'none'
                    },
                ],
                actions: true,
                sortable: true,
                selectable: 'multi',
                rowsPerPage: 50,
                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon reactivate"><i class="icon icon-check"></i></button>`;

                    td.querySelector('button.reactivate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(1).text();
                        var email = $(tr).find('td').eq(4).text();
                        var result = confirm("Are you sure you want to reactivate user?");
                        if (result) {
                            $.ajax({
                                url: "api/cms/activate?email=" + email,
                                type: "PUT",
                                success: function (res) {
                                    const notification = new eds.Notification({
                                        title: "User Action",
                                        description: name + ' has been reactivated',
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

            const toggleActivateBtn = () => {
                (document.querySelector('#reactivate-users')).style.display =
                    (table.selected.length === 0) ? 'none' : '';
            };
            document.querySelector('#reactivate-users').addEventListener('click', () => {
                var result = confirm("Are you sure you want to reactivate users?");
                if (result) {
                    table.selected.forEach((d) => {
                        var email = d["email"];
                        $.ajax({
                            url: "api/cms/reactivate?email=" + email,
                            type: "PUT",
                            success: function (res) {
                                getData();
                            }
                        });
                    });
                }
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been reactivated',
                });
                notification.init();
                toggleActivateBtn();
            });

            tableDOM.addEventListener('toggleSelectRow', toggleActivateBtn);
            toggleActivateBtn()

            document.querySelector('#export-dusers').addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Table data is exported to IMK_DeactivatedUsers.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
                table.data.forEach(row => {
                    rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["registeredAt"]]);
                });
                exportToCsv("IMK_DeactivatedUsers.csv", rows)

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
                    rows.push([row["country"], row["name"], row["email"], row["role"]]);
                });
                exportToCsv("IMK_Approvers.csv", rows)

            });
        }
    });


    ////////////////////////// ASP Companies //////////////////////////

    $.ajax({
        url: "api/cms/asps",
        type: "GET",
        success: function (res) {
            const tableDOM = document.querySelector('#t-asps');
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
                        key: 'asp',
                        title: 'ASP',
                        sort: 'non'
                    },
                ],
                actions: true,
                sortable: true,
                rowsPerPage: 50,
            });

            table.init();
            document.querySelector('#export-asps').addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Table data is exported to IMK_ASP_Companies.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Country', 'ASP Companies']);
                table.data.forEach(row => {
                    rows.push([row["country"], row["asp"]]);
                });
                exportToCsv("IMK_ASP_Companies.csv", rows)

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
                    var details = '';
                    for(var i =0; i< data['command'].length; i++) {
                        details += `<tr><td> ${data['command'][i]} </td><td> ${data['result'][i]}</td></tr>`;
                    }
                    td.innerHTML = '<table><th>Command</th><th>Result</th>' + details+'</table>';
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
                rows.push(['Date', 'Country', 'Site', 'Longitude', 'Latitude', 'RPI Version', 'App Version', 'User', 'Commands', 'Results']);
                table.data.forEach(row => {
                    rows.push([row["date"], row["country"], row["site"], row["longitude"], row["latitude"], row["rpi"], row["app"], row["user"], row["command"], row["result"]]);
                });
                exportToCsv("IMK_Logs.csv", rows)

            });
        }
    });

}

/// ACTION ///
const selects = document.querySelectorAll('#select-country');
if (selects) {
    Array.from(selects).forEach((selectDOM) => {
        const select = new eds.Select(selectDOM);
        select.init();
    });
}


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
    e.preventDefault();
    var values = "name=" + $("#member-name").val() + "&email=" + $("#member-email").val() + "&role=" + $('input[name="role"]:checked').val() + "&country=" + $('.item.country.active').text();
    $.ajax({
        url: "api/cms/approver?" + values,
        type: "POST",
        success: function (res) {
            const notification = new eds.Notification({
                title: "Approver Action",
                description: 'New approver has been added',
            });
            notification.init();
            getData();
        }
    });
});

$('#submit-asp').on('click', function (e) {
    e.preventDefault();
    var values = "name=" + $("#asp-name").val() + "&country=" + $('.item.country.active').text();
    $.ajax({
        url: "api/cms/asp?" + values,
        type: "POST",
        success: function (res) {
            const notification = new eds.Notification({
                title: "ASP Action",
                description: 'New ASP has been added',
            });
            notification.init();
            getData();
        }
    });
});

function searchTable(search, table)
{
    var input = document.getElementById(search);
    var filter = input.value.toUpperCase();
    var table = document.getElementById(table);
    var trs = table.tBodies[0].getElementsByTagName("tr");

    for (var i = 0; i < trs.length; i++) {
        var tds = trs[i].getElementsByTagName("td");
        trs[i].style.display = "none";
        for (var i2 = 0; i2 < tds.length; i2++) {
            if (tds[i2].innerHTML.toUpperCase().indexOf(filter) > -1) {
                trs[i].style.display = "";
                continue;
            }
        }
    }
}

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


