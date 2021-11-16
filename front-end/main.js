class Station {
  stationid;
  latitude;
  longitude;
  name;
  createdby;
  createdat;
  updatedby;
  updatedat;
  tempunit;
  windspeedunit;
  pressureunit;
  precipitationunit;
  radiationunit;
  leafwetnessunit;
  soiltempunit;
  soilmoistunit;
};



var stationsuuid = ["61098d70-4498-4f0f-b0f5-6046a640126e", "0ffad38e-66e8-4426-a9e3-7c6c80720bc2", "62223f7b-edfb-429b-b9d4-adb82ff959e7"];

var stations = Array();


var apiUrl = "https://localhost:5001/4meteo/Data";
var selectedNumber = 1;
var selectedDate = new Date();
var selectedDateStart = new Date().setDate(new Date().getDate()- 1);
var selectedDateEnd = new Date();
var selectedMode = 1;


var tempctx = document.getElementById('tempChart');
var humidityctx = document.getElementById('humidityChart');
var pressctx = document.getElementById('pressChart');
var precctx = document.getElementById('precChart');
var radctx = document.getElementById('radChart');
var windsctx = document.getElementById('windsChart');


// var tempChart = new Chart(tempctx, {
//   type: 'line',
//   data: {
//     labels: [],
//     datasets: [{
//       label: 'Temperatura',
//       data: [],
//       backgroundColor: 'rgb(255, 79, 56)',
//       borderColor: 'rgb(255, 79, 56)',
//       borderWidth: 1
//     }]
//   },
//   options: {
//     scales: {
//       y: {
//         beginAtZero: true
//       }
//     }
//   }
// });

var humidityChart = new Chart(humidityctx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'Humidade',
      data: [],
      backgroundColor: 'rgb(77, 169, 255)',
      borderColor: 'rgb(77, 169, 255)',
      borderWidth: 1
    }]
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});

var pressChart = new Chart(pressctx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'Pressão',
      data: [],
      backgroundColor: 'rgb(14, 166, 0)',
      borderColor: 'rgb(14, 166, 0)',
      borderWidth: 1
    }]
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});

var precChart = new Chart(precctx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'Precipitação',
      data: [],
      backgroundColor: 'rgb(0, 0, 255)',
      borderColor: 'rgb(0, 0, 255)',
      borderWidth: 1
    }]
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});

var radChart = new Chart(radctx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'Radiação',
      data: [],
      backgroundColor: 'rgb(250, 203, 75)',
      borderColor: 'rgb(250, 203, 75)',
      borderWidth: 1
    }]
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});

var mixedChart = new Chart(tempctx, {
  type: 'line',
  data: {
    datasets: [{
      label: 'Temperatura',
      data: [],
      backgroundColor: 'rgb(255, 79, 56)',
      borderColor: 'rgb(255, 79, 56)',
      // this dataset is drawn below
      order: 2
    }, {
      label: 'Ponto de orvalho',
      data: [],
      type: 'line',
      backgroundColor: 'rgb(250, 203, 75)',
      borderColor: 'rgb(250, 203, 75)',
      // this dataset is drawn on top
      order: 1
    }],
    labels: []
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});



var windsChart = new Chart(windsctx, {
  type: 'line',
  data: {
    labels: [],
    datasets: [{
      label: 'Intensidade do vento',
      data: [],
      backgroundColor: 'rgb(82, 230, 14)',
      borderColor: 'rgb(82, 230, 14)',
      borderWidth: 1
    }]
  },
  options: {
    scales: {
      y: {
        beginAtZero: true
      }
    }
  }
});

function classWind(d, min, max) {
  var total = 0;
  var aux = [0, 0, 0, 0, 0, 0, 0, 0]
  d.r.forEach(function (element, i) {
    total++;
    if (element >= min && element < max) {
      aux[letterToId(d.theta[i])]++;
    }
  });
  // console.log("berofe");
  // console.log(total);
  // console.log(aux);
  aux.forEach(function (element, i) {

    if (element != 0 | element != NaN) aux[i] = ((100 * element) / total).toFixed(1);
  });
  // console.log("after");
  // console.log(aux);
  return aux;
}

function findStation(id) {
  return stations.find(x => x.stationid == id);
}

function letterToId(letter) {
  switch (letter) {
    case "N": return 0;
    case "NE": return 1;
    case "E": return 2;
    case "SE": return 3;
    case "S": return 4;
    case "SW": return 5;
    case "W": return 6;
    case "NW": return 7;
    case null: return null;
  }
}


function CalcDewpoint(temp, rh) {
  a = 17.27;
  b = 237.7;
  return b * ((a * temp) / (b + temp) + Math.log(rh)) / (a - ((a * temp) / (b + temp)) + Math.log(rh));
}

function DisplayChartData(chart, urlend, canvasId, labelname) {
  $.get(apiUrl + "/" + urlend + "/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "_" + $('#modeselect').val(), function (data, status) {
    //console.log(apiUrl + "/" + urlend + "/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "_" + $('#modeselect').val());
    if (status == "success") {
      //console.log("Display")
      //console.log(new Date(selectedDate).toISOString());
      //console.log(data.length);
      try{
      if (data[0]["value"] == null) {
        $('#' + canvasId).hide();
        //console.log("hide");
      }
      else {
        console.log(data);
        $('#' + canvasId).show();
        var labels = [];
        var values = [];

        data.forEach(element => {
          labels.push(new Date(element["time"]).toLocaleString("pt-PT"))
          values.push(element["value"])
        });

        chart.data.labels.pop();
        chart.data.datasets[0].data.pop();
        chart.update();

        chart.data.datasets[0].label = labelname;
        chart.data.labels = labels;
        chart.data.datasets[0].data = values;
        chart.update();
      }
    }catch{
      chart.data.labels.pop();
      chart.data.datasets[0].data.pop();
      chart.update();
    }
    }

  })
}

function DisplayChartDataWind(Schart, urlend, canvasSId, rosePlotId) {
  $.get(apiUrl + "/" + urlend + "/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "_" + $('#modeselect').val(), function (data, status) {
    if (status == "success") {
      //console.log(data);
      try{
      if (data[0]["value"] == null) {
        $('#' + canvasSId).hide();
        $('#' + rosePlotId).hide();
      }
      else {
        $('#' + canvasSId).show();
        $('#' + rosePlotId).show();


        var labels = [];
        var values = [];

        data.forEach(element => {
          labels.push(new Date(element["time"]).toLocaleString("pt-PT"))
          values.push(element["value"])
        });


        Schart.data.labels.pop();
        Schart.data.datasets[0].data.pop();
        Schart.update();


        Schart.data.labels = labels;
        Schart.data.datasets[0].data = values;
        Schart.update();




        var d = {
          r: [],
          theta: [],
        };

        var max = 0;

        data.forEach(element => {
          d.r.push(element["value"]),
            d.theta.push(element["direction"])
          if (element["value"] > max) max = element["value"];
        });

        var interval = max / 4;


        //console.log(d);
        var d0 = classWind(d, 0, interval);
        var d1 = classWind(d, interval, 2 * interval);
        var d2 = classWind(d, 2 * interval, 3 * interval);
        var d3 = classWind(d, 3 * interval, 4 * interval);



        var data = [{
          r: d3,
          theta: ["North", "N-E", "East", "S-E", "South", "S-W", "West", "N-W"],
          name: 3 * interval.toFixed(0) + "-" + 4 * interval.toFixed(0) + " km/h",
          marker: { color: "rgb(20, 0, 150)" },
          type: "barpolar"
        }
          , {
          r: d2,
          theta: ["North", "N-E", "East", "S-E", "South", "S-W", "West", "N-W"],
          name: 2 * interval.toFixed(0) + "-" + 3 * interval.toFixed(0) + " km/h",
          marker: { color: "rgb(70,50,170)" },
          type: "barpolar"
        }, {
          r: d1,
          theta: ["North", "N-E", "East", "S-E", "South", "S-W", "West", "N-W"],
          name: interval.toFixed(2) + "-" + 2 * interval.toFixed(0) + " km/h",
          marker: { color: "rgb(120,100,190)" },
          type: "barpolar"
        }, {
          r: d0,
          theta: ["North", "N-E", "East", "S-E", "South", "S-W", "West", "N-W"],
          name: "0-" + interval.toFixed(2) + " km/h",
          marker: { color: "rgb(170,150,210)" },
          type: "barpolar"
        }]
        var layout = {
          title: "Rosa dos ventos",
          font: { size: 16 },
          legend: { font: { size: 16 } },
          polar: {
            barmode: "overlay",
            bargap: 0,
            radialaxis: { ticksuffix: "%", angle: 45, dtick: 20 },
            angularaxis: { direction: "clockwise" }
          }
        }
        Plotly.newPlot(rosePlot, data, layout)

      }
    }catch{
      Schart.data.labels.pop();
      Schart.data.datasets[0].data.pop();
      Schart.update();
    }
    }

  })
}

$(document).ready(function () {
  $('#datatable').DataTable({
    "scrollX": true,
    "dom": 'Bfrtip',
    "buttons": [
     'csv', 'excel', 'pdf', 'print'
    ]
  });
});

$.get(apiUrl + "/GetStation/" + stationsuuid[0], function (data, status) {

  if (status == "success") {

    var aux = Station;

    aux.stationid = stationsuuid[0];
    aux.name = data["name"];
    aux.longitude = data["longitude"];
    aux.latitude = data["latitude"];
    aux.createdat = data["createdat"];
    aux.createdby = data["createdby"];
    aux.createdby = data["createdby"];
    aux.updatedat = data["updatedat"];
    aux.updatedby = data["updatedby"];
    aux.tempunit = data["tempunit"];
    aux.windspeedunit = data["windspeedunit"];
    aux.pressureunit = data["pressureunit"];
    aux.precipitationunit = data["precipitationunit"];
    aux.radiationunit = data["radiationunit"];
    aux.leafwetnessunit = data["leafwetnessunit"];
    aux.soiltempunit = data["soiltempunit"];
    aux.soilmoistunit = data["soilmoistunit"];

    stations.push(aux);
    //console.log(stations);

    $("#name").html(data["name"]);
    $("#latitude").html(data["latitude"]);
    $("#longitude").html(data["longitude"]);
    $("#createdby").html(data["createdby"]);
    $("#createdat").html(new Date(data["createdat"]).toLocaleString("pt-PT"));
    $("#updatedby").html(data["updatedby"]);
    $("#updatedat").html(new Date(data["updatedat"]).toLocaleString("pt-PT"));

    // var urlmap = "https://www.openstreetmap.org/export/embed.html?bbox=" + (data["longitude"] - 0.040).toFixed(6) + "%2C" + (data["latitude"] - 0.040).toFixed(6) + "%2C" + (data["longitude"] + 0.040).toFixed(6) + "%2C" + (data["latitude"] + 0.040).toFixed(6) + "&amp;layer=mapnik&amp;marker=" + (data["latitude"]).toFixed(6) + "%2C" + (data["longitude"]).toFixed(6);
    // $("#mapsrc").attr("src", urlmap);
    // var bigmap = "https://www.openstreetmap.org/?mlat=" + data["latitude"].toFixed(6) + "&amp;mlon=" + data["longitude"].toFixed(6) + "#map=14/" + data["latitude"].toFixed(6) + "/" + data["longitude"].toFixed(6);
    // $("#bigmap").attr("href", bigmap);


    var map;


    // The overlay layer for our marker, with a simple diamond as symbol
    var overlay = new OpenLayers.Layer.Vector('Overlay', {
      styleMap: new OpenLayers.StyleMap({
        externalGraphic: 'https://img.icons8.com/windows/452/weather-station-wind-and-air.png',
        graphicWidth: 20, graphicHeight: 24, graphicYOffset: -24,
        title: '${tooltip}'
      })
    });

    // The location of our marker and popup. We usually think in geographic
    // coordinates ('EPSG:4326'), but the map is projected ('EPSG:3857').
    var myLocation = new OpenLayers.Geometry.Point(data["longitude"], data["latitude"])
      .transform('EPSG:4326', 'EPSG:3857');

    // We add the marker with a tooltip text to the overlay
    overlay.addFeatures([
      new OpenLayers.Feature.Vector(myLocation, { tooltip: 'OpenLayers' })
    ]);



    // Finally we create the map
    map = new OpenLayers.Map({
      div: "mapdiv", projection: "EPSG:3857",
      layers: [new OpenLayers.Layer.OSM(), overlay],
      center: myLocation.getBounds().getCenterLonLat(), zoom: 14
    });
    // and add the popup to it.
    //console.log(selectedDate);
    $.get(apiUrl + "/GetSummary/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "?mode=" + $('#modeselect').val(), function (data, status) {
      if (status = "success") {

        var aux = stations[0]

        $("#tempmax").html(data["maxTemp"] == null ? "--" : data["maxTemp"] + " " + aux.tempunit);
        $("#tempavg").html(data["avgTemp"] == null ? "--" : data["avgTemp"].toFixed(2) + " " + aux.tempunit);
        $("#tempmin").html(data["minTemp"] == null ? "--" : data["minTemp"] + " " + aux.tempunit);

        $("#hummax").html(data["maxHumidity"] == null ? "--" : data["maxHumidity"] + " %");
        $("#humavg").html(data["avgHumidity"] == null ? "--" : data["avgHumidity"].toFixed(2) + " %");
        $("#hummin").html(data["minHumidity"] == null ? "--" : data["minHumidity"] + " %");

        $("#windmax").html(data["maxWind"] == null ? "--" : data["maxWind"] + " " + aux.windspeedunit);
        $("#windavg").html(data["avgWind"] == null ? "--" : data["avgWind"].toFixed(2) + " " + aux.windspeedunit);
        $("#windmin").html(data["minWind"] == null ? "--" : data["minWind"] + " " + aux.windspeedunit);

        $("#presmax").html(data["maxPress"] == null ? "--" : data["maxPress"] + " " + aux.pressureunit);
        $("#presavg").html(data["avgPress"] == null ? "--" : data["avgPress"] + " " + aux.pressureunit);
        $("#presmin").html(data["maxPress"] == null ? "--" : data["manPress"] + " " + aux.pressureunit);

        $("#precmax").html(data["maxPrecipitation"] == null ? "--" : data["maxPrecipitation"] + " " + aux.precipitationunit);
        $("#precavg").html(data["avgPrecipitation"] == null ? "--" : data["avgPrecipitation"].toFixed(2) + " " + aux.precipitationunit);
        $("#precmin").html(data["minPrecipitation"] == null ? "--" : data["minPrecipitation"] + " " + aux.precipitationunit);

        $("#radmax").html(data["maxRadiation"] == null ? "--" : data["maxRadiation"] + " " + aux.radiationunit);
        $("#radavg").html(data["avgRadiation"] == null ? "--" : data["avgRadiation"].toFixed(2) + " " + aux.radiationunit);
        $("#radmin").html(data["minRadiation"] == null ? "--" : data["minRadiation"] + " " + aux.radiationunit);


        var aux = stations[0]
        DisplayChartData(mixedChart, "GetTempRecords", "tempChart", "Temperatura(" + aux.tempunit + ")");
        DisplayChartData(humidityChart, "GetHumidityRecords", "humidityChart", "Humidade(%)");
        DisplayChartData(pressChart, "GetPressureRecords", "pressChart", "Pressão(" + aux.pressureunit + ")");
        DisplayChartData(precChart, "GetPrecipitationRecords", "precChart", "Precipitação(" + aux.precipitationunit + ")");
        DisplayChartData(radChart, "GetRadiationRecords", "radChart", "Radiação(" + aux.radiationunit + ")");
        DisplayChartDataWind(windsChart, "GetWindRecords", "windsChart", "rosePlot", aux);
        DisplayDewPoint(mixedChart, stationsuuid[0]);
      }



      $.get(apiUrl + "/GetLastRecord/" + stationsuuid[0], function (data, status) {
        if (status == "success") {
          var aux = stations[0];
          //console.log("lastrecord");
          //console.log(aux);
          $("#time").html(data["time"] == null ? "--" : new Date(data["time"]).toLocaleString("pt-PT"));
          $("#temp").html(data["temperature"] == null ? "--" : data["temperature"] + " " + aux.tempunit);
          $("#pressure").html(data["pressure"] == null ? "--" : data["pressure"] + " " + aux.pressureunit);
          $("#winddir").html(data["windDir"] == null ? "--" : data["windDir"]);
          $("#windspeed").html(data["windspeed"] == null ? "--" : data["windspeed"] + " " + aux.windspeedunit);
          $("#humidity").html(data["humidity"] == null ? "--" : data["humidity"] + " %");
          $("#radiation").html(data["radiation"] == null ? "--" : data["radiation"] + " " + aux.radiationunit);
          $("#precipitation").html(data["precipitation"] == null ? "--" : data["precipitation"] + " " + aux.precipitationunit);

          var d = {
            r: [1],
            theta: [data["windDir"]],
          };

          var deg = 0;
          switch (data["windDir"]) {
            case "N": { deg = 0; break; }
            case "NE": { deg = 45; break; }
            case "E": { deg = 90; break; }
            case "SE": { deg = 135; break; }
            case "S": { deg = 180; break; }
            case "SW": { deg = 225; break; }
            case "W": { deg = 270; break; }
            case "NW":
              {
                deg = 315;
                break;
              }

          }
          // console.log("WindDir Deg");
          // console.log(deg);

          $("#windDirIcon").css('transform', 'rotate(' + deg + 'deg)');

          // console.log(selectedDateStart);
          // console.log(selectedDateEnd);
          // console.log(new Date(selectedDateStart).toISOString());
          // console.log(new Date(selectedDateEnd).toISOString());

          var t = $('#datatable').DataTable();
          $.get(apiUrl + "/GetRecords/" + stationsuuid[0] + "/" + new Date(selectedDateStart).toISOString() + "_until_" + new Date(selectedDateEnd).toISOString(), function (data, status) {
            if (status == "success") {
              var temp = false;
              var humi = false;
              var windspeed = false;
              var winddir = false;
              var press = false;
              var prec = false;
              var rad = false;
              var leaf = false;
              var sm1 = false;
              var sm2 = false;
              var sm3 = false;
              var st1 = false;
              var st2 = false;
              var st3 = false;
              var cd1 = false;
              var cd2 = false;
              var cd3 = false;
              var cd4 = false;
              var cd5 = false;
              var ct1 = false;
              var ct2 = false;
              var ct3 = false;
              var ct4 = false;
              var ct5 = false;
              //console.log(data[0]);
              data[0]["temperature"] == null ? temp = false : temp = true;
              data[0]["humidity"] == null ? humi = false : humi = true;
              data[0]["windspeed"] == null ? windspeed = false : windspeed = true;
              data[0]["winddir"] == null ? winddir = false : winddir = true;
              data[0]["pressure"] == null ? press = false : press = true;
              data[0]["precipitation"] == null ? prec = false : prec = true;
              data[0]["radiation"] == null ? rad = false : (rad = true);
              data[0]["leafwetness"] == null ? leaf = false : leaf = true;
              data[0]["soilmoisture1"] == null ? sm1 = false : sm1 = true;
              data[0]["soilmoisture2"] == null ? sm2 = false : sm2 = true;
              data[0]["soilmoisture3"] == null ? sm3 = false : sm3 = true;
              data[0]["soiltemperature1"] == null ? st1 = false : st1 = true;
              data[0]["soiltemperature2"] == null ? st2 = false : st2 = true;
              data[0]["soiltemperature3"] == null ? st3 = false : st3 = true;
              data[0]["customd1"] == null ? cd1 = false : cd1 = true;
              data[0]["customd2"] == null ? cd2 = false : cd2 = true;
              data[0]["customd3"] == null ? cd3 = false : cd3 = true;
              data[0]["customd4"] == null ? cd4 = false : cd4 = true;
              data[0]["customd5"] == null ? cd5 = false : cd5 = true;
              data[0]["customt1"] == null ? ct1 = false : ct1 = true;
              data[0]["customt2"] == null ? ct2 = false : ct2 = true;
              data[0]["customt3"] == null ? t3 = false : ct3 = true;
              data[0]["customt4"] == null ? ct4 = false : ct4 = true;
              data[0]["customt5"] == null ? ct5 = false : ct5 = true;
        
              t.column(1).visible(temp);
              t.column(2).visible(humi);
              t.column(3).visible(windspeed);
              t.column(4).visible(winddir);
              t.column(5).visible(press);
              t.column(6).visible(prec);
              t.column(7).visible(rad);
              t.column(8).visible(leaf);
              t.column(9).visible(sm1);
              t.column(10).visible(sm2);
              t.column(11).visible(sm3);
              t.column(12).visible(st1);
              t.column(13).visible(st2);
              t.column(14).visible(st3);
              t.column(15).visible(cd1);
              t.column(16).visible(cd2);
              t.column(17).visible(cd3);
              t.column(18).visible(cd4);
              t.column(19).visible(cd5);
              t.column(20).visible(ct1);
              t.column(21).visible(ct2);
              t.column(22).visible(ct3);
              t.column(23).visible(ct4);
              t.column(24).visible(ct5);
        
              data.forEach(element => {
        
                var array = new Array();
                array.push(new Date(element["time"]).toLocaleString("pt-PT"))
                array.push(element["temperature"]);
                array.push(element["humidity"]);
                array.push(element["windspeed"]);
                array.push(element["winddir"]);
                array.push(element["pressure"]);
                array.push(element["precipitation"]);
                array.push(element["radiation"]);
                array.push(element["leafwetness"]);
                array.push(element["soilmoisture1"]);
                array.push(element["soilmoisture2"]);
                array.push(element["soilmoisture3"]);
                array.push(element["soiltemperature1"]);
                array.push(element["soiltemperature2"]);
                array.push(element["soiltemperature3"]);
                array.push(element["customd1"]);
                array.push(element["customd2"]);
                array.push(element["customd3"]);
                array.push(element["customd4"]);
                array.push(element["customd5"]);
                array.push(element["customt1"]);
                array.push(element["customt2"]);
                array.push(element["customt3"]);
                array.push(element["customt4"]);
                array.push(element["customt5"]);
                //console.log(array);
                t.row.add(
        
                  array
        
                ).draw(false);
              });
            }
          })

        }
      });

    })



  }
});


function DisplayDewPoint(chart, stationuuid) {
  var tempdata = new Array();
  var rhdata = new Array();
  $.get(apiUrl + "/GetTempRecords/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "_" + $('#modeselect').val(), function (data, status) {
    if (status == "success") {
      data.forEach(element => {
        tempdata.push(element);
      });
      $.get(apiUrl + "/GetHumidityRecords/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "_" + $('#modeselect').val(), function (data, status) {
        if (status == "success") {
          data.forEach(element => {
            rhdata.push(element);
          });

          var dewpoint = new Array();
          for (var index = 0; index < tempdata.length; index++) {
            //const element = array[index];
            var aux =
            {
              time: tempdata[index].time,
              value: CalcDewpoint(tempdata[index].value, rhdata[index].value / 100).toFixed(2)
            }
            dewpoint.push(aux);
          }
          // console.log("dewpoint");
          // console.log(dewpoint);


          var labels = [];
          var values = [];

          dewpoint.forEach(element => {
            labels.push(new Date(element["time"]).toLocaleString("pt-PT"))
            values.push(element["value"])
          });

          //console.log(chart.data.datasets);
          //chart.data.labels.pop();
          chart.data.datasets[1].data.pop();
          chart.update();

          chart.data.datasets[1].label = "Ponto de orvalho";
          //chart.data.labels = labels;
          chart.data.datasets[1].data = values;
          chart.update();


        }
      })
    }
  })

}

//MarketPlace para sementes


function DisplaySummary(data, status, stationid) {
  if (status = "success") {

    var aux = stations[0]

    $("#tempmax").html(data["maxTemp"] == null ? "--" : data["maxTemp"] + " " + aux.tempunit);
    $("#tempavg").html(data["avgTemp"] == null ? "--" : data["avgTemp"].toFixed(2) + " " + aux.tempunit);
    $("#tempmin").html(data["minTemp"] == null ? "--" : data["minTemp"] + " " + aux.tempunit);

    $("#hummax").html(data["maxHumidity"] == null ? "--" : data["maxHumidity"] + " %");
    $("#humavg").html(data["avgHumidity"] == null ? "--" : data["avgHumidity"].toFixed(2) + " %");
    $("#hummin").html(data["minHumidity"] == null ? "--" : data["minHumidity"] + " %");

    $("#windmax").html(data["maxWind"] == null ? "--" : data["maxWind"] + " " + aux.windspeedunit);
    $("#windavg").html(data["avgWind"] == null ? "--" : data["avgWind"].toFixed(2) + " " + aux.windspeedunit);
    $("#windmin").html(data["minWind"] == null ? "--" : data["minWind"] + " " + aux.windspeedunit);

    $("#presmax").html(data["maxPress"] == null ? "--" : data["maxPress"] + " " + aux.pressureunit);
    $("#presavg").html(data["avgPress"] == null ? "--" : data["avgPress"] + " " + aux.pressureunit);
    $("#presmin").html(data["maxPress"] == null ? "--" : data["manPress"] + " " + aux.pressureunit);

    $("#precmax").html(data["maxPrecipitation"] == null ? "--" : data["maxPrecipitation"] + " " + aux.precipitationunit);
    $("#precavg").html(data["avgPrecipitation"] == null ? "--" : data["avgPrecipitation"].toFixed(2) + " " + aux.precipitationunit);
    $("#precmin").html(data["minPrecipitation"] == null ? "--" : data["minPrecipitation"] + " " + aux.precipitationunit);

    $("#radmax").html(data["maxRadiation"] == null ? "--" : data["maxRadiation"] + " " + aux.radiationunit);
    $("#radavg").html(data["avgRadiation"] == null ? "--" : data["avgRadiation"].toFixed(2) + " " + aux.radiationunit);
    $("#radmin").html(data["minRadiation"] == null ? "--" : data["minRadiation"] + " " + aux.radiationunit);
  }
}




$('#viewbtn').on('click', function (e) {
  $.get(apiUrl + "/GetSummary/" + stationsuuid[0] + "_" + new Date(selectedDate).toISOString() + "?mode=" + $('#modeselect').val(), function (data, status) {
    if (status = "success") {

      var aux = stations[0]

      $("#tempmax").html(data["maxTemp"] == null ? "--" : data["maxTemp"] + " " + aux.tempunit);
      $("#tempavg").html(data["avgTemp"] == null ? "--" : data["avgTemp"].toFixed(2) + " " + aux.tempunit);
      $("#tempmin").html(data["minTemp"] == null ? "--" : data["minTemp"] + " " + aux.tempunit);

      $("#hummax").html(data["maxHumidity"] == null ? "--" : data["maxHumidity"] + " %");
      $("#humavg").html(data["avgHumidity"] == null ? "--" : data["avgHumidity"].toFixed(2) + " %");
      $("#hummin").html(data["minHumidity"] == null ? "--" : data["minHumidity"] + " %");

      $("#windmax").html(data["maxWind"] == null ? "--" : data["maxWind"] + " " + aux.windspeedunit);
      $("#windavg").html(data["avgWind"] == null ? "--" : data["avgWind"].toFixed(2) + " " + aux.windspeedunit);
      $("#windmin").html(data["minWind"] == null ? "--" : data["minWind"] + " " + aux.windspeedunit);

      $("#presmax").html(data["maxPress"] == null ? "--" : data["maxPress"] + " " + aux.pressureunit);
      $("#presavg").html(data["avgPress"] == null ? "--" : data["avgPress"] + " " + aux.pressureunit);
      $("#presmin").html(data["maxPress"] == null ? "--" : data["manPress"] + " " + aux.pressureunit);

      $("#precmax").html(data["maxPrecipitation"] == null ? "--" : data["maxPrecipitation"] + " " + aux.precipitationunit);
      $("#precavg").html(data["avgPrecipitation"] == null ? "--" : data["avgPrecipitation"].toFixed(2) + " " + aux.precipitationunit);
      $("#precmin").html(data["minPrecipitation"] == null ? "--" : data["minPrecipitation"] + " " + aux.precipitationunit);

      $("#radmax").html(data["maxRadiation"] == null ? "--" : data["maxRadiation"] + " " + aux.radiationunit);
      $("#radavg").html(data["avgRadiation"] == null ? "--" : data["avgRadiation"].toFixed(2) + " " + aux.radiationunit);
      $("#radmin").html(data["minRadiation"] == null ? "--" : data["minRadiation"] + " " + aux.radiationunit);

      var aux = stations[0];
      DisplayChartData(mixedChart, "GetTempRecords", "tempChart", "Temperatura(" + aux.tempunit + ")");
      DisplayChartData(humidityChart, "GetHumidityRecords", "humidityChart", "Humidade(%)");
      DisplayChartData(pressChart, "GetPressureRecords", "pressChart", "Pressão(" + aux.pressureunit + ")");
      DisplayChartData(precChart, "GetPrecipitationRecords", "precChart", "Precipitação(" + aux.precipitationunit + ")");
      DisplayChartData(radChart, "GetRadiationRecords", "radChart", "Radiação(" + aux.radiationunit + ")");
      DisplayChartDataWind(windsChart, "GetWindRecords", "windsChart", "rosePlot");
      DisplayDewPoint(mixedChart, stationsuuid[0], "dewpointChart");


    }
  })
})

$('#exporttable').on('click', function (e) {

})



$('#viewbtntable').on('click', function (e) {
  $('#datatable').DataTable().clear();
  var t = $('#datatable').DataTable();
  var counter = 0;
  $.get(apiUrl + "/GetRecords/" + stationsuuid[0] + "/" + new Date(selectedDateStart).toISOString() + "_until_" + new Date(selectedDateEnd).toISOString(), function (data, status) {
    if (status == "success") {
      var temp = false;
      var humi = false;
      var windspeed = false;
      var winddir = false;
      var press = false;
      var prec = false;
      var rad = false;
      var leaf = false;
      var sm1 = false;
      var sm2 = false;
      var sm3 = false;
      var st1 = false;
      var st2 = false;
      var st3 = false;
      var cd1 = false;
      var cd2 = false;
      var cd3 = false;
      var cd4 = false;
      var cd5 = false;
      var ct1 = false;
      var ct2 = false;
      var ct3 = false;
      var ct4 = false;
      var ct5 = false;
      //console.log(data[0]);
      data[0]["temperature"] == null ? temp = false : temp = true;
      data[0]["humidity"] == null ? humi = false : humi = true;
      data[0]["windspeed"] == null ? windspeed = false : windspeed = true;
      data[0]["winddir"] == null ? winddir = false : winddir = true;
      data[0]["pressure"] == null ? press = false : press = true;
      data[0]["precipitation"] == null ? prec = false : prec = true;
      data[0]["radiation"] == null ? rad = false : (rad = true);
      data[0]["leafwetness"] == null ? leaf = false : leaf = true;
      data[0]["soilmoisture1"] == null ? sm1 = false : sm1 = true;
      data[0]["soilmoisture2"] == null ? sm2 = false : sm2 = true;
      data[0]["soilmoisture3"] == null ? sm3 = false : sm3 = true;
      data[0]["soiltemperature1"] == null ? st1 = false : st1 = true;
      data[0]["soiltemperature2"] == null ? st2 = false : st2 = true;
      data[0]["soiltemperature3"] == null ? st3 = false : st3 = true;
      data[0]["customd1"] == null ? cd1 = false : cd1 = true;
      data[0]["customd2"] == null ? cd2 = false : cd2 = true;
      data[0]["customd3"] == null ? cd3 = false : cd3 = true;
      data[0]["customd4"] == null ? cd4 = false : cd4 = true;
      data[0]["customd5"] == null ? cd5 = false : cd5 = true;
      data[0]["customt1"] == null ? ct1 = false : ct1 = true;
      data[0]["customt2"] == null ? ct2 = false : ct2 = true;
      data[0]["customt3"] == null ? t3 = false : ct3 = true;
      data[0]["customt4"] == null ? ct4 = false : ct4 = true;
      data[0]["customt5"] == null ? ct5 = false : ct5 = true;

      t.column(1).visible(temp);
      t.column(2).visible(humi);
      t.column(3).visible(windspeed);
      t.column(4).visible(winddir);
      t.column(5).visible(press);
      t.column(6).visible(prec);
      t.column(7).visible(rad);
      t.column(8).visible(leaf);
      t.column(9).visible(sm1);
      t.column(10).visible(sm2);
      t.column(11).visible(sm3);
      t.column(12).visible(st1);
      t.column(13).visible(st2);
      t.column(14).visible(st3);
      t.column(15).visible(cd1);
      t.column(16).visible(cd2);
      t.column(17).visible(cd3);
      t.column(18).visible(cd4);
      t.column(19).visible(cd5);
      t.column(20).visible(ct1);
      t.column(21).visible(ct2);
      t.column(22).visible(ct3);
      t.column(23).visible(ct4);
      t.column(24).visible(ct5);

      data.forEach(element => {

        var array = new Array();
        array.push(new Date(element["time"]).toLocaleString("pt-PT"))
        array.push(element["temperature"]);
        array.push(element["humidity"]);
        array.push(element["windspeed"]);
        array.push(element["winddir"]);
        array.push(element["pressure"]);
        array.push(element["precipitation"]);
        array.push(element["radiation"]);
        array.push(element["leafwetness"]);
        array.push(element["soilmoisture1"]);
        array.push(element["soilmoisture2"]);
        array.push(element["soilmoisture3"]);
        array.push(element["soiltemperature1"]);
        array.push(element["soiltemperature2"]);
        array.push(element["soiltemperature3"]);
        array.push(element["customd1"]);
        array.push(element["customd2"]);
        array.push(element["customd3"]);
        array.push(element["customd4"]);
        array.push(element["customd5"]);
        array.push(element["customt1"]);
        array.push(element["customt2"]);
        array.push(element["customt3"]);
        array.push(element["customt4"]);
        array.push(element["customt5"]);
        //console.log(array);
        t.row.add(

          array

        ).draw(false);
      });
    }
  })
})




//$('.form-control').datepicker();

$('.form-control-sum').datepicker({
  setDate: new Date(),
  //selectedDate = new Date(),
  autoclose: true
});

$('.form-control-sum').on('changeDate', function () {
  //console.log("data changed");
  //console.log($('.form-control-sum').datepicker('getFormattedDate'));
  //console.log(new Date($('.form-control-sum').datepicker('getFormattedDate')));
  selectedDate = new Date($('.form-control-sum').datepicker('getFormattedDate')).setDate(new Date($('.form-control-sum').datepicker('getFormattedDate')).getDate() + 1);
  //console.log(selectedDate);
});


$('.form-control-start').datepicker({
  setDate: new Date(),
  autoclose: true
});

$('.form-control-start').on('changeDate', function () {
  //console.log($('.form-control-start').datepicker('getFormattedDate'));
  selectedDateStart = $('.form-control-start').datepicker('getFormattedDate');
});


$('.form-control-end').datepicker({
  setDate: new Date(),
  autoclose: true
});

$('.form-control-end').on('changeDate', function () {
  //console.log($('.form-control-end').datepicker('getFormattedDate'));
  selectedDateEnd = $('.form-control-end').datepicker('getFormattedDate');
});