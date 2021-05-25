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

////////////// Graphs //////////////////

// const datepickers = document.querySelectorAll('.datepicker');

// if (datepickers) {
//   Array.from(datepickers).forEach((datepickerDOM) => {
//     const datepicker = new eds.Datepicker(datepickerDOM);
//     datepicker.init();
//   });
// }

var countriesFilter = [];
var operatorsFilter = [];
var marketArea = '';

function init() {
    marketArea = sessionStorage['marketArea'] != undefined ? sessionStorage['marketArea'] : '';
    countriesFilter = sessionStorage['countries'] != undefined ? sessionStorage['countries'] : [];
    operatorsFilter = sessionStorage['operators'] != undefined ? sessionStorage['operators'] : [];
        
    getCountries(marketArea);
    // getOperators();
    getData(countriesFilter, operatorsFilter);

}


$(function () {
    var start = moment().subtract(29, 'days');
    var end = moment();

    function cb(start, end) {
        $('#reportrange input').attr('placeholder', start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
    }

    $('#reportrange').daterangepicker({
        startDate: start,
        endDate: end,
        ranges: {
            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
            'This Month': [moment().startOf('month'), moment().endOf('month')],
            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
        }
    }, cb);

    cb(start, end);

});

$("#market-areas li").on("click", function () {
    $(this).parent().find("li.active").removeClass("active");
    $(this).addClass("active");
    getCountries($(this).attr('value'));
});

function getCountries(ma) {
    $.ajax({
        url: 'api/dashboardapi/countries',
        type: 'GET',
        data: { marketArea: ma },
        success: function (res) {
            var countries = '';
            for (var i = 0; i < res.length; i++) {
                countries +=
                    '<li>' +
                    '<span class="item" tabindex="0">' +
                    '<input class="country" name=\"country[]\" type="checkbox" onclick="getOperators()" id="c-' + i + '"value="' + res[i] + '">' +
                    '<label for="c-' + i + '">' + res[i] + '</label>' +
                    '</span>' +
                    '</li>'
            }
            document.getElementById('countries').innerHTML = countries;
        }
    });
}


function getOperators() {
    var allcountries = [];
    const checkedCheckboxes = document.querySelectorAll(".country[type='checkbox']:checked");
    checkedCheckboxes.forEach(element => {
        allcountries.push($(element).attr('value'));
    });
    $.ajax({
        url: 'api/dashboardapi/operators',
        type: 'GET',
        data: { countries: decodeURIComponent(allcountries) },
        success: function (res) {
            var operators = '';
            for (var i = 0; i < res.length; i++) {
                for (var j = 0; j < res[i].operators.length; j++) {
                    operators +=
                        '<li>' +
                        '<span class="item" tabindex="0">' +
                        '<input class="operator" name=\"operator[]\" type="checkbox" id="o-' + i+''+j + '"value="' + res[i].operators[j]["name"] + '">' +
                        '<label for="o-' + i +''+j+ '">' + res[i].operators[j]["name"] + '</label>' +
                        '</span>' +
                        '</li>'
                }
            }
            document.getElementById('operators').innerHTML = operators;
        }
    });

}


function filter() {

    var c = [];
    var o = [];
    const checkedCountries = document.querySelectorAll(".country[type='checkbox']:checked");
    const checkedOperators = document.querySelectorAll(".operator[type='checkbox']:checked");

    checkedCountries.forEach(element => {
        c.push($(element).attr('value'));
    });

    checkedOperators.forEach(element => {
        o.push($(element).attr('value'));
    });

    sessionStorage['countries'] = c;
    sessionStorage['operators'] = o;

    getData(c, o);

}

function getData(countries, operators) {

    var Data = { countries: decodeURIComponent(countries), operators: decodeURIComponent(operators) }
    $.ajax({
        url: "api/dashboardapi/unique_sites",
        type: "GET",
        data: Data,
        success: function (res) {
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: document.getElementById('unique-sites'),
                data: {
                    "common": data[0],
                    "series": data[1]
                },
                x: { unit: 'Sites' },
            });

            chart1.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/countryview",
        type: "GET",
        data: Data,
        success: function (res) {
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: document.getElementById('unique-nodes'),
                data: {
                    "common": data[0],
                    "series": data[1]
                },
                x: { unit: 'Nodes' },
            });

            chart1.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/revisits",
        type: "GET",
        data: Data,
        success: function (res) {
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: document.getElementById('site-revisits'),
                data: {
                    "common": data[0],
                    "series": data[1]
                },
                x: { unit: 'Revisits' },
            });

            chart1.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/imkfunctions",
        type: "GET",
        data: Data,
        success: function (res) {
            var functions = [];

            for (var i in res[0]) {
                functions.push(res[0][i]);
            }
            const chart = new eds.HorizontalBarChart({
                element: document.getElementById('imk-functions'),
                data: {
                    "common": ['FRU Status', 'FRU State', 'FRU Serial', 'FRU Prod No', 'RET Serial', 'TMA', 'RET Antenna', 'VSWR', 'CPRI', 'Transport', 'Transport Routes', 'Transport Interfaces',
                        'MME Status', 'GSM-TRX', 'GSM-State', 'SGW-Status', 'Traffic-3G', 'Traffic-4G', 'Traffic-5G', 'RSSI UMTS', 'RSSI-LTE FDD', 'RSSI-LTE TDD', 'RSSI-NR', 'External Alarm', 'Alarm'],
                    series: [{ "name": "Functions", "values": functions }],
                },
                x: { unit: 'Total' },
                thresholds: [
                    {
                        "moreThan": 1,
                        "color": "green"
                    },
                ]
            });

            chart.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/topasp",
        type: "GET",
        data: Data,
        success: function (res) {
            var names = [];
            var sites = [];

            for (var i in res) {
                names.push(res[i]["name"])
                sites.push(res[i]["sites"]);
            }
            // function openE2E(title) {
            //     console.log(title)

            //     const notification = new eds.Notification({
            //         title: title,
            //         description: 'jhg',
            //     });
            //     notification.init();
            // }

            const chart = new eds.HorizontalBarChart({
                element: document.getElementById('top-asp'),
                data: {
                    "common": names,
                    "series": [{ "name": "Top Asp", "values": sites }]
                },
                x: { unit: 'Sites' },
                // onSelect: common => openE2E(common),
            });

            chart.init()
        }
    });

    $.ajax({
        url: "api/dashboardapi/appversion",
        type: "GET",
        data: Data,
        success: function (res) {
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: document.getElementById('app-version'),
                data: {
                    "series": data
                },
                showValue: false,
                unit: 'Versions'
            });
            chart.init()
        }
    });


    $.ajax({
        url: "api/dashboardapi/rpversion",
        type: "GET",
        data: Data,
        success: function (res) {
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: document.getElementById('imk-version'),
                data: {
                    "series": data
                },
                showValue: false,
                unit: 'Versions',
                width: 400
            });
            chart.init()
        }
    });

    $.ajax({
        url: "api/dashboardapi/site_details",
        type: "GET",
        data: Data,
        success: function (res) {
            const tableDOM = document.querySelector('#site-details');
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'siteName',
                        title: 'Site Name',
                    },
                    {
                        key: 'country',
                        title: 'Country',
                    },
                    {
                        key: 'user',
                        title: 'Field Engineer'
                    },
                    {
                        key: 'appVersion',
                        title: 'Android Version',
                    },
                    {
                        key: 'rpiVersion',
                        title: 'IMK Version',
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                    },
                    {
                        key: 'date',
                        title: 'Date',
                    },
                ]
            });
            table.init();
        }
    });


}

function mapData(result) {
    var dates = [];
    var countries = [];
    var unique_sites = [];
    var data = [];
    var values = [];

    for (var i in result) {
        dates.push(i);
        values[i] = [];
        for (var j in result[i]) {
            values[i].push(j);
            countries[j] = [];
            countries[j].push(j);
        }
    }
    for (var c in countries) {
        unique_sites[c] = [];

        for (var i in result) {
            if (values[i].includes(c))

                unique_sites[c].push(result[i][c]);
            else
                unique_sites[c].push(0);
        }
    }
    for (var i in unique_sites) {
        data.push({
            "name": i,
            "values": unique_sites[i]
        })
    }
    if (dates.length == 0 && data.length == 0) {
        dates.push("There are None");
        data.push({
            "name": "",
            "values": [0]
        })
    }
    return [dates, data];

}

///////////////////////////////////////////////




