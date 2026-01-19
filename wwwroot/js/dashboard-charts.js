function initializeDashboardCharts(data) {
    // Personal Category Chart (Doughnut)
    if (data.personal.hasData && data.personal.categories.length > 0) {
        const personalCategoryCtx = document.getElementById('personalCategoryChart');
        if (personalCategoryCtx) {
            new Chart(personalCategoryCtx, {
                type: 'doughnut',
                data: {
                    labels: data.personal.categories.map(c => c.categoryName),
                    datasets: [{
                        data: data.personal.categories.map(c => c.count),
                        backgroundColor: data.personal.categories.map(c => c.color),
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'right',
                        },
                        title: {
                            display: false
                        }
                    }
                }
            });
        }
    }

    // Personal Trend Chart (Line)
    if (data.personal.hasData && data.personal.historical.length > 0) {
        const personalTrendCtx = document.getElementById('personalTrendChart');
        if (personalTrendCtx) {
            new Chart(personalTrendCtx, {
                type: 'line',
                data: {
                    labels: data.personal.historical.map(w => w.weekLabel),
                    datasets: [{
                        label: 'Total Items',
                        data: data.personal.historical.map(w => w.totalItems),
                        borderColor: '#36A2EB',
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Urgent Items',
                        data: data.personal.historical.map(w => w.urgentItems),
                        borderColor: '#FF6384',
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
        }
    }

    // Household Category Chart (Doughnut)
    if (data.household.hasData && data.household.categories.length > 0) {
        const householdCategoryCtx = document.getElementById('householdCategoryChart');
        if (householdCategoryCtx) {
            new Chart(householdCategoryCtx, {
                type: 'doughnut',
                data: {
                    labels: data.household.categories.map(c => c.categoryName),
                    datasets: [{
                        data: data.household.categories.map(c => c.count),
                        backgroundColor: data.household.categories.map(c => c.color),
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'right',
                        },
                        title: {
                            display: false
                        }
                    }
                }
            });
        }
    }

    // Household Trend Chart (Line)
    if (data.household.hasData && data.household.historical.length > 0) {
        const householdTrendCtx = document.getElementById('householdTrendChart');
        if (householdTrendCtx) {
            new Chart(householdTrendCtx, {
                type: 'line',
                data: {
                    labels: data.household.historical.map(w => w.weekLabel),
                    datasets: [{
                        label: 'Total Items',
                        data: data.household.historical.map(w => w.totalItems),
                        borderColor: '#36A2EB',
                        backgroundColor: 'rgba(54, 162, 235, 0.2)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Urgent Items',
                        data: data.household.historical.map(w => w.urgentItems),
                        borderColor: '#FF6384',
                        backgroundColor: 'rgba(255, 99, 132, 0.2)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
        }
    }
}
