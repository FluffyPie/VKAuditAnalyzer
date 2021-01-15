
function DrawTheChart(ChartData, ChartOptions, ChartId, ChartType){
 eval('var myLine = new Chart(document.getElementById(ChartId).getContext("2d"),{type:"'+ChartType+'",data:ChartData,options:ChartOptions});document.getElementById(ChartId).getContext("2d").stroke();')
}

function MoreChartOptions() { } 

var labels = []
for (var i = 1; i <= 10; i++) {
	labels[i - 1] = document.getElementById("Topic" + i).value;
}

var data = []
for (var i = 1; i <= 10; i++) {
	data[i - 1] = document.getElementById("Dataset" + i).value;
}

var ChartData = {
	labels : labels,
	datasets : [
		{
		data : data,
		backgroundColor :['rgba(100,180,180,1)','rgba(245,210,85,1)','rgba(4,75,139,1)','rgba(239,85,41,1)','rgba(152,214,225,1)','rgba(239,207,198,1)','rgba(137,179,74,1)','rgba(209,44,119,1)','rgba(92,114,70,1)','rgba(210,177,149,1)',],
		borderColor : ['rgba(100,180,180,1)','rgba(245,210,85,1)','rgba(4,75,139,1)','rgba(239,85,41,1)','rgba(152,214,225,1)','rgba(239,207,198,1)','rgba(137,179,74,1)','rgba(209,44,119,1)','rgba(92,114,70,1)','rgba(210,177,149,1)',],
		label:""},

	]
};
    ChartOptions= {
        responsive:false,
        layout:{padding:{top:12,left:12,bottom:12,},},
      
legend:{
	position:'bottom',
	labels:{
		fontSize:18,
		fontColor:'#ffffff',
		boxWidth:41,
		padding:12,
		usePointStyle:true,

		generateLabels: function(chart){
			 return  chart.data.labels.map( function( dataset, i ){
				return{
					text:dataset,
					lineCap:chart.data.datasets[0].borderCapStyle,
					lineDash:[],
					lineDashOffset: 0,
					lineJoin:chart.data.datasets[0].borderJoinStyle,
					pointStyle:'circle',
					fillStyle:chart.data.datasets[0].backgroundColor[i],
					strokeStyle:chart.data.datasets[0].borderColor[i],
					lineWidth:chart.data.datasets[0].pointBorderWidth,
					lineDash:chart.data.datasets[0].borderDash,
				}
			})
		},

	},
},

title:{
	display:true,
	text:'Топ популярных тем ',
	fontColor:'#ffffff',
	fontSize:32,
	fontStyle:' bold',
	},
elements: {
	arc: {borderWidth:6,
},
	line: {
},
	rectangle: {
},
},
tooltips:{
},
hover:{
	mode:'point',
	animationDuration:400,
},
plugins:{
    datalabels:{display:true,
        
        color:'#ffffff',
        offset:0,
        fontSize:32,
       
    },
},
};
 DrawTheChart(ChartData,ChartOptions,"chart-02","doughnut");
 DrawTheChart(ChartData,ChartOptions,"chart-01","bar");