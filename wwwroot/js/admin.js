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
                        var name = $(tr).find('td').eq(1).text();
                        var userId = $(tr).find('td').eq(5).text();

                        $.ajax({
                            url: "api/cms/deactivate?userId=" + userId,
                            type: "PUT",
                            success: function (res) {
                                const notification = new eds.Notification({
                                    title: "User Activated",
                                    description: name + ' has been deactivated',
                                });
                                notification.init();
                            }
                        });
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
                        var name = $(tr).find('td').eq(1).text();
                        var userId = $(tr).find('td').eq(5).text();

                        $.ajax({
                            url: "api/cms/activate?userId=" + userId,
                            type: "PUT",
                            success: function (res) {
                                const notification = new eds.Notification({
                                    title: "User Activated",
                                    description: name + ' has been activated',
                                });
                                notification.init();
                            }
                        });
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
                    td.innerHTML = `<button class="btn-icon activate"><i class="icon icon-check"></i></button>`;

                    td.querySelector('button.activate').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var name = $(tr).find('td').eq(1).text();
                        var userId = $(tr).find('td').eq(5).text();

                        $.ajax({
                            url: "api/cms/activate?userId=" + userId,
                            type: "PUT",
                            success: function (res) {
                                const notification = new eds.Notification({
                                    title: "User Activated",
                                    description: name + ' has been activated',
                                });
                                notification.init();
                            }
                        });
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
                data: res,
                columns: [
                    {
                        key: 'id',
                        title: 'ID',
                        sort: 'asc'
                    },
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
                  td.innerHTML = `<b>Command:</b> ${data['command']}<br><b>Result:</b> ${data['result']}`;
                }
            });

            table.init();
        }
    });

    /// ACTION ///
    $('#menu li.item').on('click', function(){
        $('.element').hide();
        $("#"+$(this).attr('value')+"").show();
    });

}

