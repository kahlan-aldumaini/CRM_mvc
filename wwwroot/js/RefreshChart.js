const ReportchartList = document.querySelectorAll('.chart-report');
const reportChart = {};
if (ReportchartList) {
    ReportchartList.forEach(function (ReportchartEl) {
        const color = config.colors[ReportchartEl.dataset.color], series = 0;
        const optionsBundle = radialBarChart(color, series);
        reportChart[ReportchartEl.getAttribute("data-key")] = new ApexCharts(ReportchartEl, optionsBundle);
        reportChart[ReportchartEl.getAttribute("data-key")].render();
    });
}

const analyticsBarChartEl = document.querySelector('#analyticsBarChart'),
    analyticsBarChartConfig = {
        chart: {
            height: 250,
            type: 'bar',
            toolbar: {
                show: false
            }
        },
        plotOptions: {
            bar: {
                horizontal: false,
                columnWidth: '70%',
                borderRadius: 3,
                startingShape: 'rounded'
            }
        },
        dataLabels: {
            enabled: false
        },
        colors: [config.colors.success, config.colors.danger, config.colors.info, config.colors.warning],
        series: [
            {
                name: 'الاتصالات',
                data: []
            },
            {
                name: ' رسائل sms',
                data: []
            },
            {
                name: 'رسائل ايميل',
                data: []
            },
            {
                name: ' الجلسات',
                data: []
            }
        ],
        grid: {
            borderColor: borderColor,
            padding: {
                bottom: -8
            }
        },
        xaxis: {
            categories: ['السبت', 'الاحد', 'الاثنين', 'الثلاثاء', 'الاربعاء', 'الخميس', 'الجمعة'],
            axisBorder: {
                show: false
            },
            axisTicks: {
                show: false
            },
            labels: {
                style: {
                    colors: axisColor
                }
            }
        },
        yaxis: {
            min: 0,
            tickAmount: 3,
            labels: {
                style: {
                    colors: axisColor
                }
            }
        },
        legend: {
            show: false
        }, tooltip: {
            y: {
                formatter: function (val) {
                    return val;
                }
            }
        }
    };
let analyticsBarChart = null, dailyReportDonutChart = null;

if (typeof analyticsBarChartEl !== undefined && analyticsBarChartEl !== null) {
    analyticsBarChart = new ApexCharts(analyticsBarChartEl, analyticsBarChartConfig);
    analyticsBarChart.render();
}
dailyReportDonutChart = DonutChart({
    series: [],
    labels: ['البريد الالكتروني', 'رسائل sms', 'الجلسات'],
    colors: [config.colors.success, config.colors.danger, config.colors.warning],
    total: {
        value: 0, label: 'المجموع', color: config.colors.success
    },
    elementId: '#dailyReportDonutChart',
});

let conversionBarChart = BarChart({
    series: [],
    colors: [config.colors.primary, config.colors.warning],
    elementId: '#conversionBarchart',
})

const chart = new signalR.HubConnectionBuilder().withUrl("/chart-report").build();
chart.on("ReceiveData", function (model) {
    document.getElementById('TotalHourlyCalls').innerHTML = model.dailyReport?.totalHourlyCalls;
    document.getElementById('CurrentIncomingCalls').innerHTML = model.dailyReport?.currentIncomingCalls;
    document.getElementById('CurrentOutgoingCalls').innerHTML = model.dailyReport?.currentOutgoingCalls;

    for (const reportChartKey in reportChart) {
        if (reportChartKey == 'null') continue;
        if (reportChart.hasOwnProperty(reportChartKey)) {
            reportChart[reportChartKey].el.parentElement.querySelector('h3').innerHTML = model.weeklyReport[reportChartKey];
            updateChart(reportChart[reportChartKey], [model.weeklyReport[reportChartKey]])
        }
    }

    const data = [
        {
            name: 'الاتصالات',
            data: Object.values(model.weeklyReport.callDay)
        },
        {
            name: ' رسائل sms',
            data: Object.values(model.weeklyReport.smsDay)
        },
        {
            name: 'رسائل ايميل',
            data: Object.values(model.weeklyReport.emailDay)
        },
        {
            name: ' الجلسات',
            data: Object.values(model.weeklyReport.sessionDay)
        }
    ];
    updateChart(analyticsBarChart, data)

    const dailyData = [
        model.dailyReport.emails,
        model.dailyReport.sms,
        model.dailyReport.sessions
    ];
    updateChart(dailyReportDonutChart, dailyData)

    const convData = [
        {
            name: 'الاتصالات الوارده',
            data: model.dailyReport.incomingHourlyCalls
        },
        {
            name: 'الاتصالات الصادرة',
            data: model.dailyReport.outgoingHourlyCalls.map((item) => -item)
        }
    ];
    updateChart(conversionBarChart, convData)
});

chart.start()
    .then(() => {
        setInterval(chart.invoke("Refresh", 1), 2000);
    })
    .catch((err) => {
        console.log(err);
    });

