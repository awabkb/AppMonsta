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

// // Dropdowns
// const dropdowns = document.querySelectorAll('.dropdown');
// if (dropdowns) {
//     Array.from(dropdowns).forEach(dropdownDOM => {
//         const dropdown = new eds.Dropdown(dropdownDOM);
//         dropdown.init();
//     });
// }

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
var datestart = null;
var dateend = null;


function init() {
    daterange();
    marketArea = sessionStorage['marketArea'] != undefined ? sessionStorage['marketArea'] : '';
    countriesFilter = sessionStorage['countries'] != undefined ? sessionStorage['countries'] : "all";
    operatorsFilter = sessionStorage['operators'] != undefined ? sessionStorage['operators'] : [];
    datestart = moment().subtract(29, 'days');
    dateend = moment();

    getCountries(marketArea);
    // dateFilter();
    initMap(datestart.format('YYYY-MM-DD'), dateend.format('YYYY-MM-DD'), marketArea);
    getData(datestart.format('YYYY-MM-DD'), dateend.format('YYYY-MM-DD'), countriesFilter, operatorsFilter);

}
function daterange() {
    $(function () {
        var start = moment().subtract(29, 'days');
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
});


$('#filter').on('reset', function (e) {
    e.preventDefault();
    var c = "all";
    var o = [];
    var ma = '';
    var s = (moment().subtract(29, 'days')).format('YYYY-MM-DD');
    var e = (moment()).format('YYYY-MM-DD');

    $('.country[type="checkbox"]').each(function () {
        this.checked = false;
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
});




$("#market-areas li").on("click", function () {
    $(this).parent().find("li.active").removeClass("active");
    $(this).addClass("active");
    sessionStorage['marketArea'] = $(this).attr('value');
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
                    '<input class="country" name=\"countries[]\" onclick="getOperators()" type="checkbox" id="c-' + i + '"value="' + res[i] + '"checked>' +
                    '<label for="c-' + i + '">' + res[i] + '</label>' +
                    '</span>' +
                    '</li>'
            }
            document.getElementById('countries').innerHTML = countries;
            getOperators();
        }
    });

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
                var operators = '';
                for (var i = 0; i < res.length; i++) {
                    for (var j = 0; j < res[i].operators.length; j++) {
                        operators +=
                            '<li>' +
                            '<span class="item" tabindex="0">' +
                            '<input class="operator" name=\"operators[]\" type="checkbox" id="o-' + i + '' + j + '"value="' + res[i].operators[j]["name"] + '">' +
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
    getData(s, e, c, o);
    initMap(s, e, ma);
}


function getData(startdate, enddate, countries, operators) {
    var Data = { start: decodeURIComponent(startdate), end: decodeURIComponent(enddate), countries: decodeURIComponent(countries), operators: decodeURIComponent(operators) }
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
                    functions.push(res[0][i]);
                }
            }
            const chart = new eds.HorizontalBarChart({
                element: element,
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
                ]
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

    $.ajax({
        url: "api/dashboardapi/site_details",
        type: "GET",
        data: Data,
        success: function (res) {
            const tableDOM = document.querySelector('#site-details');
            tableDOM.innerHTML = '';
            const table = new eds.Table(tableDOM, {
                data: res,
                columns: [
                    {
                        key: 'date',
                        title: 'Date',
                        sort: 'none'
                    },
                    {
                        key: 'siteName',
                        title: 'Site Name',
                        sort: 'none'
                    },
                    {
                        key: 'country',
                        title: 'Country',
                        sort: 'none'
                    },
                    {
                        key: 'user',
                        title: 'Field Engineer',
                        sort: 'none'
                    },
                    {
                        key: 'appVersion',
                        title: 'Android Version',
                        sort: 'none'
                    },
                    {
                        key: 'rpiVersion',
                        title: 'IMK Version',
                        sort: 'none'
                    },
                    {
                        key: 'asp',
                        title: 'ASP',
                        sort: 'none'
                    },
                ],
                sortable: true,
                actions: true,

                onCreatedActionsCell: (td) => {
                    td.innerHTML = `<button class="btn-icon email"><i class="icon icon-email"></i></button>`;

                    td.querySelector('button.email').addEventListener('click', (evt) => {
                        var tr = evt.target.closest('tr');
                        var siteEngineer = $(tr).find('td').eq(3).text();

                        $.ajax({
                            url: "api/dashboardapi/unique_sites",
                            type: "GET",
                            data: siteEngineer,
                            success: function (res) {
                                const notification = new eds.Notification({
                                    title: "Contact Info",
                                    description: 'An email has been sent to you',
                                });
                                notification.init();
                            }
                        });
                    });
                }
            });
            table.init();

            document.querySelector('#export-data').addEventListener('click', () => {
                const notification = new eds.Notification({
                    title: 'Export data',
                    description: 'Table data is exported to IMK_Dashboard.csv file',
                });
                notification.init();
                var rows = [];
                rows.push(['Date', 'Site Name', 'Country', 'Field Engineer', 'Android Version', 'IMK Version', 'ASP']);
                table.data.forEach(row => {
                    rows.push([row["date"], row["siteName"], row["country"], row["user"], row["appVersion"], row["rpiVersion"], row["asp"]]);
                });
                exportToCsv("IMK_Dashboard.csv", rows)

            });
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

/////////////////////////////////////////////// GLOBES 
var chart = am4core.create(document.getElementById("chartdiv"), am4maps.MapChart);
var chart2 = am4core.create(document.getElementById("chartdiv2"), am4maps.MapChart);
var chart3 = am4core.create(document.getElementById("chartdiv3"), am4maps.MapChart);
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
    'Cote D\'Ivoire': 'CI',
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
    'Tanzania': 'TZ',
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
                    if (obj["percent"] > 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] < 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

                    }
                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] > 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] < 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

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
                polygonTemplate.fill = am4core.color("#DCDCDC");

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
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

                    }

                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["usage"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

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
                polygonTemplate.fill = am4core.color("#DCDCDC");

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
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = isocode;
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

                    }

                    mapdata.push(data)
                }
                if (isocode == "MA") {
                    var data = []
                    if (obj["percent"] >= 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#BB0B02"
                    }
                    if (obj["percent"] > 10 && obj["percent"] < 20) {

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#F78F2D"
                    }
                    if (obj["percent"] <= 10) {
                        var data = [];

                        data["title"] = obj["country"]
                        data["id"] = 'EH';
                        data["customData"] = obj["users"] + " (" + obj["percent"] + "%)";
                        data["fill"] = "#E3D61E"

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
                polygonTemplate.fill = am4core.color("#DCDCDC");

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
}
//initMap()







