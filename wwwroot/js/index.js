
var options = {
    region: '002'
};
var countries = [];

$.get("/api/home/countries", function (data) {
  
    var header = ['Country', 'Popularity'];
    countries.push(header);
    for (var country of data) {
        var temp = [];
        temp.push(country.countryName);
        temp.push(10);
        countries.push(temp);
    }
    google.charts.load('current', {
        'packages': ['geochart'],
        // Note: you will need to get a mapsApiKey for your project.
        // See: https://developers.google.com/chart/interactive/docs/basic_load_libs#load-settings
        'mapsApiKey': 'AIzaSyD-9tSrke72PouQMnMX-a7eZSW0jkFMBWY'
    });
    google.charts.setOnLoadCallback(drawRegionsMap);
});
function drawRegionsMap() {
    var data = google.visualization.arrayToDataTable(countries);
    var chart = new google.visualization.GeoChart(document.getElementById('regions_div'));
    chart.draw(data, options);
}

$('#a150, #a002, #a142').on('click',function (event) {
    options.region = event.target.id.substring(1);
    drawRegionsMap();
});


