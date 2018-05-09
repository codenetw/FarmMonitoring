dashboardCharts = {

voltage: function(){
    Highcharts.chart('containerVoltage', {
        credits:
        {
            enabled: false
        },
        exporting:
        {
             enabled: false
        },
        title: {
            text: 'Вольтаж на картах'
        },
        chart: {
            inverted: true
        },
        yAxis: {
            title: { text: 'Вт' },
        },
        xAxis: {
            categories: []
        },

        series: [{
            type: 'column',
            colorByPoint: true,
            data: [],
            showInLegend: false
        }]
    });
    },
containerPolar: function() {
Highcharts.chart('containerPolar', {
    credits:
    {
        enabled: false
    },
    exporting:
    {
        enabled: false
    },
        chart: {
            polar: true,
            type: 'area'
        },

        title: {
            text: 'История карт'
        },
        pane: {
            startAngle: 0,
            endAngle: 360
        },


        xAxis: {
            tickmarkPlacement: 'on',
            categories: ['Потеря', 'Главная', 'Сбой майна', 'Отвал карты', 'Сброс частот']
        },

        plotOptions: {
            column: {
                pointPadding: 0,
                groupPadding: 0
            }
        },

        yAxis: {
            min: 0
        },
        series: [{
            name: 'GPU 0',
            fillOpacity: 0.2,
            data: [8, 7, 6, 25, 6]
        }, {

            fillOpacity: 0.2,
            name: 'GPU 1',
            data: [1, 2, 3, 4, 0]
        }, {
            fillOpacity: 0.2,
            name: 'GPU 2',
            data: [1, 8, 2, 7, 6]
        }]
    });
    },
temp: function() {
    Highcharts.chart('containerTemp', {
        credits:
        {
            enabled: false
        },
        exporting:
        {
            enabled: false
        },
            chart: {
                type: 'line'
            },
            title: {
                text: 'Температура на картах'
            },
             xAxis: {
            labels:
            {
                enabled: false
            }
            },
            yAxis: {
                title: {
                    text: 'Температура (°C)'
                }
            },
            plotOptions: {
                line: {
                    dataLabels: {
                        enabled: false
                    },
                    enableMouseTracking: true
                }
            },
            series: []
        });
},
update: function (chartId, data)
    {

        if (chartId === "#containerVoltage")
            $(chartId).highcharts().update(data);

        if (chartId === "#containerTemp") {
            var cseries = $(chartId).highcharts().series;
            var ch = $(chartId).highcharts();
            var maxPointsShow = 20;

            data.map(function (c, t, x) {
                var series = cseries.find((x) => x.name === c.name);
                if (series === undefined) {
                    ch.addSeries({ name: c.name, data: [c.data]}, true);
                } else {
                    series.addPoint(c.data);
                    var lastPoint = series.data.length - 1;
                    ch.xAxis[0].setExtremes(
                        lastPoint - (maxPointsShow - 1),
                        lastPoint); 
                }
            });
           
        }
}
}