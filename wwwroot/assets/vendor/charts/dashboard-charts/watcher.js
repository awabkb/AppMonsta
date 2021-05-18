(function (window, document, $scope, undefined) {
    "use strict";
    document.getElementById('filterDiv').setAttribute('style', 'visibility:hidden');
    document.getElementById("body").removeAttribute('hidden');
    init();
    // if(localStorage['authToken'] != null){
    //     var t1=localStorage['authToken'];
    //     var t2= new Date().getTime()
    //     var diff=parseInt((t2-t1)/(3600*1000))
    //     if(diff>=2){
    //        // window.location.replace("http://ericssonquiz.com/dashboard/login.html");
    //     } else {
    //         document.getElementById("body").removeAttribute('hidden');
    //         init();
    //     }
    // }
    // else {
    //    // window.location.replace("http://ericssonquiz.com/dashboard/login.html");
    // }
})(window, document, window.jQuery);

var showFilter = true;
var filterCountryList = [];
var filterMAList = [];
var filterOperatorList = [];
var dataTableData = '';
var topquery = '';
async function httpGetAsyncHtml(theUrl, callback) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    };
    console.log(theUrl + localStorage['PQ'] + "&test=" + new Date().getTime() + 1);
    xmlHttp.open("GET", theUrl + localStorage['PQ'] + "&test=" + new Date().getTime() + 1, true); // true for asynchronous 
    xmlHttp.send(null);
};

async function httpGetAsync(theUrl, callback) {
    theUrl = theUrl + "&test=" + new Date().getTime() + 1;
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    };
    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.send(null);
};
async function httpPostAsync(siteEng) {
    var http = new XMLHttpRequest();
    //var url = 'https://ericssonquiz.com/dashboard-backend/sendUserInfo.php';
    var params = '&Email=' + localStorage['Email'] + '&siteEng=' + siteEng;
    console.log(params);
    //http.open('POST', url, true);

    //Send the proper header information along with the request
    //http.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');

    // http.onreadystatechange = function () {//Call a function when the state changes.
    //     if (http.readyState == 4 && http.status == 200) {
    //         const json = http.responseText;
    //         const obj = JSON.parse(json);
    //         if (obj.success) {
    //             alert("An Email has been sent to you!\nIf nothing in your inbox please check your junk.")
    //         } else {
    //             alert("An Error occure ");
    //         }
    //     }
    // }
    //http.send(params);
};

var htmlFilterContent = '';

function init() {
    //getAllCountries()
    localStorage["MQ"]="marketArea=MMEA";
    getMA()
    var start = moment().subtract(29, 'days');
    var end = moment();

    function cb(start, end) {
        $('#reportrange span').html(
            start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY')
        );
        localStorage['PQ'] = 'start=\'' + start.format('YYYY-MM-DD') + '\'&end=\'' + end.format('YYYY-MM-DD') + '\''
        document.getElementById("initmapbutton").click();
        if (localStorage['CQ'] != null) {
            getdata('start=\'' + start.format('YYYY-MM-DD') + '\'&end=\'' + end.format('YYYY-MM-DD') + '\'' + localStorage['CQ']);
            localStorage['QQ'] = "'start=\'' + start.format('YYYY-MM-DD') + '\'&end=\'' + end.format('YYYY-MM-DD') + '\''+localStorage['CQ']";
        } else {
            getdata('start=\'' + start.format('YYYY-MM-DD') + '\'&end=\'' + end.format('YYYY-MM-DD') + '\'');
            localStorage['QQ'] = 'start=\'' + start.format('YYYY-MM-DD') + '\'&end=\'' + end.format('YYYY-MM-DD') + '\'';

        }
    }

    $('#reportrange').daterangepicker({
        startDate: start,
        endDate: end,
        ranges: {
            'Last 7 Days': [
                moment().subtract(6, 'days'),
                moment()
            ],
            'Last 30 Days': [
                moment().subtract(29, 'days'),
                moment()
            ],
            'This Month': [
                moment().startOf('month'), moment().endOf('month')
            ],
            'Last Month': [
                moment()
                    .subtract(1, 'month')
                    .startOf('month'),
                moment()
                    .subtract(1, 'month')
                    .endOf('month')
            ]
        }
    }, cb);

    cb(start, end);
    document.getElementById("initmapbutton").click();

}
function logoutbtn() {
    localStorage['authToken'] = '';
    window.location.replace("https://ericssonquiz.com/dashboard/login.html");
}
function filter() {
    document.getElementById("filterDiv").setAttribute('style', 'visibility:visible');
    showFilter = false;
}
function filterDone() {
    document.getElementById('filterDiv').setAttribute('style', 'visibility:hidden');
    showFilter = true;
    var arrStr = encodeURIComponent(filterCountryList);
    var arrStrOpertator = encodeURIComponent(filterOperatorList);
    
    localStorage['CQ'] = '&filetedcountries=' + arrStr + '&filetedoperators=' + arrStrOpertator;
    getdata(localStorage['PQ'] + '&filetedcountries=' + arrStr + '&filetedoperators=' + arrStrOpertator)
}

function reset() {
    var arrStr = encodeURIComponent(JSON.stringify([]));
    var arrStrOpertator = encodeURIComponent(JSON.stringify([]));
    localStorage['CQ'] = '&filetedcountries=' + arrStr + '&filetedoperators=' + arrStrOpertator;
    getdata(localStorage['PQ'] + '&filetedcountries=' + arrStr + '&filetedoperators=' + arrStrOpertator)
}

function oncheck(country) {
    if (country != "SelectALL") {
        var found = jQuery.inArray(country, filterCountryList);
        if (found >= 0) {
            filterCountryList.splice(found, 1);
        } else {
            filterCountryList.push(country);
        }
        document.getElementById("SelectAllCountries").removeAttribute("checked");
        if (filterCountryList.length == 0) {
            document.getElementById("SelectAllCountries").setAttribute("checked", "checked");
        }
    } else {
        filterCountryList = [];
        //getAllCountries()
        getMA()

     

    }

    var list = decodeURIComponent(filterCountryList);
    $.ajax({
        url: 'api/dashboardapi/operators',
        type: 'GET',
        data: { countries: list },
        success: function (res) {
            htmlFilterContent = '<h2>Operators</h2><form>';
            for (var i = 0; i < res.length; i++) {
                for (var j = 0; j < res[i].operators.length; j++) {
                    if (res[i].operators[j]["name"] != "SignIn Error" && res[i].operators[j]["name"] != "NA") {
                        var checked = '';
                        htmlFilterContent += '<label class="custom-control custom-checkbox custom-control-inline">' +
                            '<input type="checkbox" ' + checked + ' class="custom-control-input"  onclick=\'onOperatorcheck("' + res[i].operators[j]["name"] + '");\'><span class="custom-control-label">'
                            + res[i].operators[j]["name"]
                            + '</span></label>';
                    }
                }
            }
            htmlFilterContent += '</form>';
            filterOperatorList = [];
            document.getElementById('operators').innerHTML = htmlFilterContent;
        }
    });
}

function onOperatorcheck(operator) {
    var found = jQuery.inArray(operator, filterOperatorList);
    if (found >= 0) {
        filterOperatorList.splice(found, 1);
    } else {
        filterOperatorList.push(operator);
    }
    var arrStr = encodeURIComponent(JSON.stringify(filterOperatorList));

}

function getAllCountries() {
    $.ajax({
        url: 'api/dashboardapi/countries',
        type: 'GET',
        success: function (res) {
            htmlFilterContent = '<h2>Countries</h2><form>';
            htmlFilterContent += '<label class="custom-control custom-checkbox custom-control-inline">' +
                '<input type="checkbox" id="SelectAllCountries" checked="" class="custom-control-input"  onclick=\'oncheck("SelectALL");\'><span class="custom-control-label">Select All</span></label><br>'
            for (var i = 0; i < res.length; i++) {
                var checked = '';
                if (res[i] != "LocationNotOn" && res[i] != "NoInternetConnection") {
                    htmlFilterContent += '<label class="custom-control custom-checkbox custom-control-inline">' +
                        '<input type="checkbox" ' + checked + ' class="custom-control-input"  onclick=\'oncheck("' + res[i] + '");\'><span class="custom-control-label">'
                        + res[i]
                        + '</span></label>';
                }
            }
            htmlFilterContent += '</form>' + '<div id=\'operators\'></div><div class="input-group-append"><button type="button" onclick=\'filterDone()\' class="btn btn-primary">Done</button></div>'
            document.getElementById('filterBody').innerHTML = htmlFilterContent;
        }
    });
}


var radios = document.querySelectorAll('input[type=radio][name="MA"]');
radios.forEach(radio => radio.addEventListener('change', () =>  {
    localStorage['MQ']='marketArea='+radio.value.toString();
    getMA()
}));

function getMA() {
    $.ajax({
        url: 'api/dashboardapi/countries2',
        type: 'GET',
        data: localStorage['MQ'],
        success: function (res) {
            filterMAList = res;
            htmlFilterContent = '<h2>Countries</h2><form>';
            htmlFilterContent += '<label class="custom-control custom-checkbox custom-control-inline">' +
                '<input type="checkbox" id="SelectAllCountries" checked="" class="custom-control-input"  onclick=\'oncheck("SelectALL");\'><span class="custom-control-label">Select All</span></label><br>'
            for (var i = 0; i < res.length; i++) {
                var checked = '';
                if (res[i] != "LocationNotOn" && res[i] != "NoInternetConnection") {
                    htmlFilterContent += '<label class="custom-control custom-checkbox custom-control-inline">' +
                        '<input type="checkbox" ' + checked + ' class="custom-control-input"  onclick=\'oncheck("' + res[i] + '");\'><span class="custom-control-label">'
                        + res[i]
                        + '</span></label>';
                }
            }
            htmlFilterContent += '</form>' + '<div id=\'operators\'></div><div class="input-group-append"><button type="button" onclick=\'filterDone()\' class="btn btn-primary">Done</button></div>'
            document.getElementById('filterBody').innerHTML = htmlFilterContent;
        }
    });
}

function getdata(query) {
    topquery = query;
    var countries = decodeURIComponent(query);

    $.ajax({
        url: "api/dashboardapi/unique_sites",
        type: "GET",
        data: countries,
        success: function (res) {
            if ($('#site_count').length) {
                var dataperdate = [];
                var countries = [];
                for (var i in res) {
                    var obj = res[i];
                    for (var key in obj) {
                        if (dataperdate[i] == null) {
                            dataperdate[i] = [];
                        }
                        try {
                            dataperdate[i][key] == null ? dataperdate[i][key] = parseInt(obj[key]) : dataperdate[i][key] += parseInt(obj[key]);
                            countries[key] = [];
                            countries[key].push(key);
                        }
                        catch (err) {
                        }
                    }
                    var i = 1;
                    var datesvaluecountries = ['x'];
                    for (var x in dataperdate) {
                        datesvaluecountries.push(x);
                        for (var y in dataperdate[x]) {
                            countries[y].push(dataperdate[x][y]);
                        }
                        for (var n in countries) {
                            if (countries[n][i] == null) {
                                countries[n].push(0);
                            }
                        }
                        i++;

                    }
                    var data = [];
                    var dataheader = [];

                    for (var x in countries) {
                        dataheader.push(countries[x][0]);
                        data.push(countries[x]);
                    }
                    data.push(datesvaluecountries);

                    var chart = c3.generate({
                        bindto: "#site_count",
                        data: {
                            x: 'x',
                            columns: data,
                            type: 'bar',
                            groups: [
                                dataheader
                            ]
                        },

                        zoom: {
                            enabled: true
                        },
                        axis: {
                            y: {
                                show: true,
                            },
                            x: {
                                show: false,
                                type: 'timeseries',
                                tick: {
                                    format: '%Y-%m-%d',
                                }
                            }
                        }
                    });
                }
            }
        }
        /*
        success: function (res) {
            if ($('#site_count').length) {
                var rows = ['sites'];
                var rowsdate = ['x'];
                var datevalues = [];
                var sum = 0;
                for (var i in res) {
                    if (i != '0000-00-00') {
                        rows.push(res[i]);
                        sum += parseInt(res[i]);
                        rowsdate.push(i);
                        datevalues.push(i);
                    }
                }
                document.getElementById("ksamobilysitenumber").innerHTML = "#" + sum;
                var chart = c3.generate({
                    bindto: "#site_count",

                    data: {
                        x: 'x',
                        columns: [
                            rowsdate,
                            rows
                        ],
                        type: 'bar',

                        order: 'desc', // stack order by sum of values descendantly. this is default.
                        //      order: 'asc'  // stack order by sum of values ascendantly.
                        //      order: null   // stack order by data definition.

                        colors: {
                            sites: '#a6aeff'
                        }
                    },
                    zoom: {
                        enabled: true
                    },
                    axis: {
                        y: {
                            show: true,
                        },
                        x: {
                            show: false,
                            type: 'timeseries',
                            tick: {
                                format: '%Y-%m-%d',
                            }
                        }
                    },
                    grid: {
                        y: {
                            lines: [{ value: 0 }]
                        }
                    }
                });
            }
        } */
    });

    $.ajax({
        url: "api/dashboardapi/revisits",
        type: "GET",
        data: countries,
        success: function (res) {
            if ($('#revisits_view').length) {
                var dataperdate = [];
                var countries = [];
                for (var i in res) {
                    var obj = res[i];
                    for (var key in obj) {
                        if (dataperdate[i] == null) {
                            dataperdate[i] = [];
                        }
                        try {
                            dataperdate[i][key] == null ? dataperdate[i][key] = parseInt(obj[key]) : dataperdate[i][key] += parseInt(obj[key]);
                            countries[key] = [];
                            countries[key].push(key);
                        }
                        catch (err) {
                        }
                    }
                    var i = 1;
                    var datesvaluecountries = ['x'];
                    for (var x in dataperdate) {
                        datesvaluecountries.push(x);
                        for (var y in dataperdate[x]) {
                            countries[y].push(dataperdate[x][y]);
                        }
                        for (var n in countries) {
                            if (countries[n][i] == null) {
                                countries[n].push(0);
                            }
                        }
                        i++;

                    }
                    var data = [];
                    var dataheader = [];

                    for (var x in countries) {
                        dataheader.push(countries[x][0]);
                        data.push(countries[x]);
                    }
                    data.push(datesvaluecountries);

                    var chart = c3.generate({
                        bindto: "#revisits_view",
                        data: {
                            x: 'x',
                            columns: data,
                            type: 'bar',
                            groups: [
                                dataheader
                            ]
                        },

                        zoom: {
                            enabled: true
                        },
                        axis: {
                            y: {
                                show: true,
                            },
                            x: {
                                show: false,
                                type: 'timeseries',
                                tick: {
                                    format: '%Y-%m-%d',
                                }
                            }
                        }
                    });
                }
            }
        }
    });



    $.ajax({
        url: "api/dashboardapi/imkfunctions",
        type: "GET",
        data: query,
        success: function (res) {
            datarows = [];
            if ($('.tech_used').length) {
                for (var i in res[0]) {
                    datarows.push(res[0][i]);
                }
                new Chartist.Bar('.tech_used', {

                    labels: ['FRU Status','FRU State','FRU Serial','FRU Prod No','RET Serial','TMA','RET Antenna','VSWR','CPRI','Transport','Transport Routes','Transport Interfaces',
                    'MME Status','GSM-TRX', 'GSM-State','SGW-Status','Traffic-3G','Traffic-4G','Traffic-5G','RSSI UMTS','RSSI-LTE FDD','RSSI-LTE TDD','RSSI-NR','External Alarm', 'Alarm'],
                    series: [
                        datarows
                    ]
                }, {
                    color: "#a6aeff",
                    seriesBarDistance: 10,
                    reverseData: true,
                    horizontalBars: true,
                    axisY: {
                        offset: 150,
                    },
                });
            }
        }
    });


    $.ajax({
        url: "api/dashboardapi/countryview",
        type: "GET",
        data: query,
   
        success: function (res) {
            if ($('#countries_view').length) {
                var dataperdate = [];
                var countries = [];
                for (var i in res) {
                    var obj = res[i];
                    for (var key in obj) {
                        if (dataperdate[i] == null) {
                            dataperdate[i] = [];
                        }
                        try {
                            dataperdate[i][key] == null ? dataperdate[i][key] = parseInt(obj[key]) : dataperdate[i][key] += parseInt(obj[key]);
                            countries[key] = [];
                            countries[key].push(key);
                        }
                        catch (err) {
                        }
                    }
                    var i = 1;
                    var datesvaluecountries = ['x'];
                    for (var x in dataperdate) {
                        datesvaluecountries.push(x);
                        for (var y in dataperdate[x]) {
                            countries[y].push(dataperdate[x][y]);
                        }
                        for (var n in countries) {
                            if (countries[n][i] == null) {
                                countries[n].push(0);
                            }
                        }
                        i++;

                    }
                    var data = [];
                    var dataheader = [];

                    for (var x in countries) {
                        dataheader.push(countries[x][0]);
                        data.push(countries[x]);
                    }
                    data.push(datesvaluecountries);

                    var chart = c3.generate({
                        bindto: "#countries_view",
                        data: {
                            x: 'x',
                            columns: data,
                            type: 'bar',
                            groups: [
                                dataheader
                            ]
                        },

                        zoom: {
                            enabled: true
                        },
                        axis: {
                            y: {
                                show: true,
                            },
                            x: {
                                show: false,
                                type: 'timeseries',
                                tick: {
                                    format: '%Y-%m-%d',
                                }
                            }
                        }
                    });
                }
            }
        }
    });


    $.ajax({
        url: "api/dashboardapi/topasp",
        type: "GET",
        data: query,
        success: function (res) {
            if ($('.top10').length) {
                var datarows = [];
                var lables = [];
                for (var i in res) {
                    lables.push(res[i]["name"])
                    datarows.push(res[i]["sites"])
                }

                new Chartist.Bar('.top10', {

                    labels: lables,
                    series: [
                        datarows
                    ]
                }, {
                    color: "#a6aeff",
                    seriesBarDistance: 10,
                    reverseData: true,
                    horizontalBars: true,
                    axisY: {
                        offset: 150,
                    },
                    axisX: {
                        labelInterpolationFnc: function (value) {
                            return (Math.floor(value));
                        }
                    }
                });
            }
        }
    });


    $.ajax({
        url: "api/dashboardapi/appversion",
        type: "GET",
        data: query,
        success: function (res) {
            if ($('#ANDROIDchart_donut').length) {
                var imkVdata = [];
                for (var i = 0; i < res.length; i++) {
                    var temp = [];
                    temp[0] = res[i]["appVersion"];
                    temp[1] = res[i]["usage"];
                    imkVdata[i] = temp;
                }
                var chart = c3.generate({
                    bindto: "#ANDROIDchart_donut",
                    data: {
                        columns: imkVdata,
                        type: 'donut',
                        onmouseover: function (d, i) { },
                        onmouseout: function (d, i) { },
                    },
                    donut: {
                        title: res.length + " versions "
                    }
                });
            }
        }

    });

    $.ajax({
        url: "api/dashboardapi/rpversion",
        type: "GET",
        data: query,
        success: function (res) {
            if ($('#IMKchart_donut').length) {
                var androidVdata = [];
                for (var i = 0; i < res.length; i++) {
                    var temp = [];
                    temp[0] = res[i]["rpiVersion"];
                    temp[1] = res[i]["usage"];
                    androidVdata[i] = temp;
                }
                var chart = c3.generate({
                    bindto: "#IMKchart_donut",
                    data: {
                        columns: androidVdata,
                        type: 'donut',
                        onmouseover: function (d, i) { },
                        onmouseout: function (d, i) { },
                    },
                    donut: {
                        title: res.length + " versions "

                    }
                });
            }
        }
    });

    $.ajax({
        url: "api/dashboardapi/site_details",
        type: "GET",
        data: query,
        success: function (res) {
            var dataTableContent = '<div><button type="button" onclick="downloadCSV()" class="btn btn-primary">Download CSV</button></div><div class="table-responsive"><table  style="font-size: 10px !important" id="example" class="table table-striped table-bordered second  table-dark" style="width:100%"><thead><tr><th>Site Name</th><th>Country</th><th>Field Engineer</th><th>Android Versions</th><th>IMK Versions</th><th>ASP</th><th>Date</th><th>Contact Info</th></tr></thead><tbody>';
            for (var i = 0; i < res.length; i++) {
                dataTableContent += '<tr><td>' + res[i]["siteName"] + '</td><td>' + res[i]["country"] + '</td><td>' + res[i]["user"] + '</td><td>' + res[i]["androidVersion"] + '</td><td>' + res[i]["rpVersion"] + '</td><td>' + res[i]["asp"] + '</td><td>' + res[i]["date"] + '</td><td> <div onclick=\'httpPostAsync("' + res[i]["contact"] + '")\' ><i class="fas fa-address-card"></i> Click to get Email </div></td></tr></div>'
            }
            dataTableContent += '</tbody><tfoot><tr><th>Site Name</th><th>Country</th><th>Field Engineer</th><th>Android Versions</th><th>IMK Versions</th><th>ASP</th><th>Date</th><th>Contact Info</th></tr></tfoot></table><div class="pagination" id="nav"></div>'
            document.getElementById('dataTable').innerHTML = dataTableContent;
            setTimeout(paginator({
                table: document.getElementById("example"),
                box: document.getElementById("nav"),
                box_mode: "list"
            }), 10000);
        }
    });

}

function downloadCSVASP(aspname) {
    console.log('###' + "https://ericssonquiz.com/dashboard-backend/get.php?type=aspinfo&" + localStorage['QQ'] + '&aspname=' + aspname.trim().split(' ').join('%20'))
    httpGetAsync("https://ericssonquiz.com/dashboard-backend/get.php?type=aspinfo&" + localStorage['QQ'] + '&aspname=' + aspname.trim().split(' ').join('%20'), function (resp) {
        const json = resp;
        const obj = JSON.parse(json);
        var data = []
        data.push(['Site Name', 'Country', 'Field Engineer', 'Operator', 'ASP', 'Date', 'Android Versiom', 'IMK Version'])
        console.log(obj);
        for (var i = 0; i < obj.data.length; i++) {
            data.push([obj.data[i][0], obj.data[i][1], obj.data[i][2], obj.data[i][3], obj.data[i][6], obj.data[i][7], obj.data[i][5], obj.data[i][4]]);
        }
        exportToCsv("IMK_Dashboard.csv", data)
    })
}


function downloadCSV() {
    var data = []
    data.push(['Site Name', 'Country', 'Field Engineer', 'Operator', 'ASP', 'Date', 'Android Versiom', 'IMK Version'])

    for (var i = 0; i < dataTableData.data.length; i++) {
        data.push([dataTableData.data[i][0], dataTableData.data[i][1], dataTableData.data[i][2], dataTableData.data[i][3], dataTableData.data[i][5], dataTableData.data[i][4], dataTableData.data[i][6], dataTableData.data[i][7]])
    }
    exportToCsv("IMK_Dashboard.csv", data)

    //     let csvContent = "data:text/csv;charset=utf-8,";
    //     let datacsv="";
    //     data.forEach(function(rowArray) {
    //         let row = rowArray.join(",");
    //         csvContent += row + "\r\n";
    //         datacsv+= row+ "\r\n";
    //     })    

    //   if (navigator.appVersion.toString().indexOf('.NET') > 0) {
    //         // var blob = new Blob([data],"data:text/csv;charset=utf-8,");
    //         window.navigator.msSaveOrOpenBlob(csvContent, "IMK_Dashboard.csv");

    //     } else {
    //     var encodedUri = encodeURI(csvContent);
    //     var link = document.createElement("a");
    //     link.setAttribute("href", encodedUri);
    //     link.setAttribute("download", "IMK_Dashboard.csv");
    //     document.body.appendChild(link);
    //     link.click();

    //     }


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

function paginator(config) {
    // throw errors if insufficient parameters were given
    if (typeof config != "object")
        throw "Paginator was expecting a config object!";
    if (typeof config.get_rows != "function" && !(config.table instanceof Element))
        throw "Paginator was expecting a table or get_row function!";

    // get/set if things are disabled
    if (typeof config.disable == "undefined") {
        config.disable = false;
    }

    // get/make an element for storing the page numbers in
    var box;
    if (!(config.box instanceof Element)) {
        config.box = document.createElement("div");
    }
    box = config.box;

    // get/make function for getting table's rows
    if (typeof config.get_rows != "function") {
        config.get_rows = function () {
            var table = config.table
            var tbody = table.getElementsByTagName("tbody")[0] || table;

            // get all the possible rows for paging
            // exclude any rows that are just headers or empty
            children = tbody.children;
            var trs = [];
            for (var i = 0; i < children.length; i++) {
                if (children[i].nodeType = "tr") {
                    if (children[i].getElementsByTagName("td").length > 0) {
                        trs.push(children[i]);
                    }
                }
            }

            return trs;
        }
    }
    var get_rows = config.get_rows;
    var trs = get_rows();

    // get/set rows per page
    if (typeof config.rows_per_page == "undefined") {
        var selects = box.getElementsByTagName("select");
        if (typeof selects != "undefined" && (selects.length > 0 && typeof selects[0].selectedIndex != "undefined")) {
            config.rows_per_page = selects[0].options[selects[0].selectedIndex].value;
        } else {
            config.rows_per_page = 25;
        }
    }
    var rows_per_page = config.rows_per_page;

    // get/set current page
    if (typeof config.page == "undefined") {
        config.page = 1;
    }
    var page = config.page;

    // get page count
    var pages = (rows_per_page > 0) ? Math.ceil(trs.length / rows_per_page) : 1;

    // check that page and page count are sensible values
    if (pages < 1) {
        pages = 1;
    }
    if (page > pages) {
        page = pages;
    }
    if (page < 1) {
        page = 1;
    }
    config.page = page;

    // hide rows not on current page and show the rows that are
    for (var i = 0; i < trs.length; i++) {
        if (typeof trs[i]["data-display"] == "undefined") {
            trs[i]["data-display"] = trs[i].style.display || "";
        }
        if (rows_per_page > 0) {
            if (i < page * rows_per_page && i >= (page - 1) * rows_per_page) {
                trs[i].style.display = trs[i]["data-display"];
            } else {
                // Only hide if pagination is not disabled
                if (!config.disable) {
                    trs[i].style.display = "none";
                } else {
                    trs[i].style.display = trs[i]["data-display"];
                }
            }
        } else {
            trs[i].style.display = trs[i]["data-display"];
        }
    }

    // page button maker functions
    config.active_class = config.active_class || "active";
    if (typeof config.box_mode != "function" && config.box_mode != "list" && config.box_mode != "buttons") {
        config.box_mode = "button";
    }
    if (typeof config.box_mode == "function") {
        config.box_mode(config);
    } else {
        var make_button;
        if (config.box_mode == "list") {
            make_button = function (symbol, index, config, disabled, active) {
                var li = document.createElement("li");
                var a = document.createElement("a");
                a.href = "#";
                a.innerHTML = symbol;
                a.className = "page-link"
                a.addEventListener("click", function (event) {
                    event.preventDefault();
                    this.parentNode.click();
                    return false;
                }, false);
                li.appendChild(a);

                var classes = ["paginate_button", "page-item"];
                if (disabled) {
                    classes.push("disabled");
                }
                if (active) {
                    classes.push(config.active_class);
                }
                li.className = classes.join(" ");
                li.addEventListener("click", function () {
                    if (this.className.split(" ").indexOf("disabled") == -1) {
                        config.page = index;
                        paginator(config);
                    }
                }, false);
                return li;
            }
        } else {
            make_button = function (symbol, index, config, disabled, active) {
                var button = document.createElement("button");
                button.innerHTML = symbol;
                button.addEventListener("click", function (event) {
                    event.preventDefault();
                    if (this.disabled != true) {
                        config.page = index;
                        paginator(config);
                    }
                    return false;
                }, false);
                if (disabled) {
                    button.disabled = true;
                }
                if (active) {
                    button.className = config.active_class;
                }
                return button;
            }
        }

        // make page button collection
        var page_box = document.createElement(config.box_mode == "list" ? "ul" : "div");
        if (config.box_mode == "list") {
            page_box.className = "pagination";
        }

        var left = make_button("Previous", (page > 1 ? page - 1 : 1), config, (page == 1), false);
        page_box.appendChild(left);
        for (var i = 1; i <= pages; i++) {
            var li = make_button(i, i, config, false, (page == i));
            if (i % 19 == 0) {
                li.setAttribute("styke", "clear:both;")
                page_box.appendChild(li);
            } else {
                page_box.appendChild(li);
            }
        }

        var right = make_button("Next", (pages > page ? page + 1 : page), config, (page == pages), false);
        page_box.appendChild(right);
        if (box.childNodes.length) {
            while (box.childNodes.length > 1) {
                box.removeChild(box.childNodes[0]);
            }
            box.replaceChild(page_box, box.childNodes[0]);
        } else {
            box.appendChild(page_box);
        }
    }

    // make rows per page selector
    if (!(typeof config.page_options == "boolean" && !config.page_options)) {
        if (typeof config.page_options == "undefined") {
            config.page_options = [
                { value: 25, text: '25' },
                { value: 50, text: '50' },
                { value: 100, text: '100' },
                { value: 0, text: 'All' }
            ];
        }
        var options = config.page_options;
        var div = document.createElement("div");
        var select = document.createElement("select");
        for (var i = 0; i < options.length; i++) {
            var o = document.createElement("option");
            o.value = options[i].value;
            o.text = options[i].text;
            select.appendChild(o);
        }
        select.value = rows_per_page;
        select.addEventListener("change", function () {
            config.rows_per_page = this.value;
            paginator(config);
        }, false);
        div.setAttribute("class", "box")
        div.appendChild(select);
        document.getElementById("nbofrows").innerHTML = ''
        document.getElementById("nbofrows").appendChild(div);

    }

    // status message
    var stat = document.createElement("span");
    stat.setAttribute("style", "margin-left:5px")
    stat.innerHTML = "On page " + page + " of " + pages
        + ", showing rows " + (((page - 1) * rows_per_page) + 1)
        + " to " + (trs.length < page * rows_per_page || rows_per_page == 0 ? trs.length : page * rows_per_page)
        + " of " + trs.length;
    document.getElementById("nbofrows").appendChild(stat);

    // hide pagination if disabled
    if (config.disable) {
        if (typeof box["data-display"] == "undefined") {
            box["data-display"] = box.style.display || "";
        }
        box.style.display = "none";
    } else {
        if (box.style.display == "none") {
            box.style.display = box["data-display"] || "";
        }
    }

    // run tail function
    if (typeof config.tail_call == "function") {
        config.tail_call(config);
    }

    return box;
}



