
const treeDOM = document.querySelector('.tree.navigation');
const tree = new eds.Tree(treeDOM);
tree.init();

const layout = new eds.Layout(document.querySelector('body'));
layout.init();

// Layout & Page
const bodyDOM = document.querySelector('body');
const page = new eds.Page(bodyDOM);
page.init();

const selects = document.querySelectorAll('.select');

if (selects) {
    Array.from(selects).forEach((selectDOM) => {
        const select = new eds.Select(selectDOM);
        select.init();
    });
}

eds.NotificationLog.init();

const multiPanelTile = new eds.MultiPanelTile(document.querySelector('.multi-panel-tile'));



////////////// Graphs //////////////////

var showDetails = false;
var countriesFilter = [];
var operatorsFilter = [];
var marketArea = '';
var datestart = null;
var dateend = null;


function init() {
    daterange();
    marketArea = sessionStorage['marketArea'] != undefined ? sessionStorage['marketArea'] : '';
    countriesFilter = sessionStorage['countries'] != undefined ? sessionStorage['countries'] : "all";
    operatorsFilter = sessionStorage['operators'] != undefined ? sessionStorage['operators'] : [];
    datestart = moment().subtract(6, 'days');
    dateend = moment();

    getCountries(marketArea);
    // dateFilter();
    initMap(datestart.format('YYYY-MM-DD'), dateend.format('YYYY-MM-DD'), marketArea);
    getData(datestart.format('YYYY-MM-DD'), dateend.format('YYYY-MM-DD'), countriesFilter, operatorsFilter, marketArea);

}
function daterange() {
    $(function () {
        var start = moment().subtract(6, 'days');
        var end = moment();
        function cb(start, end) {
            $('#reportrange input').attr('placeholder', start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
            $('#start').attr('value', start.format('YYYY-MM-DD'));
            $('#end').attr('value', end.format('YYYY-MM-DD'));
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
}

document.getElementById("details-rows").style.visibility = showDetails ? 'visible' : 'hidden';
document.getElementById("loading-details").style.visibility = 'hidden';


$('#filter').on('submit', function (e) {
    e.preventDefault();
    // dateFilter();
    var c = [];
    var o = [];
    const checkedCountries = document.querySelectorAll(".country[type='checkbox']:checked");
    const checkedOperators = document.querySelectorAll(".operator[type='checkbox']:checked");
    var ma = $('#market-areas li.active').attr('value');
    var s = $('#start').attr('value');
    var e = $('#end').attr('value');

    checkedCountries.forEach(element => {
        c.push($(element).attr('value'));
    });

    checkedOperators.forEach(element => {
        o.push($(element).attr('value'));
    });

    sessionStorage['countries'] = c;
    sessionStorage['operators'] = o;
    sessionStorage['start'] = s;
    sessionStorage['end'] = e;

    filter(s, e, ma, c, o);

    document.getElementById("pass-fail").style.display = 'block';
    document.getElementById("resolved").style.display = 'none';

    $('#select-command').text("Passed / Failed");
    $('.item.command.active').removeClass('active');
    $('#default-passfail').addClass('active');
});


$('#filter').on('reset', function (e) {
    e.preventDefault();
    var c = "all";
    var o = [];
    var ma = '';
    var s = (moment().subtract(7, 'days')).format('YYYY-MM-DD');
    var e = (moment()).format('YYYY-MM-DD');

    $('.country[type="checkbox"]').each(function () {
        this.checked = true;
    });
    $('.operator[type="checkbox"]').each(function () {
        this.checked = false;
    });
    $('#market-areas').find("li.active").removeClass("active");
    $('#market-areas li[value=""]').addClass("active");

    sessionStorage['countries'] = c;
    sessionStorage['operators'] = o;
    sessionStorage['start'] = s;
    sessionStorage['end'] = e;
    sessionStorage['marketArea'] = ma;
    daterange();
    filter(s, e, ma, c, o);

    document.getElementById("pass-fail").style.display = 'block';
    document.getElementById("resolved").style.display = 'none';

    $('#select-command').text("Passed / Failed");
    $('.item.command.active').removeClass('active');
    $('#default-passfail').addClass('active');
});



$("#market-areas li").on("click", function () {
    $(this).parent().find("li.active").removeClass("active");
    $(this).addClass("active");
    sessionStorage['marketArea'] = $(this).attr('value');
    getCountries($(this).attr('value'));
});

$("#get-alarms-data-details").on("click", function () {
    var c = [];
    var o = [];
    const checkedCountries = document.querySelectorAll(".country[type='checkbox']:checked");
    const checkedOperators = document.querySelectorAll(".operator[type='checkbox']:checked");
    var ma = $('#market-areas li.active').attr('value');
    var s = $('#start').attr('value');
    var e = $('#end').attr('value');

    checkedCountries.forEach(element => {
        c.push($(element).attr('value'));
    });

    checkedOperators.forEach(element => {
        o.push($(element).attr('value'));
    });
    sessionStorage['countries'] = c;
    sessionStorage['operators'] = o;
    sessionStorage['start'] = s;
    sessionStorage['end'] = e;

    // var selectedCountry = $('.item.country.active')[0].innerHTML;
    var startDate = $('#start').attr('value');
    var endDate = $('#end').attr('value');
    marketArea = sessionStorage['marketArea'] != undefined ? sessionStorage['marketArea'] : '';
    countriesFilter = sessionStorage['countries'] != undefined ? sessionStorage['countries'] : "all";
    operatorsFilter = sessionStorage['operators'] != undefined ? sessionStorage['operators'] : [];
    var countriesList = countriesFilter.split(',');

    if (countriesList?.length > 1) {
        const notification = new eds.Notification({
            title: 'Please selecet only one country to see the details',
        });
        notification.init();
        return;
    }
    document.getElementById("loading-details").style.visibility = 'visible';

    getAlarms(startDate, endDate, countriesFilter, operatorsFilter);
    initMap(s, e, ma);

});

function getCountries(ma) {
    $.ajax({
        url: 'api/dashboardapi/countries',
        type: 'GET',
        data: { marketArea: ma },
        success: function (res) {
            var countries = '';
            if (res.length != 0) {
                countries =
                    '<li>' +
                    '<span class="item" tabindex="0">' +
                    '<input class="country" name=\"countries[]\" onclick="selectCountries()" type="checkbox" id="all-countries" value="all" checked>' +
                    '<label for="all-countries">Select All</label>' +
                    '</span>' +
                    '</li>';
            }
            var countriesList = "";
            for (var i = 0; i < res.length; i++) {
                countries +=
                    '<li>' +
                    '<span class="item" tabindex="0">' +
                    '<input class="country" name=\"countries[]\" onclick="checkCountry()" type="checkbox" id="c-' + i + '"value="' + res[i] + '" checked>' +
                    '<label for="c-' + i + '">' + res[i] + '</label>' +
                    '</span>' +
                    '</li>';
                countriesList += `<div class="item country">${res[i]}</div>`;

            }
            document.getElementById('countries').innerHTML = countries;
            /* document.getElementById('countries-alarms').innerHTML = countriesList;
             document.querySelector('#get-countries-alarms').addEventListener('selectOption', (evt) => {
                 var selectedValue = $('.item.country.active')[0].innerHTML;
 
             });*/

            getOperators();
            document.getElementById('all-countries').checked = true;

        }
    });

}


function selectCountries() {
    var all = document.getElementById('all-countries');
    const countries = document.querySelectorAll(".country[type='checkbox']");
    countries.forEach(element => {
        if (all.checked == true)
            element.checked = true;
        if (all.checked == false)
            element.checked = false;
    });
    getOperators();
}

function checkCountry() {
    document.getElementById('all-countries').checked = false;
    getOperators();
}

function selectOperators() {
    var all = document.getElementById('all-operators');
    const operators = document.querySelectorAll(".operator[type='checkbox']");
    operators.forEach(element => {
        if (all.checked == true)
            element.checked = true;
        if (all.checked == false)
            element.checked = false;
    });
}

function checkOperator() {
    document.getElementById('all-operators').checked = false;

}

function getOperators() {
    var allcountries = [];
    const checkedCheckboxes = document.querySelectorAll(".country[type='checkbox']:checked");

    checkedCheckboxes.forEach(element => {
        allcountries.push($(element).attr('value'));
    });
    if (allcountries.length === 0)
        document.getElementById('operators').innerHTML = '';
    else {
        $.ajax({
            url: 'api/dashboardapi/operators',
            type: 'GET',
            data: { countries: decodeURIComponent(allcountries) },
            success: function (res) {
                var operators =
                    '<li>' +
                    '<span class="item" tabindex="0">' +
                    '<input class="operator" name=\"operators[]\" onclick="selectOperators()" type="checkbox" id="all-operators" value="all">' +
                    '<label for="all-operators">Select All</label>' +
                    '</span>' +
                    '</li>'
                    ;
                for (var i = 0; i < res.length; i++) {
                    for (var j = 0; j < res[i].operators.length; j++) {
                        operators +=
                            '<li>' +
                            '<span class="item" tabindex="0">' +
                            '<input class="operator" name=\"operators[]\" onclick="checkOperator()" type="checkbox" id="o-' + i + '' + j + '"value="' + res[i].operators[j]["name"] + '">' +
                            '<label for="o-' + i + '' + j + '">' + res[i].operators[j]["name"] + '</label>' +
                            '</span>' +
                            '</li>'
                    }
                }
                document.getElementById('operators').innerHTML = operators;
            }
        });
    }

}



function filter(s, e, ma, c, o) {
    getData(s, e, c, o, ma);
    initMap(s, e, ma);
}


function getData(startdate, enddate, countries, operators, marketArea) {
    var Data = { start: decodeURIComponent(startdate), end: decodeURIComponent(enddate), countries: decodeURIComponent(countries), operators: decodeURIComponent(operators) }
    showDetails = false;
    document.getElementById("details-rows").style.visibility = showDetails ? 'visible' : 'hidden';

    $.ajax({
        url: "api/dashboardapi/unique_sites",
        type: "GET",
        data: Data,
        success: function (res) {

            console.log(res);
            const element = document.getElementById('unique-sites');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
                data: {
                    "common": data[0],
                    "series": data[1]
                },
                x: { unit: 'Sites' },
            });
            let reportData = [];

            Object.keys(res).forEach(item => {
                let data = res[item];
                console.log(data);
                Object.keys(data).forEach(subItem => {
                    let mappedItem = {
                        country: subItem,
                        numberOfSites: data[subItem],
                        integrationDate: item,
                    }
                    reportData.push(mappedItem);
                })
            })
            document.querySelector('#uniqueNodesDownload')?.addEventListener('click', (e) => {
                console.log(reportData);
                let rows = [];
                rows.push(['Country',
                    'Number Of Sites',
                    'Integration Date',

                ]);
                reportData.forEach(item => {
                    rows.push([item.country, item.numberOfSites, item.integrationDate]);
                });

                let today = new Date();
                _exportToCsv("UniquePhysicalSites_report" + today.toISOString().slice(0, 16), rows);
            })
            chart1.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/countryview",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('unique-nodes');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
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
            const element = document.getElementById('site-revisits');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
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
            const element = document.getElementById('imk-functions');
            element.innerHTML = '';
            var functions = [];

            if (res == null)
                functions.push(0);
            else {
                for (var i in res[0]) {
                    if (i != 'sgwStatus')
                        functions.push(res[0][i]);
                }
            }
            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": ['FRU Status', 'FRU State', 'FRU Serial', 'FRU Prod No', 'RET Serial', 'TMA', 'RET Antenna', 'VSWR', 'CPRI', 'Transport', 'Transport Routes', 'Transport Interfaces',
                        'MME Status', 'GSM-TRX', 'GSM-State', 'Traffic-3G', 'Traffic-4G', 'Traffic-5G', 'RSSI UMTS', 'RSSI-LTE FDD', 'RSSI-LTE TDD', 'RSSI-NR', 'External Alarm', 'Alarm'],
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
            const element = document.getElementById('top-asp');
            element.innerHTML = '';
            var names = [];
            var sites = [];

            if (res == null) {
                names.push("None");
                sites.push(0);
            }
            else {
                for (var i in res) {
                    names.push(res[i]["name"])
                    sites.push(res[i]["sites"]);
                }
            }

            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": names,
                    "series": [{ "name": "Top Asp", "values": sites }]
                },
                x: { unit: 'Sites' },
                thresholds: [
                    {
                        "moreThan": 1,
                        "color": "orange"
                    },
                ],
                height: 350
            });

            chart.init()
        }
    });

    $.ajax({
        url: "api/dashboardapi/appversion",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('app-version');
            element.innerHTML = '';
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: element,
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
            const element = document.getElementById('imk-version');
            element.innerHTML = '';
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: element,
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



    //////////////// Alarm Best / Worst Countries ///////////////////
    /* $.ajax({
         url: "api/dashboardapi/resolved-countries",
         type: "GET",
         data: { start: startdate, end: enddate, marketArea: marketArea },
         success: function (res) {
             $('#top-list').empty();
             $('#worst-list').empty();
 
             var top = res?.filter(item => item.value)?.slice(0, 3);
             var worst = res?.filter(item => item.value)?.slice(-3).reverse();
 
             top.forEach(element => {
                 var html = '<li class="entry"><div class="target"><h4 class="title">' +
                     element["key"] +
                     '</h4><div class="content">' +
                     element["value"] +
                     ' mins</div></div>';
                 $('#top-list').append(html)
             });
             worst.forEach(element => {
                 var html = '<li class="entry"><div class="target"><h4 class="title">' +
                     element["key"] +
                     '</h4><div class="content">' +
                     element["value"] +
                     ' mins</div></div>';
                 $('#worst-list').append(html)
             });
         }
     });*/

    ////////////////// Top Revisits //////////////////
    $.ajax({
        url: "api/dashboardapi/top-revisits",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('top-revisits');
            element.innerHTML = '';
            var sites = [];
            var revisits = [];

            for (var i in res) {
                for (j in res[i]) {
                    sites.push(i + " - " + j);
                    revisits.push(res[i][j]);
                }
            }
            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": sites,
                    series: [{ "name": "Revisits", "values": revisits }],
                },
                x: { unit: 'Total' },
                thresholds: [
                    {
                        "moreThan": 1,
                        "color": "yellow"
                    },
                ],
                height: 350
            });

            chart.init();
        }
    });


    ////////////////// Total Pass - Fail ///////////////////
    $.ajax({
        url: "api/dashboardapi/commands",
        type: "GET",
        data: Data,
        success: function (res) {

            $('#total-pf').empty()

            var passed = res["passed"];
            var failed = res["failed"];

            var total_passed = {
                "VSWR": passed["vswr"] ? passed["vswr"] : 0,
                "RSSI UMTS": passed["umts"] ? passed["umts"] : 0,
                "RSSI-LTE FDD": passed["fdd"] ? passed["fdd"] : 0,
                "RSSI-LTE TDD": passed["tdd"] ? passed["tdd"] : 0,
                "RSSI-NR": passed["nr"] ? passed["nr"] : 0,
            };
            var total_failed = {
                "VSWR": failed["vswr"] ? failed["vswr"] : 0,
                "RSSI UMTS": failed["umts"] ? failed["umts"] : 0,
                "RSSI-LTE FDD": failed["fdd"] ? failed["fdd"] : 0,
                "RSSI-LTE TDD": failed["tdd"] ? failed["tdd"] : 0,
                "RSSI-NR": failed["nr"] ? failed["nr"] : 0,
            };

            for (let i = 0; i < 6; i++) {
                let row = $('<tr>');
                row.append($('<td >').html(Object.keys(total_passed)[i]));
                row.append($('<td>').html(Object.values(total_passed)[i]));
                row.append($('<td>').html(Object.values(total_failed)[i]));
                $('#total-pf').append(row);
            }

        }
    });


    $.ajax({
        url: "api/dashboardapi/ratings",
        type: "Get",
        data: Data,
        success: function (res) {
            const cardContainer = document.getElementById('rating-card-container');
            console.log(res);
            const ratingValues = res.map(item => item.rate);
            const mappedData = res.map(item => {
                const element = {
                    ...item,
                    userName: item.user.name,
                    email: item.user.email,
                    answers: item.questions?.split(','),
                    date: item.date ? new Date(item.date).toISOString().slice(0, 16) : ""

                };
                return element;
            });
            console.log(mappedData);
            const averageRating = ratingValues.reduce((a, b) => a + b, 0) / ratingValues.length;
            const starPercentageRounded = `${(Math.round(averageRating * 2) * 10)}%`;
            console.log(document.querySelectorAll(".stars-inner"));
            document.querySelectorAll(".stars-inner").forEach(e => {
                e.style.width = ratingValues.length ? starPercentageRounded : "0%";
            });
            $("average-rate-val").text(averageRating);
            var reviewsfield = document.querySelector("#total-reviews");
            if (reviewsfield)
                reviewsfield.innerHTML = mappedData.length == 1 ? 1 + " review" : mappedData.length > 1000 ? mappedData.length / 1000 + "K reviews" : mappedData.length + " reviews";
            const cardsHTML = mappedData.map(e => {
                console.log(e.answers);
                const answers = e.answers.map(a => (a && a.trim() !== "Other") ? `<p style="margin-bottom:0;">${a}</p>` : "")?.toString()?.replace(/,/g, '');
                console.log(answers);

                const _card =
                    `<div class="card" style="margin-top: 0px; margin-bottom: 0px;" >
                          <div class="header">
                            <div class="left">
                              <div class="title">${e.userName} </div>
                              <div class="subtitle">
                        ${e.country ?
                        `<div style="color:gray;">${e.country}</div>` : ""}
                                ${e.date ? `<div>${e.date}</div>` : ""}
                                         <div class="stars-outer">
                                        <div class="stars-inner" style="width:${(Math.round(e.rate * 2) * 10)}%;" > </div>
                                    </div>
                               </div>
                             </div>
                            </div>
                          <div class="content">
                            <div>${answers}</div>
                    ${e.comment ? `<p style="margin-top: 14px; margin-bottom: 0px;">Comments:  ${e.comment} </p>` : ""}
                          </div>
                        </div> `       ;
                return _card;
            });
            cardContainer.innerHTML = cardsHTML;
            const cards = document.querySelectorAll('.card');
            if (cards) {
                Array.from(cards)?.forEach((cardDOM) => {
                    const card = new eds.Card(cardDOM);
                    card.init();
                });
            }
            const rows = [];
            var today = new Date();

            rows.push(['UserName',
                'UserEmail',
                'Country',
                'Rate',
                'RatingDate',
                'Answers',
                'Comments'
            ]);
            mappedData.forEach(e => {
                rows.push([e.userName, e.email, e.country, e.rate, e.date, e.questions, e.comment?.trim()]);
            })
            document.querySelector('#export-ratings')?.addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Ratings data is exported to RatingsReport_' + today.toISOString().slice(0, 16) + '.csv file',
                });
                notification.init();
                _exportToCsv("RatingsReport_" + today.toISOString().slice(0, 16), rows);
            })
        }

    })



}


function getAlarmType(alarm) {
    var allAlarms = alarms;
    console.log(allAlarms);
    console.log(alarm);
    console.log(allAlarms.find(item => item.alarm === alarm)?.type);
    return allAlarms.find(item => item.alarm == alarm)?.type;
}

function fillAverageTable(countries, operators) {
    var thisWeek = function () {
        var tmp = null;
        var passedData = { start: decodeURIComponent(moment().subtract(6, 'days').format('YYYY-MM-DD')), end: decodeURIComponent(moment().format('YYYY-MM-DD')), countries: decodeURIComponent(countries), operators: operators };
        $.ajax({
            url: "api/dashboardapi/resolved",
            type: "GET",
            data: passedData,
            success: function (res) {
                tmp = res["avg_resolution"]["alarm"];
                $('#avg-1').html(tmp == null ? 'N/A' : tmp);
            }
        })
        return tmp;
    }();
    var thisMonth = function () {
        var tmp = null;
        var passedData = { start: decodeURIComponent(moment().startOf('month').format('YYYY-MM-DD')), end: decodeURIComponent(moment().endOf('month').format('YYYY-MM-DD')), countries: decodeURIComponent(countries), operators: operators };
        $.ajax({
            url: "api/dashboardapi/resolved",
            type: "GET",
            data: passedData,
            success: function (res) {
                tmp = res["avg_resolution"]["alarm"];
                $('#avg-2').html(tmp == null ? 'N/A' : tmp);

            }
        })
        return tmp;
    }();
    var lastMonth = function () {
        var tmp = null;
        var passedData = { start: decodeURIComponent(moment().subtract(1, 'month').startOf('month').format('YYYY-MM-DD')), end: decodeURIComponent(moment().subtract(1, 'month').endOf('month').format('YYYY-MM-DD')), countries: decodeURIComponent(countries), operators: operators };
        $.ajax({
            url: "api/dashboardapi/resolved",
            type: "GET",
            data: passedData,
            success: function (res) {
                tmp = res["avg_resolution"]["alarm"];
                $('#avg-3').html(tmp == null ? 'N/A' : tmp);
            }
        })
        return tmp;
    }();
    var earlierMonth = function () {
        var tmp = null;
        var passedData = { start: decodeURIComponent(moment().subtract(2, 'month').startOf('month').format('YYYY-MM-DD')), end: decodeURIComponent(moment().subtract(2, 'month').endOf('month').format('YYYY-MM-DD')), countries: decodeURIComponent(countries), operators: operators };
        $.ajax({
            url: "api/dashboardapi/resolved",
            type: "GET",
            data: passedData,
            success: function (res) {
                tmp = res["avg_resolution"]["alarm"];
                $('#avg-4').html(tmp == null ? 'N/A' : tmp);
            }
        })
        return tmp;
    }();
}

function mapSitesData(data) {
    console.log(data);
    var details = data.map(e => {

        var alarmResult = e.logs?.map(l => {
            var r = [];
            r.push(JSON.parse(l.result)?.Description);
            var mappedLog = {
                ...l,
                result: r?.filter(r => getAlarmType(r) === "Field"),
            };
            return mappedLog;

        })
        var mappedItem = {
            ...e,
            logs: alarmResult
        };
        return mappedItem;
    });
    console.log(details);
}

function getAlarms(startDate, endDate, country, operators) {
    var Data = { start: decodeURIComponent(startDate), end: decodeURIComponent(endDate), countries: decodeURIComponent(country), operators: operators }

    ///////////////////////////new site details//////////////////
    $.ajax({
        url: "api/dashboardapi/site_details_new",
        type: "GET",
        data: Data,
        success: function (res) {
            showDetails = true;
            document.getElementById("details-rows").style.visibility = showDetails ? 'visible' : 'hidden';
            document.getElementById("loading-details").style.visibility = 'hidden';
            const tableData = res.map((e, index) => {
                const el = {
                    ...e,
                    id: index,
                    outcome: e.siteIntegration ? e.siteIntegration.outcome : "",
                    integrationResult: e.siteIntegration,
                    diagnosticStartTime: e.date.slice(0, 16) ?? "",
                    integrationStartTime: e.siteIntegration?.downloadStart.slice(0, 16) ?? "",
                    diagnostic: (e.diagnostic ? {
                        siteVisit: {
                            user: {
                                name: e.user,
                                aspCompany: { name: e.asp },
                                phone: e.phone,
                                email: e.email
                            },
                            rpiVersion: e.rpiVersion,
                            appVersion: e.appVersion,
                            startTime: e.date,
                            //alarmTime: e.alarmTime?.slice(0, 16) || ""



                        }
                    } : null),
                    //resolutionTime: e.alarmClearTime,
                    ftr: (e.ftr == true ? "Yes" : e.ftr == false ? "No" : ""),
                    resolutionTime: e.alarmClearTime?.slice(0, 16) || ""
                };
                return el;
            });
            ////not considiering revisits////
            var ftrCount = tableData?.filter(item => item.ftr?.toUpperCase() == "YES" && !item.isRevisit)?.length;
            var totalCountWithNoRevisit = tableData?.filter(item => !item.isRevisit)?.length;
            var ftrPercentage = Math.round(ftrCount / totalCountWithNoRevisit * 100);
            document.getElementById("ftr-percentage").value = ftrPercentage?.toString();
            document.getElementById("ftr-value").innerHTML = ftrPercentage?.toString() + "%";
            document.getElementById("ftr-label").innerHTML = "FTR Percentage (" + ftrCount + "/" + totalCountWithNoRevisit + ")";

            console.log(tableData);
            const tableDOM = document.querySelector('#site-details-updated');
            tableDOM.innerHTML = '';
            const search = [];
            const columns = [
                {
                    key: 'id',
                    title: '',
                    width: '1%',
                    hideFilter: true,
                    cellStyle: 'visibility: hidden'
                },

                {
                    key: 'country',
                    title: 'Country',
                    sort: 'none',
                    width: '4%'

                },
                {
                    key: 'siteName',
                    title: 'Node Name',
                    sort: 'none',
                    width: '7%'
                },
                {
                    key: "integrationStartTime",
                    title: 'LMT Integration',
                    onCreatedCell: (td, cellData) => {
                        if (cellData) {
                            const row = td.closest('tr');
                            var rowId = row.getElementsByTagName("td")[0].innerHTML;
                            const integration = tableData.find(item => item.id == rowId)?.siteIntegration;
                            td.innerHTML = `<span class="tooltip dotted">${cellData}
                                                <span class="message right">
                                                        <div>ASP: ${integration?.asp}</div>
                                                        <div>Integration Start Time: ${integration?.downloadStart?.slice(0, 16) || ""}</div>
                                                        <div>Integration End Time: ${integration?.integrateEnd?.slice(0, 16) || ""}</div>
                                                        <div>Duration: ${integration?.integrationTime}</div>
                                                        <div>Field Engineer: ${integration?.user}</div>
                                                        <div>App Version: ${integration?.androidVersion}</div>
                                                </span>
                                             </span>`;
                        }
                    },
                    sort: 'none',
                    width: '4%',

                },
                {
                    key: "outcome",
                    title: "Integration Result",
                    sort: 'none',
                    onCreatedCell: (td, cellData) => {
                        const row = td.closest('tr');
                        var rowId = row.getElementsByTagName("td")[0].innerHTML;
                        const integrationResult = tableData.find(item => item.id == rowId)?.integrationResult;
                        if (integrationResult) {
                            const integration = cellData;
                            if (integration?.toUpperCase() === 'SUCCESS')
                                td.innerHTML = `<span class="pill severity-cleared"><b>Success</b></span>`;
                            else if (integration?.toUpperCase() === 'FAILED')
                                td.innerHTML = `<span class="pill"><span class="color-red"><i class="icon icon-alarm-level4"></i></span><b>Fail</b></span>`;
                            else
                                td.innerHTML = `<span class="pill"><span class="color-yellow"><i class="icon icon-alarm-level4"></i></span><b>Incomplete</b></span>`;
                        }
                        else {
                            td.innerHTML = ""
                        }
                    },
                    width: '3%',
                    hideFilter: true,

                },
                {
                    key: 'diagnosticStartTime',
                    title: 'First Site Diagnostics',
                    onCreatedCell: (td, cellData) => {
                        if (cellData) {
                            const row = td.closest('tr');
                            var rowId = row.getElementsByTagName("td")[0].innerHTML;
                            const diagnostic = tableData.find(item => item.id == rowId)?.diagnostic;
                            if (diagnostic) {
                                td.innerHTML = `<span class="tooltip dotted">${cellData}
                                                <span class="message left">
                                                    <div>ASP: ${diagnostic?.siteVisit?.user.aspCompany.name}</div>
                                                    <div>Start Time: ${diagnostic?.siteVisit?.startTime?.slice(0, 16)}</div><div>
                                                    <div>Field Engineer: ${diagnostic?.siteVisit?.user.name}</div>
                                                    <div>Field Engineer Phone: ${diagnostic?.siteVisit?.user.phone}</div>
                                                    <div>Field Engineer Email: ${diagnostic?.siteVisit?.user.email}</div>
                                                    <div>IMK Version: ${diagnostic?.siteVisit?.rpiVersion}</div>
                                                    <div>App Version: ${diagnostic?.siteVisit?.appVersion}</div>
                                                </span>
                                            </span>`;
                            }
                            else {
                                td.innerHTML = ""
                            }
                        }
                    },
                    sort: 'none',
                    width: '4%',

                },
                {
                    key: 'resolutionTime',
                    title: 'Last Site Diagnostic',
                    sort: 'none',
                    width: '5%',
                },
                {
                    key: 'ftr',
                    title: 'FTR',
                    sort: 'none',
                    onCreatedCell: (td, cellData) => {
                        if (cellData == "Yes")
                            td.innerHTML = `<span class="pill" style="background-color: #e6f1eb;">${cellData}</span>`;
                        else if (cellData == "No")
                            td.innerHTML = `<span class="pill" style="background-color: rgb(249, 226, 225);">${cellData}</span>`;
                        else
                            td.innerHTML = "N/A"
                    },
                    hideFilter: true,
                    width: '3%'

                },
                {
                    key: 'isRevisit',
                    title: 'Revisit',
                    sort: 'none',
                    onCreatedCell: (td, cellData) => {
                        if (cellData === true)
                            td.innerHTML = `<span class="color-green"><i class="icon icon-alarm-level4"></i></span>`;
                    },
                    hideFilter: true,
                    width: '3%'

                },

            ]

            columns.forEach(column => {
                const width = "95%";
                search.push(
                    `<th> <input  id="${column.key}FIND" onkeyup="_search(this)" type="text"  style="width: ${width}; display: ${(column.hideFilter ? "none" : null)}; " placeholder="Type to filter"/>
                      </th > `
                )
            });
            const table = new eds.Table(tableDOM, {
                data: tableData,
                columns: columns,
                sortable: true,
                actions: true,
                resize: true,

                onCreatedHead: (thead, headData) => {
                    thead.innerHTML = (thead.innerHTML + search.join(''));
                }
            });
            table.init();

            var today = new Date();

            if (document.querySelector('#export-new-visits-admin') != null) {

                document.querySelector('#export-new-visits-admin')?.addEventListener('click', (e) => {
                    //////////////////////prepare data to export/////////////////////////////
                    const rows = [];
                    rows.push(['Country',
                        'Site Name',
                        'Operator',
                        'Integration Asp Company',
                        'Integration Start',
                        'Integration End',
                        'Integration Duration',
                        'Integration Field Engineer',
                        'Integration App Version',
                        'Integration Result',
                        'Diagnostics Asp Company',
                        'Diagnostics Start Time',
                        'Diagnostics Field Engineer',
                        'Diagnostics Field Engineer Phone',
                        'Diagnostics Field Engineer Email',
                        'Diagnostics IMK Version',
                        'Diagnostics App Version',
                        'Resolution Time',
                        'FTR Status',
                        'Is Revisit'

                    ]);
                    var currentTable = document.getElementById("site-details-updated");
                    var siteNames = [];
                    var trs = currentTable.tBodies[0].getElementsByTagName("tr");

                    for (var i = 0; i < trs.length; i++) {

                        if (trs[i].hidden) {
                            continue;
                        }
                        else {
                            var tds = trs[i].getElementsByTagName("td");
                            siteNames.push(tds[2].innerText)
                        }
                    }

                    var reportData = tableData.filter(item => siteNames.includes(item.siteName));

                    reportData.forEach(e => {
                        rows.push([e.country || "",
                        e.siteName || "",
                        e.operatorName || "",
                        e.siteIntegration?.asp || "",
                        e.siteIntegration?.downloadStart || "",
                        e.siteIntegration?.integrateEnd || "",
                        e.siteIntegration?.integrationTime || "",
                        e.siteIntegration?.user || "",
                        e.siteIntegration?.androidVersion || "",
                        (e.siteIntegration ? e.siteIntegration.outcome ?? "incomplete" : ""),
                        e.diagnostic?.siteVisit?.user.aspCompany.name || "",
                        e.diagnostic?.siteVisit?.startTime?.slice(0, 16) || "",
                        e.diagnostic?.siteVisit?.user?.name || "",
                        e.diagnostic?.siteVisit?.user?.phone || "",
                        e.diagnostic?.siteVisit?.user?.email || "",
                        e.diagnostic?.siteVisit?.rpiVersion || "",
                        e.diagnostic?.siteVisit?.appVersion || "",
                        e.resolutionTime || "",
                        e.ftr || "N/A",
                        e.isRevisit
                        ],
                        );
                    });

                    /////////////////////////////////////////////////////////////////////
                    const notification = new eds.Notification({
                        title: 'Export data',
                        description: 'Site visits data is exported to IMK_Site_visits' + today.toISOString() + '.csv file',
                    });
                    notification.init();
                    _exportToCsv(today.toISOString() + "IMK_Dashboard.csv", rows);

                });
            }
            else if (document.querySelector('#export-new-visits-admin') == null && document.querySelector('#export-new-visits-user') != null) {


                document.querySelector('#export-new-visits-user')?.addEventListener('click', () => {
                    ///////////prepare data to export/////////
                    const rows = [];
                    rows.push(['Country',
                        'Site Name',
                        'Operator',
                        'Integration Asp Company',
                        'Integration Start',
                        'Integration End',
                        'Integration Duration',
                        'Integration Field Engineer',
                        'Integration Android Version',
                        'Integration Result',
                        'Diagnostics Asp Company',
                        'Diagnostics Start Time',
                        'Diagnostics Field Engineer',
                        'Diagnostics IMK Version',
                        'Diagnostics Android Version',
                        'Resolution Time',
                        'FTR Status',
                        'Is Revisit'
                    ]);
                    var currentTable = document.getElementById("site-details-updated");
                    var siteNames = [];
                    var trs = currentTable.tBodies[0].getElementsByTagName("tr");
                    for (var i = 0; i < trs.length; i++) {

                        if (trs[i].hidden) {
                            continue;
                        }
                        else {
                            var tds = trs[i].getElementsByTagName("td");
                            siteNames.push(tds[2].innerText)
                        }
                    }
                    var reportData = tableData.filter(item => siteNames.includes(item.siteName));

                    reportData.forEach(e => {
                        rows.push([e.country || "",
                        e.siteName || "",
                        e.operatorName || "",
                        e.siteIntegration?.asp || "",
                        e.siteIntegration?.downloadStart || "",
                        e.siteIntegration?.downloadEnd || "",
                        e.siteIntegration?.integrationTime || "",
                        e.siteIntegration?.user || "",
                        e.siteIntegration?.androidVersion || "",
                        (e.siteIntegration ? e.siteIntegration.outcome ?? "incomplete" : ""),
                        e.diagnostic?.siteVisit?.user.aspCompany.name || "",
                        e.diagnostic?.siteVisit?.startTime?.slice(0, 16) || "",
                        e.diagnostic?.siteVisit?.user?.name || "",
                        e.diagnostic?.siteVisit?.rpiVersion || "",
                        e.diagnostic?.siteVisit?.appVersion || "",
                        e.resolutionTime || "",
                        e.ftr || "N/A",
                        e.isRevisit
                        ]);
                    });

                    ////////////////////////////////////////////
                    const notification = new eds.Notification({
                        title: 'Export data',
                        description: 'Site visits data is exported to IMK_Site_visits' + today.toISOString() + '.csv file',
                    });
                    notification.init();
                    _exportToCsv(today.toISOString() + "IMK_Dashboard.csv", rows);

                });
            }
        }
    });

    ////////////////// Pass - Fail Analysis ///////////////////
    $.ajax({
        url: "api/dashboardapi/pass-fail",
        type: "GET",
        data: Data,
        success: function (res) {

            ////////////////// Pass / Fail status (per visit)
            const element = document.getElementById('pass-fail');
            var vpassedResult = res[1].value["passed_per_visit"];
            var vfailedResult = res[1].value["failed_per_visit"];
            var resolvedResult = res[1].value["resolved_per_visit"];
            var resolution = res[1].value["avg_resolution"];
            var medians = res[1].value["median_resolution"];

            var alarmpassed = res[0].value["passed_per_visit"];
            var alarmresolved = res[0].value["resolved_per_visit"];
            var alarmfailed = res[0].value["failed_per_visit"];
            var alarmresolution = res[0].value["avg_resolution"];
            var alarmFTR = alarmpassed["alarm"] + alarmresolved["alarm"];

            var passed_per_visit = {
                "VSWR": vpassedResult["vswr"] ? vpassedResult["vswr"] : 0,
                "RSSI UMTS": vpassedResult["rssi_umts"] ? vpassedResult["rssi_umts"] : 0,
                "RSSI-LTE FDD": vpassedResult["rssi-lte EUtranCellFDD"] ? vpassedResult["rssi-lte EUtranCellFDD"] : 0,
                "RSSI-LTE TDD": vpassedResult["rssi-lte EUtranCellTDD"] ? vpassedResult["rssi-lte EUtranCellTDD"] : 0,
                "RSSI-NR": vpassedResult["rssi-nr"] ? vpassedResult["rssi-nr"] : 0,
                "Field Alarm": parseInt(alarmresolved['alarm']) + parseInt(alarmfailed['alarm']) + parseInt(alarmpassed['alarm']) || 0
            }
            var failed_per_visit = {
                "VSWR": vfailedResult["vswr"] ? vfailedResult["vswr"] : 0,
                "RSSI UMTS": vfailedResult["rssi_umts"] ? vfailedResult["rssi_umts"] : 0,
                "RSSI-LTE FDD": vfailedResult["rssi-lte EUtranCellFDD"] ? vfailedResult["rssi-lte EUtranCellFDD"] : 0,
                "RSSI-LTE TDD": vfailedResult["rssi-lte EUtranCellTDD"] ? vfailedResult["rssi-lte EUtranCellTDD"] : 0,
                "RSSI-NR": vfailedResult["rssi-nr"] ? vfailedResult["rssi-nr"] : 0,
                "Field Alarm": alarmfailed["alarm"] ? alarmfailed["alarm"] : 0
            }
            var resolved_per_visit = {
                "VSWR": resolvedResult["vswr"] ? resolvedResult["vswr"] : 0,
                "RSSI UMTS": resolvedResult["rssi_umts"] ? resolvedResult["rssi_umts"] : 0,
                "RSSI-LTE FDD": resolvedResult["rssi-lte EUtranCellFDD"] ? resolvedResult["rssi-lte EUtranCellFDD"] : 0,
                "RSSI-LTE TDD": resolvedResult["rssi-lte EUtranCellTDD"] ? resolvedResult["rssi-lte EUtranCellTDD"] : 0,
                "RSSI-NR": resolvedResult["rssi-nr"] ? resolvedResult["rssi-nr"] : 0,
                "Field Alarm": alarmresolved["alarm"] ? alarmresolved["alarm"] : 0
            }
            var resolution_time = {
                "VSWR": resolution["vswr"] ? resolution["vswr"] : 0,
                "RSSI UMTS": resolution["rssi_umts"] ? resolution["rssi_umts"] : 0,
                "RSSI-LTE FDD": resolution["rssi-lte EUtranCellFDD"] ? resolution["rssi-lte EUtranCellFDD"] : 0,
                "RSSI-LTE TDD": resolution["rssi-lte EUtranCellTDD"] ? resolution["rssi-lte EUtranCellTDD"] : 0,
                "RSSI-NR": resolution["rssi-nr"] ? resolution["rssi-nr"] : 0,
                "Field Alarm": alarmresolution["alarm"] ? alarmresolution["alarm"] : 0
            }
            var medians_time = {
                "VSWR": medians["vswr"] ? medians["vswr"] : 0,
                "RSSI UMTS": medians["rssi_umts"] ? medians["rssi_umts"] : 0,
                "RSSI-LTE FDD": medians["rssi-lte EUtranCellFDD"] ? medians["rssi-lte EUtranCellFDD"] : 0,
                "RSSI-LTE TDD": medians["rssi-lte EUtranCellTDD"] ? medians["rssi-lte EUtranCellTDD"] : 0,
                "RSSI-NR": medians["rssi-nr"] ? medians["rssi-nr"] : 0,
                "Field Alarm": medians["alarm"] ? medians["alarm"] : 0
            }

            element.innerHTML = '';
            const chart = new eds.HorizontalBarChartStacked({
                element: element,
                data: {
                    "common": ["VSWR", "RSSI UMTS", "RSSI-LTE FDD", "RSSI-LTE TDD", "RSSI-NR"
                        //, "Field Alarm"
                    ],
                    "series": [
                        { "name": "Passed FTR", "values": Object.values(passed_per_visit) },
                        { "name": "Failed", "values": Object.values(failed_per_visit) },
                    ]
                },
                x: { unit: 'Nodes' }
            });
            chart.init();

            document.querySelector('#get-command').addEventListener('selectOption', (evt) => {
                var selectedValue = $('.item.command.active')[0].innerHTML;
                if (selectedValue === "Passed / Failed") {
                    document.getElementById("pass-fail").style.display = 'block';
                    document.getElementById("resolved").style.display = 'none';
                }
                else {

                    document.getElementById("pass-fail").style.display = 'none';
                    document.getElementById("resolved").style.display = 'block';

                    $("#command").text(selectedValue);
                    $("#resolved-number").text(resolved_per_visit[selectedValue]);
                    $("#avg-time").text(resolution_time[selectedValue]);
                    //$("#median-time").text(medians_time[selectedValue]);
                    //var total = passed_per_visit[selectedValue] + failed_per_visit[selectedValue] + resolved_per_visit[selectedValue];
                    var total = passed_per_visit[selectedValue] + failed_per_visit[selectedValue];
                    var percentage = (resolved_per_visit[selectedValue] / passed_per_visit[selectedValue]) * 100;
                    $("#total-nodes").text("/ " + passed_per_visit[selectedValue]);
                    $("#progress-bar").val(Math.round(percentage || 0));
                    $("#progress-value").text(Math.round(percentage || 0) + " %");

                }
            });

            ////////// Alarm types analysis
            var alarmsList = [];
            var fieldList = []
            var remoteList = []
            var miscList = []
            var alarmTypes = res[1].value["alarm_types"];
            var fieldAlarms = res[1].value["field_alarms"];
            var remoteAlarms = res[1].value["remote_alarms"];
            var miscAlarms = res[1].value["misc_alarms"];

            for (const [key, value] of Object.entries(fieldAlarms))
                fieldList.push({ "name": key, "values": [value] })
            for (const [key, value] of Object.entries(remoteAlarms))
                remoteList.push({ "name": key, "values": [value] })
            for (const [key, value] of Object.entries(miscAlarms))
                miscList.push({ "name": key, "values": [value] })

            const alarms1 = document.getElementById('field-types');
            const alarms2 = document.getElementById('remote-types');
            const alarms3 = document.getElementById('misc-types');

            alarms1.innerHTML = '';
            const donutChart1 = new eds.Donut({
                element: alarms1,
                data: {
                    "series": fieldList
                },
                unit: 'Types'
            });
            donutChart1.init();

            alarms2.innerHTML = '';
            const donutChart2 = new eds.Donut({
                element: alarms2,
                data: {
                    "series": remoteList
                },
                unit: 'Types'
            });
            donutChart2.init();

            alarms3.innerHTML = '';
            const donutChart3 = new eds.Donut({
                element: alarms3,
                data: {
                    "series": miscList
                },
                unit: 'Types'
            });
            donutChart3.init();
            $('.alarm-types .labels .label').css("font-size", "12px");
            $('.alarm-types .chart-legend').css('display', 'none');

            document.querySelector('#get-alarmtype').addEventListener('selectOption', (evt) => {
                var selectedValue = $('.item.alarmtype.active')[0].innerHTML;
                switch (selectedValue) {
                    case "Field":
                        $('#field-types').css('display', 'block');
                        $('#remote-types').css('display', 'none');
                        $('#misc-types').css('display', 'none');
                        $('#art-table').css('visibility', '');

                        break;
                    case "Remote":
                        $('#field-types').css('display', 'none');
                        $('#remote-types').css('display', 'block');
                        $('#misc-types').css('display', 'none');
                        $('#art-table').css('visibility', 'hidden');
                        break;
                    case "Misc":
                        $('#field-types').css('display', 'none');
                        $('#remote-types').css('display', 'none');
                        $('#misc-types').css('display', 'block');
                        $('#art-table').css('visibility', 'hidden');
                        break;

                }
            });

        }
    });

    fillAverageTable(country, null);

    $.ajax({
        url: "api/dashboardapi/unique_sites",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('unique-sites');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
                data: {
                    "common": data[0],
                    "series": data[1]
                },
                x: { unit: 'Sites' },
            });
            Object.keys(res).forEach(item => {
                let data = res[item];
                console.log(data);
                Object.keys(data).forEach(subItem => {
                    let mappedItem = {
                        country: subItem,
                        numberOfSites: data[subItem],
                        integrationDate: item,
                    }
                    reportData.push(mappedItem);
                })
            })
            document.querySelector('#uniqueNodesDownload')?.addEventListener('click', (e) => {
                console.log(reportData);
                let rows = [];
                rows.push(['Country',
                    'Number Of Sites',
                    'Integration Date',

                ]);
                reportData.forEach(item => {
                    rows.push([item.country, item.numberOfSites, item.integrationDate]);
                });

                let today = new Date();
                _exportToCsv("UniquePhysicalSites_report" + today.toISOString().slice(0, 16), rows);
            })

            chart1.init();
        }
    });

    $.ajax({
        url: "api/dashboardapi/countryview",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('unique-nodes');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
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
            const element = document.getElementById('site-revisits');
            element.innerHTML = '';
            var data = mapData(res)
            const chart1 = new eds.HorizontalBarChartStacked({
                element: element,
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
            const element = document.getElementById('imk-functions');
            element.innerHTML = '';
            var functions = [];

            if (res == null)
                functions.push(0);
            else {
                for (var i in res[0]) {
                    if (i != 'sgwStatus')
                        functions.push(res[0][i]);
                }
            }
            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": ['FRU Status', 'FRU State', 'FRU Serial', 'FRU Prod No', 'RET Serial', 'TMA', 'RET Antenna', 'VSWR', 'CPRI', 'Transport', 'Transport Routes', 'Transport Interfaces',
                        'MME Status', 'GSM-TRX', 'GSM-State', 'Traffic-3G', 'Traffic-4G', 'Traffic-5G', 'RSSI UMTS', 'RSSI-LTE FDD', 'RSSI-LTE TDD', 'RSSI-NR', 'External Alarm', 'Alarm'],
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
            const element = document.getElementById('top-asp');
            element.innerHTML = '';
            var names = [];
            var sites = [];

            if (res == null) {
                names.push("None");
                sites.push(0);
            }
            else {
                for (var i in res) {
                    names.push(res[i]["name"])
                    sites.push(res[i]["sites"]);
                }
            }

            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": names,
                    "series": [{ "name": "Top Asp", "values": sites }]
                },
                height: 350,
                x: { unit: 'Sites' },
                thresholds: [
                    {
                        "moreThan": 1,
                        "color": "orange"
                    },

                ],
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
            const element = document.getElementById('app-version');
            element.innerHTML = '';
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: element,
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
            const element = document.getElementById('imk-version');
            element.innerHTML = '';
            var data = [];
            for (var i in res) {
                res[i]["values"] = [res[i]["values"]]
                data.push(res[i]);
            }
            const chart = new eds.Donut({
                element: element,
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



    //////////////// Alarm Best / Worst Countries ///////////////////
    /* $.ajax({
         url: "api/dashboardapi/resolved-countries",
         type: "GET",
         data: { start: startdate, end: enddate, marketArea: marketArea },
         success: function (res) {
             $('#top-list').empty();
             $('#worst-list').empty();
 
             var top = res?.filter(item => item.value)?.slice(0, 3);
             var worst = res?.filter(item => item.value)?.slice(-3).reverse();
 
             top.forEach(element => {
                 var html = '<li class="entry"><div class="target"><h4 class="title">' +
                     element["key"] +
                     '</h4><div class="content">' +
                     element["value"] +
                     ' mins</div></div>';
                 $('#top-list').append(html)
             });
             worst.forEach(element => {
                 var html = '<li class="entry"><div class="target"><h4 class="title">' +
                     element["key"] +
                     '</h4><div class="content">' +
                     element["value"] +
                     ' mins</div></div>';
                 $('#worst-list').append(html)
             });
         }
     });*/

    ////////////////// Top Revisits //////////////////
    $.ajax({
        url: "api/dashboardapi/top-revisits",
        type: "GET",
        data: Data,
        success: function (res) {
            const element = document.getElementById('top-revisits');
            element.innerHTML = '';
            var sites = [];
            var revisits = [];

            for (var i in res) {
                for (j in res[i]) {
                    sites.push(i + " - " + j);
                    revisits.push(res[i][j]);
                }
            }
            const chart = new eds.HorizontalBarChart({
                element: element,
                data: {
                    "common": sites,
                    series: [{ "name": "Revisits", "values": revisits }],
                },
                x: { unit: 'Total' },
                height: 350,
                thresholds: [
                    {
                        "moreThan": 1,
                        "color": "yellow"
                    },
                ]
            });

            chart.init();
        }
    });


    ////////////////// Total Pass - Fail ///////////////////
    $.ajax({
        url: "api/dashboardapi/commands",
        type: "GET",
        data: Data,
        success: function (res) {

            $('#total-pf').empty()

            var passed = res["passed"];
            var failed = res["failed"];

            var total_passed = {
                "VSWR": passed["vswr"] ? passed["vswr"] : 0,
                "RSSI UMTS": passed["umts"] ? passed["umts"] : 0,
                "RSSI-LTE FDD": passed["fdd"] ? passed["fdd"] : 0,
                "RSSI-LTE TDD": passed["tdd"] ? passed["tdd"] : 0,
                "RSSI-NR": passed["nr"] ? passed["nr"] : 0,
            };
            var total_failed = {
                "VSWR": failed["vswr"] ? failed["vswr"] : 0,
                "RSSI UMTS": failed["umts"] ? failed["umts"] : 0,
                "RSSI-LTE FDD": failed["fdd"] ? failed["fdd"] : 0,
                "RSSI-LTE TDD": failed["tdd"] ? failed["tdd"] : 0,
                "RSSI-NR": failed["nr"] ? failed["nr"] : 0,
            };

            for (let i = 0; i < 6; i++) {
                let row = $('<tr>');
                row.append($('<td >').html(Object.keys(total_passed)[i]));
                row.append($('<td>').html(Object.values(total_passed)[i]));
                row.append($('<td>').html(Object.values(total_failed)[i]));
                $('#total-pf').append(row);
            }

        }
    });


    $.ajax({
        url: "api/dashboardapi/ratings",
        type: "Get",
        data: Data,
        success: function (res) {
            const cardContainer = document.getElementById('rating-card-container');
            console.log(res);
            const ratingValues = res.map(item => item.rate);
            const mappedData = res.map(item => {
                const element = {
                    ...item,
                    userName: item.user.name,
                    email: item.user.email,
                    answers: item.questions?.split(','),
                    date: item.date ? new Date(item.date).toISOString().slice(0, 16) : ""

                };
                return element;
            });
            console.log(mappedData);
            const averageRating = ratingValues.reduce((a, b) => a + b, 0) / ratingValues.length;
            const starPercentageRounded = `${(Math.round(averageRating * 2) * 10)}%`;
            console.log(document.querySelectorAll(".stars-inner"));
            document.querySelectorAll(".stars-inner").forEach(e => {
                e.style.width = ratingValues.length ? starPercentageRounded : "0%";
            });
            $("average-rate-val").text(averageRating);
            var reviewsfield = document.querySelector("#total-reviews");
            if (reviewsfield)
                reviewsfield.innerHTML = mappedData.length == 1 ? 1 + " review" : mappedData.length > 1000 ? mappedData.length / 1000 + "K reviews" : mappedData.length + " reviews";
            const cardsHTML = mappedData.map(e => {
                console.log(e.answers);
                const answers = e.answers.map(a => (a && a.trim() !== "Other") ? `<p style="margin-bottom:0;">${a}</p>` : "")?.toString()?.replace(/,/g, '');
                console.log(answers);

                const _card =
                    `<div class="card" style="margin-top: 0px; margin-bottom: 0px;" >
                          <div class="header">
                            <div class="left">
                              <div class="title">${e.userName} </div>
                              <div class="subtitle">
                        ${e.country ?
                        `<div style="color:gray;">${e.country}</div>` : ""}
                                ${e.date ? `<div>${e.date}</div>` : ""}
                                         <div class="stars-outer">
                                        <div class="stars-inner" style="width:${(Math.round(e.rate * 2) * 10)}%;" > </div>
                                    </div>
                               </div>
                             </div>
                            </div>
                          <div class="content">
                            <div>${answers}</div>
                    ${e.comment ? `<p style="margin-top: 14px; margin-bottom: 0px;">Comments:  ${e.comment} </p>` : ""}
                          </div>
                        </div> `       ;
                return _card;
            });
            cardContainer.innerHTML = cardsHTML;
            const cards = document.querySelectorAll('.card');
            if (cards) {
                Array.from(cards)?.forEach((cardDOM) => {
                    const card = new eds.Card(cardDOM);
                    card.init();
                });
            }
            const rows = [];
            var today = new Date();

            rows.push(['UserName',
                'UserEmail',
                'Country',
                'Rate',
                'RatingDate',
                'Answers',
                'Comments'
            ]);
            mappedData.forEach(e => {
                rows.push([e.userName, e.email, e.country, e.rate, e.date, e.questions, e.comment?.trim()]);
            })
            document.querySelector('#export-ratings')?.addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Ratings data is exported to RatingsReport_' + today.toISOString().slice(0, 16) + '.csv file',
                });
                notification.init();
                _exportToCsv("RatingsReport_" + today.toISOString().slice(0, 16), rows);
            })
        }

    })


}

function search() {

    var input0 = document.getElementById("find-0");
    var input1 = document.getElementById("find-1");
    var input2 = document.getElementById("find-2");
    var input3 = document.getElementById("find-3");
    var input4 = document.getElementById("find-4");
    var input5 = document.getElementById("find-5");
    var input6 = document.getElementById("find-6");

    var table = document.getElementById("site-details");
    var trs = table.tBodies[0].getElementsByTagName("tr");

    for (var i = 0; i < trs.length; i++) {
        var tds = trs[i].getElementsByTagName("td");
        trs[i].style.display = "none";
        if (tds[0].innerHTML.toUpperCase().indexOf(input0.value.toUpperCase()) > -1
            && tds[1].innerHTML.toUpperCase().indexOf(input1.value.toUpperCase()) > -1
            && tds[2].innerHTML.toUpperCase().indexOf(input2.value.toUpperCase()) > -1
            && tds[3].innerHTML.toUpperCase().indexOf(input3.value.toUpperCase()) > -1
            && tds[4].innerHTML.toUpperCase().indexOf(input4.value.toUpperCase()) > -1
            && tds[5].innerHTML.toUpperCase().indexOf(input5.value.toUpperCase()) > -1
            && tds[6].innerHTML.toUpperCase().indexOf(input6.value.toUpperCase()) > -1) {
            trs[i].style.display = "";
            continue;
        }

    }
}

function _search(e) {


    var input0 = document.getElementById("countryFIND");
    var input1 = document.getElementById("siteNameFIND");
    var input2 = document.getElementById("integrationStartTimeFIND");
    var input3 = document.getElementById("diagnosticStartTimeFIND");
    var input4 = document.getElementById("resolutionTimeFIND");
    var table = document.getElementById("site-details-updated");

    var trs = table.tBodies[0].getElementsByTagName("tr");
    for (var i = 0; i < trs.length; i++) {
        var tds = trs[i].getElementsByTagName("td");
        trs[i].style.display = "none";
        trs[i].hidden = true;
        if (tds[1].innerHTML.toUpperCase().indexOf(input0?.value.toUpperCase()) > -1
            && tds[2].innerHTML.toUpperCase().indexOf(input1?.value.toUpperCase()) > -1
            && tds[3].innerHTML.toUpperCase().indexOf(input2?.value.toUpperCase()) > -1
            && tds[5].innerHTML.toUpperCase().indexOf(input3?.value.toUpperCase()) > -1
            && tds[6].innerHTML.toUpperCase().indexOf(input4?.value.toUpperCase()) > -1
        ) {
            trs[i].style.display = "";
            trs[i].hidden = false;

            continue;
        }

    }

}


function showAlarms() {
    var type = document.querySelector('input[name="alarmType"]:checked').value;
    switch (type) {
        case "field":
            $('#field-types').css('display', 'block');
            $('#remote-types').css('display', 'none');
            $('#misc-types').css('display', 'none');
            break;
        case "remote":
            $('#field-types').css('display', 'none');
            $('#remote-types').css('display', 'block');
            $('#misc-types').css('display', 'none');
            break;
        case "misc":
            $('#field-types').css('display', 'none');
            $('#remote-types').css('display', 'none');
            $('#misc-types').css('display', 'block');
            break;

    }
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
    countries.sort();
    for (var c in countries) {
        unique_sites[c] = [];

        for (var i in result) {
            if (values[i].includes(c))

                unique_sites[c].push(result[i][c]);
            else
                unique_sites[c].push(0);
        }
    }
    for (var i in sortObjectByKeys(unique_sites)) {
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
function sortObjectByKeys(o) {
    return Object.keys(o).sort().reduce((r, k) => (r[k] = o[k], r), {});
}

function exportToCsv(filename, rows) {
    var processRow = function (row) {
        var finalVal = '';
        for (var j = 0; j < row.length; j++) {
            var innerValue = row[j] === null ? '' : row[j]?.toString();
            if (row[j] instanceof Date) {
                innerValue = row[j]?.toLocaleString();
            };
            var result = innerValue?.replace(/"/g, '""');
            if (result?.search(/("|,|\n)/g) >= 0)
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
function _exportToCsv(filename, rows) {
    var _result = [];
    rows.forEach(row => {
        var finalVal = '';
        for (var j = 0; j < row.length; j++) {
            var innerValue = row[j] === null ? '' : row[j]?.toString();
            if (row[j] instanceof Date) {
                innerValue = row[j]?.toLocaleString();
            };
            var result = innerValue?.replace(/"/g, '""');
            if (result?.search(/("|,|\n)/g) >= 0)
                result = '"' + result + '"';
            if (j > 0)
                finalVal += ',';
            finalVal += result;
        }
        _result.push(finalVal + '\n');
    });
    if (_result.length > 1) {
        var csvFile = "";
        _result.forEach(e => {
            csvFile += e.toLocaleString();
        });
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

}

function hideLegend(tile) {
    if ($('#' + tile + ' .chart-legend').css('display') === "block")
        $('#' + tile + ' .chart-legend').css('display', 'none');
    else
        $('#' + tile + ' .chart-legend').css('display', 'block');
}

function toggleVersions(version) {
    if (version == "imk") {
        $('#imk-version').css('display', 'block');
        $('#app-version').css('display', 'none');
        $("#switch-versions").val("app");
        $("#switch-versions").html('<i class="icon icon-signal"></i>IMK');
        $("#app-imk-version").html("IMK");
    }
    else if (version == "app") {
        $('#imk-version').css('display', 'none');
        $('#app-version').css('display', 'block');
        $("#switch-versions").val("imk");
        $("#switch-versions").html('<i class="icon icon-mobile-devices"></i>App');
        $("#app-imk-version").html("App");
    }

}

/*function toggleTopCountries(category) {
    if (category == "top") {
        $('#top-list').css('display', 'block');
        $('#worst-list').css('display', 'none');
        $("#switch-list").val("worst");
        $("#switch-list").html('Best Performers');
    }
    else if (category == "worst") {
        $('#top-list').css('display', 'none');
        $('#worst-list').css('display', 'block');
        $("#switch-list").val("top");
        $("#switch-list").html('Worst Performers');
    }

}*/



/////////////////////////////////////////////// GLOBES 
var chart = am4core.create(document.getElementById("chartdiv"), am4maps.MapChart);
var chart2 = am4core.create(document.getElementById("chartdiv2"), am4maps.MapChart);
var chart3 = am4core.create(document.getElementById("chartdiv3"), am4maps.MapChart);
var chart4 = am4core.create(document.getElementById("chartdiv4"), am4maps.MapChart);
var isoCountries = isoCountries = {
    'Afghanistan': 'AF',
    'Aland Islands': 'AX',
    'Albania': 'AL',
    'Algeria': 'DZ',
    'American Samoa': 'AS',
    'Andorra': 'AD',
    'Angola': 'AO',
    'Anguilla': 'AI',
    'Antarctica': 'AQ',
    'Antigua And Barbuda': 'AG',
    'Argentina': 'AR',
    'Armenia': 'AM',
    'Aruba': 'AW',
    'Australia': 'AU',
    'Austria': 'AT',
    'Azerbaijan': 'AZ',
    'Bahamas': 'BS',
    'Bahrain': 'BH',
    'Bangladesh': 'BD',
    'Barbados': 'BB',
    'Belarus': 'BY',
    'Belgium': 'BE',
    'Belize': 'BZ',
    'Benin': 'BJ',
    'Bermuda': 'BM',
    'Bhutan': 'BT',
    'Bolivia': 'BO',
    'Bosnia And Herzegovina': 'BA',
    'Botswana': 'BW',
    'Bouvet Island': 'BV',
    'Brazil': 'BR',
    'British Indian Ocean Territory': 'IO',
    'Brunei Darussalam': 'BN',
    'Bulgaria': 'BG',
    'Burkina Faso': 'BF',
    'Burundi': 'BI',
    'Cambodia': 'KH',
    'Cameroon': 'CM',
    'Canada': 'CA',
    'Cape Verde': 'CV',
    'Cayman Islands': 'KY',
    'Central African Republic': 'CF',
    'Chad': 'TD',
    'Chile': 'CL',
    'China': 'CN',
    'Christmas Island': 'CX',
    'Cocos (Keeling) Islands': 'CC',
    'Colombia': 'CO',
    'Comoros': 'KM',
    'Congo': 'CG',
    'Congo, Democratic Republic': 'CD',
    'Cook Islands': 'CK',
    'Costa Rica': 'CR',
    'Ivory Coast': 'CI',
    'Croatia': 'HR',
    'Cuba': 'CU',
    'Cyprus': 'CY',
    'Czech Republic': 'CZ',
    'Denmark': 'DK',
    'Djibouti': 'DJ',
    'Dominica': 'DM',
    'Dominican Republic': 'DO',
    'Ecuador': 'EC',
    'Egypt': 'EG',
    'El Salvador': 'SV',
    'Equatorial Guinea': 'GQ',
    'Eritrea': 'ER',
    'Estonia': 'EE',
    'Ethiopia': 'ET',
    'Falkland Islands (Malvinas)': 'FK',
    'Faroe Islands': 'FO',
    'Fiji': 'FJ',
    'Finland': 'FI',
    'France': 'FR',
    'French Guiana': 'GF',
    'French Polynesia': 'PF',
    'French Southern Territories': 'TF',
    'Gabon': 'GA',
    'Gambia': 'GM',
    'Georgia': 'GE',
    'Germany': 'DE',
    'Ghana': 'GH',
    'Gibraltar': 'GI',
    'Greece': 'GR',
    'Greenland': 'GL',
    'Grenada': 'GD',
    'Guadeloupe': 'GP',
    'Guam': 'GU',
    'Guatemala': 'GT',
    'Guernsey': 'GG',
    'Guinea': 'GN',
    'Guinea-Bissau': 'GW',
    'Guyana': 'GY',
    'Haiti': 'HT',
    'Heard Island & Mcdonald Islands': 'HM',
    'Holy See (Vatican City State)': 'VA',
    'Honduras': 'HN',
    'Hong Kong': 'HK',
    'Hungary': 'HU',
    'Iceland': 'IS',
    'India': 'IN',
    'Indonesia': 'ID',
    'Iran, Islamic Republic Of': 'IR',
    'Iraq': 'IQ',
    'Ireland': 'IE',
    'Isle Of Man': 'IM',
    'Israel': 'IL',
    'Italy': 'IT',
    'Jamaica': 'JM',
    'Japan': 'JP',
    'Jersey': 'JE',
    'Jordan': 'JO',
    'Kazakhstan': 'KZ',
    'Kenya': 'KE',
    'Kiribati': 'KI',
    'Korea': 'KR',
    'Kuwait': 'KW',
    'Kyrgyzstan': 'KG',
    'Lao People\'s Democratic Republic': 'LA',
    'Latvia': 'LV',
    'Lebanon': 'LB',
    'Lesotho': 'LS',
    'Liberia': 'LR',
    'Libyan Arab Jamahiriya': 'LY',
    'Liechtenstein': 'LI',
    'Lithuania': 'LT',
    'Luxembourg': 'LU',
    'Macao': 'MO',
    'Macedonia': 'MK',
    'Madagascar': 'MG',
    'Malawi': 'MW',
    'Malaysia': 'MY',
    'Maldives': 'MV',
    'Mali': 'ML',
    'Malta': 'MT',
    'Marshall Islands': 'MH',
    'Martinique': 'MQ',
    'Mauritania': 'MR',
    'Mauritius': 'MU',
    'Mayotte': 'YT',
    'Mexico': 'MX',
    'Micronesia, Federated States Of': 'FM',
    'Moldova': 'MD',
    'Monaco': 'MC',
    'Mongolia': 'MN',
    'Montenegro': 'ME',
    'Montserrat': 'MS',
    'Morocco': 'MA',
    'Mozambique': 'MZ',
    'Myanmar': 'MM',
    'Namibia': 'NA',
    'Nauru': 'NR',
    'Nepal': 'NP',
    'Netherlands': 'NL',
    'Netherlands Antilles': 'AN',
    'New Caledonia': 'NC',
    'New Zealand': 'NZ',
    'Nicaragua': 'NI',
    'Niger': 'NE',
    'Nigeria': 'NG',
    'Niue': 'NU',
    'Norfolk Island': 'NF',
    'Northern Mariana Islands': 'MP',
    'Norway': 'NO',
    'Oman': 'OM',
    'Pakistan': 'PK',
    'Palau': 'PW',
    'Palestinian Territory, Occupied': 'PS',
    'Panama': 'PA',
    'Papua New Guinea': 'PG',
    'Paraguay': 'PY',
    'Peru': 'PE',
    'Philippines': 'PH',
    'Pitcairn': 'PN',
    'Poland': 'PL',
    'Portugal': 'PT',
    'Puerto Rico': 'PR',
    'Qatar': 'QA',
    'Reunion': 'RE',
    'Romania': 'RO',
    'Russian Federation': 'RU',
    'Rwanda': 'RW',
    'Saint Barthelemy': 'BL',
    'Saint Helena': 'SH',
    'Saint Kitts And Nevis': 'KN',
    'Saint Lucia': 'LC',
    'Saint Martin': 'MF',
    'Saint Pierre And Miquelon': 'PM',
    'Saint Vincent And Grenadines': 'VC',
    'Samoa': 'WS',
    'San Marino': 'SM',
    'Sao Tome And Principe': 'ST',
    'Saudi Arabia': 'SA',
    'KSA': 'SA',
    'Senegal': 'SN',
    'Serbia': 'RS',
    'Seychelles': 'SC',
    'Sierra Leone': 'SL',
    'Singapore': 'SG',
    'Slovakia': 'SK',
    'Slovenia': 'SI',
    'Solomon Islands': 'SB',
    'Somalia': 'SO',
    'South Africa': 'ZA',
    'South Georgia And Sandwich Isl.': 'GS',
    'Spain': 'ES',
    'Sri Lanka': 'LK',
    'Sudan': 'SD',
    'Suriname': 'SR',
    'Svalbard And Jan Mayen': 'SJ',
    'Swaziland': 'SZ',
    'Sweden': 'SE',
    'Switzerland': 'CH',
    'Syrian Arab Republic': 'SY',
    'Taiwan': 'TW',
    'Tajikistan': 'TJ',
    'United Republic of Tanzania': 'TZ',
    'Thailand': 'TH',
    'Timor-Leste': 'TL',
    'Togo': 'TG',
    'Tokelau': 'TK',
    'Tonga': 'TO',
    'Trinidad And Tobago': 'TT',
    'Tunisia': 'TN',
    'Turkey': 'TR',
    'Turkmenistan': 'TM',
    'Turks And Caicos Islands': 'TC',
    'Tuvalu': 'TV',
    'Uganda': 'UG',
    'Ukraine': 'UA',
    'United Arab Emirates': 'AE',
    'United Kingdom': 'GB',
    'United States': 'US',
    'United States Outlying Islands': 'UM',
    'Uruguay': 'UY',
    'Uzbekistan': 'UZ',
    'Vanuatu': 'VU',
    'Venezuela': 'VE',
    'Viet Nam': 'VN',
    'Virgin Islands, British': 'VG',
    'Virgin Islands, U.S.': 'VI',
    'Wallis And Futuna': 'WF',
    'Western Sahara': 'EH',
    'Yemen': 'YE',
    'Zambia': 'ZM',
    'Zimbabwe': 'ZW'
};

function getCountryName(countryCode) {
    if (isoCountries.hasOwnProperty(countryCode)) {
        return isoCountries[countryCode];
    } else {
        return countryCode;
    }
}
function initMap(start, end, m_a) {
    var lat;
    var lon;
    switch (m_a) {
        case 'MMEA':
            lat = -20;
            lon = -20
            break;
        case 'MANA':
            lat = -50;
            lon = 100
            break;
        case 'MELA':
            lat = -20;
            lon = 10
            break;
        case 'MNEA':
            lat = -30;
            lon = -85
            break;
        case 'MOAI':
            lat = 0;
            lon = -110
            break;
        default:
            lat = -20;
            lon = -20
    }

    $.ajax({
        url: "api/dashboardapi/new_users",
        type: "GET",
        data: { start: start, end: end, marketArea: m_a },
        success: function (res) {
            var mapdata = [];
            for (var i = 0; i < res.length; i++) {
                var obj = res[i];
                var isocode = getCountryName(obj["country"])
                if (isocode != "Other") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
            }
            am4core.ready(function () {


                // Themes begin
                // am4core.useTheme(am4themes_dark);
                am4core.useTheme(am4themes_animated);
                // Themes end

                if (chart3) {
                    chart3.dispose();
                    delete chart3;
                    chart3 = am4core.create("chartdiv3", am4maps.MapChart)
                }

                // Set map definition
                chart3.geodata = am4geodata_worldLow;

                // Set projection
                chart3.projection = new am4maps.projections.Orthographic();
                chart3.panBehavior = "rotateLongLat";
                chart3.deltaLatitude = lat;
                chart3.deltaLongitude = lon;

                chart3.padding(20, 20, 20, 20);

                // limits vertical rotation
                chart3.adapter.add("deltaLatitude", function (delatLatitude) {
                    return am4core.math.fitToRange(delatLatitude, -90, 90);
                })

                // Create map polygon series
                var polygonSeries = chart3.series.push(new am4maps.MapPolygonSeries());

                // Make map load polygon (like country names) data from GeoJSON
                polygonSeries.useGeodata = true;

                // Configure series
                var polygonTemplate = polygonSeries.mapPolygons.template;
                polygonTemplate.tooltipText = "{name} IMK Registered Users {customData} ";
                polygonTemplate.fill = am4core.color("#EDEDED");

                var graticuleSeries = chart3.series.push(new am4maps.GraticuleSeries());
                graticuleSeries.mapLines.template.line.stroke = am4core.color("#4D97ED");
                graticuleSeries.mapLines.template.line.strokeOpacity = 0.08;
                graticuleSeries.fitExtent = false;


                chart3.backgroundSeries.mapPolygons.template.polygon.fillOpacity = 0.6;
                chart3.backgroundSeries.mapPolygons.template.polygon.fill = am4core.color("#4D97ED");

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = chart.colors.getIndex(0).brighten(-0.5);

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = am4core.color("#fff");

                // Remove Antarctica
                polygonSeries.exclude = [];

                // Add some data
                polygonSeries.data = mapdata;

                // Bind "fill" property to "fill" key in data
                polygonTemplate.propertyFields.fill = "fill";
            }); // end am4core.ready()

        }
    })
    $.ajax({
        url: "api/dashboardapi/usage",
        type: "GET",
        data: { start: start, end: end, marketArea: m_a },
        success: function (res) {
            var mapdata = [];
            for (var i = 0; i < res.length; i++) {
                var obj = res[i];
                var isocode = getCountryName(obj["country"])
                if (isocode != "Other") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }

                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
            }
            am4core.ready(function () {


                // Themes begin
                // am4core.useTheme(am4themes_dark);
                am4core.useTheme(am4themes_animated);
                // Themes end

                if (chart) {
                    chart.dispose();
                    delete chart;
                    chart = am4core.create("chartdiv", am4maps.MapChart)
                }

                // Set map definition
                chart.geodata = am4geodata_worldLow;

                // Set projection
                chart.projection = new am4maps.projections.Orthographic();
                chart.panBehavior = "rotateLongLat";
                chart.deltaLatitude = lat;
                chart.deltaLongitude = lon;
                chart.padding(20, 20, 20, 20);

                // limits vertical rotation
                chart.adapter.add("deltaLatitude", function (delatLatitude) {
                    return am4core.math.fitToRange(delatLatitude, -90, 90);
                })

                // Create map polygon series
                var polygonSeries = chart.series.push(new am4maps.MapPolygonSeries());

                // Make map load polygon (like country names) data from GeoJSON
                polygonSeries.useGeodata = true;

                // Configure series
                var polygonTemplate = polygonSeries.mapPolygons.template;
                polygonTemplate.tooltipText = "{name} IMK usage {customData} ";
                polygonTemplate.fill = am4core.color("#EDEDED");

                var graticuleSeries = chart.series.push(new am4maps.GraticuleSeries());
                graticuleSeries.mapLines.template.line.stroke = am4core.color("#4D97ED");
                graticuleSeries.mapLines.template.line.strokeOpacity = 0.08;
                graticuleSeries.fitExtent = false;


                chart.backgroundSeries.mapPolygons.template.polygon.fillOpacity = 0.6;
                chart.backgroundSeries.mapPolygons.template.polygon.fill = am4core.color("#4D97ED");


                polygonSeries.exclude = [];

                // Add some data
                polygonSeries.data = mapdata;

                // Bind "fill" property to "fill" key in data
                polygonTemplate.propertyFields.fill = "fill";
            }); // end am4core.ready()

        }
    })

    $.ajax({
        url: "api/dashboardapi/active_users",
        type: "GET",
        data: { start: start, end: end, marketArea: m_a },
        success: function (res) {
            var mapdata = [];
            for (var i = 0; i < res.length; i++) {
                var obj = res[i];
                var isocode = getCountryName(obj["country"])
                if (isocode != "Other") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }

                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
            }
            am4core.ready(function () {
                // Themes begin
                // am4core.useTheme(am4themes_dark);
                am4core.useTheme(am4themes_animated);
                // Themes end


                if (chart2) {
                    chart2.dispose();
                    delete chart2;
                    chart2 = am4core.create("chartdiv2", am4maps.MapChart)
                }
                // Set map definition
                chart2.geodata = am4geodata_worldLow;

                // Set projection
                chart2.projection = new am4maps.projections.Orthographic();
                chart2.panBehavior = "rotateLongLat";
                chart2.deltaLatitude = lat;
                chart2.deltaLongitude = lon;
                chart2.padding(20, 20, 20, 20);

                // limits vertical rotation
                chart2.adapter.add("deltaLatitude", function (delatLatitude) {
                    return am4core.math.fitToRange(delatLatitude, -90, 90);
                })

                // Create map polygon series
                var polygonSeries = chart2.series.push(new am4maps.MapPolygonSeries());

                // Make map load polygon (like country names) data from GeoJSON
                polygonSeries.useGeodata = true;

                // Configure series
                var polygonTemplate = polygonSeries.mapPolygons.template;
                polygonTemplate.tooltipText = "{name} IMK Users {customData} ";
                polygonTemplate.fill = am4core.color("#EDEDED");

                var graticuleSeries = chart2.series.push(new am4maps.GraticuleSeries());
                graticuleSeries.mapLines.template.line.stroke = am4core.color("#4D97ED");
                graticuleSeries.mapLines.template.line.strokeOpacity = 0.08;
                graticuleSeries.fitExtent = false;


                chart2.backgroundSeries.mapPolygons.template.polygon.fillOpacity = 0.6;
                chart2.backgroundSeries.mapPolygons.template.polygon.fill = am4core.color("#4D97ED");

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = chart.colors.getIndex(0).brighten(-0.5);

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = am4core.color("#fff");

                // Remove Antarctica
                polygonSeries.exclude = [];

                // Add some data
                polygonSeries.data = mapdata;

                // Bind "fill" property to "fill" key in data
                polygonTemplate.propertyFields.fill = "fill";
            }); // end am4core.ready()

        }
    })

    $.ajax({
        url: "api/dashboardapi/lmt-usage",
        type: "GET",
        data: { start: start, end: end, marketArea: m_a },
        success: function (res) {
            var mapdata = [];
            for (var i = 0; i < res.length; i++) {
                var obj = res[i];
                var isocode = getCountryName(obj["country"])
                if (isocode != "Other") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#ED0E00"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#FA842A"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#CFC000"

                    }
                    mapdata.push(data)
                }
            }
            am4core.ready(function () {


                // Themes begin
                // am4core.useTheme(am4themes_dark);
                am4core.useTheme(am4themes_animated);
                // Themes end

                if (chart4) {
                    chart4.dispose();
                    delete chart4;
                    chart4 = am4core.create("chartdiv4", am4maps.MapChart)
                }

                // Set map definition
                chart4.geodata = am4geodata_worldLow;

                // Set projection
                chart4.projection = new am4maps.projections.Orthographic();
                chart4.panBehavior = "rotateLongLat";
                chart4.deltaLatitude = lat;
                chart4.deltaLongitude = lon;

                chart4.padding(20, 20, 20, 20);

                // limits vertical rotation
                chart4.adapter.add("deltaLatitude", function (delatLatitude) {
                    return am4core.math.fitToRange(delatLatitude, -90, 90);
                })

                // Create map polygon series
                var polygonSeries = chart4.series.push(new am4maps.MapPolygonSeries());

                // Make map load polygon (like country names) data from GeoJSON
                polygonSeries.useGeodata = true;

                // Configure series
                var polygonTemplate = polygonSeries.mapPolygons.template;
                polygonTemplate.tooltipText = "{name} LMT Usage {customData} ";
                polygonTemplate.fill = am4core.color("#EDEDED");

                var graticuleSeries = chart4.series.push(new am4maps.GraticuleSeries());
                graticuleSeries.mapLines.template.line.stroke = am4core.color("#4D97ED");
                graticuleSeries.mapLines.template.line.strokeOpacity = 0.08;
                graticuleSeries.fitExtent = false;


                chart4.backgroundSeries.mapPolygons.template.polygon.fillOpacity = 0.6;
                chart4.backgroundSeries.mapPolygons.template.polygon.fill = am4core.color("#4D97ED");

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = chart.colors.getIndex(0).brighten(-0.5);

                // // Create hover state and set alternative fill color
                // var hs = polygonTemplate.states.create("hover");
                // hs.properties.fill = am4core.color("#fff");

                // Remove Antarctica
                polygonSeries.exclude = [];

                // Add some data
                polygonSeries.data = mapdata;

                // Bind "fill" property to "fill" key in data
                polygonTemplate.propertyFields.fill = "fill";
            }); // end am4core.ready()

        }
    })
}
//initMap()






