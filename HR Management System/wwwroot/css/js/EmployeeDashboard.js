const currentDate = new Date();

const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];
const currentMonth = monthNames[currentDate.getMonth()];
const currentYear = currentDate.getFullYear();

document.getElementById("current-month-year").textContent = `${currentMonth} ${currentYear}`;