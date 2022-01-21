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

var datestart = null;
var dateend = null;

var activeData = getActive();
var inactiveData = getInactive();
var deactivatedData = getDeactivated();
var approversData = getApprovers();
var aspData = getAsps();
var ratingsData = getRatings();


function init() {
    daterange();
    getData(true);
}

function getCountries() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/countries",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
                var html1='';
                var html2 = '';
                res.forEach(element => {
                    html1 += "<div class='item asp-country' data-value='" + element.code + "'>"+ element.name + "</div>"
                    html2 += "<div class='item approver-country' data-value='" + element.code + "'>"+ element.name + "</div>"
                });
                $(".options-list.asp").append(html1);
                $(".options-list.approver").append(html2);
            }
        })
        return tmp;
    }();
    return data;
}

function getActive() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/users?active=true",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

function getInactive() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/users?active=false",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

function getDeactivated() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/deactivated",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

function getApprovers() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/approvers",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

function getAsps() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/asps",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

function getRatings() {
    var data = function () {
        var tmp = null;
        $.ajax({
            url: "api/cms/questions",
            type: "GET",
            async: false,
            success: function (res) {
                tmp = res;
            }
        })
        return tmp;
    }();
    return data;
}

////////////////////////// Active Users //////////////////////////

const tableDOM1 = document.querySelector('#a-users');
tableDOM1.innerHTML = '';
const activeTable = new eds.Table(tableDOM1, {
    data: activeData,
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
                if (cellData === "0001-01-01T00:00:00")
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
    selectable: 'multi',
    rowsPerPage: 50,
    onCreatedActionsCell: (td) => {
        td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-cross"></i></button>`;

        td.querySelector('button.activate').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var name = $(tr).find('td').eq(1).text();
            var email = [];
            email.push($(tr).find('td').eq(4).text());
            var result = confirm("Are you sure you want to deactivate user?");
            if (result) {
                $.ajax({
                    url: "api/cms/deactivate?emails=" + email,
                    type: "PUT",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "User Action",
                            description: name + ' has been deactivated',
                        });
                        notification.init();
                        $('#a-users').dataTable().fnDestroy();
                        activeTable.update(getActive());
                        $('#a-users').dataTable({
                            "searching": true,
                            "bSort": false
                        });
                    }
                });
            }
        });
    }
});
activeTable.init();
$('#a-users').dataTable({
    "searching": true,
    "bSort": false
});
const toggleActivateBtn1 = () => {
    (document.querySelector('#deactivate-users')).style.display =
        (activeTable.selected.length === 0) ? 'none' : '';
};
document.querySelector('#deactivate-users').addEventListener('click', () => {
    var result = confirm("Are you sure you want to deactivate users?");
    var emails = [];
    if (result) {
        activeTable.selected.forEach((d) => {
            emails.push(d["email"]);
        });
        $.ajax({
            url: "api/cms/deactivate?emails=" + emails,
            type: "PUT",
            success: function (res) {
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been deactivated',
                });
                notification.init();

                $('#a-users').dataTable().fnDestroy();
                activeTable.update(getActive());
                $('#a-users').dataTable({
                    "searching": true,
                    "bSort": false
                });
            }
        });
    }
    toggleActivateBtn1();
});


tableDOM1.addEventListener('toggleSelectRow', toggleActivateBtn1);
toggleActivateBtn1()

document.querySelector('#export-ausers').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_ActiveUsers.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
    activeTable.data.forEach(row => {
        rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["lastActive"], row["registeredAt"]]);
    });
    exportToCsv("IMK_ActiveUsers.csv", rows)

});




////////////////////////// Inactive //////////////////////////

const tableDOM2 = document.querySelector('#i-users');
tableDOM2.innerHTML = '';
const inactiveTable = new eds.Table(tableDOM2, {
    data: inactiveData,
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
                if (cellData === "0001-01-01T00:00:00")
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
    selectable: 'multi',
    rowsPerPage: 50,
    onCreatedActionsCell: (td) => {
        td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-check"></i></button>
                        <button class="ml-2 btn-icon deactivate"><i class="icon icon-cross"></i></button>`;

        td.querySelector('button.activate').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var name = $(tr).find('td').eq(1).text();
            var email = [];
            email.push($(tr).find('td').eq(4).text());
            var result = confirm("Are you sure you want to activate user?");
            if (result) {
                $.ajax({
                    url: "api/cms/activate?emails=" + email,
                    type: "PUT",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "User Action",
                            description: name + ' has been activated',
                        });
                        notification.init();
                        $('#i-users').dataTable().fnDestroy();
                        inactiveTable.update(getInactive());
                        $('#i-users').dataTable({
                            "searching": true,
                            "bSort": false
                        });                    
                    }
                });
            }
        });
        td.querySelector('button.deactivate').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var name = $(tr).find('td').eq(1).text();
            var email = $(tr).find('td').eq(4).text();
            var result = confirm("Are you sure you want to deactivate user?");
            if (result) {
                $.ajax({
                    url: "api/cms/deactivate?emails=" + email,
                    type: "PUT",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "User Action",
                            description: name + ' has been deactivated',
                        });
                        notification.init();
                        $('#i-users').dataTable().fnDestroy();
                        inactiveTable.update(getInactive());
                        $('#i-users').dataTable({
                            "searching": true,
                            "bSort": false
                        });                    
                    }
                });
            }
        });

    }
});
inactiveTable.init();
$('#i-users').dataTable({
    "searching": true,
    "bSort": false
});

const toggleActivateBtn2 = () => {
    (document.querySelector('#activate-users')).style.display =
        (inactiveTable.selected.length === 0) ? 'none' : '';
    
        (document.querySelector('#deactivate-iusers')).style.display =
        (inactiveTable.selected.length === 0) ? 'none' : '';
};
document.querySelector('#activate-users').addEventListener('click', () => {
    var result = confirm("Are you sure you want to activate users?");
    var emails = [];
    if (result) {
        inactiveTable.selected.forEach((d) => {
            emails.push(d["email"]);

        });
        $.ajax({
            url: "api/cms/activate?emails=" + emails,
            type: "PUT",
            success: function (res) {
                $('#i-users').dataTable().fnDestroy();
                inactiveTable.update(getInactive());
                $('#i-users').dataTable({
                    "searching": true,
                    "bSort": false
                });   
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been activated',
                });
                notification.init();
            }
        });
    }
    toggleActivateBtn2();
});

document.querySelector('#deactivate-iusers').addEventListener('click', () => {
    var result = confirm("Are you sure you want to deactivate users?");
    var emails = [];
    if (result) {
        inactiveTable.selected.forEach((d) => {
            emails.push(d["email"]);
        });
        $.ajax({
            url: "api/cms/deactivate?emails=" + emails,
            type: "PUT",
            success: function (res) {
                $('#i-users').dataTable().fnDestroy();
                inactiveTable.update(getInactive());
                $('#i-users').dataTable({
                    "searching": true,
                    "bSort": false
                });   
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been deactivated',
                });
                notification.init();
            }
        });
    }
    toggleActivateBtn2();
});

tableDOM2.addEventListener('toggleSelectRow', toggleActivateBtn2);
toggleActivateBtn2()

document.querySelector('#export-iusers').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_InactiveUsers.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
    inactiveTable.data.forEach(row => {
        rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["lastActive"], row["registeredAt"]]);
    });
    exportToCsv("IMK_InactiveUsers.csv", rows)

});



////////////////////////// Deactived Users //////////////////////////


const tableDOM3 = document.querySelector('#d-users');
tableDOM3.innerHTML = '';
const deactivatedTable = new eds.Table(tableDOM3, {
    data: deactivatedData,
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
                if (cellData === "0001-01-01T00:00:00")
                    td.innerHTML = "Not active yet";
            },
        },
        {
            key: 'registeredAt',
            title: 'Registered On',
            sort: 'none'
        },
        {
            key: 'status',
            title: 'Status',
            sort: 'none'
        }
    ],
    actions: true,
    selectable: 'multi',
    rowsPerPage: 50,
    onCreatedActionsCell: (td) => {
        td.innerHTML = `<button class="btn-icon reactivate"><i class="icon icon-check"></i></button>`;

        td.querySelector('button.reactivate').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var name = $(tr).find('td').eq(1).text();
            var email = [];
            email.push($(tr).find('td').eq(4).text());
            var result = confirm("Are you sure you want to reactivate user?");
            if (result) {
                $.ajax({
                    url: "api/cms/activate?emails=" + email,
                    type: "PUT",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "User Action",
                            description: name + ' has been reactivated',
                        });
                        notification.init();
                        $('#d-users').dataTable().fnDestroy();
                        deactivatedTable.update(getDeactivated());
                        $('#d-users').dataTable({
                            "searching": true,
                            "bSort": false
                        });                             
                    }
                });
            }
        });

    }
});
deactivatedTable.init();
$('#d-users').dataTable({
    "searching": true,
    "bSort": false
});

const toggleActivateBtn3 = () => {
    (document.querySelector('#reactivate-users')).style.display =
        (deactivatedTable.selected.length === 0) ? 'none' : '';
};
document.querySelector('#reactivate-users').addEventListener('click', () => {
    var result = confirm("Are you sure you want to reactivate users?");
    var emails = [];
    if (result) {
        deactivatedTable.selected.forEach((d) => {
            emails.push(d["email"]);
        });
        $.ajax({
            url: "api/cms/activate?emails=" + emails,
            type: "PUT",
            success: function (res) {
                $('#d-users').dataTable().fnDestroy();
                deactivatedTable.update(getDeactivated());
                $('#d-users').dataTable({
                    "searching": true,
                    "bSort": false
                });
                const notification = new eds.Notification({
                    title: "User Action",
                    description: 'Selected users have been reactivated',
                });
                notification.init();   
            }
        });
    }
    toggleActivateBtn3();
});

tableDOM3.addEventListener('toggleSelectRow', toggleActivateBtn3);
toggleActivateBtn3()

document.querySelector('#export-dusers').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_DeactivatedUsers.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Name', 'Country', 'ASP', 'Email', 'Phone', 'Registered On']);
    deactivatedTable.data.forEach(row => {
        rows.push([row["name"], row["country"], row["asp"], row["email"], row["phone"], row["lastActive"], row["registeredAt"]]);
    });
    exportToCsv("IMK_DeactivatedUsers.csv", rows)

});





////////////////////////// Approvers //////////////////////////


const tableDOM4 = document.querySelector('#t-approvers');
tableDOM4.innerHTML = '';
const approverTable = new eds.Table(tableDOM4, {
    data: approversData,
    columns: [
        {
            key: 'id',
            title: 'Id',
            sort: 'asc',
            hidden: true
        },
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
    rowsPerPage: 50,
    onCreatedActionsCell: (td) => {
        td.innerHTML = `<button class="btn-icon delete"><i class="icon icon-trashcan"></i></button>`;

        td.querySelector('button.delete').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var id = $(tr).find('td').eq(0).text();
            var name = $(tr).find('td').eq(2).text();
            var email = $(tr).find('td').eq(3).text();
            var result = confirm("Are you sure you want to delete approver?");
            if (result) {
                $.ajax({
                    url: "api/cms/approver?id=" + id,
                    type: "DELETE",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "Approver Action",
                            description: name + ' has been removed',
                        });
                        notification.init();
                        $('#t-approvers').dataTable().fnDestroy();
                        approverTable.update(getApprovers());
                        $('#t-approvers').dataTable({
                            "searching": true,
                            "bSort": false
                        });                             
                    }
                });
            }
        });
    }
});

approverTable.init();
$('#t-approvers').dataTable({
    "searching": true,
    "bSort": false
});
document.querySelector('#export-approvers').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_Approvers.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Country', 'Name', 'Email', 'Role']);
    approverTable.data.forEach(row => {
        rows.push([row["country"], row["name"], row["email"], row["role"]]);
    });
    exportToCsv("IMK_Approvers.csv", rows)

});



////////////////////////// ASP Companies //////////////////////////


const tableDOM5 = document.querySelector('#t-asps');
tableDOM5.innerHTML = '';
const aspTable = new eds.Table(tableDOM5, {
    data: aspData,
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
    rowsPerPage: 50,
});

aspTable.init();
$('#t-asps').dataTable({
    "searching": true,
    "bSort": false
});
document.querySelector('#export-asps').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_ASP_Companies.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Country', 'ASP Companies']);
    aspTable.data.forEach(row => {
        rows.push([row["country"], row["asp"]]);
    });
    exportToCsv("IMK_ASP_Companies.csv", rows)

});

////////////////////////// User Ratings //////////////////////////


const tableDOM6 = document.querySelector('#t-ratings');
tableDOM6.innerHTML = '';
const ratingsTable = new eds.Table(tableDOM6, {
    data: ratingsData,
    columns: [
        {
            key: 'id',
            title: 'Id',
            width: '1%',
        },
        {
            key: 'question',
            title: 'Question',
            sort: 'none'
        },      

    ],
    actions: true,
    rowsPerPage: 50,
    onCreatedActionsCell: (td) => {
        td.innerHTML = `<button class="btn-icon delete"><i class="icon icon-trashcan"></i></button>`;

        td.querySelector('button.delete').addEventListener('click', (evt) => {
            var tr = evt.target.closest('tr');
            var id = $(tr).find('td').eq(0).text();
            var result = confirm("Are you sure you want to delete this statement?");
            if (result) {
                $.ajax({
                    url: "api/cms/question?id=" + id,
                    type: "DELETE",
                    success: function (res) {
                        const notification = new eds.Notification({
                            title: "Rating Action",
                            description:'Statement has been deleted',
                        });
                        notification.init();
                        $('#t-ratings').dataTable().fnDestroy();
                        ratingsTable.update(getRatings());
                        $('#t-ratings').dataTable({
                            "searching": true,
                            "bSort": false
                        });
                    }
                });
            }
        });
    }
});

ratingsTable.init();
$('#t-ratings').dataTable({
    "searching": true,
    "bSort": false
});
document.querySelector('#export-ratings').addEventListener('click', () => {
    const notification = new eds.Notification({
        title: 'Export data',
        description: 'Table data is exported to IMK_Rating_Questions.csv file',
    });
    notification.init();
    var rows = [];
    rows.push(['Questions']);
    ratingsTable.data.forEach(row => {
        rows.push([row["question"]]); 
    });
    exportToCsv("IMK_Rating_Questions.csv", rows)

});




function getData(first) {

    getCountries();
    var startdate = $('#start').attr('value');
    var enddate = $('#end').attr('value');

    datestart = moment().subtract(6, 'days');
    dateend = moment();
    var LogData = { start: decodeURIComponent(datestart.format('YYYY-MM-DD')), end: decodeURIComponent(dateend.format('YYYY-MM-DD')) }


    ////////////////////////// Logs //////////////////////////

    $.ajax({
        url: "api/cms/logs",
        type: "GET",
        data: LogData,
        beforeSend: function() {
            $('#loader').show()
        },
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
                    for (var i = 0; i < data['command'].length; i++) {
                        details += `<tr><td> ${data['command'][i]} </td><td> ${data['result'][i]}</td></tr>`;
                    }
                    td.innerHTML = '<table><th>Command</th><th>Result</th>' + details + '</table>';
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
        },
        complete: function() {
            $('#loader').hide()
        },
    });

}

/// ACTION ///
const selects = document.querySelectorAll('.select');
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

const datepickers = document.querySelectorAll('.datepicker');
if (datepickers) {
  Array.from(datepickers).forEach((datepickerDOM) => {
    const datepicker = new eds.Datepicker(datepickerDOM);
    datepicker.init();
  });
}

const collapse = document.querySelector('.action');
const collapsed = function(evt) {
    const contentDiv = evt.target.closest('.tile').querySelector('#logs-filter');
    if(contentDiv.style.display == 'none') 
        contentDiv.style.display = 'block';
    else 
        contentDiv.style.display = 'none';
  }
collapse.addEventListener('click', collapsed);


$('#menu li.item').on('click', function () {
    $('.element').hide();
    $("#" + $(this).attr('value') + "").show();
    var table = $("#" + $(this).attr('value') + "").find('table').attr('id');

    switch (table) {
        case 'a-users':
            $('#a-users').dataTable().fnDestroy();
            activeTable.update(getActive());
            $('#a-users').dataTable({
                "searching": true,
                "bSort": false
            }); 
        break;
        case 'i-users':
            $('#i-users').dataTable().fnDestroy();
            inactiveTable.update(getInactive());
            $('#i-users').dataTable({
                "searching": true,
                "bSort": false
            }); 
        break;
        case 'd-users':
            $('#d-users').dataTable().fnDestroy();
            deactivatedTable.update(getDeactivated());
            $('#d-users').dataTable({
                "searching": true,
                "bSort": false
            }); 
        break;
        case 't-approvers':
            $('#t-approvers').dataTable().fnDestroy();
            approverTable.update(getApprovers());
            $('#t-approvers').dataTable({
                "searching": true,
                "bSort": false
            }); 
        break;
        case 't-asps':
            $('#t-asps').dataTable().fnDestroy();
            aspTable.update(getAsps());
            $('#t-asps').dataTable({
                "searching": true,
                "bSort": false
            }); 
        break;
    }
});

$('#submit-approver').on('click', function (e) {
    e.preventDefault();
    var values = "name=" + $("#member-name").val() + "&email=" + $("#member-email").val() + "&role=" + $('input[name="role"]:checked').val() + "&country=" + $('.item.approver-country.active').text();
    $.ajax({
        url: "api/cms/approver?" + values,
        type: "POST",
        success: function (res) {
            const notification = new eds.Notification({
                title: "Approver Action",
                description: 'New approver has been added',
            });
            notification.init();
            $('#t-approvers').dataTable().fnDestroy();
            approverTable.update(getApprovers());
            $('#t-approvers').dataTable({
                "searching": true,
                "bSort": false
            });               
        }
    });
});

$('#submit-asp').on('click', function (e) {
    e.preventDefault();
    var values = "name=" + $("#asp-name").val() + "&country=" + $('.item.asp-country.active').text();
    $.ajax({
        url: "api/cms/asp?" + values,
        type: "POST",
        success: function (res) {
            const notification = new eds.Notification({
                title: "ASP Action",
                description: 'New ASP has been added',
            });
            notification.init();
            $('#t-asps').dataTable().fnDestroy();
            aspTable.update(getAsps());
            $('#t-asps').dataTable({
                "searching": true,
                "bSort": false
            });               
        }
    });
});

$('#submit-question').on('click', function (e) {
    e.preventDefault();
    var values = "name=" + $("#question-name").val();
    $.ajax({
        url: "api/cms/question?" + values,
        type: "POST",
        success: function (res) {
            const notification = new eds.Notification({
                title: "Rating Action",
                description: 'New Rating Statement has been added',
            });
            notification.init();
            $('#t-ratings').dataTable().fnDestroy();
            ratingsTable.update(getRatings());
            $('#t-ratings').dataTable({
                "searching": true,
                "bSort": false
            });               
        }
    });
});

function searchTable(search, table) {
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

function daterange() {
    
    var start = moment().subtract(6, 'days');
    var end = moment();
    $('#start').attr('value', start.format('YYYY-MM-DD'));
    $('#end').attr('value', end.format('YYYY-MM-DD'));
    document.getElementsByName('datepicker-start')[0].placeholder = start.format('YYYY-MM-DD')
    document.getElementsByName('datepicker-end')[0].placeholder = end.format('YYYY-MM-DD')
   
}

$('#filter-btn').on('click', function(e) {
    e.preventDefault();
    var inputs = document.getElementById("logs-form").elements;

    var startdate = $('#start').attr('value');
    var enddate = $('#end').attr('value');
    var Data ={
        SiteName: inputs["siteName"].value,
        UserName: inputs["userName"].value,
        Country: inputs["country"].value,
        StartDate : startdate,
        EndDate: enddate,
        Command: inputs["command"].value,
        Result: inputs["result"].value
    }

    $.ajax({
        url: "api/cms/filtered-logs",
        type: "GET",
        data: Data,
        dataType: "json",
        beforeSend: function() {
            $('#loader').show()
        },
        success: function (res) {
            const tableDOM = document.querySelector('#t-logs');
            tableDOM.innerHTML = '';
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
                    for (var i = 0; i < data['command'].length; i++) {
                        details += `<tr><td> ${data['command'][i]} </td><td> ${data['result'][i]}</td></tr>`;
                    }
                    td.innerHTML = '<table><th>Command</th><th>Result</th>' + details + '</table>';
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
        },
        complete: function() {
            $('#loader').hide()
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



