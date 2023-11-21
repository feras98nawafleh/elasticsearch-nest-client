<template>
    <div style="display: block;">
        <div class="row align-items-center justify-content-end m-0">
            <div class="col-sm-2 col-md-2 col-lg-2">
                <v-label class="label_required mb-1 ">{{ $t("res_Label_SelectStructure") }}<span> *</span></v-label>
                <div id="bg-white-multi" class="with-absolute-button ">
                    <StructureUserSelector :showInternalUsers="false" :showExternalUsers="false"
                        :showInternalStructures="true" :showExternalStructures="false" :excludeSelf="false"
                        :isMultiple="false" :selectedStructureUser="structure"
                        @structureUserSelectChanged="setStructure" addressBook_header="Select Structure">
                    </StructureUserSelector>
                </div>
            </div>
            <div id="date-white-bg" class="col-sm-1 col-md-1 col-lg-1">
                <v-label>{{ $t('res_Label_getAllHierarchy') }}</v-label>
                <v-checkbox class="m-0" v-model="viewAllHierarchy"></v-checkbox>
            </div>
            <div id="date-white-bg" class="col-sm-2 col-md-2 col-lg-2">
                <v-label class="label_required mb-1 ">{{ $t("res_Label_FromDate") }}<span> *</span></v-label>
                <v-menu content-class="p-0" v-model="from_date_menu" :close-on-content-click="false"
                    transition="scale-transition" offset-y min-width="auto">
                    <template v-slot:activator="{ on, attrs }">
                        <v-text-field class="shrink dtField " label="Select Date"
                            :value="utils.dateFormat(fromDate)" solo v-on="on" append-icon="mdi-calendar">
                            <template v-slot:append>
                                <v-icon v-bind="attrs" v-on="on">mdi-calendar</v-icon>
                            </template>
                        </v-text-field>
                    </template>
                    <v-date-picker v-model="fromDate" @input="from_date_menu = false"
                        @change="onChangeDateRange($event, 'fromDate')"></v-date-picker>
                </v-menu>
            </div>
            <div id="date-white-bg" class="col-sm-2 col-md-2 col-lg-2">
                <v-label class="label_required mb-1">{{ $t("res_Label_ToDate") }}<span> *</span></v-label>
                <v-menu content-class="p-0" v-model="to_date_menu" :close-on-content-click="false"
                    transition="scale-transition" offset-y min-width="auto">
                    <template v-slot:activator="{ on, attrs }">
                        <v-text-field id="date-white-bg" class="shrink dtField " label="Select Date"
                            :value="utils.dateFormat(toDate)" solo v-on="on" append-icon="mdi-calendar">
                            <template v-slot:append>
                                <v-icon v-bind="attrs" v-on="on">mdi-calendar</v-icon>
                            </template>
                        </v-text-field>
                    </template>
                    <v-date-picker v-model="toDate" @input="to_date_menu = false"
                        @change="onChangeDateRange($event, 'toDate')"></v-date-picker>
                </v-menu>
            </div>
            <div class="my-2 d-flex justify-content-end align-items-center gap-3">
                <v-btn @click="clearForm()" color="light-blue">{{ $t("res_ButtonText_Clear") }}</v-btn>
                <v-btn dense @click="updateFilter" :disabled="isUpdateFilterDisabled()" class="primary">{{
                    $t("res_ButtonText_ApplyFilter") }}</v-btn>
            </div>
            <hr />
            <div class="d-flex mt-3">
                <div class="col-sm-6 col-md-6 col-lg-3">
                    <v-card class="bg-white d-flex justify-content-between align-items-center p-4 rounded-4">
                        <v-icon class="blue-icon"> mdi-email-open-outline </v-icon>
                        <span class="size-18">{{ $t("res_Label_OpenCorrespondences") }}</span>
                        <span class="size-18 float-right p-0">{{ categoriesCounts.Correspondences }}</span>
                    </v-card>
                </div>
                <div class="col-sm-6 col-md-6 col-lg-3">
                    <v-card class="bg-white d-flex justify-content-between align-items-center p-4 rounded-4">
                        <v-icon class="blue-icon">mdi-aspect-ratio </v-icon>
                        <span class="size-18">{{ $t("res_Label_OpenTransfers") }}</span>
                        <span v-if="categoriesCounts.countOpen >= 0" class="size-18 float-right p-0">{{
                            categoriesCounts.countOpen }}</span>
                    </v-card>
                </div>
                <div class="col-sm-6 col-md-6 col-lg-3">
                    <v-card class="bg-white d-flex justify-content-between align-items-center p-4 rounded-4">
                        <v-icon class="blue-icon"> mdi-pencil-outline </v-icon>
                        <span class="size-18">{{ $t("res_Label_NeedSignature") }}</span>
                        <span  v-if="categoriesCounts.countNeedSignature >= 0" class="size-18 float-right p-0">{{
                            categoriesCounts.countNeedSignature }}</span>
                    </v-card>
                </div>
                <div class="col-sm-6 col-md-6 col-lg-3">
                    <v-card class="bg-white d-flex justify-content-between align-items-center p-4 rounded-4">
                        <v-icon class="blue-icon"> mdi-history </v-icon>
                        <span class="size-18">{{ $t("res_Label_DelayedTransfers") }}</span>
                        <span v-if="categoriesCounts.countDelayed >= 0" class="size-18 float-right p-0">{{
                            categoriesCounts.countDelayed }}</span>
                    </v-card>
                </div>
            </div>
            <div class="d-flex">
                <div class="col-sm-12 mb-2">
                    <v-card class="bg-white rounded-top-corners rounded-4">
                        <div class="d-flex justify-content-end align-items-start flex-column mb-1">
                            <span class="dashboarCardTitle mt-2">{{ $t("res_Label_TotalReceiveVsClosed") }}</span>
                            <span v-if="structure.id != -1"
                                style="font-size: 0.8rem; padding-left: 15px; padding-right: 15px; display: block">Per
                                Structure</span>
                            <span v-else
                                style="font-size: 0.8rem; padding-left: 15px; padding-right: 15px; display: block">please
                                select a structure</span>
                        </div>
                        <apexchart v-if="structure.id != -1" type="bar" height="130%"
                            :options="receivedVsClosedOptions" :series="receivedVsClosedOptions.chartSeries"
                            style="padding-bottom: 15px;" />
                    </v-card>
                </div>
            </div>
            <div class="d-flex">
                <div class="col-sm-4">
                    <v-card class="bg-white flex-1 rounded-top-corners pt-5">
                        <div class="mb-2">
                            <span class="dashboarCardTitle m-0 ml-2">{{ $t("res_Label_Received") }}</span>
                            <apexchart height="300px"  :options="receivedDonutChartOptions"
                                :series="receivedDonutChartOptions.receivedDonutChartSeries" />
                        </div>
                        <div class="row m-0 p-2" style="background: rgb(0 0 0 / 3%);">
                            <div class="col-md-6  text-center align-self-start">
                                <span class="action-label-new">Previous Year</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0 ">---</p>
                                    <span
                                        class="text-increase d-inline-flex gap-0"><v-icon>mdi-arrow-up</v-icon>--%</span>
                                </div>
                            </div>
                            <div class="col-md-6 align-self-center text-center">
                                <span class="action-label-new ">Year 2021</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0 ">---</p>
                                    <span
                                        class="text-decrease d-inline-flex gap-0"><v-icon>mdi-arrow-down</v-icon>--%</span>
                                </div>
                            </div>
                        </div>
                    </v-card>
                </div>

                <div class="col-sm-4">
                    <v-card class="bg-white flex-1 rounded-top-corners pt-5">
                        <div class="mb-2">
                            <span class="dashboarCardTitle m-0 ml-2">{{ $t("res_Label_Closed") }}</span>
                            <apexchart  height="300px" :options="closedDonutChartOptions"
                                :series="closedDonutChartOptions.closedDonutChartSeries" />
                        </div>
                        <div class="row m-0 p-2" style="background: rgb(0 0 0 / 3%);">
                            <div class="col-md-6  text-center align-self-start">
                                <span class="action-label-new">Previous Year</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0 ">--</p>
                                    <span
                                        class="text-increase d-inline-flex gap-0"><v-icon>mdi-arrow-up</v-icon>0%</span>
                                </div>
                            </div>
                            <div class="col-md-6 align-self-center text-center">
                                <span class="action-label-new">Year 2021</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0 ">---</p>
                                    <span
                                        class="text-stable d-inline-flex gap-0"><v-icon>mdi-minus</v-icon>0%</span>
                                </div>
                            </div>
                        </div>
                    </v-card>
                </div>

                <div class="col-sm-4">
                    <v-card class="bg-white flex-1 rounded-top-corners pt-5">
                        <div class="mb-2">
                            <span class="dashboarCardTitle m-0 ml-2">{{ $t("res_Label_Overdue") }}</span>
                            <apexchart  height="300px" :options="overdueDonutChartOptions"
                                :series="overdueDonutChartOptions.overdueDonutChartSeries" />
                        </div>
                        <div class="row m-0 p-2" style="background: rgb(0 0 0 / 3%);">
                            <div class="col-md-6  text-center align-self-start">
                                <span class="action-label-new">Previous Year</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0">---</p>
                                    <span
                                        class="text-increase d-inline-flex gap-0"><v-icon>mdi-arrow-up</v-icon>--%</span>
                                </div>
                            </div>
                            <div class="col-md-6 align-self-center text-center">
                                <span class="action-label-new ">Year 2021</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0">---</p>
                                    <span
                                        class="text-decrease d-inline-flex gap-0"><v-icon>mdi-arrow-down</v-icon>--%</span>
                                </div>
                            </div>
                        </div>
                    </v-card>
                </div>
                <!-- <div v-if="structure.id != -1" class="col-sm-4">
                    <v-card v-for="(chart, key) in donutChartsData"
                    :key="key" class="bg-white flex-1 rounded-top-corners pt-5">
                        <div class="mb-2">
                            <span class="dashboarCardTitle m-0 ml-2">{{ $t("res_Label_Overdue") }}</span>
                            <apexchart  height="300px" :options="chart.chartOptions"
                                :series="chart.series" />
                        </div>
                        <div class="row m-0 p-2" style="background: rgb(0 0 0 / 3%);">
                            <div class="col-md-6  text-center align-self-start">
                                <span class="action-label-new">Previous Year</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0">---</p>
                                    <span
                                        class="text-increase d-inline-flex gap-0"><v-icon>mdi-arrow-up</v-icon>--%</span>
                                </div>
                            </div>
                            <div class="col-md-6 align-self-center text-center">
                                <span class="action-label-new ">Year 2021</span>
                                <div class="d-flex align-items-center justify-content-center gap-1">
                                    <p class="main-text-large m-0">---</p>
                                    <span
                                        class="text-decrease d-inline-flex gap-0"><v-icon>mdi-arrow-down</v-icon>--%</span>
                                </div>
                            </div>
                        </div>
                    </v-card>
                </div> -->
            </div>
            <div v-for="(dataSeries, index) in listDataSeries" :key="index" class="d-flex">
                <div v-if="delayValues.length > 0 && completionValues.length > 0" class="col-sm-12 mb-2">
                    <v-card class="bg-white rounded-top-corners rounded-4">
                        <v-card-text class="dashboarCardTitle mt-2">
                            <p>{{ dataSeries[0].headerLabel }}</p>
                            <p>date range: <span style="color: #1B9C85">{{ filter.fromYear }}</span> to <span
                                    style="color: #1B9C85">{{ filter.toYear }}</span></p>
                            <p v-if="filter.fromStructureId != '' && filter.fromStructureId != -1">
                                For structure <span style="color: #1B9C85">{{ structure.name }}</span>
                            </p>
                        </v-card-text>
                        <apexchart type="line" height="430" :options="chartOptions[index]" :series="dataSeries">
                        </apexchart>
                    </v-card>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import * as utils from "@/assets/js/common.js";
import StructureUserSelector from "@/components/StructureUserSelector.vue";
import VueApexCharts from "vue-apexcharts";
import constants from "@/assets/js/constants.js";
import axios from "axios";

export default {
    name: "Dashboard",
    components: { StructureUserSelector, VueApexCharts },
    data() {
        const englishMonths = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];
        const arabicMonths = [
            'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
            'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
        ];
        return {
            categoriesCounts: {
                Correspondences: 0,
                countOpen: 0,
                countNeedSignature: 0,
                countDelayed: 0,
            },
            viewAllHierarchy: false,
            fromDate: "",
            toDate: "",
            from_date_menu: false,
            to_date_menu: false,
            structure: { name: "", id: -1, },
            getAllHierarchy: false,
            utils: utils,
            receivedVsClosedOptions: {
                chart: { type: "bar" },
                xaxis: { categories: [] },
                series: [],
                plotOptions: {
                    bar: { horizontal: false, columnWidth: "55%", endingShape: "rounded" },
                },
                dataLabels: { enabled: false },
                stroke: { show: true, width: 0, colors: ["transparent"] },
                yaxis: {
                    title: { text: "" },
                },
                fill: { opacity: 1 },
                tooltip: {
                    y: {
                        formatter: function (val) { return val; },
                    },
                },
                chartSeries: [
                ],
            },
            donutChartsData: {
                received: {
                    chartOptions: { type: "pie" },
                    labels: [],
                    series: [],
                },
                closed: {
                    chartOptions: { type: "pie" },
                    labels: [],
                    series: [],
                },
                overdue: {
                    chartOptions: { type: "pie" },
                    labels: [],
                    series: [],
                },
            },
            receivedDonutChartOptions: {
                chart: { type: "pie" },
                labels: [],
                receivedDonutChartSeries: []
            },
            closedDonutChartOptions: { 
                chart: { type: "pie" },
                labels: [],
                closedDonutChartSeries: []
            },
            overdueDonutChartOptions: {
                chart: { type: "pie" },
                labels: [], 
                overdueDonutChartSeries: [] 
            },
            filterDialog: false,
            filter: {
                fromStructureId: "",
                fromYear: "2015-01-01",
                toYear: new Date().toISOString().slice(0, 10)
            },
            listDataSeries: [],
            delayValues: [],
            completionValues: [],
            averageCompletiondataSeries: [{
                name: "Average completion in month",
                data: [],
                headerLabel: "Average Duration for Transfer Completion"
            }],
            averageDelaydataSeries: [{
                name: "Average delay in month",
                data: [],
                headerLabel: "Average Duration for Transfer Delay"
            }],
            chartOptions: [
                {
                    title: {
                        text: "",
                        floating: 0,
                        offsetY: 20,
                        align: "center",
                        style: {
                            color: "#444"
                        }
                    },
                    markers: {
                        seriesIndex: 0,
                        colors: '#0E21A0',
                        fillColor: '#0E21A0',
                        strokeColor: '#0E21A0',
                        size: 6,
                        shape: "circle"
                    },
                    chart: {
                        height: 350,
                        type: 'line',
                        color: '#0E21A0',
                        zoom: {
                            enabled: false
                        }
                    },
                    noData: {
                        text: "No Data",
                        align: "center",
                        verticalAlign: "middle",
                    },
                    dataLabels: {
                        enabled: false
                    },
                    stroke: {
                        colors: '#0E21A0',
                        curve: 'straight'
                    },
                    grid: {
                        row: {
                            colors: ['#f3f3f3', 'transparent'],
                            opacity: 0.5
                        },
                    },
                    xaxis: {
                        categories: localStorage.getItem('lang') == 'ar' ? arabicMonths : englishMonths,
                        title: {
                            text: 'All Categories'
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Average (Days)'
                        },
                    },
                },
                {
                    title: {
                        text: "",
                        floating: 0,
                        offsetY: 20,
                        align: "center",
                        style: {
                            color: "#444"
                        }
                    },
                    markers: {
                        seriesIndex: 0,
                        colors: '#952323',
                        fillColor: '#952323',
                        strokeColor: '#952323',
                        size: 6,
                        shape: "circle"
                    },
                    chart: {
                        height: 350,
                        type: 'line',
                        color: '#952323',
                        zoom: {
                            enabled: false
                        }
                    },
                    noData: {
                        text: "No Data",
                        align: "center",
                        verticalAlign: "middle",
                    },
                    dataLabels: {
                        enabled: false
                    },
                    stroke: {
                        colors: '#952323',
                        curve: 'straight'
                    },
                    grid: {
                        row: {
                            colors: ['#f3f3f3', 'transparent'],
                            opacity: 0.5
                        },
                    },
                    xaxis: {
                        categories: localStorage.getItem('lang') == 'ar' ? arabicMonths : englishMonths,
                        title: {
                            text: 'All Categories'
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Average (Days)'
                        },
                    },
                },
            ],
        };
    },
    mounted() {
        this.getAverageCompletion('', `${this.filter.fromYear}/${this.filter.toYear}`);
        this.getAverageDelay('', `${this.filter.fromYear}/${this.filter.toYear}`);
        this.getDashboardCounts('', `${this.filter.fromYear}/${this.filter.toYear}`);
    },
    watch: {
        filter: {
            handler(newFilter) {
                this.getAverageCompletion(newFilter.fromStructureId, `${newFilter.fromYear}/${newFilter.toYear}`);
                this.getAverageDelay(newFilter.fromStructureId, `${newFilter.fromYear}/${newFilter.toYear}`);
                this.getDashboardCounts(newFilter.fromStructureId, `${newFilter.fromYear}/${newFilter.toYear}`);
            },
            deep: true,
        },
        completionValues: {
            handler(newCompletionValues) {
                const average = newCompletionValues.reduce((acc, currentValue) => acc + currentValue, 0) / 12;
                this.chartOptions[0].title.text = `Total Average: ${average} day(s)`;
            }
        },
        delayValues: {
            handler(newDelayValues) {
                const average = newDelayValues.reduce((acc, currentValue) => acc + currentValue, 0) / 12;
                this.chartOptions[1].title.text = `Total Average: ${average} day(s)`;
            }
        }
    },
    methods: {
        setStructure(item) {
            this.structure = item;
            this.filter.fromStructureId = this.structure.id;
            this.receivedVsClosedOptions.xaxis.categories.push(item.companyName);
            this.getTotalReceivedVsClosed(item.id);
            this.getReceivedCategorized(item.id);
            this.getClosedCategorized(item.id);
            this.getOverdueCategorized(item.id);
        },
        onChangeDateRange(event, FromOrTo) {
            if (FromOrTo == 'fromDate') {
                if (new Date(this.fromDate) > new Date(this.toDate)) {
                    window.showAlert("res_Msgs_FromDateCompare", "warning");
                    this.fromDate = window.newDate().toISOString().substr(0, 10);
                }
                else {
                    this.fromDate = event;
                }
            }
            else {
                if (new Date(this.toDate) < new Date(this.fromDate)) {
                    window.showAlert("res_Msgs_ToDateCompare", "warning");
                    this.toDate = window.newDate().toISOString().substr(0, 10);
                }
                else {
                    this.toDate = event;
                }
            }
        },
        getAverageCompletion(fromStructureId, fromYear) {
            try {
                this.completionValues = [];
                axios.post(`http://localhost:5000/api/Dashboard/AverageCompletion?toStructureId=${fromStructureId}&yearFromTo=${fromYear}`, { headers: constants.headers })
                    .then(res => {
                        if (res != undefined) {
                            Object.values(res.data).forEach(value => {
                                this.completionValues.push(value);
                            });
                        } else {
                            this.completionValues.push(0);
                        }
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
            finally {
                this.averageCompletiondataSeries[0].data = this.completionValues;
                this.listDataSeries[0] = this.averageCompletiondataSeries;
            }
        },
        getAverageDelay(fromStructureId, fromYear) {
            try {
                this.delayValues = [];
                axios.post(`http://localhost:5000/api/Dashboard/AverageDelay?toStructureId=${fromStructureId}&yearFromTo=${fromYear}`, { headers: constants.headers })
                    .then(res => {
                        if (res != undefined) {
                            Object.values(res.data).forEach(value => {
                                this.delayValues.push(value);
                            });
                        } else {
                            this.delayValues.push(0);
                        }
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
            finally {
                this.averageDelaydataSeries[0].data = this.delayValues;
                this.listDataSeries[1] = this.averageDelaydataSeries;
            }
        },
        getDashboardCounts(fromStructureId, fromYear) {
            try {
                axios.post(`http://localhost:5000/api/Dashboard/DashboardCounts?toStructureId=${fromStructureId}&yearFromTo=${fromYear}`, { headers: constants.headers })
                    .then(res => {
                        this.categoriesCounts.countDelayed = res.data.countDelayed;
                        this.categoriesCounts.countOpen = res.data.countOpen;
                        this.categoriesCounts.countNeedSignature = res.data.countForSignature;
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
        },
        getTotalReceivedVsClosed(id) {
            try {
                axios.post(`http://localhost:5000/api/Dashboard/ReceivedVsClosed?toStructureId=${id}&yearFromTo=${this.filter.fromYear}/${this.filter.toYear}`, { headers: constants.headers })
                    .then(res => {
                        Object.entries(res.data).forEach(([key, value]) => {
                            this.receivedVsClosedOptions.chartSeries.push({name: key, data: [value]})
                        });
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
        },
        getReceivedCategorized(id){
            try {
                axios.post(`http://localhost:5000/api/Dashboard/ReceivedByCategories?toStructureId=${id}&yearFromTo=${this.filter.fromYear}/${this.filter.toYear}`, { headers: constants.headers })
                    .then(res => {
                        Object.entries(res.data).forEach(([key, value]) => {
                            this.receivedDonutChartOptions.labels.push(key);
                            this.receivedDonutChartOptions.receivedDonutChartSeries.push(value);
                            this.donutChartsData.received.labels.push(key);
                            this.donutChartsData.received.series.push(value);
                        });
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
        },
        getClosedCategorized(id){
            try {
                axios.post(`http://localhost:5000/api/Dashboard/ClosedByCategories?toStructureId=${id}&yearFromTo=${this.filter.fromYear}/${this.filter.toYear}`, { headers: constants.headers })
                    .then(res => {
                        Object.entries(res.data).forEach(([key, value]) => {
                            this.closedDonutChartOptions.labels.push(key);
                            this.closedDonutChartOptions.closedDonutChartSeries.push(value);
                            this.donutChartsData.closed.labels.push(key);
                            this.donutChartsData.closed.series.push(value);
                        });
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
        },
        getOverdueCategorized(id){
            try {
                axios.post(`http://localhost:5000/api/Dashboard/OverDueByCategories?toStructureId=${id}&yearFromTo=${this.filter.fromYear}/${this.filter.toYear}`, { headers: constants.headers })
                    .then(res => {
                        Object.entries(res.data).forEach(([key, value]) => {
                            this.overdueDonutChartOptions.labels.push(key);
                            this.overdueDonutChartOptions.overdueDonutChartSeries.push(value);
                            this.donutChartsData.overdue.labels.push(key);
                            this.donutChartsData.overdue.series.push(value);
                        });
                    })
                    .catch(error => alert(error));
            }
            catch (error) {
                console.log(error);
            }
        },
        updateFilter() {
            this.filter.fromYear = this.fromDate;
            this.filter.toYear = this.toDate;
        },
        isUpdateFilterDisabled() {
            return (this.fromDate == "" || this.toDate == "") && this.structure.id == -1;
        },
        clearForm() {
            this.fromDate = this.toDate = "";
            this.structure = {};
            this.viewAllHierarchy = false;
        }
    },
};
</script>

<style scoped>
.dashboarCardTitle {
    font-weight: 1000;
    margin-top: 15px;
    padding: 15px;
    font-size: 1.2rem;
}
</style>