<template>
    <div class="wrapper-body flex-1">
        <v-card class="d-flex pa-0" elevation="0">
            <v-btn class="mx-2 my-2" fab light small color="#ff8517">
                <v-icon @click="filterDialog = true" class="mdi-18px">mdi-filter</v-icon>
            </v-btn>
        </v-card>
        <v-dialog transition="dialog-top-transition" v-model="filterDialog" max-width="400">
            <v-card class="pa-4">
                <v-menu style="cursor:pointer" min-width="290px">
                    <template v-slot:activator="{ on }">
                        <v-text-field class="shrink my-auto" label="From Year" placeholder="Enter Year" outlined
                            :value="tempFilter.fromYear" v-model="tempFilter.fromYear" v-on="on" @input="validateYear">
                        </v-text-field>
                    </template>
                </v-menu>
                <div>
                    <StructureUserSelector :showInternalUsers="true" :showExternalUsers="false"
                        :showInternalStructures="true" :showExternalStructures="false" :excludeSelf="true"
                        :isMultiple="true" @structureUserSelectChanged="structureSelectedChangeForDelay">
                    </StructureUserSelector>
                </div>
                <v-card-actions>
                    <div>
                        <p style="color: red" v-if="tempFilter.error">Invalid year format. Please enter a valid four-digit
                            year
                        </p>
                    </div>
                    <v-spacer></v-spacer>
                    <v-btn color="#ff8517" fab light small :disabled="tempFilter.error"
                        @click="updateFilter(); filterDialog = false">
                        <v-icon class="mdi-18px">mdi-magnify</v-icon>
                    </v-btn>
                    <v-btn color="#cccccc" fab light small @click="filterDialog = false">
                        <v-icon class="mdi-18px">mdi-close</v-icon>
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <div v-if="delayValues.length > 0 && completionValues.length > 0" class="wrapper-body flex-1">
            <v-container fluid>
                <v-row v-for="(dataSeries, index) in listDataSeries" :key="index" align="center" justify="center"
                    no-gutters>
                    <v-col cols="10">
                        <v-card class="mx-1" elevation="2" outlined tile>
                            <v-card-text class="text-left">
                                <h6>{{ dataSeries[0].headerLabel }}</h6>
                                <h6>(Starting Year: <span style="color: #1B9C85">{{ filter.fromYear }}</span>)</h6>
                                <h6 v-if="filter.fromStructureId != ''">
                                    (For structure with ID:
                                    <span style="color: #1B9C85">{{ filter.fromStructureId }}</span>)
                                </h6>
                            </v-card-text>
                            <apexchart type="line" height="430" :options="chartOptions[index]" :series="dataSeries">
                            </apexchart>
                        </v-card>
                    </v-col>
                </v-row>
            </v-container>
        </div>
    </div>
</template>
<style scoped></style>
<script>
import constants from "@/assets/js/constants.js";
import axios from "axios";
import StructureUserSelector from '../../StructureUserSelector.vue';

export default {
    name: "MetricsDashboard",
    components: { StructureUserSelector },
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
            filterDialog: false,
            tempFilter: {
                fromStructureId: "",
                fromYear: "2015",
                error: false,
            },
            filter: {
                fromStructureId: "",
                fromYear: "2015",
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
        }
    },
    watch: {
        filter: {
            handler(newFilter) {
                this.getAverageCompletion(newFilter.fromStructureId, newFilter.fromYear);
                this.getAverageDelay(newFilter.fromStructureId, newFilter.fromYear);
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
        getAverageCompletion(fromStructureId, fromYear) {
            try {
                this.completionValues = [];
                axios.post(`http://localhost:5000/api/Dashboard/AverageCompletion?toStructureId=${fromStructureId}&year=${fromYear}`, { headers: constants.headers })
                    .then(res => {
                        if (res != undefined) {
                            Object.values(res.data).forEach(value => {
                                this.completionValues.push(value);
                            });
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
                axios.post(`http://localhost:5000/api/Dashboard/AverageDelay?toStructureId=${fromStructureId}&year=${fromYear}`, { headers: constants.headers })
                    .then(res => {
                        if (res != undefined) {
                            Object.values(res.data).forEach(value => {
                                this.delayValues.push(value);
                            });
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
        validateYear() {
            const yearPattern = /^\d{4}$/;
            const currentYear = new Date().getFullYear();
            if (!yearPattern.test(this.tempFilter.fromYear) || parseInt(this.tempFilter.fromYear) > currentYear) {
                this.tempFilter.error = true;
            } else {
                this.tempFilter.error = false;
            }
        },
        structureSelectedChangeForDelay(item) {
            this.tempFilter.fromStructureId = item[item.length - 1].id;
        },
        updateFilter() {
            this.filter.fromStructureId = this.tempFilter.fromStructureId;
            this.filter.fromYear = this.tempFilter.fromYear;
        }
    },
    mounted() {
        this.getAverageCompletion('', this.tempFilter.fromYear);
        this.getAverageDelay('', this.tempFilter.fromYear);
    }
};
</script>