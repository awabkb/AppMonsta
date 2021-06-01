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

